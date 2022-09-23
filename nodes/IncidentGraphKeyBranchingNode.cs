using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class IncidentGraphKeyBranchingNode : IncidentGraphNode {

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

  IncidentGraphKeyBranching keyBranchingSaveData;
  VisualElement branchContainer;

  public override void CopyTo(IncidentGraphActivity target) => keyBranchingSaveData.CopyTo(target);

  public override void Init(IncidentGraphView graphView, Vector2 position, IncidentGraphActivity saveData) {
    base.Init(graphView, position, saveData);
    title = "Key Branching";
    keyBranchingSaveData = (IncidentGraphKeyBranching)baseData;
    if (saveData == null) {
      keyBranchingSaveData.keys = new List<IncidentGraphKeyBranching.BranchKey>();
    } else {
      ((IncidentGraphKeyBranching)saveData).CopyTo(keyBranchingSaveData);
    }

    AddStopGraphToggle(keyBranchingSaveData);
    InitPorts(
      keyBranchingSaveData.inputs,
      () => AddInputPort(CreateNewLink("Previous"), Port.Capacity.Multi, typeof(bool)),
      (link) => AddInputPort(link, Port.Capacity.Multi, typeof(bool))
    );
    IncidentGraphNodeLink defaultLink = null;
    if (keyBranchingSaveData.outputs.Count > 0) {
      defaultLink = keyBranchingSaveData.outputs[0];
    } else {
      defaultLink = CreateNewLink("Default");
      keyBranchingSaveData.keys.Add(new IncidentGraphKeyBranching.BranchKey() {
        portId = defaultLink.portId
      });
    }
    AddOutputPort(defaultLink, Port.Capacity.Single, typeof(bool));

    branchContainer = new VisualElement();
    outputContainer.Insert(0, branchContainer);

    if (keyBranchingSaveData.outputs.Count == 1) {
      CreateBranchPortWithKey(CreateNewLink(string.Empty));
    } else {
      for (int i = 1; i < keyBranchingSaveData.outputs.Count; i++) {
        CreateBranchPortWithKey(keyBranchingSaveData.outputs[i]);
      }
    }

    VisualElement customDataContainer = new VisualElement();
    customDataContainer.AddToClassList("ig-node__custom-data-container");

    Button addChoiceButton = IncidentGraphUtility.CreateButton("Add Branch", () => {
      CreateBranchPortWithKey(CreateNewLink(string.Empty));
      RefreshPorts();
    });

    customDataContainer.Add(addChoiceButton);
    extensionContainer.Add(customDataContainer);
    RefreshExpandedState();
    RefreshPorts();
  }

  void CreateBranchPortWithKey(IncidentGraphNodeLink link) {
    IncidentGraphKeyBranching.BranchKey branchKey = keyBranchingSaveData.GetBranch(link.portId);
    if (branchKey == null) {
      branchKey = new IncidentGraphKeyBranching.BranchKey() {
        portId = link.portId
      };
      keyBranchingSaveData.keys.Add(branchKey);
    }
    
    Port branchPort = AddOutputPort(link, Port.Capacity.Single, typeof(bool));
    Button deleteChoiceButton = IncidentGraphUtility.CreateButton("X", () => {
      if (keyBranchingSaveData.outputs.Count == 2) return;
      if (branchPort.connected) {
        graphView.DeleteElements(branchPort.connections);
      }
      graphView.RemoveElement(branchPort);
      graphView.portMap.Remove(link.portId);
      keyBranchingSaveData.outputs.Remove(link);
      keyBranchingSaveData.keys.Remove(branchKey);
    });

    ObjectField objectField = IncidentGraphUtility.CreateObjectField(
      branchKey.key,
      null,
      typeof(IncidentKeyAbstract),
      (context) => {
        bool isEmpty = context.newValue == null;
        if (isEmpty) {
          invalidFieldCount++;
        } else {
          invalidFieldCount--;
        }
        branchKey.key = (IncidentKeyAbstract)context.newValue;
      }
    );
    objectField.AddToClassList("ig-node__key-object-field");

    branchPort.Add(deleteChoiceButton);
    branchPort.Add(objectField);
    branchContainer.Add(branchPort);
  }

}
