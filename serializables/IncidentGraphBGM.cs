using System;
using System.Collections.Generic;
using UnityEngine;

public class IncidentGraphBGM : IncidentGraphActivity {

  [Serializable]
  public class Track {
    [ReadOnly] public float strength = 1;
    [ReadOnly] public float timeTo;

    public Track Clone() {
      return new Track() {
        strength = strength,
        timeTo = timeTo
      };
    }
  }

  [ReadOnly] public bool stopBGM;
  [ReadOnly] public FMOD.Studio.STOP_MODE stopMode;
  [FMODUnity.EventRef]
  public string fmodEvent;
  [ReadOnly] public bool skipTransition;
  [NonReorderable] public List<Track> tracks;

  public override void CopyTo(IncidentGraphActivity data) {
    base.CopyTo(data);
    IncidentGraphBGM bgmSaveData = (IncidentGraphBGM)data;    
    bgmSaveData.stopBGM = stopBGM;
    bgmSaveData.stopMode = stopMode;
    bgmSaveData.skipTransition = skipTransition;
    bgmSaveData.fmodEvent = fmodEvent;
    bgmSaveData.tracks = new List<Track>();
    foreach (Track track in tracks) {
      bgmSaveData.tracks.Add(track.Clone());
    }
  }

  public override void Run(IncidentGraphManager manager, Action<string, bool> cb) {
    if (stopBGM) {
      Game.audioManager.StopBGM(stopMode);
      return;
    } else if (!string.IsNullOrEmpty(fmodEvent) && fmodEvent != Game.audioManager.currentBGM) {
      Game.audioManager.TryChangeBGM(skipTransition, fmodEvent);
    }
    for (int i = 0; i < tracks.Count; i++) {
      Game.audioManager.ModifyTrack($"bgm_{i+1}", tracks[i].strength, tracks[i].timeTo);
    }
    cb.Invoke(defaultNextId, stopGraph);
  }

}
