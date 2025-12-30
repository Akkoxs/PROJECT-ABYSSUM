using UnityEngine;

[CreateAssetMenu(fileName = "GameStats", menuName = "Scriptable Objects/GameStats")]
public class GameStats : ScriptableObject
{
    [Header("Diver")]
    public float diverMaxHealth = 100f;
    public float diverMaxOxygen = 100f;
    public float diverMoveSpeed; //placeholder 

    [Header("Submarine")]
    public float subMaxHealth = 500f; //placeholder
    public float subMaxOxygen = 500f; //placeholder
    public float subMoveSpeed; //placeholder
    public float scanSpeed = 135f; 

    [Header("Harpoon")]
    public float harpDamage; //placeholder
    public float harpSpeed; //placeholder
    public float harpReloadSpeed; //placeholder

    [Header("Torpedos")]
    public float torpDamage; //placeholder
    public float torpSpeed; //placholder
}
