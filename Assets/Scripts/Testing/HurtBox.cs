using UnityEngine;

public class HurtBox : MonoBehaviour
{
    [SerializeField] GameObject sub;

    private void OnCollisionEnter2D(Collision2D other)
    {
        IDamageable playerDamageable = other.gameObject.GetComponent<IDamageable>(); 
        IDamageable subDamageable = sub.gameObject.GetComponent<IDamageable>();

        if(playerDamageable != null) 
            playerDamageable.TakeDamage(10f); 

        if(subDamageable != null)
            subDamageable.TakeDamage(50f);
    }
}
