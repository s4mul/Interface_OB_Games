using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class ServerBootstrap : MonoBehaviour
{
    [Header("Network Settings")]
    [SerializeField] private string listenIp = "0.0.0.0";
    [SerializeField] private ushort port = 7777;

    [Header("Prefabs")]
    [SerializeField] private GameObject sceneRelayPrefab;   // NetworkObject 포함
    [SerializeField] private GameObject clientRelayPrefab;  // NetworkObject 포함
    [SerializeField] private GameObject gameManagerPrefab;


    [Header("Gameplay")]
    [SerializeField] private string initialSceneName = "lobbyScene";
    [SerializeField] private GameObject playerPrefab;       // NetworkObject 포함

    [Header("Gameplay Prefabs")]
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private GameObject personPrefab;


    private bool started = false;

    private void Start()
    {
#if UNITY_SERVER || UNITY_DEDICATED_SERVER
        StartServer();
#else
        // 클라이언트 빌드에서는 아무것도 안 함
#endif
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;

            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void StartServer()
    {
        if (started) return;
        started = true;

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[ServerBootstrap] No NetworkManager in scene!");
            return;
        }

        // Netcode 씬 매니지먼트 비활성화 (우리는 수동 씬 관리 사용)
        NetworkManager.Singleton.NetworkConfig.EnableSceneManagement = false;

        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.SetConnectionData(listenIp, port);

        if (!NetworkManager.Singleton.StartServer())
        {
            Debug.LogError("[ServerBootstrap] Failed to start server.");
            return;
        }

        Debug.Log("[ServerBootstrap] Netcode server started.");

        SpawnSceneRelayIfNeeded();
        SpawnGameManagerIfNeeded();

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }
    private void SpawnGameManagerIfNeeded()
    {
        if (GameManager.Instance != null)
            return; // 이미 존재함

        if (gameManagerPrefab == null)
        {
            Debug.LogError("[ServerBootstrap] GameManager prefab is not assigned!");
            return;
        }

        var go = Instantiate(gameManagerPrefab);
        DontDestroyOnLoad(go);

        Debug.Log("[ServerBootstrap] GameManager spawned on server.");
    }

    private void SpawnSceneRelayIfNeeded()
    {
        if (SceneRelay.Instance != null)
        {
            SceneRelay.Instance.SetPlayerPrefab(playerPrefab);
            SceneRelay.Instance.SetInitialSceneName(initialSceneName);
            return;
        }

        var relayObj = Instantiate(sceneRelayPrefab);
        var netObj = relayObj.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("[ServerBootstrap] sceneRelayPrefab에 NetworkObject가 없습니다.");
            Destroy(relayObj);
            return;
        }

        netObj.Spawn(true);

        if (SceneRelay.Instance != null)
        {
            SceneRelay.Instance.SetPlayerPrefab(playerPrefab);
            SceneRelay.Instance.SetInitialSceneName(initialSceneName);
        }
        else
        {
            Debug.LogError("[ServerBootstrap] SceneRelay Instance 할당 실패.");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[ServerBootstrap] Client connected: {clientId}");

        if (SceneRelay.Instance == null)
        {
            Debug.LogError("[ServerBootstrap] SceneRelay.Instance가 없습니다.");
            return;
        }

        var obj = Instantiate(clientRelayPrefab);
        var netObj = obj.GetComponent<NetworkObject>();
        var relay = obj.GetComponent<ClientRelay>();

        if (netObj == null || relay == null)
        {
            Debug.LogError("[ServerBootstrap] clientRelayPrefab 설정이 잘못되었습니다.");
            Destroy(obj);
            return;
        }

        Debug.Log($"[ServerBootstrap] Spawning ClientRelay for client {clientId}");
        netObj.SpawnWithOwnership(clientId, true);

        SceneRelay.Instance.RegisterClient(clientId, relay);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[ServerBootstrap] Client disconnected: {clientId}");

        if (SceneRelay.Instance != null)
        {
            SceneRelay.Instance.UnregisterClient(clientId);
        }
    }

    public void SpawnGamePlayerFor(ulong clientId)
    {
        // 로비에서 사용하던 PlayerObject 제거
        if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
        {
            var oldObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            oldObj.Despawn();
        }

        // 역할 결정
        GameManager.Instance.RegisterClient(clientId);
        GameManager.Instance.EnsureMonsterSelected();

        bool isMonster = GameManager.Instance.IsMonster(clientId);

        GameObject prefab = isMonster ? monsterPrefab : personPrefab;

        // 실제 게임용 플레이어 스폰
        var newObj = Instantiate(prefab);
        newObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        Debug.Log($"[ServerBootstrap] GameScene Spawn: client {clientId}, Monster={isMonster}");
    }

}
