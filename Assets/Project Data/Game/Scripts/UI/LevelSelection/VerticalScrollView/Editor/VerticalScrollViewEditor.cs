using UnityEngine;
using UnityEditor;

namespace Watermelon.SquadShooter
{
    [CustomEditor(typeof(VerticalGridScrollView))]
    public class VerticalScrollViewEditor : WatermelonEditor
    {

        private VerticalGridScrollView scrollView;

        private SerializedProperty childSize;
        private SerializedProperty spacings;

        private SerializedProperty columnsAmount;

        private SerializedProperty gridItem;

        private SerializedProperty initialItemsAmount;

        private SerializedProperty rubberLength;
        private SerializedProperty rubberPower;

        private SerializedProperty inertion;

        protected override void OnEnable()
        {
            base.OnEnable();

            scrollView = (VerticalGridScrollView)serializedObject.targetObject;

            childSize = serializedObject.FindProperty("childSize");
            spacings = serializedObject.FindProperty("spacings");

            columnsAmount = serializedObject.FindProperty("columnsAmount");
            gridItem = serializedObject.FindProperty("gridItem");
            initialItemsAmount = serializedObject.FindProperty("initialItemsAmount");

            rubberLength = serializedObject.FindProperty("rubberLength");
            rubberPower = serializedObject.FindProperty("rubberPower");
            inertion = serializedObject.FindProperty("inertion");
        }

        public override void OnInspectorGUI()
        {
            var textStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight, stretchWidth = false };

            Vector2 childSizeValue = childSize.vector2Value;
            Vector2 spacingsValue = spacings.vector2Value;


            GUILayout.Label("Sizes:", textStyle);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();

            GUILayout.Label("Child Size:", textStyle);
            GUILayout.Label("Spacing:", textStyle);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();

            GUILayout.Label("x:", textStyle);
            GUILayout.Label("horysontal:", textStyle);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();

            childSizeValue.x = EditorGUILayout.FloatField("", childSizeValue.x, GUILayout.MaxWidth(64));
            spacingsValue.x = EditorGUILayout.FloatField("", spacingsValue.x, GUILayout.MaxWidth(64));

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();

            GUILayout.Label("y:", textStyle);
            GUILayout.Label("vertical:", textStyle);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();

            childSizeValue.y = EditorGUILayout.FloatField("", childSizeValue.y, GUILayout.MaxWidth(64));
            spacingsValue.y = EditorGUILayout.FloatField("", spacingsValue.y, GUILayout.MaxWidth(64));

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();

            GUILayout.FlexibleSpace();
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();

            GUILayout.FlexibleSpace();
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();

            GUILayout.FlexibleSpace();
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();

            GUILayout.FlexibleSpace();
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            GUILayout.Label("Item Settings:", textStyle);

            EditorGUILayout.PropertyField(gridItem);
            EditorGUILayout.PropertyField(initialItemsAmount);
            EditorGUILayout.PropertyField(columnsAmount);

            EditorGUILayout.Space();

            GUILayout.Label("Rubber Settings:", textStyle);

            EditorGUILayout.PropertyField(rubberLength);
            EditorGUILayout.PropertyField(rubberPower);
            EditorGUILayout.PropertyField(inertion);

            childSize.vector2Value = childSizeValue;
            spacings.vector2Value = spacingsValue;

            serializedObject.ApplyModifiedProperties();
        }
    }
}