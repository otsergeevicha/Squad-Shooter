using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace Watermelon
{
    [CustomEditor(typeof(AdsData))]
    public class AdsDataEditor : WatermelonEditor
    {
        private SerializedProperty bannerTypeProperty;
        private SerializedProperty interstitialTypeProperty;
        private SerializedProperty rewardedVideoTypeProperty;

        private IEnumerable<SerializedProperty> gdprProperties;
        private SerializedProperty testModeProperty;
        private SerializedProperty systemLogsProperty;
        private SerializedProperty insterstitialFirstDelayProperty;
        private SerializedProperty interstitialShowingDelayProperty;

        private readonly AdsContainer[] adsContainers = new AdsContainer[]
        {
            new DummyContainer("Dummy", "dummyContainer", string.Empty),
            new AdMobContainer("AdMob", "adMobContainer", "MODULE_ADMOB"),
            new UnityAdsContainer("Unity Ads Legacy", "unityAdsContainer", "MODULE_UNITYADS"),
        };

        private static GUIContent arrowDownContent;
        private static GUIContent arrowUpContent;
        private static GUIContent testIdContent;
        private static GUIStyle groupStyle;

        protected override void OnEnable()
        {
            base.OnEnable();

            bannerTypeProperty = serializedObject.FindProperty("bannerType");
            interstitialTypeProperty = serializedObject.FindProperty("interstitialType");
            rewardedVideoTypeProperty = serializedObject.FindProperty("rewardedVideoType");

            gdprProperties = serializedObject.GetPropertiesByGroup("Privacy");

            testModeProperty = serializedObject.FindProperty("testMode");
            systemLogsProperty = serializedObject.FindProperty("systemLogs");
            insterstitialFirstDelayProperty = serializedObject.FindProperty("insterstitialFirstDelay");
            interstitialShowingDelayProperty = serializedObject.FindProperty("interstitialShowingDelay");

            for (int i = 0; i < adsContainers.Length; i++)
            {
                adsContainers[i].Initialize(serializedObject);
            }

            ForceInitStyles();

            serializedObject.ApplyModifiedProperties();
        }

        protected override void Styles()
        {
            arrowDownContent = new GUIContent(EditorStylesExtended.GetTexture("icon_arrow_down", new Color(0.2f, 0.2f, 0.2f)));
            arrowUpContent = new GUIContent(EditorStylesExtended.GetTexture("icon_arrow_up", new Color(0.2f, 0.2f, 0.2f)));
            testIdContent = new GUIContent(" test id",EditorStylesExtended.GetTexture("icon_warning"),"You are using test app id value.");
            groupStyle = new GUIStyle(EditorStylesExtended.label_medium);
            groupStyle.fontSize = 14;
        }

        public override void OnInspectorGUI()
        {
            InitStyles();

            serializedObject.Update();

            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            EditorGUILayoutCustom.Header("ADVERTISING");

            EditorGUILayout.PropertyField(bannerTypeProperty);
            EditorGUILayout.PropertyField(interstitialTypeProperty);
            EditorGUILayout.PropertyField(rewardedVideoTypeProperty);

            EditorGUILayout.EndVertical();

            GUILayout.Space(8);

            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            EditorGUILayoutCustom.Header("SETTINGS");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(testModeProperty);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                UnityEditor.Advertisements.AdvertisementSettings.testMode = testModeProperty.boolValue;
            }

            EditorGUILayout.PropertyField(systemLogsProperty);
            EditorGUILayout.PropertyField(insterstitialFirstDelayProperty);
            EditorGUILayout.PropertyField(interstitialShowingDelayProperty);

            GUILayout.Space(5);

            EditorGUILayout.EndVertical();

            GUILayout.Space(8);

            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            EditorGUILayoutCustom.Header("PRIVACY");

            foreach (SerializedProperty prop in gdprProperties)
            {
                EditorGUILayout.PropertyField(prop);
            }

            EditorGUILayout.EndVertical();

            GUILayout.Space(8);

            for (int i = 0; i < adsContainers.Length; i++)
            {
                adsContainers[i].DrawContainer();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private abstract class AdsContainer
        {
            private SerializedProperty containerProperty;
            private IEnumerable<SerializedProperty> containerProperties;

            private bool isDefineEnabled = false;

            private string containerName;
            private string propertyName;
            private string defineName;

            protected SerializedProperty ContainerProperty => containerProperty;

            protected bool IsDefineEnabled => isDefineEnabled;
            protected string ContainerName => containerName;
            protected string PropertyName => propertyName;
            protected string DefineName => defineName;

            public AdsContainer(string containerName, string propertyName, string defineName)
            {
                this.containerName = containerName;
                this.propertyName = propertyName;
                this.defineName = defineName;

                if (string.IsNullOrEmpty(defineName))
                    isDefineEnabled = true;
            }

            public virtual void Initialize(SerializedObject serializedObject)
            {
                containerProperty = serializedObject.FindProperty(propertyName);
                containerProperties = containerProperty.GetChildren();

                InititalizeDefine();
            }

            public void InititalizeDefine()
            {
                if(!string.IsNullOrEmpty(defineName))
                    isDefineEnabled = DefineManager.HasDefine(defineName);
            }

            public virtual void DrawContainer()
            {
                EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

                containerProperty.isExpanded = EditorGUILayoutCustom.HeaderExpand(containerName, containerProperty.isExpanded, AdsDataEditor.arrowUpContent, AdsDataEditor.arrowDownContent);

                if (containerProperty.isExpanded)
                {
                    if (!isDefineEnabled)
                    {
                        EditorGUILayoutCustom.HelpBox(containerName + " define isn't enabled!", "Enable", delegate
                        {
                            DefineManager.EnableDefine(defineName);

                            InititalizeDefine();
                        });
                    }

                    foreach (SerializedProperty prop in containerProperties)
                    {
                        EditorGUILayout.PropertyField(prop);
                    }

                    SpecialButtons();
                }

                EditorGUILayout.EndVertical();
            }

            protected abstract void SpecialButtons();
        }

        private class AdMobContainer : AdsContainer
        {
            //App section
            private const string SETTINGS_FILE_PATH = "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";
            private const string TEST_APP_ID = "ca-app-pub-3940256099942544~3347511713";
            private const string ANDROID_APP_ID_PROPERTY_PATH = "adMobAndroidAppId";
            private const string OUR_ANDROID_APP_ID_PROPERTY_PATH = "androidAppId";
            private const string IOS_APP_ID_PROPERTY_PATH = "adMobIOSAppId";
            private const string OUR_IOS_APP_ID_PROPERTY_PATH = "iosAppId";

            private SerializedProperty androidAppIdProperty;
            private SerializedProperty iOSAppIdProperty;
            private SerializedProperty ourAndroidAppIdProperty;
            private SerializedProperty ourIOSAppIdProperty;
            private UnityEngine.Object settingsFile;
            private SerializedObject serializedObject;
            private bool fileLoaded;

            //Add Units section 
            private const string BANNER_TYPE_PROPERTY_PATH = "bannerType";
            private const string BANNER_POSITION_PROPERTY_PATH = "bannerPosition";
            private const string ANDROID_BANNER_ID_PROPERTY_PATH = "androidBannerID";
            private const string ANDROID_INTERSTITIAL_ID_PROPERTY_PATH = "androidInterstitialID";
            private const string ANDROID_REWARDED_VIDEO_ID_PROPERTY_PATH = "androidRewardedVideoID";
            private const string IOS_BANNER_ID_PROPERTY_PATH = "iOSBannerID";
            private const string IOS_INTERSTITIAL_ID_PROPERTY_PATH = "iOSInterstitialID";
            private const string IOS_REWARDED_VIDEO_ID_PROPERTY_PATH = "iOSRewardedVideoID";


            private SerializedProperty bannerTypeProperty;
            private SerializedProperty bannerPositionProperty;
            private SerializedProperty androidBannerIdProperty;
            private SerializedProperty androidInterstitialIdProperty;
            private SerializedProperty androidRewardedVideoIdProperty;
            private SerializedProperty iOSBannerIdProperty;
            private SerializedProperty iOSInterstitialIdProperty;
            private SerializedProperty iOSRewardedVideoIdProperty;

            public AdMobContainer(string containerName, string propertyName, string defineName) : base(containerName, propertyName, defineName)
            {
            }

            public override void Initialize(SerializedObject serializedObject)
            {
                base.Initialize(serializedObject);

                //for add units section
                bannerTypeProperty = base.ContainerProperty.FindPropertyRelative(BANNER_TYPE_PROPERTY_PATH);
                bannerPositionProperty = base.ContainerProperty.FindPropertyRelative(BANNER_POSITION_PROPERTY_PATH);
                androidBannerIdProperty = base.ContainerProperty.FindPropertyRelative(ANDROID_BANNER_ID_PROPERTY_PATH);
                androidInterstitialIdProperty = base.ContainerProperty.FindPropertyRelative(ANDROID_INTERSTITIAL_ID_PROPERTY_PATH);
                androidRewardedVideoIdProperty = base.ContainerProperty.FindPropertyRelative(ANDROID_REWARDED_VIDEO_ID_PROPERTY_PATH);
                iOSBannerIdProperty = base.ContainerProperty.FindPropertyRelative(IOS_BANNER_ID_PROPERTY_PATH);
                iOSInterstitialIdProperty = base.ContainerProperty.FindPropertyRelative(IOS_INTERSTITIAL_ID_PROPERTY_PATH);
                iOSRewardedVideoIdProperty = base.ContainerProperty.FindPropertyRelative(IOS_REWARDED_VIDEO_ID_PROPERTY_PATH);

                // for app section
                fileLoaded = false;
                ourAndroidAppIdProperty = base.ContainerProperty.FindPropertyRelative(OUR_ANDROID_APP_ID_PROPERTY_PATH);
                ourIOSAppIdProperty = base.ContainerProperty.FindPropertyRelative(OUR_IOS_APP_ID_PROPERTY_PATH);
                LoadFile();

                
            }

            private void LoadFile()
            {
                settingsFile = AssetDatabase.LoadMainAssetAtPath(SETTINGS_FILE_PATH);

                if (settingsFile != null)
                {
                    serializedObject = new SerializedObject(settingsFile);
                    androidAppIdProperty = serializedObject.FindProperty(ANDROID_APP_ID_PROPERTY_PATH);
                    iOSAppIdProperty = serializedObject.FindProperty(IOS_APP_ID_PROPERTY_PATH);
                    fileLoaded = true;
                }
                else
                {
#if MODULE_ADMOB
                    GoogleMobileAds.Editor.GoogleMobileAdsSettingsEditor.OpenInspector(); // Creates addmob settings file
                    LoadFile();

                    if (fileLoaded)
                    {

                        if ((androidAppIdProperty.stringValue.Length == 0) && (ourAndroidAppIdProperty.stringValue.Length != 0))
                        {
                            androidAppIdProperty.stringValue = ourAndroidAppIdProperty.stringValue;
                        }
                        else if ((androidAppIdProperty.stringValue.Length != 0) && (ourAndroidAppIdProperty.stringValue.Length == 0))
                        {
                            ourAndroidAppIdProperty.stringValue = androidAppIdProperty.stringValue;
                        }

                        if ((iOSAppIdProperty.stringValue.Length == 0) && (ourIOSAppIdProperty.stringValue.Length != 0))
                        {
                            iOSAppIdProperty.stringValue = ourIOSAppIdProperty.stringValue;
                        }
                        else if ((iOSAppIdProperty.stringValue.Length != 0) && (ourIOSAppIdProperty.stringValue.Length == 0))
                        {
                            ourIOSAppIdProperty.stringValue = iOSAppIdProperty.stringValue;
                        }


                        serializedObject.ApplyModifiedProperties();
                        ourAndroidAppIdProperty.serializedObject.ApplyModifiedProperties();
                    }
#endif
                }
            }

            public override void DrawContainer()
            {
                EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

                base.ContainerProperty.isExpanded = EditorGUILayoutCustom.HeaderExpand(base.ContainerName, base.ContainerProperty.isExpanded, AdsDataEditor.arrowUpContent, AdsDataEditor.arrowDownContent);

                if (base.ContainerProperty.isExpanded)
                {
                    if (!base.IsDefineEnabled)
                    {
                        EditorGUILayoutCustom.HelpBox(base.ContainerName + " define isn't enabled!", "Enable", delegate
                        {
                            DefineManager.EnableDefine(base.DefineName);

                            InititalizeDefine();
                        });
                    }

                    DrawAppSection();
                    DrawAddUnitsSection();
                    DrawUsefulSection();

                    base.ContainerProperty.serializedObject.ApplyModifiedProperties();

                    if (fileLoaded)
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                EditorGUILayout.EndVertical();
            }
            private void DrawAppSection()
            {
                GUILayout.Space(8);
                EditorGUILayout.LabelField("App", EditorStylesExtended.label_medium_bold);
                EditorGUILayout.LabelField("Google Mobile Ads App ID", groupStyle);

                DrawIdProperty(ourAndroidAppIdProperty, TEST_APP_ID);
                DrawIdProperty(ourIOSAppIdProperty, TEST_APP_ID);

                if (fileLoaded)
                {
                    androidAppIdProperty.stringValue = ourAndroidAppIdProperty.stringValue;
                    iOSAppIdProperty.stringValue = ourIOSAppIdProperty.stringValue;
                }

                if (GUILayout.Button("Set test app id", EditorStylesExtended.button_01))
                {
                    ourAndroidAppIdProperty.stringValue = TEST_APP_ID;
                    ourIOSAppIdProperty.stringValue = TEST_APP_ID;
                    base.ContainerProperty.serializedObject.ApplyModifiedProperties();

                    if (fileLoaded)
                    {
                        androidAppIdProperty.stringValue = ourAndroidAppIdProperty.stringValue;
                        iOSAppIdProperty.stringValue = ourIOSAppIdProperty.stringValue;
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                GUILayout.Space(8);
            }

            private void DrawIdProperty(SerializedProperty property, string testValue)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(property);

                if (property.stringValue.Equals(testValue))
                {
                    EditorGUILayout.LabelField(testIdContent, GUILayout.MaxWidth(60));
                }

                EditorGUILayout.EndHorizontal();
            }

            private void DrawAddUnitsSection()
            {
                GUILayout.Space(8);
                EditorGUILayout.LabelField("Ad Units", EditorStylesExtended.label_medium_bold);
                EditorGUILayout.LabelField("Banner ID", groupStyle);


                DrawIdProperty(androidBannerIdProperty,AdMobData.ANDROID_BANNER_TEST_ID);
                DrawIdProperty(iOSBannerIdProperty, AdMobData.IOS_BANNER_TEST_ID);
                EditorGUILayout.PropertyField(bannerTypeProperty);
                EditorGUILayout.PropertyField(bannerPositionProperty);

                EditorGUILayout.LabelField("Interstitial ID", groupStyle);
                DrawIdProperty(androidInterstitialIdProperty,AdMobData.ANDROID_INTERSTITIAL_TEST_ID);
                DrawIdProperty(iOSInterstitialIdProperty, AdMobData.IOS_INTERSTITIAL_TEST_ID);

                EditorGUILayout.LabelField("Rewarded Video ID", groupStyle);
                DrawIdProperty(androidRewardedVideoIdProperty, AdMobData.ANDROID_REWARDED_VIDEO_TEST_ID);
                DrawIdProperty(iOSRewardedVideoIdProperty, AdMobData.IOS_REWARDED_VIDEO_TEST_ID);

                if(GUILayout.Button("Set test ids", EditorStylesExtended.button_01))
                {
                    androidBannerIdProperty.stringValue = AdMobData.ANDROID_BANNER_TEST_ID;
                    iOSBannerIdProperty.stringValue = AdMobData.IOS_BANNER_TEST_ID;
                    androidInterstitialIdProperty.stringValue = AdMobData.ANDROID_INTERSTITIAL_TEST_ID;
                    iOSInterstitialIdProperty.stringValue = AdMobData.IOS_INTERSTITIAL_TEST_ID;
                    androidRewardedVideoIdProperty.stringValue = AdMobData.ANDROID_REWARDED_VIDEO_TEST_ID;
                    iOSRewardedVideoIdProperty.stringValue = AdMobData.IOS_REWARDED_VIDEO_TEST_ID;
                }

                GUILayout.Space(8);
            }

            private void DrawUsefulSection()
            {
                GUILayout.Space(8);
                EditorGUILayout.LabelField("Useful", EditorStylesExtended.label_medium_bold);

                if (GUILayout.Button("Download AdMob plugin", EditorStylesExtended.button_01))
                {
                    Application.OpenURL(@"https://github.com/googleads/googleads-mobile-unity/releases");
                }

                if (GUILayout.Button("AdMob Dashboard", EditorStylesExtended.button_01))
                {
                    Application.OpenURL(@"https://apps.admob.com/v2/home");
                }

                if (GUILayout.Button("AdMob Quick Start Guide", EditorStylesExtended.button_01))
                {
                    Application.OpenURL(@"https://developers.google.com/admob/unity/start");
                }

                GUILayout.Space(8);
            }

            protected override void SpecialButtons()
            {
            }
        }

        private class UnityAdsContainer : AdsContainer
        {
            private SerializedProperty androidAppIDProperty;
            private SerializedProperty iOSAppIDProperty;

            private SerializedProperty androidBannerIDProperty;
            private SerializedProperty iOSBannerIDProperty;
            private SerializedProperty androidInterstitialIDProperty;
            private SerializedProperty iOSInterstitialIDProperty;
            private SerializedProperty androidRewardedVideoIDProperty;
            private SerializedProperty iOSRewardedVideoIDProperty;
            private SerializedProperty bannerPositionProperty;

            public UnityAdsContainer(string containerName, string propertyName, string defineName) : base(containerName, propertyName, defineName)
            {
            }

            public override void Initialize(SerializedObject serializedObject)
            {
                base.Initialize(serializedObject);

                androidAppIDProperty = ContainerProperty.FindPropertyRelative("androidAppID");
                iOSAppIDProperty = ContainerProperty.FindPropertyRelative("iOSAppID");

                androidBannerIDProperty = ContainerProperty.FindPropertyRelative("androidBannerID");
                iOSBannerIDProperty = ContainerProperty.FindPropertyRelative("iOSBannerID");
                androidInterstitialIDProperty = ContainerProperty.FindPropertyRelative("androidInterstitialID");
                iOSInterstitialIDProperty = ContainerProperty.FindPropertyRelative("iOSInterstitialID");
                androidRewardedVideoIDProperty = ContainerProperty.FindPropertyRelative("androidRewardedVideoID");
                iOSRewardedVideoIDProperty = ContainerProperty.FindPropertyRelative("iOSRewardedVideoID");
                bannerPositionProperty = ContainerProperty.FindPropertyRelative("bannerPosition");
            }

            public override void DrawContainer()
            {
                EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

                ContainerProperty.isExpanded = EditorGUILayoutCustom.HeaderExpand(ContainerName, ContainerProperty.isExpanded, AdsDataEditor.arrowUpContent, AdsDataEditor.arrowDownContent);

                if (ContainerProperty.isExpanded)
                {
                    if (!IsDefineEnabled)
                    {
                        EditorGUILayoutCustom.HelpBox(ContainerName + " define isn't enabled!", "Enable", delegate
                        {
                            DefineManager.EnableDefine(DefineName);

                            InititalizeDefine();
                        });
                    }

                    EditorGUILayout.PropertyField(androidAppIDProperty);
                    EditorGUILayout.PropertyField(iOSAppIDProperty);

                    EditorGUILayout.PropertyField(androidBannerIDProperty);
                    EditorGUILayout.PropertyField(iOSBannerIDProperty);

                    EditorGUILayout.PropertyField(androidInterstitialIDProperty);
                    EditorGUILayout.PropertyField(iOSInterstitialIDProperty);

                    EditorGUILayout.PropertyField(androidRewardedVideoIDProperty);
                    EditorGUILayout.PropertyField(iOSRewardedVideoIDProperty);

                    EditorGUILayout.PropertyField(bannerPositionProperty);

                    SpecialButtons();
                }

                EditorGUILayout.EndVertical();
            }

            protected override void SpecialButtons()
            {
                GUILayout.Space(8);

                if (GUILayout.Button("Unity Ads Dashboard", EditorStylesExtended.button_01))
                {
                    Application.OpenURL(@"https://operate.dashboard.unity3d.com");
                }

                if (GUILayout.Button("Unity Ads Quick Start Guide", EditorStylesExtended.button_01))
                {
                    Application.OpenURL(@"https://unityads.unity3d.com/help/monetization/getting-started");
                }

                GUILayout.Space(8);

                EditorGUILayout.HelpBox("Tested with Advertisement v4.4.2", MessageType.Info);
            }
        }

        private class DummyContainer : AdsContainer
        {
            public DummyContainer(string containerName, string propertyName, string defineName) : base(containerName, propertyName, defineName)
            {
            }

            protected override void SpecialButtons()
            {
            }
        }
    }
}