using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncidentGraphActorToggle : IncidentGraphActivity {

  [ReadOnly] public bool targetState;
  [ReadOnly] public bool hideOnDisable;
  [ReadOnly] public ActorType targetActor;

  public override void CopyTo(IncidentGraphActivity data) {
    base.CopyTo(data);
    IncidentGraphActorToggle actorToggleSaveData = (IncidentGraphActorToggle)data;
    actorToggleSaveData.targetState = targetState;
    actorToggleSaveData.hideOnDisable = hideOnDisable;
    actorToggleSaveData.targetActor = targetActor;
  }

  public override void Run(IncidentGraphManager manager, Action<string, bool> cb) {
    Actor actor = Game.actors[targetActor];
    if (targetState == true) {
      actor.TryEnable();
    } else {
      actor.AddDisable();
      actor.TerminateActivity(false);
      actor.SetVelocity(Vector3.zero);
      if (hideOnDisable) {
        actor.SetPosition(Vector3.up*10000);
      }
    }
    cb.Invoke(defaultNextId, stopGraph);
  }

}
