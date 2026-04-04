using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using TMPro;

public class PingSonarSystem : MonoBehaviour
{
    [Header("Tilemap References")]
    [SerializeField] private Tilemap[] tilemapsToLight;

    [Header("Ping Settings")]
    [SerializeField] private Color normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
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

    private bool isPinging = false;
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private Color[] originalColors;

    void Start()
    {
        if (tilemapsToLight != null && tilemapsToLight.Length > 0)
        {
            originalColors = new Color[tilemapsToLight.Length];
            for (int i = 0; i < tilemapsToLight.Length; i++)
            {
                if (tilemapsToLight[i] != null)
                {
                    originalColors[i] = tilemapsToLight[i].color;
                    tilemapsToLight[i].color = normalColor;
                }
            }
        }

        //UpdateUI();
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

            //UpdateUI();
        }

        //if (SerialHandler.Instance.ping && !isPinging && !isOnCooldown)
        //{
        //    ActivatePing();
        //}
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
        SetAllTilemapColors(pingColor);

        yield return new WaitForSeconds(pingDuration);
        float elapsed = 0f;
        float fadeDuration = 1f / fadeSpeed;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            Color currentColor = Color.Lerp(pingColor, normalColor, t);
            SetAllTilemapColors(currentColor);

            yield return null;
        }

        SetAllTilemapColors(normalColor);

        isPinging = false;
    }

    void SetAllTilemapColors(Color color)
    {
        if (tilemapsToLight == null) return;

        foreach (Tilemap tilemap in tilemapsToLight)
        {
            if (tilemap != null)
            {
                tilemap.color = color;
            }
        }
    }

    //void UpdateUI()
    //{
    //    if (cooldownText != null)
    //    {
    //        if (isOnCooldown)
    //        {
    //            cooldownText.text = $"Ping: {cooldownTimer:F1}s";
    //        }
    //        else
    //        {
    //            cooldownText.text = "Ping: Ready!";
    //        }
    //    }

    //    if (cooldownFillImage != null)
    //    {
    //        if (isOnCooldown)
    //        {
    //            cooldownFillImage.fillAmount = cooldownTimer / cooldownTime;
    //        }
    //        else
    //        {
    //            cooldownFillImage.fillAmount = 1f;
    //        }
    //    }
    //}

    // Optional: Manual trigger for testing
    [ContextMenu("Test Ping")]
    public void TestPing()
    {
        ActivatePing();
    }

    //public bool IsOnCooldown()
    //{
    //    return isOnCooldown;
    //}

    //public float GetCooldownProgress()
    //{
    //    if (!isOnCooldown) return 1f;
    //    return 1f - (cooldownTimer / cooldownTime);
    //}
}