using UnityEngine;
using UnityEngine.UIElements;

public class TouchRaycaster : MonoBehaviour
{
    // Debug bool for testing
    public bool DebugMode = true;

    [SerializeField] private InputHandler _input;
    [SerializeField] private GameObject _touchVisual;

    private void Awake()
    {
        // disable by default
        _touchVisual.SetActive(false);
    }

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

    private void Update()
    {
        if (_input.TouchHeld)
        { 
            RepositionVisual(_input.TouchCurrentPosition);
        }
    }

    private void OnTouchStarted(Vector2 position)
    {
        if (DebugMode)
            Debug.Log("TouchRaycast: Started at: " + position);

        DetectWorldCollider(position);
        RepositionVisual(_input.TouchCurrentPosition);
    }

    private void DetectWorldCollider(Vector2 position)
    {
        // create ray from camera angle into tap point
        Ray ray = Camera.main.ScreenPointToRay(position);
        // if our ray hits a collider
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            // get the collider
            Collider collider = hitInfo.collider;
            // get the object
            GameObject obj = collider.gameObject;
            // get the name of the object
            string objName = obj.name;
            // log the name
            if (DebugMode)
                Debug.Log("TouchRaycast: Hit: " + objName);

            // check if the object has a Touchable component
            Touchable touchable = obj.GetComponent<Touchable>();
            // if it does, invoke the touch event
            if (touchable != null)
            {
                touchable.Touch();
            }
        }
    }

    private void RepositionVisual(Vector2 position)
    {
        // create ray from camera angle into tap point
        Ray ray = Camera.main.ScreenPointToRay(position);
        // if our ray hits a collider
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            // get the collider
            Collider collider = hitInfo.collider;
            // get the object
            GameObject obj = collider.gameObject;
            // get the name of the object
            string objName = obj.name;
            // log the name
            if (DebugMode)
                Debug.Log("TouchRaycast: Hit: " + objName);

            // moveing visual to hit point
            _touchVisual.transform.position = hitInfo.point;
            _touchVisual.SetActive(true);
        }
    }

    private void OnTouchEnded(Vector2 position)
    {
        if (DebugMode)
            Debug.Log("TouchRaycast: Ended at: " + position);

        _touchVisual.SetActive(false);
    }
}
