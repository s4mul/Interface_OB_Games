using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class AttackInvoker : MonoBehaviour
{
    private Person personInRange = null;
    [SerializeField] private InputActionAsset inputActions; // assign your .inputactions asset in Inspector
    private PlayerInput playerInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    UnityEvent attackEvent;
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("collided with " + collision.gameObject.name);
        if (collision.TryGetComponent(out Person target))
        {
            personInRange = target;
            attackEvent.Invoke();
        }
    }
}