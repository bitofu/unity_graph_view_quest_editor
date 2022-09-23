using System;
using System.Collections.Generic;
using UnityEngine;

public class IncidentGraphGameObjectToggle : IncidentGraphActivity {

  [NonReorderable][ReadOnly] public List<string> targetIds;
  [NonReorderable][ReadOnly] public List<bool> targetStates;

  public override void CopyTo(IncidentGraphActivity data) {
    base.CopyTo(data);
    IncidentGraphGameObjectToggle toggleSaveData = (IncidentGraphGameObjectToggle)data;
    toggleSaveData.targetIds = new List<string>();
    toggleSaveData.targetStates = new List<bool>();
    for (int i = 0; i < targetIds.Count; i++) {
      if (!string.IsNullOrEmpty(targetIds[i])) {
        toggleSaveData.targetIds.Add(targetIds[i]);
        toggleSaveData.targetStates.Add(targetStates[i]);
      }
    }
  }

  public override void Run(IncidentGraphManager manager, Action<string, bool> cb) {
    for (int i = 0; i < targetIds.Count; i++) {
      List<GameObject> objectList = manager.mapManager.GetObjects(targetIds[i]);
      objectList?.ForEach((o) => {
        if (o != null) o.SetActive(targetStates[i]);
      });
    }
    cb.Invoke(defaultNextId, stopGraph);
  }

}
