using System.Collections.Generic;
using UnityEngine;

public class Sonar : MonoBehaviour
{
    [SerializeField] private UIHelper uiHelper;
    [SerializeField] private SonarPing pfSonarPing;

    private Transform sweepTransform;
    private List<Collider2D> colliderList;

    [SerializeField] private float scanSpeed = 180f;
    [SerializeField] private float sonarRange = 500f; 

    private void Awake()
    {
        sweepTransform = transform.Find("Sweeper");
        colliderList = new List<Collider2D>();
    }

    private void Update()
    {
        float previousRotation = (sweepTransform.eulerAngles.z % 360) - 180;
        sweepTransform.eulerAngles -= new Vector3(0, 0, scanSpeed * Time.deltaTime);
        float currentRotation = (sweepTransform.eulerAngles.z % 360) - 180;

        if (previousRotation < 0 && currentRotation >= 0) //half rotation
        {
            colliderList.Clear();
        }

        RaycastHit2D rayCastHit2D = Physics2D.Raycast(transform.position, uiHelper.GetVectorFromAngle(sweepTransform.eulerAngles.z), sonarRange);
        if (rayCastHit2D.collider != null) // hit something 
        {
            if (!colliderList.Contains(rayCastHit2D.collider)) //if we havent alr hit it
            {
                colliderList.Add(rayCastHit2D.collider);
                SonarPing sonarPing = Instantiate(pfSonarPing, rayCastHit2D.point, Quaternion.identity).GetComponent<SonarPing>();

                if (rayCastHit2D.collider.gameObject.GetComponent<IRadarDetectable>() != null)//hit something detectable
                {
                    sonarPing.SetColor(new Color(0, 1, 0));
                    //need object logic here.
                }


                //sonarPing.SetColor(0)
            }
        }
    }

    public void SetScanSpeed(float newScanSpeed)
    {
        scanSpeed = newScanSpeed;
    }

}
