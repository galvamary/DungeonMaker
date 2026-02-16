using UnityEngine;

/// <summary>
/// Manages audio playback for sound effects and music
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource normalBgmSource;
    [SerializeField] private AudioSource battleBgmSource;

    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip battleMusic;
    [SerializeField] private float bgmVolume = 0.5f;
    [SerializeField] private float fadeDuration = 1.5f;

    private bool isBattleMusic = false;
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

        // Create SFX AudioSource if not assigned
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        // Create Normal BGM AudioSource if not assigned
        if (normalBgmSource == null)
        {
            normalBgmSource = gameObject.AddComponent<AudioSource>();
            normalBgmSource.playOnAwake = false;
            normalBgmSource.loop = true;  // Loop background music
        }

        // Create Battle BGM AudioSource if not assigned
        if (battleBgmSource == null)
        {
            battleBgmSource = gameObject.AddComponent<AudioSource>();
            battleBgmSource.playOnAwake = false;
            battleBgmSource.loop = true;  // Loop battle music
            battleBgmSource.volume = 0f;  // Start silent
        }
    }

    private void Start()
    {
        // Start playing background music
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
    /// Plays a sound effect with custom volume
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    /// <summary>
    /// Starts playing background music
    /// </summary>
    private void PlayBGM()
    {
        if (backgroundMusic != null && normalBgmSource != null)
        {
            normalBgmSource.clip = backgroundMusic;
            normalBgmSource.volume = bgmVolume;
            normalBgmSource.Play();
            Debug.Log("Background music started playing");
        }

        // Prepare battle music (silent)
        if (battleMusic != null && battleBgmSource != null)
        {
            battleBgmSource.clip = battleMusic;
            battleBgmSource.volume = 0f;
            battleBgmSource.Play();  // Play but silent, so it's ready
        }
    }

    /// <summary>
    /// Stops background music
    /// </summary>
    public void StopBGM()
    {
        if (normalBgmSource != null)
        {
            normalBgmSource.Stop();
        }
        if (battleBgmSource != null)
        {
            battleBgmSource.Stop();
        }
    }

    /// <summary>
    /// Sets background music volume
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);

        // Update current playing BGM
        if (isBattleMusic && battleBgmSource != null)
        {
            battleBgmSource.volume = bgmVolume;
        }
        else if (!isBattleMusic && normalBgmSource != null)
        {
            normalBgmSource.volume = bgmVolume;
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
    /// Switches to battle music with crossfade
    /// </summary>
    public void StartBattleMusic()
    {
        if (battleMusic == null || isBattleMusic)
            return;

        isBattleMusic = true;

        // Stop any ongoing fade
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(CrossfadeToBattle());
    }

    /// <summary>
    /// Returns to normal background music with crossfade
    /// </summary>
    public void StopBattleMusic()
    {
        if (!isBattleMusic)
            return;

        isBattleMusic = false;

        // Stop any ongoing fade
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(CrossfadeToNormal());
    }

    /// <summary>
    /// Crossfades from normal BGM to battle music
    /// </summary>
    private System.Collections.IEnumerator CrossfadeToBattle()
    {
        if (normalBgmSource == null || battleBgmSource == null || battleMusic == null)
            yield break;

        float elapsed = 0f;
        float startVolumeNormal = normalBgmSource.volume;
        float startVolumeBattle = battleBgmSource.volume;

        // Crossfade: normal fades out, battle fades in
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            normalBgmSource.volume = Mathf.Lerp(startVolumeNormal, 0f, t);
            battleBgmSource.volume = Mathf.Lerp(startVolumeBattle, bgmVolume, t);

            yield return null;
        }

        normalBgmSource.volume = 0f;
        battleBgmSource.volume = bgmVolume;
        fadeCoroutine = null;
    }

    /// <summary>
    /// Crossfades from battle music back to normal BGM
    /// </summary>
    private System.Collections.IEnumerator CrossfadeToNormal()
    {
        if (normalBgmSource == null || battleBgmSource == null || backgroundMusic == null)
            yield break;

        float elapsed = 0f;
        float startVolumeNormal = normalBgmSource.volume;
        float startVolumeBattle = battleBgmSource.volume;

        // Crossfade: battle fades out, normal fades in
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            normalBgmSource.volume = Mathf.Lerp(startVolumeNormal, bgmVolume, t);
            battleBgmSource.volume = Mathf.Lerp(startVolumeBattle, 0f, t);

            yield return null;
        }

        normalBgmSource.volume = bgmVolume;
        battleBgmSource.volume = 0f;
        fadeCoroutine = null;
    }
}
