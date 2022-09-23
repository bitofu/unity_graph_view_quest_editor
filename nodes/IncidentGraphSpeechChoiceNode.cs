using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class IncidentGraphSpeechChoiceNode : IncidentGraphNode {

  VisualElement allChoiceSpeeches;

  IncidentGraphSpeechChoice choiceSaveData;

  public override void CopyTo(IncidentGraphActivity target) => choiceSaveData.CopyTo(target);

  public override void Init(IncidentGraphView graphView, Vector2 position, IncidentGraphActivity saveData) {
    base.Init(graphView, position, saveData);
    title = "Speech Choice";
    choiceSaveData = (IncidentGraphSpeechChoice)baseData;
    if (saveData == null) {
      choiceSaveData.playerCanAct = false;
      choiceSaveData.cameraManipulation = new IncidentGraphCameraManipulation() {
        type = IncidentGraphCameraManipulation.Type.Track,
        moveToSpeed = 5
      };
      choiceSaveData.speechForChoices = new List<IncidentGraphSpeechChoice.SpeechForChoice>();
    } else {
      ((IncidentGraphSpeechChoice)saveData).CopyTo(choiceSaveData);
    }

    AddStopGraphToggle(choiceSaveData);
    AddPlayerCanActToggle(choiceSaveData);
    InitPorts(
      choiceSaveData.inputs,
      () => AddInputPort(CreateNewLink("Previous"), Port.Capacity.Multi, typeof(bool)),
      (link) => AddInputPort(link, Port.Capacity.Multi, typeof(bool))
    );

    VisualElement customDataContainer = new VisualElement();
    customDataContainer.AddToClassList("ig-node__custom-data-container");

    VisualElement topExtensionContainer = new VisualElement();
    allChoiceSpeeches = new VisualElement();

    InitPorts(
      choiceSaveData.outputs,
      () => CreateChoicePortWithSpeech(CreateNewLink("New Choice")),
      (link) => CreateChoicePortWithSpeech(link)
    );

    Button addChoiceButton = IncidentGraphUtility.CreateButton("Add Choice", () => {
      if (choiceSaveData.outputs.Count >= 3) return;
      CreateChoicePortWithSpeech(CreateNewLink("New Choice"));
      RefreshExpandedState();
      RefreshPorts();
    });

    Foldout cameraFoldout = IncidentGraphUtility.CreateFoldout("Camera", true);
    IncidentGraphSpeechNode.AddCameraManipulation(cameraFoldout, choiceSaveData.cameraManipulation);

    FloatField timeLimitField = null;
    timeLimitField = IncidentGraphUtility.CreateFloatField(
      "Time Limit:",
      choiceSaveData.timeLimit,
      (change) => {
        choiceSaveData.timeLimit = change.newValue;
        if (choiceSaveData.timeLimit < 0) {
          choiceSaveData.timeLimit = 0;
          timeLimitField.SetValueWithoutNotify(choiceSaveData.timeLimit);
        }
      }
    );
    timeLimitField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );

    topExtensionContainer.Add(addChoiceButton);
    topExtensionContainer.Add(cameraFoldout);
    topExtensionContainer.Add(timeLimitField);
    customDataContainer.Add(topExtensionContainer);
    customDataContainer.Add(allChoiceSpeeches);
    VisualElement emptyMarginContainer = new VisualElement();
    emptyMarginContainer.AddToClassList("ig-node__button-flex-container-bottom");
    customDataContainer.Add(emptyMarginContainer);
    extensionContainer.Add(customDataContainer);
    RefreshExpandedState();
    RefreshPorts();
  }

  void AddNewSpeechUnit(
    IncidentGraphNodeLink link,
    IncidentGraphSpeechChoice.SpeechForChoice speechForChoice,
    VisualElement speechesContainer
  ) {
    IncidentGraphSpeechUnit unit = new IncidentGraphSpeechUnit(
      Constants.bacchusName,
      "Text here"
    );
    speechForChoice.speechUnits.Add(unit);
    VisualElement unitContainer = new VisualElement();
    unitContainer.AddToClassList("ig-node__speech-unit-container");
    IncidentGraphSpeechNode.DrawSpeechUnit(unit, unitContainer, () => {
      speechForChoice.speechUnits.Remove(unit);
      speechesContainer.Remove(unitContainer);
    });
    speechesContainer.Add(unitContainer);
  }

  void CreateChoicePortWithSpeech(IncidentGraphNodeLink link) {
    VisualElement choiceSpeechesContainer = new VisualElement();
    choiceSpeechesContainer.AddToClassList("ig-node__choice-speeches-container");
    VisualElement speechesContainer = new VisualElement();
    
    IncidentGraphSpeechChoice.SpeechForChoice speechForChoice = choiceSaveData.speechForChoices.Find((item) =>
      item.portId == link.portId
    );
    if (speechForChoice == null) {
      speechForChoice = new IncidentGraphSpeechChoice.SpeechForChoice() {
        portId = link.portId,
        speechUnits = new List<IncidentGraphSpeechUnit>()
      };
      choiceSaveData.speechForChoices.Add(speechForChoice);
    }
    if (speechForChoice.speechUnits.Count > 0) {
      for (int i = 0; i < speechForChoice.speechUnits.Count; i++) {
        IncidentGraphSpeechUnit unit = speechForChoice.speechUnits[i];
        VisualElement unitContainer = new VisualElement();
        unitContainer.AddToClassList("ig-node__speech-unit-container");
        IncidentGraphSpeechNode.DrawSpeechUnit(unit, unitContainer, () => {
          speechForChoice.speechUnits.Remove(unit);
          speechesContainer.Remove(unitContainer);
        });
        speechesContainer.Add(unitContainer);
      }
    }

    VisualElement addSpeechUnitContainer = new VisualElement();
    addSpeechUnitContainer.AddToClassList("ig-node__button-flex-container-bottom");
    Button addSpeechUnitButton = IncidentGraphUtility.CreateButton("Add Speech Unit", () => {
      AddNewSpeechUnit(link, speechForChoice, speechesContainer);
      RefreshExpandedState();
    });
    addSpeechUnitButton.AddToClassList("ig-node__button-add");
    addSpeechUnitContainer.Add(addSpeechUnitButton);
    choiceSpeechesContainer.Add(speechesContainer);
    choiceSpeechesContainer.Add(addSpeechUnitContainer);
    
    string choiceText = link.text;
    link.text = string.Empty; // hide port label
    Port choicePort = AddOutputPort(link, Port.Capacity.Single, typeof(bool));
    link.text = choiceText;
    Button deleteChoiceButton = IncidentGraphUtility.CreateButton("X", () => {
      if (choiceSaveData.outputs.Count == 1) return;
      if (choicePort.connected) {
        graphView.DeleteElements(choicePort.connections);
      }
      graphView.RemoveElement(choicePort);
      graphView.portMap.Remove(link.portId);
      choiceSaveData.outputs.Remove(link);
      choiceSaveData.speechForChoices.Remove(speechForChoice);
      allChoiceSpeeches.Remove(choiceSpeechesContainer);
    });

    TextField choiceTextField = IncidentGraphUtility.CreateTextField(
      link.text,
      null,
      (change) => link.text = change.newValue.Trim()
    );
    choiceTextField.AddClasses(
      "ig-node__choice-textfield",
      "ig-node__textfield__hidden"
    );

    choicePort.Add(deleteChoiceButton);
    choicePort.Add(choiceTextField);
    allChoiceSpeeches.Add(choiceSpeechesContainer);
  }

  struct PortSpeechData {
    public string contentId;
    public VisualElement speechElement;
    public PortSpeechData(string id, VisualElement element) =>
      (contentId, speechElement) = (id, element);
  }

}
