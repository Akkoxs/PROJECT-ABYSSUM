using UnityEngine;

public class HurtBox : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other)
    {
        IDamageable damageable = other.gameObject.GetComponent<IDamageable>(); 
        if(damageable != null) 
            damageable.TakeDamage(10f); 
    }
}
