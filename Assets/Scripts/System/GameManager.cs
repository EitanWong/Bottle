using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NTP;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{
    public static GameManager INS;
 [SerializeField] public Setting GameSetting;//游戏设置
 public Data GameData=new Data();//游戏数据
 public Events GameEvents=new Events();//游戏事件

 private void Awake()
    {
        INS = this;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;//永不熄屏
        Application.runInBackground = true;

            Application.lowMemory+=ClearMemory;//当内存低低时候自动清理内存
            GameData.InitData();//初始化数据
            Application.focusChanged += FocusChange;
            GameEvents.OnGameLose += GameData.SaveGameData;
            StartCoroutine(GameData.UpdateGameTime());
            //GameData.CheckTimeUpdate();//检测时间
         GameEvents.OnGameLose += GameData.AddDeathCount;//当玩家游戏失败时，增加死亡数量
         if (GameData.IsNewDay)
         {
             GameEvents.OnNewDay?.Invoke();
             GameEvents.EventOnNewDay?.Invoke();
         }
        // Debug.Log(GameData.ISFirstOpenApp);
    }

 private void Start()
 {


 }
 private void FocusChange(bool obj)
 {
    GameData.SaveGameData();
 }
 private void OnDestroy()
 {
     Application.focusChanged -= FocusChange;
 }
 private void ClearMemory()
 {
   GC.Collect();
 }

 

 public void LoseGame()
 {
     GameEvents.OnGameLose?.Invoke();
     GameEvents.EventOnGameLose?.Invoke();
 }

 public void ResetGame()
 {
     GameEvents.OnGameReset?.Invoke();
     GameEvents.EventOnGameReset?.Invoke();
 }

 public void StartGame()
 {
     GameEvents.OnGameStart?.Invoke();
     GameEvents.EventOnGameStart?.Invoke();
 }

 public void EndGame()
 {
     GameEvents.OnGameEnd?.Invoke();
     GameEvents.EventOnGameEnd?.Invoke();
 }

 private void OnApplicationQuit()
 {
    GameData.SaveHeightData();
 }
 

 public void RestartGame()
 {
     GC.Collect();
     SceneManager.LoadSceneAsync(Application.loadedLevelName); 
     GC.Collect();
 }

 private void OnGUI()
 {
     GUILayout.Label(Input.acceleration.ToString());
 }
}
