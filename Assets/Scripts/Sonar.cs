using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class Sonar : MonoBehaviour
{
    [SerializeField] private UIHelper uiHelper;
    [SerializeField] private SonarPing pfSonarPOI;
    [SerializeField] private SonarPing pfSonarTerrain;
    [SerializeField] private GameObject submarine;
    [SerializeField] private RectTransform radarRect;
    [SerializeField] private RectTransform artifactDetects;

    //layer masks
    [SerializeField] private LayerMask layerMaskPOI; //must configure 
    [SerializeField] private LayerMask layerMaskTerrain;

    private Transform sweepTransform;
    private List<Collider2D> colliderList;
    private HashSet<int> detectedInstanceIDs = new HashSet<int>(); //hash set for storing detected obj.
    private Submarine sub;

    [SerializeField] private float scanSpeed = 180f;
    [SerializeField] private float sonarRange = 500f; 
    [SerializeField] private int terrainRaysPerFrame = 10;
    [SerializeField] private float terrainRaySpread = 15f; //15 deg cone
    [SerializeField] private float poiDetectionRadius = 7f; //for the raycast overlap circles to detect Artifact

    private float totalRotation = 0f;
    private float lastClearRotation = 0f;
    private float lastSweepAngle;

    private void Awake()
    {
        sweepTransform = transform.Find("Sweeper");
        sub = submarine.GetComponent<Submarine>();
        colliderList = new List<Collider2D>();
        lastSweepAngle = sweepTransform.eulerAngles.z; //new
    }

private void Update()
    {
        float rotationThisFrame = scanSpeed * Time.deltaTime;
        totalRotation += rotationThisFrame;
        sweepTransform.eulerAngles -= new Vector3(0, 0, rotationThisFrame);

    if (totalRotation - lastClearRotation >= 360f) //clear once per rot.
    {
        lastClearRotation += 360f;
        detectedInstanceIDs.Clear();
    }

        float currentAngle = sweepTransform.eulerAngles.z;

        DetectTerrain(currentAngle);
        DetectPOIArc(lastSweepAngle, currentAngle);

        lastSweepAngle = currentAngle;
    }

private void DetectPOIArc(float fromAngle, float toAngle)
    {
        // How many rays to cast across the arc this frame, # of rays is dependent on framrate so its gotta be ~60fps for good performance. 
        int raysThisFrame = Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(scanSpeed * Time.deltaTime) / 2f));

        for (int i = 0; i <= raysThisFrame; i++)
        {
            float t = raysThisFrame == 0 ? 0f : (float)i / raysThisFrame; //if raysThisFrame == 0, t=0f, else t=float(i)/raysThisFrame;
            float angle = Mathf.LerpAngle(fromAngle, toAngle, t);

            DetectPOI(angle);
        }
    }

