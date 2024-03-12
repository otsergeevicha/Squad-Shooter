using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Watermelon
{
    [CustomEditor(typeof(DefinesSettings))]
    public class DefinesSettingsEditor : WatermelonEditor
    {
        private SerializedProperty customDefinesProperty;

        private GUIContent addDefineButton;
        
        private Define[] projectDefines;

        private DefinesSettings definesData;

        private Define tempDefine = null;

        private bool isDefinesSame;
        private bool isRequireInit;
        
        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            EditorCoroutines.Execute(RecheckDefines());
        }

        private static IEnumerator RecheckDefines()
        {
            while (EditorApplication.isCompiling)
            {
                yield return null;
            }

            if (!EditorApplication.isPlaying)
            {
                DefinesSettings definesData = EditorUtils.GetAsset<DefinesSettings>();
                    
                if (definesData == null)
                {
                    if (EditorUtility.DisplayDialog("Define Manager", "Defines asset can't be found.", "Create", "Ignore"))
                    {
                        EditorUtils.CreateAsset<DefinesSettings>("Assets/Project Data/Content/Settings/Editor/Define Settings", true);
                    }
                    else
                    {
                        Debug.LogWarning("[Define Manager]: Defines Settings asset can't be found.");
                    }
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            definesData = (DefinesSettings)target;

            customDefinesProperty = serializedObject.FindProperty("customDefines");

            tempDefine = null;

            isRequireInit = true;

            CacheVariables();
        }

        protected override void Styles()
        {
            addDefineButton = new GUIContent(EditorStylesExtended.ICON_SPACE + "Add Define", EditorStylesExtended.GetTexture("icon_add", EditorStylesExtended.IconColor));
        }
        
        private string[] GetActiveStaticDefines()
        {
            string definesLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));

            if (!string.IsNullOrEmpty(definesLine))
            {
                List<string> activeDefines = new List<string>();

                string[] defines = definesLine.Split(';');

                for (int i = 0; i < DefinesSettings.STATIC_DEFINES.Length; i++)
                {
                    if (Array.FindIndex(defines, x => x.Equals(DefinesSettings.STATIC_DEFINES[i])) != -1)
                    {
                        activeDefines.Add(DefinesSettings.STATIC_DEFINES[i]);
                    }
                }

                return activeDefines.ToArray();
            }

            return null;
        }

        private void CacheVariables()
        {
            // Get project defines
            List<Define> defines = new List<Define>();

            // Get static defines
            string[] activeStaticDefines = GetActiveStaticDefines();
            if (!activeStaticDefines.IsNullOrEmpty())
            {
                for (int i = 0; i < activeStaticDefines.Length; i++)
                {
                    defines.Add(new Define(activeStaticDefines[i], Define.Type.Static, true));
                }
            }

            //Get assembly
            List<Type> gameTypes = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if(assembly != null)
                {
                    try
                    {
                        Type[] tempTypes = assembly.GetTypes();

                        tempTypes = tempTypes.Where(m => m.IsDefined(typeof(DefineAttribute), true)).ToArray();

                        if (!tempTypes.IsNullOrEmpty())
                            gameTypes.AddRange(tempTypes);
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            foreach (Type type in gameTypes)
            {
                //Get attribute
                DefineAttribute[] defineAttributes = (DefineAttribute[])Attribute.GetCustomAttributes(type, typeof(DefineAttribute));

                for (int i = 0; i < defineAttributes.Length; i++)
                {
                    int methodId = defines.FindIndex(x => x.define == defineAttributes[i].define);
                    if (methodId == -1)
                    {
                        defines.Add(new Define(defineAttributes[i].define, Define.Type.Project));
                    }
                }
            }

            // Get custom defines
            int customDefinesArraySize = customDefinesProperty.arraySize;
            if (customDefinesArraySize > 0)
            {
                for (int i = 0; i < customDefinesArraySize; i++)
                {
                    defines.Add(new Define(customDefinesProperty.GetArrayElementAtIndex(i).stringValue, Define.Type.Custom));
                }
            }

            string defineLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            string[] currentDefinesArray = defineLine.Split(';');
            for(int i = 0; i < currentDefinesArray.Length; i++)
            {
                if(!string.IsNullOrEmpty(currentDefinesArray[i]))
                {
                    if (defines.FindIndex(x => x.define == currentDefinesArray[i]) == -1)
                    {
                        defines.Add(new Define(currentDefinesArray[i], Define.Type.ThirdParty, true));
                    }
                }
            }

            projectDefines = defines.ToArray();

            LoadActiveDefines();
        }

        private void LoadActiveDefines()
        {
            string defineLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));

            string[] currentDefinesArray = defineLine.Split(';');

            if(!currentDefinesArray.IsNullOrEmpty())
            {
                for(int i = 0; i < currentDefinesArray.Length; i++)
                {
                    int defineIndex = Array.FindIndex(projectDefines, x => x.define.Equals(currentDefinesArray[i]));

                    if(defineIndex != -1)
                    {
                        projectDefines[defineIndex].isEnabled = true;
                    }
                }
            }
        }

        private string GetActiveDefinesLine()
        {
            string definesLine = "";

            for(int i = 0; i < projectDefines.Length; i++)
            {
                if(projectDefines[i].isEnabled)
                {
                    definesLine += projectDefines[i].define + ";";
                }
            }

            return definesLine;
        }

        private void SaveDefines(string definesLine)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), definesLine);
        }

        private bool CompareDefines()
        {
            string[] currentDefinesArray = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)).Split(';');

            for (int i = 0; i < projectDefines.Length; i++)
            {
                int findIndex = Array.FindIndex(currentDefinesArray, x => x == projectDefines[i].define);

                if (projectDefines[i].isEnabled)
                {
                    if (findIndex == -1)
                        return false;
                }
                else
                {
                    if (findIndex != -1)
                        return false;
                }
            }

            return true;
        }

        public override void OnInspectorGUI()
        {
            InitStyles();

            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);
            
            if(!projectDefines.IsNullOrEmpty())
            {
                EditorGUI.BeginChangeCheck();

                float defaultLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = Screen.width - 100;

                int customDefineIndex = 0;
                bool guiState = GUI.enabled;

                for (int i = 0; i < projectDefines.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    switch (projectDefines[i].type)
                    {
                        case Define.Type.Static:
                            GUI.enabled = false;

                            EditorGUILayout.Toggle(true, GUILayout.Width(20));
                            EditorGUILayout.PrefixLabel(projectDefines[i].define);

                            GUILayout.Space(22);

                            GUI.enabled = guiState;

                            break;
                        case Define.Type.Project:
                            projectDefines[i].isEnabled = EditorGUILayout.Toggle(projectDefines[i].isEnabled, GUILayout.Width(20));
                            EditorGUILayout.PrefixLabel(projectDefines[i].define);

                            break;
                        case Define.Type.Custom:
                            projectDefines[i].isEnabled = EditorGUILayout.Toggle(projectDefines[i].isEnabled, GUILayout.Width(20));
                            EditorGUILayout.PrefixLabel(projectDefines[i].define);
                            GUILayout.FlexibleSpace();

                            if (GUILayout.Button("X", EditorStylesExtended.button_04_mini, GUILayout.Height(18), GUILayout.Width(18)))
                            {
                                if (EditorUtility.DisplayDialog("Remove define", "Are you sure you want to remove define?", "Remove", "Cancel"))
                                {
                                    bool isRemovedDefineEnabled = projectDefines[i].isEnabled;

                                    customDefinesProperty.RemoveFromVariableArrayAt(customDefineIndex);

                                    CacheVariables();

                                    if(isRemovedDefineEnabled)
                                    {
                                        SaveDefines(GetActiveDefinesLine());
                                    }

                                    return;
                                }
                            }

                            customDefineIndex++;
                            break;
                        case Define.Type.ThirdParty:
                            GUI.enabled = false;
                            EditorGUILayout.Toggle(true, GUILayout.Width(20));
                            GUI.enabled = guiState;

                            EditorGUILayout.PrefixLabel(projectDefines[i].define + " (Thrid Party)");
                            GUILayout.FlexibleSpace();

                            if (GUILayout.Button("X", EditorStylesExtended.button_04_mini, GUILayout.Height(18), GUILayout.Width(18)))
                            {
                                if (EditorUtility.DisplayDialog("Remove define", "Are you sure you want to remove define?", "Remove", "Cancel"))
                                {
                                    string defineLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
                                    string[] currentDefinesArray = defineLine.Split(';');

                                    defineLine = "";
                                    for (int k = 0; k < currentDefinesArray.Length; k++)
                                    {
                                        if (currentDefinesArray[k] != projectDefines[i].define)
                                            defineLine += currentDefinesArray[k] + ";";
                                    }
                                    
                                    SaveDefines(defineLine);

                                    return;
                                }
                            }

                            customDefineIndex++;
                            break;
                    }

                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUIUtility.labelWidth = defaultLabelWidth;

                if(EditorGUI.EndChangeCheck())
                {
                    isRequireInit = true;
                }
            }
            else
            {
                EditorGUILayout.LabelField("There are no defines in project.");
            }
            
            if (tempDefine != null)
            {
                EditorGUILayout.BeginHorizontal();
                tempDefine.define = EditorGUILayout.TextField("DEFINE NAME", tempDefine.define.ToUpper());

                if (GUILayout.Button("✓", EditorStylesExtended.button_03_mini, GUILayout.Height(18), GUILayout.Width(18)))
                {
                    if (Array.FindIndex(projectDefines, x => x.define == tempDefine.define) != -1)
                    {
                        EditorUtility.DisplayDialog("Wrong Define", tempDefine.define + " define already exists!", "Close");

                        return;
                    }

                    if (string.IsNullOrEmpty(tempDefine.define))
                    {
                        EditorUtility.DisplayDialog("Wrong Define", "Define can't be empty!", "Close");

                        return;
                    }

                    serializedObject.Update();

                    int index = customDefinesProperty.arraySize;

                    customDefinesProperty.arraySize++;

                    SerializedProperty newElementProperty = customDefinesProperty.GetArrayElementAtIndex(index);

                    newElementProperty.stringValue = tempDefine.define;

                    tempDefine = null;

                    CacheVariables();

                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(addDefineButton, EditorStylesExtended.button_01, GUILayout.Width(120)))
            {
                if(tempDefine == null)
                {
                    tempDefine = new Define("", Define.Type.Custom);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            Rect compileRect = EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);
            
            if (isRequireInit)
            {
                isDefinesSame = CompareDefines();

                isRequireInit = false;
            }

            EditorGUI.BeginDisabledGroup(isDefinesSame);

            if (GUILayout.Button("Apply Defines", EditorStylesExtended.button_01))
            {
                SaveDefines(GetActiveDefinesLine());
                
                return;
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();

            EditorGUILayoutCustom.DrawCompileWindow(compileRect);
        }

        [System.Serializable]
        private class Define
        {
            public string define;
            public Type type;

            public bool isEnabled;

            public Define(string define, Type type, bool isEnabled = false)
            {
                this.define = define;
                this.type = type;
                this.isEnabled = isEnabled;
            }

            public enum Type
            {
                Static = 0,
                Project = 1,
                Custom = 2,
                ThirdParty = 3
            }
        }
    }
}

// -----------------
// Define Manager v 0.2.1
// -----------------