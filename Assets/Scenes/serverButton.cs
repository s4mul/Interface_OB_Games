using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button serverButton;

    private void Awake()
    {
        hostButton.onClick.AddListener(StartAsHost);
        clientButton.onClick.AddListener(StartAsClient);
        serverButton.onClick.AddListener(StartAsServer);
    }

    private void StartAsHost()
    {
        ResetNetworkIfRunning();

        if (NetworkManager.Singleton.StartHost())
            Debug.Log("✅ Host 시작");
        else
            Debug.LogWarning("❌ Host 시작 실패 — 이미 실행 중이거나 초기화 문제");
    }

    private void StartAsClient()
    {
        ResetNetworkIfRunning();

        if (NetworkManager.Singleton.StartClient())
            Debug.Log("✅ Client 연결 시도");
        else
            Debug.LogWarning("❌ Client 시작 실패 — 이미 실행 중이거나 초기화 문제");
    }

    private void StartAsServer()
    {
        ResetNetworkIfRunning();

        if (NetworkManager.Singleton.StartServer())
            Debug.Log("✅ Server 시작");
        else
            Debug.LogWarning("❌ Server 시작 실패 — 이미 실행 중이거나 초기화 문제");
    }

    /// <summary>
    /// 🔥 이미 실행 중인 NetworkManager가 있다면 정리 후 새로 시작
    /// </summary>
    private void ResetNetworkIfRunning()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("❌ NetworkManager.Singleton is null!");
            return;
        }

        // 이미 서버/클라이언트/호스트 중 하나라도 동작 중이면 Shutdown
        if (NetworkManager.Singleton.IsListening ||
            NetworkManager.Singleton.IsServer ||
            NetworkManager.Singleton.IsClient)
        {
            Debug.LogWarning("⚠️ 기존 세션이 실행 중입니다. 종료 후 재시작...");
            NetworkManager.Singleton.Shutdown();

            // Shutdown 직후에도 한 프레임은 남아 있을 수 있으니 즉시 리셋
            Destroy(NetworkManager.Singleton.gameObject);
        }
    }
}
