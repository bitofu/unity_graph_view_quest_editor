using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class IncidentGraphSFXNode : IncidentGraphNode {

  int invalidFieldCount {
    get => errorCount;
    set {
      errorCount = value;
      if (errorCount == 1) {
        mainContainer.AddToClassList("ig-node__main-container-error");
      } else if (errorCount == 0) {
        mainContainer.RemoveFromClassList("ig-node__main-container-error");
      }
    }
  }

  IncidentGraphSFX sfxSaveData;

  public override void CopyTo (IncidentGraphActivity target) => sfxSaveData.CopyTo(target);

  public override void Init(IncidentGraphView graphView, Vector2 position, IncidentGraphActivity saveData) {
    base.Init(graphView, position, saveData);
    title = "Play SFX OneShot";
    sfxSaveData = (IncidentGraphSFX)baseData;
    if (saveData == null) {
      sfxSaveData.sfxUnits = new List<IncidentGraphSFX.SFXUnit>();
    } else {
      ((IncidentGraphSFX)saveData).CopyTo(sfxSaveData);
    }
    
    AddStopGraphToggle(sfxSaveData);
    InitPorts(
      sfxSaveData.inputs,
      () => AddInputPort(CreateNewLink("Previous"), Port.Capacity.Multi, typeof(bool)),
      (link) => AddInputPort(link, Port.Capacity.Multi, typeof(bool))
    );
    InitPorts(
      sfxSaveData.outputs,
      () => AddOutputPort(CreateNewLink("Next"), Port.Capacity.Single, typeof(bool)),
      (link) => AddOutputPort(link, Port.Capacity.Single, typeof(bool))
    );

    VisualElement customDataContainer = new VisualElement();
    customDataContainer.AddToClassList("ig-node__custom-data-container");

    VisualElement addSFXContainer = new VisualElement();
    addSFXContainer.AddToClassList("ig-node__button-flex-container");
    Button addSFXButton = IncidentGraphUtility.CreateButton("Add SFX", () => {
      invalidFieldCount++;
      IncidentGraphSFX.SFXUnit unit = new IncidentGraphSFX.SFXUnit();
      sfxSaveData.sfxUnits.Add(unit);
      AddSFXUnit(sfxSaveData.sfxUnits.Count-1, unit, customDataContainer);
    });
    addSFXButton.AddToClassList("ig-node__button-add");
    addSFXContainer.Add(addSFXButton);

    if (sfxSaveData.sfxUnits.Count > 0) {
      for (int i = 0; i < sfxSaveData.sfxUnits.Count; i++) {
        AddSFXUnit(i, sfxSaveData.sfxUnits[i], customDataContainer);
      }
    } else {
      invalidFieldCount++;
      IncidentGraphSFX.SFXUnit unit = new IncidentGraphSFX.SFXUnit();
      sfxSaveData.sfxUnits.Add(unit);
      AddSFXUnit(0, unit, customDataContainer);
    }
    
    customDataContainer.Insert(0, addSFXContainer);
    extensionContainer.Add(customDataContainer);
    RefreshExpandedState();
    RefreshPorts();
  }

  void AddSFXUnit(int index, IncidentGraphSFX.SFXUnit unit, VisualElement container) { 
    VisualElement unitContainer = new VisualElement();
    unitContainer.AddToClassList("ig-node__container-with-border");

    VisualElement buttonContainer = new VisualElement();
    buttonContainer.AddToClassList("ig-node__button-flex-container");
    Button deleteButton = IncidentGraphUtility.CreateButton("x", () => {
      if (sfxSaveData.sfxUnits.Count <= 1) return;
      if (string.IsNullOrEmpty(unit.fmodEvent)) {
        invalidFieldCount--;
      }
      sfxSaveData.sfxUnits.Remove(unit);
      container.Remove(unitContainer);
    });
    buttonContainer.Add(deleteButton);

    VisualElement sfxContainer = new VisualElement();
    sfxContainer.AddToClassList("ig-node__container-with-bar");

    if (string.IsNullOrEmpty(unit.fmodEvent)) {
      invalidFieldCount++;
    }
    TextField fmodEventField = IncidentGraphUtility.CreateTextField(
      unit.fmodEvent,
      "FMOD Event:",
      (change) => {
        string newValue = change.newValue.Trim();
        if (unit.fmodEvent == newValue) return;
        bool wasValid = IsValidTargetId(unit.fmodEvent);
        bool newValid = IsValidTargetId(newValue);
        if (wasValid && !newValid) {
          invalidFieldCount++;
        } else if (!wasValid && newValid) {
          invalidFieldCount--;
        }
        unit.fmodEvent = newValue;
      }
    );
    fmodEventField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );

    TextField targetIdField = IncidentGraphUtility.CreateTextField(
      unit.targetId,
      "Target ID:",
      (change) => unit.targetId = change.newValue.Trim()
    );
    targetIdField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );

    ObjectField actorTypeField = IncidentGraphUtility.CreateObjectField(
      unit.targetActor,
      "Target Actor:",
      typeof(ActorType),
      (context) => unit.targetActor = (ActorType)context.newValue
    );
    actorTypeField.AddToClassList("ig-node__label");

    sfxContainer.Add(fmodEventField);
    sfxContainer.Add(targetIdField);
    sfxContainer.Add(actorTypeField);

    unitContainer.Add(buttonContainer);
    unitContainer.Add(sfxContainer);
    container.Add(unitContainer);
  }

}
