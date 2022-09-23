using System;
using UnityEngine;

[Serializable]
public class IncidentGraphCameraManipulation : ICloneable {

  public enum Type {
    Release,
    Track,
    Anchor,
  }

  [ReadOnly] public Type type;
  [ReadOnly] public string targetId;
  [ReadOnly] public float startDelay;
  [ReadOnly] public float moveToSpeed;
  [ReadOnly] public bool waitForCamera;

  public CameraManipulation BuildManipulation(IncidentGraphManager owner, Transform target) {
    if (type == Type.Release) return null;
    return new CameraManipulation() {
      targetId = targetId,
      startDelay = startDelay,
      movetoSpeed = moveToSpeed,
      trackingTarget = type == Type.Track ? target : null,
      anchorTarget = type == Type.Anchor ? target : null,
      owner = owner,
    };
  }

  public IncidentGraphCameraManipulation Clone() {
    return new IncidentGraphCameraManipulation() {
      type = type,
      targetId = targetId,
      startDelay = startDelay,
      moveToSpeed = moveToSpeed,
      waitForCamera = waitForCamera,
    };
  }
  object ICloneable.Clone() => Clone();
}

public class CameraManipulation {
  public string targetId;
  public float startDelay;
  public float movetoSpeed;
  public Transform trackingTarget;
  public Transform anchorTarget;
  public IncidentGraphManager owner;
}
