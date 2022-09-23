using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class IncidentGraphCameraNode : IncidentGraphNode {

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

  IncidentGraphCameraManipulation manipulation => cameraSaveData.manipulation;
  IncidentGraphCamera cameraSaveData;
  TextField targetIdField;
  FloatField cameraSpeedField;
  FloatField startDelayField;

  bool isManipulationTypeRelease => manipulation.type == IncidentGraphCameraManipulation.Type.Release;

  public override void CopyTo(IncidentGraphActivity target) => cameraSaveData.CopyTo(target);

  public override void Init(IncidentGraphView graphView, Vector2 position, IncidentGraphActivity saveData) {
    base.Init(graphView, position, saveData);
    title = "Camera";
    cameraSaveData = (IncidentGraphCamera)baseData;
    if (saveData == null) {
      cameraSaveData.manipulation = new IncidentGraphCameraManipulation() {
        type = IncidentGraphCameraManipulation.Type.Release,
        moveToSpeed = 5,
        waitForCamera = true
      };
    } else {
      ((IncidentGraphCamera)saveData).CopyTo(cameraSaveData);
    }

    AddStopGraphToggle(cameraSaveData);
    AddPlayerCanActToggle(cameraSaveData);
    InitPorts(
      cameraSaveData.inputs,
      () => AddInputPort(CreateNewLink("Previous"), Port.Capacity.Multi, typeof(bool)),
      (link) => AddInputPort(link, Port.Capacity.Multi, typeof(bool))
    );
    InitPorts(
      cameraSaveData.outputs,
      () => AddOutputPort(CreateNewLink("Next"), Port.Capacity.Single, typeof(bool)),
      (link) => AddOutputPort(link, Port.Capacity.Single, typeof(bool))
    );

    VisualElement customDataContainer = new VisualElement();
    customDataContainer.AddToClassList("ig-node__custom-data-container");
    
    EnumField manipulationField = IncidentGraphUtility.CreateEnumField(
      manipulation.type,
      null,
      (change) => {
        manipulation.type = (IncidentGraphCameraManipulation.Type)change.newValue;
        if (isManipulationTypeRelease && invalidFieldCount != 0) {
          invalidFieldCount = 0;
        } else if (invalidFieldCount == 0 && !IsValidTargetId(manipulation.targetId)) {
          invalidFieldCount++;
          manipulation.targetId = string.Empty;
          targetIdField.SetValueWithoutNotify(manipulation.targetId);
        }
      }
    );
    manipulationField.SetValueWithoutNotify(manipulation.type);

    targetIdField = IncidentGraphUtility.CreateTextField(
      manipulation.targetId,
      "Target ID:",
      (change) => {
        string newValue = change.newValue.Trim();
        if (manipulation.targetId == newValue) return;
        if (!isManipulationTypeRelease) {
          bool wasValid = IsValidTargetId(manipulation.targetId);
          bool newValid = IsValidTargetId(newValue);
          if (wasValid && !newValid) {
            invalidFieldCount++;
          } else if (!wasValid && newValid) {
            invalidFieldCount--;
          }
        }
        manipulation.targetId = newValue;
      }
    );
    targetIdField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );

    startDelayField = IncidentGraphUtility.CreateFloatField(
      "Start Delay:",
      manipulation.startDelay,
      (change) => {
      manipulation.startDelay = change.newValue;
      if (manipulation.startDelay < 0) {
        manipulation.startDelay = 0;
        startDelayField.SetValueWithoutNotify(manipulation.startDelay);
      }
    });
    startDelayField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );

    cameraSpeedField = IncidentGraphUtility.CreateFloatField(
      "Track/Anchor Spd:",
      manipulation.moveToSpeed,
      (change) => {
        manipulation.moveToSpeed = change.newValue;
        if (manipulation.moveToSpeed < 0.1f) {
          manipulation.moveToSpeed = 0.1f;
          cameraSpeedField.SetValueWithoutNotify(manipulation.moveToSpeed);
        }
      }
    );
    cameraSpeedField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );

    Toggle waitForCameraToggle = IncidentGraphUtility.CreateToggle(
      manipulation.waitForCamera,
      "Wait for Camera",
      null,
      (change) => {
        manipulation.waitForCamera = change.newValue;
      }
    );
    waitForCameraToggle.AddClasses(
      "ig-node__label",
      "ig-node__toggle"
    );
    
    if (!isManipulationTypeRelease && !IsValidTargetId(manipulation.targetId)) {
      invalidFieldCount++;
    }

    customDataContainer.Add(manipulationField);
    customDataContainer.Add(targetIdField);
    customDataContainer.Add(startDelayField);
    customDataContainer.Add(cameraSpeedField);
    customDataContainer.Add(waitForCameraToggle);
    extensionContainer.Add(customDataContainer);
    RefreshExpandedState();
    RefreshPorts();
  }

}
