using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SliderFiller : MonoBehaviour
{
    public float threshold;
    
    Slider slider;
    GameObject filler;
    float sliderValue = 0.012f;
    bool changed = false;

	// Use this for initialization
	void Start ()
    {
        // retrieve attached slider component
        slider = GetComponent<Slider>();

        // show a filled or empty bar based on threshold
        filler = slider.fillRect.gameObject;
        if (sliderValue <= threshold)
        {
            filler.SetActive(false);
        }
        else
        {
            filler.SetActive(true);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        // retrieve attached slider component
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }
        
        // record if an update has occured on the slider
        if (slider.value != sliderValue)
        {
            sliderValue = slider.value;
            changed = true;
        }

        // update the neccesarry changes
        if (changed)
        {
            filler = slider.fillRect.gameObject;

            if (sliderValue <= threshold)
            {
                filler.SetActive(false);
            }
            else
            {
                filler.SetActive(true);
            }

            changed = false;
        }
	}
}
