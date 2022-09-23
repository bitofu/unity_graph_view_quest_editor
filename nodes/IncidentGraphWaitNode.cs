using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class IncidentGraphWaitNode : IncidentGraphNode {

  IncidentGraphWait waitSaveData;

  public override void CopyTo(IncidentGraphActivity target) => waitSaveData.CopyTo(target);

  public override void Init(IncidentGraphView graphView, Vector2 position, IncidentGraphActivity saveData) {
    base.Init(graphView, position, saveData);
    title = "Wait";
    waitSaveData = (IncidentGraphWait)baseData;
    if (saveData == null) {
      waitSaveData.duration = 1;
    } else {
      ((IncidentGraphWait)saveData).CopyTo(waitSaveData);
    }

    AddStopGraphToggle(waitSaveData);
    InitPorts(
      waitSaveData.inputs,
      () => AddInputPort(CreateNewLink("Previous"), Port.Capacity.Multi, typeof(bool)),
      (link) => AddInputPort(link, Port.Capacity.Multi, typeof(bool))
    );
    InitPorts(
      waitSaveData.outputs,
      () => AddOutputPort(CreateNewLink("Next"), Port.Capacity.Single, typeof(bool)),
      (link) => AddOutputPort(link, Port.Capacity.Single, typeof(bool))
    );

    VisualElement customDataContainer = new VisualElement();
    customDataContainer.AddToClassList("ig-node__custom-data-container");

    FloatField timeField = null;
    timeField = IncidentGraphUtility.CreateFloatField(
      "Duration:",
      waitSaveData.duration,
      (change) => {
        waitSaveData.duration = change.newValue;
        if (waitSaveData.duration < 0) {
          waitSaveData.duration = 0;
          timeField.SetValueWithoutNotify(waitSaveData.duration);
        }
      }
    );
    timeField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );

    customDataContainer.Add(timeField);
    extensionContainer.Add(customDataContainer);
    RefreshExpandedState();
    RefreshPorts();
  }

}
