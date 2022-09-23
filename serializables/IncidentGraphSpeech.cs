using System;
using System.Collections.Generic;
using UnityEngine;

public class IncidentGraphSpeech : IncidentGraphActivity {

  public IncidentGraphCameraManipulation cameraManipulation;
  [NonReorderable] public List<IncidentGraphSpeechUnit> speechUnits;

  public override void CopyTo(IncidentGraphActivity data) {
    base.CopyTo(data);
    IncidentGraphSpeech speechSaveData = (IncidentGraphSpeech)data;
    speechSaveData.cameraManipulation = cameraManipulation.Clone();
    speechSaveData.speechUnits = new List<IncidentGraphSpeechUnit>();
    for (int i = 0; i < speechUnits.Count; i++) {
      speechSaveData.speechUnits.Add(speechUnits[i].Clone());
    }
  }

  public override void Run(IncidentGraphManager manager, Action<string, bool> cb) {
    if (!playerCanAct) {
      Game.player.AddDisable();
      Game.player.SetVelocity(Vector3.zero);
      Game.player.TriggerAnimation(Constants.triggerIdle);
    }
    List<string> choiceKeys = new List<string>() {
      defaultNextId
    };
    for (int i = 0; i < speechUnits.Count; i++) {
      List<EmoteManager.EmoteData> emotes = new List<EmoteManager.EmoteData>();
      List<SpeechManager.Animation> animations = new List<SpeechManager.Animation>();
      foreach (IncidentGraphSpeechEmote emote in speechUnits[i].emotes) {
        Transform target = emote.target.GetTransform(manager);
        emotes.Add(new EmoteManager.EmoteData(target, emote));
      }
      foreach (IncidentGraphSpeechAnimation animation in speechUnits[i].animations) {
        Transform target = animation.target.GetTransform(manager);
        animations.Add(new SpeechManager.Animation(target, animation.trigger, animation.startDelay));
      }
      SpeechActivity activity = new SpeechActivity() {
        nameplate = speechUnits[i].nameplate,
        text = speechUnits[i].text,
        playerCanAct = playerCanAct,
        hasChoice = false,
        choiceKeys = choiceKeys,
        audio = new SpeechManager.Audio(speechUnits[i].audio, manager),
        emotes = emotes,
        animations = animations
      };
      if (i == 0 && cameraManipulation.targetId != manager.previousCameraManipulation?.targetId) {
        AttachCameraManipulation(manager, activity, cameraManipulation);
      }
      if (i == speechUnits.Count-1) {
        activity.OnStop((nextId) => {
          if (!playerCanAct) Game.player.TryEnable();
          cb?.Invoke(nextId, stopGraph);
        });
      }
      Game.speechManager.Enqueue(activity);
    }
  }

  public static void AttachCameraManipulation(
    IncidentGraphManager manager,
    SpeechActivity activity,
    IncidentGraphCameraManipulation cameraManipulationData
  ) {
    if (string.IsNullOrEmpty(cameraManipulationData.targetId)) return;

    bool isPlayer = cameraManipulationData.targetId == "$default"
      && (activity.nameplate == Game.expectedPlayerName || activity.nameplate == Constants.bacchusName);
    string targetId = isPlayer ? "$player" : cameraManipulationData.targetId;
    Transform target = manager.GetTargetFromId(targetId);
    CameraManipulation manipulation = cameraManipulationData.BuildManipulation(manager, target);
    activity.OnStart(() => {
      manager.previousCameraManipulation = manipulation;
      if (manipulation == null) {
        Game.camera.ReleaseManipulation(manager, cameraManipulationData.startDelay);
      } else {
        Game.camera.SetManipulation(manipulation);
      }
    });
  }

}
