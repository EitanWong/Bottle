using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackLight : MonoBehaviour
{
    private Transform m_trans;
    public  float x, y, z, w;
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
        //第一人称控制器左右旋转
      var  offsetMouseX = Input.acceleration.x;
      var offsetMouseY = Input.acceleration.y;
      //第一人称控制器左右旋转
     // offsetMouseX = Input.mousePosition.normalized.x;
     // offsetMouseY = Input.mousePosition.normalized.y;wa
     // m_trans.rotation=new Quaternion(offsetMouseX*10,offsetMouseY*10,90,0);
     m_trans.rotation=new Quaternion(x+offsetMouseY*5,y-offsetMouseX*5,z,w);
   // m_trans.Rotate(offsetMouseX,offsetMouseY,0);


    }
}