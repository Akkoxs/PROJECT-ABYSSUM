using UnityEngine;

public class SubmarineStatSetter : MonoBehaviour
{

[SerializeField] private GameStats stats;

private Health health;
private SubmarineOxygen subOxy;
private Sonar sonar;
//private Torpedo torp;
private Submarine controller;

private void Awake()
    {
        health = GetComponent<Health>();
        subOxy = GetComponent<SubmarineOxygen>();
        sonar = GetComponent<Sonar>();
        //torp = GetComponent<Torpedo>();
        controller = GetComponent<Submarine>();

        ApplyStats();
    }

public void ApplyStats()
    {
        if (health != null)
        {
            health.SetMaxHealth(stats.subMaxHealth, true);
        }

        if (subOxy != null)
        {
            subOxy.SetMaxOxygen(stats.subMaxOxygen);
        }

    //FOR FUTURE 
        if (sonar != null)
        {
            sonar.SetScanSpeed(stats.scanSpeed);
        }

        // if (torp != null)
        // {
        //     torp.SetDamage(stats.torpDamage);
        //     torp.SetSpeed(stats.torpSpeed);
        // Damage => 
        // Speed => 
        // }

         if (controller != null)
        {
        controller.SetMoveSpeed(stats.subMoveSpeed);
        }
    }

public void ApplyUpgrade(GameStats newStats)
    {
        stats = newStats;
        ApplyStats();
    } 
}
