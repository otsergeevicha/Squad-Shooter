using Cinemachine;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class VirtualCameraCase
    {
        [SerializeField] CameraType cameraType;
        public CameraType CameraType => cameraType;

        [SerializeField] CinemachineVirtualCamera virtualCamera;
        public CinemachineVirtualCamera VirtualCamera => virtualCamera;

        private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;
        public CinemachineBasicMultiChannelPerlin CinemachineBasicMultiChannelPerlin => cinemachineBasicMultiChannelPerlin;

        private TweenCase shakeTweenCase;

        public void Initialise()
        {
            cinemachineBasicMultiChannelPerlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        public void Shake(float fadeInTime, float fadeOutTime, float duration, float gain)
        {
            if (shakeTweenCase != null && !shakeTweenCase.isCompleted)
                shakeTweenCase.Kill();

            gain *= 2;

            shakeTweenCase = Tween.DoFloat(0.0f, gain, fadeInTime, (float fadeInValue) =>
            {
                cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = fadeInValue;
            }).OnComplete(delegate
            {
                shakeTweenCase = Tween.DelayedCall(duration, delegate
                {
                    shakeTweenCase = Tween.DoFloat(gain, 0.0f, fadeOutTime, (float fadeOutValue) =>
                    {
                        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = fadeOutValue;
                    });
                });
            });
        }
    }
}