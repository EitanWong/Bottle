using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

public class CheckBreakPrice : MonoBehaviour
{
    [SerializeField] private uint TargetFragmentCount; //碎片数量
    private int NowFragmentCount;
    [SerializeField] private UnityEvent HasPrice;//当碎片数量足够时
    // Start is called before the first frame update
    private void Awake()
    {
        if (ES3.FileExists(transform.name + "FragmentCount"))
        {
            NowFragmentCount = ES3.Load<int>(transform.name + "FragmentCount");
        }
    }

    void Start()
    {
    }

    public void CheckPrice()
    {

        NowFragmentCount++;
        if (NowFragmentCount >= TargetFragmentCount)
          {
              NowFragmentCount = 0;
              HasPrice?.Invoke();
          }
        StartCoroutine(SaveData());
    }

    IEnumerator SaveData()
    {
        yield return  new WaitForFixedUpdate();
        ES3.Save<int>(transform.name+"FragmentCount",NowFragmentCount);
    }

// Update is called once per frame
    void Update()
    {
        
    }
}
