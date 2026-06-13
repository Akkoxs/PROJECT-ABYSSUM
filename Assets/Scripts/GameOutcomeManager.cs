using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Put this on a manager object in your gameplay scene (e.g. GameManager).
// It loads the Death or Win scene when those outcomes happen.

public class GameOutcomeManager : MonoBehaviour
{
    [Header("Scene Names (must match Build Settings exactly)")]
    [SerializeField] private string deathSceneName = "DeathScene";
    [SerializeField] private string winSceneName = "WinScene";

    [Header("Player")]
    [SerializeField] private Health playerHealth;   // drag the player's Health component here

    [Header("Win Condition")]
    [SerializeField] private GameManager gameManager;   // drag your GameManager here
    [SerializeField] private bool winWhenDebtCleared = true;

    [Header("Optional delay before loading (lets a sound/anim play)")]
    [SerializeField] private float delayBeforeLoad = 1.0f;

    private bool outcomeTriggered = false;

    void Start()
    {
        if (playerHealth != null)
            playerHealth.died.AddListener(OnPlayerDied);

        if (gameManager != null && winWhenDebtCleared)
            gameManager.onMoneyChanged.AddListener(OnMoneyChanged);
    }

    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.died.RemoveListener(OnPlayerDied);

        if (gameManager != null)
            gameManager.onMoneyChanged.RemoveListener(OnMoneyChanged);
    }

    private void OnMoneyChanged(int money, int debt)
    {
        if (winWhenDebtCleared && debt <= 0)
            TriggerWin();
    }

    private void OnPlayerDied()
    {
        if (outcomeTriggered) return;
        outcomeTriggered = true;
        StartCoroutine(LoadAfterDelay(deathSceneName));
    }

    // Call this from your win condition (whatever it ends up being).
    public void TriggerWin()
    {
        if (outcomeTriggered) return;
        outcomeTriggered = true;
        StartCoroutine(LoadAfterDelay(winSceneName));
    }

    private IEnumerator LoadAfterDelay(string sceneName)
    {
        // Use unscaled time in case you pause the game (Time.timeScale = 0) on outcome
        float t = 0f;
        while (t < delayBeforeLoad)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1f; // ensure normal time in the next scene
        SceneManager.LoadScene(sceneName);
    }
}