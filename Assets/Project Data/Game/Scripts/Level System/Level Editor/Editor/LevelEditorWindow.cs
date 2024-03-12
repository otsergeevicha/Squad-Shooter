#pragma warning disable 649

using UnityEngine;
using UnityEditor;
using System;
using Watermelon.LevelSystem;
using UnityEditorInternal;
using System.Collections.Generic;

namespace Watermelon.SquadShooter
{
    public class LevelEditorWindow : LevelEditorBase
    {

        //Path variables need to be changed ----------------------------------------
        private const string GAME_SCENE_PATH = "Assets/Project Data/Game/Scenes/Game.unity";
        private const string EDITOR_SCENE_PATH = "Assets/Project Data/Game/Scenes/LevelEditor.unity";
        private static string EDITOR_SCENE_NAME = "LevelEditor";

        //Window configuration
        private const string TITLE = "Level Editor";
        private const float WINDOW_MIN_WIDTH = 600;
        private const float WINDOW_MIN_HEIGHT = 560;
        private const float WINDOW_MAX_WIDTH = 800;
        private const float WINDOW_MAX_HEIGHT = 1200;

        //Level database fields
        private const string WORLDS_PROPERTY_NAME = "worlds";
        private const string OBSTACLE_PRESETS_PROPERTY_NAME = "obstaclesPresets";
        private const string EXIT_POINTS_PROPERTY_NAME = "exitPoints";
        private SerializedProperty worldsSerializedProperty;
        private SerializedProperty obstaclePresetsSerializedProperty;
        private SerializedProperty exitPointsSerializedProperty;

        //TabHandler
        private TabHandler tabHandler;
        private const string LEVELS_TAB_NAME = "Levels";
        private const string ITEMS_TAB_NAME = "Items";

        //sidebar
        private LevelRepresentation selectedLevelRepresentation;
        private const int SIDEBAR_WIDTH = 240;
        private const string OPEN_GAME_SCENE_LABEL = "Open \"Game\" scene";

        private const string REMOVE_SELECTION = "Remove selection";

        //rest of levels tab
        private const string OBJECT_MANAGEMENT = "Object management:";
        private const string CLEAR_SCENE = "Clear scene";
        private const string SAVE = "Save";
        private const string LOAD = "Load";

        private const string ITEM_ASSIGNED = "This buttton spawns item.";
        private const string TEST_LEVEL = "Test level";

        private const float ITEMS_BUTTON_MAX_WIDTH = 120;
        private const float ITEMS_BUTTON_SPACE = 8;
        private const float ITEMS_BUTTON_WIDTH = 80;
        private const float ITEMS_BUTTON_HEIGHT = 80;

        private GUIContent itemContent;
        private Vector2 levelItemsScrollVector;
        private float itemPosX;
        private float itemPosY;
        private Rect itemsRect;
        private Rect selectedLevelFieldRect;
        private Rect itemRect;
        private int itemsPerRow;
        private int rowCount;

        // NEW STUFF

        bool isDatabaseLoaded;
        int selectedWorldIndex;
        SerializedProperty selectedWorldSerializedProperty;
        ReorderableList levelsList;
        SerializedObject worldSerializedObject;
        private bool isWorldLoaded;
        private GUIContent worldNumber;
        private GUIContent presetType;
        private GUIContent previewSprite;
        private const string LEVELS_PROPERTY_PATH = "levels";
        private const string OBSTACLES_PRESET_TYPE_PROPERTY_PATH = "obstaclesPresetType";
        private const string PREVIEW_SPRITE_PROPERTY_PATH = "previewSprite";
        private const string MENU_ITEM_PATH = "Edit/Play";
        SerializedProperty levelsProperty;
        SerializedProperty previewSpriteProperty;
        SerializedProperty obstaclesPresetTypeProperty;
        SerializedProperty exitPointPrefabProperty;
        CatchedObstaclesPreset selectedPreset;
        CatchedObstaclesPreset[] obstaclePresets;
        CatchedEnemyRefs[] enemies;
        CatchedPrefabRefs[] chests;
        EnvironmentPreset[] environmentPresets;
        string[] toolbarTab = { "Obstacles", "Enemies" };
        int selectedToolbarTab = 0;
        int tempRoomTabIndex;
        GameSettings gameSettings;
        EnemiesDatabase enemiesDatabase;
        EnemyType[] enemyEnumValues;
        GameObject editorGameobject;
        bool createdEditorGameobject;

        protected override string LEVELS_FOLDER_NAME => "Worlds";

        protected override string LEVELS_DATABASE_FOLDER_PATH => "Assets/Project Data/Content/Data/Level System";

        public static void CreateLevelEditorWindow(GameSettings gameSettings, EnemiesDatabase enemiesDatabase)
        {
            window = GetWindow(typeof(LevelEditorWindow));
            window.titleContent = new GUIContent(DEFAULT_LEVEL_EDITOR_TITLE);
            window.minSize = new Vector2(DEFAULT_WINDOW_MIN_SIZE, DEFAULT_WINDOW_MIN_SIZE);
            window.Show();
            ((LevelEditorWindow)window).SetUpDatabases(gameSettings, enemiesDatabase);
        }



        protected override WindowConfiguration SetUpWindowConfiguration(WindowConfiguration.Builder builder)
        {
            builder.KeepWindowOpenOnScriptReload(true);
            builder.SetWindowMinSize(new Vector2(WINDOW_MIN_WIDTH, WINDOW_MIN_HEIGHT));
            builder.SetContentMaxSize(new Vector2(WINDOW_MAX_WIDTH, WINDOW_MAX_HEIGHT));
            builder.SetWindowMaxSize(new Vector2(WINDOW_MAX_WIDTH, WINDOW_MAX_HEIGHT));
            return builder.Build();
        }

        protected override Type GetLevelsDatabaseType()
        {
            return typeof(LevelsDatabase);
        }

        public override Type GetLevelType()
        {
            return typeof(LevelData);
        }

        protected override void ReadLevelDatabaseFields()
        {
            worldsSerializedProperty = levelsDatabaseSerializedObject.FindProperty(WORLDS_PROPERTY_NAME);
            isDatabaseLoaded = true;

        }

