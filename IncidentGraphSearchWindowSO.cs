using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

public class IncidentGraphSearchWindowSO : ScriptableObject, ISearchWindowProvider {

  IncidentGraphView graphView;

  public void Init(IncidentGraphView graphView) {
    this.graphView = graphView;
  }

  public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
    List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>() {
      new SearchTreeGroupEntry(new GUIContent("Create Element"), 0),
      new SearchTreeGroupEntry(new GUIContent("Nodes"), 1),
      AddSearchTreeEntry("Speech Node", 2, new IncidentGraphSpeechNode()),
      AddSearchTreeEntry("Speech Choice Node", 2, new IncidentGraphSpeechChoiceNode()),
      AddSearchTreeEntry("Key Advance Node", 2, new IncidentGraphKeyAdvanceNode()),
      AddSearchTreeEntry("Key Check Node", 2, new IncidentGraphKeyCheckNode()),
      AddSearchTreeEntry("Key Branching Node", 2, new IncidentGraphKeyBranchingNode()),
      AddSearchTreeEntry("Camera Node", 2, new IncidentGraphCameraNode()),
      AddSearchTreeEntry("SFX Node", 2, new IncidentGraphSFXNode()),
      AddSearchTreeEntry("BGM Node", 2, new IncidentGraphBGMNode()),
      AddSearchTreeEntry("GameObject Flip State Node", 2, new IncidentGraphGameObjectFlipStateNode()),
      AddSearchTreeEntry("GameObject Toggle Node", 2, new IncidentGraphGameObjectToggleNode()),
      AddSearchTreeEntry("Actor Toggle Node", 2, new IncidentGraphActorToggleNode()),
      AddSearchTreeEntry("Actor Animation Node", 2, new IncidentGraphActorAnimationNode()),
      AddSearchTreeEntry("Give Item Node", 2, new IncidentGraphGiveItemNode()),
      AddSearchTreeEntry("Wait Node", 2, new IncidentGraphWaitNode()),
      AddSearchTreeEntry("Start Node", 2, new IncidentGraphStartNode()),
      AddSearchTreeEntry("Bridge Start Node", 2, new IncidentGraphBridgeStartNode()),
      AddSearchTreeEntry("End Node", 2, new IncidentGraphEndNode()),
      AddSearchTreeEntry("Group", 1, new Group()),
    };
    return searchTreeEntries;
  }

  public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
    Vector2 localMousePosition = graphView.GetLocalMousePosition(context.screenMousePosition, true);
    Type nodeType;
    switch (searchTreeEntry.userData) {
      case Group _:
        IncidentGraphGroup group = graphView.CreateGroup("New Group", localMousePosition, null);
        return true;
      case IncidentGraphSpeechNode model:
        nodeType = model.GetType();
        break;
      case IncidentGraphSpeechChoiceNode model:
        nodeType = model.GetType();
        break;
      case IncidentGraphKeyAdvanceNode model:
        nodeType = model.GetType();
        break;
      case IncidentGraphKeyCheckNode model:
        nodeType = model.GetType();
        break;
      case IncidentGraphKeyBranchingNode model:
        nodeType = model.GetType();
        break;
      case IncidentGraphCameraNode model:
        nodeType = model.GetType();
        break;
      case IncidentGraphSFXNode model:
        nodeType = model.GetType();
        break;
      case IncidentGraphBGMNode model:
        nodeType = model.GetType();
        break;
      case IncidentGraphGameObjectFlipStateNode model:
        nodeType = model.GetType();
        break;
      case IncidentGraphGameObjectToggleNode model:
        nodeType = model.GetType();
        break;
      case IncidentGraphActorToggleNode model:
        nodeType = model.GetType();
        break;
      case IncidentGraphActorAnimationNode model:
        nodeType = model.GetType();
        break;
      case IncidentGraphGiveItemNode model:
        nodeType = model.GetType();
        break;
      case IncidentGraphWaitNode model:
        nodeType = model.GetType();
        break;
      case IncidentGraphStartNode model:
        nodeType = model.GetType();
        break;
      case IncidentGraphBridgeStartNode model:
        nodeType = model.GetType();
        break;
      case IncidentGraphEndNode model:
        nodeType = model.GetType();
        break;
      default: return false;
    }
    graphView.AddElement(graphView.CreateNode(nodeType, localMousePosition, null));
    return true;
  }

  SearchTreeEntry AddSearchTreeEntry(string name, int level, object data) {
    Texture2D indentationIcon = new Texture2D(1, 1);
    indentationIcon.SetPixel(0, 0, Color.clear);
    indentationIcon.Apply();
    return new SearchTreeEntry(new GUIContent(name, indentationIcon)) {
      level = level,
      userData = data
    };
  }

}
