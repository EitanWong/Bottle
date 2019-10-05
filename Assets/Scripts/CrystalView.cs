using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CrystalView : MonoBehaviour
{
   [SerializeField] private Text CrystalText;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.INS.GameEvents.OnCrystalUpdate += UpdateCrystal;
        CrystalText.text = GameManager.INS.GameData.CrystalCount.ToString();
    }

    private void UpdateCrystal(int obj)
    {
        CrystalText.text = obj.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
