using UnityEngine;

public class MonsterVision : MonoBehaviour
{
    public float viewRadius = 5.0f; // 술래의 시야 반경
    public LayerMask targetLayer;   // 시민들이 포함된 레이어 (예: "Civilian")

    void Update()
    {
        // 1. 씬에 있는 모든 시민(Collider)을 찾습니다.
        // (성능 최적화를 위해 실제 게임에서는 매 프레임 OverlapCircle 대신
        //  List로 시민들을 관리하거나, 코루틴으로 0.1초마다 체크하는 것이 좋습니다.)
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, 50.0f, targetLayer);

        foreach (Collider2D target in targets)
        {
            float distance = Vector2.Distance(transform.position, target.transform.position);

            // 시민의 SpriteRenderer 컴포넌트 가져오기
            SpriteRenderer targetRenderer = target.GetComponent<SpriteRenderer>();
            if (targetRenderer != null)
            {
                if (distance <= viewRadius)
                {
                    // 시야 안: 보임
                    targetRenderer.enabled = true;
                }
                else
                {
                    // 시야 밖: 안 보임
                    targetRenderer.enabled = false;
                }
            }
        }
    }
}