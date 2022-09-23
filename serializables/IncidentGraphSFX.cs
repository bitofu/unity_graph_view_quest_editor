using System;
using System.Collections.Generic;
using UnityEngine;

public class IncidentGraphSFX : IncidentGraphActivity {

  [Serializable]
  public class SFXUnit {
    [FMODUnity.EventRef]
    public string fmodEvent;
    [ReadOnly] public string targetId;
    [ReadOnly] public ActorType targetActor;

    public SFXUnit Clone() {
      return new SFXUnit() {
        fmodEvent = fmodEvent,
        targetId = targetId,
        targetActor = targetActor
      };
    }
  }

  [NonReorderable] public List<SFXUnit> sfxUnits;

  public override void CopyTo(IncidentGraphActivity data) {
    base.CopyTo(data);
    IncidentGraphSFX sfxSaveData = (IncidentGraphSFX)data;    
    sfxSaveData.sfxUnits = new List<SFXUnit>();
    foreach (SFXUnit unit in sfxUnits) {
      if (string.IsNullOrEmpty(unit.fmodEvent)) continue;
      sfxSaveData.sfxUnits.Add(unit.Clone());
    }
  }

  public override void Run(IncidentGraphManager manager, Action<string, bool> cb) {
    foreach (SFXUnit unit in sfxUnits) {
      Transform target = manager.transform;
      bool isAttached = !string.IsNullOrEmpty(unit.targetId) || unit.targetActor;
      if (!string.IsNullOrEmpty(unit.targetId)) {
        target = manager.GetTargetFromId(unit.targetId);
      } else if (unit.targetActor) {
        target = Game.actors[unit.targetActor].transform;
      }
      Game.audioManager.PlayOneShot(target, unit.fmodEvent, isAttached);
    }
    cb.Invoke(defaultNextId, stopGraph);
  }

}
