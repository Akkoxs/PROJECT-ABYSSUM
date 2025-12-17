using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider; 
    [SerializeField] private Health health; 
    [SerializeField] private UIHelper uiHelper;
    [SerializeField] private Image barFill;
    [SerializeField] private float flashDuration = 0.2f;

    private Color flashColor = Color.white; 
    private Color regColor;

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
        StartCoroutine(uiHelper.BarFlash(flashDuration, flashColor, regColor, barFill));
    }
}
