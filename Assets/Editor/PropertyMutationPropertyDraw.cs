using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public class PropertyMutationPropertyDraw : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            /*// Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 2;

            // Calculate rects
            var rect = new Rect(position.x, position.y, position.width, position.height);
            EditorGUI.Popup(rect, 0, new string[] { "Test", "This" });

            rect = new Rect(position.x, position.y + 18, position.width, position.height);
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("PropertyValue"), new GUIContent("Value"));

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();*/
        }
    }
}