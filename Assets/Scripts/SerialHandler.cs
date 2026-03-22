using System.Globalization;
using UnityEngine;

public class SerialHandler : MonoBehaviour
{
    //March 22 2026
    //digital outputs (LEDs) YES
    //digital outputs (oled) NOT YET
    //digital/analog reads YES

    //MAY HAVE TO ADD A MOVING AVERAGE FILTER FOR SUBMARINE CONTROLS CAUSE VALS ARE NOISY

    [SerializeField] KeyCode blinkStartKey = KeyCode.K;
    [SerializeField] KeyCode blinkStopKey = KeyCode.L;

    private SerialController serialController;

    //serial catch
    float JoyX; 
    float JoyY;
    float slide_pot; //slide potientiometer
    float rot_pot; //rotary potientiometer
    bool button1; 
    bool button2;

    //multipliers for analog to digi.
    float joyStickMult = 1840f;
    float joyMin = 800f;
    float joyMax = 3200f;
    float potentiometerMult = 4095f;

    void Awake()
    {
        if (serialController == null)
        {
            serialController = GetComponentInParent<SerialController>();
        }
    }

    void OnApplicationQuit()
    {
        if (serialController != null)
            serialController.enabled = false;
    }

    #if UNITY_EDITOR
    void OnDestroy()
    {
        // forcing Ardity to shut down before Unity tears down
        if (serialController != null)
        {
            serialController.enabled = false;
            
            //give the thread time to die
            System.Threading.Thread.Sleep(200);
        }
    }
    #endif

    void Update()
    {
        ReceiveSerialData();
        SendSerialData();
    }

    void ReceiveSerialData()
    {
        string raw_message = serialController.ReadSerialMessage();

        if (raw_message == null)
            return;

        //check if the message is plain data or a connect/disconnect event.
        if (ReferenceEquals(raw_message, SerialController.SERIAL_DEVICE_CONNECTED))
            Debug.Log(message: "SERIAL CONNECTED");
        else if (ReferenceEquals(raw_message, SerialController.SERIAL_DEVICE_DISCONNECTED))
            Debug.Log(message: "SERIAL DISCONNECTED");
        else
        {
            string trimmed_msg = raw_message.Trim();
            string[] parts = trimmed_msg.Split(',');

            int[] serial_catch = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out serial_catch[i]))
                {
                    Debug.LogError("Failed to parse integer from serial: " + parts[i]);
                }
            }

            JoyX = NormalizeJoystick(serial_catch[0]);
            JoyY = NormalizeJoystick(serial_catch[1]);
            slide_pot = serial_catch[2] / potentiometerMult;
            rot_pot = serial_catch[3] / potentiometerMult;
            button1 = serial_catch[4] == 1;
            button2 = serial_catch[5] == 1;

            Debug.Log($"Received Serial Data - JoyX: {JoyX}, JoyY: {JoyY}, Slide Pot: {slide_pot}, Rot Pot: {rot_pot}, Button1: {button1}, Button2: {button2}");
        }
    }

    void SendSerialData()
    {
        if (Input.GetKeyDown(blinkStartKey))
        {
            Debug.Log(message: "Sending LED_TEST!");
            serialController.SendSerialMessage("LED_TEST!");
        }

        else if(Input.GetKeyDown(blinkStopKey))
        {
            Debug.Log(message: "Sending LED_STOP!");
            serialController.SendSerialMessage("LED_STOP!");
        }
    }

    //the Joystick is a Grove joystick v1.1, which has a raw val scale of 800-3200 for its x and y axes with the center ~2048, this is with the 12-bit ADC we have on the ESP32
    float NormalizeJoystick(float raw)
    {
        if (raw > 3500f) return 0f; //this is for when joystick is pressed down (like a button), it gives a high value which we interpret as 0, which is same as center rn. MAY CHANGE THIS LATER

        //inverse lerp returns a 0-1 value depending on where raw is between joyMin and joyMax
        return Mathf.InverseLerp(joyMin, joyMax, raw)* 2f - 1f; //the *2f -1f is to remap the 0-1 range to -1 to 1 so get directions
    }
}
