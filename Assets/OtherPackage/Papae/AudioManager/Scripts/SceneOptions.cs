using Papae.UnitySDK.Managers;
using UnityEngine;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif


public class SceneOptions : MonoBehaviour
{
    // reference to the black screen animator component.
    public Animator blackscreenAnimator;
    // fade to transparent animation clip
    public AnimationClip fadeToTransparentAnimationClip;

    public bool IsInMainMenu
    {
		get { return SceneManager.GetActiveScene().buildIndex == 1; }
    }

    // reference to the PanelOptions script
    PanelOptions panelOptions;                  

    
    void Awake()
	{
		// retrieve the attached PanelOptions script
		panelOptions = GetComponent<PanelOptions> ();
	}

    void Start()
	{
		if (IsInMainMenu) 
		{
			AudioManager.Instance.EmptyPlaylist();
		} 
		else
		{
			AudioManager.Instance.LoadPlaylist("Playlist", true);
		}

        // fade the black screen to transparent
        blackscreenAnimator.SetTrigger("toTransparent");
        // play right background music if none is currently active
		PlayMusic();
	}

    public void PlayMusic()
    {
        AudioClip clip = null;

        // load the scenes by name to decide which music clip to play.
		switch (SceneManager.GetActiveScene().buildIndex)
        {
            // load the title music from the resources folder using AudioManager
            case 1:
                clip = AudioManager.Instance.LoadClip("MenuMusic");
                break;
            // load the game music from the resources folder using AudioManager
            case 2:
                clip = AudioManager.Instance.LoadClip("GameMusic");
                break;
        }

        // play the background music using the fade transition from the assigned clip
		if (clip && !AudioManager.Instance.IsMusicPlaying)
        {
			AudioManager.Instance.PlayBGM(clip);
        }
    }

    // function used to load or exit gameplay
    public void StartGame(bool flag)
    {
		if (flag) 
		{
			AudioManager.Instance.PlayOneShot (AudioManager.Instance.LoadClip ("button"));
			LoadGame();
		} 
		else 
		{
			AudioManager.Instance.PlayOneShot (AudioManager.Instance.LoadClip ("button"));
			LoadMainMenu();
		}
    }

    void LoadGame()
    {
        // fade out current music and fade in next music in 1s
		AudioManager.Instance.PlayBGM(AudioManager.Instance.LoadClip("GameMusic"), MusicTransition.CrossFade, 2f);
        
        // disable interaction with the main menu UI
        panelOptions.DisableMainMenu();

        // delay calling of LoadGameScene by half the length of fadeColorAnimationClip
        Invoke("LoadGameScene", fadeToTransparentAnimationClip.length * 1);

        // trigger the transparent Animator to start transition to the FadeToOpaque state.
        blackscreenAnimator.SetTrigger("toOpaque");
    }

    void LoadGameScene()
	{
        SceneManager.LoadScene(2);
	}

    void LoadMainMenu()
    {
        // fade out current music and fade in next music in 1s
		AudioManager.Instance.PlayBGM(AudioManager.Instance.LoadClip ("MenuMusic"), MusicTransition.CrossFade, 2f);

        // disable interaction with the pause menu UI
        panelOptions.DisablePauseMenu();

        // delay calling of LoadMenuScene by half the length of fadeColorAnimationClip
        Invoke("LoadMenuScene", fadeToTransparentAnimationClip.length * 1);

        // trigger of Animator 'animColorFade' to start transition to the FadeToOpaque state.
        blackscreenAnimator.SetTrigger("toOpaque");
    }

	void LoadMenuScene()
	{
        SceneManager.LoadScene(1);
    }

    // display on or off the options menu after the button sound has been played
    public void DisplayOptionsMenu(bool flag)
	{
		if (flag) 
		{
			AudioManager.Instance.PlayOneShot (AudioManager.Instance.LoadClip ("button"));
			panelOptions.ShowOptionsMenu();
		} 
		else
		{
			AudioManager.Instance.PlayOneShot (AudioManager.Instance.LoadClip ("button"));
			panelOptions.HideOptionsMenu();
		}
    }

	public void PlaySoundInPlaylist(int index)
	{
		AudioManager.Instance.PlayOneShot (AudioManager.Instance.Playlist[index]);
	}

    public void QuitGame()
    {
		AudioManager.Instance.PlayOneShot(AudioManager.Instance.LoadClip("button"));
		Exit();
    }

    void Exit()
    {
        //If we are running in a standalone build of the game
        #if UNITY_STANDALONE
        //Quit the application
        Application.Quit();
        #endif

        //If we are running in the editor
        #if UNITY_EDITOR
        //Stop playing the scene
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
