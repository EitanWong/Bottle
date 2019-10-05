using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Trigger : MonoBehaviour
{
    [SerializeField] private Transform EnterTarget;
    [SerializeField]
    UnityEvent OnEnter;

    [SerializeField] private UnityEvent OnExit;
    
    private void OnTriggerEnter(Collider other)
    {
        if (EnterTarget)
        {
            if(other.transform!=EnterTarget)
                return;
        }

        OnEnter?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (EnterTarget)
        {
            if(other.transform!=EnterTarget)
                return;
        }
        OnExit?.Invoke();
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
