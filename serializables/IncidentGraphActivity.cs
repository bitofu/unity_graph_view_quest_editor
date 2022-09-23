using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IncidentGraphActivity : ScriptableObject {

  public string defaultNextId => outputs[0].linkedNodeId;
  public string successNextId => defaultNextId;
  public string failNextId => outputs[1].linkedNodeId;
  public Type nodeType {
    get => System.Type.GetType(assemblyQualifiedName);
    set {
      assemblyQualifiedName = value.AssemblyQualifiedName;
    }
  }
  [ReadOnly] public string assemblyQualifiedName;
  [ReadOnly] public string id;
  [ReadOnly] public Vector2 position;
  [ReadOnly] public bool stopGraph;
  [ReadOnly] public bool playerCanAct;
  [NonReorderable] public List<IncidentGraphNodeLink> inputs;
  [NonReorderable] public List<IncidentGraphNodeLink> outputs;

  public virtual void CopyTo(IncidentGraphActivity data) {
    data.assemblyQualifiedName = assemblyQualifiedName;
    data.id = id;
    data.position = position;
    data.stopGraph = stopGraph;
    data.playerCanAct = playerCanAct;
    data.inputs = new List<IncidentGraphNodeLink>();
    data.outputs = new List<IncidentGraphNodeLink>();
    foreach(IncidentGraphNodeLink link in inputs) data.inputs.Add(link.Clone());
    foreach(IncidentGraphNodeLink link in outputs) data.outputs.Add(link.Clone());
  }

  // public abstract IncidentGraphActivity Migrate();

  public abstract void Run(IncidentGraphManager manager = null, Action<string, bool> _ = null);

}
