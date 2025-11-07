using System.Collections;
using UnityEngine;

namespace KeyOfHistory.Manager
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Music")]
        [SerializeField] private AudioSource MusicSource;
        [SerializeField] private float MusicFadeInDuration = 5f;
        [SerializeField] private float TargetMusicVolume = 0.5f;
        
        [Header("Voice")]
        [SerializeField] private AudioSource VoiceSource;
        
        [Header("SFX")]
        [SerializeField] private AudioSource SFXSource;
        
        // Singleton
        public static AudioManager Instance { get; private set; }
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            // Start music with fade in
            if (MusicSource != null)
            {
                MusicSource.volume = 0f; // Start at 0
                MusicSource.Play();
                StartCoroutine(FadeInMusic());
            }
        }
        
        private IEnumerator FadeInMusic()
        {
            float elapsed = 0f;
            
            while (elapsed < MusicFadeInDuration)
            {
                elapsed += Time.deltaTime;
                MusicSource.volume = Mathf.Lerp(0f, TargetMusicVolume, elapsed / MusicFadeInDuration);
                yield return null;
            }
            
            MusicSource.volume = TargetMusicVolume;
        }
        
        // Fade out music (for transitions/endings)
        public void FadeOutMusic(float duration = 2f)
        {
            StartCoroutine(FadeOutMusicCoroutine(duration));
        }
        
        private IEnumerator FadeOutMusicCoroutine(float duration)
        {
            float startVolume = MusicSource.volume;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                MusicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }
            
            MusicSource.volume = 0f;
            MusicSource.Stop();
        }
        
        // Public methods for other scripts to use
        public void PlayVoice(AudioClip clip)
        {
            if (VoiceSource != null && clip != null)
            {
                VoiceSource.clip = clip;
                VoiceSource.Play();
            }
        }
        
        public void PlaySFX(AudioClip clip)
        {
            if (SFXSource != null && clip != null)
            {
                SFXSource.PlayOneShot(clip);
            }
        }
        
        public void SetMusicVolume(float volume)
        {
            if (MusicSource != null)
            {
                TargetMusicVolume = volume;
                MusicSource.volume = volume;
            }
        }
        
        // Getters for other scripts to access audio sources
        public AudioSource GetVoiceSource() => VoiceSource;
        public AudioSource GetSFXSource() => SFXSource;
        public AudioSource GetMusicSource() => MusicSource;
    }
}