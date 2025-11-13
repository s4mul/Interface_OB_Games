using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable currentInteractableObject = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
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
        print("Now leaving behind" + collision.gameObject.name);
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == currentInteractableObject)
        {
            currentInteractableObject = null;
        }
    }
}
