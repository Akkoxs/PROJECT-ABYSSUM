using UnityEngine;

//Follow script for cameras so that we dont have to use Cinemachine because it was bein weird 

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

    void Start()
    {
        smoothPos = target.position;
        initialized = true;
        ApplyPosition();
    }

    void LateUpdate()
    {
        if (!initialized)
        {
            smoothPos = target.position;
            initialized = true;
        }

        //for dead zone behaviour 
        Vector3 targetPos = target.position;
        float dx = targetPos.x - smoothPos.x;
        float dy = targetPos.y - smoothPos.y;

        //if the target is outside the dead zone, move the camera to the edge of the dead zone in that direction
        if (Mathf.Abs(dx) > deadZone.x) smoothPos.x = targetPos.x - Mathf.Sign(dx) * deadZone.x;
        if (Mathf.Abs(dy) > deadZone.y) smoothPos.y = targetPos.y - Mathf.Sign(dy) * deadZone.y;

        ApplyPosition();
    }

    void ApplyPosition()
    {
        //this is to add an offset to the camera because the trapezoidal shape makes it such that the target is not centered in in the quad
        transform.position = new Vector3(smoothPos.x + cameraOffset.x, smoothPos.y + cameraOffset.y, z);
        //transform.rotation = Quaternion.identity;
    }
}
