using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FighterAudio : NetworkBehaviour {

	[SerializeField]private AudioSource charAudioSource;
	[SerializeField]private AudioSource windSource;
	[SerializeField]private AudioClip[] jumpSounds;
	[SerializeField]private AudioClip[] footstepSounds;
	[SerializeField]private AudioClip[] landingSounds;
	[SerializeField]private AudioClip 	windSound;
	[SerializeField]private FighterChar theCharacter;
	[SerializeField][Range(0,1f)]private float jumpVolM;		// Jump volume
	[SerializeField][Range(0,1f)]private float landVolM;		// Normal landing volume
	[SerializeField][Range(0,1f)]private float slamVolM;		// Slam volume
	[SerializeField][Range(0,1f)]private float crtrVolM;		// Crater volume
	[SerializeField][Range(0,1f)]private float windVolM;		// Wind volume
	[SerializeField][Range(0,1f)]private float stepVolM;		// Footstep volume
	[Space(10)]												    
	[SerializeField][Range(100f,500f)]private float windMinT;		// Speed at which wind sound becomes audible
	[SerializeField][Range(100f,500f)]private float windMaxT;		// Speed at which wind sound is loudest



	[SerializeField]private bool muteFootsteps;

	// Use this for initialization
	void Start () 
	{
		//charAudioSource = this.gameObject.GetComponent<AudioSource>();
	}


	public void StepSound()
	{
		print("EXECUTING STEPSOUND");
		CmdStepSound();
		//RpcStepSound();
//		if(muteFootsteps){return;}
//		float speed = theCharacter.GetSpeed();
//		float volume = stepVolM;
//		if(speed <= 30)
//		{
//			volume = stepVolM*(speed/30);
//		}
//		int whichSound = (int)Random.Range(0,4);
//		//volume *= volumeM;
//		charAudioSource.PlayOneShot(footstepSounds[1], volume);
	}

	[Command] public void CmdStepSound()
	{
		print("EXECUTING CMDSTEPSOUND");
		if(muteFootsteps){return;}
		float speed = theCharacter.GetSpeed();
		float volume = stepVolM;
		if(speed <= 30)
		{
			volume = stepVolM*(speed/30);
		}
		int whichSound = (int)Random.Range(0,4);
		//volume *= volumeM;
		charAudioSource.PlayOneShot(footstepSounds[1], volume);
	}

	[ClientRpc] public void RpcStepSound()
	{
		print("EXECUTING RPCSTEPSOUND");
		if(muteFootsteps){return;}
		float speed = theCharacter.GetSpeed();
		float volume = stepVolM;
		if(speed <= 30)
		{
			volume = stepVolM*(speed/30);
		}
		int whichSound = (int)Random.Range(0,4);
		//volume *= volumeM;
		charAudioSource.PlayOneShot(footstepSounds[1], volume);
	}

	public void LandingSound(float impactGForce)
	{
		int whichSound = 1;
		float volume = landVolM;
		if(impactGForce <= 30)
		{
			volume = landVolM*(impactGForce/30);
		}

		//		if(impactGForce > 100)
		//		{
		//			volume = slamVolM;
		//			whichSound = 2;
		//		}
		//
		//		if(impactGForce > 200)
		//		{
		//			volume = crtrVolM;
		//			whichSound = 4;
		//		}

		//int whichSound = (int)Random.Range(0,4);
		//print(whichSound);
		//volume *= volumeM;
		charAudioSource.PlayOneShot(landingSounds[whichSound], volume);
	}


	public void SlamSound(float impactGForce, float minT, float maxT)
	{
		float volume = slamVolM;
		//if(impactGForce <= 150)
		//{
		//	volume = slamVolM*(impactGForce/150);
		//}

		volume = slamVolM+((slamVolM/10)*((impactGForce-minT)/(maxT-minT)));
		charAudioSource.PlayOneShot(landingSounds[2], volume);
	}

	public void CraterSound(float impactGForce, float minT, float maxT)
	{
		float volume = crtrVolM;
		//if(impactGForce <= 250)
		//{
		//	volume = crtrVolM*(impactGForce/250);
		//}
		volume = crtrVolM+((crtrVolM/10)*((impactGForce-minT)/(maxT-minT)));
		print("Volume: "+volume);
		print("impactGForce: "+impactGForce);
		print("minT: "+minT);
		print("maxT: "+maxT);
		print("(impactGForce-minT)/(maxT-minT)="+(impactGForce-minT)/(maxT-minT));
		charAudioSource.PlayOneShot(landingSounds[4], volume);
	}

	public void JumpSound()
	{
		charAudioSource.PlayOneShot(jumpSounds[0], jumpVolM);
	}


	// Update is called once per frame
	void Update() 
	{
		float windVolume = 0;
		if(theCharacter.m_Spd > windMinT)
		{
			if(theCharacter.m_Spd < windMaxT)
			{
				windVolume = (theCharacter.m_Spd-windMinT)/(windMaxT-windMinT);
				windVolume *= 1f;
			}
			else
			{
				windVolume = 1f;
			}
		}
		windSource.volume = windVolume;
		//windSource.pitch = 1 + windVolume*0.2f;
	}
}
