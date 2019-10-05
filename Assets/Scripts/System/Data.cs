

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NTP;
using UnityEngine.Rendering.PostProcessing;

[System.Serializable]
public class Data
{
   #region 永久性存储数据
   public int BestHeight { get; set;}//玩家跳跃的最大高度
   public int CrystalCount { get; set;}//玩家收集的水晶数量
   public int DeathCount { get; set; }//玩家的死亡次数

   public int QualityIndex;

   public DateTime SaveDataTime;
   public bool IsNewDay;//新的一天

   public int X_MultiplyInpputValue=3;
   public int Y_MultiplyInpputValue=4;
   public int Schedule=0;//游戏进度
   public bool UsePostProcessing;//使用后置处理
   public bool UseLimitFPS;//使用限制FPS
   public int LimitFPS=60;//限制FPS数
   #endregion

   #region 临时存储数据

   public int Height;//高度
   public int Minute;//分
   public int Second;//秒
   public int GetCrystalCount;
   public bool HasNewHeight;
   
   public bool ISFirstOpenApp;//第一次打开游戏App
   #endregion
   
   #region 数据存储读取函数

   #region 存储数据名称

   private  const string SN_BestHeight="BestHeight";
   private  const string SN_MultiplX="MultiplXValue";
   private  const string SN_MultiplY="MultiplYValue";
   private const string SN_CrystalCount = "CrystalCount";
   private const string SN_SaveTime = "SaveTime";
   private const string SN_QualityIndex = "QualityIndex";
   private const string SN_UsePost = "UsePost";
   private const string SN_DeathCount = "DeathCount";
   private const string SN_LimitFPS = "LimitFPS";
   private const string SN_UseLimitFPS = "UseLimitFPS";
   private const string SN_ISFirstOpenApp = "ISFirstOpenApp";
   
   
   
   #endregion
   /// <summary>
   /// 初始化数据
   /// </summary>
 public void InitData()
   {
      ES3.Init();
     if(!ES3.KeyExists(SN_ISFirstOpenApp))
     {
        ISFirstOpenApp = true;
        ES3.Save<bool>(SN_ISFirstOpenApp,ISFirstOpenApp);
        GameManager.INS.GameEvents.OnFirstOpenApp?.Invoke();
        GameManager.INS.GameEvents.EventOnFirstOpenApp?.Invoke();
     }
     if (ES3.KeyExists(SN_BestHeight))
      {
        BestHeight =(int)ES3.Load<int>(SN_BestHeight);
      }
      if (ES3.KeyExists(SN_CrystalCount))
      {
        CrystalCount =(int)ES3.Load<int>(SN_CrystalCount);
      }
      if (ES3.KeyExists(SN_SaveTime))
      {
         SaveDataTime =ES3.Load<DateTime>(SN_SaveTime);
      }
      if (ES3.KeyExists(SN_QualityIndex))
      {
         QualityIndex = ES3.Load<int>(SN_QualityIndex);
         QualitySettings.SetQualityLevel(QualityIndex);
      }
      else
      {
         SaveQualityIndex(2);
      }
      if (ES3.KeyExists(SN_MultiplX))
      {
         X_MultiplyInpputValue = ES3.Load<int>(SN_MultiplX);
         if (X_MultiplyInpputValue < 1)
            X_MultiplyInpputValue = 1;
      }
      else
      {
         UpdateMultiplXValue(3);
      }
      if (ES3.KeyExists(SN_MultiplY))
      {
         Y_MultiplyInpputValue = ES3.Load<int>(SN_MultiplY);
         if (Y_MultiplyInpputValue < 1)
            Y_MultiplyInpputValue = 1;
      }
      else
      {
         UpdateMultiplYValue(4);
      }
      if (ES3.KeyExists(SN_UsePost))
      {
         UsePostProcessing = ES3.Load<bool>(SN_UsePost);
      }
      else
      {
         SavePostProcessing(false);
      }
      MonoBehaviour.FindObjectOfType<PostProcessVolume>().enabled = UsePostProcessing;
      MonoBehaviour.FindObjectOfType<PostProcessLayer>().enabled = UsePostProcessing;
      if (ES3.KeyExists(SN_DeathCount))
      {
         DeathCount = ES3.Load<int>(SN_DeathCount);
      }
      if (ES3.KeyExists(SN_LimitFPS))
      {
         LimitFPS=ES3.Load<int>(SN_LimitFPS);
      }
      else
      {
         SaveLimitFPS(60);
      }
      Application.targetFrameRate = LimitFPS;
      if (ES3.KeyExists(SN_UseLimitFPS))
      {
         UseLimitFPS = ES3.Load<bool>(SN_UseLimitFPS);
      }
      else
      {
         SaveUseLimitFPS(false);
      }

      CheckTimeUpdate();//检测时间
      GC.Collect();
   }
   /// <summary>
   /// 存取游戏数据
   /// </summary>
   public void SaveGameData()
   {
      SaveHeightData();
      // Thread t2 = new Thread(new ParameterizedThreadStart(TestMethod));
      GC.Collect();
      // StartCoroutine(SaveHeightData());
   }
   public void AddCrystal(int value)
   {
      CrystalCount += value;
      GetCrystalCount++;
      ES3.Save<int>(SN_CrystalCount,CrystalCount);
      GameManager.INS.GameEvents.OnCrystalUpdate?.Invoke(CrystalCount);
   }
   public void LessCrystal(int value)
   {
      if ((CrystalCount - value) < 0)
      {
         GameManager.INS.GameEvents. OnCrystalLessFailure.Invoke();
         return;
        
      }
      CrystalCount -=value;
      ES3.Save<int>(SN_CrystalCount,CrystalCount);
      GameManager.INS.GameEvents.OnCrystalUpdate?.Invoke(CrystalCount);
   }
   public void SaveQualityIndex(int index)
   {
      QualityIndex = index;
      ES3.Save<int>(SN_QualityIndex,QualityIndex);
      QualitySettings.SetQualityLevel(QualityIndex);
   }

