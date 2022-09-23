using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class IncidentGraphGroup : Group {

  public string id { get; set; }
  public List<string> nodes { get; set; }

  IncidentGraphView graphView;

  public IncidentGraphGroup(
    IncidentGraphView graphView,
    string groupTitle,
    Vector2 position,
    IncidentGraphSO.NodeGroup saveData
  ) {
    this.graphView = graphView;
    id = saveData?.id ?? IncidentGraphUtility.CreateGuid();
    title = groupTitle;
    nodes = new List<string>();
    SetPosition(new Rect(position, Vector2.zero));
  }

}
