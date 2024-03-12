using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;

namespace Watermelon
{
    [CustomEditor(typeof(ProjectInitSettings))]
    public class ProjectInitSettingsEditor : WatermelonEditor
    {
        private readonly string INIT_MODULES_PROPERTY_NAME = "initModules";
        private readonly string MODULE_NAME_PROPERTY_NAME = "moduleName";
        private static string DEFAULT_PROJECT_INIT_SETTINGS_PATH = "Assets/Project Data/Content/Settings/Project Init Settings.asset";

        private SerializedProperty initModulesProperty;
        private InitModuleContainer[] initModulesEditors;

        private GUIContent addButton;
        private GUIContent arrowDownContent;
        private GUIContent arrowUpContent;

        private GUIStyle arrowButtonStyle;

        private Type[] allowedTypes;

        private GenericMenu modulesGenericMenu;

        [MenuItem("Tools/Editor/Project Init Settings")]
        public static void SelectProjectInitSettings()
        {
            UnityEngine.Object selectedObject = AssetDatabase.LoadAssetAtPath(DEFAULT_PROJECT_INIT_SETTINGS_PATH, typeof(ProjectInitSettings));

            if(selectedObject == null)
            {
                selectedObject = EditorUtils.GetAsset<ProjectInitSettings>();

                if(selectedObject == null)
                {
                    Debug.LogError("Asset with type \"ProjectInitSettings\" don`t exist.");
                }
                else
                {
                    Selection.activeObject = selectedObject;
                    Debug.LogWarning($"Asset with type \"ProjectInitSettings\" is misplaced. Expected path: {DEFAULT_PROJECT_INIT_SETTINGS_PATH} .Actual path: {AssetDatabase.GetAssetPath(selectedObject)}");

                }
            }
            else
            {
                Selection.activeObject = selectedObject;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            initModulesProperty = serializedObject.FindProperty(INIT_MODULES_PROPERTY_NAME);

            InitGenericMenu();
            LoadEditorsList();

            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private void InitGenericMenu()
        {
            ProjectInitSettings projectInitSettings = (ProjectInitSettings)target;

            modulesGenericMenu = new GenericMenu();

            //Load all modules
            Type[] allowedTypes = Assembly.GetAssembly(typeof(InitModule)).GetTypes().Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(InitModule))).ToArray();
            InitModule[] initModules = projectInitSettings.InitModules;

            for (int i = 0; i < allowedTypes.Length; i++)
            {
                Type tempType = allowedTypes[i];

                RegisterModuleAttribute[] defineAttributes = (RegisterModuleAttribute[])Attribute.GetCustomAttributes(tempType, typeof(RegisterModuleAttribute));
                for (int m = 0; m < defineAttributes.Length; m++)
                {
                    int index = i;

                    bool isAlreadyActive = initModules != null && projectInitSettings.InitModules.Any(x => x.GetType() == tempType);
                    if (isAlreadyActive)
                    {
                        modulesGenericMenu.AddDisabledItem(new GUIContent(defineAttributes[m].path), false);
                    }
                    else
                    {
                        modulesGenericMenu.AddItem(new GUIContent(defineAttributes[m].path), false, delegate
                        {
                            AddModule(tempType);

                            InitGenericMenu();
                        });
                    }
                }
            }
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= LogPlayModeState;
        }

        private void LogPlayModeState(PlayModeStateChange obj)
        {
            if (Selection.activeObject == target)
                Selection.activeObject = null;
        }

        protected override void Styles()
        {
            addButton = new GUIContent("", EditorStylesExtended.GetTexture("icon_add", EditorStylesExtended.IconColor));

            Color arrowColor = EditorGUIUtility.isProSkin ? new Color(0.9f, 0.9f, 0.9f) : new Color(0.2f, 0.2f, 0.2f);

            arrowDownContent = new GUIContent(EditorStylesExtended.GetTexture("icon_arrow_down", arrowColor));
            arrowUpContent = new GUIContent(EditorStylesExtended.GetTexture("icon_arrow_up", arrowColor));

            arrowButtonStyle = new GUIStyle(EditorStylesExtended.padding00);
        }

