#pragma warning disable 649

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace Watermelon
{
    public abstract class LevelEditorBase : WatermelonWindow
    {
        public static EditorWindow window;
        public static readonly int DEFAULT_WINDOW_MIN_SIZE = 200;
        public static readonly string DEFAULT_LEVEL_EDITOR_TITLE = "Level Editor";
        protected static string DEFAULT_LEVEL_FOLDER_NAME = "Levels";
        private static LevelEditorBase instance;


        protected const string PATH_SEPARATOR = "/";
        private const string ASSETS = "Assets";
        private const string LEVEL_DATABASE_ASSET_FULL_NAME = "Levels Database.asset";
        private const string LEVELS_DATABASE_NOT_FOUND_MESSAGE = "Levels Database can't be found.";
        private const string CREATE_LEVELS_DATABASE_LABEL = "Create Levels Database";
        public StringBuilder stringBuilder;
        protected UnityEngine.Object levelsDatabase;
        protected SerializedObject levelsDatabaseSerializedObject;
        protected Vector2 contentScrollViewVector;
        [SerializeField] private WindowConfiguration windowConfiguration;

        private bool guiInitialized;
        private static Color defaultGUIColor;

        public static LevelEditorBase Instance { get => instance; }
        public string LEVELS_FOLDER_PATH { get => LEVELS_DATABASE_FOLDER_PATH + PATH_SEPARATOR + LEVELS_FOLDER_NAME; }
        protected virtual string LEVELS_FOLDER_NAME { get => "Levels"; }
        protected virtual string LEVELS_DATABASE_FOLDER_PATH { get => "Assets/Project Data/Content/LevelSystem"; }
        protected Color DefaultGUIColor { get => defaultGUIColor; }

        public static float SINGLE_LINE_HEIGHT { get => EditorGUIUtility.singleLineHeight; }
        public static float LABEL_WIDTH{ get => EditorGUIUtility.labelWidth; set => EditorGUIUtility.labelWidth = value; }

        [MenuItem("Tools/Level Editor")]
        static void ShowWindow()
        {
            System.Type childType = GetChildType();

            if (childType == null)
            {
                Debug.LogError("Child class of LevelEditorBase not found.");
                return;
            }

            window = EditorWindow.GetWindow(childType);
            window.titleContent = new GUIContent(DEFAULT_LEVEL_EDITOR_TITLE);
            window.minSize = new Vector2(DEFAULT_WINDOW_MIN_SIZE, DEFAULT_WINDOW_MIN_SIZE);
            window.Show();
        }

        static System.Type GetChildType()
        {
            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (System.Type classType in assembly.GetTypes())
                {
                    if (classType.IsSubclassOf(typeof(LevelEditorBase)))
                    {
                        return classType;
                    }
                }
            }

            return null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            //init variables
            stringBuilder = new StringBuilder();
            defaultGUIColor = GUI.color;
            guiInitialized = false;
            instance = this;

            CreateFolderIfNotExist(LEVELS_DATABASE_FOLDER_PATH);
            CreateFolderIfNotExist(LEVELS_FOLDER_PATH);
            levelsDatabase = EditorUtils.GetAsset(GetLevelsDatabaseType());

            if (levelsDatabase != null)
            {
                levelsDatabaseSerializedObject = new SerializedObject(levelsDatabase);
                ReadLevelDatabaseFields();
                InitialiseVariables();
                AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            }
        }

        protected abstract WindowConfiguration SetUpWindowConfiguration(WindowConfiguration.Builder builder);
        protected abstract System.Type GetLevelsDatabaseType();
        public abstract System.Type GetLevelType();
        protected abstract void ReadLevelDatabaseFields();
        protected abstract void InitialiseVariables();

        private void ApplyWindowConfiguration()
        {
            if (windowConfiguration == null)
            {
                //Debug.Log("windowConfiguration == null");
                return;
            }

            if (window == null)
            {
                //Debug.Log("window == null");
                return;
            }

            window.titleContent = new GUIContent(windowConfiguration.WindowTitle);

            if (windowConfiguration.RestrictWindowMinSize)
            {
                window.minSize = windowConfiguration.WindowMinSize;
            }

            if (windowConfiguration.RestrictWindowMaxSize)
            {
                window.maxSize = windowConfiguration.WindowMaxSize;
            }
        }


        #region Scene level editor bugs fix
        // commented because we recompile enum in editor
        // fixing bug that occurs when  update script or reimport script with active level editor window
        public virtual void OnBeforeAssemblyReload()
        {
            if ((windowConfiguration != null) && (!windowConfiguration.KeepWindowOpenOnScriptReload) && (window != null))
            {
                window.Close();
            }
        }

        // fixing bug that occurs when run game with active level editor window
        public virtual bool WindowClosedInPlaymode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (window != null)
                {
                    window.Close();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        public void OnGUI()
        {
            if (!guiInitialized)
            {
                InitializeGUI();
                guiInitialized = true;
            }

            if (WindowClosedInPlaymode())
            {
                return;
            }

            if (levelsDatabase == null)
            {
                DrawCreateLevelDatabase();
                return;
            }

            if (windowConfiguration.RestictContentHeight)
            {
                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(windowConfiguration.ContentMaxSize.x), GUILayout.MaxHeight(windowConfiguration.ContentMaxSize.y));
            }
            else
            {
                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(windowConfiguration.ContentMaxSize.x));
            }
            
            contentScrollViewVector = EditorGUILayout.BeginScrollView(contentScrollViewVector);
            DrawContent();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            AfterOnGUI();
            levelsDatabaseSerializedObject.ApplyModifiedProperties();
        }

        

        private void InitializeGUI()
        {
            ForceInitStyles();
            EditorStylesExtended.InitializeStyles();
            windowConfiguration = SetUpWindowConfiguration(new WindowConfiguration.Builder());
            ApplyWindowConfiguration();
        }

        private void DrawCreateLevelDatabase()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox(LEVELS_DATABASE_NOT_FOUND_MESSAGE, MessageType.Error, true);

            if (GUILayout.Button(CREATE_LEVELS_DATABASE_LABEL))
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance(GetLevelsDatabaseType()), LEVELS_DATABASE_FOLDER_PATH + PATH_SEPARATOR + LEVEL_DATABASE_ASSET_FULL_NAME);
                AssetDatabase.Refresh();
                OnEnable();
            }

            EditorGUILayout.EndVertical();
        }

        public abstract void OpenLevel(UnityEngine.Object levelObject, int index);
        public abstract string GetLevelLabel(UnityEngine.Object levelObject, int index);
        public abstract void ClearLevel(UnityEngine.Object levelObject);

        public virtual void LogErrorsForGlobalValidation(UnityEngine.Object levelObject, int index)
        {
            Debug.LogError("LogErrorsForGlobalValidation method not overriden.");
        }

        protected abstract void DrawContent();

        protected virtual void AfterOnGUI()
        {
        }

        #region useful functions

        public static string GetProjectPath()
        {
            return Application.dataPath.Replace(ASSETS, string.Empty);
        }

        public static void CreateFolderIfNotExist(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        public static void OpenScene(string scenePath)
        {
            EditorSceneManager.OpenScene(scenePath);
        }


        public static void DrawColorRect(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = defaultGUIColor;
        }

        public static bool IsPropertyChanged(SerializedProperty serializedProperty, GUIContent content)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedProperty, content);
            return EditorGUI.EndChangeCheck();
        }

        public static bool IsPropertyChanged(SerializedProperty serializedProperty)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedProperty);
            return EditorGUI.EndChangeCheck();
        }

        public static void BeginDisabledGroup(bool value)
        {
            EditorGUI.BeginDisabledGroup(value);
        }

        public static void EndDisabledGroup()
        {
            EditorGUI.EndDisabledGroup();
        }

        public static float GetLabelWidth(string label)
        {
            return GUI.skin.label.CalcSize(new GUIContent(label)).x;
        }

        #endregion

        protected abstract class LevelRepresentationBase
        {
            protected const string NUMBER = "#";
            protected const string SEPARATOR = " | ";
            protected const string NULL_FILE = "[Null file]";
            private const string INCORRECT = "[Incorrect]";

            protected SerializedObject serializedLevelObject;
            protected UnityEngine.Object levelObject;
            private bool nullLevel;
            public List<string> errorLabels;


            public bool NullLevel { get => nullLevel; }

            protected virtual bool LEVEL_CHECK_ENABLED  { get => false; }
            public bool IsLevelCorrect { get => errorLabels.Count == 0; }

            public LevelRepresentationBase(UnityEngine.Object levelObject)
            {
                this.levelObject = levelObject;
                nullLevel = (levelObject == null);
                errorLabels = new List<string>();

                if (!nullLevel)
                {
                    serializedLevelObject = new SerializedObject(levelObject);
                    ReadFields();
                }
            }

            protected abstract void ReadFields();

            public abstract void Clear();

            public void ApplyChanges()
            {
                if (!NullLevel)
                {
                    serializedLevelObject.ApplyModifiedProperties();
                }
            }

            public virtual string GetLevelLabel(int index, StringBuilder stringBuilder)
            {
                stringBuilder.Clear();
                stringBuilder.Append(NUMBER);
                stringBuilder.Append(index + 1);
                stringBuilder.Append(SEPARATOR);

                if (NullLevel)
                {
                    stringBuilder.Append(NULL_FILE);
                }
                else
                {
                    stringBuilder.Append(levelObject.name);

                    if (LEVEL_CHECK_ENABLED)
                    {
                        ValidateLevel();

                        if (!IsLevelCorrect)
                        {
                            stringBuilder.Append(SEPARATOR);
                            stringBuilder.Append(INCORRECT);
                        }
                    }
                }

                return stringBuilder.ToString();
            }

            public virtual void ValidateLevel()
            {
            }
        }
    }
}

// -----------------
// Level editor base v 1.4
// -----------------

// Changelog
// v 1.4
// • Make restraining window content height into separate setting
// • Make all useful methods static
// • Added callbacks to LevelsHandler
// • Fixed IndexChangeWindow in LevelsHandler
// • Added more useful stuff to LevelEditorBase
// v 1.3
// • Fixed styles initialization in our custom list
// • Reordered some methods
// v 1.2
// • Replaced Reordable list with our CustomList
// v 1.1
// • Added global validation support
// • Changed validation
// v 1 basic version works