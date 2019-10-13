using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTimeCheck : MonoBehaviour
{
    private Data Gamedata;

    [SerializeField] private Text TimeText; 
    // Start is called before the first frame update
    void Start()
    {
        Gamedata = GameManager.INS.GameData;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        TimeText.text=String.Format("{0}分{1}秒",Gamedata.Minute,Gamedata.Second);
    }
}
