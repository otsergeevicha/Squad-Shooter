using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Watermelon
{
    [System.Serializable]
    public abstract class SetupButton
    {
        [System.NonSerialized]
        public GUIContent content;

        public string title;
        public string textureName;

        public virtual void Init()
        {
            content = new GUIContent(title, EditorStylesExtended.GetTexture(textureName, EditorStylesExtended.IconColor));
        }

        public void Draw(GUIStyle style)
        {
            if (GUILayout.Button(content, style, GUILayout.Height(50), GUILayout.ExpandWidth(true)))
            {
                OnClick();
            }
        }

        public abstract void OnClick();
    }

    [System.Serializable]
    public class SetupButtonWindow : SetupButton
    {
        public string menuPath;

        public override void OnClick()
        {
            EditorApplication.ExecuteMenuItem(menuPath);
        }
    }

    [System.Serializable]
    public class SetupButtonFolder : SetupButton
    {
        public string folderPath;

        private Object settingObject;

        public override void Init()
        {
            base.Init();

            settingObject = AssetDatabase.LoadAssetAtPath(folderPath, typeof(Object));
        }

        public override void OnClick()
        {
            Selection.activeObject = settingObject;
            EditorGUIUtility.PingObject(settingObject);
        }
    }

    [System.Serializable]
    public class SetupButtonFile : SetupButton
    {
        public string filePath;

        private Object settingObject;

        public override void Init()
        {
            base.Init();

            settingObject = AssetDatabase.LoadAssetAtPath(filePath, typeof(ScriptableObject));
        }

        public override void OnClick()
        {
            Selection.activeObject = settingObject;
            EditorGUIUtility.PingObject(settingObject);
        }
    }
}

// -----------------
// Setup Guide v 1.0.1
// -----------------