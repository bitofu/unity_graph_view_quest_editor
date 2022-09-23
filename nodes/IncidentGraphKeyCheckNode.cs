using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class IncidentGraphKeyCheckNode : IncidentGraphNode {

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

  IncidentGraphKeyCheck keyCheckSaveData;

  public override void CopyTo(IncidentGraphActivity target) => keyCheckSaveData.CopyTo(target);

  public override void Init(IncidentGraphView graphView, Vector2 position, IncidentGraphActivity saveData) {
    base.Init(graphView, position, saveData);
    title = "Key Check";
    keyCheckSaveData = (IncidentGraphKeyCheck)baseData;
    if (saveData == null) {
      keyCheckSaveData.keys = new List<IncidentKeyAbstract>();
    } else {
      ((IncidentGraphKeyCheck)saveData).CopyTo(keyCheckSaveData);
    }
    
    AddStopGraphToggle(keyCheckSaveData);
    InitPorts(
      keyCheckSaveData.inputs,
      () => AddInputPort(CreateNewLink("Previous"), Port.Capacity.Multi, typeof(bool)),
      (link) => AddInputPort(link, Port.Capacity.Multi, typeof(bool))
    );
    InitPorts(
      keyCheckSaveData.outputs,
      () => {
        AddOutputPort(CreateNewLink("On Success"), Port.Capacity.Single, typeof(bool));
        AddOutputPort(CreateNewLink("On Fail"), Port.Capacity.Single, typeof(bool));
      },
      (link) => AddOutputPort(link, Port.Capacity.Single, typeof(bool))
    );

    VisualElement customDataContainer = new VisualElement();
    customDataContainer.AddToClassList("ig-node__custom-data-container");

    VisualElement addKeyContainer = new VisualElement();
    addKeyContainer.AddToClassList("ig-node__button-flex-container");
    Button addKeyButton = IncidentGraphUtility.CreateButton("Add Key", () => {
      invalidFieldCount++;
      keyCheckSaveData.keys.Add(null);
      AddKeyField(keyCheckSaveData.keys.Count-1, null, customDataContainer);
    });
    addKeyButton.AddToClassList("ig-node__button-add");
    addKeyContainer.Add(addKeyButton);

    if (keyCheckSaveData.keys.Count > 0) {
      for (int i = 0; i < keyCheckSaveData.keys.Count; i++) {
        AddKeyField(i, keyCheckSaveData.keys[i], customDataContainer);
      }
    } else {
      invalidFieldCount++;
      keyCheckSaveData.keys.Add(null);
      AddKeyField(keyCheckSaveData.keys.Count-1, null, customDataContainer);
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
      (change) => {
        bool isEmpty = change.newValue == null;
        if (isEmpty) {
          invalidFieldCount++;
        } else {
          invalidFieldCount--;
        }
        keyCheckSaveData.keys[index] = (IncidentKeyAbstract)change.newValue;
      }
    );
    objectField.AddToClassList("ig-node__key-object-field");

    Button deleteButton = IncidentGraphUtility.CreateButton("x", () => {
      if (objectFieldCount <= 1) return;
      if (keyCheckSaveData.keys[index] == null) {
        invalidFieldCount--;
      }
      keyCheckSaveData.keys[index] = null;
      container.Remove(keyFieldContainer);
    });

    keyFieldContainer.Add(objectField);
    keyFieldContainer.Add(deleteButton);
    container.Add(keyFieldContainer);
    objectFieldCount++;
  }

}
