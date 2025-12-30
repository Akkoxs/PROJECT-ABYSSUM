using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ArtifactApplicator : MonoBehaviour
{
    [SerializeField] private GameStats gameStats;
    [SerializeField] private UpgradeTracker upgradeTracker;

    //player
    [SerializeField] private GameObject player; 
    private Health playerHealth;
    private PlayerOxygen playerOxy; 
    private PlayerStatSetter playerStatSetter; 

    //submarine
    [SerializeField] private GameObject sub; 
    private Health subHealth;
    private SubmarineOxygen subOxy;
    private SubmarineStatSetter subStatSetter; 


    //UI
    [SerializeField] private Sonar sonar; 

    //events
    public UnityEvent <ArtifactType, float> onStatUpgraded; 
    public UnityEvent <string> onReplenished;

    private void Awake()
    {
        playerHealth = player.GetComponent<Health>();
        playerOxy = player.GetComponent<PlayerOxygen>();
        playerStatSetter = player.GetComponent<PlayerStatSetter>();

        subHealth = sub.GetComponent<Health>();
        subOxy = sub.GetComponent<SubmarineOxygen>();
        subStatSetter = sub.GetComponent<SubmarineStatSetter>();
    }

    public void ApplyArtifact(ArtifactStats artifactStat)
    {
        if (upgradeTracker.IsUpgradableArtifact(artifactStat.artifactType))
        {
            foreach (StatEffect effect in artifactStat.statEffects)
            {
                ApplyArtifactStats(effect.stat, effect.value);
            }

            upgradeTracker.IncrementArtifactLevel(artifactStat.artifactType);
            RefreshStats();

            float newLevel = upgradeTracker.GetArtifactLevel(artifactStat.artifactType);
            onStatUpgraded?.Invoke(artifactStat.artifactType, newLevel);
            Debug.Log(newLevel);
        }

        ApplySpecialLogic(artifactStat);
    }

    private void ApplyArtifactStats(StatType stat, float value)
    {
        switch(stat)
        {
            case StatType.DiverHealth:
            gameStats.diverMaxHealth += value;
            break;

            case StatType.DiverOxygen:
            gameStats.diverMaxOxygen += value;
            break;

            case StatType.SubHealth:
            gameStats.subMaxHealth += value;
            break;

            case StatType.SubOxygen:
            gameStats.subMaxOxygen += value;
            break;

            case StatType.SonarSpeed:
            gameStats.scanSpeed += value;
            break;

            case StatType.TorpedoDamage:
            gameStats.torpDamage += value;
            break;

            case StatType.TorpedoSpeed:
            gameStats.torpSpeed += value;
            break;

            case StatType.HarpoonDamage:
            gameStats.harpDamage += value;
            break;

            case StatType.HarpoonSpeed:
            gameStats.harpSpeed += value;
            break;

            case StatType.HarpoonReload:
            gameStats.harpReloadSpeed += value;
            break;

            case StatType.DiverSpeed:
            gameStats.diverMoveSpeed += value;
            break;

            case StatType.SubSpeed:
            gameStats.subMoveSpeed += value;
            break;
        }
    }

    private void ApplySpecialLogic(ArtifactStats artifactStat)
    {
        if (artifactStat.replenishPlayerHealth)
        {
            float amount = playerHealth.MaxHealth;
            playerHealth.Heal(amount);
        }

        if (artifactStat.replenishSubHealth)
        {
            float amount = subHealth.MaxHealth;
            subHealth.Heal(amount);       
        }

        if (artifactStat.replenishPlayerOxygen)
        {
            playerOxy.ResetOxygen();
        }

        if (artifactStat.replenishSubOxygen)
        {
            subOxy.ResetOxygen();
        }

        if (artifactStat.replenishTorpedo)
        {
            //
        }

        if (artifactStat.grantExtraTorpedo)
        {
            //
        }
    }

    private void RefreshStats()
    {
        if (playerStatSetter != null)
            playerStatSetter.ApplyStats();

        if (subStatSetter != null)
            subStatSetter.ApplyStats();
    }
}
