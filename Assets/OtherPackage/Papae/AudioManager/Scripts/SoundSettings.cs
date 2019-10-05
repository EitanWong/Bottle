using Papae.UnitySDK.Managers;
using UnityEngine;
using UnityEngine.UI;


public class SoundSettings : MonoBehaviour
{
    public Toggle MusicToggle, SoundFxToggle;

    void OnEnable()
    {
        if (!MusicToggle || !SoundFxToggle)
        {
            Debug.LogError("Please assign the neccesary toggle variables in the inspector", this);
            return;
        }

        // set Toggle Properties
		MusicToggle.isOn = AudioManager.Instance.IsMusicOn;
		SoundFxToggle.isOn = AudioManager.Instance.IsSoundOn;

        // add Listeners
        MusicToggle.onValueChanged.AddListener(ToggleBGMusic);
        SoundFxToggle.onValueChanged.AddListener(ToggleSoundFx);
    }

    void OnDisable()
    {
        // remove Listeners
        MusicToggle.onValueChanged.RemoveAllListeners();
        SoundFxToggle.onValueChanged.RemoveAllListeners();
    }

    public void ToggleBGMusic(bool flag)
    {
		AudioManager.Instance.IsMusicOn = flag;
    }

    public void ToggleSoundFx(bool flag)
    {
		AudioManager.Instance.IsSoundOn = flag;
    }
}

