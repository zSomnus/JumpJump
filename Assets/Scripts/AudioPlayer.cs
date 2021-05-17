using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : PoolableObject
{
    [SerializeField] AudioSource audioSource;
    AudioClip audioClip;
    
    public override void Reset()
    {
        audioClip = null;
        audioSource.volume = 1;
        audioSource.pitch = 1;
    }

    private void OnEnable()
    {
        if (audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }

    private void Update()
    {
        if(!audioSource.isPlaying)
        {
            gameObject.SetActive(false);
        }
    }

    public void SetAudioClip(AudioClip audioClip, float volume = 1, float pitch = 1)
    {
        this.audioClip = audioClip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
    }
}
