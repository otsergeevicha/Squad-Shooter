using UnityEditor;
using UnityEngine;
using System;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(EnumArrayAttribute))]
    public class EnumArrayDrawer : UnityEditor.PropertyDrawer
    {
        private bool m_Inited = false;
        private string[] m_EnumValues;

        private void Init()
        {
            EnumArrayAttribute enumAttribute = (EnumArrayAttribute)attribute;

            m_EnumValues = Enum.GetNames(enumAttribute.selectedEnum);

            m_Inited = true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!m_Inited)
                Init();

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginProperty(position, label, property);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = property.depth;

            int propertyIndex = property.GetPropertyArrayIndex();
            if (propertyIndex < m_EnumValues.Length)
            {
                EditorGUI.PropertyField(position, property, new GUIContent(m_EnumValues[property.GetPropertyArrayIndex()]), false);
            }
            else
            {
                EditorGUI.LabelField(position, "ERROR:", "Wrong object reference!");
            }

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}