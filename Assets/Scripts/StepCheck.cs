using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepCheck : MonoBehaviour
{
    private Transform trans;
    [SerializeField] private float radius;
    [SerializeField]private Material NextMaterial;
   [SerializeField] private LayerMask WatIsStep;
   private Material LastStepMatertial;
    private MeshRenderer NextStep;

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

             LastStepMatertial = result.GetComponent<MeshRenderer>().material;
            
             if (NextStep)
             {
                 if(NextStep.material!=NextMaterial)
                    NextStep.material = LastStepMatertial;
             }
             NextStep = result.GetComponent<MeshRenderer>();
             NextStep.material=NextMaterial;
         }
     }
    }



    private void OnDrawGizmos()
    {
        Gizmos.color=Color.magenta;
        Gizmos.DrawWireSphere(transform.position,radius);
    }
}
