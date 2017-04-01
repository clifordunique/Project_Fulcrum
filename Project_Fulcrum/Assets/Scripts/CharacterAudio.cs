using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAudio : MonoBehaviour {

	private AudioSource charAudioSource;
	[SerializeField]private AudioClip[] jumpSounds;
	[SerializeField]private AudioClip[] footstepSounds;
	[SerializeField]private AudioClip[] landingSounds;
	[SerializeField]private PlatformerCharacter2D theCharacter;
	[SerializeField][Range(0,1f)]private float volumeM;

	[SerializeField]private bool muteFootsteps;

	// Use this for initialization
	void Start () 
	{
		charAudioSource = this.gameObject.GetComponent<AudioSource>();
	}

	public void StepSound()
	{
		if(muteFootsteps){return;}
		float speed = theCharacter.GetSpeed();
		float volume = volumeM;
		if(speed <= 30)
		{
			volume = volumeM*(speed/30);
		}
		int whichSound = (int)Random.Range(0,4);
		//volume *= volumeM;
		charAudioSource.PlayOneShot(footstepSounds[1], volume);
	}

	public void LandingSound(float impactGForce)
	{
		int whichSound = 1;
		float volume = volumeM;
		if(impactGForce <= 30)
		{
			volume = volumeM*(impactGForce/30);
		}

		if(impactGForce > 100)
		{
			whichSound = 2;
		}
	
		//int whichSound = (int)Random.Range(0,4);
		//print(whichSound);
		volume *= volumeM;
		charAudioSource.PlayOneShot(landingSounds[whichSound], volume);
	}

	public void JumpSound()
	{
		charAudioSource.PlayOneShot(jumpSounds[0], volumeM*0.45f);
	}


	// Update is called once per frame
	void Update () 
	{
		
	}
}
