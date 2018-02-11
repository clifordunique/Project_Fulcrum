using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using AK.Wwise;
public class FighterAudio : NetworkBehaviour {

	[SerializeField]private AK.Wwise.Event e_Step = null;
	[SerializeField]private AK.Wwise.Event e_ZonPulse = null;
	[SerializeField]private AK.Wwise.Event e_Punch = null;
	[SerializeField]private AK.Wwise.Event e_PunchHit = null;
	[SerializeField]private AK.Wwise.Event e_Landing = null;
	[SerializeField]private AK.Wwise.Event e_Slam = null;
	[SerializeField]private AK.Wwise.Event e_Crater = null;
	[SerializeField]private AK.Wwise.Event e_Jump = null;
	[SerializeField]private AK.Wwise.Event e_StrandJump = null;
	[SerializeField]private AK.Wwise.Event e_SuperJump = null;

//	[SerializeField]private AudioSource charAudioSource;
//	[SerializeField]private AudioSource windSource;
//	[SerializeField]private AudioSource slideSource;
//	[SerializeField]private AudioClip[] jumpSounds;
//	[SerializeField]private AudioClip[] sfxSounds;
//	[SerializeField]private AudioClip[] slideSounds;
//	[SerializeField]private AudioClip[] footstepSounds;
//	[SerializeField]private AudioClip[] landingSounds;
//	[SerializeField]private AudioClip[] punchSounds;
//	[SerializeField]private AudioClip[] punchHitSounds;
//	[SerializeField]private AudioClip 	windSound;
	[SerializeField][ReadOnlyAttribute] private FighterChar theCharacter;
	[SerializeField][Range(0,1f)]private float jumpVolM;		// Jump volume
	[SerializeField][Range(0,1f)]private float strandJumpVolM;	// Strand jump volume
	[SerializeField][Range(0,1f)]private float landVolM;		// Normal landing volume
	[SerializeField][Range(0,1f)]private float slamVolM;		// Slam volume
	[SerializeField][Range(0,1f)]private float crtrVolM;		// Crater volume
	[SerializeField][Range(0,1f)]private float windVolM;		// Wind volume
	[SerializeField][Range(0,1f)]private float slideVolM;		// Slide volume
	[SerializeField][Range(0,1f)]private float stepVolM;		// Footstep volume
	[SerializeField][Range(0,1f)]private float punchVolM;		// Punch volume
	[SerializeField][Range(0,1f)]private float punchHitVolM;	// Punch hit volume
	[SerializeField][Range(0,1f)]private float sfxVolM;			// Special effects volume
	[Space(10)]												    
	[SerializeField][Range(100f,500f)]private float windMinT;						// Speed at which wind sound becomes audible
	[SerializeField][Range(100f,500f)]private float windMaxT;						// Speed at which wind sound is loudest
	[SerializeField][ReadOnlyAttribute] private float curWindIntensity = 0;			// Wind loudness based on speed. Lerps toward destWindIntensity		
	[SerializeField][ReadOnlyAttribute] private float destWindIntensity = 0;		// Goal wind loudness

	[SerializeField][Range(0f,20f)]private float slideMinT;						// Speed at which slide sound becomes audible
	[SerializeField][Range(20f,100f)]private float slideMaxT;						// Speed at which slide sound is loudest

	[SerializeField]private bool muteFootsteps;

	// Use this for initialization
	void Start() 
	{
		theCharacter = this.gameObject.GetComponent<FighterChar>();
	}


	public void StepSound()
	{
		if(muteFootsteps){return;}
		float speed = theCharacter.GetSpeed();
		float volume = stepVolM;
		if(speed <= 30)
		{
			volume = stepVolM*(speed/30);
		}
//		if(theCharacter.g_IsInGrass > 0)
//		{
//			int soundIndex = (int)Random.Range(4,7);
//			//charAudioSource.PlayOneShot(footstepSounds[soundIndex], volume/4);
//		}
//		else
//		{
//			//charAudioSource.PlayOneShot(footstepSounds[1], volume);
//		}

		e_Step.Post(this.gameObject);
	}

	public void ZonPulseSound()
	{
		float volume = sfxVolM;
		//charAudioSource.PlayOneShot(sfxSounds[0], volume);
		e_ZonPulse.Post(this.gameObject);
	}

	public void PunchSound()
	{
		//print("Punchsound played locally");
		int whichSound = 0;
		float volume = punchVolM;
		whichSound = (int)Random.Range(0,4);
		//charAudioSource.PlayOneShot(punchSounds[whichSound], volume);
		e_Punch.Post(this.gameObject);
		//CmdPunchSound(volume);
	}

	public void PunchHitSound()
	{
		//print("PunchHitsound played locally");

		int whichSound = 0;
		float volume = punchHitVolM;
		whichSound = (int)Random.Range(0,4);
		//charAudioSource.PlayOneShot(punchHitSounds[whichSound], volume);
		e_PunchHit.Post(this.gameObject);
	}

	//	[Command] public void CmdPunchSound(float theVolume)
	//	{
	//		RpcPunchSound(theVolume);
	//	}
	//	[ClientRpc] public void RpcPunchSound(float theVolume)
	//	{
	//		//print("RPCSTEPSOUND ACTIVATED");
	//		//if(isLocalPlayer){return;}
	//		if(isLocalPlayer)
	//		{
	//			return;
	//		}
	//		if(isClient)
	//		{
	//			int whichSound = 0;
	//			float volume = punchVolM;
	//			whichSound = (int)Random.Range(0,4);
	//			charAudioSource.PlayOneShot(punchSounds[whichSound], theVolume);
	//		}
	//	}