        protected override void InitialiseVariables()
        {
            gameSettings = AssetDatabase.LoadAssetAtPath<GameSettings>("Assets/Project Data/Content/Data/Game Settings.asset");
            enemiesDatabase = AssetDatabase.LoadAssetAtPath<EnemiesDatabase>("Assets/Project Data/Content/Data/Enemies/Enemies Database.asset");
            CollectDataFromLevelsSettings();
            CollectDataFromEnemiesDatabase();
            tabHandler = new TabHandler();
            tabHandler.AddTab(new TabHandler.Tab("Obstacles", DisplayObstaclesListSection));
            tabHandler.AddTab(new TabHandler.Tab("Enemies", DisplayObstaclesListSection));
            previewSprite = new GUIContent("Preview Sprite:");
            presetType = new GUIContent("Preset type:");
            selectedWorldIndex = 0;
            OpenWorld();
        }

        public void SetUpDatabases(GameSettings gameSettings, EnemiesDatabase enemiesDatabase)
        {
            this.gameSettings = gameSettings;
            this.enemiesDatabase = enemiesDatabase;
            CollectDataFromLevelsSettings();
            CollectDataFromEnemiesDatabase();
            selectedWorldIndex = 0;
            OpenWorld();
            levelsList.index = 0;
            selectedLevelRepresentation = new LevelRepresentation(levelsProperty.GetArrayElementAtIndex(0));
            LoadLevel();
            selectedLevelRepresentation.selectedRoomindex = 0;
            selectedLevelRepresentation.OpenRoom(0);
            EditorSceneController.Instance.SelectRoom(0);


        }

        private void OnDestroy()
        {
            SaveLevelIfPosssible();

            if (createdEditorGameobject)
            {
                DestroyImmediate(editorGameobject);
            }
        }

        private void DisplayLevelFields()
        {
            EditorGUILayout.PropertyField(selectedLevelRepresentation.levelTypeProperty);
            EditorGUILayout.PropertyField(selectedLevelRepresentation.xpAmountProperty);
            EditorGUILayout.PropertyField(selectedLevelRepresentation.requiredUpgProperty);
            EditorGUILayout.PropertyField(selectedLevelRepresentation.enemiesLevelProperty);
            EditorGUILayout.PropertyField(selectedLevelRepresentation.hasCharacterSuggestionProperty);
            EditorGUILayout.PropertyField(selectedLevelRepresentation.healSpawnPercentProperty);
            EditorGUILayout.PropertyField(selectedLevelRepresentation.dropDataProperty);
            EditorGUILayout.PropertyField(selectedLevelRepresentation.specialBehavioursProperty);
        }

        private void CollectDataFromLevelsSettings()
        {
            if (gameSettings == null)
            {
                Debug.LogError("Game settings file is null");
            }
            SerializedObject levelSettingsObject = new SerializedObject(gameSettings);

            SerializedProperty element;

            //levels database
            levelsDatabase = levelSettingsObject.FindProperty("levelsDatabase").objectReferenceValue;
            levelsDatabaseSerializedObject = new SerializedObject(levelsDatabase);
            ReadLevelDatabaseFields();

            //exit exitPoints
            exitPointPrefabProperty = levelSettingsObject.FindProperty("exitPointPrefab");

            //selectedPreset
            SerializedProperty obstaclesPresets = levelSettingsObject.FindProperty("worldPresets");
            obstaclePresets = new CatchedObstaclesPreset[obstaclesPresets.arraySize];

            for (int obstaclePresetsIndex = 0; obstaclePresetsIndex < obstaclesPresets.arraySize; obstaclePresetsIndex++)
            {
                selectedPreset = new CatchedObstaclesPreset();
                SerializedProperty presetProperty = obstaclesPresets.GetArrayElementAtIndex(obstaclePresetsIndex);
                selectedPreset.typeEnumValueIndex = presetProperty.FindPropertyRelative("type").enumValueIndex;

                SerializedProperty obstaclesProperty = presetProperty.FindPropertyRelative("obstacles");
                selectedPreset.obstacles = new CatchedPrefabRefs[obstaclesProperty.arraySize];

                for (int i = 0; i < obstaclesProperty.arraySize; i++)
                {
                    element = obstaclesProperty.GetArrayElementAtIndex(i);
                    selectedPreset.obstacles[i] = new CatchedPrefabRefs();
                    selectedPreset.obstacles[i].prefabRef = element.FindPropertyRelative("prefab").objectReferenceValue;
                    selectedPreset.obstacles[i].typeEnumValueIndex = i; //the same way as in the game
                }

                SerializedProperty environmentsProperty = presetProperty.FindPropertyRelative("environments");
                selectedPreset.environments = new CatchedPrefabRefs[environmentsProperty.arraySize];

                for (int i = 0; i < environmentsProperty.arraySize; i++)
                {
                    element = environmentsProperty.GetArrayElementAtIndex(i);
                    selectedPreset.environments[i] = new CatchedPrefabRefs();
                    selectedPreset.environments[i].prefabRef = element.FindPropertyRelative("prefab").objectReferenceValue;
                    selectedPreset.environments[i].typeEnumValueIndex = i;//the same way as in the game
                }

                obstaclePresets[obstaclePresetsIndex] = selectedPreset;
            }

            //environment preset
            SerializedProperty envtPresetsProperty = levelSettingsObject.FindProperty("roomEnvPresets");
            environmentPresets = new EnvironmentPreset[envtPresetsProperty.arraySize];

            for (int i = 0; i < envtPresetsProperty.arraySize; i++)
            {
                environmentPresets[i] = new EnvironmentPreset();
                environmentPresets[i].type = (RoomEnvironmentPresetType)envtPresetsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("type").enumValueIndex;
                environmentPresets[i].spawnPos = envtPresetsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("spawnPos").vector3Value;
                environmentPresets[i].exitPointPos = envtPresetsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("exitPointPos").vector3Value;
                environmentPresets[i].data = envtPresetsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("environmentEntities");
            }

            //Chest
            var chestsDataProperty = levelSettingsObject.FindProperty("chestData");
            chests = new CatchedPrefabRefs[chestsDataProperty.arraySize];

            for (int i = 0; i < chestsDataProperty.arraySize; i++)
            {
                var chestProp = chestsDataProperty.GetArrayElementAtIndex(i);
                var chestRefs = new CatchedPrefabRefs();

                chestRefs.prefabRef = chestProp.FindPropertyRelative("prefab").objectReferenceValue;
                chestRefs.typeEnumValueIndex = chestProp.FindPropertyRelative("type").intValue;

                chests[i] = chestRefs;

            }
        }

