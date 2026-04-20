using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    public AudioClip footstep;
    public AudioClip jump;
    public AudioClip impactSmall;
    public AudioClip impactBig;
    public AudioClip ambientLoop;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CreateAudioManagerIfMissing()
    {
        if (FindFirstObjectByType<AudioManager>() != null)
        {
            return;
        }

        GameObject audioManagerObject = new GameObject("AudioManager");
        audioManagerObject.AddComponent<AudioManager>();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();
        LoadDefaultClipsIfNeeded();
    }

    void EnsureAudioSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        if (sources.Length < 1)
        {
            gameObject.AddComponent<AudioSource>();
        }

        if (sources.Length < 2)
        {
            gameObject.AddComponent<AudioSource>();
        }

        sources = GetComponents<AudioSource>();
        sfxSource = sources[0];
        musicSource = sources[1];
        musicSource.loop = true;
        musicSource.playOnAwake = false;
    }

    void LoadDefaultClipsIfNeeded()
    {
        if (footstep == null)
        {
            footstep = Resources.Load<AudioClip>("Audio/SFX/footstep");
        }

        if (jump == null)
        {
            jump = Resources.Load<AudioClip>("Audio/SFX/jump");
        }

        if (impactSmall == null)
        {
            impactSmall = Resources.Load<AudioClip>("Audio/SFX/impact_small");
        }

        if (impactBig == null)
        {
            impactBig = Resources.Load<AudioClip>("Audio/SFX/impact_big");
        }

        if (ambientLoop == null)
        {
            ambientLoop = Resources.Load<AudioClip>("Audio/Music/ambient_loop");
        }
    }

    void Start()
    {
        if (musicSource != null && ambientLoop != null)
        {
            musicSource.clip = ambientLoop;
            musicSource.loop = true;
            musicSource.volume = 0.3f;
            musicSource.Play();
        }
    }

    public void PlayFootstep()
    {
        if (sfxSource == null || footstep == null)
        {
            return;
        }

        sfxSource.pitch = Random.Range(0.9f, 1.1f);
        sfxSource.PlayOneShot(footstep, 0.3f);
    }

    public void PlayJump()
    {
        if (sfxSource == null || jump == null)
        {
            return;
        }

        sfxSource.pitch = Random.Range(0.95f, 1.05f);
        sfxSource.PlayOneShot(jump, 0.5f);
    }

    public void PlayImpact(float impact)
    {
        if (sfxSource == null)
        {
            return;
        }

        float volume = Mathf.Clamp(impact / 10f, 0.3f, 1f);
        sfxSource.pitch = Random.Range(0.9f, 1.1f);

        if (impact > 6f)
        {
            if (impactBig != null)
            {
                sfxSource.PlayOneShot(impactBig, volume);
            }
        }
        else
        {
            if (impactSmall != null)
            {
                sfxSource.PlayOneShot(impactSmall, volume);
            }
        }
    }
}
