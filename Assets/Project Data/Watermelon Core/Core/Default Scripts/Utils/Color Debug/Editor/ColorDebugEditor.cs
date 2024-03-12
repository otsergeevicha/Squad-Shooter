using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static Watermelon.ColorDebugSettings;

namespace Watermelon
{
    [CustomEditor(typeof(ColorDebugSettings))]
    public class ColorDebugEditor : WatermelonEditor
    {
        private const string COLORS_LABEL = "COLORS";
        private const string DELETE_LABEL = "X";
        private const string UPDATING_LABEL = "Updating...";
        private const string NAME_LABEL = "name";
        private const string CURRENT_VALUE_LABEL = "value";
        private const string ADD_BUTTON_ICON_NAME = "icon_preferences";

        // Properties
        private readonly string COLORS_INFOS_PROPERTY_NAME = "colorInfos";
        private readonly string COLOR_PROPERTY_NAME = "colorEnum";
        private readonly string VALUE_PROPERTY_NAME = "colorValue";

        private const string COLOR_DEBUG_SCRIPT_NAME = "ColorDebug";
        private readonly string PATH_SUFFICS = "/ColorGenerated.cs";
        private string[] requiredSettingsNames = { };

        private SerializedProperty colorsProperty;
        private GUIContent addButton;
        private GUIStyle errorLabel;
        private GUIStyle columnLabelsStyle;

        private string fullEnumFilePath;
        private string enumAssetPath;
        private bool updating;

        private SerializedProperty keyProperty;
        private Rect fieldRect;

        private string fieldName;
        private Color[] tempValues;

        protected override void OnEnable()
        {
            base.OnEnable();

            //read property            
            colorsProperty = serializedObject.FindProperty(COLORS_INFOS_PROPERTY_NAME);

            // File path
            MonoScript colorDebugScript = EditorUtils.GetAssetByName<MonoScript>(COLOR_DEBUG_SCRIPT_NAME);
            enumAssetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(colorDebugScript)) + PATH_SUFFICS;
            fullEnumFilePath = EditorUtils.projectFolderPath + enumAssetPath;

            //handle enum update
            for (int i = 0; i < colorsProperty.arraySize; i++)
            {
                colorsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(COLOR_PROPERTY_NAME).enumValueIndex = i;
            }

            updating = false;

            PrepareForValidation();
            CollectRequiredAttributes();

            EditorStylesExtended.InitializeStyles();
        }

        private void CollectRequiredAttributes()
        {
            //collect types with attributes
            List<Type> types = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type[] tempTypes;

            foreach (Assembly assembly in assemblies)
            {
                if (assembly != null)
                {
                    try
                    {
                        tempTypes = assembly.GetTypes().Where(el => el.IsDefined(typeof(RequireColorAttribute), true)).ToArray();

                        if (!tempTypes.IsNullOrEmpty())
                        {
                            types.AddRange(tempTypes);
                        }

                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        Debug.Log("[Color Debug] " + e.Message);
                    }
                }
            }

            //return when there no defined required settings
            if (types.Count == 0)
            {
                return;
            }

            // collect attributes from types
            List<RequireColorAttribute> settingAttributes = new List<RequireColorAttribute>();

            foreach (Type tempType in types)
            {
                settingAttributes.AddRange((RequireColorAttribute[])Attribute.GetCustomAttributes(tempType, typeof(RequireColorAttribute)));
            }

            //remove mutiple entries of same attribute
            for (int i = 0; i < settingAttributes.Count - 1; i++)
            {
                for (int j = settingAttributes.Count - 1; j >= i + 1; j--)
                {
                    if (settingAttributes[i].name.Equals(settingAttributes[j].name))
                    {
                        settingAttributes.RemoveAt(j);
                    }
                }
            }

            //cache names of required setting to stop user from deleting them
            requiredSettingsNames = new string[settingAttributes.Count];

            for (int i = 0; i < settingAttributes.Count; i++)
            {
                requiredSettingsNames[i] = settingAttributes[i].name;
            }

            //remove settings that already present
            string[] existingSettingNames = new string[colorsProperty.arraySize];

            for (int i = 0; i < colorsProperty.arraySize; i++)
            {
                keyProperty = colorsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(COLOR_PROPERTY_NAME);
                existingSettingNames[i] = keyProperty.enumNames[keyProperty.enumValueIndex];
            }

            for (int i = settingAttributes.Count - 1; i >= 0; i--)
            {
                if (existingSettingNames.Contains(settingAttributes[i].name))
                {
                    settingAttributes.RemoveAt(i);
                }
            }

            //return when all required settings present
            if (settingAttributes.Count == 0)
            {
                return;
            }

            //adding required settings
            ColorData[] newElements = new ColorData[settingAttributes.Count];

            for (int i = 0; i < settingAttributes.Count; i++)
            {
                newElements[i] = new ColorData(settingAttributes[i].name, Color.black);
            }
            AddSetting(newElements);
        }

