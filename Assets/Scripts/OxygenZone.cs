using UnityEngine;

public class OxygenZone : MonoBehaviour
{

    [SerializeField] private Collider2D airProbe;
    [SerializeField] private Oxygen oxy;

    private Collider2D oxyCollider;


    void Start()
    {
        oxyCollider = GetComponent<Collider2D>();

        if (airProbe.IsTouching(oxyCollider))
            oxy.EnterOxygenZone();
        else
            oxy.ExitOxygenZone();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Oxygen oxy = other.gameObject.GetComponent<Oxygen>();
        if (oxy != null)
            oxy.EnterOxygenZone();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Oxygen oxy = other.gameObject.GetComponent<Oxygen>();
        if (oxy != null)
            oxy.ExitOxygenZone();
    }

}
