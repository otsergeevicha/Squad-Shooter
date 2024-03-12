using UnityEngine;

namespace Watermelon
{
    public static class CustomMusicController
    {
        private static AudioCaseCustom musicAudioCase;
        private static TweenCase fadeTweenCase;

        public static void Initialise(AudioClip defaultMusic)
        {
            musicAudioCase = AudioController.GetCustomSource(true, AudioController.AudioType.Music);

            PlayMusic(defaultMusic);
        }

        public static void PlayMusic(AudioClip audioClip)
        {
            musicAudioCase.source.clip = audioClip;
            musicAudioCase.Play();
        }

        public static void ToggleMusic(AudioClip audioClip, float fadeInTime, float fadeOutTime)
        {
            if (musicAudioCase.source != null && musicAudioCase.source.isPlaying && musicAudioCase.source.clip == audioClip) return;

            fadeTweenCase.KillActive();

            fadeTweenCase = Tween.DoFloat(musicAudioCase.source.volume, 0.0f, fadeInTime, (value) =>
            {
                musicAudioCase.source.volume = value;
            }).OnComplete(() =>
            {
                musicAudioCase.source.clip = audioClip;
                musicAudioCase.source.Play();

                fadeTweenCase = Tween.DoFloat(0.0f, 1.0f, fadeInTime, (value) =>
                {
                    musicAudioCase.source.volume = value;
                });
            });
        }
    }
}

// -----------------
// Audio Controller v 0.3.3
// -----------------

// Changelog
// v 0.3.2
// • Added audio listener creation method
// v 0.3.2
// • Added volume float
// • AudioSettings variable removed (now sounds, music and vibrations can be reached directly)
// v 0.3.1
// • Added OnVolumeChanged callback
// • Renamed AudioSettings to Settings
// v 0.3
// • Added IsAudioModuleEnabled method
// • Added IsVibrationModuleEnabled method
// • Removed VibrationToggleButton class
// v 0.2
// • Removed MODULE_VIBRATION
// v 0.1
// • Added basic version
// • Added support of new initialization
// • Music and Sound volume is combined