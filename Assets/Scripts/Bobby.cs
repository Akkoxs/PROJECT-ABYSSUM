using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class Bobby : MonoBehaviour
{
    [Header("Player Component Reference")]
    [SerializeField] Rigidbody2D rb;

    [Header("Buoyancy Bobbing")]
    [SerializeField] float bobbingStrength = 0.5f;
    [SerializeField] float bobbingSpeed = 1f;
    [SerializeField] bool enableBobbing = true;

    private float bobbingTimer = 0f;

    // Update is called once per frame
    void FixedUpdate()
    {
            bobbingTimer += Time.fixedDeltaTime * bobbingSpeed;
            float bobbingForce = Mathf.Sin(bobbingTimer) * bobbingStrength;
            rb.AddForce(Vector2.up * bobbingForce, ForceMode2D.Force);
        
    }
}
