uusing UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class LocalLightingManager : MonoBehaviour
{
    [Header("Scene Light Settings")]
    [SerializeField] private Light2D globalLight; // 씬 전체를 비추는 Global Light
    [SerializeField] private float civilianIntensity = 1.0f; // 시민용 밝기
    [SerializeField] private float monsterIntensity = 0.05f; // 술래용 밝기 (아주 어둡게)
    [SerializeField] private Color monsterColor = new Color(0.1f, 0.1f, 0.2f); // 술래용 분위기(푸르스름한 어둠)

    private void Start()
    {
        // 게임 시작 시, 내 캐릭터가 스폰될 때까지 기다리는 코루틴 실행
        StartCoroutine(WaitForLocalPlayerAndSetupLight());
    }

    private IEnumerator WaitForLocalPlayerAndSetupLight()
    {
        // 1. 네트워크 매니저가 준비될 때까지 대기
        while (NetworkManager.Singleton == null)
        {
            yield return null;
        }

        // 2. 내 로컬 플레이어 객체가 생성될 때까지 대기
        // (ServerBootstrap에서 스폰해주는데 약간의 시간차 있음)
        while (NetworkManager.Singleton.LocalClient == null ||
               NetworkManager.Singleton.LocalClient.PlayerObject == null)
        {
            yield return null;
        }

        // 3. 내 플레이어 오브젝트 가져오기
        GameObject myPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;

        // 4. 내가 술래인지 확인
        // (ServerBootstrap이 술래에게는 'MonsterPrefab'을 줬으므로, 
        //  아까 만든 'MonsterVision' 컴포넌트나 'Light2D'가 있는지로 판단 가능)
        bool amIMonster = myPlayer.GetComponent<MonsterVision>() != null;

        // 혹은 MonsterVision 스크립트 이름이 다르다면 아래처럼 태그로 확인해도 됩니다.
        // bool amIMonster = myPlayer.CompareTag("Monster");

        // 5. 역할에 따른 조명 설정 적용
        ApplyLighting(amIMonster);
    }

    private void ApplyLighting(bool isMonster)
    {
        if (globalLight == null)
        {
            Debug.LogError("[LocalLightingManager] Global Light 2D가 연결되지 않았습니다!");
            return;
        }

        if (isMonster)
        {
            // 술래 설정: 어둡고 음산하게
            globalLight.intensity = monsterIntensity;
            globalLight.color = monsterColor;
            Debug.Log("[LocalLightingManager] 나는 '술래'입니다. 시야를 제한합니다.");
        }
        else
        {
            // 시민 설정: 밝고 평범하게
            globalLight.intensity = civilianIntensity;
            globalLight.color = Color.white;
            Debug.Log("[LocalLightingManager] 나는 '시민'입니다. 시야를 밝게 유지합니다.");
        }
    }
}