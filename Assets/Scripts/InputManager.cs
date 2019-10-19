using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager Manager;
    int ScreenMiddle = Screen.width / 2;
    private bool V_IsRightTouch;
    public bool m_RightTouch//是否在右边屏幕按下
    {
        get
        {
            return V_IsRightTouch;
        }
    }
    private bool V_IsLeftTouch;
    public bool m_LeftTouch//是否在右边屏幕按下
    {
        get
        {
            return V_IsLeftTouch;
        }
    }
    public static InputManager INS
    {
        get
        {
            if (Manager == null)
                Manager = Transform.FindObjectOfType<InputManager>();
            return Manager;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Input.multiTouchEnabled = true;//启用多点触控
    }
    // Update is called once per frame
    void Update()
    {
CheckInput();
    }
    private void CheckInput()//检测输入
    {
        if (Application.isMobilePlatform)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)//检测屏幕按下
                {
                    if (touch.position.x >= ScreenMiddle)//如果在右边按下
                    {
                        V_IsRightTouch = true;
                    }
                    else//如果在左边按下
                    {
                        V_IsLeftTouch = true;
                    }
                }
                if (touch.phase == TouchPhase.Ended)//检测手指从屏幕抬起
                {
                    if (touch.position.x >= ScreenMiddle)//如果在右边抬起
                    {
                        V_IsRightTouch = false;
                    }
                    else//如果在左边抬起
                    {
                        V_IsLeftTouch = false;
                    }
                }
            }
        }
        else
        {
            V_IsRightTouch = Input.GetMouseButton(0);
            V_IsLeftTouch = Input.GetMouseButton(1);
        }
    }
    private void OnGUI() {
         GUI.Label(new Rect(10, 100, 200, 200), String.Format("左边{0},右边{1}", m_LeftTouch, m_RightTouch));
    }
}