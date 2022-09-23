using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Incident Graph")]
public class IncidentGraphSO : ScriptableObject {

  [Serializable]
  public class NodeGroup {
    [ReadOnly] public string id;
    [ReadOnly] public string title;
    [ReadOnly] public Vector2 position;
    [ReadOnly][NonReorderable] public List<string> nodes;
  }

  [ReadOnly] public string runtimeNodeId;
  [ReadOnly] public IncidentGraphEnd.Type onTerminateEndType;
  [ReadOnly] public List<IncidentGraphStart> startNodes = new List<IncidentGraphStart>();
  [ReadOnly] public List<IncidentGraphBridgeStart> bridgeNodes = new List<IncidentGraphBridgeStart>();
  [ReadOnly] public List<IncidentGraphActivity> nodes = new List<IncidentGraphActivity>();
  public List<NodeGroup> groups = new List<NodeGroup>();

  Dictionary<string, IncidentGraphActivity> _nodeMap;
  Dictionary<string, IncidentGraphActivity> nodeMap {
    get {
      if (_nodeMap == null) {
        _nodeMap = new Dictionary<string, IncidentGraphActivity>();
        foreach (IncidentGraphActivity node in nodes) {
          _nodeMap.Add(node.id, node);
        }
      }
      return _nodeMap;
    }
  }

  public IncidentGraphActivity GetNode(string id) => nodeMap[id];

}
