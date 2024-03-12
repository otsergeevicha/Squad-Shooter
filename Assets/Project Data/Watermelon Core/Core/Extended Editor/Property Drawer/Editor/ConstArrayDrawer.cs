using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(ConstArrayAttribute))]
    public class ConstArrayDrawer : UnityEditor.PropertyDrawer
    {
        private bool m_Inited = false;
        private string[] m_EnumValues;

        private void Init()
        {
            ConstArrayAttribute enumAttribute = (ConstArrayAttribute)attribute;

            m_EnumValues = enumAttribute.labelValues;

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
