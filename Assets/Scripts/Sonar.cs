using System.Collections.Generic;
using UnityEngine;

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
        if(sub.PlayerInside)
        {
            float previousRotation = (sweepTransform.eulerAngles.z % 360) - 180;
            sweepTransform.eulerAngles -= new Vector3(0, 0, scanSpeed * Time.deltaTime);
            float currentRotation = (sweepTransform.eulerAngles.z % 360) - 180;

            if (previousRotation < 0 && currentRotation >= 0) //half rotation
            {
                colliderList.Clear();
            }

            float baseAngle = sweepTransform.eulerAngles.z;

            DetectPOI(baseAngle);
            DetectTerrain(baseAngle);   
        }
    }

    private void DetectPOI(float angle)
    {
        Vector2 direction = uiHelper.GetVectorFromAngle(angle);
        RaycastHit2D[] hits = Physics2D.RaycastAll(submarine.transform.position, direction, sonarRange, layerMaskPOI);
            
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && !colliderList.Contains(hit.collider)) // hit something that we havent alr hit
            {
                colliderList.Add(hit.collider);
                IRadarDetectable hitObj = hit.collider.gameObject.GetComponent<IRadarDetectable>(); 

                if (hitObj != null) //hit something detectable
                {
                    RadarObjectType objectType = hitObj.GetObjectType();

                    if (objectType == RadarObjectType.Fauna || objectType == RadarObjectType.Artifact)
                    {
                        CreatePing(hit.point, objectType);   
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
                if (hit.collider != null && !colliderList.Contains(hit.collider))
                {
                    CreatePing(hit.point, RadarObjectType.Terrain);
                }
            }
        }
    }

    private void CreatePing(Vector2 hitPoint, RadarObjectType objectType)
    {
        SonarPing pingPrefab;
        Color pingColor;

        switch (objectType)
        {
            case RadarObjectType.Fauna:
                pingPrefab = pfSonarPOI;
                pingColor = Color.red;
                break;

            case RadarObjectType.Artifact:
                pingPrefab = pfSonarPOI;
                pingColor = Color.purple;
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

}
