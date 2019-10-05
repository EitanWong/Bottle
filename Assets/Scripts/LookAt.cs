using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
  [SerializeField]  private Transform Target;

  private Transform m_trans;

  private void Awake()
  {
      m_trans = transform;
  }

  // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if(Target)
        m_trans.LookAt(Target.position/100);
    }
}
