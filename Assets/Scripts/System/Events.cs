
using System;
using UnityEngine.Events;

[System.Serializable]
public class Events
{
    public Action OnGameLose;//游戏失败事件
    public Action OnGameStart;//游戏开始事件
    public Action OnGameStop;//游戏停止事件
    public Action OnGameEnd;//游戏结局事件
    public Action OnGameReset;//游戏重制事件
    public Action OnNewDay;//新的一天
    public Action OnFirstOpenApp;//第一次启动游戏
    public UnityEvent EventOnGameLose;//游戏失败事件
    public UnityEvent EventOnGameStart;//游戏开始事件
    public UnityEvent EventOnGameStop;//游戏停止事件
    public UnityEvent EventOnGameEnd;//游戏结局事件
    public UnityEvent EventOnGameReset;//游戏重制事件

    public UnityEvent EventOnNewDay;//新的一天
    public UnityEvent EventOnFirstOpenApp;//第一次启动游戏
    
    public Action<int> OnNewHeightUpdate;
    public Action<int> OnCrystalUpdate;
    public Action OnCrystalLessFailure;
}
