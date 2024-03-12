using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    [CustomPropertyDrawer(typeof(DropData))]
    public class DropDataPropertyDrawer : UnityEditor.PropertyDrawer
    {
        private const int ColumnCount = 3;
        private const int GapSize = 4;
        private const int GapCount = ColumnCount - 1;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var x = position.x;
            var y = position.y;
            var width = (position.width - GapCount * GapSize) / ColumnCount;
            var height = EditorGUIUtility.singleLineHeight;
            var offset = width + GapSize;

            var dropTypeProperty = property.FindPropertyRelative("dropType");
            DropableItemType dropType = (DropableItemType)dropTypeProperty.intValue;

            EditorGUI.PropertyField(new Rect(x, y, width, height), dropTypeProperty, GUIContent.none);
            EditorGUI.PropertyField(new Rect(x + offset, y, width, height), property.FindPropertyRelative("amount"), GUIContent.none);

            if (dropType == DropableItemType.Currency)
            {
                EditorGUI.PropertyField(new Rect(x + offset + offset, y, width, height), property.FindPropertyRelative("currencyType"), GUIContent.none);
            }
            else if (dropType == DropableItemType.WeaponCard)
            {
                EditorGUI.PropertyField(new Rect(x + offset + offset, y, width, height), property.FindPropertyRelative("cardType"), GUIContent.none);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
}
