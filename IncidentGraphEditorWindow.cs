using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Callbacks;
using System.Collections.Generic;
using System;
using System.Linq;

public class IncidentGraphEditorWindow : EditorWindow {

  string graphSOPath;
  string toolbarSearchString;
  IncidentGraphSO graphSO;
  IncidentGraphView graphView;
  ObjectField graphSOField;
  Button saveButton;
  Button miniMapButton;

  [MenuItem("Window/Incident Graph")]
  public static void Open() {
    GetWindow<IncidentGraphEditorWindow>("Incident Graph");
  }

  [OnOpenAsset(1)]
  public static bool ShowWindow(int _instanceID, int line) {
    UnityEngine.Object item = EditorUtility.InstanceIDToObject(_instanceID);
    if (item is IncidentGraphSO container) {
      IncidentGraphEditorWindow window = (IncidentGraphEditorWindow)GetWindow<IncidentGraphEditorWindow>();
      window.titleContent = new GUIContent("Incident Graph");
      window.graphSO = container;
      window.minSize = new Vector2(500, 500);
      window.LoadScriptableObject();
    }
    return false;
  }
  
  public void EnableSave() => saveButton.SetEnabled(true);
  public void DisableSave() => saveButton.SetEnabled(false);

  void OnEnable() {
    EditorApplication.playModeStateChanged += RemoveRuntimeNode;
    AddGraphView();
    AddToolbar();
    rootVisualElement.AddStyleSheets("./styles/IncidentGraphVariables.uss");
    
    graphView.RegisterCallback<KeyDownEvent>((evt) => {
      if (evt.keyCode == KeyCode.M) ToggleMiniMap();
      if (evt.ctrlKey && evt.keyCode == KeyCode.S) Save();
    });
    if (graphSO) LoadScriptableObject();
  }

  void OnDisable() {
    EditorApplication.playModeStateChanged -= RemoveRuntimeNode;
    rootVisualElement.Remove(graphView);
  }

  void OnInspectorUpdate() {
    if (!graphSO) return;
    if (Application.isPlaying) {
      graphView.UpdateRuntimeView(graphSO.runtimeNodeId);
    }
  }

  void RemoveRuntimeNode(PlayModeStateChange stateChange) {
    if (graphSO && stateChange == PlayModeStateChange.EnteredEditMode) {
      graphView.UpdateRuntimeView(string.Empty);
    }
  }

  void AddGraphView() {
    graphView = new IncidentGraphView(this);
    graphView.StretchToParentSize();
    rootVisualElement.Add(graphView);
  }

  void AddToolbar() {
    Toolbar toolbar = new Toolbar();
    toolbar.AddStyleSheets("./styles/IncidentGraphToolbarStyles.uss");

    graphSOField = IncidentGraphUtility.CreateObjectField(
      graphSO,
      null,
      typeof(IncidentGraphSO),
      (context) => {
        IncidentGraphSO newGraphSO = (IncidentGraphSO)context.newValue;
        if (context.newValue == null) {
          graphSOField.SetValueWithoutNotify(graphSO);
        } else if (graphSO != newGraphSO) {
          graphSO = newGraphSO;
          LoadScriptableObject();
        }
      }
    );
    saveButton = IncidentGraphUtility.CreateButton("Save", Save);
    Button resetButton = IncidentGraphUtility.CreateButton("Reset", LoadScriptableObject);
    Button clearButton = IncidentGraphUtility.CreateButton("Clear", () => {
      graphView.ClearGraph();
      graphView.AddDefaultStart();
    });
    miniMapButton = IncidentGraphUtility.CreateButton("MiniMap", ToggleMiniMap);
    Button pingNodeButton = IncidentGraphUtility.CreateButton("Ping Node:", () => {
      if (!graphSO) return;
      IncidentGraphActivity node = graphSO.nodes.Find((n) => n.id == toolbarSearchString);
      if (node) {
        EditorGUIUtility.PingObject(node);
      }
    });
    TextField nodeIdField = IncidentGraphUtility.CreateTextField(toolbarSearchString, string.Empty, (change) => {
      toolbarSearchString = change.newValue;
      graphView.UpdateDebugHighlightView(toolbarSearchString);
    });
    toolbar.Add(graphSOField);
    toolbar.Add(saveButton);
    toolbar.Add(resetButton);
    toolbar.Add(clearButton);
    toolbar.Add(miniMapButton);
    toolbar.Add(pingNodeButton);
    toolbar.Add(nodeIdField);
    rootVisualElement.Add(toolbar);
  }

  // void Load() {
  //   string filePath = EditorUtility.OpenFilePanel("Incident Graphs", "Assets/custom/data", "asset");
  //   if (!string.IsNullOrEmpty(filePath)) {
  //     string relativePath = "Assets" + filePath.Substring(Application.dataPath.Length);
  //     graphSO = AssetDatabase.LoadAssetAtPath<IncidentGraphSO>(relativePath);
  //     LoadScriptableObject();
  //   }
  // }

