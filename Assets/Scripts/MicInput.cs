
/*
 *语音录入类
 *获取到当前语音录入的音量
 */

using System;
using UnityEngine;
using UnityEngine.UI;

public class MicInput : MonoBehaviour
{
 
//用于观察麦克风输入的音量大小
public float volume;
//音量倍数
 [SerializeField] private float difference=0;
 public float m_difference
 {
     get { return difference;}
     set
     {
         if (value < 1)
             value = 1;
         difference = value;
         ES3.Save<float>("MicDifference",difference);
     }
 }

//存放录制的语音
AudioClip micRecord;
//麦克风的设备名称
string device;

private void Awake()
{
    if (!ES3.KeyExists("MicDifference"))
    {
        difference = 5;
        ES3.Save<float>("MicDifference",difference);
    }


    difference = ES3.Load<float>("MicDifference");
}

private void Start()
{
    //读取麦克风设备名，一台手机或者电脑可以接入多个麦克风，下标为0读取第一个麦克风
    device = Microphone.devices[0];
    //开始录音，device麦克风名称；loop循环录制；lengthSec录制长度；frequency频率啥的，这里的44100是默认值
    micRecord = Microphone.Start(device, true, 999, 44100);
}

private void Update()
{
    //取得当前输入的最大音量值
    volume = (float)Math.Round(GetMaxVolume(), 4) * difference;
}
 
///<summary>
///获取当前输入的音量最大值
///</summary>
///<returns>返回最大音量</returns>
float GetMaxVolume()
{
    float maxVolume = 0f;
 
    //定义一个float类型的数组用于存储这段录音的音量数组
    float[] volumeData = new float[128];
    //偏移样本，从当前麦克风所在位置开始读取
    int offset = Microphone.GetPosition(device) - 128 + 1;
    if (offset < 0)//麦克风的开始位置通常是负数，规范偏移值
    {
        return 0;
    }
 
    //从offset位置开始，获取一段录音的数据并存放到volumeData数组中
    micRecord.GetData(volumeData, offset);
    //从取得的数组中找出最大值
    for (int i = 0; i < volumeData.Length; i++)
    {
        float tempMax = volumeData[i];
        if (tempMax > maxVolume)
        {
            maxVolume = tempMax;
        }
    }
    return maxVolume;
}
 
}