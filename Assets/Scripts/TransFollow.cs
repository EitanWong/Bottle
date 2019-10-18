using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    // Start is called before the first frame update
    private Transform m_trans;

    private void Awake()
    {
        m_trans = transform;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (target)
        {
            m_trans.position = target.position;
        }
    }
}
