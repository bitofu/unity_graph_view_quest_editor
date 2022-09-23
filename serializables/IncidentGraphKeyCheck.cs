using System;
using System.Collections.Generic;
using UnityEngine;

public class IncidentGraphKeyCheck : IncidentGraphActivity {

  [NonReorderable][ReadOnly] public List<IncidentKeyAbstract> keys;

  public override void CopyTo(IncidentGraphActivity data) {
    base.CopyTo(data);
    IncidentGraphKeyCheck keyCheckSaveData = (IncidentGraphKeyCheck)data;
    keyCheckSaveData.keys = new List<IncidentKeyAbstract>();
    foreach (IncidentKeyAbstract key in keys) {
      if (key != null) keyCheckSaveData.keys.Add(key);
    }
  }

  public override void Run(IncidentGraphManager manager, Action<string, bool> cb) {
    string nextId = IsValid() ? successNextId : failNextId;
    cb.Invoke(nextId, stopGraph);
  }

  bool IsValid() {
    bool isValid = true;
    foreach (IncidentKeyAbstract key in keys) {
      if (!key.isValid) return false;
    }
    return isValid;
  }

}
