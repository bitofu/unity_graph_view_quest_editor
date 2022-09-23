using System;

public class IncidentGraphGiveItem : IncidentGraphActivity {

  [ReadOnly] public PickupAbstract.Type pickupType;
  [ReadOnly] public PickupAbstract specificPickup;
  [ReadOnly] public int requiredCount;

  Action<string, bool> cb;
  int currentRequired = -1;

  void OnEnable() {
    currentRequired = requiredCount;
  }

  public override void CopyTo(IncidentGraphActivity data) {
    base.CopyTo(data);
    IncidentGraphGiveItem giveItemSaveData = (IncidentGraphGiveItem)data;
    giveItemSaveData.pickupType = pickupType;
    giveItemSaveData.specificPickup = specificPickup;
    giveItemSaveData.requiredCount = requiredCount;
  }

  public override void Run(IncidentGraphManager manager, Action<string, bool> cb) {
    this.cb = cb;
    InventoryMenu.OpenGiveItem(OnGiveItem, OnCloseInventory);
  }

  void OnGiveItem(PickupAbstract givenPickup) {
    if (specificPickup != null && givenPickup == specificPickup) {
      OnCorrectItem();
    } else if (specificPickup == null && givenPickup.type == pickupType) {
      OnCorrectItem();
    } else {
      cb.Invoke(outputs[1].linkedNodeId, stopGraph);
      InventoryMenu.Close();
    }
  }

  void OnCorrectItem() {
    currentRequired--;
    if (currentRequired == 0) {
      cb.Invoke(outputs[0].linkedNodeId, stopGraph);
      InventoryMenu.Close();
    }
  }

  void OnCloseInventory() {
    cb.Invoke(outputs[2].linkedNodeId, stopGraph);
  }

}
