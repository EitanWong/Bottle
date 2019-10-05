using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using Random = System.Random;

[Serializable]
public class Crystal : MonoBehaviour
{
    [SerializeField] private float rotatespeed;
    [SerializeField] private UnityEvent OnCrystalAdd;
    Transform CrystalTrans;
    private void Awake()
    {
        if (!CrystalManager.INS.CrystalList.Contains(transform.name))
        {
            Destroy(gameObject);
        }
        CrystalTrans = transform;
    }
    float radian = 0; // 弧度  
    float perRadian = 0.03f; // 每次变化的弧度   上下浮动
    float radius = 0.25f; // 半径  
    Vector3 oldPos; // 开始时候的位置坐标  
    // Use this for initialization  

    // Update is called once per frame  
    void Update()
    {
        radian += perRadian; // 弧度每次加0.03  
        float dy = Mathf.Sin(radian) * radius; // dy定义的是针对y轴的变量，也可以使用sin，找到一个适合的值就可以  
        transform.position = oldPos + new Vector3(0, dy, 0);
    }

    private void Start()
    {
        StartCoroutine(RoateCrystal());
        oldPos = transform.position; // 将最初的位置保存到oldPos  
    }

   

    IEnumerator RoateCrystal()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0f,1f));
 
        while (true)
        {
            CrystalTrans.Rotate(Vector3.up*Time.deltaTime*rotatespeed);
            yield return  new WaitForFixedUpdate();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        CrystalManager.INS.AddCrystal(1,this);
        OnCrystalAdd?.Invoke();
    }
}
