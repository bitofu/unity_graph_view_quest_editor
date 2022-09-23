using System;
using UnityEngine;

public class IncidentGraphActorAnimation : IncidentGraphActivity {
  
  [ReadOnly] public bool waitForAnimation;
  [ReadOnly] public ActorType targetActor;
  [ReadOnly] public string animationTrigger;

  public override void CopyTo(IncidentGraphActivity data) {
    base.CopyTo(data);
    IncidentGraphActorAnimation actorAnimationSaveData = (IncidentGraphActorAnimation)data;
    actorAnimationSaveData.waitForAnimation = waitForAnimation;
    actorAnimationSaveData.targetActor = targetActor;
    actorAnimationSaveData.animationTrigger = animationTrigger;
  }

  public override void Run(IncidentGraphManager manager, Action<string, bool> cb) {
    Actor actor = Game.actors[targetActor];
    actor.TriggerAnimation(animationTrigger);
    if (waitForAnimation) {
      Action onAnimationEnd = null;
      onAnimationEnd = () => {
        cb.Invoke(defaultNextId, stopGraph);
        actor.AnimationEndEvent -= onAnimationEnd;
      };
      actor.AnimationEndEvent += onAnimationEnd;
    } else {
      cb.Invoke(defaultNextId, stopGraph);
    }
  }

}
