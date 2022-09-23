using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

public class IncidentGraphEndNode : IncidentGraphNode {

  IncidentGraphEnd endSaveData;

  public override void CopyTo(IncidentGraphActivity target) => endSaveData.CopyTo(target);

  public override void Init(IncidentGraphView graphView, Vector2 position, IncidentGraphActivity saveData) {
    base.Init(graphView, position, saveData);
    title = "End";
    endSaveData = (IncidentGraphEnd)baseData;
    if (saveData == null) {
      endSaveData.stopGraph = true;
    } else {
      ((IncidentGraphEnd)saveData).CopyTo(endSaveData);
    }
    graphView.endNodes.Add(endSaveData);
    
    AddStopGraphToggle(endSaveData);
    InitPorts(
      endSaveData.inputs,
      () => AddInputPort(CreateNewLink("Previous"), Port.Capacity.Multi, typeof(bool)),
      (link) => AddInputPort(link, Port.Capacity.Multi, typeof(bool))
    );

    VisualElement customDataContainer = new VisualElement();
    customDataContainer.AddToClassList("ig-node__custom-data-container");

    EnumField enumField = IncidentGraphUtility.CreateEnumField(
      endSaveData.endType,
      null,
      (context) => endSaveData.endType = (IncidentGraphEnd.Type)context.newValue
    );
    enumField.SetValueWithoutNotify(endSaveData.endType);

    customDataContainer.Add(enumField);
    extensionContainer.Add(customDataContainer);
    RefreshExpandedState();
    RefreshPorts();
  }

}
