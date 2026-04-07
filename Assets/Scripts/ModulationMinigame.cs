using System.Collections;
using UnityEngine;

public class ModulationMinigame : MonoBehaviour
{
    [Header("Identity")]
    public string minigameID = "modulation_minigame_01";

    [Header("Tuning")]
    public float successBuffer = 0.08f;
    public float holdDuration = 0.5f;

    [Header("Trigger")]
    public string playerTag = "Player";

    [Header("Floating Text")]
    public GameObject floatingTextPrefab;
    public string entryMessage = "MOVE TO ADM";
    public string completionMessage = "DECRYPTION SUCCESSFUL";

    // 0 = Rotary, 1 = Horizontal Slider, 2 = Vertical Slider
    private readonly int[] channelModes = { 0, 1, 0, 2 };

    private float[] targets = new float[4];
    private bool[] inRange = new bool[4];
    private bool completed = false;
    private bool started = false;
    private bool playerInZone = false;
    private float holdTimer = 0f;

    public System.Action OnCompleted;

    void Update()
    {
        // Only run minigame logic if the player is physically standing in the zone
        if (!playerInZone || completed) return;

        float[] current = GetCurrentValues();

        // Calculate hits
        for (int i = 0; i < 4; i++)
        {
            inRange[i] = Mathf.Abs(current[i] - targets[i]) <= successBuffer;
        }

        // Send the data to the Global UI screen
        if (GlobalModulationUI.Instance != null)
        {
            GlobalModulationUI.Instance.UpdateVisuals(current, inRange, channelModes);
        }

        CheckCompletion();
    }

    // void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (completed || !other.CompareTag(playerTag)) return;

    //     playerInZone = true;

    //     // The math stays, but the player never sees these numbers!
    //     for (int i = 0; i < 4; i++)
    //         targets[i] = Random.Range(0.1f, 0.9f);

    //     if (GlobalModulationUI.Instance != null)
    //         GlobalModulationUI.Instance.ActivateUI(); // No targets passed!

    //     if (!started)
    //     {
    //         started = true;
    //         //MinigameProgressManager.Instance.SetMinigameStarted(minigameID);
    //     }
    // }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (completed || !other.CompareTag(playerTag)) return;

        playerInZone = true;

        for (int i = 0; i < 4; i++)
            targets[i] = Random.Range(0.1f, 0.9f);

        if (GlobalModulationUI.Instance != null)
            GlobalModulationUI.Instance.ActivateUI();

        if (!started)
        {
            started = true;
        }

        // Spawn floating text above the player
        SpawnFloatingText(other.transform, entryMessage);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        
        playerInZone = false;
        
        // Turn off the global UI if they step out
        if (GlobalModulationUI.Instance != null)
            GlobalModulationUI.Instance.DeactivateUI();
    }

    float[] GetCurrentValues()
    {
        SerialHandler sh = SerialHandler.Instance;
        return new float[] { sh.playerPot_a, sh.playerSlider_h, sh.playerPot_k, sh.playerSlider_c };
    }

    void CheckCompletion()
    {
        foreach (bool b in inRange)
            if (!b) { holdTimer = 0f; return; }

        holdTimer += Time.deltaTime;
        if (holdTimer >= holdDuration)
            StartCoroutine(Complete());
    }

    IEnumerator Complete()
    {
        completed = true;
        OnCompleted?.Invoke();

        // Spawn completion text — find the player by tag
        GameObject player = GameObject.FindWithTag(playerTag);
        if (player != null) SpawnFloatingText(player.transform, completionMessage);

        yield return new WaitForSeconds(1.5f);

        if (GlobalModulationUI.Instance != null)
            GlobalModulationUI.Instance.DeactivateUI();

        Destroy(gameObject);
    }

    //helper

    void SpawnFloatingText(Transform target, string message)
    {
        if (floatingTextPrefab == null) return;

        Vector3 spawnPos = target.position + Vector3.up * 1.5f;
        GameObject ft = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);
        ft.GetComponent<FloatingText>().Init(message);
    }


}