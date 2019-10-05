using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    [SerializeField] private Transform target;

    private Transform m_trans;
    private Vector3 dir;
    //private Light m_light;
    private void Awake()
    {
        m_trans = GetComponent<Transform>();
    }

    // Start is called before the first frame update
    void Start()
    {
     dir=   m_trans.position - target.position;
    }

    // Update is called once per frame
    void Update()
    {
        m_trans.position = target.position + dir;
    }
}
