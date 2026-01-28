using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * EndGameUI (Sprint 3 – WIN/LOSE UI) [FINAL MAGIC v2]:
 * ZMIANY (Twoje):
 * - Napis WIN/LOSE trzyma siê ~4 sekundy (d³u¿szy "hold" animacji).
 * - Bez znaków typu \o/ (czysty tekst).
 * - Wiêkszy napis.
 * - Wiêcej konfetti: góra œrodek + góra lewo + góra prawo + dó³.
 * - Konfetti tego samego typu, ale ró¿ne kolory (per emitter).
 *
 * WIN:
 * - fanfara (EndGameSfx)
 * - konfetti x4 (ró¿ne kolory)
 * - wiêkszy napis
 * - flash (b³ysk ekranu)
 * - micro shake UI
 *
 * LOSE:
 * - ni¿szy dŸwiêk (EndGameSfx)
 * - smutna buŸka :(
 * - “opadniêcie” (slide down + shrink)
 *
 * Freeze 0.15s:
 * - WaitForSecondsRealtime (bez psucia UI/Coroutine).
 */
public class EndGameUI : MonoBehaviour
{
    public static EndGameUI Instance { get; private set; }

    [Header("Opcjonalnie: przypnij rêcznie (jeœli chcesz)")]
    [SerializeField] private CanvasGroup group;
    [SerializeField] private RectTransform root;
    [SerializeField] private TextMeshProUGUI label;

    [Header("Flash (b³ysk)")]
    [SerializeField] private CanvasGroup flashGroup;
    [SerializeField] private Image flashImage;

    [Header("KONFETTI (auto) – emitery")]
    [SerializeField] private ParticleSystem confettiTopCenter;
    [SerializeField] private ParticleSystem confettiTopLeft;
    [SerializeField] private ParticleSystem confettiTopRight;
    [SerializeField] private ParticleSystem confettiBottom;

    [Header("Timing")]
    // TECH: d³u¿ej na ekranie (Twoja proœba)
    [SerializeField] private float showDuration = 4.0f;
    [SerializeField] private float enterTime = 0.30f;
    [SerializeField] private float exitTime = 0.35f;

    [Header("Freeze (impact)")]
    [SerializeField] private bool useFreeze = true;
    [SerializeField] private float freezeSeconds = 0.15f;

    [Header("Anim")]
    [SerializeField] private float startOffsetY = 240f;
    [SerializeField] private float bounce = 22f;
    [SerializeField] private float pulseScaleWin = 1.12f;
    [SerializeField] private float pulseScaleLose = 1.06f;

    // TECH: wiêkszy napis (Twoja proœba)
    [SerializeField] private float winFontSize = 98f;
    [SerializeField] private float loseFontSize = 88f;

    private Coroutine routine;

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
        // TECH: czysty tekst bez \o/ (Twoja proœba)
        ShowMessage($"YOU WIN!   LVL {levelNumber}", isGameOver: false);

        if (EndGameSfx.Instance != null) EndGameSfx.Instance.PlayWin();

        // MAGIC: flash + konfetti
        StartCoroutine(FlashRoutine());
        PlayConfettiBurst();
    }

    public void ShowGameOver()
    {
        // TECH: smutna buŸka ASCII (pewne w TMP)
        ShowMessage("GAME OVER  :(", isGameOver: true);

        if (EndGameSfx.Instance != null) EndGameSfx.Instance.PlayLose();

        // MAGIC: krótszy/s³abszy flash (bardziej “smutny b³ysk”)
        StartCoroutine(FlashRoutine(weaker: true));
    }

    // ===================== Core =====================

    private void ShowMessage(string text, bool isGameOver)
    {
        EnsureUiExists();
        if (label != null) label.text = text;

        // TECH: kolor “win/lose”
        if (label != null)
        {
            label.color = isGameOver
                ? new Color(1f, 0.25f, 0.25f, 1f)   // czerwony
                : new Color(0.25f, 1f, 0.55f, 1f);  // zielony
        }

        // TECH: wiêkszy napis (Twoja proœba)
        if (label != null)
        {
            label.fontSize = isGameOver ? loseFontSize : winFontSize;
        }

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(isGameOver));
    }

    private IEnumerator ShowRoutine(bool isGameOver)
    {
        if (group != null) group.blocksRaycasts = false;

        // Freeze “impact”
        if (useFreeze && freezeSeconds > 0f)
        {
            yield return StartCoroutine(FreezeRoutine(freezeSeconds));
        }

        // start
        if (group != null) group.alpha = 0f;

        if (root != null)
        {
            root.anchoredPosition = new Vector2(0f, startOffsetY);
            root.localScale = Vector3.one * (isGameOver ? 0.90f : 0.92f);
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

                // LOSE: ciê¿ej, bez bounce
                if (isGameOver) y = Mathf.Lerp(startOffsetY, -10f, eased);

                root.anchoredPosition = new Vector2(0f, y);

                float sFrom = isGameOver ? 0.90f : 0.92f;
                root.localScale = Vector3.one * Mathf.Lerp(sFrom, 1.00f, eased);
            }

            yield return null;
        }

        // pulse + micro-shake (trochê d³u¿ej, ¿eby animacja by³a “czytelna”)
        float pulseTime = 0.38f;
        float pulseScale = isGameOver ? pulseScaleLose : pulseScaleWin;

        float pulseT = 0f;
        while (pulseT < pulseTime)
        {
            pulseT += Time.deltaTime;
            float p = pulseTime <= 0f ? 1f : Mathf.Clamp01(pulseT / pulseTime);

            if (root != null)
            {
                float s = Mathf.Lerp(1.00f, pulseScale, Mathf.Sin(p * Mathf.PI));
                root.localScale = Vector3.one * s;

                float amp = isGameOver ? 2.8f : 6.5f; // WIN trochê mocniej
                float x = (Mathf.PerlinNoise(Time.time * 25f, 0.1f) - 0.5f) * amp;
                float y = (Mathf.PerlinNoise(0.1f, Time.time * 25f) - 0.5f) * amp;

                root.anchoredPosition = new Vector2(x, y);
            }

            yield return null;
        }

        if (root != null)
        {
            root.localScale = Vector3.one;
            root.anchoredPosition = Vector2.zero;
        }

        // hold: reszta czasu do ~4s (Twoja proœba)
        float hold = Mathf.Max(0f, showDuration - pulseTime);
        if (hold > 0f) yield return new WaitForSeconds(hold);

        // exit:
        t = 0f;
        while (t < exitTime)
        {
            t += Time.deltaTime;
            float p = exitTime <= 0f ? 1f : Mathf.Clamp01(t / exitTime);
            float eased = EaseInCubic(p);

            if (group != null) group.alpha = Mathf.Lerp(1f, 0f, eased);

            if (root != null)
            {
                if (isGameOver)
                {
                    // LOSE: “opadniêcie” (Twoja proœba)
                    float y = Mathf.Lerp(0f, -150f, eased);
                    float s = Mathf.Lerp(1.0f, 0.83f, eased);
                    root.anchoredPosition = new Vector2(0f, y);
                    root.localScale = Vector3.one * s;
                }
                else
                {
                    float y = Mathf.Lerp(0f, -80f, eased);
                    root.anchoredPosition = new Vector2(0f, y);
                }
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

    private IEnumerator FlashRoutine(bool weaker = false)
    {
        EnsureFlashExists();

        if (flashGroup == null) yield break;

        float peak = weaker ? 0.28f : 0.65f;

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

    private void PlayConfettiBurst()
    {
        EnsureConfettiExists();

        // TECH: 4 emitery — góra œrodek + lewa + prawa + dó³ (Twoja proœba)
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

    // ===================== UI bootstrap =====================

    private void EnsureUiExists()
    {
        if (group != null && root != null && label != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject c = new GameObject("Canvas", typeof(Canvas));
            canvas = c.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            c.AddComponent<CanvasScaler>();
            c.AddComponent<GraphicRaycaster>();
        }

        GameObject uiRoot = GameObject.Find("EndGameUI_Root");
        if (uiRoot == null)
        {
            uiRoot = new GameObject("EndGameUI_Root");
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

    private void EnsureFlashExists()
    {
        if (flashGroup != null && flashImage != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject flashObj = GameObject.Find("EndGameUI_Flash");
        if (flashObj == null)
        {
            flashObj = new GameObject("EndGameUI_Flash");
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

    private void EnsureConfettiExists()
    {
        // TECH: tworzymy 4 emitery (ró¿ne kolory)
        if (confettiTopCenter == null) confettiTopCenter = EnsureOneConfetti("EndGameUI_Confetti_TopCenter", ConfettiAnchor.TopCenter, ConfettiPalette.Warm);
        if (confettiTopLeft == null) confettiTopLeft = EnsureOneConfetti("EndGameUI_Confetti_TopLeft", ConfettiAnchor.TopLeft, ConfettiPalette.Cool);
        if (confettiTopRight == null) confettiTopRight = EnsureOneConfetti("EndGameUI_Confetti_TopRight", ConfettiAnchor.TopRight, ConfettiPalette.Gold);
        if (confettiBottom == null) confettiBottom = EnsureOneConfetti("EndGameUI_Confetti_Bottom", ConfettiAnchor.BottomCenter, ConfettiPalette.Mixed);
    }

    // ===================== Confetti helpers =====================

    private enum ConfettiAnchor { TopCenter, TopLeft, TopRight, BottomCenter }
    private enum ConfettiPalette { Warm, Cool, Gold, Mixed }

    private ParticleSystem EnsureOneConfetti(string name, ConfettiAnchor anchor, ConfettiPalette palette)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null)
        {
            obj = new GameObject(name);
        }

        ParticleSystem ps = obj.GetComponent<ParticleSystem>();
        if (ps == null) ps = obj.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.loop = false;
        main.duration = 0.9f;
        main.startLifetime = 1.25f;
        main.startSpeed = 11f;
        main.startSize = 0.18f;
        main.maxParticles = 520; // TECH: wiêcej konfetti (Twoja proœba)
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 240) }); // TECH: wiêkszy burst

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 30f;
        shape.radius = 0.35f;

        // pozycja emitera (kamera -> viewport)
        Camera cam = Camera.main;
        Vector3 pos;

        if (cam != null)
        {
            // UWAGA: z = dystans do p³aszczyzny œwiata; u nas kamera stoi na -10, wiêc bierzemy abs(z)
            float z = Mathf.Abs(cam.transform.position.z);

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

            obj.transform.position = new Vector3(pos.x, pos.y, 0f);
        }
        else
        {
            // fallback (jakby kamera by³a nietypowa)
            obj.transform.position = anchor == ConfettiAnchor.BottomCenter ? new Vector3(0f, -5f, 0f) : new Vector3(0f, 8f, 0f);
        }

        // grawitacja / opadanie
        var force = ps.forceOverLifetime;
        force.enabled = true;
        force.y = -6.5f;

        // renderer
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        // kolor
        var colorOver = ps.colorOverLifetime;
        colorOver.enabled = true;
        colorOver.color = BuildConfettiGradient(palette);

        return ps;
    }

    private static Gradient BuildConfettiGradient(ConfettiPalette palette)
    {
        Gradient grad = new Gradient();

        // TECH: ró¿ne palety kolorów dla ró¿nych emiterów (Twoja proœba)
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
