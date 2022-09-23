using System;

public class IncidentGraphStart : IncidentGraphActivity {

  [ReadOnly] public IncidentGraphEnd.Type onTerminateEndType = IncidentGraphEnd.Type.RemoveCollider;
  [ReadOnly] public string tag;
  [ReadOnly] public bool cannotTerminate;
  [ReadOnly] public bool canInterrupt;
  [ReadOnly] public string bridgeGroup;

  public override void CopyTo(IncidentGraphActivity data) {
    base.CopyTo(data);
    IncidentGraphStart startSaveData = (IncidentGraphStart)data;
    startSaveData.onTerminateEndType = onTerminateEndType;
    startSaveData.tag = tag;
    startSaveData.cannotTerminate = cannotTerminate;
    startSaveData.canInterrupt = canInterrupt;
    startSaveData.bridgeGroup = bridgeGroup;
  }
  
  public override void Run(IncidentGraphManager manager, Action<string, bool> cb) {
    cb.Invoke(defaultNextId, stopGraph);
  }

}
