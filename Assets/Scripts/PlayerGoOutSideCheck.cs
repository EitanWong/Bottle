using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGoOutSideCheck : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("Player"))
        {
            var TargetPos = other.transform.position;
            var X = TargetPos.x;
            var Z = TargetPos.z;
            if (TargetPos.x < -40 || TargetPos.x > 40)
            {
                X = 0;
            }
            if (TargetPos.z < -40 || TargetPos.z > 40)
            {
                Z= 0;
            }
            other.transform.position=new Vector3(X,5f,Z);
            
        }
    }
}
