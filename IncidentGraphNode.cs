using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

public class IncidentGraphNode : Node {

  public class ErrorData {
    public Color colour { get; set; }
    public ErrorData() {
      colour = new Color32(
        (byte)UnityEngine.Random.Range(65, 256),
        (byte)UnityEngine.Random.Range(50, 176),
        (byte)UnityEngine.Random.Range(50, 176),
        255
      );
    }
  }

  public string id => baseData.id;
  public bool isStartNode { get; protected set; }
  public bool isBridgeNode { get; protected set; }
  public IncidentGraphActivity baseData { get; private set; }

  protected IncidentGraphView graphView;
  protected int errorCount;

  Type _runtimeType;
  Type runtimeType {
    get {
      if (_runtimeType == null) {
        string editorType = this.GetType().AssemblyQualifiedName;
        string expectedType = editorType.Replace("Node", "").Replace("-Editor", "");
        _runtimeType = System.Type.GetType(expectedType);
      }
      return _runtimeType;
    }
  }

  public IncidentGraphActivity CreateSave() {
    IncidentGraphActivity newSaveData = (IncidentGraphActivity)ScriptableObject.CreateInstance(runtimeType);
    baseData.CopyTo(newSaveData);
    newSaveData.name = newSaveData.id;
    return newSaveData;
  }

  public virtual void CopyTo(IncidentGraphActivity target) => baseData.CopyTo(target);
  
  public virtual void Init(IncidentGraphView graphView, Vector2 position, IncidentGraphActivity data) {
    this.graphView = graphView;
    baseData = (IncidentGraphActivity)ScriptableObject.CreateInstance(runtimeType);
    if (data == null) {
      baseData.nodeType = GetType();
      baseData.id = IncidentGraphUtility.CreateGuid();
      baseData.position = position;
      baseData.stopGraph = false;
      baseData.playerCanAct = true;
      baseData.inputs = new List<IncidentGraphNodeLink>();
      baseData.outputs = new List<IncidentGraphNodeLink>();
    } else {
      data.CopyTo(baseData);
    }
    SetPosition(new Rect(position, Vector2.zero));

    mainContainer.AddToClassList("ig-node__main-container");
    topContainer.AddToClassList("ig-node__top-container");
    extensionContainer.AddToClassList("ig-node__extension-container");
  }
 
  public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
    evt.menu.AppendAction("Disconnect Inputs", actionEvent => DisconnectPorts(inputContainer));
    evt.menu.AppendAction("Disconnect Outputs", actionEvent => DisconnectPorts(outputContainer));
    base.BuildContextualMenu(evt);
  }
  
  public void DisconnectAllPorts() {
    DisconnectPorts(inputContainer);
    DisconnectPorts(outputContainer);
  }

  protected bool IsValidTargetId(string id) => !string.IsNullOrEmpty(id);

  protected IncidentGraphNodeLink CreateNewLink(string text) {
    return new IncidentGraphNodeLink(text, IncidentGraphUtility.CreateGuid());
  }

  protected void AddStopGraphToggle(IncidentGraphActivity data) {
    Toggle stopGraphToggle = IncidentGraphUtility.CreateToggle(data.stopGraph, null, "Break", (context) => {
      data.stopGraph = context.newValue;
    });
    stopGraphToggle.AddToClassList("ig-node__title-toggle");
    titleButtonContainer.Insert(0, stopGraphToggle);
  }

  protected void AddPlayerCanActToggle(IncidentGraphActivity data) {
    Toggle playerCanActToggle = IncidentGraphUtility.CreateToggle(data.playerCanAct, null, "Can Act", (change) => {
      data.playerCanAct = change.newValue;
    });
    playerCanActToggle.AddToClassList("ig-node__title-toggle");
    titleButtonContainer.Insert(0, playerCanActToggle);
  }

  protected void InitPorts(
    List<IncidentGraphNodeLink> set,
    Action defaultCb,
    Action<IncidentGraphNodeLink> loadLinkCb
  ) {
    if (set.Count == 0) {
      defaultCb();
    } else {
      foreach (IncidentGraphNodeLink link in set) {
        loadLinkCb(link);
      }
    }
  }

  protected Port AddInputPort(IncidentGraphNodeLink link, Port.Capacity capacity, Type type) {
    Port port = DrawPort(inputContainer, link, Direction.Input, capacity, type);
    graphView.portMap.Add(link.portId, port);
    if (!baseData.inputs.Contains(link)) {
      baseData.inputs.Add(link);
    }
    return port;
  }

  protected Port AddOutputPort(IncidentGraphNodeLink link, Port.Capacity capacity, Type type) {
    Port port = DrawPort(outputContainer, link, Direction.Output, capacity, type);
    port.ToggleInClassList("ig-node__port-error");
    graphView.portMap.Add(link.portId, port);
    if (!baseData.outputs.Contains(link)) {
      baseData.outputs.Add(link);
    }
    return port;
  }

  Port DrawPort(VisualElement container, IncidentGraphNodeLink link, Direction direction, Port.Capacity capacity, Type type) {
    Port port = InstantiatePort(Orientation.Horizontal, direction, capacity, type);
    port.portName = link.text;
    port.userData = link;
    container.Add(port);
    return port;
  }

  void DisconnectPorts(VisualElement container) {
    foreach (VisualElement element in container.Children()) {
      if (element is Port port && port.connected) {
        graphView.DeleteElements(port.connections);
      }
    }
  }

}
