using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;

public class SceneRelay : NetworkBehaviour
{
    public static SceneRelay Instance { get; private set; }

    // 각 클라이언트 ↔ 해당 ClientRelay 매핑
    private readonly Dictionary<ulong, ClientRelay> clientRelays = new();

    // 씬 로드 상태
    private readonly HashSet<ulong> lobbyLoadedClients = new();
    private readonly HashSet<ulong> gameLoadedClients = new();

    // 플레이어 스폰 여부
    private readonly HashSet<ulong> spawnedPlayers = new();

    private GameObject playerPrefab;

    // ServerBootstrap에서 SetInitialSceneName으로 세팅됨
    private string initialSceneName = "LobbyScene";

    // 게임 맵 씬 이름 (인스펙터에서 바꾸거나, 그냥 "GameMap" 쓰면 됨)
    [SerializeField] private string gameSceneName = "GameMap";

    // ───────────────────────────────────
    // 설정
    // ───────────────────────────────────
    public void SetPlayerPrefab(GameObject prefab)
    {
        playerPrefab = prefab;
    }

    public void SetInitialSceneName(string sceneName)
    {
        initialSceneName = sceneName;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            Destroy(gameObject);
            return;
        }

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[SceneRelay] Server-side SceneRelay spawned.");
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this)
            Instance = null;
    }

    // ───────────────────────────────────
    // 클라이언트 등록 + 로비로 이동
    // ───────────────────────────────────
    public void RegisterClient(ulong clientId, ClientRelay relay)
    {
        if (!IsServer) return;

        clientRelays[clientId] = relay;
        lobbyLoadedClients.Remove(clientId);
        gameLoadedClients.Remove(clientId);
        spawnedPlayers.Remove(clientId);

        Debug.Log($"[SceneRelay] Register client {clientId}, move to lobby.");

        // 무조건 로비 씬으로 보내기
        relay.SendSceneChangeClientRpc(
            initialSceneName,
            new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            });
    }

    public void UnregisterClient(ulong clientId)
    {
        if (!IsServer) return;

        clientRelays.Remove(clientId);
        lobbyLoadedClients.Remove(clientId);
        gameLoadedClients.Remove(clientId);
        spawnedPlayers.Remove(clientId);

        Debug.Log($"[SceneRelay] Unregister client {clientId}");
    }

    // ───────────────────────────────────
    // 클라이언트 씬 로드 완료 보고
    // ───────────────────────────────────
    public void NotifyClientLoaded(ulong clientId, string sceneName)
    {
        if (!IsServer) return;

        Debug.Log($"[SceneRelay] Client {clientId} loaded '{sceneName}'");
        Debug.Log($"[DEBUG] sceneName='{sceneName}', gameSceneName='{gameSceneName}'");

        // 로비 씬
        if (sceneName == initialSceneName)
        {
            lobbyLoadedClients.Add(clientId);
            SpawnPlayerIfNeeded(clientId);
        }
        // 게임 맵 씬
        else if (sceneName == gameSceneName)
        {
            gameLoadedClients.Add(clientId);
            TrySpawnGamePlayer(clientId);
            /*
            var bootstrap = FindObjectOfType<ServerBootstrap>();
            bootstrap.SpawnGamePlayerFor(clientId);

            TryPlaceAllPlayers();
            */
        }
        else
        {
            // 기타 씬이 있다면 여기서 필요시 처리
        }
    }

    // ───────────────────────────────────
    // 로비 진입 시 플레이어 스폰 (최초 1회)
    // ───────────────────────────────────
    private void SpawnPlayerIfNeeded(ulong clientId)
    {
        if (!IsServer) return;
        if (spawnedPlayers.Contains(clientId)) return;

        if (playerPrefab == null)
        {
            Debug.LogError("[SceneRelay] playerPrefab not set.");
            return;
        }

        var playerObj = Instantiate(playerPrefab);
        var netObj = playerObj.GetComponent<NetworkObject>();

        if (netObj == null)
        {
            Debug.LogError("[SceneRelay] Player prefab has no NetworkObject.");
            Destroy(playerObj);
            return;
        }

        netObj.SpawnAsPlayerObject(clientId);
        spawnedPlayers.Add(clientId);

        Debug.Log($"[SceneRelay] Spawned player for client {clientId}");
    }

    private void TrySpawnGamePlayer(ulong clientId)
    {
        if (!IsServer) return;
        if (spawnedPlayers.Contains(clientId)) return;

        var bootstrap = FindObjectOfType<ServerBootstrap>();
        if (bootstrap == null)
        {
            Debug.LogWarning("[SceneRelay] ServerBootstrap not found yet. Delaying spawn...");
            return;   // GameManager / Prefab 아직 초기화 전이면 빠져나오고 다음 NotifyClientLoaded 때 다시 시도됨
        }
        Debug.LogWarning("[SceneRelay] Try Spawn...\n");
        bootstrap.SpawnGamePlayerFor(clientId);
        spawnedPlayers.Add(clientId);
    }


    // ───────────────────────────────────
    // 게임씬에서 모든 플레이어를 SpawnPoint 위치로 배치
    // ───────────────────────────────────
    private void TryPlaceAllPlayers()
    {
        Debug.Log($"[DEBUG] loadedInGame={gameLoadedClients.Count}, totalPlayers={clientRelays.Count}");

        // 현재 접속 중인 "실제 플레이어" 목록
        // (clientRelays에 등록된 클라이언트만 대상으로 함)
        int totalPlayers = clientRelays.Count;
        int loadedInGame = gameLoadedClients.Count;

        Debug.Log($"[SceneRelay] TryPlaceAllPlayers: loadedInGame={loadedInGame}, totalPlayers={totalPlayers}");

        // 아직 전원이 GameMap을 로드하지 않음
        if (loadedInGame != totalPlayers)
        {
            Debug.Log("[SceneRelay] Not all players loaded GameMap yet. Waiting...");
            return;
        }

        // 씬 내 SpawnPoint 수집
        var spawnPoints = GameObject.FindObjectsOfType<SpawnPoint>();
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[SceneRelay] No SpawnPoints found in GameMap!");
            return;
        }

        // index 기준 정렬
        Array.Sort(spawnPoints, (a, b) => a.index.CompareTo(b.index));

        Debug.Log($"[SceneRelay] Found {spawnPoints.Length} spawn points. Placing players...");

        int i = 0;
        foreach (var pair in clientRelays)
        {
            ulong clientId = pair.Key;
            var spawnPoint = spawnPoints[i % spawnPoints.Length];
            PlacePlayerAtPoint(clientId, spawnPoint.transform.position);
            i++;
        }

        Debug.Log("[SceneRelay] All players placed on GameMap spawn points.");
    }

    private void PlacePlayerAtPoint(ulong clientId, Vector3 pos)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            Debug.LogWarning($"[SceneRelay] No ConnectedClient for clientId {clientId}");
            return;
        }

        var playerObj = client.PlayerObject;
        if (playerObj == null)
        {
            Debug.LogWarning($"[SceneRelay] PlayerObject is null for clientId {clientId}");
            return;
        }

        playerObj.transform.position = pos;
        Debug.Log($"[SceneRelay] Player {clientId} moved to {pos}");
    }

    // ───────────────────────────────────
    // 하나의 클라이언트 요청 → 전체 클라 씬 이동
    // ───────────────────────────────────
    public void ChangeSceneFromClientRequest(ulong requesterId, string sceneName)
    {
        if (!IsServer) return;

        Debug.Log($"[SceneRelay] Client {requesterId} requested scene '{sceneName}'. Broadcasting to ALL.");

        // 씬 전환 시 로드 상태 초기화
        lobbyLoadedClients.Clear();
        gameLoadedClients.Clear();

        foreach (var pair in clientRelays)
        {
            ulong targetId = pair.Key;
            var relay = pair.Value;

            relay.SendSceneChangeClientRpc(
                sceneName,
                new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] { targetId }
                    }
                });
        }
    }
}
