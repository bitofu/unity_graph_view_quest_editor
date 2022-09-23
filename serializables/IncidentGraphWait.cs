using System;
using UnityEngine;

public class IncidentGraphWait : IncidentGraphActivity {

  [ReadOnly] public float duration;

  public override void CopyTo(IncidentGraphActivity data) {
    base.CopyTo(data);
    IncidentGraphWait waitSaveData = (IncidentGraphWait)data;
    waitSaveData.duration = duration;
  }

  public override void Run(IncidentGraphManager manager, Action<string, bool> cb) {
    manager.StartCoroutine(manager.InvokeWithDelay(duration, () => cb.Invoke(defaultNextId, stopGraph)));
  }

}
