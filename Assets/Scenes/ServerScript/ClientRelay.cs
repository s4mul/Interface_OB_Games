using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;

public class ClientRelay : NetworkBehaviour
{
    public static ClientRelay LocalInstance { get; private set; }

    public override void OnNetworkSpawn()
    {
        // 네트워크 오브젝트는 씬 전환 시 파괴되면 안 되므로, 모두 DDOL로 이동
        DontDestroyOnLoad(gameObject);

        if (IsOwner && IsClient)
        {
            LocalInstance = this;
            Debug.Log("[ClientRelay] Local owner relay spawned.");
        }
        else
        {
            Debug.Log("[ClientRelay] Non-owner relay instance.");
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner && LocalInstance == this)
        {
            LocalInstance = null;
        }
    }

    // ───────────────────────────────────
    // 서버 → 클라: 특정 씬 로드
    // ───────────────────────────────────
    [ClientRpc]
    public void SendSceneChangeClientRpc(string sceneName, ClientRpcParams rpcParams = default)
    {
        // 이 ClientRelay는 특정 클라이언트의 소유이므로, Owner인 쪽에서만 처리
        if (!IsOwner || !IsClient)
            return;

        Debug.Log($"[ClientRelay] Received scene change to '{sceneName}'. Loading...");
        StartCoroutine(LoadSceneAndNotify(sceneName));
    }

    private IEnumerator LoadSceneAndNotify(string sceneName)
    {
        var op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
            yield return null;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

        Debug.Log($"[ClientRelay] Scene '{sceneName}' loaded. Notifying server...");
        NotifySceneLoadedServerRpc(sceneName);
    }

    // ───────────────────────────────────
    // 클라 → 서버: 씬 로드 완료 알림
    // ───────────────────────────────────
    [ServerRpc(RequireOwnership = false)]
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

    // ───────────────────────────────────
    // 버튼 등에서 호출하는 씬 변경 요청
    // ───────────────────────────────────
    public void RequestSceneChange(string sceneName)
    {
        if (!IsOwner || !IsClient) return;

        Debug.Log($"[ClientRelay] Requesting scene '{sceneName}' from server...");
        RequestSceneChangeServerRpc(sceneName);
    }

    // ───────────────────────────────────
    // 클라 → 서버: 씬 변경 요청
    // ───────────────────────────────────
    [ServerRpc(RequireOwnership = false)]
    private void RequestSceneChangeServerRpc(string sceneName, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (SceneRelay.Instance != null)
        {
            SceneRelay.Instance.ChangeSceneFromClientRequest(clientId, sceneName);
        }
        else
        {
            Debug.LogError("[ClientRelay] SceneRelay.Instance is null on server.");
        }
    }
}
