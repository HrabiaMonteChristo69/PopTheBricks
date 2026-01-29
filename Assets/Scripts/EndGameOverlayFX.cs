using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * EndGameOverlayFX – overlay FX w trakcie gry (GameScene):
 * - Napis "YOU WIN / GAME OVER" (na chwilê)
 * - Flash (b³ysk)
 * - Konfetti (4 emitery)
 *
 * To NIE jest skrypt do EndGameScene paneli. To jest do GameScene.
 * Nie gryzie siê z Twoim EndGameUI od paneli win/lose.
 */
public class EndGameOverlayFX : MonoBehaviour
{
    public static EndGameOverlayFX Instance { get; private set; }

    [Header("Opcjonalnie: przypnij rêcznie (jeœli chcesz)")]
    [SerializeField] private CanvasGroup group;
    [SerializeField] private RectTransform root;
    [SerializeField] private TextMeshProUGUI label;

    [Header("Flash (b³ysk)")]
    [SerializeField] private CanvasGroup flashGroup;
    [SerializeField] private Image flashImage;

    [Header("Konfetti (emitery)")]
    [SerializeField] private ParticleSystem confettiTopCenter;
    [SerializeField] private ParticleSystem confettiTopLeft;
    [SerializeField] private ParticleSystem confettiTopRight;
    [SerializeField] private ParticleSystem confettiBottom;

    [Header("Timing")]
    [SerializeField] private float showDuration = 1.2f;
    [SerializeField] private float enterTime = 0.25f;
    [SerializeField] private float exitTime = 0.25f;

    [Header("Freeze (impact)")]
    [SerializeField] private bool useFreeze = true;
    [SerializeField] private float freezeSeconds = 0.12f;

    [Header("Anim")]
    [SerializeField] private float startOffsetY = 220f;
    [SerializeField] private float bounce = 18f;
    [SerializeField] private float winFontSize = 86f;
    [SerializeField] private float loseFontSize = 78f;

    private Coroutine routine;

