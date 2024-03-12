using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEditorInternal;
namespace Watermelon
{
    public class SavePresetsWindow : WatermelonWindow
    {
        private const string PRESET_PREFIX = "savePreset_";
        private const string PRESET_FOLDER_PREFIX = "SavePresets/savePreset_";
        private const string PRESETS_ORDER_FILE = "presetsOrderFile";
        private const string PRESETS_FOLDER_NAME = "SavePresets";

        private const string SAVE_FILE_NAME = "save";
        private const int REMOVE_BUTTON_WIDTH = 20;
        private const int UPDATE_BUTTON_WIDTH = 80;
        private const int ACTIVATE_BUTTON_WIDTH = 80;
        private const int DATE_LABEL_WIDTH = 50;
        private const int DEFAULT_SPACE = 8;

        private static readonly Vector2 WINDOW_SIZE = new Vector2(490, 495);
        private static readonly string WINDOW_TITLE = "Save Presets";

        private static SavePresetsWindow setupWindow;

        private Vector2 scrollView;

        private List<SavePreset> savePresets;
        private string tempPresetName;
        private ReorderableList savePresetsList;
        private Rect workRect;
        private Color color1;
        private Color color2;

        [MenuItem("Tools/Save Presets")]
        [MenuItem("Window/Save Presets")]
        static void ShowWindow()
        {
            SavePresetsWindow tempWindow = (SavePresetsWindow)GetWindow(typeof(SavePresetsWindow), false, WINDOW_TITLE);
            tempWindow.minSize = WINDOW_SIZE;
            tempWindow.titleContent = new GUIContent(WINDOW_TITLE, EditorStylesExtended.GetTexture("icon_title", EditorStylesExtended.IconColor));

            setupWindow = tempWindow;

            EditorStylesExtended.InitializeStyles();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            setupWindow = this;

            savePresets = new List<SavePreset>();
            List<SavePreset> unsortedSavePresets = new List<SavePreset>();
            string directoryPath = Path.Combine(Application.persistentDataPath, PRESETS_FOLDER_NAME);


            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string[] fileEntries = Directory.GetFiles(Application.persistentDataPath);
            string[] order = new string[0];
            string name;

            //move files into new folder
            for (int i = 0; i < fileEntries.Length; i++)
            {
                if (fileEntries[i].Contains(PRESET_PREFIX))
                {
                    name = Path.GetFileName(fileEntries[i]).Replace(PRESET_PREFIX, "");
                    File.Move(fileEntries[i], GetPresetPath(name));

                }

                if (fileEntries[i].Contains(PRESETS_ORDER_FILE))
                {
                    File.Move(fileEntries[i], GetOrderFilePath());
                }
            }

            fileEntries = Directory.GetFiles(directoryPath);


            //load
            for (int i = 0; i < fileEntries.Length; i++)
            {
                if (fileEntries[i].Contains(PRESET_PREFIX))
                {
                    unsortedSavePresets.Add(new SavePreset(File.GetCreationTimeUtc(fileEntries[i]), fileEntries[i]));
                }

                if (fileEntries[i].Contains(PRESETS_ORDER_FILE))
                {
                    order = File.ReadAllLines(fileEntries[i]);
                }
            }

            for (int i = 0; i < order.Length; i++)
            {
                for (int j = 0; j < unsortedSavePresets.Count; j++)
                {
                    if (order[i].Equals(unsortedSavePresets[j].name))
                    {
                        savePresets.Add(unsortedSavePresets[j]);
                        unsortedSavePresets.RemoveAt(j);
                        break;
                    }
                }
            }

            savePresets.AddRange(unsortedSavePresets);



            ForceInitStyles();

            savePresetsList = new ReorderableList(savePresets, typeof(SavePreset), true, false, false, true);
            savePresetsList.elementHeight = 26;
            savePresetsList.drawElementCallback = DrawElement;
            savePresetsList.onRemoveCallback = RemoveCallback;
            savePresetsList.onChangedCallback = ListChangedCallback;
            workRect = new Rect();
        }

        

        private void RemoveCallback(ReorderableList list)
        {
            if (EditorUtility.DisplayDialog("This preset will be removed!", "Are you sure?", "Remove", "Cancel"))
            {
                RemovePreset(savePresetsList.index);
                savePresetsList.ClearSelection();
            }
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            workRect.Set(rect.x + rect.width, rect.y + 4, 0, 18);

            workRect.x -= UPDATE_BUTTON_WIDTH + DEFAULT_SPACE;
            workRect.width = UPDATE_BUTTON_WIDTH;

            if (GUI.Button(workRect, "Update"))
            {
                if (EditorUtility.DisplayDialog("This preset will rewrited!", "Are you sure?", "Rewrite", "Cancel"))
                {
                    UpdatePreset(savePresets[index]);
                }
            }

            workRect.x -= ACTIVATE_BUTTON_WIDTH + DEFAULT_SPACE;
            workRect.width = ACTIVATE_BUTTON_WIDTH;

            if (GUI.Button(workRect, "Activate", EditorStylesExtended.button_03))
            {
                ActivatePreset(savePresets[index].name);
            }

            workRect.x -= DATE_LABEL_WIDTH + DEFAULT_SPACE;
            workRect.width = DATE_LABEL_WIDTH;

            GUI.Label(workRect, savePresets[index].creationDate.ToString("dd.MM"));

            workRect.x -= DEFAULT_SPACE;
            workRect.width = workRect.x - rect.x;
            workRect.x = rect.x;

            GUI.Label(workRect, savePresets[index].name);
        }

