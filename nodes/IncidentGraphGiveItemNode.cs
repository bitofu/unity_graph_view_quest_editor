using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class IncidentGraphGiveItemNode : IncidentGraphNode {

  IncidentGraphGiveItem giveItemSaveData;

  public override void CopyTo(IncidentGraphActivity target) => giveItemSaveData.CopyTo(target);

  public override void Init(IncidentGraphView graphView, Vector2 position, IncidentGraphActivity saveData) {
    base.Init(graphView, position, saveData);
    title = "Give Item";
    giveItemSaveData = (IncidentGraphGiveItem)baseData;
    if (saveData == null) {
      giveItemSaveData.pickupType = PickupAbstract.Type.Null;
      giveItemSaveData.requiredCount = 1;
    } else {
      ((IncidentGraphGiveItem)saveData).CopyTo(giveItemSaveData);
    }
    
    AddStopGraphToggle(giveItemSaveData);
    InitPorts(
      giveItemSaveData.inputs,
      () => AddInputPort(CreateNewLink("Previous"), Port.Capacity.Multi, typeof(bool)),
      (link) => AddInputPort(link, Port.Capacity.Multi, typeof(bool))
    );
    InitPorts(
      giveItemSaveData.outputs,
      () => {
        AddOutputPort(CreateNewLink("On Give Correct"), Port.Capacity.Single, typeof(bool));
        AddOutputPort(CreateNewLink("On Give Wrong"), Port.Capacity.Single, typeof(bool));
        AddOutputPort(CreateNewLink("On Inventory Close"), Port.Capacity.Single, typeof(bool));
      },
      (link) => AddOutputPort(link, Port.Capacity.Single, typeof(bool))
    );

    VisualElement customDataContainer = new VisualElement();
    customDataContainer.AddToClassList("ig-node__custom-data-container");

    EnumField enumField = IncidentGraphUtility.CreateEnumField(
      giveItemSaveData.pickupType,
      null,
      (context) => giveItemSaveData.pickupType = (PickupAbstract.Type)context.newValue
    );
    enumField.SetValueWithoutNotify(giveItemSaveData.pickupType);

    ObjectField specificPickupField = IncidentGraphUtility.CreateObjectField(
      giveItemSaveData.specificPickup,
      "Specific Pickup:",
      typeof(PickupAbstract),
      (context) => giveItemSaveData.specificPickup = (PickupAbstract)context.newValue
    );
    specificPickupField.AddToClassList("ig-node__label");

    IntegerField intField = null;
    intField = IncidentGraphUtility.CreateIntegerField(
      "Required Items:",
      giveItemSaveData.requiredCount,
      (change) => {
        giveItemSaveData.requiredCount = change.newValue;
        if (giveItemSaveData.requiredCount < 1) {
          giveItemSaveData.requiredCount = 1;
          intField.SetValueWithoutNotify(giveItemSaveData.requiredCount);
        }
      }
    );
    intField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );

    customDataContainer.Add(enumField);
    customDataContainer.Add(specificPickupField);
    customDataContainer.Add(intField);
    extensionContainer.Add(customDataContainer);
    RefreshExpandedState();
    RefreshPorts();
  }

}
