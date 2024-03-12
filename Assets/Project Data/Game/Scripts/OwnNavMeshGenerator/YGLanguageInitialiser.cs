using UnityEngine;
using YG;

public class YGLanguageInitialiser : MonoBehaviour
{
    public static YGLanguageInitialiser Instance;
    public string language;
    private void Start()
    {
        Instance = this;
        language = YandexGame.lang;
    }
}
