using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{

//[SerializeField] ArtifactSpawnPoint artifactSpawnPoint;
[SerializeField ]public List<ArtifactSpawnPoint> artifactSpawns = new List<ArtifactSpawnPoint>(); 

    //get all spawn points
    public int GetTotalSpawnPoints()
    {
        return artifactSpawns.Count;
    }

    //Get all occupied spawn points 
    public int GetOccupiedSpawnPoints()
    {
        return artifactSpawns.Count(sp => sp.IsOccupied); 
        //LINQ (lang. integrated query), count all obj. in artifactSpawns that are occupied.
    }

    //get all spawn points not alr occupied 
    public int GetAvailableSpawnPoints()
    {
        return artifactSpawns.Count(sp => !sp.IsOccupied);
    }

    //Given an integer representing spawn level, find all spawn points that correspond to it that are not occupied 
    public List<ArtifactSpawnPoint> GetAvailableSpawnPointsAtLevel(int level)
    {
        return artifactSpawns.Where(sp => sp.SpawnLevel == level && !sp.IsOccupied).ToList();
        // LINQ, find all obj. in artifactSpawns that match with the method arg and are not alr occupied
    }

    //Get all possible spawn points for an array of levels
    public List<ArtifactSpawnPoint> GetAvailableSpawnPointsForLevels(int[] levels)
    {
        List<ArtifactSpawnPoint> validSpawns = new List<ArtifactSpawnPoint>();

        foreach (int level in levels)
        {
            validSpawns.AddRange(GetAvailableSpawnPointsAtLevel(level));
        }

        return validSpawns;
    }

    //Given an artifact, get all places it can spawn and pick a random one.
    public ArtifactSpawnPoint GetSpawnPointForArtifact(ArtifactStats artifact)
    {
        List<ArtifactSpawnPoint> validSpawns = GetAvailableSpawnPointsForLevels(artifact.spawnLevels);
        
        return validSpawns[Random.Range(0, validSpawns.Count)];
    }

}
