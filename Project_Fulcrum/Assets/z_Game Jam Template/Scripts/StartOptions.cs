using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

public class StartOptions : NetworkBehaviour {

	public bool isMultiplayer = false;
	public FulcrumNetworkManager Mngr;
	public InputField inputField;
	public int sceneToStart = 1;										//Index number in build settings of scene to load if changeScenes is true
	public bool changeScenes;											//If true, load a new scene when Start is pressed, if false, fade out UI and continue in single scene
	public bool changeMusicOnStart;										//Choose whether to continue playing menu music or start a new music clip
	[SerializeField] private GameObject p_MatchSelectButton;
	[SerializeField] private RectTransform o_MatchSelectScrollbar;

	[HideInInspector] public bool inMainMenu = true;					//If true, pause button disabled in main menu (Cancel in input manager, default escape key)
	[HideInInspector] public Animator animColorFade; 					//Reference to animator which will fade to and from black when starting game.
	[HideInInspector] public Animator animMenuAlpha;					//Reference to animator that will fade out alpha of MenuPanel canvas group
	public AnimationClip fadeColorAnimationClip;						//Animation clip fading to color (black default) when changing scenes
	[HideInInspector] public AnimationClip fadeAlphaAnimationClip;		//Animation clip fading out UI elements alpha


	private PlayMusic playMusic;										//Reference to PlayMusic script
	private float fastFadeIn = .01f;									//Very short fade time (10 milliseconds) to start playing music immediately without a click/glitch
	private float slowFadeIn = 10.0f;									
	private ShowPanels showPanels;										//Reference to ShowPanels script on UI GameObject, to show and hide panels

	
	void Awake()
	{
		//Get a reference to ShowPanels attached to UI object
		showPanels = GetComponent<ShowPanels>();

		//Get a reference to the NetworkManager
		Mngr = GameObject.Find("PFGameManager").GetComponent<FulcrumNetworkManager>();

		//Get a reference to PlayMusic attached to UI object
		playMusic = GetComponent<PlayMusic> ();
		Mngr.StartMatchMaker();
	}



	public void OnMatchList(bool b, string s, List<MatchInfoSnapshot> matchBook)
	{
		print(matchBook.Count);
		foreach(Transform oldMatchButton in o_MatchSelectScrollbar.transform)
		{
			GameObject.Destroy(oldMatchButton.gameObject);
		}
		foreach(MatchInfoSnapshot match in matchBook)
		{
			print(match.name+"-("+match.currentSize+"/"+match.maxSize+")");
			GameObject matchSelectButton = (GameObject)Instantiate(p_MatchSelectButton);
			matchSelectButton.transform.SetParent(o_MatchSelectScrollbar);
			matchSelectButton.transform.localScale = new Vector3(1,1,1);
			matchSelectButton.GetComponentInChildren<Text>().text = match.name+"-("+match.currentSize+"/"+match.maxSize+")";
			matchSelectButton.GetComponent<Button>().onClick.AddListener(() => MatchSelectButtonClicked(match));
		}
		Mngr.matches = matchBook;
		//Mngr.OnMatchList(b, s, matchBook);
	}

//	public void OnMatchCreate(bool b, string s, MatchInfo matchInfo)
//	{
//
//	}

	public void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
	{
		if(success)
		{
			Utility.SetAccessTokenForNetwork(matchInfo.networkId, matchInfo.accessToken);
			Mngr.StartClient(matchInfo);
		}
		else
		{
			if (LogFilter.logError) {  Debug.LogError("Join Failed:" + matchInfo); }
		}
	}

	public void MatchSelectButtonClicked(MatchInfoSnapshot match)
	{
		inMainMenu = false;
		showPanels.HideMenu();
		isMultiplayer = true;
		Mngr.matchName = match.name;
		Mngr.matchMaker.JoinMatch(match.networkId, "","","", 0, 0, Mngr.OnMatchJoined);
	}

	public void SingleplayerButtonClicked()
	{

		playMusic.FadeDown(fadeColorAnimationClip.length);
			
		//Use invoke to delay calling of LoadDelayed by half the length of fadeColorAnimationClip
		Invoke("LoadDelayed", fadeColorAnimationClip.length * .5f);

		//Set the trigger of Animator animColorFade to start transition to the FadeToOpaque state.
		animColorFade.SetTrigger("fade");
	}

//	public void SingleplayerButtonClicked()
//	{
//		playMusic.FadeDown(fadeColorAnimationClip.length);
//		ChangeSceneMultiplayer(true);
//		inMainMenu = false;
//		showPanels.HideMenu();
//	}

