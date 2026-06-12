using UnityEngine;

// Repurposes the four COM-port controls (two knobs + two sliders) as a submarine repair
// mechanic whenever the artifact modulation minigame is NOT running.
//
// When the sub rams Terrain hard enough to take damage (see Submarine.OnCollisionEnter2D),
// there is a burstChance that a random healthy pipe/valve bursts. A burst element drains the
// sub's Health over time until it is repaired.
//
// Repair is MOVEMENT based, not position based: while the bound control is actively being moved
// (a knob rotated in either direction, a slider pushed either way) the player accumulates repair
// time and a ratchet noise plays. Pausing never resets progress (potentiometers bottom out, so a
// brief stop is expected) — it just stops accumulating until movement resumes. Once a burst
// element has been moved for repairDuration seconds total, it is fixed. Multiple elements can be
// burst at once.
//
// While an artifact minigame is active (GlobalModulationUI.IsActive) the whole mechanic freezes:
// no new bursts, no repair progress, no health drain, and the repair UI hides so the modulation
// UI can take over the shared Diver Control screen.
public class PipeRepairSystem : MonoBehaviour
{
    public static PipeRepairSystem Instance { get; private set; } // singleton

    [Header("References")]
    [SerializeField] private Health subHealth;
    [SerializeField] private PipeRepairUI ui;

    [Header("Floating Text")]
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private string burstMessage = "PIPE BURST";
    [SerializeField] private string repairMessage = "REPAIRED";

    [Header("Tuning")]
    [Tooltip("Chance (0-1) that a damaging ram bursts a pipe.")]
    [SerializeField] private float burstChance = 0.5f;
    [Tooltip("Total seconds of control movement needed to repair a burst element.")]
    [SerializeField] private float repairDuration = 2f;
    [Tooltip("Per-frame control change (0-1) above which the control counts as 'moving'. Raise it if pot/slider noise triggers false repairs.")]
    [SerializeField] private float movementThreshold = 0.03f;
    [Tooltip("Health drained per second for each currently-burst element.")]
    [SerializeField] private float drainPerSecond = 5f;
    [Tooltip("Seconds between ratchet noises while a control is being moved.")]
    [SerializeField] private float sfxInterval = 0.15f;

    // index -> control: 0 = playerPot_a (rotary), 1 = playerSlider_h (h-slider),
    //                   2 = playerPot_k (rotary), 3 = playerSlider_c (v-slider)
    // 0 = rotary, 1 = horizontal slider, 2 = vertical slider (matches ModulationMinigame layout)
    private readonly int[] channelModes = { 0, 1, 0, 2 };

    private readonly bool[] burst = new bool[4];
    private readonly float[] repairProgress = new float[4]; // accumulated movement time, never reset by pausing
    private readonly float[] lastValue = new float[4];      // previous frame's control value, for movement detection
    private float sfxTimer;
    private bool uiVisible;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this.gameObject); return; }
        Instance = this;
    }

    //called by Submarine on a damaging ram into terrain.
    public void OnSubmarineDamaged()
    {
        if (IsGated()) return;
        if (Random.value > burstChance) return; // failed the burst roll

        //collect the elements that are currently healthy
        int healthyCount = 0;
        for (int i = 0; i < 4; i++)
            if (!burst[i]) healthyCount++;

        if (healthyCount == 0) return; // everything already burst

        //pick a random healthy element
        int pick = Random.Range(0, healthyCount);
        for (int i = 0; i < 4; i++)
        {
            if (burst[i]) continue;
            if (pick == 0)
            {
                BurstElement(i);
                return;
            }
            pick--;
        }
    }

    private void BurstElement(int i)
    {
        burst[i] = true;
        repairProgress[i] = 0f;
        lastValue[i] = GetCurrentValues()[i]; // baseline so the next frame measures real movement

        SpawnFloatingText(burstMessage);
    }

    private void Update()
    {
        //freeze entirely while an artifact modulation minigame owns the screen
        if (IsGated())
        {
            SetUIVisible(false);
            return;
        }

        SetUIVisible(true);

        float[] current = GetCurrentValues();
        bool anyMoving = false;

        for (int i = 0; i < 4; i++)
        {
            if (!burst[i]) continue;

            //bleed health while this element is burst
            if (subHealth != null) subHealth.TakeDamage(drainPerSecond * Time.deltaTime);

            //movement-based repair: any direction counts; pausing holds progress, never resets it
            float delta = Mathf.Abs(current[i] - lastValue[i]);
            lastValue[i] = current[i];

            if (delta > movementThreshold)
            {
                anyMoving = true;
                repairProgress[i] += Time.deltaTime;
                if (repairProgress[i] >= repairDuration)
                    RepairElement(i);
            }
        }

        //ratchet noise on an interval while any burst control is being worked
        sfxTimer -= Time.deltaTime;
        if (anyMoving && sfxTimer <= 0f)
        {
            if (ui != null) ui.PlayMoveSFX();
            sfxTimer = sfxInterval;
        }

        if (ui != null) ui.UpdateVisuals(current, burst, channelModes);
    }

    private void RepairElement(int i)
    {
        burst[i] = false;
        repairProgress[i] = 0f;

        if (ui != null) ui.PlayRepairDoneSFX();
        SpawnFloatingText(repairMessage);
    }

    private void SetUIVisible(bool visible)
    {
        if (visible == uiVisible) return; // only toggle on transitions, mirrors ADM Activate/Deactivate
        uiVisible = visible;
        if (ui == null) return;
        if (visible) ui.ActivateUI();
        else ui.DeactivateUI();
    }

    //true while an artifact modulation minigame is happening; the repair mechanic stays frozen.
    private bool IsGated()
    {
        return GlobalModulationUI.Instance != null && GlobalModulationUI.Instance.IsActive;
    }

    //fetch the four live control values from the SerialHandler (same order ModulationMinigame uses)
    private float[] GetCurrentValues()
    {
        SerialHandler sh = SerialHandler.Instance;
        if (sh == null) return new float[4];
        return new float[] { sh.playerPot_a, sh.playerSlider_h, sh.playerPot_k, sh.playerSlider_c };
    }

    private void SpawnFloatingText(string message)
    {
        if (floatingTextPrefab == null) return;

        Vector3 spawnPos = transform.position + Vector3.up * 1.5f;
        GameObject ft = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);
        ft.GetComponent<FloatingText>().Init(message);
    }
}
