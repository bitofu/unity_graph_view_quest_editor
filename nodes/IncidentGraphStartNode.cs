using System;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class IncidentGraphStartNode : IncidentGraphNode {

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

  IncidentGraphStart startSaveData;

  public override void CopyTo(IncidentGraphActivity target) => startSaveData.CopyTo(target);

  public override void Init(IncidentGraphView graphView, Vector2 position, IncidentGraphActivity saveData) {
    base.Init(graphView, position, saveData);
    title = "Start";
    isStartNode = true;
    startSaveData = (IncidentGraphStart)baseData;
    if (saveData == null) {
      startSaveData.tag = string.Empty;
    } else {
      ((IncidentGraphStart)saveData).CopyTo(startSaveData);
    }

    InitPorts(
      baseData.outputs,
      () => AddOutputPort(CreateNewLink("Next"), Port.Capacity.Single, typeof(bool)),
      (link) => AddOutputPort(link, Port.Capacity.Single, typeof(bool))
    );

    VisualElement customDataContainer = new VisualElement();
    customDataContainer.AddToClassList("ig-node__custom-data-container");

    VisualElement tagContainer = new VisualElement();
    tagContainer.AddToClassList("ig-node__container-with-bar");

    if (string.IsNullOrEmpty(startSaveData.tag)) {
      invalidFieldCount++;
    }
    TextField tagField = IncidentGraphUtility.CreateTextField(startSaveData.tag, "Tag:", (change) => {
      if (startSaveData.tag == change.newValue) return;
      bool wasValid = IsValidTargetId(startSaveData.tag);
      bool newValid = IsValidTargetId(change.newValue);
      if (wasValid && !newValid) {
        invalidFieldCount++;
      } else if (!wasValid && newValid) {
        invalidFieldCount--;
      }
      startSaveData.tag = change.newValue;
    });
    tagField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );

    Toggle cannotTerminateToggle = IncidentGraphUtility.CreateToggle(
      startSaveData.cannotTerminate, "Cannot Terminate", null, (change) => {
        startSaveData.cannotTerminate = change.newValue;
      }
    );
    cannotTerminateToggle.AddClasses("ig-node__label", "ig-node__toggle");

    tagContainer.Add(tagField);
    tagContainer.Add(cannotTerminateToggle);

    VisualElement interruptionContainer = new VisualElement();
    interruptionContainer.AddToClassList("ig-node__container-with-bar");

    Toggle canInterruptToggle = IncidentGraphUtility.CreateToggle(
      startSaveData.canInterrupt, "Can Interrupt", null, (change) => {
        startSaveData.canInterrupt = change.newValue;
      }
    );
    canInterruptToggle.AddClasses("ig-node__label", "ig-node__toggle");

    TextField bridgeField = IncidentGraphUtility.CreateTextField(
      startSaveData.bridgeGroup,
      "Bridge Group:",
      (change) => startSaveData.bridgeGroup = change.newValue
    );
    bridgeField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );

    interruptionContainer.Add(canInterruptToggle);
    interruptionContainer.Add(bridgeField);
    
    customDataContainer.Add(tagContainer);
    customDataContainer.Add(interruptionContainer);
    extensionContainer.Add(customDataContainer);
    RefreshExpandedState();
    RefreshPorts();
  }

}
