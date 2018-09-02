
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
	[SerializeField][ReadOnlyAttribute]protected FighterChar enemyTarget; 	// Reference to the enemy combatant the NPC wants to fight.
	[SerializeField][ReadOnlyAttribute]protected float distanceToTarget; 		// Distance from enemy target.
	[SerializeField][ReadOnlyAttribute]protected Vector2 goalLocation;		// Location that the NPC attempts to run at when in simple mode. Does not use pathfinding.
	[SerializeField][ReadOnlyAttribute]protected int DecisionState;			// Defines what action the NPC is currently trying to do.
	[SerializeField]protected float attackRange = 1f;		// Range at which the NPC will stop running and punch.
	[SerializeField]protected float PunchDelay = 1;
	[SerializeField]protected float PunchDelayVariance = 0.3f;
	[SerializeField][ReadOnlyAttribute]protected float PunchCooldown;
	[SerializeField]protected bool d_Think; 									// When false, AI brain is shut down. Fighter will just stand there.
	[SerializeField]protected bool d_DumbNav; 								// When true, the fighter will simply charge at enemies and not attempt to navigate to them.

	[SerializeField]public bool d_aiDebug;									// When true, enables ai debug messaging.
	[Space(10)]
	[Header("NAV MESH:")]
	[SerializeField][ReadOnlyAttribute] protected int n_NavState = 0; // Used for nav movement state machine.
	[SerializeField][ReadOnlyAttribute] protected float n_TraversalTime; // Time the NPC has been attempting a traversal.
	[SerializeField][ReadOnlyAttribute] protected bool n_AtExit; // True when the NPC has a destination and is at the exit point leading to that destination.
	[SerializeField][ReadOnlyAttribute] protected bool n_HasJumped; // True once the NPC has expended its one jump to reach its destination.
	[SerializeField][ReadOnlyAttribute] protected float n_WindUpGoal = -1; // Goal location on the current surface to get enough runway to make the jump.
	[SerializeField][ReadOnlyAttribute] protected float n_TraversalTimer = 0; // Time the NPC has been attempting a traversal. Once it exceeds its maximum, the traversal is deemed a failure.
	[SerializeField][ReadOnlyAttribute] protected int n_PathProgress; // Indicates which connection of the current path the NPC is on. 
	[SerializeField][ReadOnlyAttribute] protected bool n_Traversing; // True when the NPC is moving between surfaces.
	[SerializeField][ReadOnlyAttribute] protected bool n_HasAPath; // True when the NPC has an active currentpath.


	[SerializeField][ReadOnlyAttribute] protected LineRenderer o_NavDebugLine; // Line between NavDebugMarker and the NPC.
	[SerializeField][ReadOnlyAttribute] protected Transform o_NavDebugMarker; // Set to the position of the current goal point.
	[SerializeField][ReadOnlyAttribute] protected NavPath n_CurrentPath; // Currentpath is a chain of navconnections which lead to the NPC's desired final destination.
	[SerializeField][ReadOnlyAttribute] protected NavConnection n_ActiveConnection; // Connection the NPC is traversing.
	[SerializeField][ReadOnlyAttribute] protected NavSurface n_DestSurf;	// Surface the AI is trying to reach.
	[SerializeField] protected int n_DestSurfID = -1; // ID of the destination surface.
	[SerializeField][ReadOnlyAttribute] protected float n_SurfLineDist; // Distance from the current surface.

	//########################################################################################################################################
	// CORE FUNCTIONS
	//########################################################################################################################################
	#region Core Functions

	protected void Start () 
	{
		this.FighterState.FinalPos = this.transform.position;
		o_NavDebugMarker = this.transform.Find("NavDebugMarker");
		o_NavDebugMarker.parent = null;
		o_NavDebugLine = o_NavDebugMarker.GetComponent<LineRenderer>();
	}

	protected override void Awake()
	{
		isAPlayer = false;
		FighterAwake();
	}

	protected override void FixedUpdate()
	{
		d_TickCounter++;
		d_TickCounter = (d_TickCounter > 60) ? 0 : d_TickCounter; // Rolls back to zero when hitting 60
		UpdateCurrentNavSurf();
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
		FighterState.EtherKeyPress = false;
	}
	#endregion
	//###################################################################################################################################
	// CUSTOM FUNCTIONS
	//###################################################################################################################################
	#region Custom Functions

	protected virtual void FixedUpdateAI()
	{
		FixedUpdateAIThink(); // Gathers information and decides which mode to use

		#region Decision Making
		switch (DecisionState)
		{
		case 0: // Searching
			{
				if(d_aiDebug){print("searching...");}
				o_NavDebugMarker.position=this.transform.position;
				Vector3[] positions = {this.transform.position,o_NavDebugMarker.position};
				o_NavDebugLine.SetPositions(positions);
				
				NavSearchSimple();

				break;
			}
		case 1: // Pursuing
			{
				if(d_aiDebug){print("Chasing!");}
				NavPursuitMelee();
				break;
			}

		case 2: // Attacking
			{
				if(d_aiDebug){print("ATTACKING!");}
				NavAttackMeleeSimple();
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
	}

	protected virtual void FixedUpdateAIThink() //FUAIT
	{
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

		if(DecisionState == 1&&!d_DumbNav)
		{
			if((enemyTarget.n_CurrentSurfID != n_CurrentSurfID) && (enemyTarget.n_CurrentSurfID != n_DestSurfID) && (enemyTarget.n_CurrentSurfID!=-1))
			{
				n_DestSurfID = enemyTarget.n_CurrentSurf.id;
			}
			else if(enemyTarget.n_CurrentSurfID == n_CurrentSurfID)
			{
				n_DestSurfID = -1;
			}

			if(n_DestSurfID!=-1) // if a nav destination is set and npc is pursuing, use the nav system.
			{
				DecisionState = 3;
			}
		}

		if(!d_Think)
		{
			DecisionState = 3; // Mindlessly running to a destination which is only set manually.
		}

	}

	protected void NavSearchSimple() //NSS
	{
		int layerMask = 1 << 13;
		Collider2D[] NearbyEntities = Physics2D.OverlapCircleAll(this.FighterState.FinalPos, 10f, layerMask, -1, 1);
		foreach(Collider2D i in NearbyEntities)
		{
			if(this.GetComponent<Collider2D>() == i){continue;}
			if(d_aiDebug){print("TESTING ENTITY:"+i);}
			FighterChar fighter = i.GetComponent<FighterChar>();
			if(enemyTarget==null&&fighter!=null)
			{
				if(fighter.isAlive()&&fighter.IsPlayer())
				{
					enemyTarget = fighter;
				}
			}
		}
	}

	protected void NavPursuitMelee() //NPM
	{
		this.FighterState.MouseWorldPos = enemyTarget.GetPosition();

		o_NavDebugMarker.position=(Vector3)this.FighterState.MouseWorldPos;
		Vector3[] positions = {this.transform.position,o_NavDebugMarker.position};
		o_NavDebugLine.SetPositions(positions);

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
		if( (this.GetSpeed() >= 30) && (PunchCooldown<=0) )
		{
			this.FighterState.LeftClickHold = true;
			this.FighterState.LeftClickPress = true;
		}
	}

	protected void NavAttackMeleeSimple() // NAMS
	{
		o_NavDebugMarker.position=(Vector3)enemyTarget.GetPosition();
		Vector3[] positions = {this.transform.position,o_NavDebugMarker.position};
		o_NavDebugLine.SetPositions(positions);

		this.FighterState.MouseWorldPos = enemyTarget.GetPosition();

		this.FighterState.RightKeyHold = false;
		this.FighterState.LeftKeyHold = false;

		if(PunchCooldown <= 0)
		{
			this.FighterState.LeftClickRelease = true;
			PunchCooldown = PunchDelay + UnityEngine.Random.Range(-PunchDelayVariance,PunchDelayVariance);
		}
	}

	protected void NavMeshMovement()
	{
		#region Nav Knowledge Gathering

		n_NavState = 0;

		if(n_DestSurfID==-1)
		{
			SetDestination();
		}

		if(n_CurrentSurf!=null)
		{
			if(n_DestSurf==null)
			{
				SetDestination();
			}
			else if(n_DestSurf.id!=n_DestSurfID)
			{
				SetDestination();
			}

			if(n_DestSurf!=null)
			{
				if(!n_HasAPath) // If no path yet, get one.
				{
					if(d_aiDebug){print("["+d_TickCounter+"]:Set new currentpath.");}
					SetCurrentPath();
				}
				if(n_HasAPath) // If NPC now has path, use it.
				{
					if(n_CurrentPath==null)
					{
						if(d_aiDebug){print("["+d_TickCounter+"]: n_CurrentPath==null");}
					}
					n_ActiveConnection = n_CurrentPath.edges[n_PathProgress];

					if(n_CurrentSurfID!=-1 && n_CurrentSurfID != n_ActiveConnection.orig.id) // If on the wrong surface for the current connection, find a new path.
					{
						SetCurrentPath();
						return;
					}

					if(!n_AtExit)
					{
						n_NavState = 1;
					}
					else
					{
						n_WindUpGoal = -1;
						n_Traversing = true;
						n_NavState = 2;
					}
				}
			}
		}
		else if(n_Traversing)
		{
			n_NavState = 2;
		}
		else
		{
			n_NavState = 3;
		}
		#endregion
		#region Nav Visualizing
		if(n_HasAPath&&n_CurrentPath!=null)
		{
			Vector3[] linePos = new Vector3[n_CurrentPath.edges.Length-n_PathProgress+2];
			linePos[0] = this.transform.position;
			for(int i = n_PathProgress; i < n_CurrentPath.edges.Length; i++)
			{
				linePos[i+1] = (Vector3)n_CurrentPath.edges[i].dest.LinToWorldPos(n_CurrentPath.edges[i].destPosition);
			}
			if(enemyTarget!=null)
			{
				o_NavDebugMarker.position=(Vector3)enemyTarget.GetPosition();
				linePos[linePos.Length-1] = o_NavDebugMarker.position;
			}
			else
			{
				if(linePos[linePos.Length-2]!=null)
				{
					o_NavDebugMarker.position=linePos[linePos.Length-2];
					linePos[linePos.Length-1] = o_NavDebugMarker.position;
				}
			}
			o_NavDebugLine.positionCount = linePos.Length;
			o_NavDebugLine.SetPositions(linePos);
		}
		else
		{
			o_NavDebugLine.positionCount = 2;
		}
		#endregion
		#region Nav Decision Making

		ClearAllInput(); // Ensure that keys are polled every ai frame and do not remain on when not being held.

		switch(n_NavState)
		{
			case 0: //Idle - no destination
				{
					if(d_aiDebug){print("Idle.");}
					o_NavDebugMarker.position = this.transform.position;
					Vector3[] positions = {this.transform.position,o_NavDebugMarker.position};
					o_NavDebugLine.SetPositions(positions);

					if(FighterState.Vel.x > 5f) // Slow to a halt since there is no goal.
					{
						if(d_aiDebug){print("Stopping right movement.");}
						FighterState.LeftKeyHold = true;
					}
					else if(FighterState.Vel.x < -5f) // Slow to a halt since there is no goal.
					{
						if(d_aiDebug){print("Stopping left movement.");}
						FighterState.RightKeyHold = true; 
					}
					break;	
				}
			case 1: // Moving to exit point.
				{
					if(n_CurrentSurf.surfaceType >= 2)
					{
						NavWallSlide(n_ActiveConnection);
						break;
					}
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
		}
		#endregion
	}

	protected void NavWallSlide(NavConnection navCon)
	{
		if(d_aiDebug){print("["+d_TickCounter+"]:NavSlideToExitPoint");}
		float linPos = n_CurrentSurf.WorldToLinPos(this.m_GroundFoot.position);
		float distFromExitPoint = linPos-navCon.exitPosition;



		if(n_ActiveConnection.exitVel.y<0) // If required exit velocity is negative.
		{
			if(distFromExitPoint<0) // if past the destination, get a new path.
			{
				float distanceNeeded = NavGetWindupDist((n_ActiveConnection.minExitVel), 0);
				if(d_aiDebug){print("["+d_TickCounter+"]:Distance needed: "+distanceNeeded+", distToExitPoint: "+distFromExitPoint);}
				if(d_aiDebug){print("["+d_TickCounter+"]:ExitVel Negative, Vel Negative, passed exitpoint and must turn around.");}
			}
			else
			{
				if(navCon.orig.surfaceType==2) // Run down the left wall.
				{
					FighterState.RightKeyHold = true;
				}
				else 						// Run down the right wall.
				{
					FighterState.LeftKeyHold = true;
				}
			}
		}
		else if(n_ActiveConnection.exitVel.y>0) // If velocity is positive.
		{
			if(FighterState.Vel.y>=0) // if y velocity is also positive
			{
				if(navCon.orig.surfaceType==3) // Run up the right wall.
				{
					FighterState.RightKeyHold = true;
				}
				else // Run up the left wall.
				{
					FighterState.LeftKeyHold = true;
				}
			}
			else // if y velocity is negative, slide down the wall and check if you passed the exit point.
			{
				if(distFromExitPoint<0) // if the destination is passed.
				{
					float distanceNeeded = NavGetWindupDist((n_ActiveConnection.minExitVel), 0);
					if(d_aiDebug){print("["+d_TickCounter+"]:Distance needed: "+distanceNeeded+", distToExitPoint: "+distFromExitPoint);}
					if(d_aiDebug){print("["+d_TickCounter+"]:ExitVel positive, Vel Negative, passed exitpoint and must turn around.");}
				}
				if(navCon.orig.surfaceType==3) // Run up the right wall.
				{
					FighterState.RightKeyHold = true;
				}
				else // Run up the left wall.
				{
					FighterState.LeftKeyHold = true;
				}
			}
		}
		else
		{
			if(d_aiDebug){print("exitVelocity is zero");}
			if(navCon.orig.surfaceType==3) // Run up the right wall.
			{
				FighterState.RightKeyHold = true;
			}
			else // Run up the left wall.
			{
				FighterState.LeftKeyHold = true;
			}
		}

		if( (Mathf.Abs(distFromExitPoint)<=navCon.exitPositionRange))
		{
			if((FighterState.Vel.x>=navCon.minExitVel) && (FighterState.Vel.x<=navCon.maxExitVel))
			{
				if(d_aiDebug){print("["+d_TickCounter+"]:EXITING.");}
				n_AtExit = true;
			}
			else
			{
				if(d_aiDebug){print("["+d_TickCounter+"]:min("+navCon.minExitVel+"), Vel("+FighterState.Vel.x+"), max("+navCon.maxExitVel+")");}
				if(d_aiDebug){print("["+d_TickCounter+"]:Vel["+FighterState.Vel.x+"] <= minExitVel("+navCon.maxExitVel+")");}
			}
		}
	}

	protected void SetCurrentPath()
	{
		NavPath[] pathChoices = o_NavMaster.GetPathList(n_CurrentSurf.id, n_DestSurf.id);	
		if(pathChoices!=null)
		{
			if(pathChoices[0]==null)
			{
				if(d_aiDebug){print("["+d_TickCounter+"]: pathChoices[0]==null upon creation!");}
			}
			else if(pathChoices[0].edges==null)
			{
				if(d_aiDebug){print("["+d_TickCounter+"]: pathChoices[0].edges==null upon creation!");}
			}
			else
			{
				n_CurrentPath = pathChoices[0];
				n_HasAPath = true;
				n_PathProgress = 0;
			}
		}
		else
		{
			if(d_aiDebug){print("["+d_TickCounter+"]:No pathchoices were found!");}
			n_DestSurfID = -1;
		}
	}

	protected void SetDestination()
	{
		n_ActiveConnection=null;
		n_CurrentPath=null;
		n_HasAPath = false;

		if( n_DestSurfID!=-1 && n_DestSurfID<o_NavMaster.GetSurfaces().Length)
		{
			if(n_DestSurfID != n_CurrentSurfID)
			{
				n_DestSurf = o_NavMaster.GetSurfaces()[n_DestSurfID];
				if(d_aiDebug){print("["+d_TickCounter+"]:Destination set to NavSurface["+n_DestSurfID+"]");}
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
		
	protected void EndTraverse(bool successful)
	{
		if(successful)
		{
			//n_CurrentSurf = n_ActiveConnection.dest;
			//n_CurrentSurfID = n_ActiveConnection.dest.id;

			n_CurrentPath.edges[n_PathProgress].successfulTraversals++;
			n_CurrentPath.edges[n_PathProgress].averageTraversalTime = (n_CurrentPath.edges[n_PathProgress].averageTraversalTime+n_TraversalTimer)/2;


			if(d_aiDebug){print("["+d_TickCounter+"]:Arrived at destination!!");}
			n_PathProgress++;
			if(n_PathProgress>=n_CurrentPath.edges.Length)
			{
				if(d_aiDebug){print("["+d_TickCounter+"]:Path traversal completed :)");}
				n_CurrentPath = null;
				n_HasAPath = false;
				n_DestSurfID = -1;
				n_PathProgress = 0;
			}
		}
		else
		{
			n_CurrentPath.edges[n_PathProgress].failedTraversals++;
			n_CurrentPath = null;
			n_HasAPath = false;
			n_DestSurfID = -1;
			n_PathProgress = 0;
		}
		n_Traversing = false;
		n_TraversalTimer = 0;
		n_AtExit = false;
		n_ActiveConnection = null;
		n_HasJumped = false;
	}

	protected void NavGotoWindupPoint()
	{
		if(d_aiDebug){print("["+d_TickCounter+"]:NavGotoWindupPoint");}
		float linPos = n_CurrentSurf.WorldToLinPos(this.m_GroundFoot.position);
		float distToWindupPoint = linPos-n_WindUpGoal;

		if(d_aiDebug){print("["+d_TickCounter+"]: distToWindupPoint="+distToWindupPoint+"("+linPos+"-"+n_WindUpGoal+")");}

		if(Mathf.Abs(distToWindupPoint)>0.25f)
		{
			if( distToWindupPoint>0 )
			{
				if(d_aiDebug){print("Moving left to exit point at: "+distToWindupPoint);}
				FighterState.LeftKeyHold = true;
			}
			else
			{
				if(d_aiDebug){print("Moving right to exit point at: "+distToWindupPoint);}
				FighterState.RightKeyHold = true;
			}
		}
		else
		{
			if(d_aiDebug){print("["+d_TickCounter+"]:At windup point!");}
			n_WindUpGoal = -1;
		}
	}

	protected void NavGotoExitPoint(NavConnection navCon)
	{
		if(d_aiDebug){print("["+d_TickCounter+"]:NavGotoExitPoint");}
		float linPos = n_CurrentSurf.WorldToLinPos(this.m_GroundFoot.position);
		float distFromExitPoint = linPos-navCon.exitPosition;

		if(n_ActiveConnection.exitVel.x<0) // If required exit velocity is negative.
		{
			if(FighterState.Vel.x<=0) // if x velocity is also negative
			{
				if(distFromExitPoint<0) // if the destination is in the other direction, turn around.
				{
					float distanceNeeded = NavGetWindupDist((n_ActiveConnection.minExitVel), 0);
					if(d_aiDebug){print("["+d_TickCounter+"]:Distance needed: "+distanceNeeded+", distToExitPoint: "+distFromExitPoint);}
					n_WindUpGoal = navCon.exitPosition+distanceNeeded-0.1f;
					if(n_WindUpGoal<0)
					{
						if(d_aiDebug){print("["+d_TickCounter+"]:Windupgoal is past the edge of the surface! Cannot use this connection.");}
					}
					if(d_aiDebug){print("["+d_TickCounter+"]:ExitVel Negative, Vel Negative, passed exitpoint and must turn around.");}
				}
				else if(FighterState.Vel.x<navCon.minExitVel+0.5f) // If going too fast.
				{
					if(d_aiDebug){print("["+d_TickCounter+"]:ExitVel Negative, Vel Negative, speed too fast");}
					// Do nothing! You will slow down over time. Replace this with a deceleration distance equation in the future for better results.
				}
				else if(FighterState.Vel.x<navCon.maxExitVel-0.5f) // If going fast enough, keep accelerating up to the max velocity anyway.
				{
					if(d_aiDebug){print("["+d_TickCounter+"]:ExitVel Negative, Vel Negative, speed good");}
					FighterState.LeftKeyHold = true;
				}
				else // If not going fast enough
				{
					float distanceNeeded = NavGetWindupDist((n_ActiveConnection.maxExitVel), FighterState.Vel.x);
					if(d_aiDebug){print("["+d_TickCounter+"]:Distance needed: "+distanceNeeded+", distToExitPoint: "+distFromExitPoint);}

					if(distanceNeeded>distFromExitPoint) // If the player does not have enough room to reach speed, set a starting point further back.
					{
						if(d_aiDebug){print("["+d_TickCounter+"]:NOT ENOUGH RUNWAY!");}
						if(d_aiDebug){print("["+d_TickCounter+"]:ExitVel Negative, Vel Negative, speed too slow, setting windup point");}

						n_WindUpGoal = navCon.exitPosition+distanceNeeded+0.1f;
					}
					else 								// If the player has enough room to reach speed, just run.
					{
						if(d_aiDebug){print("["+d_TickCounter+"]:ExitVel Negative, Vel Negative, speed too slow, accelerating");}
						FighterState.LeftKeyHold = true;
					}
				}

			}
			else// if(FighterState.Vel.x>0) // if x velocity is positive, away from the exit point, decelerate to turn around.
			{
				if(d_aiDebug){print("["+d_TickCounter+"]:ExitVel Negative, Vel Positive, reversing direction");}
				FighterState.LeftKeyHold = true;
				float distanceForStop = NavGetWindupDist(0, FighterState.Vel.x);
				if(d_aiDebug){print("["+d_TickCounter+"]:DistanceForStop: "+distanceForStop);}
			}
//			else
//			{
//				float distanceNeeded = NavGetWindupDist((n_ActiveConnection.maxExitVel));
//				print("["+d_TickCounter+"]:Distance needed: "+distanceNeeded+", distToExitPoint: "+distToExitPoint);
//				if(distanceNeeded<distToExitPoint) // if more distance is needed in the negative direction
//				{
//					print("["+d_TickCounter+"]:Stationary: NOT ENOUGH RUNWAY!");
//					n_WindUpGoal = navCon.exitPosition+distanceNeeded+0.1f;
//				}
//				else
//				{
//					FighterState.LeftKeyHold = true;
//				}
//			}
		}
		else if(n_ActiveConnection.exitVel.x>0) // If velocity is positive.
		{
			if(FighterState.Vel.x>=0) // if x velocity is also positive
			{
				if(distFromExitPoint>0) // if the destination is in the other direction, turn around.
				{
					float distanceNeeded = NavGetWindupDist((n_ActiveConnection.minExitVel), 0);
					if(d_aiDebug){print("["+d_TickCounter+"]:Distance needed: "+distanceNeeded+", distToExitPoint: "+distFromExitPoint);}
					n_WindUpGoal = navCon.exitPosition+distanceNeeded-0.1f;
					if(d_aiDebug){print("["+d_TickCounter+"]:ExitVel Positive, Vel Positive, passed exitpoint and must turn around.");}
				}
				else if(FighterState.Vel.x>n_ActiveConnection.maxExitVel-0.5f) 		// If going too fast.
				{
					if(d_aiDebug){print("["+d_TickCounter+"]:ExitVel Positive, Vel Positive, speed too fast");}
					// Do nothing! You will slow down over time. Replace this with a deceleration distance equation in the future for better results.
				}
				else if(FighterState.Vel.x>n_ActiveConnection.minExitVel+0.5f) // If going fast enough, keep accelerating up to the max velocity anyway.
				{
					if(d_aiDebug){print("["+d_TickCounter+"]:ExitVel Positive, Vel Positive, speed good");}
					FighterState.RightKeyHold = true;
				}
				else 														// If not going fast enough.
				{
					float distanceNeeded = NavGetWindupDist((n_ActiveConnection.minExitVel), FighterState.Vel.x);
					if(d_aiDebug){print("["+d_TickCounter+"]:Distance needed: "+distanceNeeded+", distToExitPoint: "+distFromExitPoint);}

					if(distanceNeeded<distFromExitPoint) // If the player does not have enough room to reach speed, set a starting point further back.
					{
						if(d_aiDebug){print("["+d_TickCounter+"]:NOT ENOUGH RUNWAY!");}
						if(d_aiDebug){print("["+d_TickCounter+"]:ExitVel Positive, Vel Positive, speed too slow, setting windup point");}
						n_WindUpGoal = navCon.exitPosition+distanceNeeded-0.1f;
					}
					else
					{
						if(d_aiDebug){print("["+d_TickCounter+"]:ExitVel Positive, Vel Positive, speed too slow, accelerating");}
						FighterState.RightKeyHold = true; // If the player has enough room to reach speed, just run.
					}
				}
			}
			else// if(FighterState.Vel.x>0) // if x velocity is negative, away from the exit point, decelerate to turn around.
			{
				FighterState.RightKeyHold = true;
				float distanceForStop = NavGetWindupDist(0, FighterState.Vel.x);
				if(d_aiDebug){print("["+d_TickCounter+"]:ExitVel Positive, Vel Negative, reversing direction");}
				if(d_aiDebug){print("["+d_TickCounter+"]:DistanceForStop: "+distanceForStop);}
			}
//			else // x velocity stationary
//			{
////				float distanceNeeded = NavGetWindupDist((n_ActiveConnection.exitVel.x-n_ActiveConnection.exitVelRange));
////				print("["+d_TickCounter+"]:Distance needed: "+distanceNeeded+", distToExitPoint: "+distToExitPoint);
////				if(distanceNeeded>distToExitPoint)
////				{
////					print("["+d_TickCounter+"]:Stationary: NOT ENOUGH RUNWAY!");
////					n_WindUpGoal = navCon.exitPosition+distanceNeeded-0.1f;
////				}
////				else
////				{
////					FighterState.RightKeyHold = true;
////				}
//			}
		}
		else
		{
			if(d_aiDebug){print("exitVelocity is zero");}
			if(distFromExitPoint>0)
			{
				FighterState.LeftKeyHold = true;
			}
			else if(distFromExitPoint<0)
			{
				FighterState.RightKeyHold = true;
			}
		}
		
		if( (Mathf.Abs(distFromExitPoint)<=navCon.exitPositionRange))
		{
			if((FighterState.Vel.x>=navCon.minExitVel) && (FighterState.Vel.x<=navCon.maxExitVel))
			{
				if(d_aiDebug){print("["+d_TickCounter+"]:EXITING.");}
				n_AtExit = true;
			}
			else
			{
				if(d_aiDebug){print("["+d_TickCounter+"]:min("+navCon.minExitVel+"), Vel("+FighterState.Vel.x+"), max("+navCon.maxExitVel+")");}
				if(d_aiDebug){print("["+d_TickCounter+"]:Vel["+FighterState.Vel.x+"] <= minExitVel("+navCon.maxExitVel+")");}
			}
		}
	}

	protected void NavTraverse(NavConnection navCon)
	{
		if(navCon==null)
		{
			if(d_aiDebug){print("["+d_TickCounter+"]:navCon NULL! ARGH");}
		}

		if(n_DestSurf==null)
		{
			if(d_aiDebug){print("["+d_TickCounter+"]:n_destsurf NULL!");}
		}

		n_TraversalTimer += Time.fixedDeltaTime;
		if(navCon==null)
		{
			EndTraverse(false);
		}
		if(n_TraversalTimer>navCon.traversaltimeout)
		{
			if(d_aiDebug){print("["+d_TickCounter+"]:Traversal timeout! Recalculating...");}
			EndTraverse(false);
			return;
		}
		if((n_CurrentSurfID!=-1) && (n_CurrentSurfID != n_DestSurfID)&&(n_HasJumped))
		{
			if(d_aiDebug){print("["+d_TickCounter+"]:Bad landing! Attempting again...");}
			EndTraverse(false);
			return;
		}



		Vector2 directionVector = navCon.dest.LinToWorldPos(navCon.destPosition)-this.GetFootPosition();
//		if( directionVector.magnitude<=0.5f )
//		{
//			if(d_aiDebug){print("n_DestSurf.LinToWorldPos(navCon.destPosition) = "+n_DestSurf.LinToWorldPos(navCon.destPosition));}
//			if(d_aiDebug){print("this.GetFootPosition() = "+this.GetFootPosition());}
//			EndTraverse(true);
//			return;
//		}
			
		if((n_CurrentSurfID==navCon.dest.id)&&(n_CurrentSurfID!=-1))
		{
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
					if(d_aiDebug){print("Nav jump activated");}
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
		if(d_aiDebug){print("linDist="+linDist);}
		float linDistBelowThreshold = ((m_TractionChangeT*m_TractionChangeT)/(m_LinearAccelRate/Time.fixedDeltaTime));
		if(d_aiDebug){print("linDistBelowThreshold="+linDistBelowThreshold);}
		float linDistAboveThreshold = linDist-linDistBelowThreshold;

		float fastDist = ((m_TractionChangeT*m_TractionChangeT)/(m_StartupAccelRate/Time.fixedDeltaTime));


		if(Mathf.Abs(reqVel)<m_TractionChangeT)
		{
			distance = ((reqVel*reqVel)/(m_StartupAccelRate/Time.fixedDeltaTime));
		}
		else
		{
			distance = linDistAboveThreshold+fastDist; // The acceleration is a piecewise function, so the bottom of lindist needs to be chopped off and replaced by fastdist result.
		}

		if(reqVel>0)
		{
			distance *= -1;
		}

		if(Mathf.Abs(reqVel)<m_TractionChangeT)
		if(d_aiDebug){print("["+d_TickCounter+"]:Windup Distance: "+distance+" for vel of "+reqVel);}
		else
		if(d_aiDebug){print("["+d_TickCounter+"]:Windup Distance: "+distance+" for vel of "+reqVel+". AboveThreshDist: "+linDistAboveThreshold+", BelowThreshDist="+fastDist);}

		return distance;
	}

	public float NavGetWindupDist(float reqVel, float curVel) // reqVel is the required additional velocity. The distance returned is how much
	{
		float distanceNeeded = NavGetWindupDist(reqVel);
		float distanceNotNeeded = NavGetWindupDist(curVel);
		float finalDistance = distanceNeeded-distanceNotNeeded;
		if(d_aiDebug){print("["+d_TickCounter+"]:Total Windup Distance: "+distanceNeeded+"-"+distanceNotNeeded+"="+(distanceNeeded-distanceNotNeeded)+" to reach "+reqVel+" kph from "+curVel+" kph.");}
		return finalDistance;
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
		FighterState.CurVigor = g_MaxVigor;
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
			FighterState.EtherKeyPress = false;
		}

		//################################################################################
		//### ALL INPUT AFTER THIS POINT IS DISABLED WHEN THE PLAYER IS INCAPACITATED. ###
		//################################################################################

		FighterState.PlayerMouseVector = FighterState.MouseWorldPos-Vec2(this.transform.position);
		if(!(FighterState.LeftKeyHold||FighterState.RightKeyHold) || (FighterState.LeftKeyHold && FighterState.RightKeyHold))
		{
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
			CtrlH = -1;
		}
		else
		{
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

		if(FighterState.DownKeyHold&&m_Grounded)
		{
			m_Kneeling = true;
			CtrlH = 0;
		}


		if(FighterState.JumpKeyPress&&(m_Grounded||m_Ceilinged||m_LeftWalled||m_RightWalled))
		{
			if(m_Kneeling)
			{
				//EtherJump(FighterState.PlayerMouseVector.normalized);
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
			FighterState.LeftClickRelease = false;
		}

		if(FighterState.RightClickHold)
		{
			FighterState.Stance = 2;
		}

		if(FighterState.DisperseKeyPress)
		{
			EtherPulse();
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
			EtherPulse();
			FighterState.DisperseKeyPress = false;
		}


		// Once the input has been processed, set the press inputs to false so they don't run several times before being changed by update() again. 
		// FixedUpdate can run multiple times before Update refreshes, so a keydown input can be registered as true multiple times before update changes it back to false, instead of just the intended one time.
		FighterState.LeftClickPress = false; 	
		FighterState.RightClickPress = false;
		FighterState.EtherKeyPress = false;				
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
