using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class IncidentGraphKeyAdvanceNode : IncidentGraphNode {

  int objectFieldCount;
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

  IncidentGraphKeyAdvance keyAdvanceSaveData;

  public override void CopyTo (IncidentGraphActivity target) => keyAdvanceSaveData.CopyTo(target);

  public override void Init(IncidentGraphView graphView, Vector2 position, IncidentGraphActivity saveData) {
    base.Init(graphView, position, saveData);
    title = "Key Advance";
    keyAdvanceSaveData = (IncidentGraphKeyAdvance)baseData;
    if (saveData == null) {
      keyAdvanceSaveData.keys = new List<IncidentKeyAbstract>();
    } else {
      ((IncidentGraphKeyAdvance)saveData).CopyTo(keyAdvanceSaveData);
    }
    
    AddStopGraphToggle(keyAdvanceSaveData);
    InitPorts(
      keyAdvanceSaveData.inputs,
      () => AddInputPort(CreateNewLink("Previous"), Port.Capacity.Multi, typeof(bool)),
      (link) => AddInputPort(link, Port.Capacity.Multi, typeof(bool))
    );
    InitPorts(
      keyAdvanceSaveData.outputs,
      () => AddOutputPort(CreateNewLink("Next"), Port.Capacity.Single, typeof(bool)),
      (link) => AddOutputPort(link, Port.Capacity.Single, typeof(bool))
    );

    VisualElement customDataContainer = new VisualElement();
    customDataContainer.AddToClassList("ig-node__custom-data-container");

    VisualElement addKeyContainer = new VisualElement();
    addKeyContainer.AddToClassList("ig-node__button-flex-container");
    Button addKeyButton = IncidentGraphUtility.CreateButton("Add Key", () => {
      invalidFieldCount++;
      keyAdvanceSaveData.keys.Add(null);
      AddKeyField(keyAdvanceSaveData.keys.Count-1, null, customDataContainer);
    });
    addKeyButton.AddToClassList("ig-node__button-add");
    addKeyContainer.Add(addKeyButton);

    if (keyAdvanceSaveData.keys.Count > 0) {
      for (int i = 0; i < keyAdvanceSaveData.keys.Count; i++) {
        AddKeyField(i, keyAdvanceSaveData.keys[i], customDataContainer);
      }
    } else {
      invalidFieldCount++;
      keyAdvanceSaveData.keys.Add(null);
      AddKeyField(keyAdvanceSaveData.keys.Count-1, null, customDataContainer);
    }
    
    customDataContainer.Insert(0, addKeyContainer);
    extensionContainer.Add(customDataContainer);
    RefreshExpandedState();
    RefreshPorts();
  }

  void AddKeyField(int index, IncidentKeyAbstract key, VisualElement container) {
    VisualElement keyFieldContainer = new VisualElement();
    keyFieldContainer.AddToClassList("ig-node__object-field-container");

    ObjectField objectField = IncidentGraphUtility.CreateObjectField(
      key,
      null,
      typeof(IncidentKeyAbstract),
      (context) => {
        bool isEmpty = context.newValue == null;
        if (isEmpty) {
          invalidFieldCount++;
        } else {
          invalidFieldCount--;
        }
        keyAdvanceSaveData.keys[index] = (IncidentKeyAbstract)context.newValue;
      }
    );
    objectField.AddToClassList("ig-node__key-object-field");

    Button deleteButton = IncidentGraphUtility.CreateButton("x", () => {
      if (objectFieldCount == 1) return;
      if (keyAdvanceSaveData.keys[index] == null) {
        invalidFieldCount--;
      }
      keyAdvanceSaveData.keys[index] = null;
      container.Remove(keyFieldContainer);
    });

    keyFieldContainer.Add(objectField);
    keyFieldContainer.Add(deleteButton);
    container.Add(keyFieldContainer);
    objectFieldCount++;
  }

}
