using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Watermelon
{
    [CustomEditor(typeof(SetupGuideInfo))]
    public class SetupGuideInfoEditor : WatermelonEditor
    {    
        private static SetupGuideInfoEditor instance;

        private const string SITE_URL = @"https://wmelongames.com";

        private const string PROTOTYPE_URL = @"https://wmelongames.com/prototype/guide.php";
        private const string MAIL_URL = "https://wmelongames.com/contact/";
        private const string DISCORD_URL = "https://discord.gg/xEGUnBg";
        private const string WATERMELON_CORE_FOLDER_NAME = "Watermelon Core";
        private const string CORE_CHANGELOG_PATH_SUFFIX = "/Core Changelog.txt";
        private const string DOCUMENTATION_PATH_SUFFIX = "/DOCUMENTATION.txt";
        private const string CHANGELOG_PATH_SUFFIX = "/Template Changelog.txt";
        private const string DEFAULT_VALUE = "[unknown]";
        private const string DOCUMENTATION_URL_PROPERTY_PATH = "documentationURL";
        private static readonly string PROJECT_DESCRIPTION = @"Thank you for purchasing {0}.\nBefore you start working with project, read the documentation.\nPlease, leave a review and rate the project.";

        private SetupGuideInfo setupGuideInfo;

        private GUIStyle descriptionStyle;
        private GUIStyle setupButtonStyle;
        private GUIStyle gameButtonStyle;
        private GUIStyle textGamesStyle;
        private GUIStyle logoStyle;
        private GUIStyle projectStyle;

        private GUIContent logoContent;
        private GUIContent mailButtonContent;
        private GUIContent discordButtonContent;
        private GUIContent documentationButtonContent;

        private string description;
        
        private static SetupButton[] setupButtons;
        private static FinishedProject[] finishedProjects;

        private SerializedObject targetSerializedObject;

        private string coreVersion;
        private string projectVersion;
        private string documentationUrl;
        private float defaultLength;

        protected override void OnEnable()
        {
            base.OnEnable();

            instance = this;
            
            setupGuideInfo = target as SetupGuideInfo;
            targetSerializedObject = new SerializedObject(target);

            List<SetupButton> tempSetupButtons = new List<SetupButton>();
            if(!setupGuideInfo.windowButtons.IsNullOrEmpty())
                tempSetupButtons.AddRange(setupGuideInfo.windowButtons);
            if (!setupGuideInfo.folderButtons.IsNullOrEmpty())
                tempSetupButtons.AddRange(setupGuideInfo.folderButtons);
            if (!setupGuideInfo.fileButtons.IsNullOrEmpty())
                tempSetupButtons.AddRange(setupGuideInfo.fileButtons);

            setupButtons = tempSetupButtons.ToArray();

            string coreFolderPath =  EditorUtils.FindFolderPath(WATERMELON_CORE_FOLDER_NAME).Replace('\\','/');
            string coreChangelogPath = coreFolderPath + CORE_CHANGELOG_PATH_SUFFIX;
            string changelogPath = coreFolderPath.Substring(0, coreFolderPath.Length - 16) + CHANGELOG_PATH_SUFFIX; // 16 symbols in "/Watermelon Core"
            string documentationPath = coreFolderPath.Substring(0, coreFolderPath.Length - 16) + DOCUMENTATION_PATH_SUFFIX; // 16 symbols in "/Watermelon Core"

            try
            {
                using (System.IO.StreamReader fileReader = new System.IO.StreamReader(coreChangelogPath))
                {
                    coreVersion = fileReader.ReadLine();
                }
            }
            catch
            {
                coreVersion = DEFAULT_VALUE;
            }

            try
            {
                using (System.IO.StreamReader fileReader = new System.IO.StreamReader(changelogPath))
                {
                    projectVersion = fileReader.ReadLine();
                }
            }
            catch
            {
                projectVersion = DEFAULT_VALUE;
            }

            try
            {
                string[] lines = System.IO.File.ReadAllLines(documentationPath);
                string lastLine = lines[lines.Length - 1];
                documentationUrl = lastLine.Substring(lastLine.IndexOf("http"));
                SerializedObject serializedObject = new SerializedObject(target);
                serializedObject.FindProperty(DOCUMENTATION_URL_PROPERTY_PATH).stringValue = documentationUrl;
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }
            catch
            {
                
            }
        }

        protected override void Styles()
        {
            description = string.Format(PROJECT_DESCRIPTION, setupGuideInfo.gameName).Replace("\\n", "\n");

            logoContent = new GUIContent(EditorGUIUtility.isProSkin ? EditorStylesExtended.GetTexture("logo_white") : EditorStylesExtended.GetTexture("logo_black"), SITE_URL);

            textGamesStyle = EditorStylesExtended.GetAligmentStyle(EditorStylesExtended.label_small, TextAnchor.MiddleCenter);
            textGamesStyle.alignment = TextAnchor.MiddleCenter;
            textGamesStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

            gameButtonStyle = EditorStylesExtended.GetPaddingStyle(EditorStylesExtended.button_05, new RectOffset(2, 2, 2, 2));

            descriptionStyle = new GUIStyle(EditorStyles.label);
            descriptionStyle.wordWrap = true;

            setupButtonStyle = new GUIStyle(EditorStylesExtended.button_01);
            setupButtonStyle.imagePosition = ImagePosition.ImageAbove;

            mailButtonContent = new GUIContent(EditorStylesExtended.GetTexture("icon_mail", EditorStylesExtended.IconColor));
            discordButtonContent = new GUIContent(EditorStylesExtended.GetTexture("icon_discord", EditorStylesExtended.IconColor));
            documentationButtonContent = new GUIContent(EditorStylesExtended.ICON_SPACE + "Documentation", EditorStylesExtended.GetTexture("icon_documentation", EditorStylesExtended.IconColor));

            logoStyle = new GUIStyle(GUIStyle.none);
            logoStyle.alignment = TextAnchor.MiddleCenter;
            logoStyle.padding = new RectOffset(10, 10, 10, 10);

            projectStyle = new GUIStyle(GUI.skin.label);
            projectStyle.alignment = TextAnchor.MiddleCenter;
            projectStyle.wordWrap = false;
            projectStyle.clipping = TextClipping.Overflow;

            for (int i = 0; i < setupButtons.Length; i++)
            {
                setupButtons[i].Init();
            }

            if (finishedProjects.IsNullOrEmpty())
            {
                EditorCoroutines.Execute(instance.GetRequest(PROTOTYPE_URL));
            }
            else
            {
                for (int i = 0; i < finishedProjects.Length; i++)
                {
                    finishedProjects[i].LoadTexture();
                }
            }
        }
        
        public override void OnInspectorGUI()
        {
            InitStyles();

            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(logoContent, logoStyle, GUILayout.Width(80), GUILayout.Height(80)))
            {
                Application.OpenURL(SITE_URL);
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal(EditorStylesExtended.padding05, GUILayout.Height(21), GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("GREETINGS!", EditorStylesExtended.boxHeader, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(discordButtonContent, EditorStylesExtended.button_01, GUILayout.Width(22), GUILayout.Height(22)))
            {
                Application.OpenURL(DISCORD_URL);
            }

            if (GUILayout.Button(mailButtonContent, EditorStylesExtended.button_01, GUILayout.Width(22), GUILayout.Height(22)))
            {
                Application.OpenURL(MAIL_URL);
            }

            if (GUILayout.Button(documentationButtonContent, EditorStylesExtended.button_01, GUILayout.Height(22), GUILayout.MinWidth(112)))
            {
                Application.OpenURL(setupGuideInfo.documentationURL);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(description, descriptionStyle);

            defaultLength = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 115;

            EditorGUILayout.LabelField("Project version", projectVersion);
            EditorGUILayout.LabelField("Core version", coreVersion);

            EditorGUIUtility.labelWidth = defaultLength;
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            EditorGUILayout.LabelField(GUIContent.none, EditorStylesExtended.editorSkin.horizontalSlider);
            GUILayout.Space(-15);

            if (setupButtons.Length > 0)
            {
                EditorGUILayoutCustom.Header("LINKS");

                EditorGUILayout.BeginHorizontal();

                for (int i = 0; i < setupButtons.Length; i++)
                {
                    setupButtons[i].Draw(setupButtonStyle);
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(20);
                EditorGUILayout.LabelField(GUIContent.none, EditorStylesExtended.editorSkin.horizontalSlider);
                GUILayout.Space(-10);
            }

            EditorGUILayoutCustom.Header("OUR TEMPLATES");

            EditorGUILayout.BeginHorizontal();

            if (finishedProjects != null)
            {
                GUILayout.FlexibleSpace();
                for (int i = 0; i < finishedProjects.Length; i++)
                {
                    EditorGUILayout.BeginVertical();
                    if (GUILayout.Button(new GUIContent(finishedProjects[i].gameTexture, finishedProjects[i].name), gameButtonStyle, GUILayout.Height(65), GUILayout.Width(65)))
                    {
                        Application.OpenURL(finishedProjects[i].url);
                    }
                    EditorGUILayout.LabelField(finishedProjects[i].name, projectStyle, GUILayout.Width(70));
                    EditorGUILayout.EndVertical();

                    GUILayout.FlexibleSpace();

                    if(i != 0 && (i + 1) % 4 == 0)
                    {
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(10);
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                    }    
                }
            }
            else
            {
                EditorGUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Loading templates..", textGamesStyle);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
            targetSerializedObject.ApplyModifiedProperties();
        }
        
        #region Web
        private IEnumerator GetRequest(string uri)
        {
            UnityWebRequest www = UnityWebRequest.Get(uri);
            www.SendWebRequest();

            while (!www.isDone)
            {
                yield return null;
            }

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("[Setup Guide]: " + www.error);
            }
            else
            {
                // Or retrieve results as binary data
                byte[] results = www.downloadHandler.data;

                // For that you will need to add reference to System.Runtime.Serialization
                var jsonReader = JsonReaderWriterFactory.CreateJsonReader(results, new System.Xml.XmlDictionaryReaderQuotas());

                // For that you will need to add reference to System.Xml and System.Xml.Linq
                var root = XElement.Load(jsonReader);

                List<FinishedProject> finishedProjectsTemp = new List<FinishedProject>();
                foreach (var element in root.Elements())
                {
                    FinishedProject projectTemp = new FinishedProject(element.XPathSelectElement("name").Value, element.XPathSelectElement("url").Value, element.XPathSelectElement("image").Value);

                    projectTemp.LoadTexture();

                    finishedProjectsTemp.Add(projectTemp);
                }

                finishedProjects = finishedProjectsTemp.ToArray();
            }
        }
        #endregion

        private static void RepaintEditor()
        {
            if (instance != null)
                instance.Repaint();
        }

        [MenuItem("CONTEXT/SetupGuideInfo/Create Documentation")]
        static void CreateDocumentation(MenuCommand command)
        {
            SetupGuideInfo setupGuideInfo = (SetupGuideInfo)command.context;
            if (setupGuideInfo != null)
            {
                OnlineDocumentation onlineDocumentation = new OnlineDocumentation(setupGuideInfo.documentationURL);
                onlineDocumentation.SaveToFile();
            }
        }

        private class FinishedProject
        {
            public string name = "";
            public string url = "";

            public string imageUrl = "";
            public Texture2D gameTexture;

            public FinishedProject(string name, string url, string imageUrl)
            {
                this.name = name;
                this.url = url;
                this.imageUrl = imageUrl;
            }

            public void LoadTexture()
            {
                if(!string.IsNullOrEmpty(url))
                {
                    EditorCoroutines.Execute(GetTexture(imageUrl, (texture) =>
                    {
                        gameTexture = texture;

                        SetupGuideInfoEditor.RepaintEditor();
                        SetupGuideWindow.RepaintWindow();
                    }));
                }
            }

            private IEnumerator GetTexture(string uri, System.Action<Texture2D> onLoad)
            {
                UnityWebRequest www = UnityWebRequestTexture.GetTexture(uri);
                www.SendWebRequest();

                while (!www.isDone)
                {
                    yield return null;
                }

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    if (myTexture != null)
                    {
                        onLoad.Invoke(myTexture);
                    }
                }
            }
        }
    }
}

// -----------------
// Setup Guide v 1.0.2
// -----------------