private void DetectPOI(float angle)
{
    Vector2 direction = uiHelper.GetVectorFromAngle(angle);
    
    // sample multiple points along the ray, put overlap circle raycasts along the line segment to have a better chance at hitting Artifact colliders
    int steps = 12;
    for (int i = 1; i <= steps; i++)
    {
        float distance = sonarRange * ((float)i / steps);
        Vector2 samplePoint = (Vector2)submarine.transform.position + direction * distance;
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(samplePoint, poiDetectionRadius, layerMaskPOI);
        
        foreach (Collider2D col in hits)
        {
            if (col == null) continue;
            
            int instanceID = col.gameObject.GetInstanceID();
            if (detectedInstanceIDs.Contains(instanceID)) continue;
            
            detectedInstanceIDs.Add(instanceID);
            
            IRadarDetectable hitObj = col.gameObject.GetComponent<IRadarDetectable>();
            if (hitObj != null)
            {
                RadarObjectType objectType = hitObj.GetObjectType();
                if (objectType == RadarObjectType.Fauna || objectType == RadarObjectType.Artifact)
                {
                    CreatePing(col.transform.position, objectType, hitObj.GetRadarDisplayName());
                }
            }
        }
    }
}

    private void DetectTerrain(float angle)
    {
        for (int i = 0; i < terrainRaysPerFrame; i++)
        {
            float angleOffset = -terrainRaySpread / 2 + (terrainRaySpread / (terrainRaysPerFrame - 1)) * i; //for placement of next ray 
            float rayAngle = angle + angleOffset;
            Vector2 direction = uiHelper.GetVectorFromAngle(rayAngle);

            List<Vector2> hitPoints = GetTerrainHitPoints(submarine.transform.position, direction, sonarRange);

            foreach (Vector2 point in hitPoints)
            {
                CreatePing(point, RadarObjectType.Terrain, null);
            }
        }
    }

    private void CreatePing(Vector2 hitPoint, RadarObjectType objectType, string artifactName)
    {
        SonarPing pingPrefab;
        Color pingColor;
        string label = null;

        switch (objectType)
        {
            case RadarObjectType.Fauna:
                pingPrefab = pfSonarPOI;
                pingColor = Color.red;
                break;

            case RadarObjectType.Artifact:
                pingPrefab = pfSonarPOI;
                pingColor = Color.purple;
                label = artifactName;
                break;

            case RadarObjectType.Terrain:
            default:
                pingPrefab = pfSonarTerrain;
                pingColor = Color.white;
                break;
        }
        
        //this is to ensure that artifact labels render ABOVE the terrain points 
        RectTransform parent = radarRect; 
        Vector2 radarPos = WorldToRadarPosition(hitPoint);

        if (objectType == RadarObjectType.Artifact)
        {
            if (!artifactDetects.rect.Contains(radarPos))
            {
                return; // don't spawn if outside bounds
            }
            parent = artifactDetects;
        }
        
        SonarPing sonarPing = Instantiate(pingPrefab, parent);
        sonarPing.transform.localPosition = WorldToRadarPosition(hitPoint);
        sonarPing.SetColor(pingColor);
        sonarPing.SetText(label);
        sonarPing.SetDissapearTimer(360f / scanSpeed);//pings dissapear by 1 rotation
    }

    public void SetScanSpeed(float newScanSpeed)
    {
        scanSpeed = newScanSpeed;
    }

    private Vector2 WorldToRadarPosition(Vector2 worldPos)
    {
        Vector2 offset = worldPos - (Vector2)submarine.transform.position; //vector from sub to radar hit
        float distance = Mathf.Min(offset.magnitude, sonarRange); //clamp 
        Vector2 direction = offset.normalized; 

        float radarRadius = radarRect.rect.width * 0.5f; //what is this 
        float radarDistance = (distance / sonarRange) * radarRadius;

        return direction * radarDistance;
    }

    //this is for handling the fact that all terrain is technically 1 collider because of the composite tilemap collider. So we can see internal tunnels and stuff.
    private List<Vector2> GetTerrainHitPoints(Vector2 origin, Vector2 direction, float maxRange)
    {
        List<Vector2> hitPoints = new List<Vector2>();
        float distanceTravelled = 0f;
        Vector2 currentOrigin = origin;

        //for every hit surface, we are starting a new raycast from this point onwards to get the next hit and so forth, until the max sonar range is reached
        while (distanceTravelled < maxRange)
        {
            float remainingRange = maxRange - distanceTravelled;
            RaycastHit2D hit = Physics2D.Raycast(currentOrigin, direction, remainingRange, layerMaskTerrain);

            if (hit.collider == null) break;

            hitPoints.Add(hit.point);

            //start the next ray a bit after  
            float nudge = 0.05f;
            currentOrigin = hit.point + direction * nudge;
            distanceTravelled += hit.distance + nudge;
        }

        return hitPoints;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || sweepTransform == null) return;

        float baseAngle = sweepTransform.eulerAngles.z;

        for (int i = 0; i < terrainRaysPerFrame; i++)
        {
            float angleOffset = -terrainRaySpread / 2 + (terrainRaySpread / (terrainRaysPerFrame - 1)) * i;
            float rayAngle = baseAngle + angleOffset;

            Vector2 direction = uiHelper.GetVectorFromAngle(rayAngle);
            Vector2 origin = submarine.transform.position;

            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, sonarRange, layerMaskTerrain);

            if (hits.Length > 0)
            {
                // Draw ray up to first hit in yellow
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(origin, hits[0].point);

                foreach (RaycastHit2D hit in hits)
                {
                    // Draw hit point as a sphere
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(hit.point, 0.3f);

                }

                // Draw remaining ray after last hit in grey
                Gizmos.color = Color.grey;
                Gizmos.DrawLine(hits[hits.Length - 1].point, origin + direction * sonarRange);
            }
            else
            {
                // No hit, draw full ray in green
                Gizmos.color = Color.green;
                Gizmos.DrawLine(origin, origin + direction * sonarRange);
            }
        }
    }

}
