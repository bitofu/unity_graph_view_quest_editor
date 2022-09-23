using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class IncidentGraphActorAnimationNode : IncidentGraphNode {

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

  IncidentGraphActorAnimation actorAnimationSaveData;

  public override void CopyTo(IncidentGraphActivity target) => actorAnimationSaveData.CopyTo(target);

  public override void Init(IncidentGraphView graphView, Vector2 position, IncidentGraphActivity saveData) {
    base.Init(graphView, position, saveData);
    title = "Actor Animation";
    actorAnimationSaveData = (IncidentGraphActorAnimation)baseData;
    if (saveData == null) {
      //
    } else {
      ((IncidentGraphActorAnimation)saveData).CopyTo(actorAnimationSaveData);
    }

    if (actorAnimationSaveData.targetActor == null) {
      invalidFieldCount++;
    } 
    if (string.IsNullOrEmpty(actorAnimationSaveData.animationTrigger)) {
      invalidFieldCount++;
    }

    AddStopGraphToggle(actorAnimationSaveData);
    InitPorts(
      actorAnimationSaveData.inputs,
      () => AddInputPort(CreateNewLink("Previous"), Port.Capacity.Multi, typeof(bool)),
      (link) => AddInputPort(link, Port.Capacity.Multi, typeof(bool))
    );
    InitPorts(
      actorAnimationSaveData.outputs,
      () => AddOutputPort(CreateNewLink("Next"), Port.Capacity.Single, typeof(bool)),
      (link) => AddOutputPort(link, Port.Capacity.Single, typeof(bool))
    );

    VisualElement customDataContainer = new VisualElement();
    customDataContainer.AddToClassList("ig-node__custom-data-container");

    Toggle waitForAnimationToggle = IncidentGraphUtility.CreateToggle(
      actorAnimationSaveData.waitForAnimation,
      "Wait For Animation:",
      null,
      (change) => actorAnimationSaveData.waitForAnimation = change.newValue
    );
    waitForAnimationToggle.AddToClassList("ig-node__label");

    ObjectField actorTypeField = IncidentGraphUtility.CreateObjectField(
      actorAnimationSaveData.targetActor,
      "Target Actor:",
      typeof(ActorType),
      (change) => {
        bool isEmpty = change.newValue == null;
        if (isEmpty) {
          invalidFieldCount++;
        } else if (!string.IsNullOrEmpty(actorAnimationSaveData.animationTrigger)) {
          invalidFieldCount--;
        }
        actorAnimationSaveData.targetActor = (ActorType)change.newValue;
      }
    );
    actorTypeField.AddToClassList("ig-node__label");

    TextField animationTriggerField = IncidentGraphUtility.CreateTextField(
      actorAnimationSaveData.animationTrigger,
      "Animation Trigger:",
      (change) => {
        if (actorAnimationSaveData.animationTrigger == change.newValue) return;
        bool wasValid = !string.IsNullOrEmpty(actorAnimationSaveData.animationTrigger);
        bool newValid = !string.IsNullOrEmpty(change.newValue);
        if (wasValid && !newValid) {
          invalidFieldCount++;
        } else if (!wasValid && newValid) {
          invalidFieldCount--;
        }
        actorAnimationSaveData.animationTrigger = change.newValue.Trim();
      }
    );
    animationTriggerField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );

    customDataContainer.Add(waitForAnimationToggle);
    customDataContainer.Add(actorTypeField);
    customDataContainer.Add(animationTriggerField);
    extensionContainer.Add(customDataContainer);
    RefreshExpandedState();
    RefreshPorts();
  }

}
