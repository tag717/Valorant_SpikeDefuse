using UnityEngine;

public class AudioPlayer
{
    AudioSource audioSource;
    public AudioPlayer(AudioSource audioSource)
    {
        this.audioSource = audioSource;
    }
    public void PlaySE(AudioClip _clip)
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }

}
