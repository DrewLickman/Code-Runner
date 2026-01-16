using UnityEngine;

public class HangingPlug : MonoBehaviour
{
    [Header("Chain")]
    [Min(1)] public int segmentCount = 12;
    public float segmentLength = 0.5f;
    public float segmentMass = 0.2f;
    public float segmentLinearDrag = 0.5f;
    public float segmentAngularDrag = 0.05f;

    [Header("Reset / Damping")]
    [Tooltip("Higher drag used when the plug is not being grabbed, so it settles faster.")]
    public float idleLinearDrag = 2.2f;

    [Tooltip("Higher angular drag used when the plug is not being grabbed, so it settles faster.")]
    public float idleAngularDrag = 2.2f;

    [Tooltip("Max angle (degrees) each segment can bend relative to the previous one. Lower = more rigid cable.")]
    [Range(0f, 90f)]
    public float maxBendAngle = 18f;

    [Header("Grip")]
    public float gripRadius = 0.25f;

    [Header("Visuals")]
    public Color anchorColor = Color.black;
    public Color wireColor = new Color(0.55f, 0.55f, 0.55f, 1f);
    public float anchorSize = 0.28f;
    public float wireThickness = 0.14f;

    private bool built;
    private static Sprite sharedSprite;
    private Rigidbody2D[] segmentBodies;
    private SwingGrip grip;

    private void Start()
    {
        BuildIfNeeded();
    }

    private void FixedUpdate()
    {
        if (!built) return;
        if (segmentBodies == null || segmentBodies.Length == 0) return;

        bool grabbed = grip != null && grip.IsGrabbed;
        float targetDrag = grabbed ? segmentLinearDrag : idleLinearDrag;
        float targetAngDrag = grabbed ? segmentAngularDrag : idleAngularDrag;

        for (int i = 0; i < segmentBodies.Length; i++)
        {
            Rigidbody2D b = segmentBodies[i];
            if (b == null) continue;
            b.drag = targetDrag;
            b.angularDrag = targetAngDrag;
        }
    }

    private static Sprite GetSharedSprite()
    {
        if (sharedSprite != null) return sharedSprite;

        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Point;

        sharedSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
        return sharedSprite;
    }

    private void BuildIfNeeded()
    {
        if (built) return;
        built = true;

        // Top anchor
        GameObject anchor = new GameObject("PlugAnchor");
        anchor.transform.SetParent(transform, false);
        anchor.transform.position = transform.position;
        Rigidbody2D prevBody = anchor.AddComponent<Rigidbody2D>();
        prevBody.bodyType = RigidbodyType2D.Static;

        SpriteRenderer anchorSr = anchor.AddComponent<SpriteRenderer>();
        anchorSr.sprite = GetSharedSprite();
        anchorSr.color = anchorColor;
        anchorSr.drawMode = SpriteDrawMode.Sliced;
        anchorSr.size = new Vector2(anchorSize, anchorSize);

        segmentBodies = new Rigidbody2D[Mathf.Max(1, segmentCount)];

        for (int i = 0; i < segmentCount; i++)
        {
            GameObject seg = new GameObject(i == segmentCount - 1 ? "PlugGrip" : $"PlugSeg_{i:00}");
            seg.transform.SetParent(transform, false);
            seg.transform.position = transform.position + Vector3.down * segmentLength * (i + 1);

            Rigidbody2D body = seg.AddComponent<Rigidbody2D>();
            body.mass = segmentMass;
            // Start in "idle" state so it settles quickly when spawned.
            body.drag = idleLinearDrag;
            body.angularDrag = idleAngularDrag;
            segmentBodies[i] = body;

            SpriteRenderer sr = seg.AddComponent<SpriteRenderer>();
            sr.sprite = GetSharedSprite();
            sr.color = wireColor;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(wireThickness, segmentLength);

            HingeJoint2D hinge = seg.AddComponent<HingeJoint2D>();
            hinge.connectedBody = prevBody;
            hinge.autoConfigureConnectedAnchor = false;
            // Connect to the BOTTOM of the previous segment (or to the anchor's center for the first link).
            hinge.connectedAnchor = i == 0 ? Vector2.zero : new Vector2(0f, -segmentLength * 0.5f);
            hinge.anchor = new Vector2(0f, segmentLength * 0.5f);
            hinge.enableCollision = false;

            hinge.useLimits = maxBendAngle > 0.01f;
            if (hinge.useLimits)
            {
                JointAngleLimits2D limits = hinge.limits;
                limits.min = -maxBendAngle;
                limits.max = maxBendAngle;
                hinge.limits = limits;
            }

            // No physics collider on intermediate segments (prevents the player from pushing the rope near the ceiling
            // and causing joint instability). The grip at the end remains a trigger for attaching.

            if (i == segmentCount - 1)
            {
                // Add a trigger grip collider and marker.
                CircleCollider2D gripCol = seg.AddComponent<CircleCollider2D>();
                gripCol.radius = gripRadius;
                gripCol.isTrigger = true;

                grip = seg.AddComponent<SwingGrip>();
            }

            prevBody = body;
        }
    }
}

