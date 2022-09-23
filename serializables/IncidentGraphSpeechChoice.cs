using System;
using System.Collections.Generic;
using UnityEngine;

public class IncidentGraphSpeechChoice : IncidentGraphActivity {

  [Serializable]
  public class SpeechForChoice {
    [ReadOnly] public string portId;
    // [ReadOnly] public string nextNodeId;
    [NonReorderable] public List<IncidentGraphSpeechUnit> speechUnits;
  }

  public IncidentGraphCameraManipulation cameraManipulation;
  [ReadOnly] public float timeLimit;
  [NonReorderable] public List<SpeechForChoice> speechForChoices;

  // public List<IncidentGraphSpeechUnit> GetSpeechUnitsForPort(string portId) {
  //   List<IncidentGraphSpeechUnit> speechUnits = new List<IncidentGraphSpeechUnit>();
  //   SpeechForChoice choice = speechForChoices.Find((c) => c.portId == portId);
  //   for (int i = 0; i < choice?.speechUnits.Count; i++) {
  //     speechUnits.Add(choice.speechUnits[i]);
  //   }
  //   return speechUnits;
  // }

  public override void CopyTo(IncidentGraphActivity data) {
    base.CopyTo(data);
    IncidentGraphSpeechChoice choiceSaveData = (IncidentGraphSpeechChoice)data;
    choiceSaveData.cameraManipulation = cameraManipulation.Clone();
    choiceSaveData.speechForChoices = new List<SpeechForChoice>();
    for (int i = 0; i < speechForChoices.Count; i++) {
      List<IncidentGraphSpeechUnit> newSpeechUnits = new List<IncidentGraphSpeechUnit>();
      for (int j = 0; j < speechForChoices[i].speechUnits.Count; j++) {
        newSpeechUnits.Add(speechForChoices[i].speechUnits[j].Clone());
      }
      choiceSaveData.speechForChoices.Add(new SpeechForChoice() {
        portId = speechForChoices[i].portId,
        // nextNodeId = speechForChoices[i].nextNodeId,
        speechUnits = newSpeechUnits
      });
    }
  }

  public override void Run(IncidentGraphManager manager, Action<string, bool> cb) {
    if (!playerCanAct) {
      Game.player.AddDisable();
      Game.player.SetVelocity(Vector3.zero);
      Game.player.TriggerAnimation(Constants.triggerIdle);
    }
    List<string> choiceKeys = new List<string>();
    List<string> choiceTexts = new List<string>();
    foreach (IncidentGraphNodeLink link in outputs) {
      choiceKeys.Add(link.portId);
      choiceTexts.Add(link.text);
    }
    SpeechActivity choiceActivity = new SpeechActivity() {
      text = string.Empty,
      playerCanAct = playerCanAct,
      hasChoice = true,
      choiceKeys = choiceKeys,
      choiceTexts = choiceTexts,
      choiceTimeLimit = timeLimit
    };
    IncidentGraphSpeech.AttachCameraManipulation(manager, choiceActivity, cameraManipulation);
    choiceActivity.OnStop(OnChoice(manager, cb));
    Game.speechManager.Enqueue(choiceActivity);
  }

  Action<string> OnChoice(IncidentGraphManager manager, Action<string, bool> cb) {
    return (portId) => {
      if (string.IsNullOrEmpty(portId)) {
        cb.Invoke(string.Empty, stopGraph);
        return;
      }
      
      IncidentGraphNodeLink link = outputs.Find((o) => o.portId == portId);
      SpeechForChoice choice = speechForChoices.Find((c) => c.portId == portId);
      if (choice.speechUnits.Count == 0) {
        if (!playerCanAct) Game.player.TryEnable();
        cb.Invoke(link.linkedNodeId, stopGraph);
        return;
      }
      for (int i = 0; i < choice.speechUnits.Count; i++) {
        IncidentGraphSpeechUnit speechUnit = choice.speechUnits[i];
        List<EmoteManager.EmoteData> emotes = new List<EmoteManager.EmoteData>();
        List<SpeechManager.Animation> animations = new List<SpeechManager.Animation>();
        foreach (IncidentGraphSpeechEmote emote in speechUnit.emotes) {
          Transform target = emote.target.GetTransform(manager);
          emotes.Add(new EmoteManager.EmoteData(target, emote));
        }
        foreach (IncidentGraphSpeechAnimation animation in speechUnit.animations) {
          Transform target = animation.target.GetTransform(manager);
          animations.Add(new SpeechManager.Animation(target, animation.trigger, animation.startDelay));
        }
        SpeechActivity activity = new SpeechActivity {
          nameplate = speechUnit.nameplate,
          text = speechUnit.text,
          playerCanAct = playerCanAct,
          hasChoice = false,
          choiceKeys = new List<string>() {link.linkedNodeId},
          audio = new SpeechManager.Audio(speechUnit.audio, manager),
          emotes = emotes,
          animations = animations
        };
        if (i == choice.speechUnits.Count-1) {
          activity.OnStop((nextId) => {
            if (!playerCanAct) Game.player.TryEnable();
            cb?.Invoke(nextId, stopGraph);
          });
        }
        Game.speechManager.Enqueue(activity);
      }
    };
  }

}
