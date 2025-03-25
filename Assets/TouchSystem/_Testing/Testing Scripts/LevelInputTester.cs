using UnityEngine;

public class LevelInputTester : MonoBehaviour
{
    // Debug bool for testing
    public bool DebugMode = false;

    [SerializeField] private InputHandler _input;

    private void OnEnable()
    {
        _input.TouchStarted += OnTouchStarted;
        _input.TouchEnded += OnTouchEnded;
    }

    private void OnDisable()
    {
        _input.TouchStarted -= OnTouchStarted;
        _input.TouchEnded -= OnTouchEnded;
    }

    private void OnTouchStarted(Vector2 position)
    {
        if (DebugMode)
            Debug.Log("Level: Started at: " + position);
    }

    private void OnTouchEnded(Vector2 position)
    {
        if (DebugMode)
            Debug.Log("Level: Ended at: " + position);
    }
}
