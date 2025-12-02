using UnityEngine;

public class CameraFollowController : MonoBehaviour
{
    [SerializeField] private Transform target;   // 따라다닐 플레이어
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset;

    public void SetTarget(Transform t)
    {
        target = t;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * smoothSpeed);
    }
}
