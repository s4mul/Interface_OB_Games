using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;


public class ClientBootstrap : MonoBehaviour
{
    [SerializeField] private string serverIp = "100.99.57.78"; // Tailscale IP
    [SerializeField] private ushort port = 7777;

    private void Start()
    {
#if UNITY_SERVER || UNITY_DEDICATED_SERVER
        Debug.Log("[ClientBootstrap] Dedicated Server detected. Client will not start.");
        return;
#else
        Debug.Log("[ClientBootstrap] Starting Netcode client...");
        StartClient();
#endif

    }

    private void StartClient()
    {
        // Netcode 초기화
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found in scene!");
            return;
        }

        NetworkManager.Singleton.NetworkConfig.EnableSceneManagement = false;

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(serverIp, port);
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Client started successfully");
        }
        else
        {
            Debug.LogError("Failed to start client");
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[ClientBootstrap] Connected to server (ClientId: {clientId})");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[ClientBootstrap] Disconnected from server (ClientId: {clientId})");
    }
}
