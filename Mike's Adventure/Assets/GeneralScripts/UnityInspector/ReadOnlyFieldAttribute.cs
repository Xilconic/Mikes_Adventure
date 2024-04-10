using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Marks a field as 'read-only' within the Unity Inspector.
/// </summary>
/// <seealso href="https://dev.to/jayjeckel/unity-tips-properties-and-the-inspector-1goo"/>
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class ReadOnlyFieldAttribute : PropertyAttribute { }

/// <summary>
/// Renders the field as read-only within the Unity Inspector.
/// </summary>
/// <seealso href="https://dev.to/jayjeckel/unity-tips-properties-and-the-inspector-1goo"/>
[UsedImplicitly, CustomPropertyDrawer(typeof(ReadOnlyFieldAttribute))]
public class ReadOnlyFieldAttributeDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 
        EditorGUI.GetPropertyHeight(property, label, true);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
