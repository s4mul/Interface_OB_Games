using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // 따라갈 타겟 (플레이어)의 Transform
    public Transform target;

    // 카메라 이동 속도 (값이 클수록 빠르게 따라감)
    public float smoothSpeed = 5f;

    // 카메라와 타겟 사이의 Z축 거리 유지
    // (기본 카메라의 Z축 값인 -10f 등으로 설정)
    public float zOffset = -10f;

    // LateUpdate는 모든 Update가 끝난 후에 호출됩니다.
    // 플레이어가 움직인 '후'에 카메라가 따라가게 하므로,
    // 카메라 떨림(Jitter) 현상을 방지할 수 있습니다.
    void LateUpdate()
    {
        if (target != null)
        {
            // 1. 목표 위치 설정 (플레이어의 x, y 값 + 카메라의 z offset)
            Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, zOffset);

            // 2. 부드러운 이동 (Lerp: 선형 보간)
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // 3. 카메라 위치를 부드럽게 이동한 위치로 업데이트
            transform.position = smoothedPosition;

            /*
            // 만약 부드러운 이동(Lerp) 없이 즉시 따라가게 하려면
            // 아래 한 줄로 대체하세요.
            // transform.position = desiredPosition;
            */
        }
    }
}