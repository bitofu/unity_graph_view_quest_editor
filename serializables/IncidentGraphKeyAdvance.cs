using System;
using System.Collections.Generic;
using UnityEngine;

public class IncidentGraphKeyAdvance : IncidentGraphActivity {

  [NonReorderable][ReadOnly] public List<IncidentKeyAbstract> keys;

  public override void CopyTo(IncidentGraphActivity data) {
    base.CopyTo(data);
    IncidentGraphKeyAdvance keyAdvanceSaveData = (IncidentGraphKeyAdvance)data;
    keyAdvanceSaveData.keys = new List<IncidentKeyAbstract>();
    foreach (IncidentKeyAbstract key in keys) {
      if (key != null) keyAdvanceSaveData.keys.Add(key);
    }
  }
  
  public override void Run(IncidentGraphManager manager, Action<string, bool> cb) {
    foreach (IncidentKeyAbstract key in keys) {
      key.Advance();
    }
    cb.Invoke(defaultNextId, stopGraph);
  }

}
