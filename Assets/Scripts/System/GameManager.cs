using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NTP;
using Papae.UnitySDK.Managers;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{
    public static GameManager INS;
    [SerializeField] public Setting GameSetting;//游戏设置
    public Data GameData = new Data();//游戏数据
    public Events GameEvents = new Events();//游戏事件

    private void Awake()
    {
        INS = this;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;//永不熄屏
        Application.runInBackground = true;

        Application.lowMemory += ClearMemory;//当内存低低时候自动清理内存
        GameData.InitData();//初始化数据
        Application.focusChanged += FocusChange;
        GameEvents.OnGameLose += GameData.SaveGameData;
        Physics.gravity = GameSetting.Gravity;
        //GameData.CheckTimeUpdate();//检测时间
        GameEvents.OnGameLose += GameData.AddDeathCount;//当玩家游戏失败时，增加死亡数量
        if (GameData.IsNewDay)
        {
            GameEvents.OnNewDay?.Invoke();
            GameEvents.EventOnNewDay?.Invoke();
        }

        GameEvents.OnGameStart += StartGameTimeCalculation;
        GameEvents.OnGameStop += UnPauseGame;
        // Debug.Log(GameData.ISFirstOpenApp);
    }

    private void Start()
    {


    }

    private void StartGameTimeCalculation()
    {
        StartCoroutine(GameData.UpdateGameTime());
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

    public void PauseGame()
    {
        FindObjectOfType<AudioManager>().PauseBGM();
        GameEvents.OnGamePause?.Invoke();
        GameEvents.EventOnGamePause?.Invoke();
        Time.timeScale = 0;
    }

    public void UnPauseGame()
    {
        FindObjectOfType<AudioManager>().ResumeBGM();
        GameEvents.OnGameResume?.Invoke();
        GameEvents.EventOnGameResume?.Invoke();

        Time.timeScale = 1;
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
    public void StopGame()
    {
        GameEvents.OnGameStop?.Invoke();
        GameEvents.EventOnGameStop?.Invoke();
        UnPauseGame();
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
        GUI.Label(new Rect(10, 30, 200, 200), Input.acceleration.ToString());
        GUI.Label(new Rect(10, 70, 200, 200), GameData.IsNewDay.ToString());

        switch (Application.internetReachability)
        {
            case NetworkReachability.ReachableViaLocalAreaNetwork:
                GUI.Label(new Rect(10, 120, 200, 200), "当前使用的是：WiFi");
                break;
            case NetworkReachability.ReachableViaCarrierDataNetwork:
                GUI.Label(new Rect(10, 120, 200, 200), "当前使用的是移动网络");
                break;
            default:
                GUI.Label(new Rect(10, 120, 200, 200), "当前没有联网");
                break;

        }
    }
}
