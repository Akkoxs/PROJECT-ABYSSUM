using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable, IHealable
{

    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth; 

    //public read only 
    public float MaxHealth => maxHealth; 
    public float CurrentHealth => currentHealth; 
    public bool isDead {get; private set;} 

    //Events
    public UnityEvent <float, float> healthChanged;  
    public UnityEvent died; 


    private void Awake()
    {
       currentHealth = maxHealth;
       isDead = false;
    }

    public void TakeDamage(float dmgAmount)
    {
        if (isDead)
            return;
        
        currentHealth = Mathf.Max(0, currentHealth - dmgAmount); //keeps between min and current 
        healthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        if (isDead)
            return; 

        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount); //keeps between max and current 
        healthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        currentHealth = 0;
        isDead = true; 
        died?.Invoke();
    }

    public void SetMaxHealth(float newMax, bool fullHealth)
    {
        maxHealth = newMax; 
        
        if (fullHealth)
            currentHealth = maxHealth;

        else if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        healthChanged?.Invoke(currentHealth, maxHealth);

    } 


}