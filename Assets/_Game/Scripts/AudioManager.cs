using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // get music track
    [SerializeField] AudioClip _introSong;

    // start playing music from the music manager
    private void Start()
    {
        // set the volume of the music player
        MusicManager.Instance.Play(_introSong, 3f);
    }
}
