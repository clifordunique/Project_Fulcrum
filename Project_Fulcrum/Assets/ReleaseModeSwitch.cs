﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReleaseModeSwitch : MonoBehaviour {

	[SerializeField]private GameObject[] ReleaseOnlyObjects;
	[SerializeField]private GameObject[] EditorOnlyObjects;
	[SerializeField][ReadOnlyAttribute]private NavMaster theNavMaster;
	[Tooltip("If true, disables both editor and release modes and runs the scene as-is.")]
	[SerializeField] private bool disable; 

	// Use this for initialization
	void Awake() 
	{
		if(disable)
		{
			return;
		}
		theNavMaster = GameObject.Find("NavMaster").GetComponent<NavMaster>();
		if(Application.isEditor)
		{
			print("STARTING GAME IN EDITOR!");
			foreach(GameObject g in ReleaseOnlyObjects)
			{
				if(g!=null)
				{
					g.SetActive(false);
				}
			}
			foreach(GameObject g in EditorOnlyObjects)
			{
				if(g!=null)
				{
					g.SetActive(true);
				}
			}
		}
		else
		{
			print("STARTING GAME IN ALPHA CLIENT!");
			theNavMaster.setAllVisible = false;
			theNavMaster.UpdateAllSurfaces();
			foreach(GameObject g in ReleaseOnlyObjects)
			{
				g.SetActive(true);
			}
			foreach(GameObject g in EditorOnlyObjects)
			{
				g.SetActive(false);
			}
		}
	}
}