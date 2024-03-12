using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomEditor(typeof(AudioSettings))]
    public class AudioSettingsEditor : WatermelonEditor
    {
        private const string SOUNDS_PROPERTY_PATH = "sounds";
        private const string MUSIC_PROPERTY_PATH = "music";
        private const string VIBRATIONS_PROPERTY_PATH = "vibrations";
        private const string SOUND_VARIABLE_EXAMPLE = "exampleSound";
        private const string MUSIC_VARIABLE_EXAMPLE = "exampleMusic";
        private const string SOUNDS_CLASS_NAME = "Sounds";
        private const string MUSIC_CLASS_NAME = "Music";

        

        
        private const string REGEX_PATTERN = "^[a-z][0-9a-zA-Z]+$";
        private const string INVALID_CHARS_USED_MESSAGE = "New variable invalid.Name should start with lower case latin letter and only latin letters and numbers allowed.";
        private const string VARIABLE_NOT_UNIQUE_MESSAGE = "New variable invalid. Variable name must be unique.";
        private const string VALIDATION_PASSED_MESSAGE = "New variable passed validation.";
        private const string NEW_SOUND_VARIABLE = "New sound variable";
        private const string NEW_MUSIC_VARIABLE = "New music variable";
        private string newSoundVariableName;
        private string newMusicVariableName;
        private string soundValidationText;
        private string musicValidationText;
        private bool musicVariablePassedValidation;
        private bool soundVariablePassedValidation;
        private string soundFilePath;
        private string musicFilePath;
        private List<string> existingSoundVariables;
        private List<string> existingMusicVariables;
        private string tempValidationText;
        private bool tempPassedValidation;
        
        private bool addSoundPanelVisible;
        private bool addMusicPanelVisible;
        private Regex regex;

        private GUIContent addButtonContent;
        private GUIContent checkmarkButtonContent;
        private GUIContent addSoundButtonContent;
        private GUIContent addMusicButtonContent;
        private GUIContent removeButtonContent;

        private IEnumerable<SerializedProperty> soundsProperties;
        private IEnumerable<SerializedProperty> musicProperties;
        private IEnumerable<SerializedProperty> vibrationsProperties;

        protected override void OnEnable()
        {
            base.OnEnable();

            soundsProperties = serializedObject.FindProperty(SOUNDS_PROPERTY_PATH).GetChildren();
            musicProperties = serializedObject.FindProperty(MUSIC_PROPERTY_PATH).GetChildren();
            vibrationsProperties = serializedObject.FindProperty(VIBRATIONS_PROPERTY_PATH).GetChildren();

            existingSoundVariables = new List<string>();
            existingMusicVariables = new List<string>();

            foreach (SerializedProperty property in soundsProperties)
            {
                existingSoundVariables.Add(property.name);
            }

            foreach (SerializedProperty property in musicProperties)
            {
                existingMusicVariables.Add(property.name);
            }

            regex = new Regex(REGEX_PATTERN, RegexOptions.Singleline | RegexOptions.Compiled);
            MonoScript soundsScript = EditorUtils.GetAssetByName<MonoScript>(SOUNDS_CLASS_NAME);
            MonoScript musicScript = EditorUtils.GetAssetByName<MonoScript>(MUSIC_CLASS_NAME);
            soundFilePath = EditorUtils.projectFolderPath + AssetDatabase.GetAssetPath(soundsScript);
            musicFilePath = EditorUtils.projectFolderPath + AssetDatabase.GetAssetPath(musicScript);
            newSoundVariableName = SOUND_VARIABLE_EXAMPLE;
            newMusicVariableName = MUSIC_VARIABLE_EXAMPLE;

            if (existingSoundVariables.Contains(SOUND_VARIABLE_EXAMPLE))
            {
                soundValidationText = VARIABLE_NOT_UNIQUE_MESSAGE;
                soundVariablePassedValidation = false;
            }
            else
            {
                soundValidationText = VALIDATION_PASSED_MESSAGE;
                soundVariablePassedValidation = true;
            }

            if (existingMusicVariables.Contains(MUSIC_VARIABLE_EXAMPLE))
            {
                musicValidationText = VARIABLE_NOT_UNIQUE_MESSAGE;
                musicVariablePassedValidation = false;
            }
            else
            {
                musicValidationText = VALIDATION_PASSED_MESSAGE;
                musicVariablePassedValidation = true;
            }
        }

        protected override void Styles()
        {
            addButtonContent = new GUIContent(EditorStylesExtended.GetTexture("icon_add", EditorStylesExtended.IconColor));
            checkmarkButtonContent = new GUIContent(EditorStylesExtended.GetTexture("icon_check", EditorStylesExtended.IconColor));
        }

        public override void OnInspectorGUI()
        {
            InitStyles();

            Rect windowRect = EditorGUILayout.BeginVertical();

            serializedObject.Update();

            EditorStyles.textArea.wordWrap = true;

            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            EditorGUILayoutCustom.Header("AUDIO");

            if(HandleDisplay(soundsProperties, true) || HandleAddElementPannel(true, addSoundPanelVisible, existingSoundVariables)) // avoiding exeptions caused by script reload
            {
                return;
            }

            EditorGUILayout.EndVertical();

            GUILayout.Space(8);

            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            EditorGUILayoutCustom.Header("MUSIC");

            if(HandleDisplay(musicProperties, false) || HandleAddElementPannel(false, addMusicPanelVisible, existingMusicVariables))// avoiding exeptions caused by script reload
            {
                return;
            }

            EditorGUILayout.EndVertical();

            GUILayout.Space(8);

            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            EditorGUI.BeginChangeCheck();
            EditorGUILayoutCustom.Header("VIBRATION");

            foreach (SerializedProperty property in vibrationsProperties)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(property.displayName + " (ms)"));
            }

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.EndVertical();

            EditorGUILayoutCustom.DrawCompileWindow(windowRect);
        }

        private bool HandleDisplay(IEnumerable<SerializedProperty> parentProperty, bool isSound)
        {
            foreach (SerializedProperty property in parentProperty)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(property);

                EditorGUILayout.BeginVertical(GUILayout.Width(16));
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("X", EditorStylesExtended.button_04_mini, GUILayout.Width(24), GUILayout.Height(18)))
                {
                    if (EditorUtility.DisplayDialog("Warning", "Are you sure that you want to delete \"" + property.displayName + "\"?", "Yes", "Cancel"))
                    {
                        if (isSound)
                        {
                            existingSoundVariables.Remove(property.name);
                            UpdateEnum(existingSoundVariables.ToArray(), soundFilePath, SOUNDS_CLASS_NAME);
                        }
                        else
                        {
                            existingMusicVariables.Remove(property.name);
                            UpdateEnum(existingMusicVariables.ToArray(), musicFilePath, MUSIC_CLASS_NAME);
                        }
                    }

                    return true;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                
            }

            return false;
        }

        private bool HandleAddElementPannel(bool isSound, bool panelVisible,List<string> usedVariableNames)
        {
            if (panelVisible)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();

                if (isSound)
                {
                    newSoundVariableName = EditorGUILayout.TextField(NEW_SOUND_VARIABLE, newSoundVariableName);
                }
                else
                {
                    newMusicVariableName = EditorGUILayout.TextField(NEW_MUSIC_VARIABLE, newMusicVariableName);
                }


                if (EditorGUI.EndChangeCheck())
                {
                    if (!regex.IsMatch(newSoundVariableName))
                    {
                        tempValidationText = INVALID_CHARS_USED_MESSAGE;
                        tempPassedValidation = false;
                    }
                    else if (usedVariableNames.Contains(isSound? newSoundVariableName:newMusicVariableName))
                    {
                        tempValidationText = VARIABLE_NOT_UNIQUE_MESSAGE;
                        tempPassedValidation = false;
                    }
                    else
                    {
                        tempValidationText = VALIDATION_PASSED_MESSAGE;
                        tempPassedValidation = true;
                    }

                    if (isSound)
                    {
                        soundValidationText = tempValidationText;
                        soundVariablePassedValidation = tempPassedValidation;
                    }
                    else
                    {
                        musicValidationText = tempValidationText;
                        musicVariablePassedValidation = tempPassedValidation;
                    }
                }

                EditorGUILayout.BeginHorizontal(GUILayout.Width(34));

                EditorGUI.BeginDisabledGroup(isSound? (!soundVariablePassedValidation) : (!musicVariablePassedValidation));

                if (GUILayout.Button(checkmarkButtonContent))
                {
                    string[] variableNames = new string[usedVariableNames.Count + 1];

                    for (int i = 0; i < usedVariableNames.Count; i++)
                    {
                        variableNames[i] = usedVariableNames[i];
                    }

                    if (isSound)
                    {
                        variableNames[variableNames.Length - 1] = newSoundVariableName;

                        newSoundVariableName = SOUND_VARIABLE_EXAMPLE;

                        UpdateEnum(variableNames, soundFilePath, SOUNDS_CLASS_NAME);
                    }
                    else
                    {
                        variableNames[variableNames.Length - 1] = newMusicVariableName;

                        newMusicVariableName = MUSIC_VARIABLE_EXAMPLE;

                        UpdateEnum(variableNames, musicFilePath, MUSIC_CLASS_NAME);
                    }

                    return true;
                }

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.BeginVertical();
                GUILayout.Space(3);

                if (GUILayout.Button("X", EditorStylesExtended.button_04_mini, GUILayout.Width(24), GUILayout.Height(18)))
                {
                    if (isSound)
                    {
                        addSoundPanelVisible = false;
                    }
                    else
                    {
                        addMusicPanelVisible = false;
                    }
                    
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();

                if (isSound)
                {
                    EditorGUILayout.HelpBox(soundValidationText, soundVariablePassedValidation ? MessageType.Info : MessageType.Error);
                }
                else
                {
                    EditorGUILayout.HelpBox(musicValidationText, musicVariablePassedValidation ? MessageType.Info : MessageType.Error);
                }
                

                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(addButtonContent))
                {
                    if (isSound)
                    {
                        addSoundPanelVisible = true;
                    }
                    else
                    {
                        addMusicPanelVisible = true;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            return false;
        }

        private void UpdateEnum(string[] variableNames, string path, string className)
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            stringBuilder.Append("// GENERATED BY CODE! DO NOT MODIFY!");
            stringBuilder.AppendLine();
            stringBuilder.Append("using UnityEngine;");
            stringBuilder.AppendLine();
            stringBuilder.Append("namespace Watermelon");
            stringBuilder.AppendLine();
            stringBuilder.Append("{");
            stringBuilder.AppendLine();
            stringBuilder.Append("\t[System.Serializable]");
            stringBuilder.AppendLine();
            stringBuilder.Append("\tpublic class ");
            stringBuilder.Append(className);
            stringBuilder.AppendLine();
            stringBuilder.Append("\t{");

            for (int i = 0; i < variableNames.Length; i++)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("\t\tpublic AudioClip ");
                stringBuilder.Append(variableNames[i]);
                stringBuilder.Append(";");
            }

            stringBuilder.AppendLine();
            stringBuilder.Append("\t}");
            stringBuilder.AppendLine();
            stringBuilder.Append("}");

            System.IO.File.WriteAllText(path, stringBuilder.ToString(), System.Text.Encoding.UTF8);
            AssetDatabase.Refresh();
        }

    }

    
}

// -----------------
// Audio Controller v 0.3.3
// -----------------