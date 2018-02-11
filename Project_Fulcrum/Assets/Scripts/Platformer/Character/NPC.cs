
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : FighterChar {
	//##########################################################################################################################################################################
	// PLAYER INPUT VARIABLES
	//###########################################################################################################################################################################
	#region PLAYERINPUT
	[SerializeField][ReadOnlyAttribute]public float i_LeftClickHoldDuration;
	#endregion
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

	[SerializeField]private bool d_Think; 									// When false, AI brain is shut down. Fighter will just stand there.
	[SerializeField]public bool d_aiDebug;									// When true, enables ai debug messaging.

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
		if(k_IsKinematic)
		{
			FixedUpdateKinematic();	
		}
		else
		{
			FixedUpdateAI();
			FixedUpdateProcessInput();
			FixedUpdatePhysics();
		}
		FixedUpdateLogic();
		FixedUpdateAnimation();
		FighterState.RightClickPress = false;
		FighterState.LeftClickPress = false;
		FighterState.ZonKeyPress = false;
	}
	#endregion
	//###################################################################################################################################
	// CUSTOM FUNCTIONS
	//###################################################################################################################################
	#region Custom Functions

	protected void FixedUpdateAI()
	{
		if(!d_Think){return;}

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
					this.FighterState.LeftKeyHold = true;
					this.FighterState.RightKeyHold = false;
				}
				else if(goalLocation.x > 0)
				{
					this.FighterState.RightKeyHold = true;
					this.FighterState.LeftKeyHold = false;
				}
				else
				{
					this.FighterState.RightKeyHold = false;
					this.FighterState.LeftKeyHold = false;
				}

				if(goalLocation.y > 5f)
				{
					this.FighterState.JumpKeyPress = true;
				}
				else
				{
					this.FighterState.JumpKeyPress = false;
				}
				if(this.GetSpeed() >= 30)
				{
					this.FighterState.LeftClickHold = true;
					this.FighterState.LeftClickPress = true;
				}
				break;
			}
		case 2: // Attacking
			{
				if(d_aiDebug){print("ATTACKING!");}
				this.FighterState.RightKeyHold = false;
				this.FighterState.LeftKeyHold = false;
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

	protected override void FixedUpdateProcessInput() // FUPI
	{
		m_WorldImpact = false;
		m_Landing = false;
		m_Kneeling = false;

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
			FighterState.RightClickPress = false;
			FighterState.LeftClickPress = false;
			FighterState.UpKeyHold = false;
			FighterState.LeftKeyHold = false;
			FighterState.DownKeyHold = false;
			FighterState.RightKeyHold = false;
			FighterState.JumpKeyPress = false;
			FighterState.ZonKeyPress = false;
		}

		//################################################################################
		//### ALL INPUT AFTER THIS POINT IS DISABLED WHEN THE PLAYER IS INCAPACITATED. ###
		//################################################################################

		FighterState.PlayerMouseVector = FighterState.MouseWorldPos-Vec2(this.transform.position);
		if(!(FighterState.LeftKeyHold||FighterState.RightKeyHold) || (FighterState.LeftKeyHold && FighterState.RightKeyHold))
		{
			//print("BOTH OR NEITHER");
			if(!(autoPressLeft||autoPressRight))
			{
				CtrlH = 0;
			}
			else if(autoPressLeft)
			{
				CtrlH = -1;
			}
			else
			{
				CtrlH = 1;
			}
		}
		else if(FighterState.LeftKeyHold)
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
		if(FighterState.DownKeyHold&&m_Grounded)
		{
			m_Kneeling = true;
			CtrlH = 0;
		}


		if(FighterState.JumpKeyPress&&(m_Grounded||m_Ceilinged||m_LeftWalled||m_RightWalled))
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

		if(FighterState.LeftClickRelease&&!(FighterState.DevMode||d_ClickToKnockFighter)&&!m_Kneeling)
		{
			if(!(FighterState.LeftKeyHold&&(FighterState.PlayerMouseVector.normalized.x>0))&&!(FighterState.RightKeyHold&&(FighterState.PlayerMouseVector.normalized.x<0))) // If trying to run opposite your punch direction, do not punch.
			{
				ThrowPunch(FighterState.PlayerMouseVector.normalized);
			}
			//print("Leftclick detected");
			//g_VelocityPunchExpended = false;
			FighterState.LeftClickRelease = false;
		}	

		if(FighterState.LeftClickHold)
		{
			i_LeftClickHoldDuration += Time.fixedDeltaTime;
			FighterState.Stance = 1;

			if((i_LeftClickHoldDuration>=g_VelocityPunchChargeTime) && (!this.isSliding()))
			{
				g_VelocityPunching = true;
				o_VelocityPunch.inUse = true;
			}
			else
			{
				g_VelocityPunching = false;
				o_VelocityPunch.inUse = false;
			}
		}
		else
		{
			i_LeftClickHoldDuration = 0;
			g_VelocityPunching = false;
			o_VelocityPunch.inUse = false;
		}

		// Once the input has been processed, set the press inputs to false so they don't run several times before being changed by update() again. 
		// FixedUpdate can run multiple times before Update refreshes, so a keydown input can be registered as true multiple times before update changes it back to false, instead of just the intended one time.
		FighterState.LeftClickPress = false; 	
		FighterState.RightClickPress = false;
		FighterState.ZonKeyPress = false;				
		FighterState.DisperseKeyPress = false;				
		FighterState.JumpKeyPress = false;				
		FighterState.LeftKeyPress = false;
		FighterState.RightKeyPress = false;
		FighterState.UpKeyPress = false;
		FighterState.DownKeyPress = false;
	}


	#endregion
}
