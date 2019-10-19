using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraTrack : MonoBehaviour
{
    private CinemachineVirtualCamera v_Camera;
    private CinemachineComposer Composer;

    private bool isTouch;
    [SerializeField]
   bool IsMainCamera;
 //   private CinemachineTransposer Transposer;
  //  private CinemachinePOV POV;
  private float OriginFieldOfView;
    private void Awake()
    {
        //Input.multiTouchEnabled = true;//开启多点触碰
        v_Camera=GetComponent<CinemachineVirtualCamera>();
        Composer = v_Camera.GetCinemachineComponent<CinemachineComposer>();
      //  Transposer=v_Camera.GetCinemachineComponent<CinemachineTransposer>();
        //POV=v_Camera.GetCinemachineComponent<CinemachinePOV>();
       v_Camera.m_Lens.FieldOfView=60f;
        OriginFieldOfView = v_Camera.m_Lens.FieldOfView;
    }

    void Start ()
    {

    }
    void Update ()
    {

        var Ismobie = Application.isMobilePlatform;


if(InputManager.INS)
    isTouch= isTouch=InputManager.INS.m_LeftTouch;
        if (isTouch)
        {
            if(v_Camera.m_Lens.FieldOfView<100)
                v_Camera.m_Lens.FieldOfView += (v_Camera.m_Lens.FieldOfView*Time.deltaTime)+Time.deltaTime*ChangeValue;
        }
        else
        {
            if (v_Camera.m_Lens.FieldOfView > OriginFieldOfView)
            {
                var value= Time.deltaTime*ChangeValue*5;
                if (v_Camera.m_Lens.FieldOfView - value >= OriginFieldOfView)
                {
                    v_Camera.m_Lens.FieldOfView -= value;
                }
                
                if (v_Camera.m_Lens.FieldOfView < OriginFieldOfView)
                {
                    v_Camera.m_Lens.FieldOfView = OriginFieldOfView;
                }
            }
        }

        
        
        if (Ismobie)
        {
            if (Composer)
            {
                Composer.m_TrackedObjectOffset.x = -Input.acceleration.x;
                Composer.m_TrackedObjectOffset.y = -Input.acceleration.y;
            }
            if (IsMainCamera)
            {
                transform.eulerAngles=new Vector3(60+(-Input.acceleration.y),0,-Input.acceleration.x);
            }
            //Transposer.m_FollowOffset.x=Input.acceleration.x;
         //  Transposer.m_FollowOffset.y=1+Input.acceleration.y;
            
        }
        else
        {
            return;
            var Middle = new Vector3(Screen.width / 2, Screen.height / 2);
            if (Composer)
            {
                Composer.m_TrackedObjectOffset.x = (Input.mousePosition-Middle).normalized.x;
               Composer.m_TrackedObjectOffset.y = (Input.mousePosition-Middle).normalized.y; 
            }
            if (IsMainCamera)
            {
            //    transform.eulerAngles=new Vector3(60+(Input.mousePosition-Middle).normalized.x,0,(Input.mousePosition-Middle).normalized.y);
            }
            // Transposer.m_FollowOffset.x=(Input.mousePosition-Middle).normalized.x;
          //  Transposer.m_FollowOffset.y=1+(Input.mousePosition-Middle).normalized.y;
        }

 

       

      
    }

    public float ChangeValue;
   // public Movement Player;
}
