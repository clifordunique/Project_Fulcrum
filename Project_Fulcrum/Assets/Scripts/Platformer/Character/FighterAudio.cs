using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using AK.Wwise;
public class FighterAudio : NetworkBehaviour {

	[SerializeField]private AK.Wwise.Event e_Step = null;
	[SerializeField]private AK.Wwise.Event e_Pain = null;
	[SerializeField]private AK.Wwise.Event e_ZonPulse = null;
	[SerializeField]private AK.Wwise.Event e_Punch = null;
	[SerializeField]private AK.Wwise.Event e_PunchHit = null;
	[SerializeField]private AK.Wwise.Event e_Landing = null;
	[SerializeField]private AK.Wwise.Event e_Slam = null;
	[SerializeField]private AK.Wwise.Event e_Jump = null;
	[SerializeField]private AK.Wwise.Event e_StrandJump = null;
	[SerializeField]private AK.Wwise.Event e_GuardRoll = null;
	[SerializeField]private AK.Wwise.Event e_SuperJump = null;
	[SerializeField]private AK.Wwise.Event e_EquipSound = null;
	[SerializeField]private AK.Wwise.Event e_CritJump = null;

	[SerializeField]private bool muteFootsteps;
		
	public void StepSound()
	{
		if(muteFootsteps){return;}
		e_Step.Post(this.gameObject);
	}

	public void CritJumpSound()
	{
		e_CritJump.Post(this.gameObject);
	}

	public void ZonPulseSound()
	{
		e_ZonPulse.Post(this.gameObject);
	}

	public void PainSound()
	{
		e_Pain.Post(this.gameObject);
	}

	public void PunchSound()
	{
		e_Punch.Post(this.gameObject);
	}

	public void PunchHitSound()
	{
		e_PunchHit.Post(this.gameObject);
	}

	public void EquipSound()
	{
		e_EquipSound.Post(this.gameObject);
	}

	public void LandingSound(float impactGForce)
	{
		e_Landing.Post(this.gameObject);
	}

	public void SlamSound(float impactGForce, float minT, float maxT)
	{
		e_Slam.Post(this.gameObject);
	}

	public void JumpSound()
	{
		//if(!isLocalPlayer){return;}
		e_Jump.Post(this.gameObject);
	}

	public void SuperJumpSound()
	{
		//if(!isLocalPlayer){return;}
		e_SuperJump.Post(this.gameObject);
	}

	public void StrandJumpSound()
	{
		//if(!isLocalPlayer){return;}
		e_StrandJump.Post(this.gameObject);
	}

	public void GuardRollSound()
	{
		//if(!isLocalPlayer){return;}
		e_GuardRoll.Post(this.gameObject);
	}

//	private void modulateWind()
//	{
//		float windVolume = 0;
//		if(theCharacter.GetSpeed() > windMinT)
//		{
//			if(theCharacter.GetSpeed() < windMaxT)
//			{
//				windVolume = (theCharacter.GetSpeed()-windMinT)/(windMaxT-windMinT);
//
//				windVolume *= windVolM;
//			}
//			else
//			{
//				windVolume = windVolM;
//			}
//		}
//		destWindIntensity = windVolume;
//		if(curWindIntensity>destWindIntensity)
//		{
//			curWindIntensity = Mathf.Lerp(curWindIntensity, destWindIntensity, 0.05f);
//		}
//		else
//		{
//			curWindIntensity = destWindIntensity;
//		}
//		//windSource.volume = curWindIntensity;
//		//CmdWindSound(windVolume);
//		//windSource.pitch = 1 + windVolume*0.2f;
//	}
//
//	private void modulateSlide()
//	{
//		float slideVolume = 0;
//		if((theCharacter.GetSpeed()>slideMinT) && (theCharacter.isSliding()))
//		{
//			if(theCharacter.GetSpeed()<slideMaxT)
//			{
//				slideVolume = (theCharacter.GetSpeed()-slideMinT)/(slideMaxT-slideMinT);
//
//				slideVolume *= slideVolM;
//			}
//			else
//			{
//				slideVolume = slideVolM;
//			}
//		}
//		else
//		{
//			slideVolume = 0;
//		}
//		//slideSource.volume = slideVolume;
//		//CmdslideSound(slideVolume);
//		//slideSource.pitch = 1 + slideVolume*0.2f;
//	}
}
