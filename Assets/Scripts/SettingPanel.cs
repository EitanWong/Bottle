using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    [SerializeField] private Dropdown QualitySet;

    [SerializeField] private InputField XinputMu;
    [SerializeField] private InputField YinputMu;
    [SerializeField] private InputField LimitFPS;
    [SerializeField] private Toggle UseLimitFPS;
    [SerializeField] private Toggle UsePosProcessing;
    // Start is called before the first frame update
    void Start()
    {
        QualitySet.onValueChanged.AddListener(UpdateQualitySet);
        QualitySet.value = GameManager.INS.GameData.QualityIndex;
        XinputMu.text = GameManager.INS.GameData.X_MultiplyInpputValue.ToString();
        YinputMu.text = GameManager.INS.GameData.Y_MultiplyInpputValue.ToString();
        UsePosProcessing.isOn = GameManager.INS.GameData.UsePostProcessing;
        UseLimitFPS.isOn = GameManager.INS.GameData.UseLimitFPS;
        LimitFPS.gameObject.SetActive(UseLimitFPS.isOn);
        XinputMu.onValueChanged.AddListener(UpdateXInputValue);
        YinputMu.onValueChanged.AddListener(UpdateYInputValue);
        LimitFPS.onEndEdit.AddListener(UpdateLimitFPS);
        UseLimitFPS.onValueChanged.AddListener(UpdateUseLimitFPS);
        UsePosProcessing.onValueChanged.AddListener(UpdateUsePostProcessing);
    }

    private void UpdateUseLimitFPS(bool arg0)
    {
        GameManager.INS.GameData.SaveUseLimitFPS(arg0);
        LimitFPS.gameObject.SetActive(UseLimitFPS.isOn);
    }

    private void UpdateLimitFPS(string arg0)
    {
        var vale = int.Parse(arg0);
        if ( vale< 30)
        {
            vale = 30;
            LimitFPS.text = vale.ToString();
        }

        GameManager.INS.GameData.SaveLimitFPS(vale);
    }

    private void UpdateUsePostProcessing(bool arg0)
    {
        GameManager.INS.GameData.SavePostProcessing(arg0);
    }

    private void UpdateYInputValue(string arg0)
    {
        var value = int.Parse(arg0);
        if (value < 1)
        {
            value = 1;
            YinputMu.text = 1.ToString();
        }

       
        GameManager.INS.GameData.UpdateMultiplYValue(value);
    }

    private void UpdateXInputValue(string arg0)
    {
        var value = int.Parse(arg0);
        if (value < 1)
        {
            value = 1;
            XinputMu.text = 1.ToString();
        }

       
      GameManager.INS.GameData.UpdateMultiplXValue(value);
    }

    private void UpdateQualitySet(int arg0)
    {
        GameManager.INS.GameData.SaveQualityIndex(arg0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
