using UnityEngine;

[CreateAssetMenu(fileName = "GameStats", menuName = "Scriptable Objects/GameStats")]
public class GameStats : ScriptableObject
{
    [Header("Diver")]
    public float diverMaxHealth;
    public float diverMaxOxygen;
    public float diverMoveSpeed; 

    [Header("Submarine")]
    public float subMaxHealth; 
    public float subMaxOxygen; 
    public float subMoveSpeed;
    public float scanSpeed; 
    public float subCoolantCapacity;

    [Header("Harpoon")]
    public float harpDamage; 
    public float harpSpeed; 
    public float harpReloadSpeed; 

    [Header("Torpedos")]
    public float torpDamage; 
    public float torpSpeed; 

    public void ResetStats() //DEFAULT STATS
    {
        diverMaxHealth= 100f;
        diverMaxOxygen = 100f;
        diverMoveSpeed = 13.5f;

        subMaxHealth = 750f;
        subMaxOxygen = 1000f;
        subMoveSpeed = 50f;
        scanSpeed = 135f;

        harpDamage = 10f;
        harpSpeed = 30f;
        harpReloadSpeed = 2f;

        torpDamage = 30f;
        torpSpeed = 20f;
    }
}
