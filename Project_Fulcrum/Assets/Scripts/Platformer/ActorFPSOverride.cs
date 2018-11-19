using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GAF.Core;



public class ActorFPSOverride : MonoBehaviour {

	public bool is60fps = true;
	public bool sortingOverride = true;

	// Use this for initialization
	void Awake() 
	{
		if(sortingOverride)
		{
			Renderer[] allGAFRenderers = this.transform.GetComponentsInChildren<Renderer>();
			foreach(Renderer r in allGAFRenderers)
			{
				r.sortingLayerName = "Actors";
			}
		}
		if(is60fps)
		{
			Animator[] allSubAnimators = this.transform.GetComponentsInChildren<Animator>();
			foreach(Animator a in allSubAnimators)
			{
				if(a!=null)
				{
					a.speed = 2;
				}
			}
		}
	}
}
