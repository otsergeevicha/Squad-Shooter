using UnityEngine;
using YG;
using UnityEngine.UI;
using TMPro;

public class TextTranslator : MonoBehaviour
{
    [SerializeField] private float _startDelay = 0.02f;
    [SerializeField,TextArea] private string _RussianText;
    [SerializeField,TextArea] private string _EnglishText;

    private bool _usingTextMeshPro;
    private Text _textUI;
    private TMP_Text _textTMP;

    private void Start()
    {
        TryGetComponent<TMP_Text>(out _textTMP);
        TryGetComponent<Text>(out _textUI);

        if (_textUI != null)
            _usingTextMeshPro = false;
        if (_textTMP != null)
            _usingTextMeshPro = true;

        Invoke(nameof(ChangeLanguage), _startDelay);
    }

    void ChangeLanguage()
    {
        string language = YandexGame.lang;

        if (language == "ru")
        {
            if (_usingTextMeshPro)
            {
                    _textTMP.text = _RussianText;
            }
            else
            {
                    _textUI.text = _EnglishText;
            }
        }
        else
        {
            if (_usingTextMeshPro)
            {
                    _textTMP.text = _RussianText;
            }
            else
            {
                    _textUI.text = _EnglishText;
            }
        }
    }
}
