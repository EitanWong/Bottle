using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StarFX : MonoBehaviour
{
    private Transform m_tran;
  [SerializeField]float rotatespeed;
  [SerializeField]float Scalespeed;
  [SerializeField] private UnityEvent OnFXEed;
 // [SerializeField] private Transform Target;
    private void Awake()
    {
        m_tran = transform;
        StartCoroutine(Roate());
       // m_tran.position = Target.position;
    }

    // Start is called before the first frame update


    // Update is called once per frame
    void Update()
    {
        
    }
    private WaitForFixedUpdate WaitFixed=new WaitForFixedUpdate();
    IEnumerator Roate()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0f,1f));
 
        while (true)
        {

            m_tran.Rotate(Vector3.forward*Time.deltaTime*rotatespeed);
            yield return  WaitFixed;
        }
    }

    private Coroutine ScaleCor;
    public void ShowScaleFx()
    {
        ScaleCor= StartCoroutine(Scale());
    }

    IEnumerator Scale()
    {
        while (true)
        {
            m_tran.localScale-=Vector3.one *Scalespeed;
            if (m_tran.localScale.x <= 0)
            {
                m_tran.localScale=Vector3.zero;
                OnFXEed?.Invoke();
                StopCoroutine(ScaleCor);
            }
            yield return WaitFixed;
        }
        
    }
}
