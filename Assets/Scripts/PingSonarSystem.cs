using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using TMPro;

[System.Serializable]
public class TilemapColorPair
{
    public Tilemap tilemap;
    public Color normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
}

public class PingSonarSystem : MonoBehaviour
{
    [Header("Tilemap References")]
    [SerializeField] private List<TilemapColorPair> tilemapColors = new List<TilemapColorPair>();

    [Header("Ping Settings")]
    [SerializeField] private Color pingColor = Color.white;
    [SerializeField] private float pingDuration = 2f;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private float cooldownTime = 30f;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pingSound;

    [Header("UI Feedback (Optional)")]
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private UnityEngine.UI.Image cooldownFillImage;
    [SerializeField] private SubmarineTemp submarineTemp;
    [SerializeField] private float tempCost = 15f; 
    private bool isPinging = false;
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;

    void Start()
    {
        // Set all tilemaps to their respective normal colors
        foreach (TilemapColorPair pair in tilemapColors)
        {
            if (pair.tilemap != null)
            {
                pair.tilemap.color = pair.normalColor;
            }
        }
    }

    void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                cooldownTimer = 0f;
            }
        }

        if (SerialHandler.Instance.ping && !isPinging && !isOnCooldown)
        {
            ActivatePing();
        }
    }

    public void OnPing(InputAction.CallbackContext context)
    {
        if (context.performed && !isPinging && !isOnCooldown)
        {
            ActivatePing();
        }
    }

    public void ActivatePing()
    {
        submarineTemp.AddFlatHeat(tempCost);
        if (isPinging || isOnCooldown) return;

        if (audioSource != null && pingSound != null)
        {
            audioSource.PlayOneShot(pingSound);
        }

        StartCoroutine(PingCoroutine());

        isOnCooldown = true;
        cooldownTimer = cooldownTime;

        Debug.Log("Ping activated!");
    }

    IEnumerator PingCoroutine()
    {
        isPinging = true;

        // Flash all tilemaps to ping color
        SetAllTilemapsToColor(pingColor);

        yield return new WaitForSeconds(pingDuration);

        float elapsed = 0f;
        float fadeDuration = 1f / fadeSpeed;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // Fade each tilemap back to its individual normal color
            FadeTilemapsToNormal(t);

            yield return null;
        }

        // Ensure all tilemaps are back to their exact normal colors
        SetTilemapsToNormal();

        isPinging = false;
    }

    void SetAllTilemapsToColor(Color color)
    {
        foreach (TilemapColorPair pair in tilemapColors)
        {
            if (pair.tilemap != null)
            {
                pair.tilemap.color = color;
            }
        }
    }

    void FadeTilemapsToNormal(float t)
    {
        foreach (TilemapColorPair pair in tilemapColors)
        {
            if (pair.tilemap != null)
            {
                Color fadedColor = Color.Lerp(pingColor, pair.normalColor, t);
                pair.tilemap.color = fadedColor;
            }
        }
    }

    void SetTilemapsToNormal()
    {
        foreach (TilemapColorPair pair in tilemapColors)
        {
            if (pair.tilemap != null)
            {
                pair.tilemap.color = pair.normalColor;
            }
        }
    }

    [ContextMenu("Test Ping")]
    public void TestPing()
    {
        ActivatePing();
    }
}