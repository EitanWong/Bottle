using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraTrack : MonoBehaviour
{
    private CinemachineVirtualCamera v_Camera;
    private CinemachineComposer Composer;

    // public CinemachineTransposer Transposer { get; private set; }

    private bool isTouch;
    [SerializeField]
    bool IsMainCamera;
    [SerializeField] float MaxZ;
    [SerializeField] float MinZ;
    private CinemachineTransposer Transposer;
    //  private CinemachinePOV POV;
    private float OriginFieldOfView;
    [SerializeField] Vector3 OriginOffset;
    bool Ismobie;
    private void Awake()
    {
        //Input.multiTouchEnabled = true;//开启多点触碰
        v_Camera = GetComponent<CinemachineVirtualCamera>();
        Composer = v_Camera.GetCinemachineComponent<CinemachineComposer>();
        Transposer = v_Camera.GetCinemachineComponent<CinemachineTransposer>();
        //POV=v_Camera.GetCinemachineComponent<CinemachinePOV>();
        v_Camera.m_Lens.FieldOfView = 60f;
        v_Camera.m_Lens.LensShift.x = 0;
        v_Camera.m_Lens.LensShift.y = 0;
        Transposer.m_FollowOffset.z = MinZ;
        Transposer.m_FollowOffset = OriginOffset;
        OriginFieldOfView = v_Camera.m_Lens.FieldOfView;
        Ismobie = Application.isMobilePlatform;
    }

    void Start()
    {

    }
    private void LateUpdate()
    {
        if (!Ismobie)
        {
            var Middle = new Vector3(Screen.width / 2, Screen.height / 2);
            // Composer.m_TrackedObjectOffset.x = (Input.mousePosition-Middle).normalized.x;
            // Composer.m_TrackedObjectOffset.y = (Input.mousePosition-Middle).normalized.y; 
            if (!IsMainCamera)
            {
                v_Camera.m_Lens.LensShift.x = (Input.mousePosition - Middle).normalized.x / 5;
                v_Camera.m_Lens.LensShift.y = (Input.mousePosition - Middle).normalized.y / 5;
                //    transform.eulerAngles=new Vector3(60+(Input.mousePosition-Middle).normalized.x,0,(Input.mousePosition-Middle).normalized.y);
            }
            // Transposer.m_FollowOffset.x=(Input.mousePosition-Middle).normalized.x;
            //  Transposer.m_FollowOffset.y=1+(Input.mousePosition-Middle).normalized.y;
        }
    }
    void Update()
    {
        if (InputManager.INS)
            isTouch = isTouch = InputManager.INS.m_LeftTouch;
        if (isTouch)
        {
            if (v_Camera.m_Lens.FieldOfView < 100)
                v_Camera.m_Lens.FieldOfView += (v_Camera.m_Lens.FieldOfView * Time.deltaTime) + Time.deltaTime * ChangeValue;
        }
        else
        {
            if (v_Camera.m_Lens.FieldOfView > OriginFieldOfView)
            {
                var value = Time.deltaTime * ChangeValue * 5;
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




        if (IsMainCamera)
        {
            if (InputManager.INS.m_RightTouch)
            {
                Transposer.m_FollowOffset.z -= Time.deltaTime * JumpViewChangeValue;
                if (Transposer.m_FollowOffset.z <= MaxZ)
                    Transposer.m_FollowOffset.z = MaxZ;
            }
            else
            {
                Transposer.m_FollowOffset.z += Time.deltaTime * JumpViewChangeValue;
                if (Transposer.m_FollowOffset.z >= MinZ)
                    Transposer.m_FollowOffset.z = MinZ;
            }

        }






    }
    public float JumpViewChangeValue;
    public float ChangeValue;
    // public Movement Player;
}
