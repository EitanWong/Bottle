using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultCheck : MonoBehaviour
{
    [SerializeField] private Text MaxHeight;

    [SerializeField] private Text DeathCount;
    [SerializeField] private Text GameTime;

    [SerializeField] private Text CrystalCountText;

    public void UpdateData()
    {
        if (GameManager.INS.GameData.HasNewHeight)
        {
            MaxHeight.text = string.Format("新记录:{0}m",GameManager.INS.GameData.BestHeight.ToString()); 
        }
        else
        {
            MaxHeight.text = string.Format("最佳:{0}m",GameManager.INS.GameData.Height.ToString()); 
        }
        DeathCount.text = String.Format("总计破碎{0}次",GameManager.INS.GameData.DeathCount.ToString());
        
        GameTime.text=String.Format("时间:{0}分{1}秒",GameManager.INS.GameData.Minute,GameManager.INS.GameData.Second);
        CrystalCountText.text = GameManager.INS.GameData.GetCrystalCount.ToString();
        GC.Collect();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
