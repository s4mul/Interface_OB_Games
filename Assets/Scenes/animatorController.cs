using UnityEngine;
using UnityEngine.InputSystem;

public class animatorController : MonoBehaviour
{

    private static bool isWalk = false;
    private static bool isStay = true;
    private static bool isRun = false;
    private bool[] animLis = {isWalk, isStay, isRun};
    private string[] animName = { "isWalk", "isStay", "isRun" };
    private Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        if(animator == null)
        {
            Debug.LogError("Animator component not found on the GameObject.");
            return;
        }
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
        animLis[0] = move && !shift; // isWalking
        animLis[1] = !move; // isStay
        animLis[2] = move && shift; // isRunning
        Debug.Log($"isWalking: {animLis[0]}, isStay: {animLis[1]}, isRunning: {animLis[2]}");

        for (int i = 0; i < animLis.Length; i++)
        {
            animator.SetBool(animName[i], animLis[i]);
        }
    }
}