        private void LoadEditorsList()
        {
            if (initModulesEditors != null)
            {
                // Destroy old editors
                for (int i = 0; i < initModulesEditors.Length; i++)
                {
                    if (initModulesEditors[i] != null && initModulesEditors[i].editor != null)
                    {
                        DestroyImmediate(initModulesEditors[i].editor);
                    }
                }
            }

            int initModulesArraySize = initModulesProperty.arraySize;
            initModulesEditors = new InitModuleContainer[initModulesArraySize];
            for(int i = 0; i < initModulesArraySize; i++)
            {
                SerializedProperty initModule = initModulesProperty.GetArrayElementAtIndex(i);

                if(initModule.objectReferenceValue != null)
                {
                    SerializedObject initModuleSerializedObject = new SerializedObject(initModule.objectReferenceValue);

                    initModulesEditors[i] = new InitModuleContainer(initModuleSerializedObject, Editor.CreateEditor(initModuleSerializedObject.targetObject));
                }
            }
        }

        private void OnDestroy()
        {
            if (initModulesEditors != null)
            {
                // Destroy old editors
                for (int i = 0; i < initModulesEditors.Length; i++)
                {
                    if (initModulesEditors[i] != null && initModulesEditors[i].editor != null)
                    {
                        DestroyImmediate(initModulesEditors[i].editor);
                    }
                }

                initModulesEditors = null;
            }
        }

