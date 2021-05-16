using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : PoolableObject
{
    AudioSource audioSource;
    AudioClip audioClip;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public override void Reset()
    {
        audioClip = null;
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

    public void SetAudioClip(AudioClip audioClip)
    {
        this.audioClip = audioClip;
    }
}
