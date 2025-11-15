using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;

public class ClientRelay : NetworkBehaviour
{
    public static ClientRelay LocalInstance { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (IsOwner && IsClient)
        {
            // 이 클라이언트가 소유한 Relay만 활성
            LocalInstance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[ClientRelay] Local owner relay spawned.");
        }
        else
        {
            // 소유하지 않은 클라에서는 로직 안 돌리도록 둬도 됨 (Destroy까지는 선택)
            Debug.Log("[ClientRelay] Non-owner relay instance on this client.");
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner && LocalInstance == this)
        {
            LocalInstance = null;
        }
    }

    // ─────────────────────────────────────
    // 서버 → 클라: 특정 씬으로 이동하라는 명령
    // ─────────────────────────────────────
    [ClientRpc]
    public void SendSceneChangeClientRpc(string sceneName, ClientRpcParams rpcParams = default)
    {
        if (!IsOwner || !IsClient)
            return;

        Debug.Log($"[ClientRelay] Received scene change to '{sceneName}'. Loading...");

        // 씬 로드 + 로딩 완료 후 서버에 보고
        StartCoroutine(LoadSceneAndNotify(sceneName));
    }

    private IEnumerator LoadSceneAndNotify(string sceneName)
    {
        var op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
        {
            yield return null;
        }

        Debug.Log($"[ClientRelay] Scene '{sceneName}' loaded. Notifying server...");

        NotifySceneLoadedServerRpc(sceneName);
    }

    // ─────────────────────────────────────
    // 클라 → 서버: 현재 씬 로드 완료 보고
    // ─────────────────────────────────────
    [ServerRpc(RequireOwnership = true)]
    private void NotifySceneLoadedServerRpc(string sceneName, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (SceneRelay.Instance != null)
        {
            SceneRelay.Instance.NotifyClientLoaded(clientId, sceneName);
        }
        else
        {
            Debug.LogError("[ClientRelay] SceneRelay.Instance is null on server.");
        }
    }

    // ─────────────────────────────────────
    // (옵션) 클라에서 버튼 눌러서 씬 변경 요청하는 용도
    // ─────────────────────────────────────
    public void RequestSceneChange(string sceneName)
    {
        if (!IsOwner || !IsClient) return;

        Debug.Log($"[ClientRelay] Requesting scene change to '{sceneName}' to server.");
        RequestSceneChangeServerRpc(sceneName);
    }

    [ServerRpc(RequireOwnership = true)]
    private void RequestSceneChangeServerRpc(string sceneName, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (SceneRelay.Instance != null)
        {
            SceneRelay.Instance.ChangeSceneForClient(clientId, sceneName);
        }
        else
        {
            Debug.LogError("[ClientRelay] SceneRelay.Instance is null on server.");
        }
    }
}
