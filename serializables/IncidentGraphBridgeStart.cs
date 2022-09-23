using System;

public class IncidentGraphBridgeStart : IncidentGraphActivity {

  [ReadOnly] public string tag;

  public override void CopyTo(IncidentGraphActivity data) {
    base.CopyTo(data);
    IncidentGraphBridgeStart startSaveData = (IncidentGraphBridgeStart)data;
    startSaveData.tag = tag;
  }

  public override void Run(IncidentGraphManager manager, Action<string, bool> cb) {
    cb.Invoke(defaultNextId, stopGraph);
  }

}
