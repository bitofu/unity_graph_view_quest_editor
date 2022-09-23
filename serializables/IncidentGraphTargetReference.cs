using UnityEngine;

[System.Serializable]
public class IncidentGraphTargetReference {
  public enum Type {
    TargetID,
    ActorType
  }
  public int useActorType;
  public string objectId;
  public ActorType actorType;

  public Transform GetTransform(IncidentGraphManager manager) {
    return useActorType == 0
      ? manager.GetTargetFromId(objectId)
      : Game.actors[actorType].transform;
  }
}
