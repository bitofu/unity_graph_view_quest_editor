using System;
using System.Collections.Generic;
using UnityEngine;

public class IncidentGraphKeyBranching : IncidentGraphActivity {

  [Serializable]
  public class BranchKey {
    [ReadOnly] public string portId;
    [ReadOnly] public IncidentKeyAbstract key;
  }

  [NonReorderable] public List<BranchKey> keys;

  public BranchKey GetBranch(string portId) {
    return keys.Find((key) => key.portId == portId);
  }

  public override void CopyTo(IncidentGraphActivity data) {
    base.CopyTo(data);
    IncidentGraphKeyBranching keyBranchingSaveData = (IncidentGraphKeyBranching)data;
    keyBranchingSaveData.keys = new List<BranchKey>();
    foreach (BranchKey branch in keys) {
      keyBranchingSaveData.keys.Add(new BranchKey() {
        portId = branch.portId,
        key = branch.key 
      });
    }
  }

  public override void Run(IncidentGraphManager manager, Action<string, bool> cb) {
    for (int i = 1; i < outputs.Count; i++) {
      if (keys[i].key.isValid) {
        cb.Invoke(outputs[i].linkedNodeId, stopGraph);
        return;
      }
    }
    cb.Invoke(outputs[0].linkedNodeId, stopGraph);
  }

}
