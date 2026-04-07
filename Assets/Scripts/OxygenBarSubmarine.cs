using UnityEngine;
using UnityEngine.UI;

public class OxygenBarSubmarine : MonoBehaviour
{
    [SerializeField] private Slider slider; 
    [SerializeField] private SubmarineOxygen oxy; 
    [SerializeField] private UIHelper uiHelper;
    [SerializeField] private GameObject linkLight;
    [SerializeField] private Image barFill;
    [SerializeField] private Image flashFill;
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private Submarine sub;

    private Color flashColor = Color.white; 
    private Color regColor;

    private void OnEnable()
    {
        regColor = barFill.color;
        flashFill.color = regColor;

        linkLight.SetActive(!sub.PlayerInside);

        UpdateOxygen(oxy.CurrentOxygen, oxy.MaxOxygen);
        oxy.oxygenChanged.AddListener(UpdateOxygen);
        oxy.oxygenDepleted.AddListener(OxygenDepleted);
        sub.enteredSubmarine.AddListener(OnEnteredSubmarine);
        sub.exitedSubmarine.AddListener(OnExitedSubmarine);
    }

    private void OnDisable()
    {
        oxy.oxygenChanged.RemoveListener(UpdateOxygen);
        oxy.oxygenDepleted.RemoveListener(OxygenDepleted);   
        sub.enteredSubmarine.RemoveListener(OnEnteredSubmarine);
        sub.exitedSubmarine.RemoveListener(OnExitedSubmarine);
    }

    private void UpdateOxygen(float currentOxygen, float maxOxygen)
    {
        slider.value = currentOxygen/maxOxygen;
    }

    private void OxygenDepleted()
    {
        StartCoroutine(uiHelper.BarFlash(flashDuration, flashColor, regColor, flashFill, true));
    }

    private void OnEnteredSubmarine() => linkLight.SetActive(false);
    private void OnExitedSubmarine() => linkLight.SetActive(true);
}
