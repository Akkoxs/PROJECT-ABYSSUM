using UnityEngine;

[CreateAssetMenu(fileName = "ArtifactStats", menuName = "Scriptable Objects/ArtifactStats")]
public class ArtifactStats : ScriptableObject
{
    [Header("General")]
    public string artifactName;
    public ArtifactType artifactType;
    public Sprite icon;
    public int sellValue;

    [Header("Spawning")]
    public int[] spawnLevels;

    [Header("Stat Effects")]
    public StatEffect[] statEffects; //array of changing stats

    [Header("Special Logic")]
    public bool replenishPlayerHealth;
    public bool replenishSubHealth;
    public bool replenishPlayerOxygen;
    public bool replenishSubOxygen;
    public bool replenishTorpedo;
    public bool grantExtraTorpedo;
}

[System.Serializable]
public struct StatEffect //pair stat changes to magnitude 
{
    public StatType stat; 
    public float value; 
}

public enum StatType //all types of stats
{
    DiverHealth, 
    DiverOxygen, 
    SubHealth, 
    SubOxygen,
    SonarSpeed,
    TorpedoDamage,
    TorpedoSpeed,
    HarpoonDamage,
    HarpoonSpeed,
    HarpoonReload,
    DiverSpeed,
    SubSpeed
}

public enum ArtifactType
{
    //replenish artifacts 
    TORP_RACK,
    MEDKIT,
    HULL_PATCH_KIT,
    OXY_TANKS,

    //upgrade artifacts 
    HP,
    HULL,
    TANK,
    O2,
    SONAR,
    TORP,
    HARP,
    RLD,
    SWIM,
    PROP
}
