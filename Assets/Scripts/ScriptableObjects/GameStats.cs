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
        diverMoveSpeed = 12.5f;

        subMaxHealth = 750f;
        subMaxOxygen = 500f;
        subMoveSpeed = 20f;
        scanSpeed = 135f;

        harpDamage = 10f;
        harpSpeed = 10f;
        harpReloadSpeed = 2f;

        torpDamage = 50f;
        torpSpeed = 20f;
    }
}
