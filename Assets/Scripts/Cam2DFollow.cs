using UnityEngine;

public class Cam2DFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    [Header("Cam Settings")]
    public float z = -10f;
    public Vector2 deadZone = Vector2.zero;
    public Vector2 cameraOffset = Vector2.zero;
    private Vector3 smoothPos;
    private bool initialized;

    void OnEnable()
    {
        // Reset smoothPos whenever the camera is enabled (including after scene reload)
        if (target != null)
        {
            smoothPos = target.position;
            initialized = true;
            ApplyPosition();
        }
    }

    void Start()
    {
        if (target != null)
        {
            smoothPos = target.position;
            initialized = true;
            ApplyPosition();
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (!initialized)
        {
            smoothPos = target.position;
            initialized = true;
        }

        Vector3 targetPos = target.position;
        float dx = targetPos.x - smoothPos.x;
        float dy = targetPos.y - smoothPos.y;

        if (Mathf.Abs(dx) > deadZone.x) smoothPos.x = targetPos.x - Mathf.Sign(dx) * deadZone.x;
        if (Mathf.Abs(dy) > deadZone.y) smoothPos.y = targetPos.y - Mathf.Sign(dy) * deadZone.y;

        ApplyPosition();
    }

    // Call this to immediately snap camera to target (used when redirecting back from pause)
    public void SnapToTarget()
    {
        if (target != null)
        {
            smoothPos = target.position;
            initialized = true;
            ApplyPosition();
        }
    }

    void ApplyPosition()
    {
        transform.position = new Vector3(smoothPos.x + cameraOffset.x, smoothPos.y + cameraOffset.y, z);
    }
}