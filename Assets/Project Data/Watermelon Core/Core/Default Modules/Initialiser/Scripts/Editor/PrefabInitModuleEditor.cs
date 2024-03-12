using UnityEngine;
using UnityEditor;

namespace Watermelon
{
    [CustomEditor(typeof(PrefabInitModule))]
    public class PrefabInitModuleEditor : InitModuleEditor
    {
        private readonly string INIT_PREFABS_PROPERTY_NAME = "prefabs";

        private SerializedProperty initPrefabsProperty;

        private MonoScript script;

        private void OnEnable()
        {
            try
            {
                initPrefabsProperty = serializedObject.FindProperty(INIT_PREFABS_PROPERTY_NAME);

                script = MonoScript.FromScriptableObject((ScriptableObject)target);
            }
            catch (System.Exception)
            {
                return;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
            GUI.enabled = true;

            int initObjectsArraySize = initPrefabsProperty.arraySize;
            if (initObjectsArraySize > 0)
            {
                for (int i = 0; i < initObjectsArraySize; i++)
                {
                    SerializedProperty arrayElement = initPrefabsProperty.GetArrayElementAtIndex(i);

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PrefixLabel("Element " + i);
                    EditorGUILayout.PropertyField(arrayElement, GUIContent.none);

                    if (GUILayout.Button("X", EditorStylesExtended.button_04_mini, GUILayout.Height(18), GUILayout.Width(18)))
                    {
                        if (EditorUtility.DisplayDialog("Remove element", "Are you sure you want to remove prefab object?", "Remove", "Cancel"))
                        {
                            initPrefabsProperty.RemoveFromObjectArrayAt(i);

                            return;
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.LabelField("List is empty!");
            }

            serializedObject.ApplyModifiedProperties();
        }

        public override void Buttons()
        {
            if (GUILayout.Button("Add", GUILayout.Width(90)))
            {
                initPrefabsProperty.serializedObject.Update();

                int arraySize = initPrefabsProperty.arraySize;
                initPrefabsProperty.arraySize++;
                initPrefabsProperty.GetArrayElementAtIndex(arraySize).objectReferenceValue = null;

                initPrefabsProperty.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

// -----------------
// Initialiser v 0.4.2
// -----------------