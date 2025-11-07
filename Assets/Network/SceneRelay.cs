using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SceneRelay : NetworkBehaviour
{

    private void Awake()
    {
        var existingRelays = FindObjectsByType<SceneRelay>(FindObjectsSortMode.None);

        if (existingRelays.Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }


    // ────────────────────────────────
    // 서버 → 클라이언트: 씬 교체 명령
    // ────────────────────────────────
    [ClientRpc]
    public void ChangeSceneClientRpc(string sceneName, ClientRpcParams rpcParams = default)
    {
        if (IsServer)
            return; // 서버는 무시

        Debug.Log($"[SceneRelay] Received scene change command: {sceneName}");
        SceneManager.LoadScene(sceneName); // 로컬에서 씬 전환
    }

    // ────────────────────────────────
    // 클라이언트 → 서버: 로드 완료 보고
    // ────────────────────────────────
    [ServerRpc(RequireOwnership = false)]
    public void NotifySceneLoadedServerRpc(ServerRpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"[SceneRelay] Client {clientId} finished loading scene.");
        // TODO: 여기에 서버가 로드 완료한 클라이언트 관리 로직 추가 가능
    }
}
