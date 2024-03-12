using UnityEditor;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    [CustomPropertyDrawer(typeof(LevelObstacle))]
    public class LevelObstaclePropertyDrawer : UnityEditor.PropertyDrawer
    {
        private const int ColumnCount = 2;
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

            string[] enumNames = System.Enum.GetNames(typeof(LevelObstaclesType));

            int propertyIndex = property.GetPropertyArrayIndex();
            string name = enumNames.IsInRange(propertyIndex) ? enumNames[propertyIndex] : "Unknown";

            EditorGUI.LabelField(new Rect(x, y, width, height), name);
            EditorGUI.PropertyField(new Rect(x + offset, y, width, height), property.FindPropertyRelative("prefab"), GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}