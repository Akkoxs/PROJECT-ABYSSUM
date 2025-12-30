using UnityEngine;
using UnityEngine.UI;

public class UpgradeBar : MonoBehaviour
{

    [SerializeField] GameObject gameManager;
    [SerializeField] ArtifactType artifactType;
    
    private Slider slider; 
    private ArtifactApplicator artifactApp;


    private void Awake()
    {
        if (gameManager != null)
            artifactApp = gameManager.GetComponent<ArtifactApplicator>();

        slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        artifactApp.onStatUpgraded.AddListener(UpdateUpgradeBar);
    }

    private void OnDisable()
    {
        artifactApp.onStatUpgraded.RemoveListener(UpdateUpgradeBar);
    }

    private void UpdateUpgradeBar(ArtifactType type, float level)
    {
        if (artifactType == type)
            slider.value = level*(float)0.25;
    }





}
