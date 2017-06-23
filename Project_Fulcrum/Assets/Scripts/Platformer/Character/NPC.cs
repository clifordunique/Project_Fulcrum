//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//
//public class NPC : FighterChar {
//	//##########################################################################################################################################################################
//	// AI VARIABLES
//	//###########################################################################################################################################################################
//	[Header("NPC AI:")]
//	[SerializeField]private FighterChar enemyTarget;
//	[SerializeField]private float distanceToTarget;
//	[SerializeField]private Vector2 goalLocation;
//	[SerializeField]private int DecisionState;
//
//
//	//########################################################################################################################################
//	// CORE FUNCTIONS
//	//########################################################################################################################################
//	#region Core Functions
//	protected void Start () 
//	{
//		this.FighterState.FinalPos = this.transform.position;
//	}
//
//	protected override void FixedUpdate ()
//	{
//		FixedUpdateAI();
//		FixedUpdateProcessInput();
//		FixedUpdatePhysics();
//		FixedUpdateLogic();
//		FixedUpdateAnimation();
//		FighterState.RightClick = false;
//		FighterState.LeftClick = false;
//		FighterState.ZonKey = false;
//	}
//	#endregion
//	//###################################################################################################################################
//	// CUSTOM FUNCTIONS
//	//###################################################################################################################################
//	#region Custom Functions
//
//	protected void FixedUpdateAI()
//	{//LayerMask.NameToLayer("IgnoreRaycast")
//		#region Knowledge Gathering
//		#endregion
//		if(enemyTarget)
//		{
//			goalLocation = enemyTarget.GetPosition()-this.GetPosition();
//			distanceToTarget = goalLocation.magnitude;
//			if(goalLocation.x < 0&&distanceToTarget >= 3)
//			{
//				this.FighterState.LeftKey = true;
//				this.FighterState.RightKey = false;
//			}
//			else if(goalLocation.x > 0&&distanceToTarget >= 3)
//			{
//				this.FighterState.RightKey = true;
//				this.FighterState.LeftKey = false;
//			}
//			else
//			{
//				this.FighterState.RightKey = false;
//				this.FighterState.LeftKey = false;
//			}
//		}
//		else
//		{
//			int layerMask = 1 << 13;
//			Collider2D[] NearbyEntities = Physics2D.OverlapCircleAll(this.FighterState.FinalPos, 0.001f, layerMask, -1, 1);
//			foreach(Collider2D i in NearbyEntities)
//			{
//				if(this.GetComponent<Collider2D>() == i){continue;}
//				print("TESTING ENTITY:"+i);
//				FighterChar floob = i.GetComponent<FighterChar>();
//				if(enemyTarget==null&&floob!=null)
//				{
//					print("THE FLOOB IS TRUE!");
//					enemyTarget = floob;
//				}
//			}
//		}
//		#region Decision Making
//		#
//	}
//
//	protected override void Respawn()
//	{
//		FighterState.Dead = false;
//		FighterState.CurHealth = g_MaxHealth;
//		o_Anim.SetBool("Dead", false);
//		o_SpriteRenderer.color = new Color(1,0.6f,0,1);
//	}
//
//	protected override void FixedUpdateProcessInput()
//	{
//		m_Impact = false;
//		m_Landing = false;
//		m_Kneeling = false;
//		g_ZonStance = -1;
//
////		if(FighterState.RightClick&&(FighterState.DevMode))
////		{
////			//GameObject newMarker = (GameObject)Instantiate(o_DebugMarker);
////			//newMarker.name = "DebugMarker";
////			//newMarker.transform.position = FighterState.MouseWorldPos;
////			FighterState.RightClick = false;
////			float Magnitude = 2f;
////			//float Magnitude = 0.5f;
////			float Roughness = 10f;
////			//float FadeOutTime = 0.6f;
////			float FadeOutTime = 5f;
////			float FadeInTime = 0f;
////			//Vector3 RotInfluence = new Vector3(0,0,0);
////			//Vector3 PosInfluence = new Vector3(1,1,0);
////			Vector3 RotInfluence = new Vector3(1,1,1);
////			Vector3 PosInfluence = new Vector3(0.15f,0.15f,0.15f);
////			CameraShaker.Instance.ShakeOnce(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);
////		}	
////
////		if(FighterState.LeftClick&&(FighterState.DevMode||d_ClickToKnockPlayer))
////		{
////			FighterState.Vel += FighterState.PlayerMouseVector*10;
////			//print("Knocking the player.");
////			FighterState.LeftClick = false;
////		}	
////
////		if(i_DevKey1)
////		{
////			if(FighterState.DevMode)
////			{
////				FighterState.DevMode = false;
////			}
////			else
////			{
////				FighterState.DevMode = true;
////			}
////			i_DevKey1 = false;
////		}
////
////
////		if(i_DevKey2)
////		{
////			this.Respawn();
////			i_DevKey2 = false;
////		}
////
////
////		if(i_DevKey3)
////		{
////			i_DevKey3 = false;
////		}
////
////
////		if(i_DevKey4)
////		{
////			FighterState.CurHealth -= 10;
////			i_DevKey4 = false;
////		}
////
//
//
//
//		if(IsDisabled())
//		{
//			FighterState.RightClick = false;
//			FighterState.LeftClick = false;
//			FighterState.UpKey = false;
//			FighterState.LeftKey = false;
//			FighterState.DownKey = false;
//			FighterState.RightKey = false;
//			FighterState.JumpKey = false;
//			FighterState.ZonKey = false;
//		}
//
//		//################################################################################
//		//### ALL INPUT AFTER THIS POINT IS DISABLED WHEN THE PLAYER IS INCAPACITATED. ###
//		//################################################################################
//
//		//FighterState.PlayerMouseVector = FighterState.MouseWorldPos-Vec2(this.transform.position);
//		if(!(FighterState.LeftKey||FighterState.RightKey) || (FighterState.LeftKey && FighterState.RightKey))
//		{
//			//print("BOTH OR NEITHER");
//			if(!(autoRunLeft||autoRunRight))
//			{
//				CtrlH = 0;
//			}
//			else if(autoRunLeft)
//			{
//				CtrlH = -1;
//			}
//			else
//			{
//				CtrlH = 1;
//			}
//		}
//		else if(FighterState.LeftKey)
//		{
//			//print("LEFT");
//			CtrlH = -1;
//		}
//		else
//		{
//			//print("RIGHT");
//			CtrlH = 1;
//		}
//
//		if (CtrlH < 0) 
//		{
//			facingDirection = false; //true means right (the direction), false means left.
//		} 
//		else if (CtrlH > 0)
//		{
//			facingDirection = true; //true means right (the direction), false means left.
//		}
//
//		//print("CTRLH=" + CtrlH);
//		if(FighterState.DownKey&&m_Grounded)
//		{
//			m_Kneeling = true;
//			CtrlH = 0;
//			g_ZonStance = 0; // Kneeling stance.
//		}
//		else
//		{
//			g_ZonJumpCharge=0;
//		}
//
//		if(FighterState.JumpKey&&(m_Grounded||m_Ceilinged||m_LeftWalled||m_RightWalled))
//		{
//			if(m_Kneeling)
//			{
//				//ZonJump(FighterState.PlayerMouseVector.normalized);
//			}
//			else
//			{
//				Jump(CtrlH);
//			}
//		}
//
//		if(FighterState.LeftClickRelease&&!(FighterState.DevMode||d_ClickToKnockPlayer)&&!m_Kneeling)
//		{
//			if(!(FighterState.LeftKey&&(FighterState.PlayerMouseVector.normalized.x>0))&&!(FighterState.RightKey&&(FighterState.PlayerMouseVector.normalized.x<0))) // If trying to run opposite your punch direction, do not punch.
//			{
//				//ThrowPunch(FighterState.PlayerMouseVector.normalized);
//			}
//			//print("Leftclick detected");
//			//g_VelocityPunchExpended = false;
//			FighterState.LeftClickRelease = false;
//		}	
//
////		if(FighterState.LeftClickHold)
////		{
////			//i_LeftClickHoldDuration += Time.fixedDeltaTime;
////			if(g_VelocityPunching)
////			{
////				if(FighterState.Vel.magnitude <= 70||g_VelocityPunchExpended)
////				{
////					g_VelocityPunching = false;
////					o_VelocityPunch.inUse = false;
////					g_VelocityPunchExpended = true;
////				}
////			}
////			else
////			{
////				if((FighterState.Vel.magnitude > 70)&&(!g_VelocityPunchExpended)&&(i_LeftClickHoldDuration>=0.5f)) //If going fast enough and holding click for long enough.
////				{
////					g_VelocityPunching = true;
////					o_VelocityPunch.inUse = true;
////				}
////			}
////		}
////		else
////		{
////			//i_LeftClickHoldDuration = 0;
////			g_VelocityPunching = false;
////			o_VelocityPunch.inUse = false;
////			g_VelocityPunchExpended = false;
////		}
//	}
//
//		
//	#endregion
//}
//
//////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : FighterChar {
	//##########################################################################################################################################################################
	// AI VARIABLES
	//###########################################################################################################################################################################
	[Header("NPC AI:")]
	[SerializeField][ReadOnlyAttribute]private FighterChar enemyTarget; 	// Reference to the enemy combatant the NPC wants to fight.
	[SerializeField][ReadOnlyAttribute]private float distanceToTarget; 		// Distance from enemy target.
	[SerializeField][ReadOnlyAttribute]private float attackRange = 1f;		// Range at which the NPC will stop running and punch.
	[SerializeField][ReadOnlyAttribute]private Vector2 goalLocation;		// Location that the NPC attempts to reach while moving.
	[SerializeField][ReadOnlyAttribute]private int DecisionState;			// Defines what action the NPC is currently trying to do.
	[SerializeField]private float PunchDelay = 1;
	[SerializeField]private float PunchDelayVariance = 0.3f;
	[SerializeField]private float PunchCooldown;

	[SerializeField]public bool d_aiDebug;

	//########################################################################################################################################
	// CORE FUNCTIONS
	//########################################################################################################################################
	#region Core Functions

	protected void Start () 
	{
		this.FighterState.FinalPos = this.transform.position;
	}

	protected override void Awake()
	{
		isAPlayer = false;
		FighterAwake();
	}

	protected override void FixedUpdate ()
	{
		FixedUpdateAI();
		FixedUpdateProcessInput();
		FixedUpdatePhysics();
		FixedUpdateLogic();
		FixedUpdateAnimation();
		FighterState.RightClick = false;
		FighterState.LeftClick = false;
		FighterState.ZonKey = false;
	}
	#endregion
	//###################################################################################################################################
	// CUSTOM FUNCTIONS
	//###################################################################################################################################
	#region Custom Functions

	protected void FixedUpdateAI()
	{
		#region Knowledge Gathering

		if(PunchCooldown > 0)
		{
			PunchCooldown -= Time.fixedDeltaTime;
		}

		if(enemyTarget)
		{
			if(enemyTarget.isAlive())
			{
				goalLocation = enemyTarget.GetPosition()-this.GetPosition();
				distanceToTarget = goalLocation.magnitude;
				if(distanceToTarget <= attackRange)
				{
					DecisionState = 2; // Attacking
				}
				else
				{
					DecisionState = 1; // Pursuing
				}
			}
			else
			{
				enemyTarget = null;
				DecisionState = 0; // Searching
			}
		}
		else
		{
			DecisionState = 0; // Searching
		}
		if(FighterState.Dead)
		{
			DecisionState = -1; // Idle
		}
		#endregion

		#region Decision Making
		switch (DecisionState)
		{
		case 0: // Searching
			{
				if(d_aiDebug){print("searching...");}
				int layerMask = 1 << 13;
				Collider2D[] NearbyEntities = Physics2D.OverlapCircleAll(this.FighterState.FinalPos, 10f, layerMask, -1, 1);
				foreach(Collider2D i in NearbyEntities)
				{
					if(this.GetComponent<Collider2D>() == i){continue;}
					//print("TESTING ENTITY:"+i);
					FighterChar floob = i.GetComponent<FighterChar>();
					if(enemyTarget==null&&floob!=null)
					{
						if(floob.isAlive())
						{
							enemyTarget = floob;
						}
					}
				}
				break;
			}
		case 1: // Pursuing
			{
				if(d_aiDebug){print("Chasing!");}
				if(goalLocation.x < 0)
				{
					this.FighterState.LeftKey = true;
					this.FighterState.RightKey = false;
				}
				else if(goalLocation.x > 0)
				{
					this.FighterState.RightKey = true;
					this.FighterState.LeftKey = false;
				}
				else
				{
					this.FighterState.RightKey = false;
					this.FighterState.LeftKey = false;
				}
				break;
			}
		case 2: // Attacking
			{
				if(d_aiDebug){print("ATTACKING!");}
				this.FighterState.RightKey = false;
				this.FighterState.LeftKey = false;
				if(PunchCooldown <= 0)
				{
					this.FighterState.MouseWorldPos = enemyTarget.GetPosition();
					this.FighterState.LeftClickRelease = true;
					PunchCooldown = PunchDelay + UnityEngine.Random.Range(-PunchDelayVariance,PunchDelayVariance);
				}
				break;
			}
		default: // Idle
			{
				break;
			}
		}
		#endregion
	}

	protected override void Respawn()
	{
		FighterState.Dead = false;
		FighterState.CurHealth = g_MaxHealth;
		o_Anim.SetBool("Dead", false);
		o_SpriteRenderer.color = new Color(1,0.6f,0,1);
	}

	protected override void FixedUpdateProcessInput()
	{
		m_Impact = false;
		m_Landing = false;
		m_Kneeling = false;
		g_ZonStance = -1;

		//		if(FighterState.RightClick&&(FighterState.DevMode))
		//		{
		//			//GameObject newMarker = (GameObject)Instantiate(o_DebugMarker);
		//			//newMarker.name = "DebugMarker";
		//			//newMarker.transform.position = FighterState.MouseWorldPos;
		//			FighterState.RightClick = false;
		//			float Magnitude = 2f;
		//			//float Magnitude = 0.5f;
		//			float Roughness = 10f;
		//			//float FadeOutTime = 0.6f;
		//			float FadeOutTime = 5f;
		//			float FadeInTime = 0f;
		//			//Vector3 RotInfluence = new Vector3(0,0,0);
		//			//Vector3 PosInfluence = new Vector3(1,1,0);
		//			Vector3 RotInfluence = new Vector3(1,1,1);
		//			Vector3 PosInfluence = new Vector3(0.15f,0.15f,0.15f);
		//			CameraShaker.Instance.ShakeOnce(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);
		//		}	
		//
		//		if(FighterState.LeftClick&&(FighterState.DevMode||d_ClickToKnockPlayer))
		//		{
		//			FighterState.Vel += FighterState.PlayerMouseVector*10;
		//			//print("Knocking the player.");
		//			FighterState.LeftClick = false;
		//		}	
		//
		//		if(i_DevKey1)
		//		{
		//			if(FighterState.DevMode)
		//			{
		//				FighterState.DevMode = false;
		//			}
		//			else
		//			{
		//				FighterState.DevMode = true;
		//			}
		//			i_DevKey1 = false;
		//		}
		//
		//
		//		if(i_DevKey2)
		//		{
		//			this.Respawn();
		//			i_DevKey2 = false;
		//		}
		//
		//
		//		if(i_DevKey3)
		//		{
		//			i_DevKey3 = false;
		//		}
		//
		//
		//		if(i_DevKey4)
		//		{
		//			FighterState.CurHealth -= 10;
		//			i_DevKey4 = false;
		//		}
		//



		if(IsDisabled())
		{
			FighterState.RightClick = false;
			FighterState.LeftClick = false;
			FighterState.UpKey = false;
			FighterState.LeftKey = false;
			FighterState.DownKey = false;
			FighterState.RightKey = false;
			FighterState.JumpKey = false;
			FighterState.ZonKey = false;
		}

		//################################################################################
		//### ALL INPUT AFTER THIS POINT IS DISABLED WHEN THE PLAYER IS INCAPACITATED. ###
		//################################################################################

		FighterState.PlayerMouseVector = FighterState.MouseWorldPos-Vec2(this.transform.position);
		if(!(FighterState.LeftKey||FighterState.RightKey) || (FighterState.LeftKey && FighterState.RightKey))
		{
			//print("BOTH OR NEITHER");
			if(!(autoRunLeft||autoRunRight))
			{
				CtrlH = 0;
			}
			else if(autoRunLeft)
			{
				CtrlH = -1;
			}
			else
			{
				CtrlH = 1;
			}
		}
		else if(FighterState.LeftKey)
		{
			//print("LEFT");
			CtrlH = -1;
		}
		else
		{
			//print("RIGHT");
			CtrlH = 1;
		}

		if (CtrlH < 0) 
		{
			facingDirection = false; //true means right (the direction), false means left.
		} 
		else if (CtrlH > 0)
		{
			facingDirection = true; //true means right (the direction), false means left.
		}

		//print("CTRLH=" + CtrlH);
		if(FighterState.DownKey&&m_Grounded)
		{
			m_Kneeling = true;
			CtrlH = 0;
			g_ZonStance = 0; // Kneeling stance.
		}
		else
		{
			g_ZonJumpCharge=0;
		}

		if(FighterState.JumpKey&&(m_Grounded||m_Ceilinged||m_LeftWalled||m_RightWalled))
		{
			if(m_Kneeling)
			{
				//ZonJump(FighterState.PlayerMouseVector.normalized);
			}
			else
			{
				Jump(CtrlH);
			}
		}

		if(FighterState.LeftClickRelease&&!(FighterState.DevMode||d_ClickToKnockPlayer)&&!m_Kneeling)
		{
			if(!(FighterState.LeftKey&&(FighterState.PlayerMouseVector.normalized.x>0))&&!(FighterState.RightKey&&(FighterState.PlayerMouseVector.normalized.x<0))) // If trying to run opposite your punch direction, do not punch.
			{
				ThrowPunch(FighterState.PlayerMouseVector.normalized);
			}
			//print("Leftclick detected");
			//g_VelocityPunchExpended = false;
			FighterState.LeftClickRelease = false;
		}	

		//		if(FighterState.LeftClickHold)
		//		{
		//			//i_LeftClickHoldDuration += Time.fixedDeltaTime;
		//			if(g_VelocityPunching)
		//			{
		//				if(FighterState.Vel.magnitude <= 70||g_VelocityPunchExpended)
		//				{
		//					g_VelocityPunching = false;
		//					o_VelocityPunch.inUse = false;
		//					g_VelocityPunchExpended = true;
		//				}
		//			}
		//			else
		//			{
		//				if((FighterState.Vel.magnitude > 70)&&(!g_VelocityPunchExpended)&&(i_LeftClickHoldDuration>=0.5f)) //If going fast enough and holding click for long enough.
		//				{
		//					g_VelocityPunching = true;
		//					o_VelocityPunch.inUse = true;
		//				}
		//			}
		//		}
		//		else
		//		{
		//			//i_LeftClickHoldDuration = 0;
		//			g_VelocityPunching = false;
		//			o_VelocityPunch.inUse = false;
		//			g_VelocityPunchExpended = false;
		//		}
	}


	#endregion
}
