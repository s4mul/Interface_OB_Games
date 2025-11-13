using Unity.Netcode;
using UnityEngine;

public class Door : NetworkBehaviour, IInteractable
{
    public NetworkVariable<bool> isOpen = new NetworkVariable<bool>(false);
    public int doorId { get; private set; }
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider2D; 
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite opendSprite;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        doorId = GlobalHelper.GenerateUniqueID();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();
    }



    public void Interact()
    {
        if (!CanInteract()) return;
        print("is interacting");
        isOpen.Value = !isOpen.Value;
        if (isOpen.Value)
            openDoor();
        else
            closeDoor(); 
    }

    public bool CanInteract()
    {
        return true;
    }

    private void openDoor()
    {
        print("isOpened");
        boxCollider2D.enabled = false;        
        spriteRenderer.sprite = opendSprite;
    }
    private void closeDoor()
    {
        print("isClosed");
        boxCollider2D.enabled = false;        
        boxCollider2D.enabled = true;        
        spriteRenderer.sprite = closedSprite;
    }
}
