using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{

    [SerializeField] private Slider slider; 
    [SerializeField] private Health health; 
    [SerializeField] private Image barFill;
    [SerializeField] private float flashDuration = 0.2f;

    private Color flashColor = Color.white; 
    private Color regColor;
    private Coroutine flashCoroutine = null;

    public void Awake()
    {
        regColor = barFill.color;
    }

    private void OnEnable()
    {
        UpdateHealth(health.CurrentHealth, health.MaxHealth);
        barFill.color = regColor;
        health.healthChanged.AddListener(UpdateHealth);
    }

    private void OnDisable()
    {
        health.healthChanged.RemoveListener(UpdateHealth);   
    }

    private void UpdateHealth(float currentHealth, float maxHealth)
    {
        slider.value = currentHealth/maxHealth;
        
        if(flashCoroutine == null)
            flashCoroutine = StartCoroutine(BarFlash(flashDuration));
    }

    private IEnumerator BarFlash(float flashTime)
    {
        barFill.color = flashColor;  
        yield return new WaitForSeconds(flashTime);
        barFill.color = regColor;
        flashCoroutine = null;
    }

}
