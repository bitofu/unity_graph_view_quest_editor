using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class IncidentGraphSpeechNode : IncidentGraphNode {

  IncidentGraphSpeech speechSaveData;

  public override void CopyTo(IncidentGraphActivity target) => speechSaveData.CopyTo(target);

  public override void Init(IncidentGraphView graphView, Vector2 position, IncidentGraphActivity saveData) {
    base.Init(graphView, position, saveData);
    title = "Speech";
    speechSaveData = (IncidentGraphSpeech)baseData;
    if (saveData == null) {
      speechSaveData.playerCanAct = false;
      speechSaveData.cameraManipulation = new IncidentGraphCameraManipulation() {
        type = IncidentGraphCameraManipulation.Type.Track,
        moveToSpeed = 5
      };
      speechSaveData.speechUnits = new List<IncidentGraphSpeechUnit>();
    } else {
      ((IncidentGraphSpeech)saveData).CopyTo(speechSaveData);
    }

    AddStopGraphToggle(speechSaveData);
    AddPlayerCanActToggle(speechSaveData);
    InitPorts(
      speechSaveData.inputs,
      () => AddInputPort(CreateNewLink("Previous"), Port.Capacity.Multi, typeof(bool)),
      (link) => AddInputPort(link, Port.Capacity.Multi, typeof(bool))
    );
    InitPorts(
      speechSaveData.outputs,
      () => AddOutputPort(CreateNewLink("Next"), Port.Capacity.Single, typeof(bool)),
      (link) => AddOutputPort(link, Port.Capacity.Single, typeof(bool))
    );

    VisualElement customDataContainer = new VisualElement();
    customDataContainer.AddToClassList("ig-node__custom-data-container");

    Foldout cameraFoldout = IncidentGraphUtility.CreateFoldout("Camera", true);
    AddCameraManipulation(cameraFoldout, speechSaveData.cameraManipulation);

    VisualElement speechesContainer = new VisualElement();
    if (speechSaveData.speechUnits.Count > 0) {
      for (int i = 0; i < speechSaveData.speechUnits.Count; i++) {
        IncidentGraphSpeechUnit unit = speechSaveData.speechUnits[i];
        VisualElement unitContainer = new VisualElement();
        unitContainer.AddToClassList("ig-node__speech-unit-container");
        DrawSpeechUnit(unit, unitContainer, () => {
          if (speechSaveData.speechUnits.Count == 1) return;
          speechSaveData.speechUnits.Remove(unit);
          speechesContainer.Remove(unitContainer);
        });
        speechesContainer.Add(unitContainer);
      }
    } else {
      AddNewSpeechUnit(speechesContainer);
    }

    VisualElement addSpeechUnitContainer = new VisualElement();
    addSpeechUnitContainer.AddToClassList("ig-node__button-flex-container-bottom");
    Button addSpeechUnitButton = IncidentGraphUtility.CreateButton("Add Speech Unit", () => {
      AddNewSpeechUnit(speechesContainer);
      RefreshExpandedState();
    });
    addSpeechUnitButton.AddToClassList("ig-node__button-add");
    addSpeechUnitContainer.Add(addSpeechUnitButton);

    customDataContainer.Add(cameraFoldout);
    customDataContainer.Add(speechesContainer);
    customDataContainer.Add(addSpeechUnitContainer);
    extensionContainer.Add(customDataContainer);
    RefreshExpandedState();
    RefreshPorts();
  }

  void AddNewSpeechUnit(VisualElement speechesContainer) {
    IncidentGraphSpeechUnit unit = new IncidentGraphSpeechUnit(
      "Nameplate",
      "Text here"
    );
    speechSaveData.speechUnits.Add(unit);
    VisualElement unitContainer = new VisualElement();
    unitContainer.AddToClassList("ig-node__speech-unit-container");
    DrawSpeechUnit(unit, unitContainer, () => {
      if (speechSaveData.speechUnits.Count == 1) return;
      speechSaveData.speechUnits.Remove(unit);
      speechesContainer.Remove(unitContainer);
    });
    speechesContainer.Add(unitContainer);
  }

  public static void AddCameraManipulation(VisualElement container, IncidentGraphCameraManipulation camera) {
    VisualElement cameraContainer = new VisualElement();
    cameraContainer.AddToClassList("ig-node__container-with-bar");

    EnumField manipulationField = IncidentGraphUtility.CreateEnumField(
      camera.type,
      null,
      (change) => {
        camera.type = (IncidentGraphCameraManipulation.Type)change.newValue;
      }
    );
    manipulationField.SetValueWithoutNotify(camera.type);

    TextField cameraTargetIdField = IncidentGraphUtility.CreateTextField(
      camera.targetId,
      "Target ID:",
      (change) => {
        camera.targetId = change.newValue;
      }
    );
    cameraTargetIdField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );
    
    FloatField cameraStartDelayField = null;
    cameraStartDelayField = IncidentGraphUtility.CreateFloatField(
      "Start Delay:",
      camera.startDelay,
      (change) => {
        camera.startDelay = change.newValue;
        if (camera.startDelay < 0) {
          camera.startDelay = 0;
          cameraStartDelayField.SetValueWithoutNotify(camera.startDelay);
        }
      }
    );
    cameraStartDelayField.AddToClassList("ig-node__label");

    FloatField cameraSpeedField = null;
    cameraSpeedField = IncidentGraphUtility.CreateFloatField(
      "Track/Anchor Spd:",
      camera.moveToSpeed,
      (change) => {
        camera.moveToSpeed = change.newValue;
        if (camera.moveToSpeed < 0.1f) {
          camera.moveToSpeed = 0.1f;
          cameraSpeedField.SetValueWithoutNotify(camera.moveToSpeed);
        }
      }
    );
    cameraSpeedField.AddToClassList("ig-node__label");
  
    cameraContainer.Add(manipulationField);
    cameraContainer.Add(cameraTargetIdField);
    cameraContainer.Add(cameraStartDelayField);
    cameraContainer.Add(cameraSpeedField);

    container.Add(cameraContainer);
  }

  public static void DrawSpeechUnit(
    IncidentGraphSpeechUnit unit,
    VisualElement unitContainer,
    Action onDelete
  ) {
    VisualElement nameplateContainer = new VisualElement();
    nameplateContainer.AddToClassList("ig-node__nameplate-container");
    TextField nameplateTextField = null;
    nameplateTextField = IncidentGraphUtility.CreateTextField(
      unit.nameplate,
      null,
      (change) => unit.nameplate = change.newValue.Trim()
    );
    nameplateTextField.AddToClassList("ig-node__nameplate-textfield");
    nameplateContainer.Add(nameplateTextField);

    TextField textArea = null;
    textArea = IncidentGraphUtility.CreateTextArea(
      unit.text,
      null,
      (change) => unit.text = change.newValue.Trim()
    );
    textArea.AddToClassList("ig-node__text-area-textfield");

    VisualElement buttonContainer = new VisualElement();
    buttonContainer.AddToClassList("ig-node__button-flex-container");
    Button deleteSpeechUnitButton = IncidentGraphUtility.CreateButton("x", onDelete);
    buttonContainer.Add(deleteSpeechUnitButton);
  
    VisualElement extraContainer = new VisualElement();
    extraContainer.AddToClassList("ig-node__container-with-bar");

    Foldout audioFoldout = IncidentGraphUtility.CreateFoldout("Audio", true);
    VisualElement audioContainer = new VisualElement();
    audioContainer.AddToClassList("ig-node__foldout-container");
    TextField fmodEventField = IncidentGraphUtility.CreateTextField(
      unit.audio.fmodEvent,
      "FMOD Event:",
      (change) => unit.audio.fmodEvent = change.newValue.Trim()
    );
    fmodEventField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );
    VisualElement audioTargetField = IncidentGraphUtility.CreateTargetField(unit.audio.target);
    audioContainer.Add(fmodEventField);
    audioContainer.Add(audioTargetField);
    audioFoldout.Add(audioContainer);

    Foldout emoteFoldout = IncidentGraphUtility.CreateFoldout("Emote", true);
    VisualElement emoteContainer = new VisualElement();
    VisualElement addEmoteContainer = new VisualElement();
    addEmoteContainer.AddToClassList("ig-node__button-flex-container");
    if (unit.emotes == null) {
      unit.emotes = new List<IncidentGraphSpeechEmote>();
    }
    Button addEmoteButton = IncidentGraphUtility.CreateButton("Add Emote", () => {
      IncidentGraphSpeechEmote emote = new IncidentGraphSpeechEmote();
      AddEmoteField(unit, emote, emoteContainer);
      unit.emotes.Add(emote);
    });
    addEmoteButton.AddToClassList("ig-node__button-add");
    foreach (IncidentGraphSpeechEmote emote in unit.emotes) {
      AddEmoteField(unit, emote, emoteContainer);
    }
    addEmoteContainer.Add(addEmoteButton);
    emoteFoldout.Add(emoteContainer);
    emoteFoldout.Add(addEmoteContainer);

    Foldout animationFoldout = IncidentGraphUtility.CreateFoldout("Animation", true);
    VisualElement animationContainer = new VisualElement();
    VisualElement addAnimationContainer = new VisualElement();
    addAnimationContainer.AddToClassList("ig-node__button-flex-container");
    if (unit.animations == null) {
      unit.animations = new List<IncidentGraphSpeechAnimation>();
    }
    Button addAnimationButton = IncidentGraphUtility.CreateButton("Add Animation", () => {
      IncidentGraphSpeechAnimation animation = new IncidentGraphSpeechAnimation();
      AddAnimationField(unit, animation, animationContainer);
      unit.animations.Add(animation);
    });
    addAnimationButton.AddToClassList("ig-node__button-add");
    foreach (IncidentGraphSpeechAnimation animation in unit.animations) {
      AddAnimationField(unit, animation, animationContainer);
    }
    addAnimationContainer.Add(addAnimationButton);
    animationFoldout.Add(animationContainer);
    animationFoldout.Add(addAnimationContainer);

    extraContainer.Add(buttonContainer);
    extraContainer.Add(audioFoldout);
    extraContainer.Add(emoteFoldout);
    extraContainer.Add(animationFoldout);
    
    unitContainer.Add(nameplateContainer);
    unitContainer.Add(textArea);
    unitContainer.Add(extraContainer);
  }

  static void AddEmoteField(
    IncidentGraphSpeechUnit unit,
    IncidentGraphSpeechEmote emote,
    VisualElement container
  ) {
    VisualElement singleContainer = new VisualElement();
    singleContainer.AddToClassList("ig-node__foldout-container-with-border");

    VisualElement buttonContainer = new VisualElement();
    buttonContainer.AddToClassList("ig-node__button-flex-container");
    Button deleteSingleButton = IncidentGraphUtility.CreateButton("x", () => {
      unit.emotes.Remove(emote);
      container.Remove(singleContainer);
    });
    buttonContainer.Add(deleteSingleButton);

    FloatField timeoutField = null;
    timeoutField = IncidentGraphUtility.CreateFloatField(
      "Timeout:",
      emote.timeout,
      (change) => {
        emote.timeout = change.newValue;
        if (emote.timeout < 0) {
          emote.timeout = 0;
          timeoutField.SetValueWithoutNotify(emote.timeout);
        }
      }
    );
    timeoutField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );

    Toggle loopToggle = IncidentGraphUtility.CreateToggle(
      emote.loop,
      "Loop Animation",
      null,
      (change) => emote.loop = change.newValue
    );
    loopToggle.AddClasses(
      "ig-node__label",
      "ig-node__toggle"
    );

    ObjectField emoteAnimation = IncidentGraphUtility.CreateObjectField(
      emote.animation, "Emote Animation:", typeof(SpriteAnimationAbstract), (context) => {
        emote.animation = (SpriteAnimationAbstract)context.newValue;
      }
    );
    emoteAnimation.AddToClassList("ig-node__label");

    VisualElement emoteTargetField = IncidentGraphUtility.CreateTargetField(emote.target);

    singleContainer.Add(buttonContainer);
    singleContainer.Add(timeoutField);
    singleContainer.Add(loopToggle);
    singleContainer.Add(emoteAnimation);
    singleContainer.Add(emoteTargetField);
    container.Add(singleContainer);
  }
    
  static void AddAnimationField(
    IncidentGraphSpeechUnit unit,
    IncidentGraphSpeechAnimation animation,
    VisualElement container
  ) {
    VisualElement singleContainer = new VisualElement();
    singleContainer.AddToClassList("ig-node__foldout-container-with-border");

    VisualElement buttonContainer = new VisualElement();
    buttonContainer.AddToClassList("ig-node__button-flex-container");
    Button deleteSingleButton = IncidentGraphUtility.CreateButton("x", () => {
      unit.animations.Remove(animation);
      container.Remove(singleContainer);
    });
    buttonContainer.Add(deleteSingleButton);

    FloatField startDelayField = null;
    startDelayField = IncidentGraphUtility.CreateFloatField(
      "Start Delay:",
      animation.startDelay,
      (change) => {
        animation.startDelay = change.newValue;
        if (animation.startDelay < 0) {
          animation.startDelay = 0;
          startDelayField.SetValueWithoutNotify(animation.startDelay);
        }
      }
    );
    startDelayField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );

    TextField animationTriggerField = IncidentGraphUtility.CreateTextField(
      animation.trigger,
      "Trigger:",
      (change) => animation.trigger = change.newValue.Trim()
    );
    animationTriggerField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );

    VisualElement targetField = IncidentGraphUtility.CreateTargetField(animation.target);

    singleContainer.Add(buttonContainer);
    singleContainer.Add(startDelayField);
    singleContainer.Add(animationTriggerField);
    singleContainer.Add(targetField);
    container.Add(singleContainer);
  }

}
