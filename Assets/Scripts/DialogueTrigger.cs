// using UnityEngine;

// //attaches to a gameobject to trigger a dialogue system entry 

// [RequireComponent(typeof(Collider2D))]
// public class DialogueTrigger : MonoBehaviour
// {
//     [Header("Target")]
//     [SerializeField] private DialogueSystem dialogueSystem;

//     [Header("Trigger Settings")]
//     [SerializeField] private bool triggerOnStart = false;
//     [SerializeField] private bool triggerOnce = true;
//     [SerializeField] private string requiredTag = "";

//     private bool hasTriggered = false;

//     private void Start()
//     {
//         if (triggerOnStart)
//         {
//             TriggerDialogue();
//         }
//     }

//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         if (!other.CompareTag("Submarine") || !other.CompareTag("Player"))
//             return;

//         TriggerDialogue();
//     }

//     public void TriggerDialogue()
//     {
//         if (triggerOnce && hasTriggered) return;
        
//         hasTriggered = true;
//         dialogueSystem.StartDialogue();
//     }
// }