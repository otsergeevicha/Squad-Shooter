using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.IO;

namespace Watermelon
{
    [CustomEditor(typeof(CoreOrganizer))]
    public class CoreOrganizerEditor : WatermelonEditor
    {
        private const string CORE_SETTINGS_PROPERTY_PATH = "coreSettings";
        private const string NAME_PROPERTY_PATH = "name";
        private const string DIRECTORY_PROPERTY_PATH = "directory";
        private const string STATUS_PROPERTY_PATH = "status";
        private const string INCLUDE_PROPERTY_PATH = "include";

        private const string LIST_HEADER = "Core Settings"; 
        private const string UPDATE_STATUS_LABEL = "Update status"; 
        private const string ORGANIZE_LABEL = "Organize";
        private const string SEPARATOR = " | ";
        private const string EXISTS_STATUS = "Exists";
        private const string TO_DELETE_STATUS = "To Delete";
        private const string REMOVED_STATUS = "Removed";
        private const string MISSING_STATUS = "Missing";

        private const int DEFAULT_PADDING = 8;
        private const int SMALL_PADDING = 2;
        private const string PATH_SEPARATOR = "/";
        private const string META_FILE_SUFFIX = ".meta";
        private readonly float EXPANDED_ELEMENT_HEIGHT = (EditorGUIUtility.singleLineHeight + SMALL_PADDING) * 5;
        SerializedProperty coreSettingsProperty;
        SerializedProperty nameProperty;
        SerializedProperty directoryProperty;
        SerializedProperty statusProperty;
        SerializedProperty includeProperty;
        SerializedProperty currentElementProperty;
        

        ReorderableList list;
        Rect workRect;
        Rect fieldRect;
        GUIStyle greenStyle;
        GUIStyle yellowStyle;
        GUIStyle redStyle;
        GUIStyle labelStyle;

        //Colors to change
        Color greenBackroundColor = new Color(0.466f, 0.729f, 0.6f, 1f);
        Color yellowBackroundColor = new Color(0.906f, 0.663f, 0.467f, 1f);
        Color redBackroundColor = new Color(0.666f, 0.266f, 0.396f, 1f);
        Color defaultTextColor = Color.black;
        Color activeTextColor = Color.white;

        

        protected override void OnEnable()
        {
            base.OnEnable();
            coreSettingsProperty = serializedObject.FindProperty(CORE_SETTINGS_PROPERTY_PATH);

            list = new ReorderableList(serializedObject, coreSettingsProperty, true, true, true, true);
            
            list.drawHeaderCallback += DrawHeaderCallback;
            list.drawElementCallback += DrawElementCallback;
            list.elementHeightCallback += ElementHeightCallback;
            list.drawElementBackgroundCallback += DrawElementBackgroundCallback;
            list.onAddCallback += OnAddCallback;
            
        }

        protected override void Styles()
        {
            base.Styles();
            greenStyle = new GUIStyle();
            greenStyle.onNormal.background = MakeColoredTexture(2, 2, greenBackroundColor);
            yellowStyle = new GUIStyle();
            yellowStyle.onNormal.background = MakeColoredTexture(2, 2, yellowBackroundColor);
            redStyle = new GUIStyle();
            redStyle.onNormal.background = MakeColoredTexture(2, 2, redBackroundColor);
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = defaultTextColor;
            labelStyle.focused.textColor = activeTextColor;
            labelStyle.active.textColor = activeTextColor;
            labelStyle.hover.textColor = activeTextColor;
        }

