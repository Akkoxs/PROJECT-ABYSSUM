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
    [Header("Tilemap References and Others")]
    [SerializeField] private List<TilemapColorPair> tilemapColors = new List<TilemapColorPair>();
    [SerializeField] private SubmarineTemp submarineTemp;

    [Header("Ping Settings")]
    [SerializeField] private Color pingColor = Color.white;
    [SerializeField] private float pingDuration = 2f;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private float cooldownTime = 30f;

    [Header("Zoom Settings")]
    [SerializeField] private Camera sonarCamera;
    [SerializeField] private float normalOrthoSize = 5f;
    [SerializeField] private float pingOrthoSize = 12f;
    [SerializeField] private float zoomSpeed = 3f;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pingSound;

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
            Debug.Log("ping activated");
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

        SetAllTilemapsToColor(pingColor);

        // Zoom out
        yield return StartCoroutine(ZoomCamera(normalOrthoSize, pingOrthoSize));

        yield return new WaitForSeconds(pingDuration);

        // Fade tilemaps back
        float elapsed = 0f;
        float fadeDuration = 1f / fadeSpeed;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            FadeTilemapsToNormal(t);
            yield return null;
        }

        SetTilemapsToNormal();

        // Zoom back in
        yield return StartCoroutine(ZoomCamera(pingOrthoSize, normalOrthoSize));

        isPinging = false;
    }

    IEnumerator ZoomCamera(float from, float to)
    {
        if (sonarCamera == null) yield break;

        float elapsed = 0f;
        float duration = 1f / zoomSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            sonarCamera.orthographicSize = Mathf.Lerp(from, to, t);
            yield return null;
        }

        sonarCamera.orthographicSize = to;
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