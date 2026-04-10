using System.Collections;
using UnityEngine;

public class ModulationMinigame : MonoBehaviour
{
    //Actually used in the modulation minigame 

    [Header("Identity")]
    public string minigameID = "modulation_minigame_01";
    public string playerTag = "Player";

    [Header("Game Tuning")]
    public float successBuffer = 0.08f;
    public float holdDuration = 0.5f;
    public float completionDelay = 1.5f;

    [Header("Floating Text")]
    public GameObject floatingTextPrefab;
    public string entryMessage = "MOVE TO ADM";
    public string completionMessage = "DECRYPTION SUCCESSFUL";

    //0 = rotary 
    // 1 = horizontal Slider 
    // 2 = vertical Slider
    private readonly int[] channelModes = { 0, 1, 0, 2 }; //arrangement 

    private float[] targets = new float[4];
    private bool[] inRange = new bool[4];
    private bool completed = false;
    private bool started = false;
    private bool playerInZone = false;
    private float holdTimer = 0f;

    public System.Action OnCompleted;

    void Update()
    {
        //minigame logic only runs when player is in the zone or the game is complete
        if (!playerInZone || completed) return;

        float[] current = GetCurrentValues();

        //calculate for each target if we are within the sweetspot 
        for (int i = 0; i < 4; i++)
        {
            inRange[i] = Mathf.Abs(current[i] - targets[i]) <= successBuffer;
        }

        //ensure globalMOdulationUi singleton
        if (GlobalModulationUI.Instance != null)
        {
            GlobalModulationUI.Instance.UpdateVisuals(current, inRange, channelModes);
        }

        CheckCompletion();
    }

    //triger the minigame when we enter the collider 
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

        //spawn floating text entry message
        SpawnFloatingText(other.transform, entryMessage);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        
        playerInZone = false;
        
        //turn off the global UI if they step out
        if (GlobalModulationUI.Instance != null)
            GlobalModulationUI.Instance.DeactivateUI();
    }

    //fetch the current vals from the SerialHandler
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

        yield return new WaitForSeconds(completionDelay); //give player some time to move back to other screen

        if (GlobalModulationUI.Instance != null)
        {
            GlobalModulationUI.Instance.DeactivateUI();
        }

        //spawn completion text & find the player by tag
        GameObject player = GameObject.FindWithTag(playerTag);
        if (player != null) SpawnFloatingText(player.transform, completionMessage);

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