using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeTracker", menuName = "Scriptable Objects/UpgradeTracker")]
public class UpgradeTracker : ScriptableObject
{
    private int maxLevel = 4; 

    [Header("Current Upgrade Levels")]
    public int LVL_HP = 0;
    public int LVL_TANK = 0;
    public int LVL_SWIM = 0;
    public int LVL_HULL = 0;
    public int LVL_O2 = 0;
    public int LVL_PROP = 0;
    public int LVL_SONR = 0;
    public int LVL_TORP = 0;
    public int LVL_HARP = 0;
    public int LVL_RLD = 0;

    public int GetArtifactLevel(ArtifactType type)
    {
        switch (type)
        {
            case ArtifactType.HP:
                return LVL_HP;

            case ArtifactType.TANK:
                return LVL_TANK;

            case ArtifactType.SWIM:
                return LVL_SWIM;

            case ArtifactType.HULL:
                return LVL_HULL;

            case ArtifactType.O2:
                return LVL_O2;

            case ArtifactType.PROP:
                return LVL_PROP;

            case ArtifactType.SONAR:
                return LVL_SONR;

            case ArtifactType.TORP:
                return LVL_TORP;

            case ArtifactType.HARP:
                return LVL_HARP;

            case ArtifactType.RLD:
                return LVL_RLD;
            
            default:
                return 99;
            //there exists a case where an unupgradable artifact can be be passed into this
        }
    }

    public bool IsUpgradableArtifact(ArtifactType type)
    {
        switch (type)
        {
            case ArtifactType.MEDKIT:
            case ArtifactType.HULL_PATCH_KIT:
            case ArtifactType.TORP_RACK:
            case ArtifactType.OXY_TANKS:
                return false;

            default:
                return true;
        }
    }
    
    public void IncrementArtifactLevel(ArtifactType type)
    {
        if (!IsUpgradableArtifact(type))
            return;

        switch (type)
        {
            case ArtifactType.HP:
            LVL_HP = Mathf.Min(maxLevel, LVL_HP + 1);
            break;

            case ArtifactType.TANK:
            LVL_TANK = Mathf.Min(maxLevel, LVL_TANK + 1);
            break;

            case ArtifactType.SWIM:
            LVL_SWIM = Mathf.Min(maxLevel, LVL_SWIM + 1);
            break;

            case ArtifactType.HULL:
            LVL_HULL = Mathf.Min(maxLevel, LVL_HULL + 1);
            break;

            case ArtifactType.O2:
            LVL_O2 = Mathf.Min(maxLevel, LVL_O2 + 1);
            break;

            case ArtifactType.PROP:
            LVL_PROP = Mathf.Min(maxLevel, LVL_PROP + 1);
            break;

            case ArtifactType.SONAR:
            LVL_SONR = Mathf.Min(maxLevel, LVL_SONR + 1);
            break;

            case ArtifactType.TORP:
            LVL_TORP = Mathf.Min(maxLevel, LVL_TORP + 1);
            break;

            case ArtifactType.HARP:
            LVL_HARP = Mathf.Min(maxLevel, LVL_HARP + 1);
            break;

            case ArtifactType.RLD:
            LVL_RLD = Mathf.Min(maxLevel, LVL_RLD + 1);
            break;
        }
    }

    public void ResetAll()
    {
        LVL_HP = 0;
        LVL_TANK = 0;
        LVL_SWIM = 0;
        LVL_HULL = 0;
        LVL_O2 = 0;
        LVL_PROP = 0;
        LVL_SONR = 0;
        LVL_TORP = 0;
        LVL_HARP = 0;
        LVL_RLD = 0;
    }











    


}
