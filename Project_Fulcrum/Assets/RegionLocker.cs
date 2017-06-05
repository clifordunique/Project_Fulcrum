using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RegionLocker : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
	{
		NetworkManager networkManager = FindObjectOfType<NetworkManager>();
		networkManager.SetMatchHost("us1-mm.unet.unity3d.com", networkManager.matchPort, true);
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
}


//
//Vector2 distanceTravelled = Vector3.zero;
//Vector2 finalPos = new Vector2(this.transform.position.x+m_RemainingMovement.x, this.transform.position.y+m_RemainingMovement.y);
//this.transform.position = finalPos;
//UpdateContactNormals(true);
//
//Vector2 initialVel = m_Vel;
//
//
//m_Impact = false;
//m_Landing = false;
//m_Kneeling = false;
//g_ZonStance = -1;
//
////print("Initial Pos: " + startingPos);
////print("Initial Vel: " +  m_Vel);
//
//if(m_Grounded)
//{//Locomotion!
//	Traction(CtrlH);
//}
//else if(m_RightWalled)
//{//Wallsliding!
//	WallTraction(CtrlH,m_RightNormal);
//}
//else if(m_LeftWalled)
//{
//	WallTraction(CtrlH,m_LeftNormal);
//}
//else if(!noGravity)
//{//Gravity!
//	m_Vel = new Vector2 (m_Vel.x, m_Vel.y - 1);
//	m_Ceilinged = false;
//}
//
//
//errorDetectingRecursionCount = 0; //Used for Collizion();
//
////print("Velocity before Coll ision: "+m_Vel);
////print("Position before Coll ision: "+this.transform.position);
//
//m_RemainingVelM = 1f;
//m_RemainingMovement = m_Vel*Time.fixedDeltaTime;
//Vector2 startingPos = this.transform.position;
//
////print("m_RemainingMovement before collision: "+m_RemainingMovement);
//
//Collision();
//
////print("Per frame velocity at end of Collizion() "+m_Vel*Time.fixedDeltaTime);
////print("Velocity at end of Collizion() "+m_Vel);
////print("Per frame velocity at end of updatecontactnormals "+m_Vel*Time.fixedDeltaTime);
////print("m_RemainingMovement after collision: "+m_RemainingMovement);
//
//distanceTravelled = new Vector2(this.transform.position.x-startingPos.x,this.transform.position.y-startingPos.y);
////print("distanceTravelled: "+distanceTravelled);
////print("m_RemainingMovement: "+m_RemainingMovement);
////print("m_RemainingMovement after removing distancetravelled: "+m_RemainingMovement);
//
//if(initialVel.magnitude>0)
//{
//	m_RemainingVelM = (((initialVel.magnitude*Time.fixedDeltaTime)-distanceTravelled.magnitude)/(initialVel.magnitude*Time.fixedDeltaTime));
//}
//else
//{
//	m_RemainingVelM = 1f;
//}
//
////print("m_RemainingVelM: "+m_RemainingVelM);
////print("movement after distance travelled: "+m_RemainingMovement);
////print("Speed this frame: "+m_Vel.magnitude);
//
//m_RemainingMovement = m_Vel*m_RemainingVelM*Time.fixedDeltaTime;
//
////print("Corrected remaining movement: "+m_RemainingMovement);
//
//m_Spd = m_Vel.magnitude;
//
//Vector2 deltaV = m_Vel-initialVel;
//m_IGF = deltaV.magnitude;
//m_CGF += m_IGF;
//if(m_CGF>=1){m_CGF --;}
//if(m_CGF>=10){m_CGF -= (m_CGF/10);}
//
////if(m_CGF>=200)
////{
////	//m_CGF = 0f;
////	print("m_CGF over limit!!");	
////}
//
//if(m_Impact)
//{
//	if(m_IGF >= m_CraterT)
//	{
//		Crater();
//	}
//	else if(m_IGF >= m_SlamT)
//	{
//		Slam();
//	}
//	else
//	{
//		o_FighterAudio.LandingSound(m_IGF);
//	}
//}
//
//if(m_Grounded)
//{
//	this.GetComponent<NetworkTransform>().grounded = true;
//}
//else
//{
//	this.GetComponent<NetworkTransform>().grounded = false;
//}
////print("Per frame velocity at end of physics frame: "+m_Vel*Time.fixedDeltaTime);
////print("m_RemainingMovement at end of physics frame: "+m_RemainingMovement);
////print("Pos at end of physics frame: "+this.transform.position);
////print("##############################################################################################");
////print("FinaL Pos: " + this.transform.position);
////print("FinaL Vel: " + m_Vel);
////print("Speed at end of frame: " + m_Vel.magnitude);
//
////		#region audio
////		if(m_Landing)
////		{
////			o_FighterAudio.LandingSound(m_IGF); // Makes a landing sound when the player hits ground, using the impact force to determine loudness.
////		}
////		#endregion
