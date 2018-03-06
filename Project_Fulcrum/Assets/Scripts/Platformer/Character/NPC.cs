
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
	[SerializeField][ReadOnlyAttribute]private Vector2 goalLocation;		// Location that the NPC attempts to run at when in simple mode. Does not use pathfinding.
	[SerializeField][ReadOnlyAttribute]private int DecisionState;			// Defines what action the NPC is currently trying to do.
	[SerializeField]private float PunchDelay = 1;
	[SerializeField]private float PunchDelayVariance = 0.3f;
	[SerializeField]private float PunchCooldown;

	[SerializeField]private bool d_Think; 									// When false, AI brain is shut down. Fighter will just stand there.
	[SerializeField]public bool d_aiDebug;									// When true, enables ai debug messaging.
	[Space(10)]
	[Header("NAV MESH:")]
	[SerializeField][ReadOnlyAttribute] private NavMaster n_NavMaster; // Global navmesh handler for the level.
	[SerializeField][ReadOnlyAttribute] private int n_NavState = 0; // Used for nav movement state machine.
	[SerializeField][ReadOnlyAttribute] private float n_TraversalTime; // Time the NPC has been attempting a traversal.
	[SerializeField][ReadOnlyAttribute] private bool n_AtExit; // True when the NPC has a destination and is at the exit point leading to that destination.
	[SerializeField][ReadOnlyAttribute] private bool n_HasJumped; // True once the NPC has expended its one jump to reach its destination.
	[SerializeField][ReadOnlyAttribute] private float n_WindUpGoal = -1; // Goal location on the current surface to get enough runway to make the jump.

	[SerializeField][ReadOnlyAttribute] private float n_TraversalTimer = 0; // Time the NPC has been attempting a traversal. Once it exceeds its maximum, the traversal is deemed a failure.


	[SerializeField][ReadOnlyAttribute] private NavPath n_CurrentPath; // Currentpath is a chain of navconnections which lead to the NPC's desired final destination.
	[SerializeField][ReadOnlyAttribute] private int n_PathProgress; // Indicates which connection of the current path the NPC is on. 
	[SerializeField][ReadOnlyAttribute] private NavConnection n_ActiveConnection; // Connection the NPC is traversing.
	[SerializeField][ReadOnlyAttribute] private NavSurface[] n_SurfaceList;
	[SerializeField][ReadOnlyAttribute] private NavSurface n_CurrentSurf; // Surface the AI is standing on.
	[SerializeField][ReadOnlyAttribute] private NavSurface n_DestSurf;	// Surface the AI is trying to reach.
	[SerializeField] private int n_DestSurfID = -1; // ID of the destination surface.
	[SerializeField][ReadOnlyAttribute] private float n_SurfLineDist; // Distance from the current surface.
	[SerializeField] private float n_MaxSurfLineDist = 100; // Max distance from the current surface. Outside of this, it is considered off the surface.


	//########################################################################################################################################
	// CORE FUNCTIONS
	//########################################################################################################################################
	#region Core Functions

	protected void Start () 
	{
		this.FighterState.FinalPos = this.transform.position;
		n_NavMaster = GameObject.Find("NavMaster").GetComponent<NavMaster>();
		n_SurfaceList = n_NavMaster.GetSurfaces();
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

		if(!d_Think)
		{
			DecisionState = 3; // Mindlessly running to a destination which is only set manually.
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
					FighterChar fighter = i.GetComponent<FighterChar>();
					if(enemyTarget==null&&fighter!=null)
					{
						if(fighter.isAlive())
						{
							enemyTarget = fighter;
						}
					}
				}
				break;
			}
		case 1: // Pursuing
			{
				if(d_aiDebug){print("Chasing!");}
				this.FighterState.MouseWorldPos = enemyTarget.GetPosition();
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
				if( (this.GetSpeed() >= 30) && (PunchCooldown<=0) )
				{
					this.FighterState.LeftClickHold = true;
					this.FighterState.LeftClickPress = true;
				}
				break;
			}
		case 2: // Attacking
			{
				if(d_aiDebug){print("ATTACKING!");}

				this.FighterState.MouseWorldPos = enemyTarget.GetPosition();

				this.FighterState.RightKeyHold = false;
				this.FighterState.LeftKeyHold = false;

				if(PunchCooldown <= 0)
				{
					this.FighterState.LeftClickRelease = true;
					PunchCooldown = PunchDelay + UnityEngine.Random.Range(-PunchDelayVariance,PunchDelayVariance);
				}
				break;
			}
		case 3: // Traversing NavMesh
			{
				NavMeshMovement();
				break;
			}
		default: // Idle
			{
				break;
			}
		}
		#endregion

		//print("END OF AI FRAME #############################################################################################");
	}

	private void NavMeshMovement()
	{
		#region Nav Knowledge Gathering

		n_NavState = 0;

		if(n_CurrentSurf==null) // If  currentsurf = null, try and find a currentsurface.
		{
			for(int i = 0; i<n_SurfaceList.Length; i++)
			{
				if( n_SurfaceList[i].DistFromLine(this.m_GroundFoot.position)<=n_MaxSurfLineDist )
				{
					//print("CurrentSurf set to NavSurface["+i+"], dist: "+n_SurfaceList[i].DistFromLine(this.m_GroundFoot.position));
					n_CurrentSurf = n_SurfaceList[i];
					break;
				}
				else
				{
					//print("distance to NavSurface["+i+"] was too far. Dist= "+n_SurfaceList[i].DistFromLine(this.m_GroundFoot.position));
				}
			}
		}
		else // if there is a currentsurface, check if it's too far now.
		{
			if(n_CurrentSurf.DistFromLine(this.m_GroundFoot.position)>n_MaxSurfLineDist)
			{
				n_CurrentSurf=null; // If current surface too distant, reset it and skip this ai frame.
				return;
			}
		}

//		n_DestSurfID = Random.Range(0, n_SurfaceList.Length-1);
		if(n_CurrentSurf!=null)
		{
			if(n_DestSurf==null)
			{
				n_ActiveConnection=null;
				n_CurrentPath=null;

				if( n_DestSurfID!=-1 && n_DestSurfID<n_SurfaceList.Length)
				{
					if(n_DestSurfID != n_CurrentSurf.id)
					{
						n_DestSurf = n_SurfaceList[n_DestSurfID];
						print("n_DestSurf set to NavSurface["+n_DestSurfID+"]");
					}
					else
					{
						n_DestSurfID = -1;
					}
				}
				else 
				{
					n_NavState = 0;
				}
			}
			else if(n_DestSurf.id!=n_DestSurfID)
			{
				n_ActiveConnection=null;
				n_CurrentPath=null;

				if(n_DestSurfID!=-1)
				{
					if( n_DestSurfID!=-1 && n_DestSurfID<n_SurfaceList.Length-1 )
					{
						n_DestSurf = n_SurfaceList[n_DestSurfID];
						print("n_DestSurf set to NavSurface["+n_DestSurfID+"]");
					}
					else
					{
						n_DestSurfID = -1;
					}
				}
				else
				{
					n_NavState = 0;
					n_DestSurf = null;
				}
			}

			if(n_DestSurf!=null)
			{
				if(n_CurrentPath==null)
				{
					NavPath[] pathChoices = n_NavMaster.GetPathList(n_CurrentSurf.id, n_DestSurf.id);	
					if(pathChoices!=null)
					{
						n_CurrentPath = pathChoices[0];
						n_PathProgress = 0;
					}
					else
					{
						print("Pathchoices is null!");
						n_DestSurfID = -1;
					}
				}
				if(n_CurrentPath!=null)
				{
					n_ActiveConnection = n_CurrentPath.edges[n_PathProgress];
					if(!n_AtExit)
					{
						n_NavState = 1;
					}
					else
					{
						n_NavState = 2;
					}
				}
			}
		}
		#endregion
		#region Nav Decision Making

		ClearAllInput(); // Ensure that keys are polled every ai frame and do not remain on when not being held.

		switch(n_NavState)
		{
			case 0: //Idle - no destination
				{
					//print("Idle.");
					if(FighterState.Vel.x > 5f) // Slow to a halt since there is no goal.
					{
						//print("Stopping right movement.");
						FighterState.LeftKeyHold = true;
					}
					else if(FighterState.Vel.x < -5f) // Slow to a halt since there is no goal.
					{
						//print("Stopping left movement.");
						FighterState.RightKeyHold = true; 
					}
					break;	
				}
			case 1: // Moving to exit point.
				{
					if(n_WindUpGoal > 0)
					{
						NavGotoWindupPoint();
					}
					else
					{
						NavGotoExitPoint(n_ActiveConnection);
					}

					break;	
				}
			case 2: // Traversing.
				{
					NavTraverse(n_ActiveConnection);
					break;	
				}
			case 3: // No current surface - orphaned from the nav mesh.
				{
					break;
				}
			case 4: // Windup - Not enough speed to make jump, backing up to take a running start.
				{
					break;
				}
		}
		#endregion


	}

	private void EndTraverse(bool successful)
	{
		if(successful)
		{
			n_CurrentSurf = n_ActiveConnection.dest;
			//n_DestSurf = null;

			//test code

			//end test code
			print("Arrived at destination!!");
			n_PathProgress++;
			if(n_PathProgress>n_CurrentPath.edges.Length-1)
			{
				print("Path traversal completed :)");
				n_CurrentPath = null;
				n_DestSurfID = -1;
			}
		}
		else
		{
			n_CurrentPath = null;
			n_CurrentSurf = null;
			n_PathProgress = 0;
		}
		n_TraversalTimer = 0;
		n_AtExit = false;
		n_ActiveConnection = null;
		n_HasJumped = false;
	}

	private void NavGotoWindupPoint()
	{
		float linPos = n_CurrentSurf.WorldToLinPos(this.m_GroundFoot.position);
		float distToWindupPoint = n_WindUpGoal-linPos;

		if(Mathf.Abs(distToWindupPoint)>0.5f)
		{
			if( distToWindupPoint<0 )
			{
				//print("Moving left to exit point at: "+distToWindupPoint);
				FighterState.LeftKeyHold = true;
			}
			else
			{
				//print("Moving right to exit point at: "+distToWindupPoint);
				FighterState.RightKeyHold = true;
			}
		}
		else
		{
			print("At windup point!");
			n_WindUpGoal = -1;
		}
	}

	private void NavGotoExitPoint(NavConnection navCon)
	{
		print("NavGotoExitPoint");
		float linPos = n_CurrentSurf.WorldToLinPos(this.m_GroundFoot.position);
		float distToExitPoint = navCon.exitPosition-linPos;

		if(n_ActiveConnection.exitVel.x<=0) // If required exit velocity is negative.
		{
			if(FighterState.Vel.x<0) // if x velocity is also negative
			{
				if(FighterState.Vel.x<navCon.minExitVel) // If going too fast.
				{
					 // Do nothing! You will slow down over time. Replace this with a deceleration distance equation in the future for better results.
				}
				else
				{
					float distanceNeeded = NavGetWindupDist((n_ActiveConnection.maxExitVel), FighterState.Vel.x);
					print("Distance needed: "+distanceNeeded+", distToExitPoint: "+Mathf.Abs(distToExitPoint));

					if(distanceNeeded>Mathf.Abs(distToExitPoint))
					{
						print("NOT ENOUGH RUNWAY!");
						n_WindUpGoal = navCon.exitPosition+distanceNeeded;
					}
					else
					{
						FighterState.LeftKeyHold = true;
					}
				}

			}
			else if(FighterState.Vel.x>0) // if x velocity is positive, away from the exit point
			{
				FighterState.LeftKeyHold = true;
				float distanceForStop = NavGetWindupDist(0, FighterState.Vel.x);
				print("DistanceForStop: "+distanceForStop);
			}
			else
			{
				float distanceNeeded = NavGetWindupDist((n_ActiveConnection.maxExitVel));
				print("Distance needed: "+distanceNeeded+", distToExitPoint: "+Mathf.Abs(distToExitPoint));
				if(distanceNeeded>Mathf.Abs(distToExitPoint))
				{
					print("Stationary: NOT ENOUGH RUNWAY!");
					n_WindUpGoal = navCon.exitPosition+distanceNeeded;
				}
				else
				{
					FighterState.LeftKeyHold = true;
				}
			}
		}
		else if(n_ActiveConnection.exitVel.x>0) // If velocity is positive.
		{
			if(FighterState.Vel.x>0) // if x velocity is also positive
			{
				if(FighterState.Vel.x>n_ActiveConnection.maxExitVel) // If going too fast.
				{
					// Do nothing! You will slow down over time. Replace this with a deceleration distance equation in the future for better results.
				}
				else if(FighterState.Vel.x<0) // If not going fast enough, accelerate toward exitposition. If not enough room, set windup goal.
				{
					FighterState.RightKeyHold = true;
					float distanceForStop = NavGetWindupDist(0, FighterState.Vel.x);
					print("DistanceForStop: "+distanceForStop);
				}
				else
				{
					float distanceNeeded = NavGetWindupDist((n_ActiveConnection.minExitVel), FighterState.Vel.x);
					print("Distance needed: "+distanceNeeded+", distToExitPoint: "+Mathf.Abs(distToExitPoint));

					if(distanceNeeded>Mathf.Abs(distToExitPoint))
					{
						print("NOT ENOUGH RUNWAY!");
						n_WindUpGoal = navCon.exitPosition-distanceNeeded;
					}
					else
					{
						FighterState.RightKeyHold = true;
					}
				}

			}
			else if(FighterState.Vel.x>0) // if x velocity is negative, away from the exit point
			{
				FighterState.RightKeyHold = true;
//				float distanceNeeded = NavGetWindupDist((n_ActiveConnection.exitVel.x-n_ActiveConnection.exitVelRange));
//				float distanceForStop = NavGetWindupDist(0, FighterState.Vel.x);
//				print("DistanceForStop: "+distanceForStop);

			}
			else // x velocity stationary
			{
				float distanceNeeded = NavGetWindupDist((n_ActiveConnection.exitVel.x-n_ActiveConnection.exitVelRange));
				print("Distance needed: "+distanceNeeded+", distToExitPoint: "+Mathf.Abs(distToExitPoint));
				if(distanceNeeded>Mathf.Abs(distToExitPoint))
				{
					print("Stationary: NOT ENOUGH RUNWAY!");
					n_WindUpGoal = navCon.exitPosition-distanceNeeded;
				}
				else
				{
					FighterState.RightKeyHold = true;
				}
			}
		}

		if( Mathf.Abs(distToExitPoint)<=navCon.exitPositionRange )
		{
			print("At exit point.");
			n_AtExit = true;
		}
	}

	private void NavTraverse(NavConnection navCon)
	{
		n_TraversalTimer += Time.fixedDeltaTime;

		if(n_TraversalTimer>navCon.traversaltimeout)
		{
			print("Traversal timeout! Recalculating...");
			EndTraverse(false);
		}
		if(navCon==null)
		{
			print("navCon NULL! ARGH");
		}

		if(n_DestSurf==null)
		{
			print("n_destsurf NULL!");
		}

		Vector2 directionVector = navCon.dest.LinToWorldPos(navCon.destPosition)-this.GetFootPosition();
		if( directionVector.magnitude<=0.5f )
		{
			//print("n_DestSurf.LinToWorldPos(navCon.destPosition) = "+n_DestSurf.LinToWorldPos(navCon.destPosition));
			//print("this.GetFootPosition() = "+this.GetFootPosition());
			EndTraverse(true);
			return;
		}


		switch(navCon.traverseType)
		{
		case 0: // Walking
			{
				if( directionVector.x < 0 )
				{
					this.FighterState.RightKeyHold = false;
					this.FighterState.LeftKeyHold = true;
				}
				else
				{
					this.FighterState.RightKeyHold = true;
					this.FighterState.LeftKeyHold = false;
				}
				break;	
			}
		case 1: // Jumping
			{
				if( !n_HasJumped )
				{
					this.FighterState.JumpKeyPress = true;
					n_HasJumped = true;
				}
				if( directionVector.x < 0 )
				{
					this.FighterState.RightKeyHold = false;
					this.FighterState.LeftKeyHold = true;
				}
				else
				{
					this.FighterState.RightKeyHold = true;
					this.FighterState.LeftKeyHold = false;
				}
				break;	
			}
		}
	}

	public float NavGetWindupDist(float reqVel) // reqVel is the required velocity. The distance returned is how much runway the NPC needs to reach that speed.
	{
		float distance;

		float linDist = ((reqVel*reqVel)/(m_LinearAccelRate/Time.fixedDeltaTime));
		//print("linDist="+linDist);
		float linDistBelowThreshold = ((m_TractionChangeT*m_TractionChangeT)/(m_LinearAccelRate/Time.fixedDeltaTime));
		//print("linDistBelowThreshold="+linDistBelowThreshold);
		float linDistAboveThreshold = linDist-linDistBelowThreshold;

		float fastDist = ((m_TractionChangeT*m_TractionChangeT)/(m_StartupAccelRate/Time.fixedDeltaTime));


		if(Mathf.Abs(reqVel)<m_TractionChangeT)
		{
			distance = ((reqVel*reqVel)/(m_StartupAccelRate/Time.fixedDeltaTime));;
			print("1-arg Windup Distance: "+distance+" for vel of "+reqVel);
		}
		else
		{
			distance = linDistAboveThreshold+fastDist; // The acceleration is a piecewise function, so the bottom of lindist needs to be chopped off and replaced by fastdist result.
			print("1-arg Windup Distance: "+distance+" for vel of "+reqVel+". LinearDistanceAboveTheshold="+linDistAboveThreshold+", fastDistBelowThreshold="+fastDist);
		}

		return distance;
	}

	public float NavGetWindupDist(float reqVel, float curVel) // reqVel is the required additional velocity. The distance returned is how much
	{
//
//		float accelRate = 0;
//
//		if(FighterState.Vel.magnitude<m_TractionChangeT)
//			accelRate = m_StartupAccelRate;
//		else
//			accelRate = m_LinearAccelRate;
//		
//
//		float distance = ((reqVel*reqVel)/(accelRate/Time.fixedDeltaTime));
//		float subtracted = ((curVel*curVel)/(accelRate/Time.fixedDeltaTime));
//		return distance-subtracted;

		float distanceNeeded = NavGetWindupDist(reqVel);
		float distanceNotNeeded = NavGetWindupDist(curVel);
		print("2arg Windup Distance: "+(distanceNeeded-distanceNotNeeded)+" for vel of "+reqVel+", with curvel="+curVel+". distanceNeeded="+distanceNeeded+", distanceNotNeeded="+distanceNotNeeded);
		return distanceNeeded-distanceNotNeeded;
	}

	public void ClearAllInput()
	{
		this.FighterState.RightKeyHold = false;
		this.FighterState.LeftKeyHold = false;
		this.FighterState.JumpKeyPress = false;
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
		m_WorldImpact = false; //Placeholder??
		FighterState.Stance = 0;
		m_Kneeling = false;

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

		if(m_Airborne)
		{
			if(FighterState.Vel.x>0)
			{
				facingDirection = true;
			}
			else if(FighterState.Vel.x<0)
			{
				facingDirection = false;
			}
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
			if(IsVelocityPunching())
			{
				v_TriggerAtkHit = true;
			}
			else
			{
				if(!(FighterState.LeftKeyHold && (FighterState.PlayerMouseVector.normalized.x>0)) && !(FighterState.RightKeyHold && (FighterState.PlayerMouseVector.normalized.x<0))) // If trying to run opposite your punch direction, do not punch.
				{
					ThrowPunch(FighterState.PlayerMouseVector.normalized);
				}
				else
				{
					ThrowPunch(FighterState.Vel.normalized);
				}
			}
			//print("Leftclick detected");
			FighterState.LeftClickRelease = false;
		}

		if(FighterState.RightClickHold)
		{
			FighterState.Stance = 2;
		}

		if(FighterState.DisperseKeyPress)
		{
			ZonPulse();
			FighterState.DisperseKeyPress = false;
		}

		if(FighterState.LeftClickHold)
		{
			FighterState.LeftClickHoldDuration += Time.fixedDeltaTime;
			FighterState.Stance = 1;

			if((FighterState.LeftClickHoldDuration>=g_VelocityPunchChargeTime) && (!this.isSliding()))
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
			FighterState.LeftClickHoldDuration = 0;
			g_VelocityPunching = false;
			o_VelocityPunch.inUse = false;
		}

		if(FighterState.DisperseKeyPress)
		{
			ZonPulse();
			FighterState.DisperseKeyPress = false;
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

		FighterState.LeftClickRelease = false; 	
		FighterState.RightClickRelease = false;			
		FighterState.LeftKeyRelease = false;
		FighterState.RightKeyRelease = false;
		FighterState.UpKeyRelease = false;
		FighterState.DownKeyRelease = false;
	}


	#endregion
}
