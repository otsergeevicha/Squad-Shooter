#pragma warning disable 649

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System.Text;
using System;
using Watermelon.List;

namespace Watermelon
{
    public class LevelsHandler
    {

        //strings
        private const string LEVEL_PREFIX = "Level_";
        private const string ASSET_SUFFIX = ".asset";
        private const string OLD_PREFIX = "old_";
        private const string REMOVE_LEVEL = "Are you sure you want to remove ";
        private const string BRACKET = "\"";
        private const string QUESTION_MARK = "?";
        private const string REMOVING_LEVEL_TITLE = "Removing level";
        private const string YES = "Yes";
        private const string CANCEL = "Cancel";
        private const string FORMAT_TYPE = "000";
        private const string PATH_SEPARATOR = "/";
        private const string DEFAULT_LEVEL_LIST_HEADER = "Levels amount: ";
        private const string REMOVE_SELECTION = "Remove selection";
        private const string RENAME_LEVELS_LABEL = "Rename Levels";
        private const string GLOBAL_VALIDATION_LABEL = "Global Validation";
        private const string REMOVE_ELEMENT_CALLBACK = "Remove element";
        private const string ON_ENABLE_OVERRIDEN_ERROR = "LevelEditorBase.Instance == null. OnEnable() overriden without base.OnEnable() call";
        private const string SET_POSITION_LABEL = "Set position";
        private const string INDEX_CHANGE_WINDOW = "Index change window";
        private readonly Vector2 INDEX_CHANGE_WINDOW_SIZE = new Vector2(300, 64);

        #region delegates

        public delegate void AddElementCallbackDelegate();
        public delegate void RemoveElementCallbackDelegate();
        public delegate void DisplayContextMenuCallbackDelegate(GenericMenu genericMenu);
        public delegate void OnClearSelectionCallbackDelegate();
        public delegate void OnRenameAllCallbackDelegate();

        public AddElementCallbackDelegate addElementCallback;
        public RemoveElementCallbackDelegate removeElementCallback;
        public DisplayContextMenuCallbackDelegate displayContextMenuCallback;
        public OnClearSelectionCallbackDelegate onClearSelectionCallback;
        public OnRenameAllCallbackDelegate onRenameAllCallback;
        #endregion

        private List<string> levelLabels;
        private SerializedObject levelsDatabaseSerializedObject;
        private SerializedProperty levelsSerializedProperty;
        private UnityEngine.Object selectedLevelObjectReferenceValue;
        private CustomList customList;

        public int SelectedLevelIndex { get => customList.SelectedIndex; }
        public SerializedProperty SelectedLevelProperty { get => levelsSerializedProperty.GetArrayElementAtIndex(SelectedLevelIndex); set => levelsSerializedProperty.GetArrayElementAtIndex(SelectedLevelIndex).objectReferenceValue = value.objectReferenceValue; }


        public LevelsHandler(SerializedObject levelsDatabaseSerializedObject, SerializedProperty levelsSerializedProperty)
        {
            this.levelsDatabaseSerializedObject = levelsDatabaseSerializedObject;
            this.levelsSerializedProperty = levelsSerializedProperty;
            this.levelLabels = new List<string>();
            this.selectedLevelObjectReferenceValue = null;

            SetLevelLabels();
            SetCustomList();
        }

        #region Reordable list

        private void SetCustomList()
        {
            //customList = new CustomList(levelsDatabaseSerializedObject, levelsSerializedProperty);
            customList = new CustomList(levelsDatabaseSerializedObject, levelsSerializedProperty,  GetLabel);

            customList.getHeaderLabelCallback += GetHeaderCallback;
            customList.selectionChangedCallback += SelectionChangedCallback;
            customList.listReorderedCallback += ListReorderedCallback;
            customList.addElementCallback += AddElementCallback;
            customList.removeElementCallback += RemoveElementCallback;
            customList.displayContextMenuCallback += DisplayContextMenuCallback;
        }

        private string GetLabel(SerializedProperty elementProperty, int elementIndex)
        {
            return levelLabels[elementIndex];
        }

