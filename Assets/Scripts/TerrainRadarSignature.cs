using UnityEngine;

public class TerrainRadarSignature : MonoBehaviour, IRadarDetectable
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
