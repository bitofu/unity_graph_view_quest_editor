using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncidentGraphTriggerOnExit : IncidentGraphManager {

  [SerializeField] GameObject nonPlayerTrigger = default;

  void OnTriggerExit(Collider collider) {
    if (nonPlayerTrigger != null) {
      if (collider.gameObject == nonPlayerTrigger) {
        RunIncidents(collider.GetComponent<Actor>());
      }
    } else if (collider.CompareTag(Constants.playerTag)) {
      RunIncidents(collider.GetComponent<Actor>());
    }
  }
  
}
