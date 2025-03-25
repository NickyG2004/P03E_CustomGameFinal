using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class InputHandler : MonoBehaviour
{
    // Debug bool for testing
    public bool DebugMode = false;

    private InputSystem_Actions _inputSystemActions;
    public event Action<Vector2> TouchStarted;
    public event Action<Vector2> TouchEnded;
    public Vector2 TouchStartPosition { get; private set; }
    public Vector2 TouchCurrentPosition { get; private set; }
    public bool TouchHeld { get; private set; }

    private void Awake()
    {
        _inputSystemActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _inputSystemActions.Enable();
        _inputSystemActions.Player.TouchPoint.performed += OnTouchPerformed;
        _inputSystemActions.Player.TouchPoint.canceled += OnTouchCanceled;
    }

    private void OnDisable()
    {
        _inputSystemActions.Player.TouchPoint.performed -= OnTouchPerformed;
        _inputSystemActions.Player.TouchPoint.canceled -= OnTouchCanceled;
        _inputSystemActions.Disable();
    }

    private void OnTouchPerformed(InputAction.CallbackContext context)
    {
        if (DebugMode)
            Debug.Log("Touch");
        //Change our public bool
        TouchHeld = true;
        // read position from our input action
        Vector2 TouchPosition = context.ReadValue<Vector2>();
        // save start position
        TouchStartPosition = TouchPosition;
        // update current position - here it's our start
        TouchCurrentPosition = TouchPosition;
        // send event notification for listeners
        TouchStarted?.Invoke(TouchPosition);
        if (DebugMode)
            Debug.Log("Touch Start Position: " + TouchStartPosition);
    }

    private void OnTouchCanceled(InputAction.CallbackContext context)
    {
        if (DebugMode)
            Debug.Log("Release");
        // revert our public bool
        TouchHeld = false;
        // send notification for listeners of last know position
        TouchEnded?.Invoke(TouchCurrentPosition);
        if (DebugMode)
            Debug.Log("Touch End Position: " + TouchCurrentPosition);
        // clear out touch positions when there's no input
        TouchStartPosition = Vector2.zero;
        TouchCurrentPosition = Vector2.zero;
    }

    private void Update()
    {
        if (TouchHeld)
        {
            // update current position
            TouchCurrentPosition = _inputSystemActions.Player.TouchPoint.ReadValue<Vector2>();
        }
    }
}

