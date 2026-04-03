public interface IRadarDetectable
{    
    string GetRadarDisplayName(); //returns display name 
    RadarObjectType GetObjectType();
}

public enum RadarObjectType //returns obj. type
{
    Terrain,
    Fauna,
    Flora,
    Artifact
}