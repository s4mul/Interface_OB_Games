using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

enum Role {
    MONSTER = 1,
    PERSON = 0
};

public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private GameObject personPrefab;
    private GameObject spawnedObject;
    private void Awake()
    {
        // Subscribe once when the game starts
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        Role role = figureOutRole(clientId);
        if (role == Role.MONSTER)
            spawnedObject = Instantiate(monsterPrefab);
        else
            spawnedObject = Instantiate(personPrefab);
        spawnedObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            
    }

    private Role figureOutRole(ulong clientId)
    {
        if (clientId == NetworkManager.ServerClientId) return Role.MONSTER;
        return Role.PERSON;
    }
}
