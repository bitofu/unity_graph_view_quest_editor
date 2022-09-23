using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Incident Key")]
public class IncidentKeyAbstract : ScriptableObject {

  public int stepsToComplete => _stepsToComplete;
  public bool isValid => Game.incidentKeyProgress.ContainsKey(this.name)
    && Game.incidentKeyProgress[this.name] == 0;

  [Range(1, 5)]
  [SerializeField] int _stepsToComplete = 1;
  [SerializeField][ReadOnly] int currentState = -1;

  public void Advance() {
    if (!Game.incidentKeyProgress.ContainsKey(this.name)) {
      Game.incidentKeyProgress[this.name] = _stepsToComplete;
    }
    Game.incidentKeyProgress[this.name]--;
    Game.incidentKeyProgress[this.name] = Mathf.Max(Game.incidentKeyProgress[this.name], 0);
    currentState = Game.incidentKeyProgress[this.name];
  }

  public void Retreat() {
    if (!Game.incidentKeyProgress.ContainsKey(this.name)) {
      Game.incidentKeyProgress[this.name] = _stepsToComplete;
    }
    Game.incidentKeyProgress[this.name]++;
    currentState = Game.incidentKeyProgress[this.name];
  }
  
  public void ResetSteps() {
    Game.incidentKeyProgress[this.name] = stepsToComplete;
    currentState = Game.incidentKeyProgress[this.name];
  }

  public void LoadState(int loadState) {
    Game.incidentKeyProgress[this.name] = loadState;
    currentState = Game.incidentKeyProgress[this.name];
  }

}
