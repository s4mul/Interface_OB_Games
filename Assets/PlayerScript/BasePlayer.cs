using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NetworkObject))]
public class BasePlayer : NetworkBehaviour
{
    // velocity를 사용할 것이므로 Time.deltaTime이 빠집니다.
    // Inspector에서 이 값을 1.0이 아닌 5.0이나 10.0처럼 더 큰 값으로 설정해야 합니다.
    [SerializeField] protected float speed = 5.0f;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Animator animator;
    [SerializeField] protected Collider2D interactiveDetector;
    [SerializeField] private InputActionAsset inputActions;

    protected Vector2 movement;
    protected float playerHalfWidth;
    protected float xPosLastFrame;

    // NEW: 물리 처리를 위한 Rigidbody 2D 변수
    protected Rigidbody2D rb;
    // NEW: 입력 값을 저장할 변수
    protected Vector2 movementInput;
    private InputAction moveAction;

    // 애니메이션 상태 동기화를 위한 NetworkVariable
    private NetworkVariable<bool> isRunning = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    // 바라보는 방향 (true = 오른쪽, false = 왼쪽)
    private NetworkVariable<bool> facingRight = new(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // OnNetworkSpawn은 네트워크 관련 초기화에 사용
    public override void OnNetworkSpawn()
    {
        // 플레이어는 씬 전환에도 유지되어야 함
        DontDestroyOnLoad(gameObject);

        if (IsOwner)
        {
            // Enable the action map for local player
            var map = inputActions.FindActionMap("Player");
            map.Enable();

            // Cache movement action
            moveAction = map.FindAction("Move");
        }

        // 네트워크 값 변경 시 애니메이션/스프라이트 갱신
        isRunning.OnValueChanged += OnIsRunningChanged;
        facingRight.OnValueChanged += OnFacingRightChanged;

        // 스폰 시 초기 상태 반영
        OnIsRunningChanged(false, isRunning.Value);
        OnFacingRightChanged(true, facingRight.Value);
    }

    public override void OnNetworkDespawn()
    {
        isRunning.OnValueChanged -= OnIsRunningChanged;
        facingRight.OnValueChanged -= OnFacingRightChanged;
    }

    private void OnIsRunningChanged(bool previous, bool current)
    {
        if (animator != null)
        {
            animator.SetBool("isRunning", current);
        }
    }

    private void OnFacingRightChanged(bool previous, bool current)
    {
        if (spriteRenderer != null)
        {
            // 오른쪽이면 flipX = false
            spriteRenderer.flipX = !current;
        }
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
        // 서버 이동 처리로 변경되었으므로 클라이언트는 처리하지 않음
        if (!IsServer) return;

        // NOTE: 서버는 MoveServerRpc에서 매 프레임 velocity를 갱신하므로
        // FixedUpdate에서 별도 호출할 필요 없음.
    }

    // NEW: 입력 처리와 시각적 처리를 담당
    private void HandleInputAndVisuals()
    {
        if (moveAction == null) return;

        // Read input from InputAction
        movementInput = moveAction.ReadValue<Vector2>();

        // Animation
        animator.SetBool("isRunning", movementInput.magnitude > 0.1f);

        // Flip sprite based on X input
        if (movementInput.x > 0) spriteRenderer.flipX = false;
        else if (movementInput.x < 0) spriteRenderer.flipX = true;

        // 애니메이션 상태 동기화
        bool runningNow = movementInput.magnitude > 0.1f;
        isRunning.Value = runningNow;

        // 방향 동기화
        if (movementInput.x > 0)
        {
            facingRight.Value = true;
        }
        else if (movementInput.x < 0)
        {
            facingRight.Value = false;
        }

        // 서버 이동 요청
        MoveServerRpc(movementInput);
    }

    // MODIFIED: 이제 Rigidbody의 velocity를 서버에서 설정합니다.
    // Time.deltaTime을 곱하지 않습니다.
    [ServerRpc]
    public void MoveServerRpc(Vector2 input)
    {
        if (!IsServer) return;

        // Rigidbody의 속도(velocity)를 직접 설정합니다.
        rb.linearVelocity = input * speed;
    }

    // REMOVED: FlipCharacterX() 함수는 HandleInputAndVisuals()로 통합되었습니다.
}
