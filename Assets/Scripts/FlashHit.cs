using System.Collections;
using UnityEngine;

public class FlashHit : MonoBehaviour
{
    private Renderer objectRenderer;
    private Material material;
    private Color originalColor;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        material = objectRenderer.material;
        originalColor = material.color;
    }

    public void TriggerFlash()
    {
        StartCoroutine(FlashRed());
    }

    IEnumerator FlashRed()
    {
        material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        material.color = originalColor;
    }
}