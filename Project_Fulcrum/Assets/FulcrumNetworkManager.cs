using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FulcrumNetworkManager : NetworkManager {

	public NetworkConnection clientConn;

	public override void OnClientConnect(NetworkConnection conn)
	{
		clientConn = conn;
	}

//	public override void OnClientSceneChanged()
//	{
//		
//	}

//	void OnEnable()
//	{
//		SceneManager.sceneLoaded += SceneWasLoaded;
//	}
//
//	void OnDisable()
//	{
//		SceneManager.sceneLoaded -= SceneWasLoaded;
//	}
//
//	// Once the level has loaded, check if we want to call PlayLevelMusic
//	void SceneWasLoaded(Scene scene, LoadSceneMode mode)
//	{
//		if(scene.name == "MainMenu" ){print("MainMenu Loaded."); return;}
//		if(isMultiplayer)
//		{
//			print("Multiplayer Loaded.");
//			//			ClientScene.Ready(Mngr.client.connection);
//			//			ClientScene.AddPlayer(0);
//		}
//		else
//		{
//			print("Singleplayer Loaded.");
//			NetworkClient localhost = Mngr.StartHost();
//			ClientScene.AddPlayer(localhost.connection, 0);
//		}
//	}
}
