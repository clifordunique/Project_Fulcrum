using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimAudio : MonoBehaviour {

	private GameObject thePlayer;

	// Use this for initialization
	void Start () 
	{
		thePlayer = this.transform.parent.gameObject;
	}

	public void StepSound()
	{
		AkSoundEngine.PostEvent("Footstep", thePlayer);
	}
}
