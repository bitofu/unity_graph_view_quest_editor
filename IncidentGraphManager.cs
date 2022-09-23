using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IncidentGraphManager : MonoBehaviour {

  public static IncidentGraphManager currentGraphManager;

  public MapManager mapManager => _mapManager;
  public CameraManipulation previousCameraManipulation { get; set; }
  public string firstNodeId => graph.GetFirstNodeId();
  public string previousNodeId { get; private set; }
  public string previousPreviousNodeId { get; private set; }
  public bool cannotTerminate => graph.cannotTerminate;

  [SerializeField][ReadOnly] MapManager _mapManager = default;
  [SerializeField] IncidentGraphReference graph = default;

  protected Action OnRun;
  protected Action OnStop;
  protected Action OnComplete;

  IncidentGraphActivity bridgeIn;
  string _nextNodeId;
  string nextNodeId {
    get => _nextNodeId;
    set {
      previousPreviousNodeId = previousNodeId;
      previousNodeId = _nextNodeId;
      _nextNodeId = value;
    }
  }
  bool isActive;
  bool shouldTerminate;

  #if UNITY_EDITOR
  void OnValidate() {
    if (!_mapManager) {
      MapManager[] maps = FindObjectsOfType<MapManager>();
      foreach (MapManager map in maps) {
        if (map.gameObject.scene == gameObject.scene) {
          _mapManager = map;
          UnityEditor.EditorUtility.SetDirty(this);
          break;
        }
      }
    }
  }
  #endif

  void Start() {
    nextNodeId = firstNodeId;
  }

  void OnDisable() {
    graph.runtimeNodeId = string.Empty;
    StopAllCoroutines();
  }

  public Transform GetTargetFromId(string id) {
    if (mapManager.ExistsObject(id)) {
      return mapManager.GetObjects(id)[0].transform;
    }

    return id switch {
      "$player" => Game.player.transform,
      // "$arys" => Game.actors.findwhere
      "$default" => transform,
      _ => transform,
    };
  }

  public void RunIncidents(Actor actor) {
    if (isActive) return;

    isActive = true;
    OnRun?.Invoke();
    RunNextNode(nextNodeId);
  }

  public void Terminate() {
    shouldTerminate = true;
    Game.speechManager.TerminateAndClear();
  }

  void RunNextNode(string id) {
    if (currentGraphManager != null && currentGraphManager != this) {
      if (graph.canInterrupt) {
        Debug.Log("set bridge in");
        Game.speechManager.PrepareInterruption();
        bridgeIn = graph.GetBridgeIn();
        bridgeIn.Run(this, null);
      } else if (currentGraphManager.cannotTerminate) {
        isActive = false;
        OnStop?.Invoke();
        return;
      } else {
        currentGraphManager?.Terminate();
      }
    }
    currentGraphManager = this;
    graph.runtimeNodeId = id;
    graph.GetNode(id).Run(this, OnNodeEnd);
  }

  void OnNodeEnd(string nextNodeId, bool shouldStop) {
    if (!this) return;
    if (shouldTerminate) {
      shouldTerminate = false;
      switch (graph.predictedEndType) {
        case IncidentGraphEnd.Type.GoBackTwo:
          nextNodeId = previousPreviousNodeId;
          break;
        case IncidentGraphEnd.Type.ReturnToStart:
          nextNodeId = firstNodeId;
          break;
        default:
          nextNodeId = string.Empty;
          break;
      }
    }
    if (string.IsNullOrEmpty(nextNodeId)) {
      OnGraphStop();
      OnComplete?.Invoke();
      Destroy(gameObject);
      return;
    }
    this.nextNodeId = nextNodeId;
    if (shouldStop) {
      OnGraphStop();
      isActive = false;
      OnStop?.Invoke();
    } else {
      RunNextNode(nextNodeId);
    }
  }

  void OnGraphStop() {
    Game.camera.ReleaseManipulation(this, 0);
    previousCameraManipulation = null;
    if (graph.canInterrupt && bridgeIn) {
      graph.GetNode(bridgeIn.defaultNextId).Run(this, (nextId, stopGraph) => {
        Game.speechManager.EndInterruption();
      });
    }
    if (currentGraphManager == this) {
      currentGraphManager = null;
    }
  }

}
