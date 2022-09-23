using System;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class IncidentGraphBridgeStartNode : IncidentGraphNode {

  public string tag => bridgeSaveData.tag;

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

  IncidentGraphBridgeStart bridgeSaveData;
  TextField tagField;

  public override void CopyTo(IncidentGraphActivity target) => bridgeSaveData.CopyTo(target);

  public override void Init(IncidentGraphView graphView, Vector2 position, IncidentGraphActivity saveData) {
    base.Init(graphView, position, saveData);
    title = "Bridge Start";
    isBridgeNode = true;
    bridgeSaveData = (IncidentGraphBridgeStart)baseData;
    if (saveData == null) {
      bridgeSaveData.tag = string.Empty;
    } else {
      ((IncidentGraphBridgeStart)saveData).CopyTo(bridgeSaveData);
    }

    InitPorts(
      baseData.outputs,
      () => AddOutputPort(CreateNewLink("Next"), Port.Capacity.Single, typeof(bool)),
      (link) => AddOutputPort(link, Port.Capacity.Single, typeof(bool))
    );

    VisualElement customDataContainer = new VisualElement();
    customDataContainer.AddToClassList("ig-node__custom-data-container");

    if (string.IsNullOrEmpty(bridgeSaveData.tag)) invalidFieldCount++;
    tagField = IncidentGraphUtility.CreateTextField(
      bridgeSaveData.tag,
      "Tag:",
      (change) => {
        string newValue = change.newValue.Trim();
        if (bridgeSaveData.tag == newValue) return;
        bool wasValid = IsValidTargetId(bridgeSaveData.tag);
        bool newValid = IsValidTargetId(newValue);
        if (wasValid && !newValid) {
          invalidFieldCount++;
        } else if (!wasValid && newValid) {
          invalidFieldCount--;
        }
        bridgeSaveData.tag = newValue;
      }
    );
    tagField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );

    customDataContainer.Add(tagField);
    extensionContainer.Add(customDataContainer);
    RefreshExpandedState();
    RefreshPorts();
  }

  public void SetTag(string tag) {
    tagField.value = tag;
  }

}
