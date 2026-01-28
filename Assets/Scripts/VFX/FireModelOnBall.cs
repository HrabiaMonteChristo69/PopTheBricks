using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_2021_2_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif

/*
 * FireModeOnBall (Sprint 2 – Fire Mode po 6 hitach) [FINAL HARD - Twoje ustawienia]:
 * - HARD ustawienia TrailRenderer dok³adnie jak na Twoim screenie:
 *   Material: Default-ParticleSystem
 *   Width: 1.0
 *   Time: 0.8
 *   Color: mocny pomarañczowy -> jaœniej -> alpha 0 na koñcu
 * - Light2D te¿ ustawiamy stabilnie.
 */
[DisallowMultipleComponent]
public class FireModeOnBall : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private int hitsToActivate = 6;
    [SerializeField] private float durationSeconds = 5f;

    [Header("Trail (auto-find)")]
    [SerializeField] private TrailRenderer trail;

#if UNITY_2021_2_OR_NEWER
    [Header("2D Light (auto-find, URP 2D)")]
    [SerializeField] private Light2D ballLight;
#endif

    // ================== HARD (Twoje ustawienia) ==================
    private const float TRAIL_TIME = 0.4f;
    private const float TRAIL_WIDTH_MULT = 0.6f;
    private const float TRAIL_MIN_VERTEX_DISTANCE = 0.1f;

    // Light2D
    private const float NORMAL_LIGHT_INTENSITY = 0.35f;
    private const float FIRE_LIGHT_INTENSITY = 1.25f;
    private const float FIRE_LIGHT_OUTER_RADIUS = 3.2f;

    // Camera feedback
    private const float FIRE_HIT_SHAKE = 0.10f;
    private const float FIRE_ACTIVATE_PUNCH = 0.22f;
    // =============================================================

    private int currentHits;
    private bool isActive;
    private Coroutine activeRoutine;

    private void Awake()
    {
        if (trail == null) trail = GetComponent<TrailRenderer>();

#if UNITY_2021_2_OR_NEWER
        if (ballLight == null) ballLight = GetComponent<Light2D>();
#endif

        ApplyHardTrailSettings();
        ApplyHardLightSettings();

        ApplyVisualState(active: false);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ApplyHardTrailSettings();
        ApplyHardLightSettings();
    }
#endif

    // ===================== API =====================

    public void NotifyBreakableBrickHit()
    {
        if (isActive)
        {
            if (CameraController.Instance != null) CameraController.Instance.AddShake(FIRE_HIT_SHAKE);
            return;
        }

        currentHits++;
        if (currentHits >= hitsToActivate)
        {
            ActivateFireMode();
        }
    }

    public void ResetCombo()
    {
        currentHits = 0;
    }

    // ===================== Core =====================

    private void ActivateFireMode()
    {
        if (isActive) return;
        isActive = true;

        if (CameraController.Instance != null)
        {
            CameraController.Instance.PunchZoom(FIRE_ACTIVATE_PUNCH);
            CameraController.Instance.ShakeMedium();
        }

        ApplyVisualState(active: true);

        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(FireModeTimer());
    }

    private IEnumerator FireModeTimer()
    {
        float t = 0f;
        while (t < durationSeconds)
        {
            t += Time.deltaTime;
            yield return null;
        }

        isActive = false;
        currentHits = 0;

        ApplyVisualState(active: false);
        activeRoutine = null;
    }

    private void ApplyVisualState(bool active)
    {
        if (trail != null) trail.emitting = active;

#if UNITY_2021_2_OR_NEWER
        if (ballLight != null) ballLight.intensity = active ? FIRE_LIGHT_INTENSITY : NORMAL_LIGHT_INTENSITY;
#endif
    }

    // ===================== HARD SETTINGS =====================

    private void ApplyHardTrailSettings()
    {
        if (trail == null) return;

        trail.time = TRAIL_TIME;
        trail.minVertexDistance = TRAIL_MIN_VERTEX_DISTANCE;
        trail.widthMultiplier = TRAIL_WIDTH_MULT;

        trail.alignment = LineAlignment.View;
        trail.textureMode = LineTextureMode.Stretch;

        // TECH: materia³ EXACT jak u Ciebie
        Shader particleShader = Shader.Find("Particles/System Material");
        if (particleShader != null)
        {
            trail.material = new Material(particleShader);
        }

        // TECH: kolor "mocny pomarañczowy" -> jaœniejszy -> przezroczysty
        var grad = new Gradient();

        grad.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(1.0f, 0.45f, 0.0f, 1f), 0f),  // mocny pomarañcz
                new GradientColorKey(new Color(1.0f, 0.75f, 0.2f, 1f), 0.6f),
                new GradientColorKey(new Color(1.0f, 0.9f, 0.6f, 1f), 1f),
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.6f, 0.5f),
                new GradientAlphaKey(0f, 1f),
            }
        );

        trail.colorGradient = grad;

        trail.shadowCastingMode = ShadowCastingMode.Off;
        trail.receiveShadows = false;
    }

    private void ApplyHardLightSettings()
    {
#if UNITY_2021_2_OR_NEWER
        if (ballLight == null) return;

        ballLight.lightType = Light2D.LightType.Point;
        ballLight.intensity = NORMAL_LIGHT_INTENSITY;
        ballLight.pointLightOuterRadius = FIRE_LIGHT_OUTER_RADIUS;
#endif
    }
}