        private void DisplayContextMenuCallback()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent(SET_POSITION_LABEL), false, OpenSetIndexModalWindow);
            genericMenu.AddItem(new GUIContent(REMOVE_SELECTION), false, ClearSelection);
            genericMenu.AddItem(new GUIContent(REMOVE_ELEMENT_CALLBACK), false, RemoveElementCallback);
            displayContextMenuCallback?.Invoke(genericMenu);
            genericMenu.ShowAsContext();
        }

        private void RemoveElementCallback()
        {
            DeleteLevel(SelectedLevelIndex);
            removeElementCallback?.Invoke();
        }

        private void AddElementCallback()
        {
            AddLevel();
            addElementCallback?.Invoke();
        }

        private void ListReorderedCallback()
        {
            SetLevelLabels();
        }

        private void SelectionChangedCallback()
        {
            OpenLevel(SelectedLevelIndex);
        }

        private string GetHeaderCallback()
        {
            return DEFAULT_LEVEL_LIST_HEADER + levelsSerializedProperty.arraySize;
        }

        public void ClearSelection()
        {
            customList.SelectedIndex = -1;
            onClearSelectionCallback?.Invoke();
        }

        public void UpdateCurrentLevelLabel(string label)
        {
            if (SelectedLevelIndex != -1)
            {
                levelLabels[SelectedLevelIndex] = label;
            }
        }

        public void DisplayReordableList()
        {
            customList.Display();
        }

        #endregion

        public void OpenLevel(int index)
        {
            if (LevelEditorBase.Instance == null)
            {
                Debug.LogError(ON_ENABLE_OVERRIDEN_ERROR);
            }
            else
            {
                if(selectedLevelObjectReferenceValue == levelsSerializedProperty.GetArrayElementAtIndex(index).objectReferenceValue)
                {
                    return;
                }

                LevelEditorBase.Instance.OpenLevel(levelsSerializedProperty.GetArrayElementAtIndex(index).objectReferenceValue, index);
            }
        }

        public void ReopenLevel()
        {
            selectedLevelObjectReferenceValue = levelsSerializedProperty.GetArrayElementAtIndex(SelectedLevelIndex).objectReferenceValue;
            OpenLevel(SelectedLevelIndex);
        }


        public void AddLevel()
        {
            if (LevelEditorBase.Instance == null)
            {
                Debug.LogError(ON_ENABLE_OVERRIDEN_ERROR);
                return;
            }

            levelsSerializedProperty.arraySize++;
            int newLevelIndex = levelsSerializedProperty.arraySize - 1;
            UnityEngine.Object level = ScriptableObject.CreateInstance(LevelEditorBase.Instance.GetLevelType());

            AssetDatabase.CreateAsset(level, GetRelativeLevelAssetPathByNumber(GetLevelNumber(levelsSerializedProperty.arraySize)));
            LevelEditorBase.Instance.ClearLevel(level);
            levelLabels.Add(LevelEditorBase.Instance.GetLevelLabel(level, newLevelIndex));
            levelsSerializedProperty.GetArrayElementAtIndex(newLevelIndex).objectReferenceValue = level;
            AssetDatabase.SaveAssets();


            customList.SelectedIndex = newLevelIndex;

            OpenLevel(newLevelIndex);
        }



        private string GetLevelNumber(int arraySize)
        {
            int levelNumber = arraySize - 1;

            do
            {
                levelNumber++;
            }
            while (File.Exists(LevelEditorBase.GetProjectPath() + GetRelativeLevelAssetPathByNumber(FormatNumber(levelNumber))));

            return FormatNumber(levelNumber);
        }

        private static string GetRelativeLevelAssetPathByNumber(string levelNumber)
        {
            return LevelEditorBase.Instance.LEVELS_FOLDER_PATH + PATH_SEPARATOR + LEVEL_PREFIX + levelNumber + ASSET_SUFFIX;
        }

        private static string FormatNumber(int maxIndex)
        {
            return maxIndex.ToString(FORMAT_TYPE);
        }


        public void DeleteLevel(int levelIndex)
        {
            StringBuilder stringBuilder = LevelEditorBase.Instance.stringBuilder;
            stringBuilder.Clear();
            stringBuilder.Append(REMOVE_LEVEL);
            stringBuilder.Append(BRACKET);
            stringBuilder.Append(levelLabels[levelIndex]);
            stringBuilder.Append(BRACKET);
            stringBuilder.Append(QUESTION_MARK);

            if (EditorUtility.DisplayDialog(REMOVING_LEVEL_TITLE, stringBuilder.ToString(), YES, CANCEL))
            {
                HandleDeleteLevel(levelIndex);
            }
        }

        private void HandleDeleteLevel(int levelIndex)
        {
            UnityEngine.Object tempObject = levelsSerializedProperty.GetArrayElementAtIndex(levelIndex).objectReferenceValue;

            if (tempObject != null)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(tempObject));
                AssetDatabase.Refresh();
            }

            if (customList != null)
            {
                customList.SelectedIndex = -1;
            }

            levelLabels.RemoveAt(levelIndex);
            levelsSerializedProperty.GetArrayElementAtIndex(levelIndex).objectReferenceValue = null;
            levelsSerializedProperty.DeleteArrayElementAtIndex(levelIndex);
        }

        public void SetLevelLabels()
        {
            levelLabels.Clear();

            if (LevelEditorBase.Instance == null)
            {
                Debug.LogError(ON_ENABLE_OVERRIDEN_ERROR);
                return;
            }

            for (int i = 0; i < levelsSerializedProperty.arraySize; i++)
            {
                levelLabels.Add(LevelEditorBase.Instance.GetLevelLabel(levelsSerializedProperty.GetArrayElementAtIndex(i).objectReferenceValue, i));
            }
        }

        public void RenameLevels()
        {
            List<int> indexesOfIncorrectLevels = new List<int>();

            for (int i = 0; i < levelsSerializedProperty.arraySize; i++)
            {
                if (!levelsSerializedProperty.GetArrayElementAtIndex(i).objectReferenceValue.name.Equals(LEVEL_PREFIX + FormatNumber(i + 1)))
                {
                    indexesOfIncorrectLevels.Add(i);
                }
            }

            string name;

            foreach (int index in indexesOfIncorrectLevels)
            {
                if (levelsSerializedProperty.GetArrayElementAtIndex(index).objectReferenceValue != null)
                {
                    name = levelsSerializedProperty.GetArrayElementAtIndex(index).objectReferenceValue.name;
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(levelsSerializedProperty.GetArrayElementAtIndex(index).objectReferenceValue), OLD_PREFIX + name);
                }
            }

            foreach (int index in indexesOfIncorrectLevels)
            {
                if (levelsSerializedProperty.GetArrayElementAtIndex(index).objectReferenceValue != null)
                {
                    name = levelsSerializedProperty.GetArrayElementAtIndex(index).objectReferenceValue.name;
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(levelsSerializedProperty.GetArrayElementAtIndex(index).objectReferenceValue), LEVEL_PREFIX + FormatNumber(index + 1));
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            SetLevelLabels();
            onRenameAllCallback?.Invoke();
        }

        #region draw buttons

        public void DrawRenameLevelsButton()
        {
            if (GUILayout.Button(RENAME_LEVELS_LABEL, EditorStylesExtended.button_01))
            {
                RenameLevels();
            }
        }

        public void DrawClearSelectionButton()
        {
            if (GUILayout.Button(REMOVE_SELECTION, EditorStylesExtended.button_01))
            {
                ClearSelection();
            }
        }

        public void DrawGlobalValidationButton()
        {
            if (GUILayout.Button(GLOBAL_VALIDATION_LABEL, EditorStylesExtended.button_01))
            {
                Debug.Log("Global validation log begins");

                for (int i = 0; i < levelsSerializedProperty.arraySize; i++)
                {
                    LevelEditorBase.Instance.LogErrorsForGlobalValidation(levelsSerializedProperty.GetArrayElementAtIndex(i).objectReferenceValue, i);
                }

                Debug.Log("Global validation log ends");
            }
        }

        #endregion

        #region Set index modal window

        private void OpenSetIndexModalWindow()
        {
            SetIndexModalWindow window = ScriptableObject.CreateInstance<SetIndexModalWindow>();
            window.SetData(SelectedLevelIndex, levelsSerializedProperty.arraySize, this);
            window.minSize = INDEX_CHANGE_WINDOW_SIZE;
            window.maxSize = INDEX_CHANGE_WINDOW_SIZE;
            window.titleContent = new GUIContent(INDEX_CHANGE_WINDOW);

            window.ShowModal();
        }

        private void ModalWindowProcessChange(int originalIndex, int newIndex)
        {
            levelsSerializedProperty.MoveArrayElement(originalIndex, newIndex);
            levelsDatabaseSerializedObject.ApplyModifiedProperties();
            customList.SelectedIndex = newIndex;
            ListReorderedCallback();
        }

        private class SetIndexModalWindow : EditorWindow
        {
            private const string INT_FIELD_LABEL = "target element new #";
            private const string CANCEL_BUTTON_LABEL = "Cancel";
            private const string CHANGE_BUTTON_LABEL = "Change";
            private const string DEFAULT_LABEL = "target element #";
            public int elementOrinialIndex;
            public int arraySize;
            public LevelsHandler levelsHandler;
            private string label;
            private int newPositionNumber;

            public void SetData(int elementOrinialIndex,int arraySize, LevelsHandler levelsHandler)
            {
                this.elementOrinialIndex = elementOrinialIndex;
                this.levelsHandler = levelsHandler;
                this.arraySize = arraySize;
                label = DEFAULT_LABEL + (elementOrinialIndex + 1);
                newPositionNumber = elementOrinialIndex + 1;
            }

            void OnGUI()
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(label);
                newPositionNumber = EditorGUILayout.IntField(INT_FIELD_LABEL, newPositionNumber);
                newPositionNumber = Mathf.Clamp(newPositionNumber, 1, arraySize);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(CANCEL_BUTTON_LABEL))
                {
                    this.Close();
                }

                if (GUILayout.Button(CHANGE_BUTTON_LABEL))
                {
                    levelsHandler.ModalWindowProcessChange(elementOrinialIndex, newPositionNumber - 1);
                    this.Close();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
        }

        #endregion
    }
}
