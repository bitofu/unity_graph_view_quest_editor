using System;
using System.Collections.Generic;
using UnityEngine;

public class IncidentGraphGameObjectFlipState : IncidentGraphActivity {

  [NonReorderable][ReadOnly] public List<string> targetIds;

  public override void CopyTo(IncidentGraphActivity data) {
    base.CopyTo(data);
    IncidentGraphGameObjectFlipState toggleSaveData = (IncidentGraphGameObjectFlipState)data;
    toggleSaveData.targetIds = new List<string>();
    for (int i = 0; i < targetIds.Count; i++) {
      if (!string.IsNullOrEmpty(targetIds[i])) {
        toggleSaveData.targetIds.Add(targetIds[i]);
      }
    }
  }

  public override void Run(IncidentGraphManager manager, Action<string, bool> cb) {
    for (int i = 0; i < targetIds.Count; i++) {
      List<GameObject> objectList = manager.mapManager.GetObjects(targetIds[i]);
      objectList?.ForEach((o) => o.SetActive(!o.activeInHierarchy));
    }
    cb.Invoke(defaultNextId, stopGraph);
  }

}
