using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OxygenBarPlayer : MonoBehaviour
{
    [SerializeField] private Slider slider; 
    [SerializeField] private PlayerOxygen oxy; 
    [SerializeField] private Submarine sub;
    [SerializeField] private GameObject linkLight;
    [SerializeField] private UIHelper uiHelper;
    [SerializeField] private Image barFill;
    [SerializeField] private Image flashFill;
    [SerializeField] private float flashDuration = 0.2f;

    private Color flashColor = Color.white; 
    private Color regColor;

    private void OnEnable()
    {
        flashFill.color = regColor;
        UpdateOxygen(oxy.CurrentOxygen, oxy.MaxOxygen);
        oxy.oxygenChanged.AddListener(UpdateOxygen);
        oxy.oxygenDepleted.AddListener(OxygenDepleted);
        sub.enteredSubmarine.AddListener(OxygenLink);
        sub.exitedSubmarine.AddListener(OxygenUnlink);
    }

    private void OnDisable()
    {
        oxy.oxygenChanged.RemoveListener(UpdateOxygen);
        oxy.oxygenDepleted.RemoveListener(OxygenDepleted);   
    }

    private void UpdateOxygen(float currentOxygen, float maxOxygen)
    {
        slider.value = currentOxygen/maxOxygen;
    }

    private void OxygenDepleted()
    {
        StartCoroutine(uiHelper.BarFlash(flashDuration, flashColor, regColor, flashFill));
    }

    private void OxygenLink()
    {
        linkLight.SetActive(false);
    }

    private void OxygenUnlink()
    {
        linkLight.SetActive(true);
    }

}
