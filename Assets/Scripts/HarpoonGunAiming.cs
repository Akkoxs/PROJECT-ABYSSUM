using UnityEngine;

public class HarpoonGunAiming : MonoBehaviour
{
    [SerializeField] private MouseAiming mouseAiming;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject lightContainer;
    private Vector3 mousePos;
    private float angleThreshold = 15f;

    void Update()
    {
        mousePos = mouseAiming.GetMousePos();
        Vector2 direction = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 180f);

            if (direction.x > 0)
            {
                spriteRenderer.flipY = true;
                spriteRenderer.transform.localPosition = new Vector3(0.063f, spriteRenderer.transform.localPosition.y, spriteRenderer.transform.localPosition.z);
                lightContainer.transform.localPosition = new Vector3(0.063f, lightContainer.transform.localPosition.y, lightContainer.transform.localPosition.z);
            }
            else
            {
                spriteRenderer.flipY = false;
                spriteRenderer.transform.localPosition = new Vector3(-0.063f, spriteRenderer.transform.localPosition.y, spriteRenderer.transform.localPosition.z);
                lightContainer.transform.localPosition = new Vector3(-0.063f, lightContainer.transform.localPosition.y, lightContainer.transform.localPosition.z);
            }
    }
}
