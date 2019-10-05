using UnityEngine;
using UnityEngine.UI;

public class PanelOptions : MonoBehaviour
{
    // store a reference to the Game Object MenuPanel 
    public GameObject mainMenu;
    // store a reference to the Game Object OptionsPanel 
    public GameObject optionsMenu;
    // store a reference to the Game Object overlay
    public GameObject overlay;
    // store a reference to the Game Object PausePanel 
    public GameObject pauseMenu;
    // store a reference to the Game Object MenuButton
    public GameObject menuButton;                           

    // function to activate and display the main menu panel
    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        mainMenu.GetComponent<CanvasGroup>().interactable = true;
    }

    // function to dsiable interaction with the main menu panel
    public void DisableMainMenu()
    {
        mainMenu.GetComponent<CanvasGroup>().interactable = false;
    }

    // function to deactivate and hide the main menu panel
    public void HideMainMenu()
    {
        mainMenu.SetActive(false);
    }

    // function to activate and display the pptions panel
    public void ShowOptionsMenu()
	{
        optionsMenu.SetActive(true);
		overlay.SetActive(true);
	}

	// function to deactivate and hide the options panel
	public void HideOptionsMenu()
	{
        optionsMenu.SetActive(false);
		bool enable = UnityEngine.SceneManagement.SceneManager.GetActiveScene ().buildIndex == 2;
		overlay.SetActive(enable);
    }

    // function to activate and display the pause menu panel
    public void ShowPauseMenu()
	{
        pauseMenu.SetActive (true);
		overlay.SetActive(true);
	}

    // function to disable interaction with the pause menu panel
    public void DisablePauseMenu()
    {
        pauseMenu.GetComponent<CanvasGroup>().interactable = false;
    }

    // function to deactivate and hide the pause menu panel
    public void HidePauseMenu()
	{
        pauseMenu.SetActive (false);
		overlay.SetActive(false);
	}

    // function to enable interaction with the menu button
    public void EnableMenuButton()
    {
        menuButton.GetComponent<Button>().enabled = true;
    }

    // function to disable interaction with th menu button
    public void DisableMenuButton()
    {
        menuButton.GetComponent<Button>().enabled = false;
    }
}
