using UnityEngine;
using UnityEngine.UI;
using YG;
public class CooldownReward : MonoBehaviour
{
    private float minusTimer;

    public float cooldown;

    public GameObject _textCanvas;
    public Text _text;
    public float coolDownAmount = 30f;
    private bool canTime = false;

    private void Start()
    {
        Invoke(nameof(ViewAD), coolDownAmount);
    }
    private void Update()
    {
        if (canTime)
        {
            minusTimer -= Time.deltaTime;
            _text.text = Mathf.RoundToInt(minusTimer).ToString();
            if (minusTimer <= 0)
            {
                NewViewAD();
                canTime = false;
                _textCanvas.SetActive(false);
            }
        }
    }

    void NewViewAD()
    {
        YandexGame.Instance._FullscreenShow();
    }

    void ViewAD()
    {
        _textCanvas.SetActive(true);
        minusTimer = cooldown;
        canTime = true;
        Invoke(nameof(ViewAD), coolDownAmount+cooldown);
    }
}
