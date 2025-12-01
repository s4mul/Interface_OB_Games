using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class ServerSceneController : MonoBehaviour
{
    private string _serverSceneName;

    // ────────────────────────────────
    // 초기화
    // ────────────────────────────────
    public void Initialize()
    {
        DontDestroyOnLoad(gameObject);
        _serverSceneName = SceneManager.GetActiveScene().name;

        Debug.Log($"[ServerSceneController] Initialized. Persistent server scene: {_serverSceneName}");

        // 클라이언트 로드 완료 이벤트 구독
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnClientsLoadedScene;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnClientsLoadedScene;
    }

    // ────────────────────────────────
    //  클라이언트에게만 씬 전환 명령
    // ────────────────────────────────
    public void LoadSceneForClients(string sceneName)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("[ServerSceneController] Not running on server, ignoring scene load request.");
            return;
        }

        Debug.Log($"[ServerSceneController] Requesting clients to load '{sceneName}'");
        
        // 클라이언트에게만 씬 로드 지시
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

    }

    // ────────────────────────────────
    // 모든 클라이언트가 로드 완료했을 때
    // ────────────────────────────────
    private void OnClientsLoadedScene(string sceneName, LoadSceneMode mode,
                                      List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        Debug.Log($"[ServerSceneController] Scene '{sceneName}' loaded by {clientsCompleted.Count} clients (Timeout: {clientsTimedOut.Count})");

        // TODO: 모든 클라이언트가 로드 완료했을 때 처리할 로직 (예: 스폰, 게임 시작 등)
    }
}
