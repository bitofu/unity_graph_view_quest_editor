using System;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

[Serializable]
public class IncidentGraphSpeechUnit : ICloneable {
  [ReadOnly] public string nameplate;
  [ReadOnly] public string text;
  [ReadOnly] public IncidentGraphSpeechAudio audio;
  [NonReorderable] public List<IncidentGraphSpeechEmote> emotes;
  [NonReorderable] public List<IncidentGraphSpeechAnimation> animations;

  public IncidentGraphSpeechUnit(string nameplate, string text) {
    this.nameplate = nameplate;
    this.text = text;
    this.audio = new IncidentGraphSpeechAudio();
  }

  public IncidentGraphSpeechUnit Clone() => new IncidentGraphSpeechUnit(nameplate, text) {
    audio = audio.Clone(),
    emotes = CloneEmotes(),
    animations = CloneAnimations()
  };

  object ICloneable.Clone() => Clone();

  List<IncidentGraphSpeechEmote> CloneEmotes() {
    List<IncidentGraphSpeechEmote> copy = new List<IncidentGraphSpeechEmote>();
    foreach (IncidentGraphSpeechEmote emote in emotes) {
      if (emote.animation == null) continue;
      copy.Add(emote.Clone());
    }
    return copy;
  }

  List<IncidentGraphSpeechAnimation> CloneAnimations() {
    List<IncidentGraphSpeechAnimation> copy = new List<IncidentGraphSpeechAnimation>();
    foreach (IncidentGraphSpeechAnimation animation in animations) {
      if (string.IsNullOrEmpty(animation.trigger)) continue;
      copy.Add(animation.Clone());
    }
    return copy;
  }
}

[Serializable]
public class IncidentGraphSpeechAudio {
  [ReadOnly][EventRef] public string fmodEvent;
  [ReadOnly] public IncidentGraphTargetReference target;

  public IncidentGraphSpeechAudio() {
    target = new IncidentGraphTargetReference();
  }

  public IncidentGraphSpeechAudio Clone() {
    return new IncidentGraphSpeechAudio() {
      fmodEvent = fmodEvent,
      target = new IncidentGraphTargetReference() {
        useActorType = target.useActorType,
        objectId = target.objectId,
        actorType = target.actorType
      }
    };
  }
}

[Serializable]
public class IncidentGraphSpeechEmote {
  [ReadOnly] public float timeout;
  [ReadOnly] public bool loop;
  [ReadOnly] public SpriteAnimationAbstract animation;
  [ReadOnly] public IncidentGraphTargetReference target;

  public IncidentGraphSpeechEmote() {
    target = new IncidentGraphTargetReference();
  }

  public IncidentGraphSpeechEmote Clone() {
    return new IncidentGraphSpeechEmote() {
      timeout = timeout,
      loop = loop,
      animation = animation,
      target = new IncidentGraphTargetReference() {
        useActorType = target.useActorType,
        objectId = target.objectId,
        actorType = target.actorType
      }
    };
  }
}

[Serializable]
public class IncidentGraphSpeechAnimation {
  [ReadOnly] public float startDelay;
  [ReadOnly] public string trigger;
  [ReadOnly] public IncidentGraphTargetReference target;

  public IncidentGraphSpeechAnimation() {
    target = new IncidentGraphTargetReference();
  }

  public IncidentGraphSpeechAnimation Clone() {
    return new IncidentGraphSpeechAnimation() {
      startDelay = startDelay,
      trigger = trigger,
      target = new IncidentGraphTargetReference() {
        useActorType = target.useActorType,
        objectId = target.objectId,
        actorType = target.actorType
      }
    };
  }
}
