using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static Watermelon.PrefsSettings;

namespace Watermelon
{
    [CustomEditor(typeof(PrefsSettings))]
    public class PrefsSettingsEditor : WatermelonEditor
    {
        private const string SETTINGS_LABEL = "SETTINGS";
        private const string DELETE_LABEL = "X";
        private const string UPDATING_LABEL = "Updating...";
        private const string NAME_LABEL = "name";
        private const string DEFAULT_VALUE_LABEL = "value (default)";
        private const string CURRENT_VALUE_LABEL = "value";
        private const string ADD_BUTTON_ICON_NAME = "icon_preferences";
        private const string RESET_BUTTON_ICON_NAME = "icon_reset";
        private const string PREFS_SETTINGS_SCRIPT_NAME = "KeyGenerated";
        private readonly string SETTING_PROPERTY_NAME = "settings";
        private readonly string NEEDS_UPDATE_PROPERTY_NAME = "needsUpdate";
        private readonly string SETTINGS_INFOS_PROPERTY_NAME = "settingInfos";
        private readonly string BOOL_SETTINGS_PROPERTY_NAME = "boolSettings";
        private readonly string FLOAT_SETTINGS_PROPERTY_NAME = "floatSettings";
        private readonly string INT_SETTINGS_PROPERTY_NAME = "intSettings";
        private readonly string LONG_SETTINGS_PROPERTY_NAME = "longSettings";
        private readonly string STRING_SETTINGS_PROPERTY_NAME = "stringSettings";
        private readonly string DATE_TIME_SETTINGS_PROPERTY_NAME = "dateTimeSettings";
        private readonly string DOUBLE_SETTINGS_PROPERTY_NAME = "doubleSettings";
        private readonly string KEY_PROPERTY_NAME = "key";
        private readonly string FIELD_TYPE_PROPERTY_NAME = "fieldType";
        private readonly string INDEX_PROPERTY_NAME = "index";
        private readonly string DEFAULT_VALUE_PROPERTY_NAME = "defaultValue";
        private readonly string VALUE_PROPERTY_NAME = "value";
        private readonly string PATH_SUFFICS = "\\KeyGenerated.cs";
        private string[] requiredSettingsNames = { };

        private SerializedProperty settingsProperty;
        private SerializedProperty settingInfosProperty;
        private SerializedProperty needsUpdateProperty;
        private SerializedProperty boolSettingsProperty;
        private SerializedProperty floatSettingsProperty;
        private SerializedProperty intSettingsProperty;
        private SerializedProperty longSettingsProperty;
        private SerializedProperty stringSettingsProperty;
        private SerializedProperty dateTimeSettingsProperty;
        private SerializedProperty doubleSettingsProperty;
        private GUIContent addButton;
        private GUIContent resetButton;
        private GUIStyle errorLabel;
        private GUIStyle resetButtonStyle;
        private GUIStyle columnLabelsStyle;
        private string fullEnumFilePath;
        private string enumAssetPath;
        private bool displayCurrentValues;
        private string[] tempValues;
        private string[] tempDefaultValues;

        //allocate memory to temp variables
        private Rect fieldRect;
        private FieldType fieldType;
        private Key key;
        private SerializedProperty keyProperty;
        private string fieldName;
        private int valuePropertyIndex;
        private SerializedProperty valueProperty;
        private float tempFloatValue;
        private int tempIntValue;
        private long tempLongValue;
        private double tempDoubleValue;
        private DateTime tempDateTimeValue;
        

        protected override void OnEnable()
        {
            base.OnEnable();

            //read properties
            settingsProperty = serializedObject.FindProperty(SETTING_PROPERTY_NAME);
            needsUpdateProperty = settingsProperty.FindPropertyRelative(NEEDS_UPDATE_PROPERTY_NAME);
            settingInfosProperty = settingsProperty.FindPropertyRelative(SETTINGS_INFOS_PROPERTY_NAME);
            boolSettingsProperty = settingsProperty.FindPropertyRelative(BOOL_SETTINGS_PROPERTY_NAME);
            floatSettingsProperty = settingsProperty.FindPropertyRelative(FLOAT_SETTINGS_PROPERTY_NAME);
            intSettingsProperty = settingsProperty.FindPropertyRelative(INT_SETTINGS_PROPERTY_NAME);
            longSettingsProperty = settingsProperty.FindPropertyRelative(LONG_SETTINGS_PROPERTY_NAME);
            stringSettingsProperty = settingsProperty.FindPropertyRelative(STRING_SETTINGS_PROPERTY_NAME);
            dateTimeSettingsProperty = settingsProperty.FindPropertyRelative(DATE_TIME_SETTINGS_PROPERTY_NAME);
            doubleSettingsProperty = settingsProperty.FindPropertyRelative(DOUBLE_SETTINGS_PROPERTY_NAME);

            AssemblyReloadEvents.afterAssemblyReload += AfterAssemblyReload;

            //ignoring the rest of initialization if  AfterAssemblyReload needs to be called
            if (!needsUpdateProperty.boolValue)
            {
                (target as PrefsSettings).Init();

                // File path
                MonoScript prefsSettingsScript = EditorUtils.GetAssetByName<MonoScript>(PREFS_SETTINGS_SCRIPT_NAME);
                enumAssetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(prefsSettingsScript)) + PATH_SUFFICS;
                fullEnumFilePath = EditorUtils.projectFolderPath + enumAssetPath;



                //updating = false;
                displayCurrentValues = true;

                PrepareForValidation();
                CollectRequiredAttributes();

                EditorStylesExtended.InitializeStyles();
            }
        }

