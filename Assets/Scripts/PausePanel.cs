using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PausePanel : MonoBehaviour
{
   // private bool ispause;
    public void Pause()
    {
       GameManager.INS.PauseGame();
       // Time.timeScale = 0;
    }
    public void UnPause()
    {
        GameManager.INS.UnPauseGame();
       // Time.timeScale = 1;
    }
    private void OnEnable()
    {
      //  Pause();
    }

    private void OnDisable()
    {
     //   UnPause();
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
