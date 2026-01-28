using UnityEngine;

/*
 * EndGameSfx (Sprint 3 – WIN/LOSE SFX) [FINAL]:
 * - Jeden AudioSource (2D) do krótkich efektów.
 * - WIN: fanfara (normalny pitch)
 * - LOSE: “smutniej” (ni¿szy pitch)
 */
[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class EndGameSfx : MonoBehaviour
{
    public static EndGameSfx Instance { get; private set; }

    [Header("Audio clips (przeci¹gnij z Assets)")]
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip loseClip;

    private AudioSource source;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D
    }

    public void PlayWin()
    {
        if (winClip == null) return;

        source.pitch = 1.0f;
        source.PlayOneShot(winClip);
    }

    public void PlayLose()
    {
        if (loseClip == null) return;

        source.pitch = 0.85f; // TECH: “smutniej”
        source.PlayOneShot(loseClip);
    }
}