        private void AfterAssemblyReload()
        {
            if (needsUpdateProperty.boolValue)
            {
                needsUpdateProperty.boolValue = false;
                //handle enum update
                for (int i = 0; i < settingInfosProperty.arraySize; i++)
                {
                    settingInfosProperty.GetArrayElementAtIndex(i).FindPropertyRelative(KEY_PROPERTY_NAME).enumValueIndex = i;
                }

                settingInfosProperty.serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
            }
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
                        tempTypes = assembly.GetTypes().Where(el => el.IsDefined(typeof(RequireSettingAttribute), true)).ToArray();

                        if (!tempTypes.IsNullOrEmpty())
                        {
                            types.AddRange(tempTypes);
                        }

                    } catch (ReflectionTypeLoadException) { }
                }
            }

            //return when there no defined required settings
            if (types.Count == 0)
            {
                return;
            }

            //collect attributes from types
            List<RequireSettingAttribute> settingAttributes = new List<RequireSettingAttribute>();

            foreach (Type tempType in types)
            {
                settingAttributes.AddRange((RequireSettingAttribute[])Attribute.GetCustomAttributes(tempType, typeof(RequireSettingAttribute)));
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
            string[] existingSettingNames = new string[settingInfosProperty.arraySize];

            for (int i = 0; i < settingInfosProperty.arraySize; i++)
            {
                keyProperty = settingInfosProperty.GetArrayElementAtIndex(i).FindPropertyRelative(KEY_PROPERTY_NAME);
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
            SettingData[] newElements = new SettingData[settingAttributes.Count];

            for (int i = 0; i < settingAttributes.Count; i++)
            {
                newElements[i] = new SettingData(settingAttributes[i].name, settingAttributes[i].fieldType, GetDefaultValue(settingAttributes[i].fieldType));
            }

            AddSetting(newElements);
        }

        private object GetDefaultValue(FieldType fieldType)
        {
            switch (fieldType)
            {
                case FieldType.Bool:
                    return false;
                case FieldType.DateTime:
                    return DateTime.Now.ToString();
                case FieldType.String:
                    return string.Empty;
                case FieldType.Float:
                    return 0f;
                case FieldType.Int:
                    return 0;
                case FieldType.Long:
                    return 0L;
                case FieldType.Double:
                    return 0.0;
                default:
                    return null;
            }
        }

        protected override void Styles()
        {
            addButton = new GUIContent(string.Empty, EditorStylesExtended.GetTexture(ADD_BUTTON_ICON_NAME, EditorStylesExtended.IconColor));
            resetButton = new GUIContent(string.Empty, EditorStylesExtended.GetTexture(RESET_BUTTON_ICON_NAME, EditorStylesExtended.IconColor));
            errorLabel = new GUIStyle(EditorStylesExtended.label_small);
            errorLabel.normal.textColor = EditorColor.red01;

            resetButtonStyle = new GUIStyle(EditorStylesExtended.button_01_mini);
            resetButtonStyle.padding = new RectOffset(3, 3, 3, 3);

            columnLabelsStyle = new GUIStyle(EditorStylesExtended.label_small);
            columnLabelsStyle.fontSize = 9;
            columnLabelsStyle.alignment = TextAnchor.MiddleLeft;
            columnLabelsStyle.stretchWidth = false;
            columnLabelsStyle.wordWrap = false;
        }

        public override void OnInspectorGUI()
        {
            InitStyles();

            if (needsUpdateProperty.boolValue)
            {
                EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

                EditorGUILayout.BeginHorizontal(EditorStylesExtended.padding05, GUILayout.Height(21), GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField(SETTINGS_LABEL, EditorStylesExtended.boxHeader, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField(UPDATING_LABEL, EditorStylesExtended.label_small, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                EditorGUILayout.EndVertical();

                return;
            }

            if (Application.isPlaying && serializedObject.UpdateIfRequiredOrScript())
            {
                PrepareForValidation();
            }

            //Settings label and add settings button
            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            Rect rect = EditorGUILayout.BeginHorizontal(EditorStylesExtended.padding05, GUILayout.Height(21), GUILayout.ExpandWidth(true));

            EditorGUILayout.LabelField(SETTINGS_LABEL, EditorStylesExtended.boxHeader, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            if (GUILayout.Button(addButton, EditorStylesExtended.button_01, GUILayout.Height(24), GUILayout.Width(24)))
            {
                PopupWindow.Show(new Rect(rect.width, rect.y, 0, rect.height), new PrefsSettingsWindow(this, errorLabel));
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            
            //name label and default/current label
            EditorGUILayout.BeginHorizontal(GUILayout.Height(14));
            EditorGUILayout.LabelField(NAME_LABEL, columnLabelsStyle, GUILayout.Width(EditorGUIUtility.labelWidth));

            if (GUILayout.Button(displayCurrentValues ? CURRENT_VALUE_LABEL : DEFAULT_VALUE_LABEL, columnLabelsStyle))
            {
                displayCurrentValues = !displayCurrentValues;
            }

            EditorGUILayout.EndHorizontal();

            //settings
            for (int i = 0; i < settingInfosProperty.arraySize; i++)
            {
                fieldRect = EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(Application.isPlaying && (!displayCurrentValues));
                DrawPropertyField(settingInfosProperty.GetArrayElementAtIndex(i), i);
                EditorGUI.EndDisabledGroup();

                if (displayCurrentValues)
                {
                    if (GUILayout.Button(resetButton, resetButtonStyle, GUILayout.Width(16), GUILayout.Height(16)))
                    {
                        ResetProperty(settingInfosProperty.GetArrayElementAtIndex(i), i);
                    }
                }

                EditorGUI.BeginDisabledGroup(requiredSettingsNames.Contains(fieldName) || Application.isPlaying);

                if (GUILayout.Button(DELETE_LABEL, EditorStylesExtended.button_04_mini, GUILayout.Width(16), GUILayout.Height(16)))
                {
                    if(EditorUtility.DisplayDialog("Delete dialog","Are you sure you want to delete \"" +fieldName +"\" setting?", "Ok", "Cancel"))
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

        private void PrepareForValidation()
        {
            tempValues = new string[settingInfosProperty.arraySize];
            tempDefaultValues = new string[settingInfosProperty.arraySize];
            SerializedProperty settingInfo;

            for (int i = 0; i < settingInfosProperty.arraySize; i++)
            {
                settingInfo = settingInfosProperty.GetArrayElementAtIndex(i);
                fieldType = (FieldType)settingInfo.FindPropertyRelative(FIELD_TYPE_PROPERTY_NAME).enumValueIndex;
                valuePropertyIndex = settingInfo.FindPropertyRelative(INDEX_PROPERTY_NAME).intValue;

                switch (fieldType)
                {
                    case FieldType.Bool: // don`t need to validate
                        tempValues[i] = boolSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(VALUE_PROPERTY_NAME).boolValue.ToString();
                        tempDefaultValues[i] = boolSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).boolValue.ToString();
                        break;
                    case FieldType.Float:
                        tempValues[i] = floatSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(VALUE_PROPERTY_NAME).floatValue.ToString();
                        tempDefaultValues[i] = floatSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).floatValue.ToString();
                        break;
                    case FieldType.Int:
                        tempValues[i] = intSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(VALUE_PROPERTY_NAME).intValue.ToString();
                        tempDefaultValues[i] = intSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).intValue.ToString();
                        break;
                    case FieldType.Long:
                        tempValues[i] = longSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(VALUE_PROPERTY_NAME).longValue.ToString();
                        tempDefaultValues[i] = longSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).longValue.ToString();
                        break;
                    case FieldType.String: // don`t need to validate
                        tempValues[i] = stringSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(VALUE_PROPERTY_NAME).stringValue;
                        tempDefaultValues[i] = stringSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).stringValue;
                        break;
                    case FieldType.DateTime:
                        tempValues[i] = dateTimeSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(VALUE_PROPERTY_NAME).stringValue.ToString();
                        tempDefaultValues[i] = dateTimeSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).stringValue.ToString();
                        break;
                    case FieldType.Double:
                        tempValues[i] = doubleSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(VALUE_PROPERTY_NAME).doubleValue.ToString();
                        tempDefaultValues[i] = doubleSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).doubleValue.ToString();
                        break;
                }
            }
        }

        private void ResetProperty(SerializedProperty settingInfo, int settingInfoIndex)
        {
            fieldType = (FieldType)settingInfo.FindPropertyRelative(FIELD_TYPE_PROPERTY_NAME).enumValueIndex;
            keyProperty = settingInfo.FindPropertyRelative(KEY_PROPERTY_NAME);
            fieldName = keyProperty.enumDisplayNames[keyProperty.enumValueIndex];
            valuePropertyIndex = settingInfo.FindPropertyRelative(INDEX_PROPERTY_NAME).intValue;
            key = (Key)keyProperty.enumValueIndex;

            switch (fieldType)
            {
                case FieldType.Bool:
                    GetValueProperty(fieldType, valuePropertyIndex, true).boolValue =  GetValueProperty(fieldType, valuePropertyIndex, false).boolValue;
                    PrefsSettings.SetBool(key, GetValueProperty(fieldType, valuePropertyIndex, false).boolValue);
                    break;
                case FieldType.Float:
                    GetValueProperty(fieldType, valuePropertyIndex, true).floatValue = GetValueProperty(fieldType, valuePropertyIndex, false).floatValue;
                    PrefsSettings.SetFloat(key, GetValueProperty(fieldType, valuePropertyIndex, false).floatValue);
                    break;
                case FieldType.Int:
                    GetValueProperty(fieldType, valuePropertyIndex, true).intValue = GetValueProperty(fieldType, valuePropertyIndex, false).intValue;
                    PrefsSettings.SetInt(key, GetValueProperty(fieldType, valuePropertyIndex, false).intValue);
                    break;
                case FieldType.Long:
                    GetValueProperty(fieldType, valuePropertyIndex, true).longValue = GetValueProperty(fieldType, valuePropertyIndex, false).longValue;
                    PrefsSettings.SetLong(key, GetValueProperty(fieldType, valuePropertyIndex, false).longValue);
                    break;
                case FieldType.String:
                    GetValueProperty(fieldType, valuePropertyIndex, true).stringValue = GetValueProperty(fieldType, valuePropertyIndex, false).stringValue;
                    PrefsSettings.SetString(key, GetValueProperty(fieldType, valuePropertyIndex, false).stringValue);
                    break;
                case FieldType.DateTime:
                    GetValueProperty(fieldType, valuePropertyIndex, true).stringValue = GetValueProperty(fieldType, valuePropertyIndex, false).stringValue;
                    PrefsSettings.SetDateTime(key, GetValueProperty(fieldType, valuePropertyIndex, false).stringValue);
                    break;
                case FieldType.Double:
                    GetValueProperty(fieldType, valuePropertyIndex, true).doubleValue = GetValueProperty(fieldType, valuePropertyIndex, false).doubleValue;
                    PrefsSettings.SetDouble(key, GetValueProperty(fieldType, valuePropertyIndex, false).doubleValue);
                    break;
            }

            PrepareForValidation();
        }

        private void DrawPropertyField(SerializedProperty settingInfo, int settingInfoIndex)
        {
            fieldType = (FieldType)settingInfo.FindPropertyRelative(FIELD_TYPE_PROPERTY_NAME).enumValueIndex;
            keyProperty = settingInfo.FindPropertyRelative(KEY_PROPERTY_NAME);
            fieldName = keyProperty.enumDisplayNames[keyProperty.enumValueIndex];
            valuePropertyIndex = settingInfo.FindPropertyRelative(INDEX_PROPERTY_NAME).intValue;
            key = (Key)keyProperty.enumValueIndex;

            valueProperty = GetValueProperty(fieldType, valuePropertyIndex, displayCurrentValues);
            
            if(fieldType == FieldType.Bool)
            {
                valueProperty.boolValue = EditorGUILayout.Toggle(fieldName, valueProperty.boolValue);

                
                if (displayCurrentValues)
                {
                    if (displayCurrentValues && (!valueProperty.boolValue.ToString().Equals(tempValues[settingInfoIndex])))
                    {
                        PrefsSettings.SetBool(key, valueProperty.boolValue);// write change into PlayerPrefs
                        tempValues[settingInfoIndex] = valueProperty.boolValue.ToString();
                    }
                }
            }
            else if(fieldType == FieldType.String)
            {
                valueProperty.stringValue = EditorGUILayout.TextField(fieldName, valueProperty.stringValue);

                if (displayCurrentValues && (!valueProperty.stringValue.Equals(tempValues[settingInfoIndex])))
                {
                    PrefsSettings.SetString(key, valueProperty.stringValue);// write change into PlayerPrefs
                    tempValues[settingInfoIndex] = valueProperty.stringValue;
                }
            }
            else
            {
                if (displayCurrentValues)
                {
                    tempValues[settingInfoIndex] = EditorGUILayout.TextField(fieldName, tempValues[settingInfoIndex]);
                    tempValues[settingInfoIndex] = PreprocessValue(tempValues[settingInfoIndex], fieldType);
                    UpdateValue(key, tempValues[settingInfoIndex], fieldType);

                }
                else
                {
                    tempDefaultValues[settingInfoIndex] = EditorGUILayout.TextField(fieldName, tempDefaultValues[settingInfoIndex]);
                    tempDefaultValues[settingInfoIndex] = PreprocessValue(tempDefaultValues[settingInfoIndex], fieldType);
                    UpdateValue(key, tempDefaultValues[settingInfoIndex], fieldType);
                }
            }

        }

        private SerializedProperty GetValueProperty(FieldType fieldType, int valuePropertyIndex, bool getCurrentValue)
        {
            switch (fieldType)
            {
                case FieldType.Bool:
                    return boolSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(getCurrentValue ? VALUE_PROPERTY_NAME : DEFAULT_VALUE_PROPERTY_NAME);
                case FieldType.Float:
                    return floatSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(getCurrentValue ? VALUE_PROPERTY_NAME : DEFAULT_VALUE_PROPERTY_NAME);
                case FieldType.Int:
                    return intSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(getCurrentValue ? VALUE_PROPERTY_NAME : DEFAULT_VALUE_PROPERTY_NAME);
                case FieldType.Long:
                    return longSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(getCurrentValue ? VALUE_PROPERTY_NAME : DEFAULT_VALUE_PROPERTY_NAME);
                case FieldType.String:
                    return stringSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(getCurrentValue ? VALUE_PROPERTY_NAME : DEFAULT_VALUE_PROPERTY_NAME);
                case FieldType.DateTime:
                    return dateTimeSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(getCurrentValue ? VALUE_PROPERTY_NAME : DEFAULT_VALUE_PROPERTY_NAME);
                case FieldType.Double:
                    return doubleSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(getCurrentValue ? VALUE_PROPERTY_NAME : DEFAULT_VALUE_PROPERTY_NAME);
                default:
                    throw new System.Exception("Unknown enum value");
            }
        }

        private string PreprocessValue(string currentValue, FieldType fieldType)
        {
            if ((fieldType == FieldType.Float)||(fieldType == FieldType.Double))
            {
                if (currentValue.Contains('.'))
                {
                    return currentValue.Replace('.', ',');
                }
            }

            return currentValue;
        }

        private void UpdateValue(Key key, string currentTempValue, FieldType fieldType)
        {
            if (fieldType == FieldType.Float)
            {
                if (float.TryParse(currentTempValue, out tempFloatValue))
                {
                    if (displayCurrentValues && (valueProperty.floatValue != tempFloatValue)) // write change into PlayerPrefs
                    {
                        PrefsSettings.SetFloat(key, tempFloatValue);
                    }

                    valueProperty.floatValue = tempFloatValue;
                }
                else
                {
                    EditorGUILayout.LabelField("Incorrect float value", errorLabel);
                }
            }
            else if (fieldType == FieldType.Int)
            {
                if (int.TryParse(currentTempValue, out tempIntValue))
                {
                    if (displayCurrentValues && (valueProperty.intValue != tempIntValue)) // write change into PlayerPrefs
                    {
                        PrefsSettings.SetInt(key, tempIntValue);
                    }

                    valueProperty.intValue = tempIntValue;
                }
                else
                {
                    EditorGUILayout.LabelField("Incorrect int value", errorLabel);
                }
            }
            else if (fieldType == FieldType.Long)
            {
                if (long.TryParse(currentTempValue, out tempLongValue))
                {
                    if (displayCurrentValues && (valueProperty.longValue != tempLongValue)) // write change into PlayerPrefs
                    {
                        PrefsSettings.SetLong(key, tempLongValue);
                    }

                    valueProperty.longValue = tempLongValue;
                }
                else
                {
                    EditorGUILayout.LabelField("Incorrect long value", errorLabel);
                }
            }
            else if (fieldType == FieldType.DateTime)
            {
                if (DateTime.TryParse(currentTempValue, out tempDateTimeValue))
                {
                    if (displayCurrentValues && (!valueProperty.stringValue.Equals(tempDateTimeValue.ToString()))) // write change into PlayerPrefs
                    {
                        PrefsSettings.SetDateTime(key, tempDateTimeValue);
                    }

                    valueProperty.stringValue = tempDateTimeValue.ToString();
                }
                else
                {
                    EditorGUILayout.LabelField("Incorrect DateTime value", errorLabel);
                }
            }
            else if (fieldType == FieldType.Double)
            {
                if (double.TryParse(currentTempValue, out tempDoubleValue))
                {
                    if (displayCurrentValues && (valueProperty.doubleValue != tempDoubleValue)) // write change into PlayerPrefs
                    {
                        PrefsSettings.SetDouble(key, tempDoubleValue);
                    }
                    valueProperty.doubleValue = tempDoubleValue;
                }
                else
                {
                    EditorGUILayout.LabelField("Incorrect double value", errorLabel);
                }
            }
        }

        private void DeleteSetting(int index)
        {
            needsUpdateProperty.boolValue = true;
            List<SettingData> settingDatas = CollectSettingsData();
            settingDatas.RemoveAt(index);
            UpdateSettings(settingDatas);
        }

        private void AddSetting(SettingData newElement)
        {
            needsUpdateProperty.boolValue = true;
            List<SettingData> settingDatas = CollectSettingsData();
            settingDatas.Add(newElement);
            UpdateSettings(settingDatas);
        }

        private void AddSetting(SettingData[] newElements)
        {
            needsUpdateProperty.boolValue = true;
            List<SettingData> settingDatas = CollectSettingsData();
            settingDatas.AddRange(newElements);
            UpdateSettings(settingDatas);
        }

        private List<SettingData> CollectSettingsData()
        {
            List<SettingData> resultList = new List<SettingData>();
            SerializedProperty settingInfo;
            object value = null;
            object defaultValue = null;

            for (int i = 0; i < settingInfosProperty.arraySize; i++)
            {
                settingInfo = settingInfosProperty.GetArrayElementAtIndex(i);
                fieldType = (FieldType)settingInfo.FindPropertyRelative(FIELD_TYPE_PROPERTY_NAME).enumValueIndex;
                keyProperty = settingInfo.FindPropertyRelative(KEY_PROPERTY_NAME);
                fieldName = keyProperty.enumNames[keyProperty.enumValueIndex];
                valuePropertyIndex = settingInfo.FindPropertyRelative(INDEX_PROPERTY_NAME).intValue;

                switch (fieldType)
                {
                    case FieldType.Bool:
                        value = boolSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(VALUE_PROPERTY_NAME).boolValue;
                        defaultValue = boolSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).boolValue;
                        break;
                    case FieldType.Float:
                        value = floatSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(VALUE_PROPERTY_NAME).floatValue;
                        defaultValue = floatSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).floatValue;
                        break;
                    case FieldType.Int:
                        value = intSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(VALUE_PROPERTY_NAME).intValue;
                        defaultValue = intSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).intValue;
                        break;
                    case FieldType.Long:
                        value = longSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(VALUE_PROPERTY_NAME).longValue;
                        defaultValue = longSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).longValue;
                        break;
                    case FieldType.String:
                        value = stringSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(VALUE_PROPERTY_NAME).stringValue;
                        defaultValue = stringSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).stringValue;
                        break;
                    case FieldType.DateTime:
                        value = dateTimeSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(VALUE_PROPERTY_NAME).stringValue;
                        defaultValue = dateTimeSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).stringValue;
                        break;
                    case FieldType.Double:
                        value = doubleSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(VALUE_PROPERTY_NAME).doubleValue;
                        defaultValue = doubleSettingsProperty.GetArrayElementAtIndex(valuePropertyIndex).FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).doubleValue;
                        break;
                }

                resultList.Add(new SettingData(fieldName, fieldType, value, defaultValue));
            }

            return resultList;
        }

        private void UpdateSettings(List<SettingData> settingDatas)
        {
            ClearDatabase();
            UpdateEnum(settingDatas);
            FillDatabase(settingDatas);
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.ImportAsset(enumAssetPath, ImportAssetOptions.ForceUpdate);
            EditorUtility.RequestScriptReload();
        }

        private void ClearDatabase()
        {
            settingInfosProperty.arraySize = 0;
            boolSettingsProperty.arraySize = 0;
            floatSettingsProperty.arraySize = 0;
            intSettingsProperty.arraySize = 0;
            longSettingsProperty.arraySize = 0;
            stringSettingsProperty.arraySize = 0;
            dateTimeSettingsProperty.arraySize = 0;
            doubleSettingsProperty.arraySize = 0;
        }

        private void UpdateEnum(List<SettingData> settingDatas)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("// GENERATED BY CODE! DO NOT MODIFY!");
            stringBuilder.AppendLine();
            stringBuilder.Append("namespace Watermelon");
            stringBuilder.AppendLine();
            stringBuilder.Append("{");
            stringBuilder.AppendLine();
            stringBuilder.Append("\tpublic partial class PrefsSettings");
            stringBuilder.AppendLine();
            stringBuilder.Append("\t{");
            stringBuilder.AppendLine();
            stringBuilder.Append("\t\tpublic enum Key");
            stringBuilder.AppendLine();
            stringBuilder.Append("\t\t{");            
            stringBuilder.AppendLine();

            for (int i = 0; i < settingDatas.Count; i++)
            {
                stringBuilder.Append("\t\t\t");
                stringBuilder.Append(settingDatas[i].name);
                stringBuilder.Append(" = ");
                stringBuilder.Append(i);
                stringBuilder.Append(",");
                stringBuilder.AppendLine();
            }

            stringBuilder.Append("\t\t}");
            stringBuilder.AppendLine();
            stringBuilder.Append("\t}");
            stringBuilder.AppendLine();
            stringBuilder.Append("}");

            File.WriteAllText(fullEnumFilePath, stringBuilder.ToString(), Encoding.UTF8);
        }

        private void FillDatabase(List<SettingData> settingDatas)
        {
            SerializedProperty settingInfo;
            SerializedProperty tempProperty;

            for (int i = 0; i < settingDatas.Count; i++)
            {
                settingInfosProperty.arraySize++;
                settingInfo = settingInfosProperty.GetArrayElementAtIndex(i);
                settingInfo.FindPropertyRelative(FIELD_TYPE_PROPERTY_NAME).enumValueIndex = (int)settingDatas[i].fieldType;

                switch (settingDatas[i].fieldType)
                {
                    case FieldType.Bool:
                        settingInfo.FindPropertyRelative(INDEX_PROPERTY_NAME).intValue = boolSettingsProperty.arraySize;
                        boolSettingsProperty.arraySize++;
                        tempProperty = boolSettingsProperty.GetArrayElementAtIndex(boolSettingsProperty.arraySize - 1);
                        tempProperty.FindPropertyRelative(VALUE_PROPERTY_NAME).boolValue = (bool)settingDatas[i].value;
                        tempProperty.FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).boolValue = (bool)settingDatas[i].defaultValue;
                        break;
                    case FieldType.Float:
                        settingInfo.FindPropertyRelative(INDEX_PROPERTY_NAME).intValue = floatSettingsProperty.arraySize;
                        floatSettingsProperty.arraySize++;
                        tempProperty = floatSettingsProperty.GetArrayElementAtIndex(floatSettingsProperty.arraySize - 1);
                        tempProperty.FindPropertyRelative(VALUE_PROPERTY_NAME).floatValue = (float)settingDatas[i].value;
                        tempProperty.FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).floatValue = (float)settingDatas[i].defaultValue;
                        break;
                    case FieldType.Int:
                        settingInfo.FindPropertyRelative(INDEX_PROPERTY_NAME).intValue = intSettingsProperty.arraySize;
                        intSettingsProperty.arraySize++;
                        tempProperty = intSettingsProperty.GetArrayElementAtIndex(intSettingsProperty.arraySize - 1);
                        tempProperty.FindPropertyRelative(VALUE_PROPERTY_NAME).intValue = (int)settingDatas[i].value;
                        tempProperty.FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).intValue = (int)settingDatas[i].defaultValue;
                        break;
                    case FieldType.Long:
                        settingInfo.FindPropertyRelative(INDEX_PROPERTY_NAME).intValue = longSettingsProperty.arraySize;
                        longSettingsProperty.arraySize++;
                        tempProperty = longSettingsProperty.GetArrayElementAtIndex(longSettingsProperty.arraySize - 1);
                        tempProperty.FindPropertyRelative(VALUE_PROPERTY_NAME).longValue = (long)settingDatas[i].value;
                        tempProperty.FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).longValue = (long)settingDatas[i].defaultValue;
                        break;
                    case FieldType.String:
                        settingInfo.FindPropertyRelative(INDEX_PROPERTY_NAME).intValue = stringSettingsProperty.arraySize;
                        stringSettingsProperty.arraySize++;
                        tempProperty = stringSettingsProperty.GetArrayElementAtIndex(stringSettingsProperty.arraySize - 1);
                        tempProperty.FindPropertyRelative(VALUE_PROPERTY_NAME).stringValue = (string)settingDatas[i].value;
                        tempProperty.FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).stringValue = (string)settingDatas[i].defaultValue;
                        break;
                    case FieldType.DateTime:
                        settingInfo.FindPropertyRelative(INDEX_PROPERTY_NAME).intValue = dateTimeSettingsProperty.arraySize;
                        dateTimeSettingsProperty.arraySize++;
                        tempProperty = dateTimeSettingsProperty.GetArrayElementAtIndex(dateTimeSettingsProperty.arraySize - 1);
                        tempProperty.FindPropertyRelative(VALUE_PROPERTY_NAME).stringValue = (string)settingDatas[i].value;
                        tempProperty.FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).stringValue = (string)settingDatas[i].defaultValue;
                        break;
                    case FieldType.Double:
                        settingInfo.FindPropertyRelative(INDEX_PROPERTY_NAME).intValue = doubleSettingsProperty.arraySize;
                        doubleSettingsProperty.arraySize++;
                        tempProperty = doubleSettingsProperty.GetArrayElementAtIndex(doubleSettingsProperty.arraySize - 1);
                        tempProperty.FindPropertyRelative(VALUE_PROPERTY_NAME).doubleValue = (double)settingDatas[i].value;
                        tempProperty.FindPropertyRelative(DEFAULT_VALUE_PROPERTY_NAME).doubleValue = (double)settingDatas[i].defaultValue;
                        break;
                }
            }
        }

        private struct SettingData
        {
            public string name;
            public FieldType fieldType;
            public object value;
            public object defaultValue;

            public SettingData(string name, FieldType fieldType, object value, object defaultValue)
            {
                this.name = name;
                this.fieldType = fieldType;
                this.value = value;
                this.defaultValue = defaultValue;
            }

            public SettingData(string name, FieldType fieldType, object defaultValue)
            {
                this.name = name;
                this.fieldType = fieldType;
                this.defaultValue = defaultValue;
                this.value = defaultValue;
            }

        }

        private class PrefsSettingsWindow : PopupWindowContent
        {
            private const string DEFAULT_VALUE_LABEL = "Default Value";
            private static readonly Vector2 WINDOW_SIZE = new Vector2(344, 102);

            private string fieldName = "";
            private FieldType fieldType = FieldType.String;

            private GUIStyle errorLabel;
            private string errorMessage;
            private PrefsSettingsEditor prefsSettingsEditor;
            private bool defaultBoolValue;
            private float defaultFloatValue;
            private int defaultIntValue;
            private long defaultLongValue;
            private string defaultStringValue;
            private string tempValue;
            private DateTime defaultDateTimeValue;
            private double defaultDoubleValue;
            private object defaultValue;

            public PrefsSettingsWindow(PrefsSettingsEditor prefsSettingsEditor, GUIStyle errorLabel)
            {
                this.prefsSettingsEditor = prefsSettingsEditor;
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
                defaultBoolValue = false;
                defaultFloatValue = 0;
                defaultIntValue = 0;
                defaultLongValue = 0;
                defaultStringValue = string.Empty;
                defaultDateTimeValue = DateTime.Now;
                defaultDoubleValue = 0;
                errorMessage = string.Empty;

                switch (fieldType)
                {
                    case FieldType.Float:
                        defaultValue = defaultFloatValue;
                        tempValue = defaultFloatValue.ToString();
                        break;
                    case FieldType.Int:
                        defaultValue = defaultIntValue;
                        tempValue = defaultIntValue.ToString();
                        break;
                    case FieldType.Long:
                        defaultValue = defaultLongValue;
                        tempValue = defaultLongValue.ToString();
                        break;
                    case FieldType.DateTime:
                        defaultValue = defaultDateTimeValue.ToString();
                        tempValue = defaultDateTimeValue.ToString();
                        break;
                    case FieldType.Double:
                        defaultValue = defaultDoubleValue;
                        tempValue = defaultDoubleValue.ToString();
                        break;
                }
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

                EditorGUI.BeginChangeCheck();
                fieldType = (FieldType)EditorGUILayout.EnumPopup("Type", fieldType);
                if (EditorGUI.EndChangeCheck())
                {
                    ResetDefaultValues();
                }

                DrawDefaulValue();

                EditorGUILayout.BeginHorizontal();

                if (!string.IsNullOrEmpty(errorMessage))
                    EditorGUILayout.LabelField(errorMessage, errorLabel);

                GUILayout.FlexibleSpace();

                EditorGUI.BeginDisabledGroup(!string.IsNullOrEmpty(errorMessage));
                if (GUILayout.Button("Add", GUILayout.Width(55)))
                {
                    ValidateName(fieldName);

                    if (string.IsNullOrEmpty(errorMessage))
                    {
                        editorWindow.Close();
                        prefsSettingsEditor.AddSetting(new SettingData(fieldName, fieldType, defaultValue));
                    }
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            private void DrawDefaulValue()
            {
                if (fieldType == FieldType.Bool)
                {
                    defaultBoolValue = EditorGUILayout.Toggle(DEFAULT_VALUE_LABEL, defaultBoolValue);
                    defaultValue = defaultBoolValue;
                }
                else if(fieldType == FieldType.Float)
                {
                    EditorGUI.BeginChangeCheck();
                    tempValue = EditorGUILayout.TextField(DEFAULT_VALUE_LABEL, tempValue);

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (tempValue.Contains('.'))
                        {
                            tempValue = tempValue.Replace('.', ',');
                        }

                        if (float.TryParse(tempValue, out defaultFloatValue))
                        {
                            defaultValue = defaultFloatValue;
                            errorMessage = string.Empty;
                        }
                        else
                        {
                            errorMessage = "Incorrect default float value";
                        }
                    }
                }
                else if (fieldType == FieldType.Int)
                {
                    EditorGUI.BeginChangeCheck();
                    tempValue = EditorGUILayout.TextField(DEFAULT_VALUE_LABEL, tempValue);

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (int.TryParse(tempValue, out defaultIntValue))
                        {
                            defaultValue = defaultIntValue;
                            errorMessage = string.Empty;
                        }
                        else
                        {
                            errorMessage = "Incorrect default int value";
                        }
                    }
                }
                else if (fieldType == FieldType.Long)
                {
                    EditorGUI.BeginChangeCheck();
                    tempValue = EditorGUILayout.TextField(DEFAULT_VALUE_LABEL, tempValue);

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (long.TryParse(tempValue, out defaultLongValue))
                        {
                            defaultValue = defaultLongValue;
                            errorMessage = string.Empty;
                        }
                        else
                        {
                            errorMessage = "Incorrect default long value";
                        }
                    }
                }
                else if (fieldType == FieldType.String)
                {
                    defaultStringValue = EditorGUILayout.TextField(DEFAULT_VALUE_LABEL, defaultStringValue);
                    defaultValue = defaultStringValue;
                }
                else if (fieldType == FieldType.DateTime)
                {
                    EditorGUI.BeginChangeCheck();
                    tempValue = EditorGUILayout.TextField(DEFAULT_VALUE_LABEL, tempValue);

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (DateTime.TryParse(tempValue,out defaultDateTimeValue))
                        {
                            defaultValue = defaultDateTimeValue.ToString(); ;
                            errorMessage = string.Empty;
                        }
                        else
                        {
                            errorMessage = "Incorrect default DateTime value";
                        }
                    }
                }
                else if (fieldType == FieldType.Double)
                {
                    EditorGUI.BeginChangeCheck();
                    tempValue = EditorGUILayout.TextField(DEFAULT_VALUE_LABEL, tempValue);

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (tempValue.Contains('.'))
                        {
                            tempValue = tempValue.Replace('.', ',');
                        }

                        if (double.TryParse(tempValue, out defaultDoubleValue))
                        {
                            defaultValue = defaultDoubleValue;
                            errorMessage = string.Empty;
                        }
                        else
                        {
                            errorMessage = "Incorrect default double value";
                        }
                    }
                }
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
                else if (System.Enum.IsDefined(typeof(PrefsSettings.Key), name))
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
