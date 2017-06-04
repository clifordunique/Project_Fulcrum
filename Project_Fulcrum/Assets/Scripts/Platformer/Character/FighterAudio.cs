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
		if(!isLocalPlayer){return;}
		if(muteFootsteps){return;}
		float speed = theCharacter.GetSpeed();
		float volume = stepVolM;
		if(speed <= 30)
		{
			volume = stepVolM*(speed/30);
		}
		//int whichSound = (int)Random.Range(0,4);
		//volume *= volumeM;
		charAudioSource.PlayOneShot(footstepSounds[1], volume);
		CmdStepSound(volume);
	}
	[Command] public void CmdStepSound(float vol)
	{
		RpcStepSound(vol);
	}
	[ClientRpc] public void RpcStepSound(float vol)
	{
		if(isLocalPlayer){return;}
		print("EXECUTING RPCSTEPSOUND");
		if(muteFootsteps){return;}
		float volume = vol;
		//int whichSound = (int)Random.Range(0,4);
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
		charAudioSource.PlayOneShot(landingSounds[whichSound], volume);
		CmdLandingSound(volume);
	}
	[Command] public void CmdLandingSound(float volume)
	{
		RpcLandingSound(volume);
	}
	[ClientRpc] public void RpcLandingSound(float volume)
	{
		if(isLocalPlayer){return;}
		charAudioSource.PlayOneShot(landingSounds[1], volume);
	}


	public void SlamSound(float impactGForce, float minT, float maxT)
	{
		if(!isLocalPlayer){return;}
		float volume = slamVolM;
		//if(impactGForce <= 150)
		//{
		//	volume = slamVolM*(impactGForce/150);
		//}

		volume = slamVolM+((slamVolM/10)*((impactGForce-minT)/(maxT-minT)));
		CmdSlamSound(volume);
		charAudioSource.PlayOneShot(landingSounds[2], volume);
	}
	[Command] public void CmdSlamSound(float volume)
	{
		RpcSlamSound(volume);
	}
	[ClientRpc] public void RpcSlamSound(float volume)
	{
		if(isLocalPlayer){return;}
		charAudioSource.PlayOneShot(landingSounds[2], volume);
	}

	public void CraterSound(float impactGForce, float minT, float maxT)
	{
		if(!isLocalPlayer){return;}
		float volume = crtrVolM;
		volume = crtrVolM+((crtrVolM/10)*((impactGForce-minT)/(maxT-minT)));
//		print("Volume: "+volume);
//		print("impactGForce: "+impactGForce);
//		print("minT: "+minT);
//		print("maxT: "+maxT);
//		print("(impactGForce-minT)/(maxT-minT)="+(impactGForce-minT)/(maxT-minT));
		charAudioSource.PlayOneShot(landingSounds[4], volume);
		CmdCraterSound(volume);
	}
	[Command] public void CmdCraterSound(float volume)
	{
		RpcCraterSound(volume);
	}
	[ClientRpc] public void RpcCraterSound(float volume)
	{
		charAudioSource.PlayOneShot(landingSounds[4], volume);
	}


	public void JumpSound()
	{
		if(!isLocalPlayer){return;}
		charAudioSource.PlayOneShot(jumpSounds[0], jumpVolM);
		CmdJumpSound(jumpVolM);
	}
	[Command] public void CmdJumpSound(float volume)
	{
		RpcJumpSound(volume);
	}
	[ClientRpc] public void RpcJumpSound(float volume)
	{
		if(isLocalPlayer){return;}
		charAudioSource.PlayOneShot(jumpSounds[0], volume);
	}

	[Command] public void CmdWindSound(float volume)
	{
		RpcWindSound(volume);
	}
	[ClientRpc] public void RpcWindSound(float volume)
	{
		if(isLocalPlayer){return;}
		windSource.volume = volume;
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
		//CmdWindSound(windVolume);
		//windSource.pitch = 1 + windVolume*0.2f;
	}
}