        protected override void Styles()
        {
            addButton = new GUIContent(string.Empty, EditorStylesExtended.GetTexture(ADD_BUTTON_ICON_NAME, EditorStylesExtended.IconColor));

            errorLabel = new GUIStyle(EditorStylesExtended.label_small);
            errorLabel.normal.textColor = EditorColor.red01;

            columnLabelsStyle = new GUIStyle(EditorStylesExtended.label_small);
            columnLabelsStyle.fontSize = 11;
            columnLabelsStyle.alignment = TextAnchor.MiddleLeft;
            columnLabelsStyle.stretchWidth = false;
            columnLabelsStyle.wordWrap = false;
        }

        public override void OnInspectorGUI()
        {
            InitStyles();

            if (updating)
            {
                EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

                EditorGUILayout.BeginHorizontal(EditorStylesExtended.padding05, GUILayout.Height(21), GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField(COLORS_LABEL, EditorStylesExtended.boxHeader, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField(UPDATING_LABEL, EditorStylesExtended.label_small, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                EditorGUILayout.EndVertical();

                return;
            }
            //

            //

            //Settings label and add settings button
            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            Rect rect = EditorGUILayout.BeginHorizontal(EditorStylesExtended.padding05, GUILayout.Height(21), GUILayout.ExpandWidth(true));

            EditorGUILayout.LabelField(COLORS_LABEL, EditorStylesExtended.boxHeader, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            if (GUILayout.Button(addButton, EditorStylesExtended.button_01, GUILayout.Height(24), GUILayout.Width(24)))
            {
                PopupWindow.Show(new Rect(rect.width, rect.y, 0, rect.height), new ColorWindow(this, errorLabel));
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            //name label and default/current label
            EditorGUILayout.BeginHorizontal(GUILayout.Height(14));
            EditorGUILayout.LabelField(NAME_LABEL, columnLabelsStyle, GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUILayout.LabelField(CURRENT_VALUE_LABEL, columnLabelsStyle, GUILayout.Width(EditorGUIUtility.labelWidth));

            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < colorsProperty.arraySize; i++)
            {
                fieldRect = EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                DrawPropertyField(colorsProperty.GetArrayElementAtIndex(i), i);
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(requiredSettingsNames.Contains(fieldName) || Application.isPlaying);

                if (GUILayout.Button(DELETE_LABEL, EditorStylesExtended.button_04_mini, GUILayout.Width(16), GUILayout.Height(16)))
                {
                    if (EditorUtility.DisplayDialog("Delete dialog", "Are you sure you want to delete \"" + fieldName + "\" setting?", "Ok", "Cancel"))
                    {
                        DeleteSetting(i);
                        return;
                    }
                }

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPropertyField(SerializedProperty settingInfo, int settingInfoIndex)
        {
            keyProperty = settingInfo.FindPropertyRelative(COLOR_PROPERTY_NAME);
            fieldName = keyProperty.enumDisplayNames[keyProperty.enumValueIndex];

            tempValues[settingInfoIndex] = EditorGUILayout.ColorField(fieldName, tempValues[settingInfoIndex]);
        }

        private void PrepareForValidation()
        {
            tempValues = new Color[colorsProperty.arraySize];

            for (int i = 0; i < colorsProperty.arraySize; i++)
            {
                tempValues[i] = colorsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(VALUE_PROPERTY_NAME).colorValue;
            }
        }

        private void DeleteSetting(int index)
        {
            updating = true;
            List<ColorData> settingDatas = CollectColorsData();
            settingDatas.RemoveAt(index);
            UpdateSettings(settingDatas);
        }

        private void AddSetting(ColorData newElement)
        {
            updating = true;
            List<ColorData> settingDatas = CollectColorsData();
            settingDatas.Add(newElement);
            UpdateSettings(settingDatas);
        }

        private void AddSetting(ColorData[] newElements)
        {
            updating = true;
            List<ColorData> settingDatas = CollectColorsData();
            settingDatas.AddRange(newElements);
            UpdateSettings(settingDatas);
        }

        private List<ColorData> CollectColorsData()
        {
            List<ColorData> resultList = new List<ColorData>();
            SerializedProperty colorInfo;
            Color value = new Color();

            for (int i = 0; i < colorsProperty.arraySize; i++)
            {
                colorInfo = colorsProperty.GetArrayElementAtIndex(i);

                keyProperty = colorInfo.FindPropertyRelative(COLOR_PROPERTY_NAME);
                fieldName = keyProperty.enumNames[keyProperty.enumValueIndex];

                value = colorsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(VALUE_PROPERTY_NAME).colorValue;

                resultList.Add(new ColorData(fieldName, value));
            }

            return resultList;
        }

        private void UpdateSettings(List<ColorData> settingDatas)
        {
            ClearDatabase();
            UpdateEnum(settingDatas);
            AssetDatabase.ImportAsset(enumAssetPath, ImportAssetOptions.ForceUpdate);
            FillDatabase(settingDatas);

            serializedObject.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void ClearDatabase()
        {
            colorsProperty.arraySize = 0;
        }

        private void UpdateEnum(List<ColorData> colorDatas)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("// GENERATED BY CODE! DO NOT MODIFY!");
            stringBuilder.AppendLine();
            stringBuilder.Append("namespace Watermelon");
            stringBuilder.AppendLine();
            stringBuilder.Append("{");
            stringBuilder.AppendLine();
            stringBuilder.Append("\tpublic enum CustomColor");
            stringBuilder.AppendLine();
            stringBuilder.Append("\t{");
            stringBuilder.AppendLine();

            for (int i = 0; i < colorDatas.Count; i++)
            {
                stringBuilder.Append("\t\t");
                stringBuilder.Append(colorDatas[i].name);
                stringBuilder.Append(" = ");
                stringBuilder.Append(i);
                stringBuilder.Append(",");
                stringBuilder.AppendLine();
            }

            stringBuilder.Append("\t}");
            stringBuilder.AppendLine();
            stringBuilder.Append("}");

            File.WriteAllText(fullEnumFilePath, stringBuilder.ToString(), Encoding.UTF8);
        }

        private void FillDatabase(List<ColorData> settingDatas)
        {
            SerializedProperty colorInfo;

            for (int i = 0; i < settingDatas.Count; i++)
            {
                colorsProperty.arraySize++;
                colorInfo = colorsProperty.GetArrayElementAtIndex(i);

                colorInfo.FindPropertyRelative(VALUE_PROPERTY_NAME).colorValue = (Color)settingDatas[i].value;
            }
        }

        private struct ColorData
        {
            public string name;
            public object value;

            public ColorData(string name, object value)
            {
                this.name = name;
                this.value = value;
            }
        }

        private class ColorWindow : PopupWindowContent
        {
            private const string DEFAULT_VALUE_LABEL = "Color";
            private static readonly Vector2 WINDOW_SIZE = new Vector2(344, 90);

            private string fieldName = "";

            private GUIStyle errorLabel;
            private string errorMessage;

            private Color defaultColorValue;
            private ColorDebugEditor colorDebugEditor;

            public ColorWindow(ColorDebugEditor colorDebugEditor, GUIStyle errorLabel)
            {
                this.colorDebugEditor = colorDebugEditor;
                this.errorLabel = errorLabel;
            }

            public override void OnOpen()
            {
                ResetDefaultValues();
            }

            public override void OnClose()
            {

            }

            private void ResetDefaultValues()
            {
                defaultColorValue = Color.black;
            }

            public override void OnGUI(Rect rect)
            {
                EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

                EditorGUI.BeginChangeCheck();

                fieldName = EditorGUILayout.TextField("Name", fieldName);
                if (EditorGUI.EndChangeCheck())
                {
                    ValidateName(fieldName);
                }

                defaultColorValue = EditorGUILayout.ColorField(DEFAULT_VALUE_LABEL, defaultColorValue);

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();

                if (!string.IsNullOrEmpty(errorMessage))
                    EditorGUILayout.LabelField(errorMessage, errorLabel);

                GUILayout.FlexibleSpace();

                EditorGUI.BeginDisabledGroup(!string.IsNullOrEmpty(errorMessage)); // Make ADD button disable if there is an error
                if (GUILayout.Button("Add", GUILayout.Width(55)))
                {
                    ValidateName(fieldName);

                    if (string.IsNullOrEmpty(errorMessage))
                    {
                        editorWindow.Close();
                        colorDebugEditor.AddSetting(new ColorData(fieldName, defaultColorValue));
                    }
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            private void ValidateName(string name)
            {
                if (string.IsNullOrEmpty(name))
                {
                    errorMessage = "Name can't be empty!";
                }
                else if (name.Any(x => char.IsWhiteSpace(x)))
                {
                    errorMessage = "Name shouldn't contain spaces!";
                }
                else if (!Regex.Match(name, @"^[A-Za-z]+$").Success)
                {
                    errorMessage = "Name can only contain latin letters!";
                }
                else if (!Regex.Match(name, @"^[A-Z]").Success)
                {
                    errorMessage = "Name can only begin with capital letter!";
                }
                else if (System.Enum.IsDefined(typeof(CustomColor), name))
                {
                    errorMessage = "Field with this name already exists!";
                }
                else
                {
                    errorMessage = string.Empty;
                }
            }

            public override Vector2 GetWindowSize()
            {
                return WINDOW_SIZE;
            }
        }
    }
}