    private enum ConfettiAnchor { TopCenter, TopLeft, TopRight, BottomCenter }
    private enum ConfettiPalette { Warm, Cool, Gold, Mixed }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        EnsureUiExists();
        HideImmediate();
    }

    // ===================== Public API =====================

    public void ShowLevelClear(int levelNumber)
    {
        ShowMessage($"LVL {levelNumber} CLEAR!", isGameOver: false);

        // konfetti + flash
        StartCoroutine(FlashRoutine(weaker: false));
        PlayConfettiBurst();
    }

    public void ShowGameOver()
    {
        ShowMessage("GAME OVER", isGameOver: true);

        // s³abszy flash (bardziej “smutny”)
        StartCoroutine(FlashRoutine(weaker: true));
    }

    // ===================== Core =====================

    private void ShowMessage(string text, bool isGameOver)
    {
        EnsureUiExists();

        if (label != null)
        {
            label.text = text;
            label.fontSize = isGameOver ? loseFontSize : winFontSize;

            // kolory
            label.color = isGameOver
                ? new Color(1f, 0.25f, 0.25f, 1f)   // czerwony
                : new Color(0.25f, 1f, 0.55f, 1f);  // zielony
        }

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(isGameOver));
    }

    private IEnumerator ShowRoutine(bool isGameOver)
    {
        if (group != null) group.blocksRaycasts = false;

        // Freeze “impact”
        if (useFreeze && freezeSeconds > 0f)
            yield return FreezeRoutine(freezeSeconds);

        // start
        if (group != null) group.alpha = 0f;

        if (root != null)
        {
            root.anchoredPosition = new Vector2(0f, startOffsetY);
            root.localScale = Vector3.one * (isGameOver ? 0.92f : 0.94f);
        }

        // enter
        float t = 0f;
        while (t < enterTime)
        {
            t += Time.deltaTime;
            float p = enterTime <= 0f ? 1f : Mathf.Clamp01(t / enterTime);
            float eased = EaseOutCubic(p);

            if (group != null) group.alpha = Mathf.Lerp(0f, 1f, eased);

            if (root != null)
            {
                float y = Mathf.Lerp(startOffsetY, 0f, eased);

                // WIN: bounce
                if (!isGameOver && p > 0.75f)
                {
                    float b = (p - 0.75f) / 0.25f;
                    y -= Mathf.Sin(b * Mathf.PI) * bounce;
                }

                root.anchoredPosition = new Vector2(0f, y);
                root.localScale = Vector3.one * Mathf.Lerp(isGameOver ? 0.92f : 0.94f, 1.0f, eased);
            }

            yield return null;
        }

        // hold
        if (showDuration > 0f) yield return new WaitForSeconds(showDuration);

        // exit
        t = 0f;
        while (t < exitTime)
        {
            t += Time.deltaTime;
            float p = exitTime <= 0f ? 1f : Mathf.Clamp01(t / exitTime);
            float eased = EaseInCubic(p);

            if (group != null) group.alpha = Mathf.Lerp(1f, 0f, eased);

            if (root != null)
            {
                float y = Mathf.Lerp(0f, -60f, eased);
                root.anchoredPosition = new Vector2(0f, y);
            }

            yield return null;
        }

        HideImmediate();
        routine = null;
    }

    private IEnumerator FreezeRoutine(float seconds)
    {
        float prev = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(seconds);
        Time.timeScale = prev;
    }

    private IEnumerator FlashRoutine(bool weaker)
    {
        EnsureFlashExists();
        if (flashGroup == null) yield break;

        float peak = weaker ? 0.25f : 0.65f;

        float t = 0f;
        float inTime = 0.08f;
        while (t < inTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / inTime);
            flashGroup.alpha = Mathf.Lerp(0f, peak, p);
            yield return null;
        }

        t = 0f;
        float outTime = 0.18f;
        while (t < outTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / outTime);
            flashGroup.alpha = Mathf.Lerp(peak, 0f, p);
            yield return null;
        }

        flashGroup.alpha = 0f;
    }

    // ===================== Confetti =====================

    private void PlayConfettiBurst()
    {
        EnsureConfettiExists();

        // wa¿ne: kamera mo¿e siê ruszaæ -> ustaw pozycje ZA KA¯DYM razem
        PositionConfetti(confettiTopCenter, ConfettiAnchor.TopCenter);
        PositionConfetti(confettiTopLeft, ConfettiAnchor.TopLeft);
        PositionConfetti(confettiTopRight, ConfettiAnchor.TopRight);
        PositionConfetti(confettiBottom, ConfettiAnchor.BottomCenter);

        PlayOne(confettiTopCenter);
        PlayOne(confettiTopLeft);
        PlayOne(confettiTopRight);
        PlayOne(confettiBottom);
    }

    private static void PlayOne(ParticleSystem ps)
    {
        if (ps == null) return;
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Play(true);
    }

    private void PositionConfetti(ParticleSystem ps, ConfettiAnchor anchor)
    {
        if (ps == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        float z = Mathf.Abs(cam.transform.position.z);

        Vector3 pos;
        switch (anchor)
        {
            case ConfettiAnchor.TopLeft:
                pos = cam.ViewportToWorldPoint(new Vector3(0.15f, 1.05f, z));
                break;
            case ConfettiAnchor.TopRight:
                pos = cam.ViewportToWorldPoint(new Vector3(0.85f, 1.05f, z));
                break;
            case ConfettiAnchor.BottomCenter:
                pos = cam.ViewportToWorldPoint(new Vector3(0.50f, -0.05f, z));
                break;
            default:
                pos = cam.ViewportToWorldPoint(new Vector3(0.50f, 1.05f, z));
                break;
        }

        ps.transform.position = new Vector3(pos.x, pos.y, 0f);
    }

    private void EnsureConfettiExists()
    {
        if (confettiTopCenter == null) confettiTopCenter = EnsureOneConfetti("EGFX_Confetti_TopCenter", ConfettiPalette.Warm);
        if (confettiTopLeft == null) confettiTopLeft = EnsureOneConfetti("EGFX_Confetti_TopLeft", ConfettiPalette.Cool);
        if (confettiTopRight == null) confettiTopRight = EnsureOneConfetti("EGFX_Confetti_TopRight", ConfettiPalette.Gold);
        if (confettiBottom == null) confettiBottom = EnsureOneConfetti("EGFX_Confetti_Bottom", ConfettiPalette.Mixed);
    }

    private ParticleSystem EnsureOneConfetti(string name, ConfettiPalette palette)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null) obj = new GameObject(name);

        ParticleSystem ps = obj.GetComponent<ParticleSystem>();
        if (ps == null) ps = obj.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.loop = false;
        main.duration = 0.9f;
        main.startLifetime = 1.25f;
        main.startSpeed = 11f;
        main.startSize = 0.18f;
        main.maxParticles = 520;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 240) });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 30f;
        shape.radius = 0.35f;

        var force = ps.forceOverLifetime;
        force.enabled = true;
        force.y = -6.5f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        var colorOver = ps.colorOverLifetime;
        colorOver.enabled = true;
        colorOver.color = BuildConfettiGradient(palette);

        return ps;
    }

    private static Gradient BuildConfettiGradient(ConfettiPalette palette)
    {
        Gradient grad = new Gradient();

        switch (palette)
        {
            case ConfettiPalette.Warm:
                grad.SetKeys(
                    new[]
                    {
                        new GradientColorKey(new Color(1f, 0.35f, 0.15f), 0f),
                        new GradientColorKey(new Color(1f, 0.65f, 0.15f), 0.5f),
                        new GradientColorKey(new Color(1f, 0.90f, 0.25f), 1f),
                    },
                    new[]
                    {
                        new GradientAlphaKey(1f, 0f),
                        new GradientAlphaKey(1f, 0.7f),
                        new GradientAlphaKey(0f, 1f),
                    }
                );
                break;

            case ConfettiPalette.Cool:
                grad.SetKeys(
                    new[]
                    {
                        new GradientColorKey(new Color(0.20f, 1f, 0.60f), 0f),
                        new GradientColorKey(new Color(0.35f, 0.60f, 1f), 0.5f),
                        new GradientColorKey(new Color(0.65f, 0.35f, 1f), 1f),
                    },
                    new[]
                    {
                        new GradientAlphaKey(1f, 0f),
                        new GradientAlphaKey(1f, 0.7f),
                        new GradientAlphaKey(0f, 1f),
                    }
                );
                break;

            case ConfettiPalette.Gold:
                grad.SetKeys(
                    new[]
                    {
                        new GradientColorKey(new Color(1f, 0.85f, 0.15f), 0f),
                        new GradientColorKey(new Color(1f, 0.95f, 0.45f), 0.6f),
                        new GradientColorKey(new Color(1f, 1f, 0.75f), 1f),
                    },
                    new[]
                    {
                        new GradientAlphaKey(1f, 0f),
                        new GradientAlphaKey(1f, 0.7f),
                        new GradientAlphaKey(0f, 1f),
                    }
                );
                break;

            default: // Mixed
                grad.SetKeys(
                    new[]
                    {
                        new GradientColorKey(new Color(1f, 0.35f, 0.2f), 0f),
                        new GradientColorKey(new Color(0.2f, 1f, 0.55f), 0.33f),
                        new GradientColorKey(new Color(0.35f, 0.55f, 1f), 0.66f),
                        new GradientColorKey(new Color(1f, 0.9f, 0.3f), 1f),
                    },
                    new[]
                    {
                        new GradientAlphaKey(1f, 0f),
                        new GradientAlphaKey(1f, 0.7f),
                        new GradientAlphaKey(0f, 1f),
                    }
                );
                break;
        }

        return grad;
    }

    // ===================== UI bootstrap =====================

    private void EnsureUiExists()
    {
        if (group != null && root != null && label != null) return;

        Canvas canvas = FindOverlayCanvas();
        if (canvas == null)
        {
            // fallback: tworzymy overlay canvas (jeœli ktoœ usun¹³)
            GameObject c = new GameObject("OverlayCanvas", typeof(Canvas));
            canvas = c.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            c.AddComponent<CanvasScaler>();
            c.AddComponent<GraphicRaycaster>();
        }

        GameObject uiRoot = GameObject.Find("EndGameOverlayFX_Root");
        if (uiRoot == null)
        {
            uiRoot = new GameObject("EndGameOverlayFX_Root");
            uiRoot.transform.SetParent(canvas.transform, false);
        }

        root = uiRoot.GetComponent<RectTransform>();
        if (root == null) root = uiRoot.AddComponent<RectTransform>();
        root.anchorMin = new Vector2(0.5f, 0.82f);
        root.anchorMax = new Vector2(0.5f, 0.82f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.sizeDelta = new Vector2(1200f, 240f);

        group = uiRoot.GetComponent<CanvasGroup>();
        if (group == null) group = uiRoot.AddComponent<CanvasGroup>();

        TextMeshProUGUI existing = uiRoot.GetComponentInChildren<TextMeshProUGUI>(true);
        if (existing != null)
        {
            label = existing;
        }
        else
        {
            GameObject t = new GameObject("Label");
            t.transform.SetParent(uiRoot.transform, false);

            label = t.AddComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = winFontSize;
            label.fontStyle = FontStyles.Bold;
            label.enableWordWrapping = false;

            RectTransform tr = t.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
        }

        EnsureFlashExists();
        EnsureConfettiExists();
    }

    private Canvas FindOverlayCanvas()
    {
        var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            if (c != null && c.renderMode == RenderMode.ScreenSpaceOverlay)
                return c;
        }
        return null;
    }

    private void EnsureFlashExists()
    {
        if (flashGroup != null && flashImage != null) return;

        Canvas canvas = FindOverlayCanvas();
        if (canvas == null) return;

        GameObject flashObj = GameObject.Find("EndGameOverlayFX_Flash");
        if (flashObj == null)
        {
            flashObj = new GameObject("EndGameOverlayFX_Flash");
            flashObj.transform.SetParent(canvas.transform, false);
        }

        RectTransform r = flashObj.GetComponent<RectTransform>();
        if (r == null) r = flashObj.AddComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;

        flashImage = flashObj.GetComponent<Image>();
        if (flashImage == null) flashImage = flashObj.AddComponent<Image>();
        flashImage.color = Color.white;

        flashGroup = flashObj.GetComponent<CanvasGroup>();
        if (flashGroup == null) flashGroup = flashObj.AddComponent<CanvasGroup>();

        flashGroup.alpha = 0f;
        flashGroup.blocksRaycasts = false;
    }

    private void HideImmediate()
    {
        if (group != null)
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
        }

        if (root != null)
        {
            root.anchoredPosition = new Vector2(0f, startOffsetY);
            root.localScale = Vector3.one;
        }

        if (flashGroup != null) flashGroup.alpha = 0f;
    }

    // ===================== Easing =====================

    private static float EaseOutCubic(float x)
    {
        float p = 1f - Mathf.Clamp01(x);
        return 1f - p * p * p;
    }

    private static float EaseInCubic(float x)
    {
        float p = Mathf.Clamp01(x);
        return p * p * p;
    }
}
