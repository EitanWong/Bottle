using System;
using System.Collections;
using System.Collections.Generic;
using Papae.UnitySDK.Managers;
using UnityEngine;

public class AudioTrigger: MonoBehaviour
{
    [SerializeField] private bool PlayOnAwake;
    [SerializeField] private AudioClip OnAwakePlayClip;
    [SerializeField] private float CorssTime;
    public void ChangeAudio(AudioClip clip)
    {  	float duration = Mathf.Clamp(CorssTime, 0, int.MaxValue);
       // CrossField.text = duration.ToString();
        AudioManager.Instance.PlayBGM(clip, MusicTransition.CrossFade, duration);
        
    }

    private void Awake()
    {
        if(PlayOnAwake)
            ChangeAudio(OnAwakePlayClip);
    }
}
