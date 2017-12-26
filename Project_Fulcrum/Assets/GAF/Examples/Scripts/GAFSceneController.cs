
// File:			GAFSceneController.cs
// Version:			5.2
// Last changed:	2017/3/28 14:41
// Author:			Nikitin Nikolay, Nikitin Alexey
// Copyright:		© 2017 GAFMedia
// Project:			GAF Unity plugin


using UnityEngine;
using System.Collections;
using GAF.Core;
using GAF.Assets;

public class GAFSceneController : MonoBehaviour 
{
	public GAFMovieClip clip;
	public GAFAnimationAsset[] assets;

	public void RunDemoScene(string _SceneName)
	{
		Application.LoadLevel(_SceneName);
	}

	public void UnloadAssets(string _SceneName)
	{
		Resources.UnloadUnusedAssets();
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Space))
		{
			if (Application.loadedLevelName != "Main")
				Application.LoadLevel("Main");
		}
	}
}
