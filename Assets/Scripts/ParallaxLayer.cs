using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Header("Parallax Settings (0 = fixed, 1 = follows player)")]
    [Range(0f, 1f)]
    public float parallaxFactorX = 0.1f;

    [Range(0f, 1f)]
    public float parallaxFactorY = 0f;

    [Header("Waterline")]
    public float waterSurfaceY = 0f;

    //layer will be hidden when below waterline
    public bool hideWhenUnderwater = false;

    [Header("World Bounds (X dim)")]
    public float boundsMinX = -50f;
    public float boundsMaxX = 50f;

    [Header("World Bounds (Y dim)")]
    public float boundsMinY = -10f;
    public float boundsMaxY = 30f;

    private Vector3 startPosition;   // anchor when the scene begins
    private Vector3 startCamPos;     // camera position at scene start

    private Camera  cam;
    private SpriteRenderer spriteRenderer;

    // ── Unity Lifecycle ──────────────────────────────────────────────────────
    private void Awake()
    {
        cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        startCamPos = cam.transform.position;
    }

    private void LateUpdate()
    {
        UpdatePosition();
        //UpdateVisibility();
    }

    /// calculate the target position based on camera delta and parallax factors, apply world bounds
    private void UpdatePosition()
    {
        Vector3 camPos = cam.transform.position;
        Vector3 camDelta = camPos - startCamPos;

        // how far this layer travels relative to camera movement
        float targetX = startPosition.x + camDelta.x * parallaxFactorX;

        // For Y: only follow the camera while it is ABOVE the water surface, otherwise stay there

        float effectiveCamY = Mathf.Max(camPos.y, waterSurfaceY);
        float effectiveDeltaY = effectiveCamY - startCamPos.y;
        float targetY = startPosition.y + effectiveDeltaY * parallaxFactorY;

        // clamp to defined world bounds (for edges of the art)
        targetX = Mathf.Clamp(targetX, boundsMinX, boundsMaxX);
        targetY = Mathf.Clamp(targetY, boundsMinY, boundsMaxY);

        transform.position = new Vector3(targetX, startPosition.y, transform.position.z);
    }

    // /// Optionally hides the layer when the camera is below the water surface.
    // private void UpdateVisibility()
    // {
    //     if (!hideWhenUnderwater || _spriteRenderer == null) return;
    //     bool cameraIsAboveWater = _cam.transform.position.y >= waterSurfaceY;
    //     _spriteRenderer.enabled = cameraIsAboveWater;
    // }

    private void OnDrawGizmosSelected()
    {
        // draw world bounds in scene view 
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.4f);

        Vector3 center = new Vector3(
            (boundsMinX + boundsMaxX) * 0.5f,
            (boundsMinY + boundsMaxY) * 0.5f,
            transform.position.z);

        Vector3 size = new Vector3(
            boundsMaxX - boundsMinX,
            boundsMaxY - boundsMinY,
            0.1f);

        Gizmos.DrawWireCube(center, size);

        // waterline
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.8f);
        Gizmos.DrawLine(
            new Vector3(boundsMinX, waterSurfaceY, transform.position.z),
            new Vector3(boundsMaxX, waterSurfaceY, transform.position.z));
    }
}