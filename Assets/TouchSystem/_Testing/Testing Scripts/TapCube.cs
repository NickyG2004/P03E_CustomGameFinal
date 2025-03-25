using UnityEngine;

public class TapCube : MonoBehaviour
{
    public bool DebugMode = true;

    public void DestroyCube()
    {
        if (DebugMode)
            Debug.Log("TapCube: Destroyed cube");
        // Destroy is a common method on all gameObjects
        // 'gameObject' is the keyword for THIS gameObject
        Destroy(gameObject);
    }
}