        private void CollectDataFromEnemiesDatabase()
        {
            if (enemiesDatabase == null)
            {
                Debug.LogError("enemiesDatabase database is null");
            }

            SerializedObject enemiesDatabaseObject = new SerializedObject(enemiesDatabase);
            SerializedProperty element;

            SerializedProperty enemiesProperty = enemiesDatabaseObject.FindProperty("enemies");
            enemies = new CatchedEnemyRefs[enemiesProperty.arraySize];

            enemyEnumValues = (EnemyType[])Enum.GetValues(typeof(EnemyType));

            for (int i = 0; i < enemiesProperty.arraySize; i++)
            {
                element = enemiesProperty.GetArrayElementAtIndex(i);
                enemies[i] = new CatchedEnemyRefs();
                enemies[i].prefabRef = element.FindPropertyRelative("prefab").objectReferenceValue;
                enemies[i].typeEnumValueIndex = element.FindPropertyRelative("enemyType").enumValueIndex;
                enemies[i].enemyType = enemyEnumValues[enemies[i].typeEnumValueIndex];
                enemies[i].image = element.FindPropertyRelative("icon").objectReferenceValue as Texture2D;

            }
        }

        public int ConvertToEnumIndex(int enumValueIndex)
        {
            EnemyType[] values = (EnemyType[])Enum.GetValues(typeof(EnemyType));
            return (int)values[enumValueIndex];

        }

        private void OpenWorld()
        {
            SaveLevelIfPosssible();
            selectedLevelRepresentation = null;
            worldNumber = new GUIContent("World #" + (selectedWorldIndex + 1));
            selectedWorldSerializedProperty = worldsSerializedProperty.GetArrayElementAtIndex(selectedWorldIndex);
            isWorldLoaded = selectedWorldSerializedProperty.objectReferenceValue != null;

            if (!isWorldLoaded)
                return;

            worldSerializedObject = new SerializedObject(selectedWorldSerializedProperty.objectReferenceValue);

            levelsProperty = worldSerializedObject.FindProperty(LEVELS_PROPERTY_PATH);
            obstaclesPresetTypeProperty = worldSerializedObject.FindProperty(OBSTACLES_PRESET_TYPE_PROPERTY_PATH);
            previewSpriteProperty = worldSerializedObject.FindProperty(PREVIEW_SPRITE_PROPERTY_PATH);

            for (int i = 0; i < obstaclePresets.Length; i++)
            {
                if (obstaclePresets[i].typeEnumValueIndex == obstaclesPresetTypeProperty.enumValueIndex)
                {
                    selectedPreset = obstaclePresets[i];
                }
            }

            levelsList = new ReorderableList(worldSerializedObject, levelsProperty, true, true, true, true);
            levelsList.onRemoveCallback = RemoveCallback;
            levelsList.drawHeaderCallback = HeaderCallback;
            levelsList.drawElementCallback = ElementCallback;
            levelsList.onSelectCallback = LevelSelectedCallback;
            levelsList.onAddCallback = AddCallback;
        }

        private void AddCallback(ReorderableList list)
        {
            levelsProperty.arraySize++;
            new LevelRepresentation(levelsProperty.GetArrayElementAtIndex(levelsProperty.arraySize - 1)).Clear();
            worldSerializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            levelsList.Select(levelsProperty.arraySize - 1);
            LevelSelectedCallback(list);
        }

        private void ElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (levelsProperty.GetArrayElementAtIndex(index).FindPropertyRelative("rooms").arraySize == 0)
            {
                GUI.Label(rect, $"Level #{index + 1} | [Empty]");
            }
            else
            {
                GUI.Label(rect, "Level #" + (index + 1));
            }

        }

        private void RemoveCallback(ReorderableList list)
        {
            if (EditorUtility.DisplayDialog("Warning", "Are you sure you want to remove level #" + (list.index + 1) + "?", "Yes", "Cancel"))
            {
                levelsProperty.DeleteArrayElementAtIndex(levelsList.index);
                worldSerializedObject.ApplyModifiedProperties();
                selectedLevelRepresentation = null;
                AssetDatabase.SaveAssets();
            }
        }

        private void HeaderCallback(Rect rect)
        {
            GUI.Label(rect, "Levels amount: " + levelsProperty.arraySize);
        }


        private void LevelSelectedCallback(ReorderableList list)
        {
            SaveLevelIfPosssible();
            selectedLevelRepresentation = new LevelRepresentation(levelsProperty.GetArrayElementAtIndex(list.index));
            LoadLevel();
        }

        protected override void Styles()
        {
            if (tabHandler != null)
            {
                tabHandler.SetDefaultToolbarStyle();
            }
        }

        #region unusedStuff
        public override void OpenLevel(UnityEngine.Object levelObject, int index)
        {
        }

        public override string GetLevelLabel(UnityEngine.Object levelObject, int index)
        {
            return string.Empty;
        }

        public override void ClearLevel(UnityEngine.Object levelObject)
        {
        }



        #endregion




