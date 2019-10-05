using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ColliderCheck : MonoBehaviour
{
    [SerializeField] private UnityEvent OnEnter;

    [SerializeField] private UnityEvent OnExit;

    private void OnCollisionEnter(Collision other)
    {
        if(other.transform!=transform&&other.transform!=transform.parent)
             OnEnter?.Invoke();
    }

    private void OnCollisionExit(Collision other)
    {
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
