using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable currentInteractableObject = null;
    [SerializeField] private InputActionAsset inputActions; // assign your .inputactions asset in Inspector
    private PlayerInput playerInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        playerInput.defaultActionMap = "InteractionDetecter";
        playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
        playerInput.onActionTriggered += OnInteract;
    } 

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.action.name == "Interact" && context.performed)
        {
            print("did stuff");
            currentInteractableObject?.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("collided with " + collision.gameObject.name);
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            currentInteractableObject = interactable;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        print("Now leaving " + collision.gameObject.name);
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == currentInteractableObject)
        {
            currentInteractableObject = null;
        }
    }
}