using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncidentGraphCamera : IncidentGraphActivity {

  public IncidentGraphCameraManipulation manipulation;

  public override void CopyTo(IncidentGraphActivity data) {
    base.CopyTo(data);
    IncidentGraphCamera cameraSaveData = (IncidentGraphCamera)data;
    cameraSaveData.manipulation = manipulation.Clone();
  }

  public override void Run(IncidentGraphManager manager, Action<string, bool> cb) {
    if (string.IsNullOrEmpty(manipulation.targetId)) {
      cb.Invoke(defaultNextId, stopGraph);
      return;
    }

    Transform target = manager.GetTargetFromId(manipulation.targetId);
    CameraManipulation cameraManipulation = manipulation.BuildManipulation(manager, target);
    if (manipulation == null) {
      Game.camera.ReleaseOverrideManipulation(manipulation.startDelay);
    } else {
      Game.camera.SetOverrideManipulation(cameraManipulation);
    }
    if (manipulation.waitForCamera) {
      manager.StartCoroutine(manager.InvokeWithDelay(manipulation.startDelay, () => {
        Game.camera.OnOverrideManipulationFinish(() => {
          cb?.Invoke(defaultNextId, stopGraph);
        });
      }));
    } else {
      cb.Invoke(defaultNextId, stopGraph);
    }
  }

}
