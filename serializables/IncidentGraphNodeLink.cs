using System;

[Serializable]
public class IncidentGraphNodeLink : ICloneable {
  [ReadOnly] public string text;
  [ReadOnly] public string portId;
  [ReadOnly] public string linkedNodeId;
  [ReadOnly] public string linkedPortId;

  public IncidentGraphNodeLink(string text, string portId) {
    this.text = text;
    this.portId = portId;
  }

  public IncidentGraphNodeLink Clone() {
    return new IncidentGraphNodeLink(text, portId) {
      linkedNodeId = linkedNodeId,
      linkedPortId = linkedPortId
    };
  }
  object ICloneable.Clone() => Clone();
}
