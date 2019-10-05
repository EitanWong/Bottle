// 我的FPS显示，除了FPS显示功能外还有计算时间差功能

using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class FPSShow : MonoBehaviour   //继承MyUtil，个人习惯，表明Util层的内容，这边可以直接继承MonoBehaviour
{
 
    // 单例
    private static FPSShow instance;
    public FPSShow()
    {
        instance = this;
    }
    public static FPSShow getInstance()
    {
        return instance;
    }
 
    // fps
    float fpsPassTime = 0f; //fps计算经过了多少秒
    int fpsCount = 0;   //当前fps的计数
    float fps = 0f; //最终每秒的fps数量
 
    // 时间差
    bool isNeedEndTime = false; //是否需要计算差值时间
    float startTime = 0f;   //时间差开始的时间
    float endTime = 0f; //结束的时间
 
    // 获取一次时间
    public void getEndTime()
    {
        isNeedEndTime = true;
    }
 
    // 增加帧率计数，可以由外部到用，计算FixedUpdate、Update都是可以的，不过注意该函数最好只在一个地方调用
    public void calcFrame()
    {
        ++fpsCount;
    }
 
    // 更新
    void Update ()
    {
        calcFrame();    //计算fps
 Debug.Log(Process.GetCurrentProcess ().PrivateMemorySize64*0.0009766);
      //  if (!gameSystem.isDebug)
         //   return;
 
        if (isNeedEndTime)  //需要计算差值时间，把结束的时间给开始时间，然后结束时间赋值为当前时间，最后在OnGUI中计算一个差值
        {
            startTime = endTime;
            endTime = Time.time;
            isNeedEndTime = false;
        }
 
        fpsPassTime += Time.deltaTime;  //增加fps经过的时间
        if(fpsPassTime > 2.0f)  //超过2秒了，平均一下，计算出一秒的帧率
        {
            fps = fpsCount / fpsPassTime;
            fpsCount = 0;
            fpsPassTime = 0f;
        }
    }

    [SerializeField] private int fontSize=10;
    void OnGUI()
    {
        //if (!gameSystem.isDebug)    //是否在debug状态，可以注释
         //   return;
 
        // 输出黄色、40号大字，太小怕看不见（害羞）
        GUIStyle style = new GUIStyle();
        style.normal.background = null;
        if(fps>=30)
            style.normal.textColor=Color.green;
        else
        {
            style.normal.textColor=Color.red;
        }
        //style.normal.textColor = new Color(1.0f, 0.5f, 0.0f);
        style.fontSize = fontSize;
 
        // 显示！
        GUI.Label(new Rect(Screen.width / 2 - 40, 0, 200, 200), string.Format("FPS:{0:0.00}, dTime:{1:0.0000}", fps, endTime - startTime), style); 
    }
}