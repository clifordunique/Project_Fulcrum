using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.UI;
using AK.Wwise;

public class SetAudioLevels : MonoBehaviour {

	//Call this function and pass in the float parameter musicLvl to set the volume of the AudioMixerGroup Music in mainMixer
	public void SetMusicLevel(float musicLvl)
	{
		AkSoundEngine.SetRTPCValue("Volume_Music", musicLvl);
	}

	//Call this function and pass in the float parameter sfxLevel to set the volume of the AudioMixerGroup SoundFx in mainMixer
	public void SetSfxLevel(float sfxLevel)
	{
		AkSoundEngine.SetRTPCValue("Volume_Effects", sfxLevel);
	}
}
