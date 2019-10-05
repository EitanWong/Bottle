using Papae.UnitySDK.Managers;
using UnityEngine;
using System.Collections;

public class PauseOptions : MonoBehaviour
{
    // reference to the SceneOptions script
    private SceneOptions sceneOptions;
    // reference to the PanelOptions script
    private PanelOptions panelOptions;

    bool isPaused = false;

    void Awake()
	{
        // retrieve the attached PanelOptions script
        panelOptions = GetComponent<PanelOptions>();
        // retrieve the attached SceneOptions script
        sceneOptions = GetComponent<SceneOptions>();
	}

	// Update is called once per frame
	void Update ()
    {
		// is Escape key pressed while the game is not paused and that we're not in main menu
		if (Input.GetButtonDown ("Cancel") && !isPaused && !sceneOptions.IsInMainMenu) 
		{
            // pause the game
            Pause();
		} 
		// if game is paused and not in main menu
		else if (Input.GetButtonDown ("Cancel") && isPaused && !sceneOptions.IsInMainMenu) 
		{
            // unpause the game
            Resume();
		}
	}


	public void PauseGame()
	{
		AudioManager.Instance.PlayOneShot(AudioManager.Instance.LoadClip("button"), Pause);
    }

    void Pause()
    {
        isPaused = true;
        // display the pause menu
        panelOptions.ShowPauseMenu();
        // this will cause animations and physics to stop updating
        //Time.timeScale = 0;
    }

    public void UnpauseGame()
	{
		AudioManager.Instance.PlayOneShot(AudioManager.Instance.LoadClip("button"), Resume);
	}

    void Resume()
    {
        // this will cause animations and physics to continue updating at regular speed
        //Time.timeScale = 1;

        isPaused = false;
        // hide the pause menu
        panelOptions.HidePauseMenu();
    }

}