   public void UpdateMultiplXValue(int value)
   {
      X_MultiplyInpputValue = value;
      ES3.Save<int>(SN_MultiplX,X_MultiplyInpputValue);
   }
   public void UpdateMultiplYValue(int value)
   {
      Y_MultiplyInpputValue = value;
      ES3.Save<int>(SN_MultiplY,Y_MultiplyInpputValue);
   }

   public void CheckTimeUpdate()
   {
      if (!ES3.KeyExists(SN_SaveTime))
      {
         ES3.Save<DateTime>(SN_SaveTime,NetworkTime.GetInstance().GetNetworkTime());//第一次存取时间
      }
      int compare1 = DateTime.Compare(NetworkTime.GetInstance().GetDay(),SaveDataTime);
      // Debug.Log(compare1);
      if (compare1 >= 1)
      {
         IsNewDay = true;
         ES3.Save<DateTime>(SN_SaveTime,NetworkTime.GetInstance().GetNetworkTime());//更新新的时间
      }
   }
   public void UpdateBestHeight(int Value)
   {
      
    GameManager.INS.GameEvents.OnNewHeightUpdate?.Invoke(Value);
      BestHeight = (int)Value;
      HasNewHeight = true;
   }
 
 public  void SaveHeightData()
   {
      //yield return new WaitForFixedUpdate();
      ES3.Save<int>(SN_BestHeight, (int)BestHeight);
   }

 public void SavePostProcessing(bool value)
 {
    UsePostProcessing = value;
     ES3.Save<bool>(SN_UsePost,UsePostProcessing);
     MonoBehaviour.FindObjectOfType<PostProcessVolume>().enabled = UsePostProcessing;
     MonoBehaviour.FindObjectOfType<PostProcessLayer>().enabled = UsePostProcessing;
 }

 public void AddDeathCount()
 {
    DeathCount++;
    ES3.Save<int>(SN_DeathCount,DeathCount);
 }

 WaitForSecondsRealtime OneSecond=new WaitForSecondsRealtime(1f);
public  IEnumerator UpdateGameTime()
 {
    while (true)
    {
       yield return OneSecond;
       Second++;
       if (Second >= 60)
       {
          Second = 0;
          Minute++;
       }
    }
 }

public void SaveUseLimitFPS(bool value)
{
   UseLimitFPS = value;
   ES3.Save<bool>(SN_UseLimitFPS,UseLimitFPS);
   Application.targetFrameRate = LimitFPS;
   if (!UseLimitFPS)
      Application.targetFrameRate = -1;
}

public void SaveLimitFPS(int value)
{
   LimitFPS = value;
   ES3.Save<int>(SN_LimitFPS,LimitFPS);
   Application.targetFrameRate = value;
}

#endregion
  
}