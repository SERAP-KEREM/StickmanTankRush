using UnityEngine;
using System.Collections.Generic;
using SerapKeremGameTools._Game._objectPool;
using SerapKeremGameTools._Game._Singleton;

namespace SerapKeremGameTools._Game._AudioSystem
{
    /// <summary>
    /// Manages audio playback, pooling for AudioPlayers, and provides playback controls.
    /// This class ensures that audio is efficiently managed and reused in the game.
    /// </summary>
    public class AudioManager : MonoSingleton<AudioManager>
    {
        [Header("Audio Clips List")]
        [Tooltip("A list of all available audio clips that can be played.")]
        [SerializeField]
        private List<Audio> audioClips = new List<Audio>(); // List to store audio clips

        [Header("AudioPlayer Prefab")]
        [Tooltip("The AudioPlayer prefab used to play audio.")]
        [SerializeField]
        private AudioPlayer audioPlayerPrefab; // Reference to the AudioPlayer prefab

        private ObjectPool<AudioPlayer> audioPlayerPool; // Object pool for AudioPlayers
        [SerializeField]
        [Tooltip("The maximum number of AudioPlayers that can be in the pool.")]
        private int poolSize = 10; // Maximum number of AudioPlayers in the pool

        // Holds the currently playing audio name
        private string currentAudio = string.Empty;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource soundSource;

        [Header("Volume Settings")]
        [SerializeField] private float defaultSoundVolume = 1f;
        [SerializeField] private float defaultMusicVolume = 1f;

        private float soundVolume;
        private float musicVolume;

        public float SoundVolume => soundVolume;
        public float MusicVolume => musicVolume;

        /// <summary>
        /// Initializes the AudioManager instance and sets up the audio pool.
        /// Ensures only one instance of AudioManager exists and loads audio clips.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Create the audio player pool with a capacity of poolSize
            InitializeAudioPlayerPool();

            // Load the audio clips from Resources folder
            LoadAudioClips();
            InitializeVolumes();
        }
        private void InitializeVolumes()
        {
            soundVolume = PlayerPrefs.GetFloat("SoundVolume", defaultSoundVolume);
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);

            ApplyVolumeSettings();
        }
        private void ApplyVolumeSettings()
        {
            if (soundSource != null)
                soundSource.volume = soundVolume;

            if (musicSource != null)
                musicSource.volume = musicVolume;
        }

        public void PlaySound(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (soundSource != null && clip != null)
            {
                soundSource.PlayOneShot(clip, soundVolume * volumeMultiplier);
            }
        }

        public void PlayMusic(AudioClip clip)
        {
            if (musicSource != null && clip != null)
            {
                musicSource.clip = clip;
                musicSource.volume = musicVolume;
                musicSource.loop = true;
                musicSource.Play();
            }
        }

        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        private void OnApplicationQuit()
        {
            PlayerPrefs.SetFloat("SoundVolume", soundVolume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.Save();
        }
        public void SetSoundVolume(float volume)
        {
            soundVolume = Mathf.Clamp01(volume);

            if (soundSource != null)
                soundSource.volume = soundVolume;

            PlayerPrefs.SetFloat("SoundVolume", soundVolume);
            PlayerPrefs.Save();

            Debug.Log($"[AudioManager] Sound volume set to: {soundVolume}");
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);

            if (musicSource != null)
                musicSource.volume = musicVolume;

            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.Save();

        }
        /// <summary>
        /// Initializes the ObjectPool for AudioPlayers with a specified pool size.
        /// </summary>
        private void InitializeAudioPlayerPool()
        {
            audioPlayerPool = new ObjectPool<AudioPlayer>(audioPlayerPrefab, poolSize, transform);
        }

        /// <summary>
        /// Loads all audio clips from the Resources/Audio folder.
        /// </summary>
        private void LoadAudioClips()
        {
            // Load all AudioClips from Resources/Audio
            AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio");
            foreach (var clip in clips)
            {
                Audio newAudio = new Audio()
                {
                    Name = clip.name,
                    Clip = clip,
                    Volume = 1f,
                    Pitch = 1f,
                    Loop = false
                };
                audioClips.Add(newAudio);
            }
        }

        /// <summary>
        /// Plays an audio clip by its name from the audioClips list.
        /// If the audio is already playing, it won't play again.
        /// </summary>
        /// <param name="audioName">The name of the audio clip to play.</param>
        public void PlayAudio(string audioName)
        {
            // Find the audio clip by name
            Audio audio = audioClips.Find(a => a.Name == audioName);
            if (audio != null)
            {
                // Check if the audio is already playing
                if (currentAudio == audioName)
                {
#if UNITY_EDITOR
                    Debug.Log($"Audio {audioName} is already playing.");
#endif
                    return;
                }

                // Get an AudioPlayer from the pool and play the audio
                AudioPlayer audioPlayer = audioPlayerPool.GetObject();
                audioPlayer.PlayAudio(audio);

                // Set the current playing audio to this one
                currentAudio = audioName;
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Audio not found: {audioName}");
#endif
            }
        }

        /// <summary>
        /// Pauses all active AudioSources in the scene.
        /// This is useful for pausing all audio during a pause menu or when switching scenes.
        /// </summary>
        public void PauseAllAudio()
        {
            AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
            foreach (var source in audioSources)
            {
                if (source.isPlaying)
                {
                    source.Pause();
                }
            }
        }

        /// <summary>
        /// Resumes all paused AudioSources in the scene.
        /// This resumes the playback of any paused audio.
        /// </summary>
        public void ResumeAllAudio()
        {
            AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
            foreach (var source in audioSources)
            {
                if (!source.isPlaying)
                {
                    source.UnPause();
                }
            }
        }

        /// <summary>
        /// Checks if the audio clip with the given name is currently playing.
        /// </summary>
        /// <param name="audioName">The name of the audio clip to check.</param>
        /// <returns>True if the audio is playing, otherwise false.</returns>
        public bool IsPlaying(string audioName)
        {
            // Return true if the given audio is the one currently playing
            return currentAudio == audioName;
        }

        /// <summary>
        /// Returns the AudioPlayer to the pool after it has finished playing.
        /// </summary>
        /// <param name="audioPlayer">The AudioPlayer to return to the pool.</param>
        public void ReturnAudioPlayerToPool(AudioPlayer audioPlayer)
        {
            audioPlayerPool.ReturnObject(audioPlayer);
        }
    }
}
