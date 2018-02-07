using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AK.Wwise;

public class MenuAudio : MonoBehaviour {

	public void HighlightSound()
	{
		AkSoundEngine.PostEvent("Menu_Highlight", gameObject);
	}
	public void SelectSound()
	{
		AkSoundEngine.PostEvent("Menu_Select", gameObject);
	}
	public void PauseSound()
	{
		AkSoundEngine.PostEvent("Menu_Pause", gameObject);
	}
}
