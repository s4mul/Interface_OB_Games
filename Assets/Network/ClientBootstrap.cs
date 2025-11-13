using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class ClientBootstrap : MonoBehaviour
{
    [SerializeField] private string serverIp = "100.99.57.78";
    [SerializeField] private ushort port = 7777;

    private bool clientStarted = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
#if UNITY_SERVER || UNITY_DEDICATED_SERVER
        // 서버 빌드에서는 클라를 시작하지 않음
        return;
#else
        StartClient();
#endif
    }

    private void StartClient()
    {
        if (clientStarted) return;

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[ClientBootstrap] NetworkManager not found!");
            return;
        }

        clientStarted = true;

        NetworkManager.Singleton.NetworkConfig.EnableSceneManagement = false;

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(serverIp, port);

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        if (!NetworkManager.Singleton.StartClient())
        {
            Debug.LogError("[ClientBootstrap] Failed to start client");
        }
        else
        {
            Debug.Log("[ClientBootstrap] Client started, connecting to server...");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
            return;

        Debug.Log("[ClientBootstrap] Connected to server.");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
            return;

        Debug.Log("[ClientBootstrap] Disconnected from server.");
    }
}
