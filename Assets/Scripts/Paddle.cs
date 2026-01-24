using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Paddle : MonoBehaviour
{
    [Header("Ruch")]
    public float speed = 10f;

    [Header("Œciany (przeci¹gnij z Hierarchy)")]
    public Collider2D wallLeft;
    public Collider2D wallRight;

    [Header("Fallback (gdy nie przypniesz œcian)")]
    public bool useCameraFallback = true;

    private Rigidbody2D rb;
    private Collider2D col;

    private float halfWidth;
    private float minX, maxX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Ustawienia fizyki dla paletki (stabilne odbicia pi³ki)
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CacheHalfWidth();
        AutoFindWallsIfMissing();
        RecalculateLimits();
    }

    void CacheHalfWidth()
    {
        // po³owa szerokoœci paletki w œwiecie (z colliera!)
        halfWidth = col.bounds.extents.x;
    }

    void AutoFindWallsIfMissing()
    {
        if (wallLeft == null)
        {
            var go = GameObject.Find("WallLeft");
            if (go) wallLeft = go.GetComponent<Collider2D>();
        }

        if (wallRight == null)
        {
            var go = GameObject.Find("WallRight");
            if (go) wallRight = go.GetComponent<Collider2D>();
        }
    }

    void RecalculateLimits()
    {
        // jeœli zmieni³eœ collider paletki w edytorze, warto odœwie¿yæ halfWidth
        CacheHalfWidth();

        if (wallLeft != null && wallRight != null)
        {
            // wewnêtrzne krawêdzie œcian
            minX = wallLeft.bounds.max.x + halfWidth;
            maxX = wallRight.bounds.min.x - halfWidth;
        }
        else if (useCameraFallback && Camera.main != null)
        {
            var cam = Camera.main;
            float camHalfWidth = cam.orthographicSize * cam.aspect;

            minX = cam.transform.position.x - camHalfWidth + halfWidth;
            maxX = cam.transform.position.x + camHalfWidth - halfWidth;

            Debug.LogWarning("Paddle: Nie przypiêto WallLeft/WallRight – u¿ywam granic kamery (fallback).");
        }
        else
        {
            // awaryjnie: brak ograniczeñ
            minX = float.NegativeInfinity;
            maxX = float.PositiveInfinity;
        }
    }

    void FixedUpdate()
    {
        float input = Input.GetAxisRaw("Horizontal");

        Vector2 pos = rb.position;
        pos.x = Mathf.Clamp(pos.x + input * speed * Time.fixedDeltaTime, minX, maxX);

        rb.MovePosition(pos);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // ¿eby w edytorze po zmianie colliera/œcian od razu liczy³o granice
        if (!Application.isPlaying)
        {
            col = GetComponent<Collider2D>();
            if (col != null)
            {
                CacheHalfWidth();
                RecalculateLimits();
            }
        }
    }
#endif
}
