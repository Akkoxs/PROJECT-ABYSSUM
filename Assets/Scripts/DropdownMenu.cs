using UnityEngine;
using UnityEngine.InputSystem;

public class DropdownMenu : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private RectTransform panelInPos;
    [SerializeField] private RectTransform panelOutPos;
    [SerializeField] private float deploySpeed;
    [SerializeField] private UIHelper uiHelper;
    private RectTransform target;
    private Coroutine translateCoroutine;
    private bool panelDeployed = false;

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
            ToggleMenu();
    }

    private void ToggleMenu()
    {
        if (translateCoroutine != null)
        {
            StopCoroutine(translateCoroutine);
            translateCoroutine = null;
        }

        if (panelDeployed)
        {
            target = panelInPos;
            panelDeployed = false;
        }
        else
        {
            target = panelOutPos;
            panelDeployed = true;  
        }
            

        translateCoroutine = StartCoroutine(uiHelper.Translate(panel, target, deploySpeed));
    }
   
}
