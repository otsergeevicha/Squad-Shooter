using UnityEngine;
using UnityEngine.Events;
using YG;

public class RewardChecker : MonoBehaviour
{
    public static RewardChecker Instance;

    private void Start()
    {
        Instance = this;
    }
    public UnityEvent unityEvent;
    private void OnEnable() => YandexGame.RewardVideoEvent += Rewarded;
    private void OnDisable() => YandexGame.RewardVideoEvent -= Rewarded;
    void Rewarded(int id)
    {
        if (id == 0)
            ADLook(0);
    }

    void ADLook(int value)
    {
        if (value == 0)
        {
           unityEvent.Invoke();
        }
    }
}
