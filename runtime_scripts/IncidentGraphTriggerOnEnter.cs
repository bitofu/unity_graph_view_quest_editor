using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncidentGraphTriggerOnEnter : IncidentGraphManager {

  [SerializeField] GameObject nonPlayerTrigger = default;

  void OnTriggerEnter(Collider collider) {
    if (nonPlayerTrigger != null) {
      if (collider.gameObject == nonPlayerTrigger && collider.TryGetComponent<Actor>(out Actor actor)) {
        RunIncidents(actor);
      }
    } else if (collider.CompareTag(Constants.playerTag)) {
      RunIncidents(collider.GetComponent<Actor>());
    }
  }
  
}
