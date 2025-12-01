using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameSpriteButton : MonoBehaviour
{
    [SerializeField] private string lobbySceneName = "lobbyScene";
    [SerializeField] private string gameSceneName = "GameMap";

    private void Start()
    {
       
        
    }

    private void OnMouseDown()
    {
        Debug.Log("[SpriteButton] Start Game clicked.");

        if (ClientRelay.LocalInstance == null)
        {
            Debug.LogWarning("[SpriteButton] ClientRelay instance not ready.");
            return;
        }

        ClientRelay.LocalInstance.RequestSceneChange(gameSceneName);
    }
}
