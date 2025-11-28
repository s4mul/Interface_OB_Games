using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NetworkObject))]
public class BasePlayer : NetworkBehaviour
{
    [SerializeField] protected float speed = 5.0f;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Animator animator;
    [SerializeField] protected Collider2D interactiveDetector;

    protected Rigidbody2D rb;
    protected Vector2 movementInput;

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

    public override void OnNetworkSpawn()
    {
        // 플레이어는 씬 전환에도 유지되어야 함
        DontDestroyOnLoad(gameObject);

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
            spriteRenderer.flipX = !current; // 오른쪽이면 flipX = false
        }
    }

    private void Update()
    {
        // 입력은 Owner만 처리
        if (!IsOwner) return;

        HandleInput();
    }

    private void FixedUpdate()
    {
        // 이동도 Owner만 처리
        if (!IsOwner) return;

        HandleMovement();
    }

    private void HandleInput()
    {
        float xinput = Input.GetAxisRaw("Horizontal");
        float yinput = Input.GetAxisRaw("Vertical");

        movementInput = new Vector2(xinput, yinput).normalized;

        // 애니메이션 상태 동기화
        bool runningNow = movementInput.magnitude > 0.1f;
        isRunning.Value = runningNow;

        // 방향 동기화
        if (xinput > 0)
        {
            facingRight.Value = true;
        }
        else if (xinput < 0)
        {
            facingRight.Value = false;
        }
    }

    private void HandleMovement()
    {
        // Rigidbody의 속도(velocity)를 직접 설정
        rb.linearVelocity = movementInput * speed;
    }

    [ServerRpc]
    public void TestServerRpc()
    {
        // 테스트용
    }
}
