using UnityEngine;
using UnityEngine.InputSystem;

public class movementController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    float speed = 0.1f;
    void Start()
    {
        Application.targetFrameRate = 60; // Set the target frame rate to 60 FPS
    }

    // Update is called once per frame
    void Update()
    {
        bool move =
            Keyboard.current.wKey.isPressed ||
            Keyboard.current.aKey.isPressed ||
            Keyboard.current.sKey.isPressed ||
            Keyboard.current.dKey.isPressed;

        bool shift =
            Keyboard.current.leftShiftKey.isPressed ||
            Keyboard.current.rightShiftKey.isPressed;
        int x = 0;
        int y = 0;
        if (move)
        {
            if (shift)
            {
                speed = 0.5f; // Running speed
            }
            else
            {
                speed = 0.2f; // Walking speed
            }

            if (Keyboard.current.wKey.isPressed)
            {
                (x, y)  = (0, 1);
            } else if(Keyboard.current.aKey.isPressed)
            {
                (x, y) = (-1, 0);
            } else if(Keyboard.current.sKey.isPressed)
            {
                (x, y) = (0, -1);
            } else if(Keyboard.current.dKey.isPressed)
            {
                (x, y) = (1, 0);
            }
        }
        else
        {
            (x, y) = (0, 0);
        }

        transform.Translate(x * speed, y * speed, 0);
    }

}
