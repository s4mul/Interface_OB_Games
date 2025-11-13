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

    [Header("Gameplay")]
    [SerializeField] private string initialSceneName = "LobbyScene";
    [SerializeField] private GameObject playerPrefab;       // NetworkObject 포함

    private bool started = false;

    private void Start()
    {
#if UNITY_SERVER || UNITY_DEDICATED_SERVER
        StartServer();
#else
        // 클라이언트 빌드에서는 아무것도 안 함
#endif
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

        // Netcode 씬 매니지먼트는 사용하지 않음 (커스텀 방식)
        NetworkManager.Singleton.NetworkConfig.EnableSceneManagement = false;

        // Transport 설정
        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.SetConnectionData(listenIp, port);

        // 서버 시작
        if (!NetworkManager.Singleton.StartServer())
        {
            Debug.LogError("[ServerBootstrap] Failed to start server.");
            return;
        }

        Debug.Log("[ServerBootstrap] Netcode server started.");

        // SceneRelay 스폰 (서버 전용 싱글톤)
        SpawnSceneRelayIfNeeded();

        // 클라이언트 접속 이벤트 구독
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void SpawnSceneRelayIfNeeded()
    {
        if (SceneRelay.Instance != null)
        {
            // 이미 존재
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
        // 서버 자신(Host 용)은 없다 가정 (Dedicated 서버)
        Debug.Log($"[ServerBootstrap] Client connected: {clientId}");

        if (SceneRelay.Instance == null)
        {
            Debug.LogError("[ServerBootstrap] SceneRelay.Instance가 없습니다.");
            return;
        }

        // 클라마다 ClientRelay 네트워크 오브젝트 생성 + 해당 클라에게 오너십 부여
        var obj = Instantiate(clientRelayPrefab);
        var netObj = obj.GetComponent<NetworkObject>();
        var relay = obj.GetComponent<ClientRelay>();

        if (netObj == null || relay == null)
        {
            Debug.LogError("[ServerBootstrap] clientRelayPrefab 설정이 잘못되었습니다.");
            Destroy(obj);
            return;
        }

        // 이 클라이언트에게 소유권 부여해서 스폰
        Debug.Log($"Spawning ClientRelay for client {clientId}");
        netObj.SpawnWithOwnership(clientId, true);
        

        // SceneRelay에게 이 클라이언트 등록 + 초기 씬으로 이동 요청
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
}
