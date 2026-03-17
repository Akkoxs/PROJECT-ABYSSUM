using UnityEngine;

// Hold Left Shift + Up/Down  =  resize dead zone
// Hold Left Ctrl  + Up/Down  =  resize play area

public class DeadZoneController : MonoBehaviour
{
    [Header("Dead Zone Keyboard Control")]
    public KeyCode deadZoneModifier = KeyCode.LeftShift;
    public float   deadZoneResizeSpeed = 0.4f;

    [Header("Dead Zone Clamp")]
    public float minHalf = 0.05f;
    public float maxHalf = 0.45f;

    [Header("Play Area Keyboard Control")]
    public KeyCode playAreaModifier = KeyCode.LeftControl;
    public float   playAreaResizeSpeed = 100f; // pixels per second

    [Header("Play Area Clamp")]
    public float minPlayAreaSize = 200f;  // minimum square side in pixels
    public float maxPlayAreaSize = 0f;    // 0 = auto-set to screen height in Awake

    Splitscreen ss;

    void Awake()
    {
        ss = GetComponent<Splitscreen>();
        // auto-set max to screen height if not manually assigned
        if (maxPlayAreaSize <= 0f)
            maxPlayAreaSize = Screen.height;
    }

    void Update()
    {
        HandleDeadZone();
        HandlePlayArea();
    }

    void HandleDeadZone()
    {
        if (!Input.GetKey(deadZoneModifier)) return;

        float delta = 0f;
        if (Input.GetKey(KeyCode.UpArrow))   delta =  deadZoneResizeSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow)) delta = -deadZoneResizeSpeed * Time.deltaTime;
        if (Mathf.Abs(delta) < 1e-5f) return;

        ss.deadZoneHalf = Mathf.Clamp(ss.deadZoneHalf + delta, minHalf, maxHalf);
        ss.Rebuild();
    }

    void HandlePlayArea()
    {
        if (!Input.GetKey(playAreaModifier)) return;

        float delta = 0f;
        if (Input.GetKey(KeyCode.UpArrow))   delta =  playAreaResizeSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow)) delta = -playAreaResizeSpeed * Time.deltaTime;
        if (Mathf.Abs(delta) < 1e-5f) return;

        // if playAreaSize is 0 (auto mode), initialise it to current screen height
        // before we start adjusting so the first keypress doesn't jump
        if (ss.playAreaSize <= 0f)
            ss.playAreaSize = Screen.height;

        ss.playAreaSize = Mathf.Clamp(
            ss.playAreaSize + delta,
            minPlayAreaSize,
            maxPlayAreaSize);

        ss.Rebuild();
    }
}