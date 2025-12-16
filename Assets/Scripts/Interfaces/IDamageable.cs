using UnityEngine;

public interface IDamageable
{    
    void TakeDamage(float dmgAmount);
}

//Interface for anything that has Health
//Using this interface, we can heal/damage anything with health without knowing what it is and how its done. 
//"Check if an object is damageable, and if so, damage it" We dont care what we are damaging, only to damage it if it can be damaged. 
//As so: 

// if (Physics.Raycast(ray, out hitInfo, weaponRange))
// {
//     IDamageable damageable = hitInfo.collider.GetComponent<IDamageable>();
//     if (damageable != null)
//     {
//     damageable.TakeDamage(weaponDamage);
//     }
// }
