using UnityEngine;
using System.IO;

[RequireComponent(typeof(SerialController))]
public class PortConfigLoader : MonoBehaviour
{
    private SerialController serialController;

    void Start()
    {
        serialController = GetComponent<SerialController>();
        LoadPortFromFile();
    }

    void LoadPortFromFile()
    {
        // This path works in both the Editor and the final Build
        string filePath = Path.Combine(Application.streamingAssetsPath, "port_config.txt");

        if (File.Exists(filePath))
        {
            // Read the text, trim any accidental spaces or hidden newline characters
            string targetPort = File.ReadAllText(filePath).Trim();
            
            Debug.Log($"[PortConfigLoader] Read port {targetPort} from config file.");
            
            // Call the custom method we added to Ardity
            serialController.ConnectToPort(targetPort);
        }
        else
        {
            Debug.LogError("[PortConfigLoader] port_config.txt not found in StreamingAssets!");
        }
    }
}