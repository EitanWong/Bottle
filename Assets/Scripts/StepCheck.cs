using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepCheck : MonoBehaviour
{
    private Transform trans;
    [SerializeField] private float radius;
   // [SerializeField]private Material NextMaterial;
   [SerializeField] private LayerMask WatIsStep;
   private Outline LastStepMatertial;

   private void Awake()
    {
        trans = transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        CheckNetStep();
    }

    // Update is called once per frame


    private void CheckNetStep()
    {
     var AllStep=  Physics.OverlapSphere(trans.position, radius,WatIsStep);
     float distence=Mathf.Infinity;
     
     foreach (var VARIABLE in AllStep)
     {
         var dis = Vector3.Distance(VARIABLE.transform.position,trans.position);
         Transform result=null;
         if (VARIABLE.transform.position.y > trans.position.y)
         {
             if (dis < distence)
             {
                 distence = dis;
                 result = VARIABLE.transform;
             }
         }
         if (result)
         {

             if(LastStepMatertial)
             LastStepMatertial.enabled=false;
             
             var Step =result.GetComponent<Outline>();
             Step.enabled = true;
             LastStepMatertial = Step;
         }
     }
    }



    private void OnDrawGizmos()
    {
        Gizmos.color=Color.magenta;
        Gizmos.DrawWireSphere(transform.position,radius);
    }
}
