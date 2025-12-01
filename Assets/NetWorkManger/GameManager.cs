using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private List<ulong> connectedClients = new();
    private bool monsterSelected = false;

    public ulong MonsterId { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // 서버에서 호출됨
    public void RegisterClient(ulong clientId)
    {
        if (!connectedClients.Contains(clientId))
            connectedClients.Add(clientId);
    }

    // 몬스터는 1번만 랜덤 결정
    public void EnsureMonsterSelected()
    {
        if (monsterSelected) return;

        if (connectedClients.Count > 0)
        {
            int index = Random.Range(0, connectedClients.Count);
            MonsterId = connectedClients[index];
            monsterSelected = true;

            Debug.Log($"[GameManager] Monster selected: {MonsterId}");
        }
    }

    public bool IsMonster(ulong clientId)
    {
        return clientId == MonsterId;
    }
}
