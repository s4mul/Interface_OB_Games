using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
//using Unity.Netcode.SceneManagement;

public class ServerBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject sceneRelayPrefab;
    [SerializeField] private GameObject playerPrefab;
    private SceneRelay _sceneRelay;

    private void Start()
    {
#if UNITY_SERVER || UNITY_DEDICATED_SERVER
        Debug.Log("[ServerBootstrap] Dedicated Server detected. Starting Netcode server...");
        StartServer();
#else
        Debug.Log("[ServerBootstrap] Client/Editor build detected. Server not started automatically.");
#endif
    }

    private void StartServer()
    {
        //init Netcode
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found in scene!");
            return;
        }

        NetworkManager.Singleton.NetworkConfig.EnableSceneManagement = false;

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData("0.0.0.0", 7777);

        if (NetworkManager.Singleton.StartServer())
        {
            Debug.Log("Server started successfully");
            // SceneRelay 인스턴스 생성
            var relayObj = Instantiate(sceneRelayPrefab);
            relayObj.GetComponent<NetworkObject>().Spawn(true);
            _sceneRelay = relayObj.GetComponent<SceneRelay>();

            //클라이언트 접속 이벤트 감지
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        else
        {
            Debug.LogError("Failed to start server");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");

        var playerObj = Instantiate(playerPrefab);
        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        Debug.Log($"Spawned player for client {clientId}");
    }
}