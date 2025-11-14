
using Unity.Netcode;

public abstract class BaseNetworkInteractable : NetworkBehaviour, IInteractable 
{
    public abstract bool CanInteract();
    public abstract void Interact();
}