public interface IRadarDetectable
{    
    string GetRadarDisplayName();
    RadarObjectType GetObjectType();
}

public enum RadarObjectType
{
    Terrain,
    Fauna,
    Flora,
    Artifact
}