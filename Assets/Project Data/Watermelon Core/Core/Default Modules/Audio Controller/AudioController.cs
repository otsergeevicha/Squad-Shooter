using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Watermelon
{
    [RequireSetting("Vibration", PrefsSettings.FieldType.Bool)]
    [RequireSetting("Volume", PrefsSettings.FieldType.Float)]
    public class AudioController
    {
        private static AudioController audioController;
        private FieldInfo[] fields;
        private const int AUDIO_SOURCES_AMOUNT = 4;

        private GameObject targetGameObject;

        private List<AudioSource> audioSources = new List<AudioSource>();

        private List<AudioSource> activeSounds = new List<AudioSource>();
        private List<AudioSource> activeMusic = new List<AudioSource>();

        private List<AudioSource> customSources = new List<AudioSource>();
        private List<AudioCaseCustom> activeCustomSourcesCases = new List<AudioCaseCustom>();

        private static bool vibrationState;
        private static float volume;

        private static AudioClip[] musicAudioClips;
        public static AudioClip[] MusicAudioClips => musicAudioClips;

        private static Sounds sounds;
        public static Sounds Sounds => sounds;

        private static Music music;
        public static Music Music => music;

        private static Vibrations vibrations;
        public static Vibrations Vibrations => vibrations;

        public static OnVolumeChangedCallback OnVolumeChanged;
        public static OnVibrationChangedCallback OnVibrationChanged;

        private static AudioListener audioListener;
        public static AudioListener AudioListener => audioListener;

        public void Init(AudioSettings settings, GameObject targetGameObject)
        {
            if(audioController != null)
            {
                Debug.Log("[Audio Controller]: Module already exists!");

                return;
            }

            if (settings == null)
            {
                Debug.LogError("[AudioController]: Audio Settings is NULL! Please assign audio settings scriptable on Audio Controller script.");

                return;
            }

            this.targetGameObject = targetGameObject;

            audioController = this;
            fields = typeof(Music).GetFields();
            musicAudioClips = new AudioClip[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                musicAudioClips[i] = fields[i].GetValue(settings.Music) as AudioClip;
            }

            music = settings.Music;
            sounds = settings.Sounds;
            vibrations = settings.Vibrations;

            //Create audio source objects
            audioSources.Clear();
            for (int i = 0; i < AUDIO_SOURCES_AMOUNT; i++)
            {
                audioSources.Add(CreateAudioSourceObject(false));
            }

            // Load default states
            vibrationState = PrefsSettings.GetBool(PrefsSettings.Key.Vibration);
            volume = PrefsSettings.GetFloat(PrefsSettings.Key.Volume);
        }

        public static void CreateAudioListener()
        {
            if (audioListener != null)
                return;

            // Create game object for listener
            GameObject listenerObject = new GameObject("[AUDIO LISTENER]");
            listenerObject.transform.position = Vector3.zero;

            // Mark as non-destroyable
            GameObject.DontDestroyOnLoad(listenerObject);

            // Add listener component to created object
            audioListener = listenerObject.AddComponent<AudioListener>();
        }

        public static bool IsVibrationModuleEnabled()
        {
            return PrefsSettings.GetBool(PrefsSettings.Key.Vibration);
        }

        public static bool IsAudioModuleEnabled()
        {
            return (!((PrefsSettings.GetFloat(PrefsSettings.Key.Volume) - 0.00005f) < 0)); 
        }

        public static void PlayRandomMusic()
        {
            if(!musicAudioClips.IsNullOrEmpty())
                PlayMusic(musicAudioClips.GetRandomItem());
        }

        /// <summary>
        /// Stop all active streams
        /// </summary>
        public static void ReleaseStreams()
        {
            ReleaseMusic();
            ReleaseSounds();
            ReleaseCustomStreams();
        }

        /// <summary>
        /// Releasing all active music.
        /// </summary>
        public static void ReleaseMusic()
        {
            int activeMusicCount = audioController.activeMusic.Count - 1;
            for (int i = activeMusicCount; i >= 0; i--)
            {
                audioController.activeMusic[i].Stop();
                audioController.activeMusic[i].clip = null;
                audioController.activeMusic.RemoveAt(i);
            }
        }

        /// <summary>
        /// Releasing all active sounds.
        /// </summary>
        public static void ReleaseSounds()
        {
            int activeStreamsCount = audioController.activeSounds.Count - 1;
            for (int i = activeStreamsCount; i >= 0; i--)
            {
                audioController.activeSounds[i].Stop();
                audioController.activeSounds[i].clip = null;
                audioController.activeSounds.RemoveAt(i);
            }
        }

        /// <summary>
        /// Releasing all active custom sources.
        /// </summary>
        public static void ReleaseCustomStreams()
        {
            int activeStreamsCount = audioController.activeCustomSourcesCases.Count - 1;
            for (int i = activeStreamsCount; i >= 0; i--)
            {
                if (audioController.activeCustomSourcesCases[i].autoRelease)
                {
                    AudioSource source = audioController.activeCustomSourcesCases[i].source;
                    audioController.activeCustomSourcesCases[i].source.Stop();
                    audioController.activeCustomSourcesCases[i].source.clip = null;
                    audioController.activeCustomSourcesCases.RemoveAt(i);
                    audioController.customSources.Add(source);
                }
            }
        }

        public static void StopStream(AudioCase audioCase, float fadeTime = 0)
        {
            if (audioCase.type == AudioType.Sound)
            {
                audioController.StopSound(audioCase.source, fadeTime);
            }
            else
            {
                audioController.StopMusic(audioCase.source, fadeTime);
            }
        }

        public static void StopStream(AudioCaseCustom audioCase, float fadeTime = 0)
        {
            ReleaseCustomSource(audioCase, fadeTime);
        }

        private void StopSound(AudioSource source, float fadeTime = 0)
        {
            int streamID = activeSounds.FindIndex(x => x == source);
            if (streamID != -1)
            {
                if (fadeTime == 0)
                {
                    activeSounds[streamID].Stop();
                    activeSounds[streamID].clip = null;
                    activeSounds.RemoveAt(streamID);
                }
                else
                {
                    activeSounds[streamID].DOVolume(0f, fadeTime).OnComplete(() =>
                    {
                        activeSounds.Remove(source);
                        source.Stop();
                    });
                }
            }
        }

        private void StopMusic(AudioSource source, float fadeTime = 0)
        {
            int streamID = activeMusic.FindIndex(x => x == source);
            if (streamID != -1)
            {
                if (fadeTime == 0)
                {
                    activeMusic[streamID].Stop();
                    activeMusic[streamID].clip = null;
                    activeMusic.RemoveAt(streamID);
                }
                else
                {
                    activeMusic[streamID].DOVolume(0f, fadeTime).OnComplete(() =>
                    {
                        activeMusic.Remove(source);
                        source.Stop();
                    });
                }
            }
        }

        private static void AddMusic(AudioSource source)
        {
            if (!audioController.activeMusic.Contains(source))
            {
                audioController.activeMusic.Add(source);
            }
        }

        private static void AddSound(AudioSource source)
        {
            if (!audioController.activeSounds.Contains(source))
            {
                audioController.activeSounds.Add(source);
            }
        }

        public static void PlayMusic(AudioClip clip, float volumePercentage = 1.0f)
        {
            if (clip == null)
                Debug.LogError("[AudioController]: Audio clip is null");

            AudioSource source = audioController.GetAudioSource();

            SetSourceDefaultSettings(source, AudioType.Music);

            source.clip = clip;
            source.volume *= volumePercentage;
            source.Play();

            AddMusic(source);
        }

        public static AudioCase PlaySmartMusic(AudioClip clip, float volumePercentage = 1.0f, float pitch = 1.0f)
        {
            if (clip == null)
                Debug.LogError("[AudioController]: Audio clip is null");

            AudioSource source = audioController.GetAudioSource();

            SetSourceDefaultSettings(source, AudioType.Music);

            source.clip = clip;
            source.volume *= volumePercentage;
            source.pitch = pitch;

            AudioCase audioCase = new AudioCase(clip, source, AudioType.Music);

            audioCase.Play();

            AddMusic(source);

            return audioCase;
        }


        public static void PlaySound(AudioClip clip, float volumePercentage = 1.0f, float pitch = 1.0f)
        {
            if (clip == null)
                Debug.LogError("[AudioController]: Audio clip is null");

            AudioSource source = audioController.GetAudioSource();

            SetSourceDefaultSettings(source, AudioType.Sound);

            source.clip = clip;
            source.volume *= volumePercentage;
            source.pitch = pitch;
            source.Play();

            AddSound(source);
        }

        public static AudioCase PlaySmartSound(AudioClip clip, float volumePercentage = 1.0f, float pitch = 1.0f)
        {
            if (clip == null)
                Debug.LogError("[AudioController]: Audio clip is null");

            AudioSource source = audioController.GetAudioSource();

            SetSourceDefaultSettings(source, AudioType.Sound);

            source.clip = clip;
            source.volume *= volumePercentage;
            source.pitch = pitch;

            AudioCase audioCase = new AudioCase(clip, source, AudioType.Sound);
            audioCase.Play();

            AddSound(source);

            return audioCase;
        }

        public static AudioCaseCustom GetCustomSource(bool autoRelease, AudioType audioType)
        {
            AudioSource source = null;

            if (!audioController.customSources.IsNullOrEmpty())
            {
                source = audioController.customSources[0];
                audioController.customSources.RemoveAt(0);
            }
            else
            {
                source = audioController.CreateAudioSourceObject(true);
            }

            SetSourceDefaultSettings(source, audioType);

            AudioCaseCustom audioCase = new AudioCaseCustom(null, source, audioType, autoRelease);

            audioController.activeCustomSourcesCases.Add(audioCase);

            return audioCase;
        }

        public static void ReleaseCustomSource(AudioCaseCustom audioCase, float fadeTime = 0)
        {
            int streamID = audioController.activeCustomSourcesCases.FindIndex(x => x.source == audioCase.source);
            if (streamID != -1)
            {
                if (fadeTime == 0)
                {
                    audioController.activeCustomSourcesCases[streamID].source.Stop();
                    audioController.activeCustomSourcesCases[streamID].source.clip = null;
                    audioController.activeCustomSourcesCases.RemoveAt(streamID);
                    audioController.customSources.Add(audioCase.source);
                }
                else
                {
                    audioController.activeCustomSourcesCases[streamID].source.DOVolume(0f, fadeTime).OnComplete(() =>
                    {
                        audioController.activeCustomSourcesCases.Remove(audioCase);
                        audioCase.source.Stop();
                        audioController.customSources.Add(audioCase.source);
                    });
                }
            }
        }

        private AudioSource GetAudioSource()
        {
            int sourcesAmount = audioSources.Count;
            for (int i = 0; i < sourcesAmount; i++)
            {
                if (!audioSources[i].isPlaying)
                {
                    return audioSources[i];
                }
            }

            AudioSource createdSource = CreateAudioSourceObject(false);
            audioSources.Add(createdSource);

            return createdSource;
        }

        private AudioSource CreateAudioSourceObject(bool isCustom)
        {
            AudioSource audioSource = targetGameObject.AddComponent<AudioSource>();
            SetSourceDefaultSettings(audioSource);

            return audioSource;
        }

        private void SetVolumeForAudioSources(float volume)
        {
            // setuping all active sound sources
            int activeSoundSourcesCount = activeSounds.Count;
            for (int i = 0; i < activeSoundSourcesCount; i++)
            {
                activeSounds[i].volume = volume;
            }

            activeSoundSourcesCount = activeMusic.Count;
            for (int i = 0; i < activeSoundSourcesCount; i++)
            {
                activeMusic[i].volume = volume;
            }

            // setuping all custom sound sources
            activeSoundSourcesCount = activeCustomSourcesCases.Count;
            for (int i = 0; i < activeSoundSourcesCount; i++)
            {
                activeCustomSourcesCases[i].source.volume = volume;
            }
        }
        
        public static void SetVolume(float volume)
        {
            AudioController.volume = volume;

            PrefsSettings.SetFloat(PrefsSettings.Key.Volume, volume);

            audioController.SetVolumeForAudioSources(volume);

            OnVolumeChanged?.Invoke(volume);
        }

        public static float GetVolume()
        {
            return volume;
        }

        public static bool IsVibrationEnabled()
        {
            return vibrationState;
        }

        public static void SetVibrationState(bool vibrationState)
        {
            AudioController.vibrationState = vibrationState;

            PrefsSettings.SetBool(PrefsSettings.Key.Vibration, vibrationState);

            OnVibrationChanged?.Invoke(vibrationState);
        }

        public static void SetSourceDefaultSettings(AudioSource source, AudioType type = AudioType.Sound)
        {
            float volume = PrefsSettings.GetFloat(PrefsSettings.Key.Volume);

            if (type == AudioType.Sound)
            {
                source.loop = false;
            }
            else if (type == AudioType.Music)
            {
                source.loop = true;
            }

            source.clip = null;

            source.volume = volume;
            source.pitch = 1.0f;
            source.spatialBlend = 0; // 2D Sound
            source.mute = false;
            source.playOnAwake = false;
            source.outputAudioMixerGroup = null;
        }

        public enum AudioType
        {
            Music = 0,
            Sound = 1
        }

        public delegate void OnVolumeChangedCallback(float volume);
        public delegate void OnVibrationChangedCallback(bool state);
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