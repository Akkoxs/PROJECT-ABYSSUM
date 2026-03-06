using UnityEngine;

[DefaultExecutionOrder(-10)] 
public class ParallaxBackground : MonoBehaviour
{
    [Header("Global Water Surface")]
    public float globalWaterSurfaceY = 0f;

    [Header("Global X Bounds Left(-) Right (+)")]
    public float globalBoundsMinX = -50f;
    public float globalBoundsMaxX =  50f;

    [Header("Global Y Bounds Down(-) Up (+)")]
    public float globalBoundsMinY = -5f;
    public float globalBoundsMaxY =  30f;

    //BCKGND ORDER Furthest to closest 
    // 0 - Night Sky
    // 1 - Moon
    // 2 - Urchins
    // 3 - BG clouds
    // 4 - Front clouds
    // 5 - Craggy peaks

    [Header("Per-Layer Parallax Speeds (furthest -> closest)")]
    public float[] layerParallaxFactorsX = { 0.05f, 0.15f, 0.25f, 0.40f, 0.60f, 0.80f };
    public float[] layerParallaxFactorsY = { 0f, 0f, 0.02f, 0.03f, 0.05f, 0.07f };

    private ParallaxLayer[] layers;

    private void Awake()
    {
        CollectAndConfigureLayers();
    }

    /// collects all ParallaxLayer children and applies the global settings to them
    public void CollectAndConfigureLayers()
    {
        layers = GetComponentsInChildren<ParallaxLayer>();

        for (int i = 0; i < layers.Length; i++)
        {
            ParallaxLayer layer = layers[i];

            // apply global water surface limit
            layer.waterSurfaceY = globalWaterSurfaceY;

            // apply all global bounds
            layer.boundsMinX = globalBoundsMinX;
            layer.boundsMaxX = globalBoundsMaxX;
            layer.boundsMinY = globalBoundsMinY;
            layer.boundsMaxY = globalBoundsMaxY;

            // apply per-layer speed factors from layerParallaxFactorsX & Y
            if (i < layerParallaxFactorsX.Length)
                layer.parallaxFactorX = layerParallaxFactorsX[i];

            if (i < layerParallaxFactorsY.Length)
                layer.parallaxFactorY = layerParallaxFactorsY[i];
        }
    }

    /// adjust the water surface Y pos. at runtime
    public void SetWaterSurfaceY(float newY)
    {
        globalWaterSurfaceY = newY;
        if (layers == null) return;
        foreach (var layer in layers)
            layer.waterSurfaceY = newY;
    }

    //helping with viz 
    private void OnDrawGizmos()
    {
        // backgorund bounding box
        Gizmos.color = new Color(1f, 0.9f, 0.1f, 0.25f);
        Vector3 center = new Vector3(
            (globalBoundsMinX + globalBoundsMaxX) * 0.5f,
            (globalBoundsMinY + globalBoundsMaxY) * 0.5f,
            transform.position.z);
        Vector3 size = new Vector3(
            globalBoundsMaxX - globalBoundsMinX,
            globalBoundsMaxY - globalBoundsMinY,
            0.1f);
        Gizmos.DrawWireCube(center, size);

        // water surface line
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.9f);
        Gizmos.DrawLine(
            new Vector3(globalBoundsMinX, globalWaterSurfaceY, transform.position.z),
            new Vector3(globalBoundsMaxX, globalWaterSurfaceY, transform.position.z));
    }
}
