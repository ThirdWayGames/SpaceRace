using Assets.Scripts.Interfaces;
using Assets.Scripts.ScriptableObjects;

using UnityEditor;

using UnityEngine;

namespace Assets.Editor
{
    [CustomPropertyDrawer(typeof(VariableReseterReference))]
    public class BaseReferencePropertyDraw : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var constantRect = new Rect(position.x, position.y, position.width, position.height);
            var relativeProp = property.FindPropertyRelative("VariableSetter");
            if (relativeProp != null)
            {
                EditorGUI.ObjectField(constantRect, relativeProp, GUIContent.none);
            }

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}