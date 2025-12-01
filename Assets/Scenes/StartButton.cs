using UnityEngine;
using UnityEngine.UI;

public class UIStartButton : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "GameMap";

    private void Start()
    {
        var btn = GetComponent<Button>();
        GetComponent<Button>().onClick.AddListener(() =>
        {
            Debug.Log("[TEST BUTTON] CLICKED");
        });
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        Debug.Log("[UIStartButton] Clicked!");

        if (ClientRelay.LocalInstance == null)
        {
            Debug.LogWarning("[UIStartButton] ClientRelay not ready.");
            return;
        }

        ClientRelay.LocalInstance.RequestSceneChange(targetSceneName);
    }
}
