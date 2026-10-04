using Assets.Scripts.Interfaces;
using Assets.Scripts.ScriptableObjects;

using UnityEditor;

using UnityEngine;

namespace Assets.Editor
{
    [CustomPropertyDrawer(typeof(BooleanReference))]
    [CustomPropertyDrawer(typeof(FloatReference))]
    [CustomPropertyDrawer(typeof(IntReference))]
    public class VariableReferencePropertyDraw : PropertyDrawer
    {
        public VarReferenceSelector RefSelector;

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
            var refSelectorRect = new Rect(position.x, position.y, 60, position.height);

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            if (property.FindPropertyRelative("UseConstant") != null)
            { 
                RefSelector = property.FindPropertyRelative("UseConstant").boolValue ? VarReferenceSelector.Constant : VarReferenceSelector.Variable;
                RefSelector = (VarReferenceSelector)EditorGUI.EnumPopup(refSelectorRect, GUIContent.none, RefSelector);
                property.FindPropertyRelative("UseConstant").boolValue = RefSelector == VarReferenceSelector.Constant;
            }

            if (RefSelector == VarReferenceSelector.Constant)
            {
                var constantRect = new Rect(position.x + 70, position.y, position.width - 70, position.height);
                EditorGUI.PropertyField(constantRect, property.FindPropertyRelative("ConstantValue"), GUIContent.none);
            }
            else
            {
                var constantRect = new Rect(position.x + 70, position.y, position.width - 70, position.height);
                EditorGUI.ObjectField(constantRect, property.FindPropertyRelative("Variable"), GUIContent.none);
            }

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}