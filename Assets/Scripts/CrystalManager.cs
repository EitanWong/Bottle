using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CrystalManager : MonoBehaviour
{
    public  static  CrystalManager INS;
[HideInInspector]public List<string> CrystalList=new  List<string>();
private void Awake()
{
    var allType=FindObjectsOfType<Crystal>();
    foreach (var VARIABLE in allType)
    {
        CrystalList.Add(VARIABLE.transform.name);
    }
         INS = this;
        if (GameManager.INS.GameData.IsNewDay)
        {
            ES3.DeleteKey("Crystals");
        }
    
        if (ES3.KeyExists("Crystals"))
        {
            var LoadCrystalsData = ES3.Load<List<string>>("Crystals");
            CrystalList=LoadCrystalsData;
        }
        else
        {
           // Debug.Log(111);
            InitCrystal();
        }
    }

    public void InitCrystal()
    {
        ES3.Save<List<string>>("Crystals",CrystalList);
        CrystalList=ES3.Load<List<string>>("Crystals");
    }
    
    public void AddCrystal(int value,Crystal crystal)
    {
        GameManager.INS.GameData.AddCrystal(value);
        CrystalList.Remove(crystal.transform.name);
        Destroy(crystal.gameObject);
        ES3.Save<List<string>>("Crystals",CrystalList);
    }
}
