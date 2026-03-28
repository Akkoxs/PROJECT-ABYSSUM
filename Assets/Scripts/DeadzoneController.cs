using UnityEngine;

// Hold Left Shift + Up/Down  =  resize dead zone (coarse)
// Hold Left Ctrl  + Up/Down  =  resize dead zone (fine)

public class DeadZoneController : MonoBehaviour
{
    [Header("Dead Zone Keyboard Control")]
    public KeyCode coarseModifier = KeyCode.LeftShift;
    public KeyCode fineModifier   = KeyCode.LeftControl;
    public float   coarseSpeed    = 0.4f;
    public float   fineSpeed      = 0.05f;

    [Header("Dead Zone Clamp")]
    public float minHalf = 0.05f;
    public float maxHalf = 0.45f;

    Splitscreen ss;

    void Awake() => ss = GetComponent<Splitscreen>();

    void Update()
    {
        float speed = 0f;
        if      (Input.GetKey(coarseModifier)) speed = coarseSpeed;
        else if (Input.GetKey(fineModifier))   speed = fineSpeed;
        else return;

        float delta = 0f;
        if (Input.GetKey(KeyCode.UpArrow))   delta =  speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow)) delta = -speed * Time.deltaTime;
        if (Mathf.Abs(delta) < 1e-5f) return;

        ss.deadZoneHalf = Mathf.Clamp(ss.deadZoneHalf + delta, minHalf, maxHalf);
        ss.Rebuild();
    }
}