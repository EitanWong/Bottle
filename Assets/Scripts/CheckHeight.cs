using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckHeight : MonoBehaviour
{
    [SerializeField] private Movement Target;

    [SerializeField] private Text HeightView;

    private string ScoreText="最佳";
    // Start is called before the first frame update
    void Start()
    {
        GameManager.INS.GameEvents.OnNewHeightUpdate += UpdateNewHeight;
    }

    private void UpdateNewHeight(int obj)
    {

        ScoreText = "新纪录";
        //HeightView.text=string.Format("新纪录:{0}m,当前:{1}m", obj,(int)Target.m_Trans.position.y);
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }
    private void FixedUpdate()
    {
        if (Target)
        {
            var NowHeight = (int) Target.m_Trans.position.y;
            if (Target.m_Grounded)
            {
                if (NowHeight < 0)
                    NowHeight = 0;
                HeightView.text = string.Format(ScoreText+":{0}m,当前:{1}m", Target.m_Height,NowHeight);
            }
        }
    }
}
