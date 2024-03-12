using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [InitializeOnLoad]
    public static class GameLoadingEditor
    {
        private static readonly string[] INIT_SCENES = new string[]
        {
            "Init"
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void LoadMain()
        {
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (currentScene != null)
            {
                if (!IsInitScene(currentScene.name))
                {
                    Initialiser initialiser = Object.FindObjectOfType<Initialiser>();
                    if (initialiser == null)
                    {
                        GameObject initialiserPrefab = EditorUtils.GetAsset<GameObject>("Initialiser");
                        if (initialiserPrefab != null)
                        {
                            GameObject initialiserObject = Object.Instantiate(initialiserPrefab);

                            initialiser = initialiserObject.GetComponent<Initialiser>();
                            initialiser.Awake();
                            initialiser.Initialise(false);
                        }
                        else
                        {
                            Debug.LogError("[Game]: Initialiser prefab is missing!");
                        }
                    }
                }
            }
        }

        private static bool IsInitScene(string sceneName)
        {
            for (int i = 0; i < INIT_SCENES.Length; i++)
            {
                if (INIT_SCENES[i] == sceneName)
                    return true;
            }

            return false;
        }
    }
}