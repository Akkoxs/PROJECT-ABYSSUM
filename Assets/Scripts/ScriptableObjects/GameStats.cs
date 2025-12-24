using UnityEngine;

[CreateAssetMenu(fileName = "GameStats", menuName = "Scriptable Objects/GameStats")]
public class GameStats : ScriptableObject
{
    [Header("Diver")]
    public float diverMaxHealth = 100f;
    public float diverMaxOxygen = 100f;
    public float diverMoveSpeed; //placeholder 

    [Header("Harpoon")]
    public float harpDamage; //placeholder
    public float harpSpeed; //placeholder
    public float harpReloadSpeed; //placeholder

    [Header("Submarine")]
    public float subMaxHealth; //placeholder
    public float subMaxOxygen; //placeholder
    public float subMoveSpeed; //placeholder
    public float scanSpeed = 360f; 

    [Header("Torpedos")]
    public float torpDamage; //placeholder
    public float torpSpeed; //placholder
}
