using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class IncidentGraphGameObjectFlipStateNode : IncidentGraphNode {

  int targetFieldCount;
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

  IncidentGraphGameObjectFlipState flipSaveData;

  public override void CopyTo(IncidentGraphActivity target) => flipSaveData.CopyTo(target);

  public override void Init(IncidentGraphView graphView, Vector2 position, IncidentGraphActivity saveData) {
    base.Init(graphView, position, saveData);
    title = "GameObject Flip State";
    flipSaveData = (IncidentGraphGameObjectFlipState)baseData;
    if (saveData == null) {
      flipSaveData.targetIds = new List<string>();
    } else {
      ((IncidentGraphGameObjectFlipState)saveData).CopyTo(flipSaveData);
    }
    
    AddStopGraphToggle(flipSaveData);
    InitPorts(
      flipSaveData.inputs,
      () => AddInputPort(CreateNewLink("Previous"), Port.Capacity.Multi, typeof(bool)),
      (link) => AddInputPort(link, Port.Capacity.Multi, typeof(bool))
    );
    InitPorts(
      flipSaveData.outputs,
      () => AddOutputPort(CreateNewLink("Next"), Port.Capacity.Single, typeof(bool)),
      (link) => AddOutputPort(link, Port.Capacity.Single, typeof(bool))
    );

    VisualElement customDataContainer = new VisualElement();
    customDataContainer.AddToClassList("ig-node__custom-data-container");

    VisualElement addKeyContainer = new VisualElement();
    addKeyContainer.AddToClassList("ig-node__button-flex-container");
    Button addKeyButton = IncidentGraphUtility.CreateButton(
      "Add Object",
      () => AddDefaultTargetField(customDataContainer)
    );
    addKeyButton.AddToClassList("ig-node__button-add");
    addKeyContainer.Add(addKeyButton);

    if (flipSaveData.targetIds.Count > 0) {
      for (int i = 0; i < flipSaveData.targetIds.Count; i++) {
        AddTargetField(i, customDataContainer);
      }
    } else {
      AddDefaultTargetField(customDataContainer);
    }
    
    customDataContainer.Insert(0, addKeyContainer);
    extensionContainer.Add(customDataContainer);
    RefreshExpandedState();
    RefreshPorts();
  }

  void AddDefaultTargetField(VisualElement container) {
    invalidFieldCount++;
    flipSaveData.targetIds.Add(string.Empty);
    AddTargetField(flipSaveData.targetIds.Count-1, container);
  }

  void AddTargetField(int index, VisualElement container) {
    VisualElement targetFieldContainer = new VisualElement();
    targetFieldContainer.AddToClassList("ig-node__object-field-container");

    TextField targetField = IncidentGraphUtility.CreateTextField(
      flipSaveData.targetIds[index],
      null,
      (change) => {
        string newValue = change.newValue.Trim();
        if (flipSaveData.targetIds[index] == newValue) return;
        bool wasValid = IsValidTargetId(flipSaveData.targetIds[index]);
        bool newValid = IsValidTargetId(newValue);
        if (wasValid && !newValid) {
          invalidFieldCount++;
        } else if (!wasValid && newValid) {
          invalidFieldCount--;
        }
        flipSaveData.targetIds[index] = newValue;
      }
    );
    targetField.AddClasses(
      "ig-node__field__input",
      "ig-node__gameobject-id-textfield"
    );

    Button deleteButton = IncidentGraphUtility.CreateButton("x", () => {
      if (targetFieldCount == 1) return;
      if (!IsValidTargetId(flipSaveData.targetIds[index])) {
        invalidFieldCount--;
      }
      targetFieldCount--;
      flipSaveData.targetIds[index] = string.Empty;
      container.Remove(targetFieldContainer);
    });

    targetFieldContainer.Add(targetField);
    targetFieldContainer.Add(deleteButton);
    container.Add(targetFieldContainer);
    targetFieldCount++;
  }

}
