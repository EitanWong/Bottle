using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MicSoundCheck : MonoBehaviour
{
    [SerializeField] private MicInput input;
    private Slider slider;
    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.maxValue = input.m_difference;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        slider.value = input.volume;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
