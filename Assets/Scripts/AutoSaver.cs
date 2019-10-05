using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

public class AutoSaver : MonoBehaviour
{
    private Transform m_Trans;
    private MeshFilter m_MeshF;
   [SerializeField] private bool SaveTrans;
   [SerializeField] private bool SaveMesh;
   [SerializeField] private bool OnLoseResetData;
   private string TransSaveName, MeshSaveName;
   private void Awake()
    {
        m_Trans = transform;
        m_MeshF = GetComponent<MeshFilter>();
        TransSaveName = m_Trans.name + "Pos";
        MeshSaveName = m_Trans.name + "Mesh";
        Application.focusChanged += OnFocusChange;

    }



   public void InitData()
    {
        if (ES3.KeyExists(TransSaveName))
        {
            m_Trans.position = ES3.Load<Vector3>(TransSaveName);
        }

        if (ES3.KeyExists(MeshSaveName))
        {
            m_MeshF.sharedMesh=ES3.Load<Mesh>(MeshSaveName);
            try
            {
               GetComponent<MeshCollider>().sharedMesh = m_MeshF.sharedMesh;
            }
            catch (Exception e)
            {
            }
        }


    }

   private bool isGameOver;
    // Start is called before the first frame update
    void Start()
    {
        InitData();
        if (OnLoseResetData)
            GameManager.INS.GameEvents.OnGameLose += ClearData;
    }

    public void ClearData()
    {
        if(OnLoseResetData)
            isGameOver = true;
        if (ES3.KeyExists(TransSaveName))
        ES3.DeleteKey(TransSaveName);
        if (ES3.KeyExists(MeshSaveName))
        ES3.DeleteKey(MeshSaveName);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnFocusChange(bool isChange)
    {
        if(this)
            StartCoroutine(Save());
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            StartCoroutine(Save());
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            StartCoroutine(Save());
    }

    IEnumerator Save()
    {
        if (!isGameOver)
        {
            yield return new  WaitForFixedUpdate();
            yield return new WaitForEndOfFrame();
            SaveData();
            GC.Collect();
        }
        else if(isGameOver)
        {
            
                ClearData();
        }
    }

    public void SaveData()
    {
        if (SaveTrans)
        {
            if (m_Trans)
            {
                ES3.Save<Vector3>(TransSaveName,m_Trans.position);   
            }
        }
        if (SaveMesh)
        {
            if (m_MeshF)
            {
                ES3.Save<Mesh>(MeshSaveName,m_MeshF.mesh);   
            }
        } 
        GC.Collect();
    }

    private void OnApplicationQuit()
    {
        SaveData();
        if(isGameOver)
            ClearData();
    }
    
}