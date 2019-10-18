using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FllowTarget : MonoBehaviour
{
    [SerializeField] private Transform Target;

    private Transform m_trans;

    private Vector3 Dis;
    // Start is called before the first frame update
    void Start()
    {
        m_trans = transform;
        if (Target)
        {
            Dis =m_trans.position- Target.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
       m_trans.position = Target.position+ Dis;

    }
}
