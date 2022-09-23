using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

public static class IncidentGraphUtility {

  public static VisualElement AddStyleSheets(this VisualElement element, params string[] styleSheetNames) {
    foreach (string styleSheetName in styleSheetNames) {
      StyleSheet styleSheet = (StyleSheet)EditorGUIUtility.Load(styleSheetName);
      element.styleSheets.Add(styleSheet);
    }
    return element;
  }

  public static VisualElement AddClasses(this VisualElement element, params string[] classNames) {
    foreach (string className in classNames) {
      element.AddToClassList(className);
    }
    return element;
  }

  public static List<T> DeepClone<T>(IEnumerable<T> listToClone) {
    List<T> copy = new List<T>();
    foreach (ICloneable item in listToClone) copy.Add((T)item.Clone());
    return copy;
  }

  public static string CreateGuid() => GUID.Generate().ToString();

  public static string GetAssetGUID(UnityEngine.Object obj) {
    return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
  }

  public static UnityEngine.Object GetAsset<T>(string guid) {
    return AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(T));
  }

  public static Foldout CreateFoldout(string title, bool collapsed = false) {
    Foldout foldout = new Foldout() {
      text = title,
      value = !collapsed
    };
    return foldout;
  }

  public static Button CreateButton(string text, Action onClick = null) {
    Button button = new Button(onClick) {
      text = text
    };
    return button;
  }

  public static VisualElement CreateTargetField(IncidentGraphTargetReference targetRef) {
    VisualElement container = new VisualElement();
    TextField targetIdField = CreateTextField(
      targetRef.objectId,
      "Target ID:",
      (change) => targetRef.objectId = change.newValue.Trim()
    );
    targetIdField.AddClasses(
      "ig-node__label",
      "ig-node__field__input",
      "ig-node__textfield"
    );
    ObjectField targetActor = CreateObjectField(
      targetRef.actorType,
      "Target Actor:",
      typeof(ActorType),
      (context) => targetRef.actorType = (ActorType)context.newValue
    );
    targetActor.AddToClassList("ig-node__label");

    IncidentGraphTargetReference.Type currentType = (IncidentGraphTargetReference.Type)targetRef.useActorType;
    EnumField targetType = CreateEnumField(
      currentType,
      "Target Type",
      (change) => {
        targetRef.useActorType = (int)(IncidentGraphTargetReference.Type)change.newValue;
        container.Add(targetRef.useActorType == 0 ? targetIdField : targetActor);
        container.Remove(targetRef.useActorType == 0 ? targetActor : targetIdField);
      }
    );
    targetType.SetValueWithoutNotify(currentType);
    targetType.AddToClassList("ig-node__label");
    
    container.Add(targetType);
    container.Add(targetRef.useActorType == 0 ? targetIdField : targetActor);
    return container;
  }

  public static FloatField CreateFloatField(
    string label,
    float value,
    EventCallback<ChangeEvent<float>> onValueChanged = null
  ) {
    FloatField floatField = new FloatField() {
      label = label,
      value = value
    };
    if (onValueChanged != null) {
      floatField.RegisterValueChangedCallback(onValueChanged);
    }
    return floatField;
  }

  public static IntegerField CreateIntegerField(
    string label,
    int value,
    EventCallback<ChangeEvent<int>> onValueChanged = null
  ) {
    IntegerField intField = new IntegerField() {
      label = label,
      value = value
    };
    if (onValueChanged != null) {
      intField.RegisterValueChangedCallback(onValueChanged);
    }
    return intField;
  }

  public static Toggle CreateToggle(
    bool value,
    string label,
    string text,
    EventCallback<ChangeEvent<bool>> onValueChanged = null) {
    Toggle toggle = new Toggle() {
      label = label,
      text = text,
      value = value
    };
    if (onValueChanged != null) {
      toggle.RegisterValueChangedCallback(onValueChanged);
    }
    return toggle;
  }

  public static TextField CreateTextField(
    string value = null,
    string label = null,
    EventCallback<ChangeEvent<string>> onValueChanged = null
  ) {
    TextField textField = new TextField() {
      value = value,
      label = label
    };
    if (onValueChanged != null) {
      textField.RegisterValueChangedCallback(onValueChanged);
    }
    return textField;
  }

  public static TextField CreateTextArea(
    string value = null,
    string label = null,
    EventCallback<ChangeEvent<string>> onValueChanged = null
  ) {
    TextField textArea = CreateTextField(value, label, onValueChanged);
    textArea.multiline = true;
    return textArea;
  }

  public static ObjectField CreateObjectField(
    UnityEngine.Object obj,
    string label,
    Type type,
    EventCallback<ChangeEvent<UnityEngine.Object>> onValueChanged = null
  ) {
    ObjectField objectField = new ObjectField() {
      objectType = type,
      allowSceneObjects = false,
      value = obj,
      label = label
    };
    if (onValueChanged != null) {
      objectField.RegisterValueChangedCallback(onValueChanged);
    }
    return objectField;
  }

  public static EnumField CreateEnumField(
    Enum enumType,
    string label,
    EventCallback<ChangeEvent<Enum>> onValueChanged = null
  ) {
    EnumField enumField = new EnumField() {
      value = enumType,
      label = label
    };
    enumField.Init(enumType);
    if (onValueChanged != null) {
      enumField.RegisterValueChangedCallback(onValueChanged);
    }
    return enumField;
  }

}