  void LoadScriptableObject() {
    if (!graphSO) return;

    graphSOPath = AssetDatabase.GetAssetPath(graphSO);
    graphSOField.SetValueWithoutNotify(graphSO);
    graphView.ClearGraph();
    foreach (IncidentGraphActivity nodeData in graphSO.nodes) {
      IncidentGraphNode node = graphView.CreateNode(nodeData.nodeType, nodeData.position, nodeData);
      graphView.AddElement(node);
    }
    foreach (IncidentGraphActivity node in graphSO.nodes) {
      foreach (IncidentGraphNodeLink link in node.outputs) {
        if (string.IsNullOrEmpty(link.linkedNodeId)) continue;
        graphView.AddElement(
          graphView.portMap[link.portId].ConnectTo(graphView.portMap[link.linkedPortId])
        );
        graphView.portMap[link.portId].ToggleInClassList("ig-node__port-error");
      }
    }
    foreach (IncidentGraphNode node in graphView.nodeMap.Values) {
      node.RefreshPorts();
    }
    foreach (IncidentGraphSO.NodeGroup data in graphSO.groups) {
      IncidentGraphGroup group = graphView.CreateGroup(data.title, data.position, data);
      foreach (string nodeId in data.nodes) {
        group.AddElement(graphView.nodeMap[nodeId]);
      }
    }
    if (graphSO.startNodes.Count == 0) {
      graphView.AddDefaultStart();
    }
    graphView.UpdateDebugHighlightView(toolbarSearchString);
  }

  void Save() {
    Type groupType = typeof(IncidentGraphGroup);
    HashSet<string> currentNodes = new HashSet<string>();
    List<IncidentGraphActivity> nodesToSaveToAsset = new List<IncidentGraphActivity>();
    List<IncidentGraphStart> startNodes = new List<IncidentGraphStart>();
    List<IncidentGraphBridgeStart> bridgeNodes = new List<IncidentGraphBridgeStart>();
    List<IncidentGraphSO.NodeGroup> groupsToSave = new List<IncidentGraphSO.NodeGroup>();
    graphView.graphElements.ForEach((element) => {
      if (element is IncidentGraphNode node) {
        IncidentGraphActivity nodeSaveData = graphSO.nodes.Find((n) => n.id == node.id);
        if (nodeSaveData == null) {
          nodeSaveData = node.CreateSave();
          nodesToSaveToAsset.Add(nodeSaveData);
        } else {
          node.CopyTo(nodeSaveData);
        }
        if (node.isStartNode) {
          startNodes.Add((IncidentGraphStart)nodeSaveData);
        } else if (node.isBridgeNode) {
          bridgeNodes.Add((IncidentGraphBridgeStart)nodeSaveData);
        }
        nodeSaveData.position = node.GetPosition().position;
        currentNodes.Add(node.id);
      } else if (element.GetType() == groupType) {
        IncidentGraphGroup group = (IncidentGraphGroup)element;
        groupsToSave.Add(new IncidentGraphSO.NodeGroup() {
          id = group.id,
          title = group.title,
          position = group.GetPosition().position,
          nodes = new List<string>(group.nodes)
        });
      }
    });
    for (int i = graphSO.nodes.Count-1; i >= 0; i--) {
      if (!currentNodes.Contains(graphSO.nodes[i].id)) {
        AssetDatabase.RemoveObjectFromAsset(graphSO.nodes[i]);
        graphSO.nodes.RemoveAt(i);
      }
    }
    foreach (IncidentGraphActivity node in nodesToSaveToAsset) {
      graphSO.nodes.Add(node);
      AssetDatabase.AddObjectToAsset(node, graphSOPath);
    }
    Dictionary<string, IncidentGraphActivity> nodeMap = new Dictionary<string, IncidentGraphActivity>();
    foreach (IncidentGraphActivity node in graphSO.nodes) nodeMap.Add(node.id, node);
    foreach (IncidentGraphEnd endNode in graphView.endNodes) {
      IncidentGraphStart startNode = (IncidentGraphStart)FindFirstNode(endNode, nodeMap);
      if (!startNode) break;
      if (startNode.onTerminateEndType == IncidentGraphEnd.Type.GoBackTwo) break;
      if (endNode.endType != IncidentGraphEnd.Type.RemoveCollider) {
        startNode.onTerminateEndType = endNode.endType;
      }
    }
    startNodes.Sort((a, b) => a.tag.CompareTo(b.tag));
    graphSO.startNodes = startNodes;
    graphSO.bridgeNodes = bridgeNodes;
    graphSO.groups = groupsToSave;
    EditorUtility.SetDirty(graphSO);
    AssetDatabase.SaveAssetIfDirty(graphSO);
  }

  IncidentGraphActivity FindFirstNode(
    IncidentGraphActivity currentNode,
    Dictionary<string, IncidentGraphActivity> nodeMap)
  {
    if (currentNode.inputs.Count == 0) {
      return currentNode;
    } else if (string.IsNullOrEmpty(currentNode.inputs[0].linkedNodeId)) {
      return null;
    } else {
      IncidentGraphActivity previousNode = nodeMap[currentNode.inputs[0].linkedNodeId];
      return FindFirstNode(previousNode, nodeMap);
    }
  }

  void ToggleMiniMap() {
    graphView.ToggleMiniMap();
    miniMapButton.ToggleInClassList("ig-toolbar__button_selected");
  }

}
