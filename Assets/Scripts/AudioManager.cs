using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource drawingSource;

    [Header("Mixer Settings")]
    public AudioMixer mainMixer;
    private const string MASTER_KEY = "MasterVol";
    private const string MUSIC_KEY = "MusicVol";
    private const string SFX_KEY = "SFXVol";

    [Header("Clips")]
    public AudioClip backgroundMusic;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip coinSound;
    public AudioClip drawingSound;

    [Header("Scene Names")]
    public string winSceneName = "you_win_scene";
    public string loseSceneName = "game_over_scene";
    public string gameSceneName = "domas_scene"; // Corrected to match user scene name

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Load and apply saved volumes
        SetMasterVolume(PlayerPrefs.GetFloat(MASTER_KEY, 0.75f));
        SetMusicVolume(PlayerPrefs.GetFloat(MUSIC_KEY, 0.75f));
        SetSFXVolume(PlayerPrefs.GetFloat(SFX_KEY, 0.75f));
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void SetMasterVolume(float volume)
    {
        ApplyVolumeToMixer(MASTER_KEY, volume);
        PlayerPrefs.SetFloat(MASTER_KEY, volume);
    }

    public void SetMusicVolume(float volume)
    {
        ApplyVolumeToMixer(MUSIC_KEY, volume);
        PlayerPrefs.SetFloat(MUSIC_KEY, volume);
    }

    public void SetSFXVolume(float volume)
    {
        ApplyVolumeToMixer(SFX_KEY, volume);
        PlayerPrefs.SetFloat(SFX_KEY, volume);
    }

    private void ApplyVolumeToMixer(string parameter, float volume)
    {
        if (mainMixer == null) return;

        // Convert 0-1 slider value to logarithmic dB (-80 to 0)
        float dB = volume > 0.0001f ? Mathf.Log10(volume) * 20 : -80f;
        mainMixer.SetFloat(parameter, dB);
    }




    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Stop all sfx/drawing sounds on scene load
        sfxSource.Stop();
        drawingSource.Stop();

        if (scene.name == winSceneName)
        {
            PlayMusic(null); // Stop music or play win theme if you prefer musicSource
            PlaySFX(winSound);
        }
        else if (scene.name == loseSceneName)
        {
            PlayMusic(null);
            PlaySFX(loseSound);
        }
        else if (scene.name == gameSceneName)
        {
            PlayMusic(backgroundMusic);
        }
        // Add more logic if needed
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            musicSource.Stop();
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayCoin()
    {
        PlaySFX(coinSound);
    }

    public void StartDrawing()
    {
        if (drawingSound != null && !drawingSource.isPlaying)
        {
            drawingSource.clip = drawingSound;
            drawingSource.loop = true;
            drawingSource.Play();
        }
    }

    public void StopDrawing()
    {
        drawingSource.Stop();
    }
}
