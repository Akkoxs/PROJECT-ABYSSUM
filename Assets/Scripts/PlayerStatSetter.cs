using UnityEngine;

public class PlayerStatSetter : MonoBehaviour
{

[SerializeField] private GameStats stats;

private Health health;
private PlayerOxygen playerOxy;
//private Harpoon harp;
//private PlayerMovement controller;

private void Awake()
    {
        health = GetComponent<Health>();
        playerOxy = GetComponent<PlayerOxygen>();
        //harp = GetComponent<Harpoon>();
        //controller = GetComponent<PlayerMovement>();

        ApplyStats();
    }

public void ApplyStats()
    {
        if (health != null)
        {
            health.SetMaxHealth(stats.diverMaxHealth, true);
        }

        if (playerOxy != null)
        {
            playerOxy.SetMaxOxygen(stats.diverMaxOxygen);
        }

        //FOR FUTURE 
        // if (harp != null)
        // {
        //     harp.SetDamage(stats.harpDamage);
        //     harp.SetSpeed(stats.harpSpeed);
        //     harp.SetReloadSpeed(stats.harpReloadSpeed);
        // }

        // if (controller != null)
        // {
        //     controller.SetMoveSpeed(stats.diverMoveSpeed);
        // }
    }

public void ApplyUpgrade(GameStats newStats)
    {
        stats = newStats;
        ApplyStats();
    } 
}