	public void JoinMatchButtonClicked()
	{
		Mngr.matchMaker.ListMatches(0, 20, "", true, 0, 0, OnMatchList);
	}

	public void LobbyIDSubmitted()
	{
		ChangeSceneMultiplayer(false);
		inMainMenu = false;
		showPanels.HideMenu();
	}

	public void MultiplayerButtonClicked()
	{
	}

	public void ChangeSceneSingleplayer()
	{
		isMultiplayer = false;
		SceneManager.LoadScene("Scenes/MultiplayerTest");
	}

	public void ChangeSceneMultiplayer(bool randomName)
	{
		string matchName;
		if(inputField.text == null||randomName)
		{
			int random = (int)Random.Range(0, 20);
			matchName = "Default#"+random.ToString();
		}
		else
		{
			matchName = inputField.text;
		}
		isMultiplayer = true;
		Mngr.ServerChangeScene("Scenes/MultiplayerTest");
		//SceneManager.LoadScene("Scenes/MultiplayerTest");
		Mngr.matchMaker.CreateMatch(inputField.text, Mngr.matchSize, true, "","","", 0, 0, Mngr.OnMatchCreate);
	}

    void OnEnable()
    {
		SceneManager.sceneLoaded += SceneWasLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneWasLoaded;
    }

   // Once the level has loaded, check if we want to call PlayLevelMusic
    void SceneWasLoaded(Scene scene, LoadSceneMode mode)
    {
		Mngr.gameObject.GetComponent<NetworkManagerHUD>().enabled = false;
		if(scene.name=="MainMenu")
		{
			Mngr.gameObject.GetComponent<TimeManager>().enabled = false;
			Mngr.gameObject.GetComponent<CloudHandler>().enabled = false;
			playMusic.PlaySelectedMusic(0);
			print("MainMenu Loaded.");
			return;
		}
		else
		{
			Mngr.gameObject.GetComponent<CloudHandler>().enabled = true;
			Mngr.gameObject.GetComponent<TimeManager>().enabled = true;
		}
		if(isMultiplayer)
		{
			print("Multiplayer Loaded.");
			Invoke("PlayNewMusic", fadeAlphaAnimationClip.length);
//			ClientScene.Ready(Mngr.client.connection);
//			ClientScene.AddPlayer(0);
		}
		else
		{
			print("Singleplayer Loaded.");
			Invoke("PlayNewMusic", fadeAlphaAnimationClip.length);
			NetworkClient localhost = Mngr.StartHost();
			ClientScene.AddPlayer(localhost.connection, 0);
		}
	}

	public void LoadDelayed()
	{
		//Pause button now works if escape is pressed since we are no longer in Main menu.
		inMainMenu = false;

		//Hide the main menu UI element
		showPanels.HideMenu();

		//Load the selected scene, by scene index number in build settings
		ChangeSceneSingleplayer();
	}

	public void HideDelayed()
	{
		//Hide the main menu UI element after fading out menu for start game in scene
		showPanels.HideMenu();
	}

	public void StartGameInScene()
	{
		//Pause button now works if escape is pressed since we are no longer in Main menu.
		inMainMenu = false;

		//If changeMusicOnStart is true, fade out volume of music group of AudioMixer by calling FadeDown function of PlayMusic, using length of fadeColorAnimationClip as time. 
		//To change fade time, change length of animation "FadeToColor"
		if (changeMusicOnStart) 
		{
			//Wait until game has started, then play new music
			Invoke("PlayNewMusic", fadeAlphaAnimationClip.length);
		}
		//Set trigger for animator to start animation fading out Menu UI
		animMenuAlpha.SetTrigger("fade");
		Invoke("HideDelayed", fadeAlphaAnimationClip.length);
		Debug.Log ("Game started in same scene! Put your game starting stuff here.");
	}


	public void PlayNewMusic()
	{
		//Play music clip assigned to mainMusic in PlayMusic script
		print("playmusic");
		playMusic.FadeUp(fadeColorAnimationClip.length);
		playMusic.PlaySelectedMusic(1);
	}
}
