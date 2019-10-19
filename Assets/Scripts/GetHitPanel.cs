using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class GetHitPanel : MonoBehaviour
{
    [SerializeField] private List<Sprite> Sprites = new List<Sprite>();
        [SerializeField] private List<Color> Colors = new List<Color>();
    Image HitImage;
    private Meshinator meshinator;
    private void Awake()
    {
        meshinator = FindObjectOfType<Meshinator>();
        HitImage = GetComponent<Image>();
        meshinator.OnCollider += PlayHitFX;
        HitImage.color = Color.clear;
    }
    void PlayHitFX(uint counts)
    {
        if (!HitImage)
            return;
       // Debug.Log((int)counts);
         if((int)counts<Sprites.Count)
         {
        HitImage.color = Colors[(int)counts];
        HitImage.sprite = Sprites[(int)counts];

         }


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
