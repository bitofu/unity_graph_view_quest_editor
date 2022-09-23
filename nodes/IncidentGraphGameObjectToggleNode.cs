using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class IncidentGraphGameObjectToggleNode : IncidentGraphNode {

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

  IncidentGraphGameObjectToggle toggleSaveData;

  public override void CopyTo(IncidentGraphActivity target) => toggleSaveData.CopyTo(target);

  public override void Init(IncidentGraphView graphView, Vector2 position, IncidentGraphActivity saveData) {
    base.Init(graphView, position, saveData);
    title = "GameObject Toggle";
    toggleSaveData = (IncidentGraphGameObjectToggle)baseData;
    if (saveData == null) {
      toggleSaveData.targetIds = new List<string>();
      toggleSaveData.targetStates = new List<bool>();
    } else {
      ((IncidentGraphGameObjectToggle)saveData).CopyTo(toggleSaveData);
    }
    
    AddStopGraphToggle(toggleSaveData);
    InitPorts(
      toggleSaveData.inputs,
      () => AddInputPort(CreateNewLink("Previous"), Port.Capacity.Multi, typeof(bool)),
      (link) => AddInputPort(link, Port.Capacity.Multi, typeof(bool))
    );
    InitPorts(
      toggleSaveData.outputs,
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

    if (toggleSaveData.targetIds.Count > 0) {
      for (int i = 0; i < toggleSaveData.targetIds.Count; i++) {
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
    toggleSaveData.targetIds.Add(string.Empty);
    toggleSaveData.targetStates.Add(false);
    AddTargetField(toggleSaveData.targetIds.Count-1, container);
  }

  void AddTargetField(int index, VisualElement container) {
    VisualElement targetFieldContainer = new VisualElement();
    targetFieldContainer.AddToClassList("ig-node__object-field-container");

    TextField targetField = IncidentGraphUtility.CreateTextField(
      toggleSaveData.targetIds[index],
      null,
      (change) => {
        string newValue = change.newValue.Trim();
        if (toggleSaveData.targetIds[index] == newValue) return;
        bool wasValid = IsValidTargetId(toggleSaveData.targetIds[index]);
        bool newValid = IsValidTargetId(newValue);
        if (wasValid && !newValid) {
          invalidFieldCount++;
        } else if (!wasValid && newValid) {
          invalidFieldCount--;
        }
        toggleSaveData.targetIds[index] = newValue;
      }
    );
    targetField.AddClasses(
      "ig-node__field__input",
      "ig-node__gameobject-id-textfield"
    );

    Toggle toggleField = IncidentGraphUtility.CreateToggle(
      toggleSaveData.targetStates[index],
      null,
      null,
      (change) => toggleSaveData.targetStates[index] = change.newValue
    );
    toggleField.AddToClassList("ig-node__toggle");

    Button deleteButton = IncidentGraphUtility.CreateButton("x", () => {
      if (targetFieldCount == 1) return;
      if (!IsValidTargetId(toggleSaveData.targetIds[index])) {
        invalidFieldCount--;
      }
      targetFieldCount--;
      toggleSaveData.targetIds[index] = string.Empty;
      container.Remove(targetFieldContainer);
    });

    targetFieldContainer.Add(targetField);
    targetFieldContainer.Add(toggleField);
    targetFieldContainer.Add(deleteButton);
    container.Add(targetFieldContainer);
    targetFieldCount++;
  }

}
