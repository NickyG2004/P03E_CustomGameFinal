using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Attach this component to any gameObject and the Unity Event will
/// fire appropriately. NOTE: This is using a component insted of
/// an interface, since it is primarily intended to be used for 
/// gameObjects in the world.
/// </summary>

public class Touchable : MonoBehaviour
{
    [SerializeField] private ParticleSystem _touchParticlePrefab;
    [SerializeField] private AudioClip _touchSound;
    public UnityEvent Touched;

    public void Touch()
    {
        // play sound effect
        if (_touchSound)
            AudioSource.PlayClipAtPoint
                    (_touchSound, Camera.main.transform.position);

        // play particle effect
        if (_touchParticlePrefab)
            Instantiate(_touchParticlePrefab, transform.position, Quaternion.identity);

        Touched?.Invoke();
    }
}
