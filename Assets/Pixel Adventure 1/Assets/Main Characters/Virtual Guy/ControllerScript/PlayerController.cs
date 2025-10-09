using UnityEngine;         // Unity의 기본 엔진 기능을 사용하기 위한 네임스페이스 (Transform, Animator, Rigidbody 등)
using Unity.Netcode;       // Netcode for GameObjects 관련 클래스 사용 (NetworkBehaviour, RPC 등)


// PlayerController 클래스: 각 플레이어 캐릭터의 움직임을 제어하는 스크립트
// NetworkBehaviour를 상속받아 네트워크 동기화와 소유자(Owner) 개념을 사용할 수 있음
public class PlayerController : NetworkBehaviour
{
    // 애니메이터, 리지드바디, 스프라이트 렌더러 참조용 변수 선언
    private Animator animator;          // 캐릭터 애니메이션 제어용
    private Rigidbody2D rb;             // 물리 기반 이동 처리를 위한 Rigidbody2D
    private SpriteRenderer sr;          // 좌우 반전(FlipX) 등을 위한 SpriteRenderer

    [SerializeField] private float moveSpeed = 5f;  // 이동 속도 (인스펙터에서 조정 가능)
    private Vector2 input;                          // 입력값 저장용 (x축 이동 방향)


    // 네트워크 객체가 스폰(생성)될 때 자동으로 호출되는 함수
    public override void OnNetworkSpawn()
    {
        // IsOwner: 이 객체가 현재 플레이어(로컬 클라이언트)의 것인지 확인
        // Owner가 아닌 경우(다른 플레이어의 캐릭터라면), 입력 제어 코드를 비활성화하여
        // 내 입력이 남의 캐릭터에 적용되지 않도록 막음
        
        if (!IsOwner)
        {
            enabled = false;
        }
        
    }


    // Start(): 게임 시작 시 한 번 실행되는 초기화 함수
    private void Start()
    {
        // Rigidbody2D, Animator, SpriteRenderer 컴포넌트를 가져옴
        // GetComponent는 해당 GameObject에 붙은 컴포넌트를 찾아 반환함
        enabled = true;
        rb = GetComponent<Rigidbody2D>();         // 물리 이동 제어용
        rb.gravityScale = 0f;                     // 중력 삭제(탑뷰)
        animator = GetComponent<Animator>();      // 애니메이션 전환용
        sr = GetComponent<SpriteRenderer>();      // 좌우 반전(Flip)용
    }


    // Update(): 매 프레임마다 호출됨 (입력 처리나 애니메이션 갱신에 사용)
    private void Update()
    {
        // 플레이어의 좌우 입력 감지
        // Input.GetAxisRaw("Horizontal")은 A/D 또는 ←/→ 키 입력을 -1, 0, 1로 반환함
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        // 애니메이터의 "isRunning" 파라미터를 설정
        // 움직이고 있으면 true, 멈춰 있으면 false → Idle/Run 애니메이션 전환에 사용
        animator.SetBool("isMoving", input.x != 0 || input.y != 0);

        // 캐릭터 방향 전환
        // 왼쪽으로 이동할 때는 스프라이트를 좌우 반전(FlipX = true)
        // 오른쪽으로 이동할 때는 기본 방향(FlipX = false)
        if (input.x != 0) sr.flipX = input.x < 0;
    }


    // FixedUpdate(): 일정한 시간 간격마다 호출됨 (물리 연산에 사용)
    private void FixedUpdate()
    {
        // Rigidbody2D의 속도를 직접 변경하여 이동 처리
        // x축 방향 속도는 입력값 × 이동속도, y축 속도는 기존 값 유지
        // 이렇게 하면 중력이나 점프 등 y축 물리효과가 유지되면서 좌우 이동만 제어됨
        rb.linearVelocity = input.normalized * moveSpeed;
    }
}
