using System.Drawing;
using UnityEngine;

public class PlayerStatSetter : MonoBehaviour
{

[SerializeField] private GameStats stats;
[SerializeField] private GameObject pfHarpoon;

private Health health;
private PlayerOxygen playerOxy;
private Projectile harp;
private MouseAiming aim;
private PlayerController controller;

private void Awake()
    {
        health = GetComponent<Health>();
        playerOxy = GetComponent<PlayerOxygen>();
        harp = pfHarpoon.GetComponent<Projectile>();
        aim = GetComponent<MouseAiming>();
        controller = GetComponent<PlayerController>();

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

        if (harp != null)
        {
            harp.SetHarpDamage(stats.harpDamage);
            harp.SetHarpForce(stats.harpSpeed);
        }

        if (aim != null)
        {
            aim.SetReloadSpeed(stats.harpReloadSpeed);
        }

        if (controller != null)
        {
            controller.SetMoveSpeed(stats.diverMoveSpeed);
        }
    }

public void ApplyUpgrade(GameStats newStats)
    {
        stats = newStats;
        ApplyStats();
    } 
}
