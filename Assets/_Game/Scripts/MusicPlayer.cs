using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MusicPlayer : MonoBehaviour
{
    private AudioSource _audioSource;
    private Coroutine _lerpVolumeRoutine = null;
    private Coroutine _stopRouine = null;

    private void Awake()
    {
        // setup audio source
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
    }

    public void Play(AudioClip musicTrack, float fadeTime)
    {
        // could add safty check later like if (_audioSource.isPlaying) return;

        // start at zero volume and fade up
        _audioSource.volume = 0;
        _audioSource.clip = musicTrack;
        _audioSource.Play();

        // FadeVolume(MusicManager.Instance.Volume, fadeTime);
        FadeVolume(0.15f, fadeTime);
    }

    public void Stop(float fadeTime)
    {
        // reset if it's already going
        if (_stopRouine != null)
        {
            StopCoroutine(_stopRouine);
        }
        _stopRouine = StartCoroutine(StopRoutine(fadeTime));
    }

    public void FadeVolume(float targetVolume, float fadeTime)
    {
        targetVolume = Mathf.Clamp(targetVolume, 0, 1);
        if (fadeTime < 0)
        {
            fadeTime = 0;
        }

        if (_lerpVolumeRoutine != null)
        {
            StopCoroutine(_lerpVolumeRoutine);
        }

        _lerpVolumeRoutine = StartCoroutine(LerpVolumeRoutine(targetVolume, fadeTime));
    }

    private IEnumerator LerpVolumeRoutine(float targetVolume, float fadeTime)
    {
        float newVolume;
        float startVolume = _audioSource.volume;
        for (float elapsedTime = 0; elapsedTime <= fadeTime; elapsedTime += Time.deltaTime)
        {
            newVolume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / fadeTime);
            _audioSource.volume = newVolume;
            yield return null;
        }
        // ensure volume is set to target value
        _audioSource.volume = targetVolume;
    }

    private IEnumerator StopRoutine(float fadeTime)
    {
        // stop any fade in progress
        if (_lerpVolumeRoutine != null)
        {
            StopCoroutine(_lerpVolumeRoutine);
        }

        _lerpVolumeRoutine = StartCoroutine(LerpVolumeRoutine(0, fadeTime));

        // wait for blend to finish
        yield return _lerpVolumeRoutine;
        _audioSource.Stop();
    }
}
