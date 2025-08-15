using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // SerializeField makes speed attribute appear in unity
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    private Vector2 movement; private float playerHalfWidth;
    private float xPosLastFrame;
    void Start()
    {
        playerHalfWidth = spriteRenderer.bounds.extents.x;
        xPosLastFrame = transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
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
}