        protected override void DrawContent()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != EDITOR_SCENE_NAME)
            {
                DrawOpenEditorScene();
                return;
            }

            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(400));
            DisplayDatabaseRef();

            if (!isDatabaseLoaded)
                return;

            DisplayArea();

            EditorGUILayout.EndVertical();
            DisplayLevelsTab();
        }



        private void DrawOpenEditorScene()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.HelpBox(EDITOR_SCENE_NAME + " scene required for level editor.", MessageType.Error, true);

            if (GUILayout.Button("Open \"" + EDITOR_SCENE_NAME + "\" scene"))
            {
                OpenScene(EDITOR_SCENE_PATH);
            }

            EditorGUILayout.EndVertical();
        }

        private void DisplayDatabaseRef()
        {

            EditorGUI.BeginChangeCheck();
            gameSettings = EditorGUILayout.ObjectField("Game Settings: ", gameSettings, typeof(GameSettings), false) as GameSettings;

            if (EditorGUI.EndChangeCheck())
            {
                CollectDataFromLevelsSettings();
                OpenWorld();
            }

            EditorGUI.BeginChangeCheck();
            enemiesDatabase = EditorGUILayout.ObjectField("Enemies database: ", enemiesDatabase, typeof(EnemiesDatabase), false) as EnemiesDatabase;

            if (EditorGUI.EndChangeCheck())
            {
                CollectDataFromEnemiesDatabase();
                OpenWorld();
            }


        }

        private void DisplayArea()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(selectedWorldIndex == 0);

            if (GUILayout.Button("◀", GUILayout.MaxWidth(30)))
            {
                selectedWorldIndex--;
                OpenWorld();
            }

            EditorGUI.EndDisabledGroup();


            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(selectedWorldSerializedProperty, worldNumber);

            if (EditorGUI.EndChangeCheck())
            {
                levelsDatabaseSerializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                OpenWorld();
            }



            EditorGUI.BeginDisabledGroup(selectedWorldIndex == worldsSerializedProperty.arraySize - 1);

            if (GUILayout.Button("▶", GUILayout.MaxWidth(30)))
            {
                selectedWorldIndex++;
                OpenWorld();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(previewSpriteProperty, previewSprite);
            EditorGUILayout.PropertyField(obstaclesPresetTypeProperty, presetType);

            worldSerializedObject.ApplyModifiedProperties();

            if (obstaclesPresetTypeProperty.enumValueIndex != selectedPreset.typeEnumValueIndex)
            {
                AssetDatabase.SaveAssets();
                OpenWorld();
            }

        }

        private void DisplayLevelsTab()
        {
            if (!isWorldLoaded)
                return;

            EditorGUILayout.BeginHorizontal();
            //sidebar 
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(SIDEBAR_WIDTH));
            levelsList.DoLayoutList();
            DisplaySidebarButtons();
            EditorGUILayout.EndVertical();

            GUILayout.Space(8);

            //level content
            EditorGUILayout.BeginVertical(GUI.skin.box);
            DisplaySelectedLevel();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DisplaySidebarButtons()
        {
            if (GUILayout.Button(REMOVE_SELECTION, EditorStylesExtended.button_01))
            {
                SaveLevelIfPosssible();
                selectedLevelRepresentation = null;
                levelsList.index = -1;
                ClearScene();
            }

            if (GUILayout.Button(OPEN_GAME_SCENE_LABEL, EditorStylesExtended.button_01))
            {
                SaveLevelIfPosssible();
                OpenScene(GAME_SCENE_PATH);
            }
        }

        private static void ClearScene()
        {
            EditorSceneController.Instance.Clear();
        }


        private void DisplaySelectedLevel()
        {
            if (selectedLevelRepresentation == null)
            {
                return;
            }

            if (GUILayout.Button(TEST_LEVEL, EditorStylesExtended.button_01, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                RewriteSave(selectedWorldIndex, levelsList.index);
            }

            DisplayRoomSection();

            EditorGUILayout.Space();

            if (selectedLevelRepresentation.selectedRoomindex != -1)
            {
                DisplayToolbar();
                EditorGUILayout.Space();
            }

            DisplyLevelObjectMenagementSection();
            EditorGUILayout.Space();


        }

        private void RewriteSave(int worldIndex, int levelIndex)
        {
            GlobalSave tempSave = SaveController.GetGlobalSave();

            LevelSave levelSave = tempSave.GetSaveObject<LevelSave>("level");
            levelSave.LevelIndex = levelIndex;
            levelSave.WorldIndex = worldIndex;
            tempSave.Flush();

            SaveController.SaveCustom(tempSave);
            SaveLevel();
            OpenScene(GAME_SCENE_PATH);
            EditorApplication.ExecuteMenuItem(MENU_ITEM_PATH);
        }

        private void DisplayRoomSection()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Toggle(selectedLevelRepresentation.selectedRoomindex == -1, "Settings", GUI.skin.button))
            {
                selectedLevelRepresentation.selectedRoomindex = -1;
            }



            tempRoomTabIndex = GUILayout.Toolbar(selectedLevelRepresentation.selectedRoomindex, selectedLevelRepresentation.roomTabs.ToArray());

            if (GUILayout.Button("+", GUILayout.MaxWidth(24)))
            {
                HandleAddRoomButton();
            }

            EditorGUILayout.EndHorizontal();

            if (tempRoomTabIndex != selectedLevelRepresentation.selectedRoomindex)
            {
                selectedLevelRepresentation.selectedRoomindex = tempRoomTabIndex;
                selectedLevelRepresentation.OpenRoom(tempRoomTabIndex);
                EditorSceneController.Instance.SelectRoom(tempRoomTabIndex);
            }

            if (selectedLevelRepresentation.selectedRoomindex != -1)
            {
                EditorGUILayout.PropertyField(selectedLevelRepresentation.spawnPointProperty);
                EditorSceneController.Instance.spawnPoint = selectedLevelRepresentation.spawnPointProperty.vector3Value;
                EditorGUILayout.PropertyField(selectedLevelRepresentation.exitPointProperty);
                EditorGUILayout.PropertyField(selectedLevelRepresentation.enemyEntitiesProperty);
                EditorGUILayout.PropertyField(selectedLevelRepresentation.obstacleEntitiesProperty);
                EditorGUILayout.PropertyField(selectedLevelRepresentation.environmentEntitiesProperty);
                EditorGUILayout.PropertyField(selectedLevelRepresentation.chestEntitiesProperty);


                EditorGUILayout.HelpBox("You can edit \"spawnPoint\" at any time. If you decide to change other values then first save all you progress then edit values and click button below.", MessageType.Info);

                if (GUILayout.Button("Save changes in property fields"))
                {
                    worldSerializedObject.ApplyModifiedProperties();
                    AssetDatabase.SaveAssets();
                    ReloadLevel();
                }

                if (GUILayout.Button("Delete room", EditorStylesExtended.button_04))
                {
                    if (EditorUtility.DisplayDialog("Warning", "Are you sure that you want to delete this room?", "Yes", "Cancel"))
                    {
                        EditorSceneController.Instance.DeleteRoom(selectedLevelRepresentation.selectedRoomindex);
                        SaveLevel();
                        ReloadLevel();
                    }
                }

            }
            else
            {
                if (selectedLevelRepresentation != null)
                {
                    DisplayLevelFields();
                }

            }

            EditorGUILayout.EndVertical();
        }

        private void ReloadLevel()
        {
            //we reload everything
            selectedLevelRepresentation = new LevelRepresentation(levelsProperty.GetArrayElementAtIndex(levelsList.index));
            LoadLevel();
        }

        private void HandleAddRoomButton()
        {
            GenericMenu menu = new GenericMenu();

            for (int i = 0; i < environmentPresets.Length; i++)
            {
                menu.AddItem(new GUIContent(environmentPresets[i].type.ToString()), false, CreateRoomFromPreset, i);
            }

            menu.ShowAsContext();
        }

        private void CreateRoomFromPreset(object data)
        {
            int index = (int)data;

            SerializedProperty element;
            int typeIndex;
            Vector3 position;
            Quaternion rotation;
            UnityEngine.Object prefab;

            EditorSceneController.Instance.SpawnRoom();

            for (int i = 0; i < environmentPresets[index].data.arraySize; i++)
            {
                element = environmentPresets[index].data.GetArrayElementAtIndex(i);
                typeIndex = element.FindPropertyRelative("EnvironmentType").enumValueIndex;
                position = element.FindPropertyRelative("Position").vector3Value;
                rotation = element.FindPropertyRelative("Rotation").quaternionValue;
                prefab = selectedPreset.environments[typeIndex].prefabRef;
                EditorSceneController.Instance.SpawnEnvironment(prefab as GameObject, position, rotation, (LevelEnvironmentType)typeIndex);
            }

            selectedLevelRepresentation.AddRoom();
            selectedLevelRepresentation.spawnPointProperty.vector3Value = environmentPresets[index].spawnPos;
            selectedLevelRepresentation.exitPointProperty.vector3Value = environmentPresets[index].exitPointPos;
            EditorSceneController.Instance.SelectRoom(selectedLevelRepresentation.selectedRoomindex);
            SaveLevel();
        }

        private void DisplayToolbar()
        {
            selectedToolbarTab = GUILayout.Toolbar(selectedToolbarTab, toolbarTab);

            if (selectedToolbarTab == 0)
            {
                DisplayObstaclesListSection();
            }
            else if (selectedToolbarTab == 1)
            {
                DisplayEnemiesListSection();
            }
        }

        private void DisplayObstaclesListSection()
        {
            selectedLevelFieldRect = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Obstacles:");
            EditorGUILayout.EndVertical();
            levelItemsScrollVector = EditorGUILayout.BeginScrollView(levelItemsScrollVector);

            itemsRect = EditorGUILayout.BeginVertical();
            itemPosX = itemsRect.x;
            itemPosY = itemsRect.y;

            //assigning space
            if (selectedPreset.obstacles.Length != 0)
            {
                itemsPerRow = Mathf.FloorToInt((Screen.width - SIDEBAR_WIDTH - 20) / (ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH));
                rowCount = Mathf.CeilToInt(selectedPreset.obstacles.Length * 1f / itemsPerRow);
                GUILayout.Space(rowCount * (ITEMS_BUTTON_SPACE + ITEMS_BUTTON_HEIGHT));
            }

            for (int i = 0; i < selectedPreset.obstacles.Length; i++)
            {
                itemContent = new GUIContent(AssetPreview.GetAssetPreview(selectedPreset.obstacles[i].prefabRef), ITEM_ASSIGNED);

                //check if need to start new row
                if (itemPosX + ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH > selectedLevelFieldRect.width - 10)
                {
                    itemPosX = itemsRect.x;
                    itemPosY = itemPosY + ITEMS_BUTTON_HEIGHT + ITEMS_BUTTON_SPACE;
                }

                itemRect = new Rect(itemPosX, itemPosY, ITEMS_BUTTON_WIDTH, ITEMS_BUTTON_HEIGHT);

                if (GUI.Button(itemRect, itemContent, EditorStylesExtended.button_01))
                {
                    EditorSceneController.Instance.SpawnObstacle(selectedPreset.obstacles[i].prefabRef as GameObject, Vector3.zero, Quaternion.identity, (LevelObstaclesType)selectedPreset.obstacles[i].typeEnumValueIndex);
                }
                itemPosX += ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH;
            }

            //chest
            for (int i = 0; i < chests.Length; i++)
            {
                itemContent = new GUIContent(AssetPreview.GetAssetPreview(chests[i].prefabRef), ITEM_ASSIGNED);

                //check if need to start new row
                if (itemPosX + ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH > selectedLevelFieldRect.width - 10)
                {
                    itemPosX = itemsRect.x;
                    itemPosY = itemPosY + ITEMS_BUTTON_HEIGHT + ITEMS_BUTTON_SPACE;
                }

                itemRect = new Rect(itemPosX, itemPosY, ITEMS_BUTTON_WIDTH, ITEMS_BUTTON_HEIGHT);

                if (GUI.Button(itemRect, itemContent, EditorStylesExtended.button_01))
                {
                    selectedLevelRepresentation.chestEntitiesProperty.arraySize++;
                    var chestProp = new ChestProperty();
                    chestProp.Init(selectedLevelRepresentation.chestEntitiesProperty.GetArrayElementAtIndex(selectedLevelRepresentation.chestEntitiesProperty.arraySize - 1));

                    chestProp.chestTypeProperty.intValue = chests[i].typeEnumValueIndex;

                    var newChestProperties = new ChestProperty[selectedLevelRepresentation.chestEntitiesProperty.arraySize];
                    Array.Copy(selectedLevelRepresentation.chestProperties, newChestProperties, newChestProperties.Length - 1);
                    newChestProperties[^1] = chestProp;
                    selectedLevelRepresentation.chestProperties = newChestProperties;

                    EditorSceneController.Instance.SpawnChest(chests[i].prefabRef as GameObject,
                        chestProp.chestPositionProperty.vector3Value,
                        chestProp.chestRotationProperty.quaternionValue,
                        (LevelChestType)chestProp.chestTypeProperty.intValue);

                    chestProp.isChestInitedProperty.boolValue = true;
                    worldSerializedObject.ApplyModifiedProperties();
                }

                itemPosX += ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void DisplayEnemiesListSection()
        {
            selectedLevelFieldRect = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Enemies:");
            EditorGUILayout.EndVertical();
            levelItemsScrollVector = EditorGUILayout.BeginScrollView(levelItemsScrollVector);

            itemsRect = EditorGUILayout.BeginVertical();
            itemPosX = itemsRect.x;
            itemPosY = itemsRect.y;

            //assigning space
            if (selectedPreset.obstacles.Length != 0)
            {
                itemsPerRow = Mathf.FloorToInt((Screen.width - SIDEBAR_WIDTH - 20) / (ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH));
                rowCount = Mathf.CeilToInt(enemies.Length * 1f / itemsPerRow);
                GUILayout.Space(rowCount * (ITEMS_BUTTON_SPACE + ITEMS_BUTTON_HEIGHT));
            }

            for (int i = 0; i < enemies.Length; i++)
            {
                itemContent = new GUIContent(enemies[i].image, ITEM_ASSIGNED);

                //check if need to start new row
                if (itemPosX + ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH > selectedLevelFieldRect.width - 10)
                {
                    itemPosX = itemsRect.x;
                    itemPosY = itemPosY + ITEMS_BUTTON_HEIGHT + ITEMS_BUTTON_SPACE;
                }

                itemRect = new Rect(itemPosX, itemPosY, ITEMS_BUTTON_WIDTH, ITEMS_BUTTON_HEIGHT);

                if (GUI.Button(itemRect, itemContent, EditorStylesExtended.button_01))
                {
                    EditorSceneController.Instance.SpawnEnemy(enemies[i].prefabRef as GameObject, Vector3.zero, Quaternion.Euler(0, 180, 0), enemies[i].enemyType, false, new Vector3[0]);
                }
                itemPosX += ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH;
            }


            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void DisplyLevelObjectMenagementSection()
        {
            EditorGUILayout.LabelField(OBJECT_MANAGEMENT);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(CLEAR_SCENE, EditorStylesExtended.button_04, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                ClearScene();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button(LOAD, EditorStylesExtended.button_03, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                LoadLevel();
            }

            if (GUILayout.Button(SAVE, EditorStylesExtended.button_02, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
            {
                SaveLevel();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void LoadLevel()
        {
            EditorSceneController.Instance.Clear();

            for (int i = 0; i < selectedLevelRepresentation.roomsProperty.arraySize; i++)
            {
                selectedLevelRepresentation.OpenRoom(i);
                EditorSceneController.Instance.SpawnRoom();
                SpawnEnvironment();
                SpawnObstacles();
                SpawnEnemy();
                SpawnExitPoint();
                SpawnChest();
            }
        }



        private void SpawnEnvironment()
        {
            SerializedProperty element;
            int typeIndex;
            Vector3 position;
            Quaternion rotation;
            UnityEngine.Object prefab;

            for (int i = 0; i < selectedLevelRepresentation.environmentEntitiesProperty.arraySize; i++)
            {
                element = selectedLevelRepresentation.environmentEntitiesProperty.GetArrayElementAtIndex(i);
                typeIndex = element.FindPropertyRelative("EnvironmentType").enumValueIndex;
                position = element.FindPropertyRelative("Position").vector3Value;
                rotation = element.FindPropertyRelative("Rotation").quaternionValue;
                prefab = selectedPreset.environments[typeIndex].prefabRef;
                EditorSceneController.Instance.SpawnEnvironment(prefab as GameObject, position, rotation, (LevelEnvironmentType)typeIndex);
            }
        }

        private void SpawnObstacles()
        {
            SerializedProperty element;
            int typeIndex;
            Vector3 position;
            Quaternion rotation;
            UnityEngine.Object prefab;

            for (int i = 0; i < selectedLevelRepresentation.obstacleEntitiesProperty.arraySize; i++)
            {
                element = selectedLevelRepresentation.obstacleEntitiesProperty.GetArrayElementAtIndex(i);
                typeIndex = element.FindPropertyRelative("ObstaclesType").enumValueIndex;
                position = element.FindPropertyRelative("Position").vector3Value;
                rotation = element.FindPropertyRelative("Rotation").quaternionValue;
                prefab = selectedPreset.obstacles[typeIndex].prefabRef;
                EditorSceneController.Instance.SpawnObstacle(prefab as GameObject, position, rotation, (LevelObstaclesType)typeIndex);
            }
        }

        private void SpawnEnemy()
        {
            SerializedProperty element;
            SerializedProperty pointsArray;
            int typeIndex;
            Vector3 position;
            Quaternion rotation;
            UnityEngine.Object prefab = enemies[0].prefabRef;
            bool isElite;
            Vector3[] pathPoints;
            EnemyType type = EnemyType.BatMelee;

            for (int i = 0; i < selectedLevelRepresentation.enemyEntitiesProperty.arraySize; i++)
            {
                element = selectedLevelRepresentation.enemyEntitiesProperty.GetArrayElementAtIndex(i);
                typeIndex = element.FindPropertyRelative("EnemyType").enumValueIndex;
                position = element.FindPropertyRelative("Position").vector3Value;
                rotation = element.FindPropertyRelative("Rotation").quaternionValue;
                isElite = element.FindPropertyRelative("IsElite").boolValue;
                pointsArray = element.FindPropertyRelative("PathPoints");
                pathPoints = new Vector3[pointsArray.arraySize];

                for (int j = 0; j < pointsArray.arraySize; j++)
                {
                    pathPoints[j] = pointsArray.GetArrayElementAtIndex(j).vector3Value;
                }


                for (int j = 0; j < enemies.Length; j++)
                {
                    if (enemies[j].typeEnumValueIndex == typeIndex)
                    {
                        prefab = enemies[j].prefabRef;
                        type = enemies[j].enemyType;
                        break;
                    }
                }

                EditorSceneController.Instance.SpawnEnemy(prefab as GameObject, position, rotation, type, isElite, pathPoints);
            }
        }

        private void SpawnExitPoint()
        {
            Vector3 position;
            UnityEngine.Object prefab = exitPointPrefabProperty.objectReferenceValue;

            position = selectedLevelRepresentation.exitPointProperty.vector3Value;

            EditorSceneController.Instance.SpawnExitPoint(prefab as GameObject, position);
        }

        private void SpawnChest()
        {
            for (int i = 0; i < selectedLevelRepresentation.chestProperties.Length; i++)
            {
                if (selectedLevelRepresentation.chestProperties[i].isChestInitedProperty.boolValue)
                {
                    var chestProp = selectedLevelRepresentation.chestProperties[i];
                    var chestType = chestProp.chestTypeProperty.intValue;
                    UnityEngine.Object prefab = null;
                    for (int j = 0; j < chests.Length; j++)
                    {
                        if (chests[j].typeEnumValueIndex == chestType)
                        {
                            prefab = chests[j].prefabRef;
                            break;
                        }
                    }

                    EditorSceneController.Instance.SpawnChest(prefab as GameObject, chestProp.chestPositionProperty.vector3Value, chestProp.chestRotationProperty.quaternionValue, (LevelChestType)chestType);
                }
            }
        }

        private void SaveLevel()
        {
            selectedLevelRepresentation.roomsProperty.arraySize = EditorSceneController.Instance.rooms.Count;

            for (int roomIndex = 0; roomIndex < selectedLevelRepresentation.roomsProperty.arraySize; roomIndex++)
            {
                selectedLevelRepresentation.OpenRoom(roomIndex);
                SaveEnvironment(roomIndex);
                SaveObstacles(roomIndex);
                SaveEnemy(roomIndex);
                SaveExitPoint(roomIndex);
                SaveChest(roomIndex);
            }

            worldSerializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        private void SaveEnvironment(int roomIndex)
        {
            SerializedProperty element;
            EnvironmentEntityData[] data = EditorSceneController.Instance.CollectEnvironmentsFromRoom(roomIndex);
            selectedLevelRepresentation.environmentEntitiesProperty.arraySize = data.Length;

            for (int i = 0; i < selectedLevelRepresentation.environmentEntitiesProperty.arraySize; i++)
            {
                element = selectedLevelRepresentation.environmentEntitiesProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("EnvironmentType").enumValueIndex = (int)data[i].EnvironmentType;
                element.FindPropertyRelative("Position").vector3Value = data[i].Position;
                element.FindPropertyRelative("Rotation").quaternionValue = data[i].Rotation;
            }
        }

        private void SaveObstacles(int roomIndex)
        {
            SerializedProperty element;
            ObstacleEntityData[] data = EditorSceneController.Instance.CollectObstaclesFromRoom(roomIndex);
            selectedLevelRepresentation.obstacleEntitiesProperty.arraySize = data.Length;

            for (int i = 0; i < selectedLevelRepresentation.obstacleEntitiesProperty.arraySize; i++)
            {
                element = selectedLevelRepresentation.obstacleEntitiesProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("ObstaclesType").enumValueIndex = (int)data[i].ObstaclesType;
                element.FindPropertyRelative("Position").vector3Value = data[i].Position;
                element.FindPropertyRelative("Rotation").quaternionValue = data[i].Rotation;
            }
        }

        private void SaveEnemy(int roomIndex)
        {
            SerializedProperty element;
            SerializedProperty pathPoints;
            EnemyEntityData[] data = EditorSceneController.Instance.CollectEnemiesFromRoom(roomIndex);
            selectedLevelRepresentation.enemyEntitiesProperty.arraySize = data.Length;

            for (int i = 0; i < selectedLevelRepresentation.enemyEntitiesProperty.arraySize; i++)
            {
                element = selectedLevelRepresentation.enemyEntitiesProperty.GetArrayElementAtIndex(i);

                for (int j = 0; j < enemies.Length; j++)
                {
                    if (enemies[j].enemyType == data[i].EnemyType)
                    {
                        element.FindPropertyRelative("EnemyType").enumValueIndex = enemies[j].typeEnumValueIndex;
                    }
                }

                element.FindPropertyRelative("Position").vector3Value = data[i].Position;
                element.FindPropertyRelative("Rotation").quaternionValue = data[i].Rotation;
                element.FindPropertyRelative("IsElite").boolValue = data[i].IsElite;

                pathPoints = element.FindPropertyRelative("PathPoints");
                pathPoints.arraySize = data[i].PathPoints.Length;

                for (int j = 0; j < pathPoints.arraySize; j++)
                {
                    pathPoints.GetArrayElementAtIndex(j).vector3Value = data[i].PathPoints[j];
                }
            }
        }

        private void SaveExitPoint(int roomIndex)
        {
            Vector3 position;

            if (EditorSceneController.Instance.CollectExitPointFromRoom(roomIndex, out position))
            {
                selectedLevelRepresentation.exitPointProperty.vector3Value = position;
            }
        }

        private void SaveChest(int roomIndex)
        {
            var chests = EditorSceneController.Instance.CollectChestFromRoom(roomIndex);

            selectedLevelRepresentation.chestEntitiesProperty.arraySize = chests.Count;

            for (int i = 0; i < chests.Count; i++)
            {
                var chestData = chests[i];

                var chestProp = new ChestProperty();
                chestProp.Init(selectedLevelRepresentation.chestEntitiesProperty.GetArrayElementAtIndex(i));

                chestProp.chestPositionProperty.vector3Value = chestData.transform.localPosition;
                chestProp.chestRotationProperty.quaternionValue = chestData.transform.localRotation;
                chestProp.isChestInitedProperty.boolValue = true;
                chestProp.chestTypeProperty.intValue = (int)chestData.type;
            }
        }

        private void SaveLevelIfPosssible()
        {

            if (selectedLevelRepresentation == null)
            {
                return;
            }

            try
            {
                SaveLevel();
            }
            catch
            {

            }

        }

        // this 2 overriden methods prevent level editor from closing in play mode 

        public override void OnBeforeAssemblyReload()
        {
        }

        public override bool WindowClosedInPlaymode()
        {
            return false;
        }

        protected class LevelRepresentation
        {
            public SerializedProperty levelProperty;

            //level
            public SerializedProperty levelTypeProperty;
            public SerializedProperty roomsProperty;
            public SerializedProperty specialBehavioursProperty;
            public SerializedProperty xpAmountProperty;
            public SerializedProperty requiredUpgProperty;
            public SerializedProperty enemiesLevelProperty;
            public SerializedProperty hasCharacterSuggestionProperty;
            public SerializedProperty dropDataProperty;
            public SerializedProperty healSpawnPercentProperty;


            //rooms
            public int selectedRoomindex;
            public SerializedProperty selectedRoom;

            public SerializedProperty spawnPointProperty;
            public SerializedProperty exitPointProperty;
            public SerializedProperty enemyEntitiesProperty;
            public SerializedProperty obstacleEntitiesProperty;
            public SerializedProperty environmentEntitiesProperty;

            public SerializedProperty chestEntitiesProperty;
            public ChestProperty[] chestProperties;

            //room tabs
            public List<string> roomTabs;


            public LevelRepresentation(SerializedProperty levelProperty)
            {
                this.levelProperty = levelProperty;
                levelTypeProperty = levelProperty.FindPropertyRelative("type");
                roomsProperty = levelProperty.FindPropertyRelative("rooms");
                specialBehavioursProperty = levelProperty.FindPropertyRelative("specialBehaviours");
                xpAmountProperty = levelProperty.FindPropertyRelative("xpAmount");
                requiredUpgProperty = levelProperty.FindPropertyRelative("requiredUpg");
                enemiesLevelProperty = levelProperty.FindPropertyRelative("enemiesLevel");
                hasCharacterSuggestionProperty = levelProperty.FindPropertyRelative("hasCharacterSuggestion");
                dropDataProperty = levelProperty.FindPropertyRelative("dropData");
                healSpawnPercentProperty = levelProperty.FindPropertyRelative("healSpawnPercent");

                selectedRoomindex = -1;
                roomTabs = new List<string>();
                selectedRoomindex = -1;

                for (int i = 0; i < roomsProperty.arraySize; i++)
                {
                    roomTabs.Add("Room #" + (i + 1));
                }
            }

            public void OpenRoom(int index)
            {
                selectedRoom = roomsProperty.GetArrayElementAtIndex(index);
                spawnPointProperty = selectedRoom.FindPropertyRelative("spawnPoint");
                exitPointProperty = selectedRoom.FindPropertyRelative("exitPoint");
                enemyEntitiesProperty = selectedRoom.FindPropertyRelative("enemyEntities");
                obstacleEntitiesProperty = selectedRoom.FindPropertyRelative("obstacleEntities");
                environmentEntitiesProperty = selectedRoom.FindPropertyRelative("environmentEntities");
                chestEntitiesProperty = selectedRoom.FindPropertyRelative("chestEntities");


                chestProperties = new ChestProperty[chestEntitiesProperty.arraySize];
                for (int i = 0; i < chestEntitiesProperty.arraySize; i++)
                {
                    var chestProperty = chestEntitiesProperty.GetArrayElementAtIndex(i);

                    chestProperties[i] = new ChestProperty();
                    chestProperties[i].Init(chestProperty);
                }
            }

            public void AddRoom()
            {
                roomsProperty.arraySize++;
                roomTabs.Add("Room #" + roomsProperty.arraySize);
                selectedRoomindex = roomsProperty.arraySize - 1;
                OpenRoom(selectedRoomindex);

                spawnPointProperty.vector3Value = new Vector3(0, 0, -90);
                exitPointProperty.vector3Value = new Vector3(0, 0, 125);
                enemyEntitiesProperty.arraySize = 0;
                obstacleEntitiesProperty.arraySize = 0;
                environmentEntitiesProperty.arraySize = 0;
                chestEntitiesProperty.arraySize = 0;
            }



            public void Clear()
            {
                levelTypeProperty.enumValueIndex = 0;
                roomsProperty.arraySize = 0;
                specialBehavioursProperty.arraySize = 0;
                xpAmountProperty.intValue = 0;
                requiredUpgProperty.intValue = 0;
                enemiesLevelProperty.intValue = 0;
                hasCharacterSuggestionProperty.boolValue = false;
                dropDataProperty.arraySize = 0;
                healSpawnPercentProperty.floatValue = 0.5f;
            }
        }

        public class ChestProperty
        {
            public SerializedProperty chestProperty;
            public SerializedProperty isChestInitedProperty;
            public SerializedProperty chestTypeProperty;
            public SerializedProperty chestPositionProperty;
            public SerializedProperty chestRotationProperty;

            public void Init(SerializedProperty chestProperty)
            {
                this.chestProperty = chestProperty;

                isChestInitedProperty = chestProperty.FindPropertyRelative("IsInited");
                chestTypeProperty = chestProperty.FindPropertyRelative("ChestType");
                chestPositionProperty = chestProperty.FindPropertyRelative("Position");
                chestRotationProperty = chestProperty.FindPropertyRelative("Rotation");
            }
        }

        private class CatchedPrefabRefs
        {
            public UnityEngine.Object prefabRef;
            public int typeEnumValueIndex;
        }

        private class CatchedEnemyRefs
        {
            public UnityEngine.Object prefabRef;
            public int typeEnumValueIndex;
            public EnemyType enemyType;
            public Texture2D image;
        }

        private class CatchedObstaclesPreset
        {
            public int typeEnumValueIndex;
            public CatchedPrefabRefs[] obstacles;
            public CatchedPrefabRefs[] environments;
        }

        private class EnvironmentPreset
        {
            public RoomEnvironmentPresetType type;
            public SerializedProperty data;
            public Vector3 spawnPos;
            public Vector3 exitPointPos;
        }
    }
}

// -----------------
// Scene interraction level editor V1.5
// -----------------

// Changelog
// v 1.4
// • Updated EnumObjectlist
// • Updated object preview
// v 1.4
// • Updated EnumObjectlist
// • Fixed bug with window size
// v 1.3
// • Updated EnumObjectlist
// • Added StartPointHandles script that can be added to gameobjects
// v 1.2
// • Reordered some methods
// v 1.1
// • Added spawner tool
// v 1 basic version works
