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
        if (objectRenderer == null)
        {
            Debug.LogWarning($"FlashHit on '{name}' has no Renderer — flash disabled.", this);
            return;
        }
        material = objectRenderer.material;
        originalColor = material.color;
    }

    public void TriggerFlash()
    {
        if (material == null) return; // no renderer/material to flash; don't throw
        StartCoroutine(FlashRed());
    }

    IEnumerator FlashRed()
    {
        material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        material.color = originalColor;
    }
}