        private void ListChangedCallback(ReorderableList list)
        {
            SaveListOrder();
        }

        private void OnDisable()
        {

        }

        protected override void Styles()
        {

        }

        private void OnGUI()
        {
            InitStyles();

            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            EditorGUILayoutCustom.Header("PRESETS");

            scrollView = EditorGUILayoutCustom.BeginScrollView(scrollView);
            savePresetsList.DoLayoutList();
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal(EditorStylesExtended.editorSkin.box);
            tempPresetName = EditorGUILayout.TextField(tempPresetName);

            if (GUILayout.Button("Add"))
            {
                AddNewPreset(tempPresetName);

                tempPresetName = "";

                GUI.FocusControl(null);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void ActivatePreset(string name)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("[Save Presets]: Preset can't be activated in playmode!");

                return;
            }

            if (EditorApplication.isCompiling)
            {
                Debug.LogError("[Save Presets]: Preset can't be activated during compiling!");

                return;
            }

            string presetPath = GetPresetPath(name);

            if (!File.Exists(presetPath))
            {
                Debug.LogError(string.Format("[Save Presets]: Preset with name {0} doesn’t  exist!", name));

                return;
            }

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Game")
            {
                EditorSceneManager.OpenScene(@"Assets\Project Data\Game\Scenes\Game.unity");
            }

            // Replace current save file with the preset
            File.Copy(presetPath, GetSavePath(), true);

            // Start game
            EditorApplication.isPlaying = true;
        }

        private void RemovePreset(int index)
        {
            if (savePresets.IsInRange(index))
            {
                // Delete preset file
                File.Delete(savePresets[index].path);

                // Remove preset from the list
                savePresets.RemoveAt(index);
            }
        }

        private void UpdatePreset(SavePreset savePreset)
        {
            if (EditorApplication.isPlaying)
                SaveController.ForceSave();

            string savePath = GetSavePath();
            if (!File.Exists(savePath))
            {
                Debug.LogError("[Save Presets]: Save file doesn’t  exist!");

                return;
            }

            if (savePreset != null)
            {
                savePreset.creationDate = DateTime.Now;

                if (EditorApplication.isPlaying)
                {
                    File.SetCreationTime(savePreset.path, DateTime.Now);
                    SaveController.PresetsSave(PRESET_FOLDER_PREFIX + savePreset.name);
                }
                else
                {
                    File.Copy(savePath, savePreset.path, true);
                    File.SetCreationTime(savePreset.path, DateTime.Now);
                }

            }

        }

        private void OnDestroy()
        {
            SaveListOrder();
        }

        private void SaveListOrder()
        {
            string[] order = new string[savePresets.Count];

            for (int i = 0; i < order.Length; i++)
            {
                order[i] = savePresets[i].name;
            }

            File.WriteAllLines(GetOrderFilePath(), order);
        }

        private void AddNewPreset(string name)
        {
            if (EditorApplication.isPlaying)
                SaveController.ForceSave();

            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("[Save Presets]: Preset name can't be empty!");

                return;
            }

            if (savePresets.FindIndex(x => x.name == name) != -1)
            {
                Debug.LogError(string.Format("[Save Presets]: Preset with name {0} already exist!", name));

                return;
            }

            string savePath = GetSavePath();

            if (!File.Exists(savePath))
            {
                Debug.LogError("[Save Presets]: Save file doesn’t exist!");

                return;
            }

            string presetPath = GetPresetPath(name);


            if (EditorApplication.isPlaying)
            {
                SaveController.PresetsSave(PRESET_FOLDER_PREFIX + name);
            }
            else
            {
                File.Copy(savePath, presetPath, true);
            }

            savePresets.Add(new SavePreset(DateTime.Now, presetPath));
        }

        private string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        }

        private string GetPresetPath(string name)
        {
            return Path.Combine(Application.persistentDataPath, PRESETS_FOLDER_NAME, PRESET_PREFIX + name);
        }

        private string GetOldPresetPath(string name)
        {
            return Path.Combine(Application.persistentDataPath, PRESET_PREFIX + name);
        }

        private string GetOrderFilePath()
        {
            return Path.Combine(Application.persistentDataPath, PRESETS_FOLDER_NAME, PRESETS_ORDER_FILE);
        }

        private class SavePreset
        {
            public string name;
            public DateTime creationDate;
            public string path;

            public SavePreset(DateTime lastModifiedDate, string path)
            {
                creationDate = lastModifiedDate;
                this.path = path;
                name = Path.GetFileName(path).Replace(PRESET_PREFIX, "");
            }
        }
    }
}