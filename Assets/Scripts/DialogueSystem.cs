using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Lightweight worldspace dialogue system for tutorial text boxes.
/// 
/// SETUP:
/// 1. Create a Canvas set to World Space and size it to match your RenderTexture dimensions.
/// 2. Add a TextMeshProUGUI child to the Canvas — assign it to `dialogueText`.
/// 3. Optionally add a "press any key" prompt TMP object → assign to `continuePrompt`.
/// 4. Point a Camera at the Canvas; set that Camera's Target Texture to your RenderTexture.
/// 5. Sample the RenderTexture on a RawImage or material on all 4 screens.
/// 6. Add DialogueEntries in the Inspector and call StartDialogue() from a DialogueTrigger.
/// </summary>
public class DialogueSystem : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────────

    [Header("References")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject continuePrompt;
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private GameObject dialogueUIBoxes; 

    [Header("Typewriter Defaults")]
    [SerializeField] private float defaultTypewriterSpeed = 40f;
    [SerializeField] private string advanceInputButton = "";

    [Header("Dialogue Sequence")]
    [SerializeField] private DialogueEntry[] entries;

    [Header("Events")]
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;
    public UnityEvent<int> onEntryStart; //passes entry index
    public UnityEvent<int> onEntryEnd; //passes entry index

    private int currentIndex = -1;
    private bool isTyping = false; //state flag for when the typewriter effect is going
    private bool skipRequested = false; //for when player presses advance key during the typewriting
    private bool advanceReady = false; //tells us when typewriter effect is done and we can advance 
    private bool externalSignal = false; //set by AdvanceDialogue() for external event triggers
    private Coroutine typewriterCoroutine;
    private Coroutine autoAdvanceCoroutine;

    //initialize teh dialogue system and pass in the entries
    public void StartDialogue() => StartDialogue(entries);

    //takes in an array of DialogueEntry objects
    
    private void Start()
    {
        StartDialogue(entries);
    }
    
    
    public void StartDialogue(DialogueEntry[] sequence)
    {
        StopAllRunningCoroutines();
        entries = sequence;
        currentIndex = -1;
        externalSignal = false;

        if (dialogueRoot != null) 
        {
            dialogueRoot.SetActive(true);
            dialogueUIBoxes.SetActive(true);
        }

        SetContinuePrompt(false);

        onDialogueStart.Invoke();
        ShowNextEntry();
    }

    public void AdvanceDialogue()
    {
        if (!gameObject.activeInHierarchy) return;

        if (isTyping)
        {
            skipRequested = true;   // finish typing instantly first
        }
        else if (advanceReady)
        {
            externalSignal = true;
        }
    }

    private void ShowNextEntry()
    {
        currentIndex++;

        if (currentIndex >= entries.Length)
        {
            FinishDialogue();
            return;
        }

        DialogueEntry entry = entries[currentIndex];
        advanceReady  = false;
        skipRequested  = false;
        externalSignal = false;

        SetContinuePrompt(false);
        onEntryStart.Invoke(currentIndex);

        typewriterCoroutine = StartCoroutine(TypewriterRoutine(entry, defaultTypewriterSpeed));
    }

    private IEnumerator TypewriterRoutine(DialogueEntry entry, float charsPerSecond)
    {
        isTyping = true;
        dialogueText.text = "";

        float interval = 1f / Mathf.Max(charsPerSecond, 1f);
        int charIndex = 0;
        string fullText = entry.text;

        while (charIndex < fullText.Length)
        {
            if (skipRequested) //skip typewriter effect
            {
                dialogueText.text = fullText;
                break;
            }

            charIndex++;
            dialogueText.text = fullText.Substring(0, charIndex);
            yield return new WaitForSeconds(interval);
        }

        isTyping = false;
        skipRequested = false;

        onEntryEnd.Invoke(currentIndex);
        yield return StartCoroutine(WaitForAdvance(entry));
    }

    private IEnumerator WaitForAdvance(DialogueEntry entry)
    {
        advanceReady = true;

        switch (entry.trigger)
        {
            case AdvanceTrigger.AnyKey:
                SetContinuePrompt(true);
                yield return new WaitUntil(IsAdvancePressed);
                SetContinuePrompt(false);
                break;

            case AdvanceTrigger.TimedAuto:
                yield return new WaitForSeconds(Mathf.Max(entry.autoAdvanceDelay, 0f));
                break;

            case AdvanceTrigger.ExternalEvent:
                SetContinuePrompt(true);
                yield return new WaitUntil(() => externalSignal);
                SetContinuePrompt(false);
                break;
        }

        advanceReady = false;
        ShowNextEntry();
    }

    private bool IsAdvancePressed() //predicate for AnyKey advance trigger 
    {
        if (!string.IsNullOrEmpty(advanceInputButton))
            return Input.GetButtonDown(advanceInputButton);

        return Input.anyKeyDown;
    }

    private void FinishDialogue()
    {
        if (dialogueRoot != null)
        {
            dialogueRoot.SetActive(false);
            dialogueUIBoxes.SetActive(false);
        } 

        SetContinuePrompt(false);
        if (dialogueText != null) dialogueText.text = "";
        currentIndex = -1;
        onDialogueEnd.Invoke();
    }

    public void EndDialogue()
    {
        StopAllRunningCoroutines();
        FinishDialogue();
    }

    private void SetContinuePrompt(bool visible)
    {
        if (continuePrompt != null)
            continuePrompt.SetActive(visible);
    }

    private void StopAllRunningCoroutines()
    {
        if (typewriterCoroutine  != null) StopCoroutine(typewriterCoroutine);
        if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);
    }

    // ── Editor Helpers ───────────────────────────────────────────────────────

// #if UNITY_EDITOR
//     private void OnValidate()
//     {
//         if (defaultTypewriterSpeed <= 0f)
//             defaultTypewriterSpeed = 40f;
//     }
// #endif
}