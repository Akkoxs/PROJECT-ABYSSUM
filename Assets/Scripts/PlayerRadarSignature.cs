using UnityEngine;

public class PlayerRadarSignature : MonoBehaviour, IRadarDetectable
{
    
    public string GetRadarDisplayName()
    {
        return null;
    }

    public RadarObjectType GetObjectType()
    {
        return RadarObjectType.Player;
    }

}
