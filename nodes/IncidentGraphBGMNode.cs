using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class IncidentGraphBGMNode : IncidentGraphNode {

  IncidentGraphBGM bgmSaveData;

  public override void CopyTo (IncidentGraphActivity target) => bgmSaveData.CopyTo(target);

  public override void Init(IncidentGraphView graphView, Vector2 position, IncidentGraphActivity saveData) {
    base.Init(graphView, position, saveData);
    title = "BGM";
    bgmSaveData = (IncidentGraphBGM)baseData;
    if (saveData == null) {
      bgmSaveData.tracks = new List<IncidentGraphBGM.Track>() {
        new IncidentGraphBGM.Track()
      };
    } else {
      ((IncidentGraphBGM)saveData).CopyTo(bgmSaveData);
    }
    
    AddStopGraphToggle(bgmSaveData);
    InitPorts(
      bgmSaveData.inputs,
      () => AddInputPort(CreateNewLink("Previous"), Port.Capacity.Multi, typeof(bool)),
      (link) => AddInputPort(link, Port.Capacity.Multi, typeof(bool))
    );
    InitPorts(
      bgmSaveData.outputs,
      () => AddOutputPort(CreateNewLink("Next"), Port.Capacity.Single, typeof(bool)),
      (link) => AddOutputPort(link, Port.Capacity.Single, typeof(bool))
    );

    VisualElement customDataContainer = new VisualElement();
    customDataContainer.AddToClassList("ig-node__custom-data-container");

    VisualElement stopBGMContainer = new VisualElement();
    stopBGMContainer.AddToClassList("ig-node__container-with-bar");
    Toggle stopBGMToggle = IncidentGraphUtility.CreateToggle(
      bgmSaveData.stopBGM,
      "Stop BGM",
      null,
      (change) => bgmSaveData.stopBGM = change.newValue
    );
    stopBGMToggle.AddClasses("ig-node__label", "ig-node__toggle");
    EnumField stopModeField = IncidentGraphUtility.CreateEnumField(
      bgmSaveData.stopMode,
      "Stop Type:",
      (context) => bgmSaveData.stopMode = (FMOD.Studio.STOP_MODE)context.newValue
    );
    stopModeField.SetValueWithoutNotify(bgmSaveData.stopMode);
    stopModeField.AddToClassList("ig-node__label");
    stopBGMContainer.Add(stopBGMToggle);
    stopBGMContainer.Add(stopModeField);

    VisualElement fmodContainer = new VisualElement();
    fmodContainer.AddToClassList("ig-node__container-with-bar");
    Toggle skipTransitionToggle = IncidentGraphUtility.CreateToggle(
      bgmSaveData.skipTransition,
      "Skip Transition",
      null,
      (change) => bgmSaveData.skipTransition = change.newValue
    );
    skipTransitionToggle.AddClasses("ig-node__label", "ig-node__toggle");
    TextField fmodEventField = IncidentGraphUtility.CreateTextField(
      bgmSaveData.fmodEvent,
      "FMOD Event:",
      (change) => bgmSaveData.fmodEvent = change.newValue.Trim()
    );
    fmodEventField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );
    fmodContainer.Add(skipTransitionToggle);
    fmodContainer.Add(fmodEventField);

    customDataContainer.Add(stopBGMContainer);
    customDataContainer.Add(fmodContainer);
    AddTrackField("'bgm_1' Stength:", bgmSaveData.tracks[0], customDataContainer);
    extensionContainer.Add(customDataContainer);
    RefreshExpandedState();
    RefreshPorts();
  }

  void AddTrackField(string trackLabel, IncidentGraphBGM.Track track, VisualElement container) {
    VisualElement unitContainer = new VisualElement();
    // unitContainer.AddToClassList("ig-node__interruption-container");
    unitContainer.AddToClassList("ig-node__container-with-bar");

    FloatField trackStrengthField = null;
    trackStrengthField = IncidentGraphUtility.CreateFloatField(
      trackLabel,
      track.strength,
      (change) => {
        track.strength = change.newValue;
        if (track.strength < 0 || track.strength > 1) {
          track.strength = Mathf.Clamp01(track.strength);
          trackStrengthField.SetValueWithoutNotify(track.strength);
        }
      }
    );
    trackStrengthField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );

    FloatField timeToField = null;
    timeToField = IncidentGraphUtility.CreateFloatField(
      "Time To:",
      track.timeTo,
      (change) => {
        track.timeTo = change.newValue;
        if (track.timeTo < 0) {
          track.timeTo = 0;
          timeToField.SetValueWithoutNotify(track.timeTo);
        }
      }
    );
    timeToField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );

    unitContainer.Add(trackStrengthField);
    unitContainer.Add(timeToField);
    container.Add(unitContainer);
  }

}
