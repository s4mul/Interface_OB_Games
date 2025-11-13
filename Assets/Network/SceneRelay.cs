using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class SceneRelay : NetworkBehaviour
{
    public static SceneRelay Instance { get; private set; }

    private readonly Dictionary<ulong, ClientRelay> clientRelays = new();
    private readonly HashSet<ulong> loadedClients = new();
    private readonly HashSet<ulong> spawnedPlayers = new();

    private GameObject playerPrefab;
    private string initialSceneName = "LobbyScene";

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
            // 클라에서는 필요 없는 객체
            Destroy(gameObject);
            return;
        }

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[SceneRelay] Duplicate instance detected. Destroying new one.");
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
        {
            Instance = null;
        }
    }

    /// <summary>
    /// 새로 접속한 클라이언트 등록 + 초기 씬으로 보내기
    /// </summary>
    public void RegisterClient(ulong clientId, ClientRelay relay)
    {
        if (!IsServer) return;
        if (relay == null)
        {
            Debug.LogError($"[SceneRelay] RegisterClient: relay is null for client {clientId}");
            return;
        }

        clientRelays[clientId] = relay;
        loadedClients.Remove(clientId);
        spawnedPlayers.Remove(clientId);

        Debug.Log($"[SceneRelay] Register client {clientId}, sending to initial scene '{initialSceneName}'.");

        // 해당 클라이언트에게만 씬 이동 명령
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
        loadedClients.Remove(clientId);
        spawnedPlayers.Remove(clientId);
    }

    /// <summary>
    /// 클라이언트가 특정 씬을 로드 완료했다고 보고했을 때 호출 (ClientRelay → ServerRpc → 여기)
    /// </summary>
    public void NotifyClientLoaded(ulong clientId, string sceneName)
    {
        if (!IsServer) return;

        Debug.Log($"[SceneRelay] Client {clientId} finished loading scene '{sceneName}'.");

        loadedClients.Add(clientId);

        // 이 타이밍에 플레이어 스폰 (클라별 개별 스폰)
        SpawnPlayerIfNeeded(clientId);
    }

    private void SpawnPlayerIfNeeded(ulong clientId)
    {
        if (!IsServer) return;
        if (spawnedPlayers.Contains(clientId)) return;

        if (playerPrefab == null)
        {
            Debug.LogError("[SceneRelay] playerPrefab이 설정되어 있지 않습니다.");
            return;
        }

        var playerObj = Instantiate(playerPrefab);
        var netObj = playerObj.GetComponent<NetworkObject>();

        if (netObj == null)
        {
            Debug.LogError("[SceneRelay] playerPrefab에 NetworkObject가 없습니다.");
            Destroy(playerObj);
            return;
        }

        netObj.SpawnAsPlayerObject(clientId);
        spawnedPlayers.Add(clientId);

        Debug.Log($"[SceneRelay] Spawned player for client {clientId}.");
    }

    /// <summary>
    /// 클라이언트가 버튼 등으로 "다른 씬으로 보내줘" 요청했을 때 사용할 수 있는 API
    /// (ClientRelay.RequestSceneChangeServerRpc → 여기)
    /// </summary>
    public void ChangeSceneForClient(ulong clientId, string sceneName)
    {
        if (!IsServer) return;

        if (!clientRelays.TryGetValue(clientId, out var relay) || relay == null)
        {
            Debug.LogError($"[SceneRelay] No ClientRelay found for client {clientId}.");
            return;
        }

        loadedClients.Remove(clientId);
        spawnedPlayers.Remove(clientId);

        Debug.Log($"[SceneRelay] Changing scene to '{sceneName}' for client {clientId}.");

        relay.SendSceneChangeClientRpc(
            sceneName,
            new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            });
    }
}
