using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public class IncidentGraphView : GraphView {

  public Dictionary<string, IncidentGraphNode> nodeMap { get; private set; }
  public Dictionary<string, Port> portMap { get; private set; }
  public List<IncidentGraphEnd> endNodes { get; private set; }

  IncidentGraphEditorWindow editorWindow;
  IncidentGraphSearchWindowSO searchWindow;
  MiniMap miniMap;

  public IncidentGraphView(IncidentGraphEditorWindow editorWindow) {
    this.editorWindow = editorWindow;
    this.AddStyleSheets(
      "./styles/IncidentGraphViewStyles.uss",
      "./styles/IncidentGraphNodeStyles.uss"
    );

    SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
    this.AddManipulator(new ContentDragger());
    this.AddManipulator(new SelectionDragger());
    this.AddManipulator(new RectangleSelector());
    this.AddManipulator(new FreehandSelector());
    this.AddManipulator(new ContextualMenuManipulator(
      (menuEvent) => menuEvent.menu.AppendAction(
        "Add Group",
        (actionEvent) => CreateGroup(
          "New Group", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition), null
        )
      )
    ));
    this.AddManipulator(CreateNodeContextualMenu("Add Speech Node", typeof(IncidentGraphSpeechNode)));
    this.AddManipulator(CreateNodeContextualMenu("Add Speech Choice Node", typeof(IncidentGraphSpeechChoiceNode)));

    GridBackground gridBackground = new GridBackground();
    gridBackground.StretchToParentSize();
    Insert(0, gridBackground);

    if (searchWindow == null) {
      searchWindow = ScriptableObject.CreateInstance<IncidentGraphSearchWindowSO>();
      searchWindow.Init(this);
    }
    nodeCreationRequest = (context) => SearchWindow.Open(
      new SearchWindowContext(context.screenMousePosition),
      searchWindow
    );

    miniMap = new MiniMap() {
      anchored = true
    };
    miniMap.SetPosition(new Rect(15, 50, 200, 200));
    miniMap.style.backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
    miniMap.style.borderTopColor = 
    miniMap.style.borderRightColor =
    miniMap.style.borderBottomColor =
    miniMap.style.borderLeftColor = new StyleColor(new Color32(51, 51, 51, 255));
    miniMap.visible = false;
    Add(miniMap);

    OnGraphViewChanged();
    OnGroupElementsAdded();
    OnGroupElementsRemoved();
    OnElementsDeleted();
    // serializeGraphElements = (elements) => {
    //   foreach (GraphElement element in elements) {
    //     if (element is IncidentGraphNode node) {
    //       Debug.Log(node.id);
    //     }
    //   }
    //   return "string";
    // };
    // unserializeAndPaste = (opName, data) => {
    //   Debug.Log(data);
    // };

    nodeMap = new Dictionary<string, IncidentGraphNode>();
    portMap = new Dictionary<string, Port>();
    endNodes = new List<IncidentGraphEnd>();
  }

  public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
    List<Port> compatiblePorts = new List<Port>();
    ports.ForEach((port) => {
      if (startPort == port) return;
      if (startPort.direction == port.direction) return;
      compatiblePorts.Add(port);
    });
    return compatiblePorts;
  }

  public void UpdateRuntimeView(string currentNodeId) {
    foreach (IncidentGraphNode node in nodeMap.Values) {
      if (currentNodeId == node.id) {
        node.mainContainer.AddToClassList("ig-node__main-container-running");
      } else {
        node.mainContainer.RemoveFromClassList("ig-node__main-container-running");
      }
    }
  }

  public void UpdateDebugHighlightView(string searchString) {
    foreach (IncidentGraphNode node in nodeMap.Values) {
      if (searchString == node.id || searchString == node.title) {
        node.mainContainer.AddToClassList("ig-node__main-container-debug");
      } else {
        node.mainContainer.RemoveFromClassList("ig-node__main-container-debug");
      }
    }
  }

  public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false) {
    Vector2 worldMousePosition = mousePosition;
    if (isSearchWindow) {
      worldMousePosition -= editorWindow.position.position;
    }
    return contentViewContainer.WorldToLocal(worldMousePosition);
  }

  public IncidentGraphNode CreateNode(Type type, Vector2 position, IncidentGraphActivity saveData) {
    IncidentGraphNode node = (IncidentGraphNode)Activator.CreateInstance(type);
    node.Init(this, position, saveData);
    nodeMap.Add(node.id, node);
    return node;
  }

  public IncidentGraphGroup CreateGroup(string title, Vector2 position, IncidentGraphSO.NodeGroup saveData) {
    IncidentGraphGroup group = new IncidentGraphGroup(this, title, position, saveData);
    group.AddToClassList("ig-group");
    AddElement(group);
    foreach (GraphElement element in selection) {
      if (element is IncidentGraphNode node) {
        group.AddElement(node);
      }
    }
    return group;
  }

  public void ClearGraph() {
    nodeMap.Clear();
    portMap.Clear();
    endNodes.Clear();
    graphElements.ForEach(RemoveElement);
  }

  public void ToggleMiniMap() {
    miniMap.visible = !miniMap.visible;
  }

  public void AddDefaultStart() {
    AddElement(CreateNode(typeof(IncidentGraphStartNode), new Vector2(100, 350), null));
  }

  IManipulator CreateNodeContextualMenu(string actionTitle, Type type) {
    ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
      (menuEvent) => menuEvent.menu.AppendAction(
        actionTitle,
        (actionEvent) => AddElement(
          CreateNode(type, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition), null)
        )
      )
    );
    return contextualMenuManipulator;
  }

  void OnGraphViewChanged() {
    graphViewChanged = (changes) => {
      if (changes.edgesToCreate != null) {
        foreach (Edge edge in changes.edgesToCreate) {
          IncidentGraphNode previousNode = (IncidentGraphNode)edge.output.node;
          IncidentGraphNode nextNode = (IncidentGraphNode)edge.input.node;
          IncidentGraphNodeLink previousLink = (IncidentGraphNodeLink)edge.output.userData;
          IncidentGraphNodeLink nextLink = (IncidentGraphNodeLink)edge.input.userData;
          previousLink.linkedNodeId = nextNode.id;
          previousLink.linkedPortId = nextLink.portId;
          nextLink.linkedNodeId = previousNode.id;
          nextLink.linkedPortId = previousLink.portId;
          if (!edge.output.connected) {
            edge.output.ToggleInClassList("ig-node__port-error");
          }
          if (edge.output.node == edge.input.node) {
            edge.output.node.mainContainer.ToggleInClassList("ig-node__main-container-warning");
          }
        }
      }
      if (changes.elementsToRemove != null) {
        Type edgeType = typeof(Edge);
        foreach (GraphElement element in changes.elementsToRemove) {
          if (element.GetType() != edgeType) continue;
          Edge edge = (Edge)element;
          IncidentGraphNode previousNode = (IncidentGraphNode)edge.output.node;
          IncidentGraphNode nextNode = (IncidentGraphNode)edge.input.node;
          IncidentGraphNodeLink previousLink = (IncidentGraphNodeLink)edge.output.userData;
          IncidentGraphNodeLink nextLink = (IncidentGraphNodeLink)edge.input.userData;
          previousLink.linkedNodeId = string.Empty;
          previousLink.linkedPortId = string.Empty;
          nextLink.linkedNodeId = string.Empty;
          nextLink.linkedPortId = string.Empty;
          if (edge.output.connected) {
            edge.output.ToggleInClassList("ig-node__port-error");
          }
          if (edge.output.node == edge.input.node) {
            edge.output.node.mainContainer.ToggleInClassList("ig-node__main-container-warning");
          }
        }
      }
      return changes;
    };
  }

  void OnGroupElementsAdded() {
    elementsAddedToGroup = (group, elements) => {
      foreach (GraphElement element in elements) {
        if (element is IncidentGraphNode node) {
          IncidentGraphGroup incidentGraphGroup = (IncidentGraphGroup)group;
          incidentGraphGroup.nodes.Add(node.id);
          if (node.isBridgeNode) {
            ((IncidentGraphBridgeStartNode)node).SetTag(group.title);
          }
        }
      }
    };
  }

  void OnGroupElementsRemoved() {
    elementsRemovedFromGroup = (group, elements) => {
      foreach (GraphElement element in elements) {
        if (element is IncidentGraphNode node) {
          ((IncidentGraphGroup)group).nodes.Remove(node.id);
        }
      }
    };
  }

  void OnElementsDeleted() {
    deleteSelection = (operationName, askUser) => {
      Type groupType = typeof(IncidentGraphGroup);
      Type edgeType = typeof(Edge);
      List<IncidentGraphGroup> groupsToDelete = new List<IncidentGraphGroup>();
      List<Edge> edgesToDelete = new List<Edge>();
      List<IncidentGraphNode> nodesToDelete = new List<IncidentGraphNode>();
      foreach (GraphElement element in selection) {
        // automatically check inherited
        if (element is IncidentGraphNode node) {
          nodesToDelete.Add(node);
        } else if (element.GetType() == edgeType) {
          edgesToDelete.Add((Edge)element);
        } else if (element.GetType() == groupType) {
          groupsToDelete.Add((IncidentGraphGroup)element);
        }
      }
      foreach (IncidentGraphGroup group in groupsToDelete) {
        List<IncidentGraphNode> groupNodes = new List<IncidentGraphNode>();
        foreach (GraphElement element in group.containedElements) {
          if (element is IncidentGraphNode node) {
            groupNodes.Add(node);
          }
        }
        group.RemoveElements(groupNodes);
        RemoveElement(group);
      }
      DeleteElements(edgesToDelete);
      foreach (IncidentGraphNode node in nodesToDelete) {
        node.DisconnectAllPorts();
        nodeMap.Remove(node.id);
        RemoveElement(node);
      }
    };
  }

}
