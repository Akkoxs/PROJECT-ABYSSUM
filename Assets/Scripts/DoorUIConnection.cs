using UnityEngine;

public class DoorUIConnection : MonoBehaviour
{

[Header("Door Button GameObjects")]
[SerializeField] private GameObject openButton;
[SerializeField] private GameObject closeButton;

Submarine sub; 

private void Awake()
{
    sub = GetComponent<Submarine>();
}

private void Update()
{
    if (sub.doorOpen)
    {
        openButton.SetActive(false);
        closeButton.SetActive(true);
    }
    else
    {
        openButton.SetActive(true);
        closeButton.SetActive(false);
    }
}

}