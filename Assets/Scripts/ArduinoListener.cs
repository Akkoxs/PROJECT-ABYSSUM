using UnityEngine;

public class ArduinoListener : MonoBehaviour
{
    public void OnMessageArrived(string msg)
    {
        Debug.Log("Arduino says: " + msg);
    }

    public void OnConnectionEvent(bool success)
    {
        Debug.Log(success ? "Arduino connected!" : "Arduino disconnected");
    }
}