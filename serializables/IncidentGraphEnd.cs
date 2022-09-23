using System;
using System.Collections.Generic;
using UnityEngine;

public class IncidentGraphEnd : IncidentGraphActivity {

  public enum Type {
    RemoveCollider,
    GoBackOne,
    GoBackTwo,
    ReturnToStart
  }

  [ReadOnly] public IncidentGraphEnd.Type endType;

  public override void CopyTo(IncidentGraphActivity data) {
    base.CopyTo(data);
    IncidentGraphEnd endSaveData = (IncidentGraphEnd)data;
    endSaveData.endType = endType;
  }

  public override void Run(IncidentGraphManager manager, Action<string, bool> cb) {
    string nextId = string.Empty;
    switch (endType) {
      case Type.RemoveCollider:
        nextId = null;
        break;
      case Type.GoBackOne:
        nextId = manager.previousNodeId;
        break;
      case Type.GoBackTwo:
        nextId = manager.previousPreviousNodeId;
        break;
      case Type.ReturnToStart:
        nextId = manager.firstNodeId;
        break;
    }
    cb.Invoke(nextId, stopGraph);
  }

}
