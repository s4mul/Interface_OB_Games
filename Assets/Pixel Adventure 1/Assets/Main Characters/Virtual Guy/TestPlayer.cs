using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class TestPlayer : NetworkBehaviour 
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // SerializeField makes speed attribute appear in unity
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    protected Vector2 movement; protected float playerHalfWidth;
    private float xPosLastFrame;

    private NetworkVariable<int> random = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private NetworkVariable<MyCustomData> customNetworkVariable = new NetworkVariable<MyCustomData>(
        new MyCustomData
        {
            val = 123,
            istrue = true,
            message = "No new message"
        }
    );

    public struct MyCustomData: INetworkSerializable
    {
        public int val;
        public bool istrue;
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref val); 
            serializer.SerializeValue(ref istrue); 
            serializer.SerializeValue(ref message); 
        }
    }

    public override void OnNetworkSpawn()
    {
        playerHalfWidth = spriteRenderer.bounds.extents.x;
        xPosLastFrame = transform.position.x;
    }


    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            random.Value = Random.Range(1, 100);
        }


        HandleMovement();
        FlipCharacterX();
    }

    private void FlipCharacterX()
    {
        if (transform.position.x > xPosLastFrame)
        {
            // moving right
            spriteRenderer.flipX = false;
        }
        else if (transform.position.x < xPosLastFrame)
        {
            // moving left
            spriteRenderer.flipX = true;
        }

        xPosLastFrame = transform.position.x;
    }

    private void HandleMovement()
    {
        float xinput = Input.GetAxisRaw("Horizontal");
        float yinput = Input.GetAxisRaw("Vertical");
        movement.x = xinput * speed * Time.deltaTime;
        movement.y = yinput * speed * Time.deltaTime;

        transform.Translate(movement);

        if (xinput != 0 || yinput != 0)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
    }

    [ServerRpc] 
    public void TestServerRpc() // 꼭 ServerRpc로 함수명이 끝나야함.
    {

    }
}
