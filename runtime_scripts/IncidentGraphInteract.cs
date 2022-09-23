using UnityEngine;

public class IncidentGraphInteract : IncidentGraphManager, IInteractable {

  public Vector3 iconPosition = Vector3.up;

  Collider col;

  void Awake() {
    col = GetComponent<Collider>();
    OnRun = OnInteract;
    OnStop = ResetCollider;
    OnComplete = Complete;
  }

  void OnTriggerEnter(Collider collider) {
    if (!collider.CompareTag(Constants.playerTag)) return;
    PlayerController.interactables.Add(this);
    if (PlayerController.interactables.Count == 1) {
      PlayerController.currentInteractable = this;
      ShowIcon();
    }
  }

  void OnTriggerExit(Collider collider) {
    if (!collider.CompareTag(Constants.playerTag)) return;
    RemoveInteractableFromActor();
  }

  public void Interact(Actor actor) => RunIncidents(actor);

  public void ShowIcon() {
    Game.interactIcon.position = iconPosition + transform.position;
    Game.interactIcon.gameObject.SetActive(true);
  }

  void ResetCollider() {
    col.enabled = true;
  }

  void OnInteract() {
    col.enabled = false;
    RemoveInteractableFromActor();
  }

  void Complete() {
    RemoveInteractableFromActor();
  }

  void RemoveInteractableFromActor() {
    if ((IncidentGraphInteract)PlayerController.currentInteractable == this) {
      PlayerController.currentInteractable = null;
    }
    PlayerController.interactables.Remove(this);
    if (PlayerController.interactables.Count > 0) {
      foreach (IInteractable interactable in PlayerController.interactables) {
        PlayerController.currentInteractable = interactable;
        interactable.ShowIcon();
        break;
      }
    } else {
      HideIcon();
    }
  }

  void HideIcon() {
    Game.interactIcon.gameObject.SetActive(false);
  }
  
}
