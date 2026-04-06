using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class MultiplayerManager : MonoBehaviour
{
    [Header("Player Prefabs")]
    [SerializeField] private GameObject playerPrefab; // Diver
    [SerializeField] private GameObject submarinePrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

    //[Header("References")]
    //[SerializeField] private SplitScreenManager splitScreenManager;

    private GameObject player1Instance;
    private GameObject player2Instance;
    private PlayerInputManager inputManager;

    void Start()
    {
        inputManager = GetComponent<PlayerInputManager>();
        // Spawn both players immediately
        SpawnPlayers();
    }

    void SpawnPlayers()
    {
        // Spawn Player 1 (Diver)
        if (playerPrefab != null && player1SpawnPoint != null)
        {
            player1Instance = Instantiate(playerPrefab, player1SpawnPoint.position, Quaternion.identity);
            player1Instance.name = "Player1_Diver";

            // Get PlayerInput and assign device
            PlayerInput p1Input = player1Instance.GetComponent<PlayerInput>();
            if (p1Input != null)
            {
                p1Input.defaultControlScheme = "Gamepad";

                // Try to assign first gamepad
                if (Gamepad.all.Count > 0)
                {
                    InputUser.PerformPairingWithDevice(Gamepad.all[0], p1Input.user);
                }
            }
        }

        // Spawn Player 2 (Submarine)
        if (submarinePrefab != null && player2SpawnPoint != null)
        {
            player2Instance = Instantiate(submarinePrefab, player2SpawnPoint.position, Quaternion.identity);
            player2Instance.name = "Player2_Submarine";

            // Get PlayerInput and assign device
            PlayerInput p2Input = player2Instance.GetComponent<PlayerInput>();
            if (p2Input != null)
            {
                p2Input.defaultControlScheme = "Gamepad";

                // Try to assign second gamepad
                if (Gamepad.all.Count > 1)
                {
                    InputUser.PerformPairingWithDevice(Gamepad.all[1], p2Input.user);
                }
            }

            // Assign to camera
        }
    }

    // Optional: Manual respawn methods
    public void RespawnPlayer1()
    {
        if (player1Instance != null)
        {
            player1Instance.transform.position = player1SpawnPoint.position;
        }
    }

    public void RespawnPlayer2()
    {
        if (player2Instance != null)
        {
            player2Instance.transform.position = player2SpawnPoint.position;
        }
    }
}