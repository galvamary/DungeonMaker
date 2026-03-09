using UnityEngine;

/// <summary>
/// Manages audio playback for sound effects and music
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;

    [Header("Background Music")]
    [SerializeField] private AudioClip preparationMusic;
    [SerializeField] private AudioClip explorationMusic;
    [SerializeField] private float bgmVolume = 0.5f;
    [SerializeField] private float fadeDuration = 1.5f;

    private bool isExplorationMusic = false;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
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

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
        }
    }

    private void Start()
    {
        PlayBGM();
    }

    /// <summary>
    /// Plays a sound effect once
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Starts playing preparation background music
    /// </summary>
    private void PlayBGM()
    {
        if (preparationMusic != null && bgmSource != null)
        {
            bgmSource.clip = preparationMusic;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
            Debug.Log("Preparation music started playing");
        }
    }

    /// <summary>
    /// Stops background music
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    /// <summary>
    /// Sets background music volume
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);

        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
    }

    /// <summary>
    /// Sets sound effects volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }

    /// <summary>
    /// Switches to exploration music with crossfade
    /// </summary>
    public void StartExplorationMusic()
    {
        if (explorationMusic == null || isExplorationMusic)
            return;

        isExplorationMusic = true;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(CrossfadeTo(explorationMusic));
    }

    /// <summary>
    /// Returns to preparation music with crossfade
    /// </summary>
    public void StopExplorationMusic()
    {
        if (!isExplorationMusic)
            return;

        isExplorationMusic = false;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(CrossfadeTo(preparationMusic));
    }

    /// <summary>
    /// Crossfades: fade out → swap clip → fade in
    /// </summary>
    private System.Collections.IEnumerator CrossfadeTo(AudioClip newClip)
    {
        if (bgmSource == null || newClip == null)
            yield break;

        float halfDuration = fadeDuration / 2f;

        // Fade out
        float elapsed = 0f;
        float startVolume = bgmSource.volume;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        // Swap clip
        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.volume = 0f;
        bgmSource.Play();

        // Fade in
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            bgmSource.volume = Mathf.Lerp(0f, bgmVolume, t);
            yield return null;
        }

        bgmSource.volume = bgmVolume;
        fadeCoroutine = null;
    }
}
