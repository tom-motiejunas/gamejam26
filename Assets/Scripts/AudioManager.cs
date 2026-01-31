using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource drawingSource;

    [Header("Clips")]
    public AudioClip backgroundMusic;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip coinSound;
    public AudioClip drawingSound;

    [Header("Scene Names")]
    public string winSceneName = "you_win_scene";
    public string loseSceneName = "game_over_scene";
    // Add other game scene names here if you loop music only in specific scenes
    public string gameSceneName = "tomas_scene"; 

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

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
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
