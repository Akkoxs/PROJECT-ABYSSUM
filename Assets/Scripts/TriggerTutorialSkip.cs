using UnityEngine;

public class TriggerTutorialSkip : MonoBehaviour
{

    [SerializeField] DialogueSystem dialogueSys; 

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            dialogueSys.EndDialogue();
        }
    }
}
