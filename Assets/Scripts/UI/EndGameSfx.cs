using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EndGameSfx : MonoBehaviour
{
    public static EndGameSfx Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip loseClip;

    [Header("Tuning")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.85f;

    private AudioSource source;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f; // 2D

        DontDestroyOnLoad(gameObject);
    }

    public void PlayWin()
    {
        Play(winClip);
    }

    public void PlayLose()
    {
        Play(loseClip);
    }

    private void Play(AudioClip clip)
    {
        if (clip == null || source == null) return;
        source.PlayOneShot(clip, volume);
    }
}