	public void LandingSound(float impactGForce)
	{
		int whichSound = 1;
		float volume = landVolM;
		if(impactGForce <= 30)
		{
			volume = landVolM*(impactGForce/30);
		}
//		if(theCharacter.g_IsInGrass > 0)
//		{
//			int soundIndex = (int)Random.Range(4,7);
//			//charAudioSource.PlayOneShot(footstepSounds[soundIndex], volume);
//		}
//		else
//		{
//			//charAudioSource.PlayOneShot(landingSounds[whichSound], volume);
//		}
		e_Landing.Post(this.gameObject);
		//CmdLandingSound(volume);
	}

	public void SlamSound(float impactGForce, float minT, float maxT)
	{
		//if(!isLocalPlayer){return;}
		float volume = slamVolM;
		//if(impactGForce <= 150)
		//{
		//	volume = slamVolM*(impactGForce/150);
		//}

		volume = slamVolM+((slamVolM/10)*((impactGForce-minT)/(maxT-minT)));
		//CmdSlamSound(volume);
		//charAudioSource.PlayOneShot(landingSounds[2], volume);
		e_Slam.Post(this.gameObject);
	}

	public void CraterSound(float impactGForce, float minT, float maxT)
	{
		//if(!isLocalPlayer){return;}
		float volume = crtrVolM;
		volume = crtrVolM+((crtrVolM/10)*((impactGForce-minT)/(maxT-minT)));
		//		print("Volume: "+volume);
		//		print("impactGForce: "+impactGForce);
		//		print("minT: "+minT);
		//		print("maxT: "+maxT);
		//		print("(impactGForce-minT)/(maxT-minT)="+(impactGForce-minT)/(maxT-minT));
		//charAudioSource.PlayOneShot(landingSounds[4], volume);
		e_Crater.Post(this.gameObject);
		//CmdCraterSound(volume);
	}

	public void JumpSound()
	{
		if(!isLocalPlayer){return;}
		//charAudioSource.PlayOneShot(jumpSounds[0], jumpVolM);
		//CmdJumpSound(jumpVolM);
		e_Jump.Post(this.gameObject);
	}

	public void SuperJumpSound()
	{
		if(!isLocalPlayer){return;}
		//charAudioSource.PlayOneShot(jumpSounds[0], jumpVolM);
		//CmdJumpSound(jumpVolM);
		e_SuperJump.Post(this.gameObject);
	}


//	[Command] public void CmdJumpSound(float volume)
//	{
//		RpcJumpSound(volume);
//	}
//	[ClientRpc] public void RpcJumpSound(float volume)
//	{
//		if(isLocalPlayer){return;}
//		//charAudioSource.PlayOneShot(jumpSounds[0], volume);
//	}
//


	public void StrandJumpSound()
	{
		if(!isLocalPlayer){return;}
		//charAudioSource.PlayOneShot(jumpSounds[1], strandJumpVolM);
		//CmdStrandJumpSound(strandJumpVolM);
		e_StrandJump.Post(this.gameObject);
	}
//	[Command] public void CmdStrandJumpSound(float volume)
//	{
//		RpcStrandJumpSound(volume);
//	}
//	[ClientRpc] public void RpcStrandJumpSound(float volume)
//	{
//		if(isLocalPlayer){return;}
//		//charAudioSource.PlayOneShot(jumpSounds[1], volume);
//	}
//
	// Update is called once per frame
	void Update() 
	{
		modulateWind();
		modulateSlide();
	}

	private void modulateWind()
	{
		float windVolume = 0;
		if(theCharacter.GetSpeed() > windMinT)
		{
			if(theCharacter.GetSpeed() < windMaxT)
			{
				windVolume = (theCharacter.GetSpeed()-windMinT)/(windMaxT-windMinT);

				windVolume *= windVolM;
			}
			else
			{
				windVolume = windVolM;
			}
		}
		destWindIntensity = windVolume;
		if(curWindIntensity>destWindIntensity)
		{
			curWindIntensity = Mathf.Lerp(curWindIntensity, destWindIntensity, 0.05f);
		}
		else
		{
			curWindIntensity = destWindIntensity;
		}
		//windSource.volume = curWindIntensity;
		//CmdWindSound(windVolume);
		//windSource.pitch = 1 + windVolume*0.2f;
	}

	private void modulateSlide()
	{
		float slideVolume = 0;
		if((theCharacter.GetSpeed()>slideMinT) && (theCharacter.isSliding()))
		{
			if(theCharacter.GetSpeed()<slideMaxT)
			{
				slideVolume = (theCharacter.GetSpeed()-slideMinT)/(slideMaxT-slideMinT);

				slideVolume *= slideVolM;
			}
			else
			{
				slideVolume = slideVolM;
			}
		}
		else
		{
			slideVolume = 0;
		}
		//slideSource.volume = slideVolume;
		//CmdslideSound(slideVolume);
		//slideSource.pitch = 1 + slideVolume*0.2f;
	}
}
