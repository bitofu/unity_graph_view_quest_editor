using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IncidentGraphReference {

  public IncidentGraphEnd.Type predictedEndType => startNode.onTerminateEndType;
  public string runtimeNodeId {
    get => graph.runtimeNodeId;
    set {
      graph.runtimeNodeId = value;
    }
  }
  public bool cannotTerminate => startNode.cannotTerminate;
  public bool canInterrupt => startNode.canInterrupt;
  
  #pragma warning disable 0414
  [SerializeField] IncidentGraphSO graph = default;
  [SerializeField] string tag = default;
  [SerializeField] string startNodeId = default;

  IncidentGraphStart _startNode;
  IncidentGraphStart startNode {
    get {
      if (_startNode == null) {
        _startNode = (IncidentGraphStart)GetNode(startNodeId);
      }
      return _startNode;
    }
  }

  public static implicit operator bool(IncidentGraphReference obj) {
    return obj != null && obj.graph != null;
  }

  public IncidentGraphActivity GetNode(string id) => graph.GetNode(id);

  public IncidentGraphActivity GetBridgeIn() {
    List<IncidentGraphBridgeStart> bridges = new List<IncidentGraphBridgeStart>();
    foreach (IncidentGraphBridgeStart bridge in graph.bridgeNodes) {
      if (startNode.bridgeGroup == bridge.tag) {
        bridges.Add(bridge);
      }
    }
    int roll = UnityEngine.Random.Range(0, bridges.Count);
    return GetNode(bridges[roll].defaultNextId);
  }

  public string GetFirstNodeId() {
    return startNode.defaultNextId;
  }

  
}
