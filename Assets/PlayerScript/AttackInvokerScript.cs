using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class AttackInvoker: MonoBehaviour
{
    private Person personInRange = null;
    [SerializeField] private InputActionAsset inputActions; // assign your .inputactions asset in Inspector
    private PlayerInput playerInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    UnityEvent attackEvent;
    void Start()
    {
        InitializeForLocalPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeForLocalPlayer()
    {
        playerInput = gameObject.AddComponent<PlayerInput>();
        playerInput.actions = inputActions;
        playerInput.defaultActionMap = "Interaction";
        playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
        playerInput.onActionTriggered += OnInteract;
    } 

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.action.name == "Interact" && context.performed)
        {
            print("attack");
            if (personInRange != null)
            {
                attackEvent.Invoke();
            } 
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("collided with " + collision.gameObject.name);
        if (collision.TryGetComponent(out Person target))
        {
            personInRange = target;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        print("Now leaving " + collision.gameObject.name);
        if (collision.TryGetComponent(out Person _))
        {
            personInRange = null;
        }
    }
}