using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

// Rigidbody 2D 컴포넌트가 이 오브젝트에 꼭 필요하다고 명시합니다.
[RequireComponent(typeof(Rigidbody2D))]
public class BasePlayer : NetworkBehaviour
{
    // velocity를 사용할 것이므로 Time.deltaTime이 빠집니다.
    // Inspector에서 이 값을 1.0이 아닌 5.0이나 10.0처럼 더 큰 값으로 설정해야 합니다.
    [SerializeField] protected float speed = 5.0f;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Animator animator;

    // NEW: 물리 처리를 위한 Rigidbody 2D 변수
    protected Rigidbody2D rb;
    // NEW: 입력 값을 저장할 변수
    protected Vector2 movementInput;

    // MODIFIED: Awake에서 컴포넌트를 가져옵니다.
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // OnNetworkSpawn은 네트워크 관련 초기화에 사용
    public override void OnNetworkSpawn()
    {
        // playerHalfWidth와 xPosLastFrame는 이제 사용하지 않습니다.
    }

    // Update는 매 프레임 호출 (입력 처리에 적합)
    void Update()
    {
        if (!IsOwner) return;

        // 입력 받기, 애니메이션, 뒤집기를 처리합니다.
        HandleInputAndVisuals();
    }

    // NEW: FixedUpdate는 고정된 주기로 호출 (물리 처리에 적합)
    void FixedUpdate()
    {
        if (!IsOwner) return;

        // 물리적인 이동을 처리합니다.
        HandleMovement();
    }

    // NEW: 입력 처리와 시각적 처리를 담당
    private void HandleInputAndVisuals()
    {
        float xinput = Input.GetAxisRaw("Horizontal");
        float yinput = Input.GetAxisRaw("Vertical");

        // 대각선 이동이 더 빠르지 않도록 입력을 정규화(normalized)
        movementInput = new Vector2(xinput, yinput).normalized;

        // --- 애니메이션 처리 ---
        // movementInput.magnitude는 벡터의 크기(길이)입니다. 0보다 크면 움직이는 중.
        if (movementInput.magnitude > 0.1f)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }

        // --- 캐릭터 뒤집기 (입력 기반) ---
        // 실제 위치가 아닌 '입력'을 기준으로 뒤집어야 벽에 부딪혔을 때도 방향을 유지합니다.
        if (xinput > 0)
        {
            spriteRenderer.flipX = false; // 오른쪽
        }
        else if (xinput < 0)
        {
            spriteRenderer.flipX = true; // 왼쪽
        }
    }

    // MODIFIED: 이제 Rigidbody의 velocity를 사용해 이동합니다.
    private void HandleMovement()
    {
        // Rigidbody의 속도(velocity)를 직접 설정합니다.
        // Time.deltaTime을 곱하지 않습니다.
        rb.linearVelocity = movementInput * speed;
    }

    // REMOVED: FlipCharacterX() 함수는 HandleInputAndVisuals()로 통합되었습니다.

    [ServerRpc]
    public void TestServerRpc() // 꼭 ServerRpc로 함수명이 끝나야함.
    {

    }
}