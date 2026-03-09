using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Sonar : MonoBehaviour
{
    [SerializeField] private UIHelper uiHelper;
    [SerializeField] private SonarPing pfSonarPOI;
    [SerializeField] private SonarPing pfSonarTerrain;
    [SerializeField] private GameObject submarine;
    [SerializeField] private RectTransform radarRect;

    //layer masks
    [SerializeField] private LayerMask layerMaskPOI; //must configure 
    [SerializeField] private LayerMask layerMaskTerrain;

    private Transform sweepTransform;
    private List<Collider2D> colliderList;
    private Submarine sub;

    [SerializeField] private float scanSpeed = 180f;
    [SerializeField] private float sonarRange = 500f; 
    [SerializeField] private int terrainRaysPerFrame = 10;
    [SerializeField] private float terrainRaySpread = 15f; //15 deg cone

    private void Awake()
    {
        sweepTransform = transform.Find("Sweeper");
        sub = submarine.GetComponent<Submarine>();
        colliderList = new List<Collider2D>();
    }

    private void Update()
    {
        float previousRotation = (sweepTransform.eulerAngles.z % 360) - 180;
        sweepTransform.eulerAngles -= new Vector3(0, 0, scanSpeed * Time.deltaTime);
        float currentRotation = (sweepTransform.eulerAngles.z % 360) - 180;

        if ((previousRotation < -90 && currentRotation >= -90) ||
            (previousRotation < 0 && currentRotation >= 0) ||
            (previousRotation < 90 && currentRotation >= 90))
        {
            colliderList.Clear();
        }

        float baseAngle = sweepTransform.eulerAngles.z;

        DetectPOI(baseAngle);
        DetectTerrain(baseAngle);   
    }

    private void DetectPOI(float angle)
    {
        Vector2 direction = uiHelper.GetVectorFromAngle(angle);
        RaycastHit2D[] hits = Physics2D.RaycastAll(submarine.transform.position, direction, sonarRange, layerMaskPOI);
            
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && !colliderList.Contains(hit.collider)) // hit something that we havent alr hit
            //if (hit.collider != null) // hit something that we havent alr hit
            {
                colliderList.Add(hit.collider);
                IRadarDetectable hitObj = hit.collider.gameObject.GetComponent<IRadarDetectable>(); 

                if (hitObj != null) //hit something detectable
                {
                    RadarObjectType objectType = hitObj.GetObjectType();

                    if (objectType == RadarObjectType.Fauna || objectType == RadarObjectType.Artifact)
                    {
                        string artifactName = hitObj.GetRadarDisplayName();
                        CreatePing(hit.point, objectType, artifactName);   
                    }
                }                   
            }
        }
    }

    private void DetectTerrain(float angle) //angle = base starting angle  
    {
        for (int i = 0; i < terrainRaysPerFrame; i++)
        {
            float angleOffset = -terrainRaySpread / 2 + (terrainRaySpread / (terrainRaysPerFrame - 1))* i; //for placement of next ray
            float rayAngle = angle + angleOffset; 

            Vector2 direction = uiHelper.GetVectorFromAngle(rayAngle);
            RaycastHit2D[] hits = Physics2D.RaycastAll(submarine.transform.position, direction, sonarRange, layerMaskTerrain);

            foreach (RaycastHit2D hit in hits)
            {
                Debug.Log(hit);
                if (hit.collider != null) //&& !colliderList.Contains(hit.collider)
                {
                    CreatePing(hit.point, RadarObjectType.Terrain, null);
                }
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
        SonarPing sonarPing = Instantiate(pingPrefab, radarRect);
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

                    // Draw the surface normal at hit point
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(hit.point, hit.point + hit.normal * 2f);
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
