using Unity.Netcode;
using UnityEngine;

public class Door : BaseNetworkInteractable 
{
    public NetworkVariable<bool> isOpen = new NetworkVariable<bool>();
    public int doorId { get; private set; }
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider2D; 
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openedSprite;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        doorId = GlobalHelper.GenerateUniqueID();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    public override void Interact()
    {
        if (!CanInteract()) return;

        print("is interacting");
        if (isOpen.Value)
            openDoor();
        else
            closeDoor(); 
        isOpen.Value = !isOpen.Value;
    }

    public override bool CanInteract()
    {
        return true;
    }

    private void openDoor()
    {
        print("isOpened");
        boxCollider2D.offset = new Vector2(0.0f, 0.71875f);      
        boxCollider2D.size = new Vector2(0.2f, 1.4375f);
        spriteRenderer.sprite = openedSprite;
    }
    private void closeDoor()
    {
        print("isClosed");
        boxCollider2D.offset = new Vector2(0.5f, 0.71875f);      
        boxCollider2D.size = new Vector2(1.0f, 1.4375f);
        spriteRenderer.sprite = closedSprite;
    }
}
