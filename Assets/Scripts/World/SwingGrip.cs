using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class SwingGrip : MonoBehaviour
{
    public Rigidbody2D Body { get; private set; }
    public bool IsGrabbed => grabbedCount > 0;

    private int grabbedCount;

    private void Awake()
    {
        Body = GetComponent<Rigidbody2D>();

        // Swing grips are intended to be grabbed via trigger overlap.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    public void SetGrabbed(bool grabbed)
    {
        grabbedCount += grabbed ? 1 : -1;
        if (grabbedCount < 0) grabbedCount = 0;
    }
}