        public override void OnInspectorGUI()
        {
            InitStyles();

            Rect projectModulesRect = EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            EditorGUILayoutCustom.Header("PROJECT MODULES");

            int initModulesArraySize = initModulesProperty.arraySize;
            if(initModulesArraySize > 0)
            {
                for (int i = 0; i < initModulesArraySize; i++)
                {
                    int index = i;
                    SerializedProperty initModule = initModulesProperty.GetArrayElementAtIndex(i);

                    if (initModule.objectReferenceValue != null)
                    {
                        SerializedObject moduleSeializedObject = new SerializedObject(initModule.objectReferenceValue);

                        moduleSeializedObject.Update();

                        SerializedProperty moduleNameProperty = moduleSeializedObject.FindProperty(MODULE_NAME_PROPERTY_NAME);

                        EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

                        Rect moduleRect = EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(EditorStylesExtended.ICON_SPACE + moduleNameProperty.stringValue, EditorStylesExtended.label_medium);
                        EditorGUILayout.EndHorizontal();

                        if (initModule.isExpanded)
                        {
                            if (initModulesEditors[index] != null && initModulesEditors[index].editor != null)
                            {
                                initModulesEditors[index].editor.OnInspectorGUI();
                            }

                            GUILayout.Space(10);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();

                            if (initModulesEditors[index].isModuleInitEditor)
                            {
                                initModulesEditors[index].initModuleEditor.Buttons();
                            }

                            if (GUILayout.Button("Remove", GUILayout.Width(90)))
                            {
                                if (EditorUtility.DisplayDialog("This object will be removed!", "Are you sure?", "Remove", "Cancel"))
                                {
                                    UnityEngine.Object removedObject = initModule.objectReferenceValue;

                                    initModulesProperty.RemoveFromVariableArrayAt(index);

                                    AssetDatabase.RemoveObjectFromAsset(removedObject);

                                    DestroyImmediate(removedObject, true);

                                    EditorUtility.SetDirty(target);

                                    AssetDatabase.SaveAssets();

                                    return;
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.EndVertical();

                        if (GUI.Button(new Rect(moduleRect.x + moduleRect.width - 15, moduleRect.y, 12, 12), arrowUpContent, arrowButtonStyle))
                        {
                            if (i > 0)
                            {
                                bool expandState = initModulesProperty.GetArrayElementAtIndex(index - 1).isExpanded;

                                InitModuleContainer tempInitModuleContainer = initModulesEditors[index];
                                initModulesEditors[index] = initModulesEditors[index - 1];
                                initModulesEditors[index - 1] = tempInitModuleContainer;

                                initModulesProperty.MoveArrayElement(index, index - 1);
                                serializedObject.ApplyModifiedProperties();

                                initModulesProperty.GetArrayElementAtIndex(index - 1).isExpanded = initModule.isExpanded;
                                initModulesProperty.GetArrayElementAtIndex(index).isExpanded = expandState;
                            }
                        }
                        if (GUI.Button(new Rect(moduleRect.x + moduleRect.width - 15, moduleRect.y + 12, 12, 12), arrowDownContent, arrowButtonStyle))
                        {
                            if (i + 1 < initModulesArraySize)
                            {
                                bool expandState = initModulesProperty.GetArrayElementAtIndex(index + 1).isExpanded;

                                InitModuleContainer tempInitModuleContainer = initModulesEditors[index];
                                initModulesEditors[index] = initModulesEditors[index + 1];
                                initModulesEditors[index + 1] = tempInitModuleContainer;

                                initModulesProperty.MoveArrayElement(index, index + 1);
                                serializedObject.ApplyModifiedProperties();

                                initModulesProperty.GetArrayElementAtIndex(index + 1).isExpanded = initModule.isExpanded;
                                initModulesProperty.GetArrayElementAtIndex(index).isExpanded = expandState;
                            }
                        }

                        if (GUI.Button(moduleRect, GUIContent.none, GUIStyle.none))
                        {
                            initModule.isExpanded = !initModule.isExpanded;
                        }

                        moduleSeializedObject.ApplyModifiedProperties();
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal(EditorStylesExtended.editorSkin.box);
                        EditorGUILayout.BeginHorizontal(EditorStylesExtended.padding00);
                        EditorGUILayout.LabelField(EditorGUIUtility.IconContent("console.warnicon"), EditorStylesExtended.padding00, GUILayout.Width(16), GUILayout.Height(16));
                        EditorGUILayout.LabelField("Object referenct is null");
                        if (GUILayout.Button("Remove", EditorStyles.miniButton))
                        {
                            initModulesProperty.RemoveFromVariableArrayAt(index);

                            InitGenericMenu();

                            GUIUtility.ExitGUI();
                            Event.current.Use();

                            return;
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Modules list is empty!", MessageType.Info);
            }
            
            // Buttons panel
            Rect buttonsPanelRect = new Rect(projectModulesRect.x + projectModulesRect.width - 40, projectModulesRect.y + projectModulesRect.height, 30, 20);
            Rect addButtonRect = new Rect(buttonsPanelRect.x + 5, buttonsPanelRect.y, 20, 20);

            GUI.Box(buttonsPanelRect, "", EditorStylesExtended.panelBottom);
            GUI.Label(addButtonRect, addButton, EditorStylesExtended.labelCentered);

            if (GUI.Button(buttonsPanelRect, GUIContent.none, GUIStyle.none))
            {
                modulesGenericMenu.ShowAsContext();
            }

            EditorGUILayout.EndVertical();

            GUILayout.Space(90);
        }

        public void AddModule(Type moduleType)
        {
            if(!moduleType.IsSubclassOf(typeof(InitModule)))
            {
                Debug.LogError("[Initialiser]: Module type should be subclass of InitModule class!");

                return;
            }

            if (initModulesEditors != null)
            {
                // Destroy old editors
                for (int i = 0; i < initModulesEditors.Length; i++)
                {
                    if (initModulesEditors[i] != null && initModulesEditors[i].editor != null)
                    {
                        DestroyImmediate(initModulesEditors[i].editor);
                    }
                }
            }

            initModulesProperty = serializedObject.FindProperty(INIT_MODULES_PROPERTY_NAME);

            serializedObject.Update();

            initModulesProperty.arraySize++;

            InitModule testInitModule = (InitModule)ScriptableObject.CreateInstance(moduleType);
            testInitModule.name = moduleType.ToString();

            AssetDatabase.AddObjectToAsset(testInitModule, target);

            initModulesProperty.GetArrayElementAtIndex(initModulesProperty.arraySize - 1).objectReferenceValue = testInitModule;

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(target);

            AssetDatabase.SaveAssets();
        }

        private class InitModuleContainer
        {
            public SerializedObject serializedObject;
            public Editor editor;

            public bool isModuleInitEditor;
            public InitModuleEditor initModuleEditor;

            public InitModuleContainer(SerializedObject serializedObject, Editor editor)
            {
                this.serializedObject = serializedObject;
                this.editor = editor;

                initModuleEditor = editor as InitModuleEditor;
                isModuleInitEditor = initModuleEditor != null;
            }
        }
    }
}

// -----------------
// Initialiser v 0.4.2
// -----------------