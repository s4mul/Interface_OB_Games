using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapColliderFixer : MonoBehaviour
{
    private Tilemap tilemap;
    private TilemapCollider2D tileCollider;
    private CompositeCollider2D composite;

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        tileCollider = GetComponent<TilemapCollider2D>();
        composite = GetComponent<CompositeCollider2D>();
    }

    private void OnEnable()
    {
        // 씬 로딩 직후 한 프레임 뒤에 Collider 재생성
        StartCoroutine(RebuildNextFrame());
    }

    private System.Collections.IEnumerator RebuildNextFrame()
    {
        // 씬 로딩 완전히 끝난 뒤 한 프레임 대기
        yield return null;
        yield return null; // 2프레임 대기하면 더 안정적

        if (tileCollider != null)
        {
            // 타일맵 변경 사항 반영
            tileCollider.ProcessTilemapChanges();
        }

        if (composite != null)
        {
            // Transform 갱신
            Physics2D.SyncTransforms();

            // Composite 콜라이더 강제로 리빌드
            composite.GenerateGeometry();
        }

        Debug.Log("[TilemapColliderFixer] Collider Rebuilt!");
    }
}