        private Texture2D MakeColoredTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = color;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }

        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, LIST_HEADER);
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            OpenElement(index);

            workRect = new Rect(rect);
            workRect.height = EditorGUIUtility.singleLineHeight;
            workRect.xMin += DEFAULT_PADDING;

            currentElementProperty.isExpanded = EditorGUI.Foldout(workRect, currentElementProperty.isExpanded, GUIContent.none);
            workRect.xMin += DEFAULT_PADDING;

            EditorGUI.BeginChangeCheck();
            includeProperty.boolValue = EditorGUI.ToggleLeft(workRect, GetElementHeaderLabel(), includeProperty.boolValue, labelStyle);

            if (EditorGUI.EndChangeCheck())
            {
                UpdateStatus();
            }


            if (currentElementProperty.isExpanded)
            {
                workRect = new Rect(rect);
                workRect.height = EditorGUIUtility.singleLineHeight;
                workRect.y += EditorGUIUtility.singleLineHeight + SMALL_PADDING; //header
                workRect.xMin += DEFAULT_PADDING * 3;
                fieldRect = new Rect(workRect);
                fieldRect.xMin += EditorGUIUtility.labelWidth;

                EditorGUI.PrefixLabel(workRect, new GUIContent(nameProperty.displayName), labelStyle);
                nameProperty.stringValue = EditorGUI.TextField(fieldRect, GUIContent.none, nameProperty.stringValue);

                workRect.y += EditorGUIUtility.singleLineHeight + SMALL_PADDING;
                fieldRect.y += EditorGUIUtility.singleLineHeight + SMALL_PADDING;
                EditorGUI.PrefixLabel(workRect, new GUIContent(directoryProperty.displayName), labelStyle);
                directoryProperty.stringValue = EditorGUI.TextField(fieldRect, GUIContent.none, directoryProperty.stringValue);

                workRect.y += EditorGUIUtility.singleLineHeight + SMALL_PADDING;
                fieldRect.y += EditorGUIUtility.singleLineHeight + SMALL_PADDING;
                EditorGUI.PrefixLabel(workRect, new GUIContent(statusProperty.displayName), labelStyle);
                statusProperty.stringValue = EditorGUI.TextField(fieldRect, GUIContent.none, statusProperty.stringValue);

                workRect.y += EditorGUIUtility.singleLineHeight + SMALL_PADDING;
                fieldRect.y += EditorGUIUtility.singleLineHeight + SMALL_PADDING;
                EditorGUI.PrefixLabel(workRect, new GUIContent(includeProperty.displayName), labelStyle);
                EditorGUI.BeginChangeCheck();
                includeProperty.boolValue = EditorGUI.Toggle(fieldRect, GUIContent.none, includeProperty.boolValue);

                if (EditorGUI.EndChangeCheck())
                {
                    UpdateStatus();
                }
            }
        }

        private string GetElementHeaderLabel()
        {
            return SEPARATOR + statusProperty.stringValue + SEPARATOR + nameProperty.stringValue;
        }

        private float ElementHeightCallback(int index)
        {
            if (coreSettingsProperty.GetArrayElementAtIndex(index).isExpanded)
            {
                return EXPANDED_ELEMENT_HEIGHT;
            }
            else
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        private void DrawElementBackgroundCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if(Event.current.type == EventType.Repaint)
            {
                if(index == -1)
                {
                    return;
                }

                OpenElement(index);

                if (statusProperty.stringValue.Equals(TO_DELETE_STATUS))
                {
                    yellowStyle.Draw(rect, false, false, true, false);
                }
                else if (statusProperty.stringValue.Equals(MISSING_STATUS))
                {
                    redStyle.Draw(rect, false, false, true, false);
                }
                else
                {
                    greenStyle.Draw(rect, false, false, true, false);
                }
            }
        }

        private void OnAddCallback(ReorderableList list)
        {
            coreSettingsProperty.arraySize++;
            OpenElement(coreSettingsProperty.arraySize - 1);
            coreSettingsProperty.GetArrayElementAtIndex(coreSettingsProperty.arraySize - 1).ClearProperty();
            coreSettingsProperty.GetArrayElementAtIndex(coreSettingsProperty.arraySize - 1).isExpanded = true;
            includeProperty.boolValue = true;
        }

        
        private void OpenElement(int index)
        {
            currentElementProperty = coreSettingsProperty.GetArrayElementAtIndex(index);
            nameProperty = currentElementProperty.FindPropertyRelative(NAME_PROPERTY_PATH);
            directoryProperty = currentElementProperty.FindPropertyRelative(DIRECTORY_PROPERTY_PATH);
            statusProperty = currentElementProperty.FindPropertyRelative(STATUS_PROPERTY_PATH);
            includeProperty = currentElementProperty.FindPropertyRelative(INCLUDE_PROPERTY_PATH);
        }


        public override void OnInspectorGUI()
        {
            InitStyles();
            list.DoLayoutList();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(UPDATE_STATUS_LABEL, EditorStylesExtended.button_02_large))
            {
                for (int i = 0; i < coreSettingsProperty.arraySize; i++)
                {
                    OpenElement(i);
                    UpdateStatus();
                }
            }

            if (GUILayout.Button(ORGANIZE_LABEL, EditorStylesExtended.button_03_large))
            {
                for (int i = 0; i < coreSettingsProperty.arraySize; i++)
                {
                    OpenElement(i);

                    if (statusProperty.stringValue.Equals(TO_DELETE_STATUS))
                    {
                        RemoveFolders();
                        UpdateStatus();
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateStatus()
        {
            string globalDirectory = Application.dataPath + PATH_SEPARATOR + directoryProperty.stringValue;
            bool exists = Directory.Exists(globalDirectory);

            if (includeProperty.boolValue && exists)
            {
                statusProperty.stringValue = EXISTS_STATUS;
            }
            else if ((!includeProperty.boolValue) && exists)
            {
                statusProperty.stringValue = TO_DELETE_STATUS;
            }
            else if ((!includeProperty.boolValue) && (!exists))
            {
                statusProperty.stringValue = REMOVED_STATUS;
            }
            else
            {
                statusProperty.stringValue = MISSING_STATUS;
            }
        }

        private void RemoveFolders()
        {
            string globalDirectory = Application.dataPath + PATH_SEPARATOR + directoryProperty.stringValue;
            if (Directory.Exists(globalDirectory))
            {
                FileUtil.DeleteFileOrDirectory(globalDirectory);
                FileUtil.DeleteFileOrDirectory(globalDirectory + META_FILE_SUFFIX);
                AssetDatabase.Refresh();
            }
        }
    }
}
