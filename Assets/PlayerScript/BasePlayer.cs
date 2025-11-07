using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;

public class BasePlayer: NetworkBehaviour 
{
    [SerializeField] protected float speed = 1.0f;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Animator animator;
    [SerializeField] protected Collider2D interactiveDetector;
    protected Vector2 movement; protected float playerHalfWidth;
    protected float xPosLastFrame;

    public override void OnNetworkSpawn()
    {
        playerHalfWidth = spriteRenderer.bounds.extents.x;
        xPosLastFrame = transform.position.x;
    }


    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

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


