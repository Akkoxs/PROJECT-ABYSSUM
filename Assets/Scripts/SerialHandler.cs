using System.Globalization;
using UnityEngine;

public class SerialHandler : MonoBehaviour
{
    //MAY HAVE TO ADD A MOVING AVERAGE FILTER FOR SUBMARINE CONTROLS CAUSE VALS ARE NOISY

    //SHOUld be usable from any other script, this class is a SINGLETON 

    //should be usable like this:
    //SerialHandler.Instance.SendSerialData("LED_ON!");
    //SerialHandler.Instance.SendSerialData("PUMP:75");

    //singleton 
    public static SerialHandler Instance { get; private set; }

    //Debu keys 

    private SerialController serialController;

    //serial catch
    [Header("Serial Catch")]
    public float playerPot_a; //modulation minigame
    public float playerSlider_h; //modulation minigame
    public float playerPot_k; //modulation minigame
    public float playerSlider_c; //modulation minigame
    public bool oxyL1; //oxy transfer +
    public bool oxyL2; //oxy transfer ++
    public bool oxyL3; //oxy transfer +++
    public bool ping; //scan lighting
    public bool radarOn;
    public bool radarOff;
    public bool door; //sub door
    public float coolantPot; //coolant pump rate 
    public float headSlider; //headlight brightness
    public float floodSlider; //taillight brightness
    public bool shoot; //submarine shoot 
    public float joy1X; //sub move X
    public float joy1Y; //sub move Y 
    public float joy2X; //sub look X 
    public float joy2Y; //sub look Y 

    [Header("Filtering")]
    [SerializeField] private int averageWindowSize = 8;
    [SerializeField] private float deadzone = 0.08f;

    //multipliers for analog to digi.
    //float joyStickMult = 1840f;
    //float joyMin = 800f;
    //float joyMax = 750f;
    float potentiometerMult = 1023f;

    private MovingAverage joy1XFilter;
    private MovingAverage joy1YFilter;
    private MovingAverage joy2XFilter;
    private MovingAverage joy2YFilter;

    public bool IsSerialReady => serialController != null && serialController.enabled;

    void Awake()
    {
        joy1XFilter = new MovingAverage(averageWindowSize);
        joy1YFilter = new MovingAverage(averageWindowSize);
        joy2XFilter = new MovingAverage(averageWindowSize);
        joy2YFilter = new MovingAverage(averageWindowSize);

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

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
    }

    //REMINDER: Might have to Mathf.clamp it between 0 and 180 
    //Send Temp by goin:
    // SerialHandler.SendSerialData("$TEMP:{TempAngle}");
    // SerialHandler.SendSerialData("$COOL:{CoolantAngle}");
    public void SendSerialData(string message)
    {
        serialController.SendSerialMessage(message);
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

            if (parts.Length < 19)
                return; // incomplete packet, skip

            int[] serial_catch = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out serial_catch[i]))
                {
                    Debug.LogError("Failed to parse integer from serial: " + parts[i]);
                }
            }

            playerPot_a = serial_catch[0] / potentiometerMult;
            playerSlider_h = serial_catch[1] / potentiometerMult;
            playerPot_k = serial_catch[2] / potentiometerMult;
            playerSlider_c = serial_catch[3] / potentiometerMult;
            oxyL1 = serial_catch[4] == 1;
            oxyL2 = serial_catch[5] == 1;
            oxyL3 = serial_catch[6] == 1;
            ping = serial_catch[7] == 1;
            radarOn = serial_catch[8] == 1;
            radarOff = serial_catch[9] == 1;
            door = serial_catch[10] == 1;
            coolantPot = serial_catch[11] / potentiometerMult;
            headSlider = serial_catch[12] / potentiometerMult;
            floodSlider = serial_catch[13] / potentiometerMult;
            shoot = serial_catch[14] == 1;
            joy1X = -1*ApplyDeadzone(joy1XFilter.Add(NormalizeJoystick(serial_catch[15], 270, 750)));
            joy1Y = ApplyDeadzone(joy1YFilter.Add(NormalizeJoystick(raw: serial_catch[16], 250, 770)));
            joy2X = ApplyDeadzone(joy2XFilter.Add(NormalizeJoystick(serial_catch[17], 250, 770)));
            joy2Y = ApplyDeadzone(joy2YFilter.Add(NormalizeJoystick(serial_catch[18], 235, 785)));
            // joy1X = (serial_catch[15]);
            // joy1Y = (serial_catch[16]);
            // joy2X = (serial_catch[17]);
            // joy2Y = (serial_catch[18]);

            // JoyX = NormalizeJoystick(serial_catch[0]);
            // JoyY = NormalizeJoystick(serial_catch[1]);
            // slide_pot = serial_catch[2] / potentiometerMult;
            // rot_pot = serial_catch[3] / potentiometerMult;
            // button1 = serial_catch[4] == 1;
            // button2 = serial_catch[5] == 1;

            //Debug.Log($"Received Serial Data - JoyX: {JoyX}, JoyY: {JoyY}, Slide Pot: {slide_pot}, Rot Pot: {rot_pot}, Button1: {button1}, Button2: {button2}");
        }
    }

    float NormalizeJoystick(float raw, float rawMin, float rawMax)
    {
        //if (raw > 1000f) return 0f;
        float clamped = Mathf.Clamp(raw, rawMin, rawMax);
        //inverse lerp returns a 0-1 value depending on where raw is between joyMin and joyMax
        return Mathf.InverseLerp(rawMin, rawMax, clamped) * 2f - 1f; //the *2f -1f is to remap the 0-1 range to -1 to 1 so get directions
    }

    float ApplyDeadzone(float value)
    {
        if (Mathf.Abs(value) < deadzone) return 0f;
        // Rescale so output still reaches -1/+1 at the edges
        return Mathf.Sign(value) * (Mathf.Abs(value) - deadzone) / (1f - deadzone);
    }
}

//helper class for filtering 
public class MovingAverage
{
    private float[] buffer;
    private int index = 0;
    private float sum = 0f;

    public MovingAverage(int windowSize)
    {
        buffer = new float[windowSize];
    }

    public float Add(float value)
    {
        sum -= buffer[index];
        buffer[index] = value;
        sum += value;
        index = (index + 1) % buffer.Length;
        return sum / buffer.Length;
    }
}
