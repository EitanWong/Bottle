using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ICE : MonoBehaviour
{
    float value;
    string DistortionName;
    public Material ICEMaterial;

   [Range(0.0001f,1f)] public float FadeSpeed;

    private void Awake()
    {
        DistortionName = "_BumpAmt";
        value = ICEMaterial.GetFloat(DistortionName);
         ICEMaterial.SetFloat(DistortionName, 0.5f);
    }
    // Start is called before the first frame update
    void Start()
    {

    }
    public void ShowIceFX(float TargetValue)
    {
StartCoroutine(FadeIceFX(TargetValue));
    }
    public void HideIceFX()
    {
StartCoroutine(FadeIceFX(0));
    }
    WaitForFixedUpdate WaitTime = new WaitForFixedUpdate();
    IEnumerator FadeIceFX(float TargetValue)
    {
        bool isdonw = false;
        while (!isdonw)
        {
            if (TargetValue > value)
            {
                value += Time.deltaTime * FadeSpeed;
                if(value>=TargetValue)
                isdonw=true;
            }
            else
            {
                value -= Time.deltaTime * FadeSpeed;
                if(value<=0)
                {
                     isdonw=true;
                     value=0;
                }
               
            }
            ICEMaterial.SetFloat(DistortionName, value);
            yield return WaitTime;
        }


    }
    // Update is called once per frame
    void Update()
    {

    }
}
