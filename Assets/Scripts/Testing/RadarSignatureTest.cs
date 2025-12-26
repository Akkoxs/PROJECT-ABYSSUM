using UnityEngine;

public class RadarSignatureTest : MonoBehaviour, IRadarDetectable
{
    
    public string GetRadarDisplayName()
    {
        return null;
    }

    public RadarObjectType GetObjectType()
    {
        return RadarObjectType.Terrain;
    }

}
