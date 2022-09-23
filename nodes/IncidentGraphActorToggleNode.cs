using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class IncidentGraphActorToggleNode : IncidentGraphNode {

  int invalidFieldCount {
    get => errorCount;
    set {
      errorCount = value;
      if (errorCount == 1) {
        mainContainer.AddToClassList("ig-node__main-container-error");
      } else if (errorCount == 0) {
        mainContainer.RemoveFromClassList("ig-node__main-container-error");
      }
    }
  }

  IncidentGraphActorToggle actorToggleSaveData;

  public override void CopyTo(IncidentGraphActivity target) => actorToggleSaveData.CopyTo(target);

  public override void Init(IncidentGraphView graphView, Vector2 position, IncidentGraphActivity saveData) {
    base.Init(graphView, position, saveData);
    title = "Actor Toggle";
    actorToggleSaveData = (IncidentGraphActorToggle)baseData;
    if (saveData == null) {
      invalidFieldCount++;
    } else {
      ((IncidentGraphActorToggle)saveData).CopyTo(actorToggleSaveData);
    }

    AddStopGraphToggle(actorToggleSaveData);
    InitPorts(
      actorToggleSaveData.inputs,
      () => AddInputPort(CreateNewLink("Previous"), Port.Capacity.Multi, typeof(bool)),
      (link) => AddInputPort(link, Port.Capacity.Multi, typeof(bool))
    );
    InitPorts(
      actorToggleSaveData.outputs,
      () => AddOutputPort(CreateNewLink("Next"), Port.Capacity.Single, typeof(bool)),
      (link) => AddOutputPort(link, Port.Capacity.Single, typeof(bool))
    );

    VisualElement customDataContainer = new VisualElement();
    customDataContainer.AddToClassList("ig-node__custom-data-container");

    Toggle targetStateToggle = IncidentGraphUtility.CreateToggle(
      actorToggleSaveData.targetState,
      "Target State:",
      null,
      (change) => actorToggleSaveData.targetState = change.newValue
    );
    targetStateToggle.AddToClassList("ig-node__label");
  
    Toggle targetHideToggle = IncidentGraphUtility.CreateToggle(
      actorToggleSaveData.hideOnDisable,
      "Hide on Disable:",
      null,
      (change) => actorToggleSaveData.hideOnDisable = change.newValue
    );
    targetHideToggle.AddToClassList("ig-node__label");

    ObjectField actorTypeField = IncidentGraphUtility.CreateObjectField(
      actorToggleSaveData.targetActor,
      "Target Actor:",
      typeof(ActorType),
      (change) => {
        bool isEmpty = change.newValue == null;
        if (isEmpty) {
          invalidFieldCount++;
        } else {
          invalidFieldCount--;
        }
        actorToggleSaveData.targetActor = (ActorType)change.newValue;
      }
    );
    actorTypeField.AddToClassList("ig-node__label");

    customDataContainer.Add(targetStateToggle);
    customDataContainer.Add(targetHideToggle);
    customDataContainer.Add(actorTypeField);
    extensionContainer.Add(customDataContainer);
    RefreshExpandedState();
    RefreshPorts();
  }

}
