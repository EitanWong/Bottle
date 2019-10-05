using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private bool RandomPitch;
    [SerializeField] private bool ignoreIsPlaying;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayAudio()
    {


        if (!ignoreIsPlaying)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            audioSource.Play();
        }

        if(RandomPitch)
            audioSource.pitch=Random.Range(0.5f,1.5f);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
