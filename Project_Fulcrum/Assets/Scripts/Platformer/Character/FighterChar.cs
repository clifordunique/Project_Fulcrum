﻿using UnityEngine.UI;
using System;
using UnityEngine;
using EZCameraShake;
using UnityEngine.Networking;

/*
 * AUTHOR'S NOTES:
 * 
 * Naming Shorthand:
 * If a variable ends with: A, it is short for:B
 * 		A	|	B
 * --------------------------
 * 		T	| 	Threshold 	(Value at which something changes)
 * 		M	| 	Multiplier 	(Used to scale things. Usually a value between 0 and 1)
 * 		H	|	Horizontal 	(Axis)
 * 		V	|	Vertical	(Axis)
 * 		G	|	Ground		(Direction)
 * 		C	|	Ceiling		(Direction)
 * 		L	|	Left		(Direction)
 * 		R	|	Right		(Direction)
*/

[System.Serializable]
public class FighterChar : NetworkBehaviour
{        	
	//############################################################################################################################################################################################################
	// HANDLING VARIABLES
	//###########################################################################################################################################################################
	#region MOVEMENT HANDLING
	[Header("Movement Tuning:")]
	[SerializeField] protected float m_MinSpeed = 10f; 							// The instant starting speed while moving
	[SerializeField] protected float m_MaxRunSpeed = 200f;						// The fastest the fighter can travel along land.
	[Range(0,2)][SerializeField] protected float m_Acceleration = 0.6f;    			// Speed the fighter accelerates at
	[SerializeField] protected float m_VJumpForce = 40f;                  		// Amount of vertical force added when the fighter jumps.
	[SerializeField] protected float m_HJumpForce = 5f;  							// Amount of horizontal force added when the fighter jumps.
	[SerializeField] protected float m_WallVJumpForce = 20f;                  	// Amount of vertical force added when the fighter walljumps.
	[SerializeField] protected float m_WallHJumpForce = 10f;  					// Amount of horizontal force added when the fighter walljumps.
	[SerializeField] protected float m_TractionChangeT = 20f;						// Threshold where movement changes from exponential to linear acceleration.  
	[SerializeField] protected float m_WallTractionT = 20f;						// Speed threshold at which wallsliding traction changes.
	[Range(0,5)][SerializeField] protected float m_LinearStopRate = 2f; 			// How fast the fighter decelerates when changing direction.
	[Range(0,5)][SerializeField] protected float m_LinearSlideRate = 0.35f;		// How fast the fighter decelerates with no input.
	[Range(0,5)][SerializeField] protected float m_LinearOverSpeedRate = 0.1f;	// How fast the fighter decelerates when running too fast.
	[Range(0,5)][SerializeField] protected float m_LinearAccelRate = 0.4f;		// How fast the fighter accelerates with input.
	[Range(1,89)][SerializeField] protected float m_ImpactDecelMinAngle = 20f;	// Any impacts at sharper angles than this will start to slow the fighter down. Reaches full halt at m_ImpactDecelMaxAngle.
	[Range(1,89)][SerializeField] protected float m_ImpactDecelMaxAngle = 80f;	// Any impacts at sharper angles than this will result in a full halt. DO NOT SET THIS LOWER THAN m_ImpactDecelMinAngle!!
	[Range(1,89)][SerializeField] protected float m_TractionLossMinAngle = 45f; 	// Changes the angle at which steeper angles start to linearly lose traction, and eventually starts slipping back down. Default of 45 degrees.
	[Range(45,90)][SerializeField] protected float m_TractionLossMaxAngle = 78f; 	// Changes the angle at which fighter loses ALL traction, and starts slipping back down. Default of 90 degrees.
	[Range(0,2)][SerializeField] protected float m_SlippingAcceleration = 1f;  	// Changes how fast the fighter slides down overly steep slopes.
	[Range(0.5f,3)][SerializeField] protected float m_SurfaceClingTime = 1f; 		// How long the fighter can cling to walls before gravity takes over.
	[Range(20,70)][SerializeField] protected float m_ClingReqGForce = 50f;		// This is the amount of impact GForce required for a full-duration ceiling cling.
	[ReadOnlyAttribute]protected Vector2 m_ExpiredNormal;							// This is the normal of the last surface clung to, to make sure the fighter doesn't repeatedly cling the same surface after clingtime expires.
	[ReadOnlyAttribute]protected float m_TimeSpentHanging = 0f;					// Amount of time the fighter has been clung to a wall.
	[ReadOnlyAttribute]protected float m_MaxTimeHanging = 0f;						// Max time the fighter can cling to current wall.
	[Range(0,0.5f)][SerializeField] protected float m_MaxEmbed = 0.02f;			// How deep into objects the character can be before actually colliding with them. MUST BE GREATER THAN m_MinEmbed!!!
	[Range(0.01f,0.4f)][SerializeField] protected float m_MinEmbed = 0.01f; 		// How deep into objects the character will sit by default. A value of zero will cause physics errors because the fighter is not technically *touching* the surface.
	[Space(10)]
	[SerializeField] protected float m_ZonJumpForcePerCharge = 5f; 				// How much force does each Zon Charge add to the jump power?
	[SerializeField] protected float m_ZonJumpForceBase = 40f; 					// How much force does a no-power Zon jump have?
	[Space(10)]
	[SerializeField] public float m_VelPunchT = 60f; 								// Impact threshold for Velocity Punch trigger
	[SerializeField] protected float m_SlamT = 100f; 								// Impact threshold for slam
	[SerializeField] protected float m_CraterT = 200f; 								// Impact threshold for crater
	[SerializeField] protected float m_GuardSlamT = 200f; 							// Guarded Impact threshold for slam
	[SerializeField] protected float m_GuardCraterT = 400f; 						// Guarded Impact threshold for crater


	#endregion
	//############################################################################################################################################################################################################
	// OBJECT REFERENCES
	//###########################################################################################################################################################################
	#region OBJECT REFERENCES
	[Header("Character Components:")]
	[SerializeField] protected Light o_TempLight;      			// Reference to a spotlight attached to the character.
	[SerializeField] public FighterAudio o_FighterAudio;		// Reference to the character's audio handler.
	[SerializeField] public GameObject p_DebugMarker;			// Reference to a sprite prefab used to mark locations ingame during development.
	[SerializeField] public VelocityPunch o_VelocityPunch;		// Reference to the velocity punch visual effect entity attached to the character.
	[SerializeField] public GameObject p_AirPunchPrefab;		// Reference to the air punch attack prefab.
	[SerializeField] public GameObject p_ShockEffectPrefab;		// Reference to the shock visual effect prefab.
	[SerializeField] public GameObject p_AirBurstPrefab;		// Reference to the air burst prefab, which is a circular windforce.
	protected Animator o_Anim;           						// Reference to the character's animator component.
	protected Rigidbody2D o_Rigidbody2D;						// Reference to the character's physics body.
	[SerializeField] protected SpriteRenderer o_SpriteRenderer;	// Reference to the character's sprite renderer.
	#endregion
	//############################################################################################################################################################################################################
	// PHYSICS&RAYCASTING
	//###########################################################################################################################################################################
	#region PHYSICS&RAYCASTING
	[SerializeField] protected LayerMask mask;			// Mask used for terrain collisions.
	[SerializeField] protected LayerMask fighterMask;	// Mask used for fighter collisions.

	protected Transform m_GroundFoot; 		// Ground collider.
	protected Vector2 m_GroundFootOffset; 	// Ground raycast endpoint.
	protected float m_GroundFootLength;		// Ground raycast length.

	protected Transform m_CeilingFoot; 		// Ceiling collider, middle.
	protected Vector2 m_CeilingFootOffset;	// Ceiling raycast endpoint.
	protected float m_CeilingFootLength;		// Ceiling raycast length.

	protected Transform m_LeftSide; 			// LeftWall collider.
	protected Vector2 m_LeftSideOffset;		// LeftWall raycast endpoint.
	protected float m_LeftSideLength;			// LeftWall raycast length.

	protected Transform m_RightSide;  		// RightWall collider.
	protected Vector2 m_RightSideOffset;		// RightWall raycast endpoint.
	protected float m_RightSideLength;		// RightWall raycast length.

	protected Vector2 m_GroundNormal;			// Vector with slope of Ground.
	protected Vector2 m_CeilingNormal;		// Vector with slope of Ceiling.
	protected Vector2 m_LeftNormal;			// Vector with slope of LeftWall.
	protected Vector2 m_RightNormal;			// Vector with slope of RightWall.

	[Header("Fighter State:")]
	[SerializeField][ReadOnlyAttribute]protected float m_IGF; 					//"Instant G-Force" of the impact this frame.
	[SerializeField][ReadOnlyAttribute]protected float m_CGF; 					//"Continuous G-Force" over time.
	[SerializeField][ReadOnlyAttribute]protected float m_RemainingVelM;		//Remaining velocity proportion after an impact. Range: 0-1.
	[SerializeField][ReadOnlyAttribute]public float m_Spd;			//Current speed.
	[SerializeField][ReadOnlyAttribute]protected Vector2 initialVel;			//Velocity at the start of the physics frame.
	[SerializeField][ReadOnlyAttribute]protected Vector2 m_DistanceTravelled;	//(x,y) distance travelled on current frame. Inversely proportional to m_RemainingMovement.
	[SerializeField][ReadOnlyAttribute]protected Vector2 m_RemainingMovement; 	//Remaining (x,y) movement after impact.
	[SerializeField][ReadOnlyAttribute]protected bool groundContact;			//True when touching surface.
	[SerializeField][ReadOnlyAttribute]protected bool ceilingContact;			//True when touching surface.
	[SerializeField][ReadOnlyAttribute]protected bool leftSideContact;			//True when touching surface.
	[SerializeField][ReadOnlyAttribute]protected bool rightSideContact;			//True when touching surface.
	[Space(10)]
	[SerializeField][ReadOnlyAttribute]protected bool m_Grounded;
	[SerializeField][ReadOnlyAttribute]protected bool m_Ceilinged; 
	[SerializeField][ReadOnlyAttribute]protected bool m_LeftWalled; 
	[SerializeField][ReadOnlyAttribute]protected bool m_RightWalled;
	[Space(10)]
	[SerializeField][ReadOnlyAttribute]protected bool m_GroundBlocked;
	[SerializeField][ReadOnlyAttribute]protected bool m_CeilingBlocked; 
	[SerializeField][ReadOnlyAttribute]protected bool m_LeftWallBlocked; 
	[SerializeField][ReadOnlyAttribute]protected bool m_RightWallBlocked; 
	[Space(10)]
	[SerializeField][ReadOnlyAttribute]protected bool m_SurfaceCling;
	[SerializeField][ReadOnlyAttribute]protected bool m_Airborne;
	[SerializeField][ReadOnlyAttribute]protected bool m_Landing;
	[SerializeField][ReadOnlyAttribute]protected bool m_Kneeling;
	[SerializeField][ReadOnlyAttribute]protected bool m_Impact;
	protected Vector3 lastSafePosition;										//Used to revert player position if they get totally stuck in something.

	#endregion
	//##########################################################################################################################################################################
	// FIGHTER INPUT VARIABLES
	//###########################################################################################################################################################################
	#region FIGHTERINPUT
	[Header("Networked Variables:")]
	[SerializeField][ReadOnlyAttribute] protected FighterState FighterState;// Struct holding all networked fighter info.
	protected int CtrlH; 													// Tracks horizontal keys pressed. Values are -1 (left), 0 (none), or 1 (right). 
	protected int CtrlV; 													// Tracks vertical keys pressed. Values are -1 (down), 0 (none), or 1 (up).
	protected bool facingDirection; 										// True means right, false means left.

	#endregion
	//############################################################################################################################################################################################################
	// DEBUGGING VARIABLES
	//##########################################################################################################################################################################
	#region DEBUGGING
	protected int errorDetectingRecursionCount; 			//Iterates each time recursive trajectory correction executes on the current frame. Not currently used.
	[Header("Debug:")]
	[SerializeField] protected bool autoRunLeft; 			// When true, fighter will behave as if the left key is pressed.
	[SerializeField] protected bool autoRunRight; 			// When true, fighter will behave as if the right key is pressed.
	[SerializeField] protected bool autoJump;				// When true, fighter jumps instantly on every surface.
	[SerializeField] protected bool antiTunneling = true;	// When true, fighter will be pushed out of objects they are stuck in.
	[SerializeField] protected bool noGravity;				// Disable gravity.
	[SerializeField] protected bool showVelocityIndicator;	// Shows a line tracing the character's movement path.
	[SerializeField] protected bool showContactIndicators;	// Shows fighter's surface-contact raycasts, which turn green when touching something.
	[SerializeField] protected bool recoverFromFullEmbed=true;// When true and the fighter is fully stuck in something, teleports fighter to last good position.
	[SerializeField] protected bool d_ClickToKnockFighter;	// When true and you left click, the fighter is propelled toward where you clicked.
	[SerializeField] protected bool d_SendCollisionMessages;// When true, the console prints messages related to collision detection
	protected LineRenderer m_DebugLine; 					// Part of above indicators.
	protected LineRenderer m_GroundLine;					// Part of above indicators.		
	protected LineRenderer m_CeilingLine;					// Part of above indicators.		
	protected LineRenderer m_LeftSideLine;					// Part of above indicators.		
	protected LineRenderer m_RightSideLine;					// Part of above indicators.		
	#endregion
	//############################################################################################################################################################################################################
	// VISUAL&SOUND VARIABLES
	//###########################################################################################################################################################################
	#region VISUALS&SOUND
	[Header("Visuals And Sound:")]
	[SerializeField][Range(0,10)]protected float v_ReversingSlideT = 5;	// How fast the fighter must be going to go into a slide posture when changing directions.
	//[SerializeField]protected float v_CameraZoom; 					// Amount of camera zoom.
	[SerializeField][Range(0,3)]protected int v_FighterGlow;			// Amount of fighter "energy glow" effect.
	#endregion 
	//############################################################################################################################################################################################################
	// GAMEPLAY VARIABLES
	//###########################################################################################################################################################################
	#region GAMEPLAY VARIABLES
	[Header("Gameplay:")]
	[SerializeField] protected int g_ZonLevel;						// Level of fighter Zon Power.
	[SerializeField] protected int g_ZonJumpCharge;					// Level of power channelled into current jump.
	[SerializeField] protected bool g_VelocityPunching;				// True when fighter is channeling a velocity fuelled punch.
	[SerializeField] protected bool g_VelocityPunchExpended;			// True when fighter's VelocityPunch has been used up.
	[SerializeField][ReadOnlyAttribute] protected int g_ZonStance;	// Which stance is the fighter in? -1 = no stance.

	[SerializeField] protected int g_MaxHealth = 100;				// Max health.
	[SerializeField] protected int g_MinSlamDMG = 5;				// Min damage a slam impact can deal.
	[SerializeField] protected int g_MaxSlamDMG = 30;				// Max damage a slam impact can deal.	
	[SerializeField] protected int g_MinCrtrDMG = 30;				// Min damage a crater impact can deal.
	[SerializeField] protected int g_MaxCrtrDMG = 60;				// Max damage a crater impact can deal.
	//[SerializeField] protected bool g_FallStunned = false;		// True when the fighter is recoiling after falling.
	[SerializeField] protected float g_CurFallStun = 0;				// How much longer the fighter is stunned after a fall. When this value is > 0  the fighter is stunned.
	[SerializeField] protected float g_MinSlamStun = 0.5f;			// Min duration the fighter can be stunned from slamming the ground.
	[SerializeField] protected float g_MaxSlamStun = 1.5f;			// Max duration the fighter can be stunned from slamming the ground.
	[SerializeField] protected float g_MinCrtrStun = 1.5f;			// Max duration the fighter can be stunned from smashing the ground hard.
	[SerializeField] protected float g_MaxCrtrStun = 3f;			// Max duration the fighter can be stunned from smashing the ground hard.
	[SerializeField] public int g_IsInGrass;						// True when greater than 1. The number equates to how many grass tiles the fighter is touching.
	[SerializeField] public bool g_fighterCollision = true;			// While true, this fighter will collide with other fighters
	protected bool isAPlayer;

	#endregion 


	//########################################################################################################################################
	// CORE FUNCTIONS
	//########################################################################################################################################
	#region CORE FUNCTIONS
	protected virtual void Awake()
	{
		FighterAwake();
	}

	protected virtual void FixedUpdate()
	{
		FixedUpdateProcessInput();
		FixedUpdatePhysics();
		FixedUpdateLogic();
		FixedUpdateAnimation();
		FighterState.RightClick = false;
		FighterState.LeftClick = false;
		FighterState.ZonKey = false;
		FighterState.DisperseKey = false;
	}

	protected virtual void Update()
	{
		UpdateInput();
	}

	protected virtual void LateUpdate()
	{
		//CameraControl();
	}
	#endregion
	//###################################################################################################################################
	// CUSTOM FUNCTIONS
	//###################################################################################################################################
	#region CUSTOM FUNCTIONS

	protected void ThrowPunch(Vector2 aimDirection)
	{
		float randomness1 = UnityEngine.Random.Range(-0.2f,0.2f);
		float randomness2 = UnityEngine.Random.Range(-0.2f,0.2f);
		float xTransform = 1f;
		float yTransform = 1f;

		if(aimDirection.x<0)
		{
			facingDirection = false;
			xTransform = -1f;
		}
		else
		{
			facingDirection = true;
		}

		Quaternion punchAngle = Quaternion.LookRotation(aimDirection);
		punchAngle.x = 0;
		punchAngle.y = 0;
		GameObject newAirPunch = (GameObject)Instantiate(p_AirPunchPrefab, this.transform.position, punchAngle, this.transform);

		if(randomness1>0)
		{
			yTransform = -1f;
			newAirPunch.GetComponentInChildren<SpriteRenderer>().sortingLayerName = "Background";	
		}

		Vector3 theLocalScale = new Vector3 (xTransform, yTransform, 1f);
		Vector3 theLocalTranslate = new Vector3(randomness1,randomness2, 0);

		newAirPunch.transform.localScale = theLocalScale;
		newAirPunch.transform.Translate(theLocalTranslate);
		newAirPunch.transform.Rotate(new Vector3(0,0,randomness1));

		newAirPunch.GetComponentInChildren<AirPunch>().aimDirection = aimDirection;
		newAirPunch.GetComponentInChildren<AirPunch>().punchThrower = this;

		o_FighterAudio.PunchSound();
	}

	protected virtual void Death()
	{
		if(FighterState.DevMode)
		{
			FighterState.CurHealth = 100;
			return;
		}
		FighterState.Dead = true;
		o_Anim.SetBool("Dead", true);
		o_SpriteRenderer.color = Color.red;
	}

	protected virtual void Respawn()
	{
		FighterState.Dead = false;
		FighterState.CurHealth = g_MaxHealth;
		o_Anim.SetBool("Dead", false);
		o_SpriteRenderer.color = Color.white;
	}

	protected virtual void SpawnShockEffect(Vector2 hitDirection)
	{
		//print("Hit direction = "+hitDirection);
		if(Math.Abs(hitDirection.x) < 0.01f){hitDirection.x = 0;} //Duct tape fix
		if(Math.Abs(hitDirection.y) < 0.01f){hitDirection.y = 0;} //Duct tape fix
		Quaternion ImpactAngle = Quaternion.LookRotation(hitDirection);
		ImpactAngle.x = 0;
		ImpactAngle.y = 0;
		if(hitDirection.x == 0) //Duct tape fix
		{
			if(hitDirection.y < 0)
			{
				ImpactAngle.eulerAngles = new Vector3(0, 0, -90);
			}
			else if(hitDirection.y > 0)
			{
				ImpactAngle.eulerAngles = new Vector3(0, 0, 90);
			}
			else
			{
				print("ERROR: IMPACT DIRECTION OF (0,0)");
			}
		}
		GameObject newShockEffect = (GameObject)Instantiate(p_ShockEffectPrefab, this.transform.position, ImpactAngle);

		float xTransform = 3f;
		if(hitDirection.x<0)
		{
			xTransform = -3f;
		}
		Vector3 theLocalScale = new Vector3 (xTransform, 3f, 1f);

		newShockEffect.transform.localScale = theLocalScale;
	}

	protected virtual void FixedUpdatePhysics()
	{
		m_DistanceTravelled = Vector2.zero;
		initialVel = FighterState.Vel;

		if(m_Grounded)
		{//Locomotion!
			Traction(CtrlH);
		}
		else if(m_RightWalled)
		{//Wallsliding!
			WallTraction(CtrlH,m_RightNormal);
		}
		else if(m_LeftWalled)
		{
			WallTraction(CtrlH,m_LeftNormal);
		}
		else if(!noGravity)
		{//Gravity!
			AirControl(CtrlH);
			FighterState.Vel = new Vector2 (FighterState.Vel.x, FighterState.Vel.y - 1f);
			m_Ceilinged = false;
		}	

		errorDetectingRecursionCount = 0; //Used for WorldCollizion(); (note: colliZion is used to help searches for the keyword 'collision' by filtering out extraneous matches)

		//print("Velocity before Collizion: "+FighterState.Vel);
		//print("Position before Collizion: "+this.transform.position);

		m_RemainingVelM = 1f;
		m_RemainingMovement = FighterState.Vel*Time.fixedDeltaTime;
		Vector2 startingPos = this.transform.position;

		//print("m_RemainingMovement before collision: "+m_RemainingMovement);

		if(g_fighterCollision)
		{
			DynamicCollision();
		}

		WorldCollision();

		//print("Per frame velocity at end of Collizion() "+FighterState.Vel*Time.fixedDeltaTime);
		//print("Velocity at end of Collizion() "+FighterState.Vel);
		//print("Per frame velocity at end of updatecontactnormals "+FighterState.Vel*Time.fixedDeltaTime);
		//print("m_RemainingMovement after collision: "+m_RemainingMovement);

		m_DistanceTravelled = new Vector2(this.transform.position.x-startingPos.x,this.transform.position.y-startingPos.y);
		//print("m_DistanceTravelled: "+m_DistanceTravelled);
		//print("m_RemainingMovement: "+m_RemainingMovement);
		//print("m_RemainingMovement after removing m_DistanceTravelled: "+m_RemainingMovement);

		if(initialVel.magnitude>0)
		{
			m_RemainingVelM = (((initialVel.magnitude*Time.fixedDeltaTime)-m_DistanceTravelled.magnitude)/(initialVel.magnitude*Time.fixedDeltaTime));
		}
		else
		{
			m_RemainingVelM = 1f;
		}

		//print("m_RemainingVelM: "+m_RemainingVelM);
		//print("movement after distance travelled: "+m_RemainingMovement);
		//print("Speed this frame: "+FighterState.Vel.magnitude);

		m_RemainingMovement = FighterState.Vel*m_RemainingVelM*Time.fixedDeltaTime;

		//print("Corrected remaining movement: "+m_RemainingMovement);

		m_Spd = FighterState.Vel.magnitude;

		Vector2 deltaV = FighterState.Vel-initialVel;
		m_IGF = deltaV.magnitude;
		m_CGF += m_IGF;
		if(m_CGF>=1){m_CGF --;}
		if(m_CGF>=10){m_CGF -= (m_CGF/10);}

		//if(m_CGF>=200)
		//{
		//	//m_CGF = 0f;
		//	print("m_CGF over limit!!");	
		//}

		if(m_Impact)
		{
			if(m_IGF >= m_CraterT)
			{
				g_VelocityPunchExpended = true;
				Crater();
			}
			else if(m_IGF >= m_SlamT)
			{
				g_VelocityPunchExpended = true;
				Slam();
			}
			else if(m_IGF >= m_VelPunchT&&g_VelocityPunching)
			{
				g_VelocityPunchExpended = true;
				Slam();
			}
			else
			{
				o_FighterAudio.LandingSound(m_IGF);
			}
		}
	
//		FighterState.FinalPos = new Vector2(this.transform.position.x+m_RemainingMovement.x, this.transform.position.y+m_RemainingMovement.y);
//
//		this.transform.position = FighterState.FinalPos;
//		UpdateContactNormals(true);
//
//		DebugUCN();

		this.transform.position = new Vector2(this.transform.position.x+m_RemainingMovement.x, this.transform.position.y+m_RemainingMovement.y);

		UpdateContactNormals(true);

		FighterState.FinalPos = this.transform.position;

		if(FighterState.DevMode)
		{
			DebugUCN();
		}

		//print("Per frame velocity at end of physics frame: "+FighterState.Vel*Time.fixedDeltaTime);
		//print("m_RemainingMovement at end of physics frame: "+m_RemainingMovement);
		//print("Pos at end of physics frame: "+this.transform.position);
		//print("##############################################################################################");
		//print("FinaL Pos: " + this.transform.position);
		//print("FinaL Vel: " + FighterState.Vel);
		//print("Speed at end of frame: " + FighterState.Vel.magnitude);

	}

	protected virtual void FixedUpdateProcessInput()
	{
		m_Impact = false;
		m_Landing = false;
		m_Kneeling = false;
		g_ZonStance = -1;

		FighterState.PlayerMouseVector = FighterState.MouseWorldPos-Vec2(this.transform.position);
		if(FighterState.LeftClick&&(FighterState.DevMode||d_ClickToKnockFighter))
		{
			FighterState.Vel += FighterState.PlayerMouseVector*10;
			print("Leftclick detected");
			FighterState.LeftClick = false;
		}	
	}

	protected virtual void FixedUpdateLogic()
	{

		if(g_CurFallStun > 0)
		{
			g_CurFallStun -= Time.deltaTime;
			if(g_CurFallStun < 0)
			{
				g_CurFallStun = 0;
			}
		}
		if(FighterState.CurHealth <= 0)
		{
			Death();
		}
		else
		{
			if(FighterState.Dead)
			{
				Respawn();
			}
		}
	}


	protected virtual void FixedUpdateAnimation()
	{
		v_FighterGlow = g_ZonLevel;
		if (v_FighterGlow > 7){v_FighterGlow = 7;}

		if(v_FighterGlow>2)
		{
			o_TempLight.color = new Color(1,1,0,1);
			o_TempLight.intensity = (v_FighterGlow)+(UnityEngine.Random.Range(-1f,1f));
		}
		else
		{
			o_TempLight.color = new Color(1,1,1,1);
			o_TempLight.intensity = 2;
		}

		o_Anim.SetBool("Walled", false);

		if(m_LeftWalled&&!m_Grounded)
		{
			o_Anim.SetBool("Walled", true);
			facingDirection = false;
		}

		if(m_RightWalled&&!m_Grounded)
		{
			o_Anim.SetBool("Walled", true);
			facingDirection = true;
		}

		if (!facingDirection) //If facing left
		{
			//print("FACING LEFT!   "+h)
			//o_CharSprite.transform.localScale = new Vector3 (-1f, 1f, 1f);
			o_SpriteRenderer.flipX = true;
			if(FighterState.Vel.x > 0 && m_Spd >= v_ReversingSlideT)
			{
				o_Anim.SetBool("Crouch", true);
			}
			else
			{
				o_Anim.SetBool("Crouch", false);
			}
		} 
		else //If facing right
		{
			//print("FACING RIGHT!   "+h);

			//o_CharSprite.transform.localScale = new Vector3 (1f, 1f, 1f);
			o_SpriteRenderer.flipX = false;
			if(FighterState.Vel.x < 0 && m_Spd >= v_ReversingSlideT)
			{
				o_Anim.SetBool("Crouch", true);
			}
			else
			{
				o_Anim.SetBool("Crouch", false);
			}
		}

		if(m_Kneeling||(g_CurFallStun>0))
		{
			o_Anim.SetBool("Crouch", true);
		}

		Vector3[] debugLineVector = new Vector3[3];

		debugLineVector[0].x = -m_DistanceTravelled.x;
		debugLineVector[0].y = -(m_DistanceTravelled.y+(m_GroundFootLength-m_MaxEmbed));
		debugLineVector[0].z = 0f;

		debugLineVector[1].x = 0f;
		debugLineVector[1].y = -(m_GroundFootLength-m_MaxEmbed);
		debugLineVector[1].z = 0f;

		debugLineVector[2].x = m_RemainingMovement.x;
		debugLineVector[2].y = (m_RemainingMovement.y)-(m_GroundFootLength-m_MaxEmbed);
		debugLineVector[2].z = 0f;

		m_DebugLine.SetPositions(debugLineVector);

		o_Anim.SetFloat("Speed", FighterState.Vel.magnitude);

		if(FighterState.Vel.magnitude >= m_TractionChangeT )
		{
			m_DebugLine.endColor = Color.white;
			m_DebugLine.startColor = Color.white;
		}   
		else
		{   
			m_DebugLine.endColor = Color.blue;
			m_DebugLine.startColor = Color.blue;
		}

		float multiplier = 1; // Animation playspeed multiplier that increases with higher velocity

		if(FighterState.Vel.magnitude > 20.0f)
		{
			multiplier = ((FighterState.Vel.magnitude - 20) / 20)+1;
		}

		o_Anim.SetFloat("Multiplier", multiplier);

		if (!m_Grounded&&!m_LeftWalled&!m_RightWalled) 
		{
			o_Anim.SetBool("Ground", false);
		}
		else
		{
			o_Anim.SetBool("Ground", true);
		}
	}

	protected virtual void UpdateInput()
	{
		if(Input.GetMouseButtonDown(0))
		{
			FighterState.LeftClick = true;
		}

		if(Input.GetMouseButtonDown(1))
		{
			//FighterState.RightClick = true;
		}

		Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		FighterState.MouseWorldPos = Vec2(mousePoint);
	}

	protected virtual void FighterAwake()
	{
		FighterState.CurHealth = 100;					// Current health.
		FighterState.Dead = false;						// True when the fighter's health reaches 0 and they die.
		Vector2 fighterOrigin = new Vector2(this.transform.position.x, this.transform.position.y);
		m_DebugLine = GetComponent<LineRenderer>();
		o_VelocityPunch = GetComponentInChildren<VelocityPunch>();

		m_GroundFoot = transform.Find("MidFoot");
		m_GroundLine = m_GroundFoot.GetComponent<LineRenderer>();
		m_GroundFootOffset.x = m_GroundFoot.position.x-fighterOrigin.x;
		m_GroundFootOffset.y = m_GroundFoot.position.y-fighterOrigin.y;
		m_GroundFootLength = m_GroundFootOffset.magnitude;

		m_CeilingFoot = transform.Find("CeilingFoot");
		m_CeilingLine = m_CeilingFoot.GetComponent<LineRenderer>();
		m_CeilingFootOffset.x = m_CeilingFoot.position.x-fighterOrigin.x;
		m_CeilingFootOffset.y = m_CeilingFoot.position.y-fighterOrigin.y;
		m_CeilingFootLength = m_CeilingFootOffset.magnitude;

		m_LeftSide = transform.Find("LeftSide");
		m_LeftSideLine = m_LeftSide.GetComponent<LineRenderer>();
		m_LeftSideOffset.x = m_LeftSide.position.x-fighterOrigin.x;
		m_LeftSideOffset.y = m_LeftSide.position.y-fighterOrigin.y;
		m_LeftSideLength = m_LeftSideOffset.magnitude;

		m_RightSide = transform.Find("RightSide");
		m_RightSideLine = m_RightSide.GetComponent<LineRenderer>();
		m_RightSideOffset.x = m_RightSide.position.x-fighterOrigin.x;
		m_RightSideOffset.y = m_RightSide.position.y-fighterOrigin.y;
		m_RightSideLength = m_RightSideOffset.magnitude;


		o_Anim = this.GetComponent<Animator>();
		o_Rigidbody2D = GetComponent<Rigidbody2D>();
		//o_SpriteRenderer = this.GetComponent<SpriteRenderer>();

		lastSafePosition = new Vector2(0,0);
		m_RemainingMovement = new Vector2(0,0);
		m_RemainingVelM = 1f;
		//print(m_RemainingMovement);


		if(!(showVelocityIndicator||FighterState.DevMode)){
			m_DebugLine.enabled = false;
		}

		if(!(showContactIndicators||FighterState.DevMode))
		{
			m_CeilingLine.enabled = false;
			m_GroundLine.enabled = false;
			m_RightSideLine.enabled = false;
			m_LeftSideLine.enabled = false;
		}
	}

	protected Vector2 Vec2(Vector3 inputVector)
	{
		return new Vector2(inputVector.x, inputVector.y);
	}

	protected void Crater() // Triggered when character impacts anything REALLY hard.
	{
		float Multiplier = (m_IGF+m_CraterT)/(2*m_CraterT);
		//print(Multiplier);
		if(Multiplier >=2){Multiplier = 2;}
		float Magnitude = Multiplier;
		float Roughness = 10f;
		float FadeOutTime = 2.5f;
		float FadeInTime = 0f;
		Vector3 RotInfluence = new Vector3(1,1,1);
		Vector3 PosInfluence = new Vector3(0.15f,0.15f,0.15f);
		CameraShaker.Instance.ShakeOnce(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);

		o_FighterAudio.CraterSound(m_IGF, m_CraterT, 1000f);

		SpawnShockEffect(this.initialVel.normalized);

		if(g_VelocityPunching)
		{
			g_VelocityPunchExpended = true;
		}

		float linScaleModifier = ((m_IGF-m_CraterT)/(1000f-m_CraterT));
		if(linScaleModifier > 1){linScaleModifier = 1;}
		float damagedealt = g_MinCrtrDMG+((g_MaxCrtrDMG-g_MinCrtrDMG)*linScaleModifier); // Damage dealt scales linearly from minDMG to maxDMG, reaching max damage at a 1000 kph impact.
		float stunTime = g_MinCrtrStun+((g_MaxCrtrStun-g_MinCrtrStun)*linScaleModifier); // Stun duration scales linearly from ...

		g_CurFallStun = stunTime;				 // Stunned for stunTime.
		FighterState.CurHealth -= (int)damagedealt;		 // Damaged by fall.
		if(FighterState.CurHealth < 0){FighterState.CurHealth = 0;}

//		GameObject newAirBurst = (GameObject)Instantiate(p_AirBurstPrefab, this.transform.position, Quaternion.identity);
//		newAirBurst.GetComponentInChildren<AirBurst>().Create(true, 30+70*linScaleModifier, 0.4f, m_IGF); 					//Set the parameters of the shockwave.
//		newAirBurst.name = "Shockwave";
		GameObject newWindGust = (GameObject)Instantiate(p_AirBurstPrefab, this.transform.position, Quaternion.identity);
		newWindGust.GetComponentInChildren<AirBurst>().Create(false, 0, 30+70*linScaleModifier, 0.8f, linScaleModifier*3, m_IGF); 		//Set the parameters of the afterslam wind.
		newWindGust.name = "AirGust";
	}

	protected void Slam() // Triggered when character impacts anything too hard.
	{
		float Multiplier = (m_IGF+m_SlamT)/(2*m_SlamT);
		//print(Multiplier);
		if(Multiplier >=2){Multiplier = 2;}
		float Magnitude = 0.5f;
		float Roughness = 20f;
		float FadeOutTime = 0.6f*Multiplier;
		float FadeInTime = 0f;
		float posM = 0.3f*Multiplier;
		Vector3 RotInfluence = new Vector3(0,0,0);
		Vector3 PosInfluence = new Vector3(posM,posM,0);
		CameraShaker.Instance.ShakeOnce(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);

		o_FighterAudio.SlamSound(m_IGF, m_SlamT, m_CraterT);

		if(g_VelocityPunching)
		{
			g_VelocityPunchExpended = true;
			SpawnShockEffect(this.initialVel.normalized);
		}

		float linScaleModifier = ((m_IGF-m_SlamT)/(m_CraterT-m_SlamT));
		float damagedealt = g_MinSlamDMG+((g_MaxSlamDMG-g_MinSlamDMG)*linScaleModifier); // Damage dealt scales linearly from minDMG to maxDMG, as you go from the min Slam Threshold to min Crater Threshold (impact speed)
		float stunTime = g_MinSlamStun+((g_MaxSlamStun-g_MinSlamStun)*linScaleModifier); // Stun duration scales linearly from ...

		g_CurFallStun = stunTime;				 // Stunned for stunTime.
		if(damagedealt >= 0)
		{
			FighterState.CurHealth -= (int)damagedealt;		 // Damaged by fall.
		}
		if(FighterState.CurHealth < 0){FighterState.CurHealth = 0;}

		GameObject newAirBurst = (GameObject)Instantiate(p_AirBurstPrefab, this.transform.position, Quaternion.identity);
		newAirBurst.GetComponentInChildren<AirBurst>().Create(true, 30*linScaleModifier, linScaleModifier*0.8f, m_IGF*2); //Set the parameters of the shockwave.
	}

	protected void DynamicCollision()
	{
		#region fightercollisions
		// FIGHTER-FIGHTER COLLISION TESTING IS SEPERATE AND PRECEDES WORLD COLLISIONS
		float crntSpeed = FighterState.Vel.magnitude*Time.fixedDeltaTime; //Current speed.
		RaycastHit2D[] fighterCollision = Physics2D.RaycastAll(this.transform.position, FighterState.Vel, crntSpeed, fighterMask);

		foreach(RaycastHit2D hit in fighterCollision)
		{
			if(hit.collider.gameObject != this.gameObject)
			{
				print("HIT: "+hit.collider.transform.gameObject);//GetComponent<>());
				if(hit.collider.GetComponent<FighterChar>())
				{
					CollideWithFighter(hit.collider.GetComponent<FighterChar>());
				}
			}
		}
		//		ToLeftWall(predictedLoc[2]);
		//		DirectionChange(m_LeftNormal);

		#endregion
	}

	protected void WorldCollision()	// Handles all collisions with terrain geometry (and fighters).
	{
		//print ("Collision->m_Grounded=" + m_Grounded);
		float crntSpeed = FighterState.Vel.magnitude*Time.fixedDeltaTime; //Current speed.
		//print("DC Executing");
		errorDetectingRecursionCount++;

		if(errorDetectingRecursionCount >= 5)
		{
			throw new Exception("Your recursion code is not working!");
			//return;
		}

		if(FighterState.Vel.x > 0.001f)
		{
			m_LeftWallBlocked = false;
		}

		if(FighterState.Vel.x < -0.001f)
		{
			m_RightWallBlocked = false;
		}

		#region worldcollision raytesting

		Vector2 adjustedBot = m_GroundFoot.position; // AdjustedBot marks the end of the ground raycast, but 0.02 shorter.
		adjustedBot.y += m_MaxEmbed;

		Vector2 adjustedTop = m_CeilingFoot.position; // AdjustedTop marks the end of the ceiling raycast, but 0.02 shorter.
		adjustedTop.y -= m_MaxEmbed;

		Vector2 adjustedLeft = m_LeftSide.position; // AdjustedLeft marks the end of the left wall raycast, but 0.02 shorter.
		adjustedLeft.x += m_MaxEmbed;

		Vector2 adjustedRight = m_RightSide.position; // AdjustedRight marks the end of the right wall raycast, but 0.02 shorter.
		adjustedRight.x -= m_MaxEmbed;

		//RaycastHit2D groundCheck = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength, mask);
		RaycastHit2D[] predictedLoc = new RaycastHit2D[4];
		//These raycasts fire from the 4 edges of the player collider in the direction of travel, effectively forming a projection of the player. This is a form of continuous collision detection.
		predictedLoc[0] = Physics2D.Raycast(adjustedBot, FighterState.Vel, crntSpeed, mask); 	// Ground
		predictedLoc[1] = Physics2D.Raycast(adjustedTop, FighterState.Vel, crntSpeed, mask); 	// Ceiling
		predictedLoc[2] = Physics2D.Raycast(adjustedLeft, FighterState.Vel, crntSpeed, mask); 	// Left
		predictedLoc[3] = Physics2D.Raycast(adjustedRight, FighterState.Vel, crntSpeed, mask);	// Right  

		float[] rayDist = new float[4];
		rayDist[0] = predictedLoc[0].distance; // Ground dist
		rayDist[1] = predictedLoc[1].distance; // Ceiling dist
		rayDist[2] = predictedLoc[2].distance; // Left dist
		rayDist[3] = predictedLoc[3].distance; // Right dist


		int shortestVertical = -1;
		int shortestHorizontal = -1;
		int shortestRaycast = -1;

		//Shortest non-zero vertical collision.
		if(rayDist[0] != 0 && rayDist[1]  != 0)
		{
			if(rayDist[0] <= rayDist[1])
			{
				shortestVertical = 0;
			}
			else
			{
				shortestVertical = 1;
			}
		}
		else if(rayDist[0] != 0)
		{
			shortestVertical = 0;
		}
		else if(rayDist[1] != 0)
		{
			shortestVertical = 1;
		}

		//Shortest non-zero horizontal collision.
		if(rayDist[2] != 0 && rayDist[3]  != 0)
		{
			if(rayDist[2] <= rayDist[3])
			{
				shortestHorizontal = 2;
			}
			else
			{
				shortestHorizontal = 3;
			}
		}
		else if(rayDist[2] != 0)
		{
			shortestHorizontal = 2;
		}
		else if(rayDist[3] != 0)
		{
			shortestHorizontal = 3;
		}

		//non-zero, shortest distance of all four colliders. This selects the collider that hits an obstacle the earliest. Zero is excluded because a non-colliding raycast returns a distance of 0.
		if(shortestVertical >= 0 && shortestHorizontal >= 0)
		{
			//print("Horiz dist="+shortestHorizontal);
			//print("Verti dist="+shortestVertical);
			//print("Verti-horiz="+(shortestVertical-shortestHorizontal));
			if(rayDist[shortestVertical] < rayDist[shortestHorizontal])
			{
				shortestRaycast = shortestVertical;
			}
			else
			{
				shortestRaycast = shortestHorizontal;
			}
		}
		else if(shortestVertical >= 0)
		{
			//print("Shortest is vertical="+shortestVertical);
			shortestRaycast = shortestVertical;
		}
		else if(shortestHorizontal >= 0)
		{
			//print("Shortest is horizontal="+shortestHorizontal);
			shortestRaycast = shortestHorizontal;
		}
		else
		{
			//print("NOTHING?");	
		}

		//print("G="+gDist+" C="+cDist+" R="+rDist+" L="+lDist);
		//print("VDist: "+shortestDistV);
		//print("HDist: "+shortestDistH);
		//print("shortestDist: "+rayDist[shortestRaycast]);

		//Count the number of sides colliding during this movement for debug purposes.
		int collisionNum = 0;

		if(predictedLoc[0])
		{
			collisionNum++;
		}
		if(predictedLoc[1])
		{
			collisionNum++;
		}
		if(predictedLoc[2])
		{
			collisionNum++;
		}
		if(predictedLoc[3])
		{
			collisionNum++;
		}

		if(collisionNum>0)
		{
			//print("TOTAL COLLISIONS: "+collisionNum);
		}

		#endregion

		Vector2 moveDirectionNormal = Perp(FighterState.Vel.normalized);
		Vector2 invertedDirectionNormal = -moveDirectionNormal;//This is made in case one of the raycasts is inside the collider, which would cause it to return an inverted normal value.

		switch (shortestRaycast)
		{
		case -1:
			{
				//print("No collision!");
				break;
			}
		case 0://Ground collision with feet
			{
				//If you're going to hit something with your feet.
				//print("FOOT_IMPACT");
				//print("Velocity before impact: "+FighterState.Vel);

				//print("GroundDist"+predictedLoc[0].distance);
				//print("RightDist"+predictedLoc[3].distance);

				if ((moveDirectionNormal != predictedLoc[0].normal) && (invertedDirectionNormal != predictedLoc[0].normal)) 
				{ // If the slope you're hitting is different than your current slope.
					ToGround(predictedLoc[0]);
					DirectionChange(m_GroundNormal);
					return;
				}
				else 
				{
					if(invertedDirectionNormal == predictedLoc[0].normal&&d_SendCollisionMessages)
					{
						throw new Exception("INVERTED GROUND IMPACT NORMAL DETECTED!");
					}
					return;
				}
			}
		case 1:
			{

				if ((moveDirectionNormal != predictedLoc[1].normal) && (invertedDirectionNormal != predictedLoc[1].normal)) 
				{ // If the slope you're hitting is different than your current slope.
					//print("CEILINm_Impact");
					ToCeiling(predictedLoc[1]);
					DirectionChange(m_CeilingNormal);
					return;
				}
				else 
				{
					if(invertedDirectionNormal == predictedLoc[1].normal&&d_SendCollisionMessages)
					{
						throw new Exception("INVERTED CEILING IMPACT NORMAL DETECTED!");
					}
					return;
				}
			}
		case 2:
			{
				if ((moveDirectionNormal != predictedLoc[2].normal) && (invertedDirectionNormal != predictedLoc[2].normal)) 
				{ // If the slope you're hitting is different than your current slope.
					//print("LEFT_IMPACT");
					ToLeftWall(predictedLoc[2]);
					DirectionChange(m_LeftNormal);
					return;
				}
				else 
				{
					if(invertedDirectionNormal == predictedLoc[2].normal&&d_SendCollisionMessages)
					{
						throw new Exception("INVERTED LEFT IMPACT NORMAL DETECTED!");
					}
					return;
				}
			}
		case 3:
			{
				if ((moveDirectionNormal != predictedLoc[3].normal) && (invertedDirectionNormal != predictedLoc[3].normal)) 
				{ // If the slope you're hitting is different than your current slope.
					//print("RIGHT_IMPACT");
					//print("predictedLoc[3].normal=("+predictedLoc[3].normal.x+","+predictedLoc[3].normal.y+")");
					//print("moveDirectionNormal=("+moveDirectionNormal.x+","+moveDirectionNormal.y+")");
					//print("moveDirectionNormal="+moveDirectionNormal);
					if(ToRightWall(predictedLoc[3])) // If you hit something on the rightwall, change direction.
					{
						DirectionChange(m_RightNormal);
					}
					return;
				}
				else 
				{
					if(invertedDirectionNormal == predictedLoc[3].normal&&d_SendCollisionMessages)
					{
						throw new Exception("INVERTED RIGHT IMPACT NORMAL DETECTED!");
					}
					return;
				}
			}
		default:
			{
				print("ERROR: DEFAULTED.");
				break;
			}
		}
	}

	protected void Traction(float horizontalInput)
	{
		Vector2 groundPerp = Perp(m_GroundNormal);

		//print("Traction");
		//print("gp="+groundPerp);

		// This block of code makes the player treat very steep left and right surfaces as walls when they aren't going fast enough to reasonably climb them. 
		// This aims to prevent a jittering effect when the player build small amounts of speed, then hits the steeper slope and starts sliding down again 
		// as frequently as 60 times a second.
		if (this.GetSpeed() <= 0.0001f) 
		{
			//print("Hitting wall slowly, considering correction.");
			float wallSteepnessAngle;
			float groundSteepnessAngle;

			if ((m_LeftWalled) && (horizontalInput < 0)) 
			{
				//print("Trying to run up left wall slowly.");
				Vector2 wallPerp = Perp (m_LeftNormal);
				wallSteepnessAngle = Vector2.Angle (Vector2.up, wallPerp);
				if (wallSteepnessAngle == 180) 
				{
					wallSteepnessAngle = 0;
				}
				if (wallSteepnessAngle >= m_TractionLossMaxAngle) 
				{ //If the wall surface the player is running
					//print("Wall steepness of "+wallSteepnessAngle+" was too steep for speed "+this.GetSpeed()+", stopping.");
					FighterState.Vel = Vector2.zero;
					m_LeftWallBlocked = true;
				}
			} 
			else if ((m_RightWalled) && (horizontalInput > 0)) 
			{
				//print("Trying to run up right wall slowly.");
				Vector2 wallPerp = Perp (m_RightNormal);
				wallSteepnessAngle = Vector2.Angle (Vector2.up, wallPerp);
				wallSteepnessAngle = 180f - wallSteepnessAngle;
				if (wallSteepnessAngle == 180) 
				{
					wallSteepnessAngle = 0;
				}
				if (wallSteepnessAngle >= m_TractionLossMaxAngle) 
				{ //If the wall surface the player is running
					//print("Wall steepness of "+wallSteepnessAngle+" was too steep for speed "+this.GetSpeed()+", stopping.");
					FighterState.Vel = Vector2.zero;
					m_RightWallBlocked = true;
				}
			}
			else 
			{
				//print("Only hitting groundcontact, test ground steepness.");
			}



	
		}
		// End of anti-slope-jitter code.


		if(groundPerp.x > 0)
		{
			groundPerp *= -1;
		}

		float steepnessAngle = Vector2.Angle(Vector2.left,groundPerp);

		steepnessAngle = (float)Math.Round(steepnessAngle,2);
		//print("SteepnessAngle:"+steepnessAngle);

		float slopeMultiplier = 0;

		if(steepnessAngle > m_TractionLossMinAngle)
		{
			if(steepnessAngle >= m_TractionLossMaxAngle)
			{
				//print("MAXED OUT!");
				slopeMultiplier = 1;
			}
			else
			{
				slopeMultiplier = ((steepnessAngle-m_TractionLossMinAngle)/(m_TractionLossMaxAngle-m_TractionLossMinAngle));
			}

			//print("slopeMultiplier: "+ slopeMultiplier);
			//print("groundPerpY: "+groundPerpY+", slopeT: "+slopeT);
		}


		if(((m_LeftWallBlocked)&&(horizontalInput < 0)) || ((m_RightWallBlocked)&&(horizontalInput > 0)))
		{// If running at an obstruction you're up against.
			//print("Running against a wall.");
			horizontalInput = 0;
		}

		//print("Traction executing");
		float rawSpeed = FighterState.Vel.magnitude;
		//print("FighterState.Vel.magnitude"+FighterState.Vel.magnitude);

		if (horizontalInput == 0) 
		{//if not pressing any move direction, slow to zero linearly.
			//print("No input, slowing...");
			if(rawSpeed <= 0.5f)
			{
				FighterState.Vel = Vector2.zero;	
			}
			else
			{
				FighterState.Vel = ChangeSpeedLinear(FighterState.Vel, -m_LinearSlideRate);
			}
		}
		else if((horizontalInput > 0 && FighterState.Vel.x >= 0) || (horizontalInput < 0 && FighterState.Vel.x <= 0))
		{//if pressing same button as move direction, move to MAXSPEED.
			//print("Moving with keypress");
			if(rawSpeed < m_MaxRunSpeed)
			{
				//print("Rawspeed("+rawSpeed+") less than max");
				if(rawSpeed > m_TractionChangeT)
				{
					//print("LinAccel-> " + rawSpeed);
					if(FighterState.Vel.y > 0)
					{ 	// If climbing, recieve uphill movement penalty.
						FighterState.Vel = ChangeSpeedLinear(FighterState.Vel, m_LinearAccelRate*(1-slopeMultiplier));
					}
					else
					{
						FighterState.Vel = ChangeSpeedLinear(FighterState.Vel, m_LinearAccelRate);
					}
				}
				else if(rawSpeed < 0.001f)
				{
					if(slopeMultiplier<0.5)
					{
						FighterState.Vel = new Vector2((m_Acceleration)*horizontalInput*(1-slopeMultiplier), 0);
					}
					else
					{
						print("Too steep!");
					}
					//print("Starting motion. Adding " + m_Acceleration);
				}
				else
				{
					//print("ExpAccel-> " + rawSpeed);
					float eqnX = (1+Mathf.Abs((1/m_TractionChangeT )*rawSpeed));
					float curveMultiplier = 1+(1/(eqnX*eqnX)); // Goes from 1/4 to 1, increasing as speed approaches 0.

					float addedSpeed = curveMultiplier*(m_Acceleration);
					if(FighterState.Vel.y > 0)
					{ // If climbing, recieve uphill movement penalty.
						addedSpeed = curveMultiplier*(m_Acceleration)*(1-slopeMultiplier);
					}
					//print("Addedspeed:"+addedSpeed);
					FighterState.Vel = (FighterState.Vel.normalized)*(rawSpeed+addedSpeed);
					//print("FighterState.Vel:"+FighterState.Vel);
				}
			}
			else
			{
				if(rawSpeed < m_MaxRunSpeed+1)
				{
					rawSpeed = m_MaxRunSpeed;
					SetSpeed(FighterState.Vel,m_MaxRunSpeed);
				}
				else
				{
					//print("Rawspeed("+rawSpeed+") more than max.");
					FighterState.Vel = ChangeSpeedLinear (FighterState.Vel, -m_LinearOverSpeedRate);
				}
			}
		}
		else if((horizontalInput > 0 && FighterState.Vel.x < 0) || (horizontalInput < 0 && FighterState.Vel.x > 0))
		{//if pressing button opposite of move direction, slow to zero exponentially.
			if(rawSpeed > m_TractionChangeT )
			{
				//print("LinDecel");
				FighterState.Vel = ChangeSpeedLinear (FighterState.Vel, -m_LinearStopRate);
			}
			else
			{
				//print("Decelerating");
				float eqnX = (1+Mathf.Abs((1/m_TractionChangeT )*rawSpeed));
				float curveMultiplier = 1+(1/(eqnX*eqnX)); // Goes from 1/4 to 1, increasing as speed approaches 0.
				float addedSpeed = curveMultiplier*(m_Acceleration-slopeMultiplier);
				FighterState.Vel = (FighterState.Vel.normalized)*(rawSpeed-2*addedSpeed);
			}

			//float modifier = Mathf.Abs(FighterState.Vel.x/FighterState.Vel.y);
			//print("SLOPE MODIFIER: " + modifier);
			//FighterState.Vel = FighterState.Vel/(1.25f);
		}

		Vector2 downSlope = FighterState.Vel.normalized; // Normal vector pointing down the current slope!
		if (downSlope.y > 0) //Make sure the vector is descending.
		{
			downSlope *= -1;
		}



		if(downSlope == Vector2.zero)
		{
			downSlope = Vector2.down;
		}

		FighterState.Vel += downSlope*m_SlippingAcceleration*slopeMultiplier;

		//	TESTINGSLOPES
		//print("downSlope="+downSlope);
		//print("m_SlippingAcceleration="+m_SlippingAcceleration);
		//print("slopeMultiplier="+slopeMultiplier);

		//ChangeSpeedLinear(FighterState.Vel, );
		//print("PostTraction velocity: "+FighterState.Vel);
	}

	protected void AirControl(float horizontalInput)
	{
		FighterState.Vel += new Vector2(horizontalInput/20, 0);
	}



	protected void WallTraction(float horizontalInput, Vector2 wallSurface)
	{
		////////////////////
		// Variable Setup //
		////////////////////
		Vector2 wallPerp = Perp(wallSurface);

		//print("horizontalInput="+horizontalInput);

		if(wallPerp.x > 0)
		{
			wallPerp *= -1;
		}

		float steepnessAngle = Vector2.Angle(Vector2.up,wallPerp);

		if(m_RightWalled)
		{
			steepnessAngle = 180f - steepnessAngle;
		}

		if(steepnessAngle == 180)
		{
			steepnessAngle=0;
		}

		if(steepnessAngle > 90 && (wallSurface != m_ExpiredNormal)) //If the sliding surface is upside down, and hasn't already been clung to.
		{
			if(!m_SurfaceCling)
			{
				m_TimeSpentHanging = 0;
				m_MaxTimeHanging = 0;
				m_SurfaceCling = true;
				if(m_CGF >= m_ClingReqGForce)
				{
					m_MaxTimeHanging = m_SurfaceClingTime;
				}
				else
				{
					m_MaxTimeHanging = m_SurfaceClingTime*(m_CGF/m_ClingReqGForce);
				}
				//print("m_MaxTimeHanging="+m_MaxTimeHanging);
			}
			else
			{
				m_TimeSpentHanging += Time.fixedDeltaTime;
				//print("time=("+m_TimeSpentHanging+"/"+m_MaxTimeHanging+")");
				if(m_TimeSpentHanging>=m_MaxTimeHanging)
				{
					m_SurfaceCling = false;
					m_ExpiredNormal = wallSurface;
					//print("EXPIRED!");
				}
			}
		}
		else
		{
			m_SurfaceCling = false;
			m_TimeSpentHanging = 0;
			m_MaxTimeHanging = 0;
		}


		//
		// This code block is likely unnecessary
		// Anti-Jitter code for transitioning to a steep slope that is too steep to climb.
		//
		if (this.GetSpeed () <= 0.0001f) 
		{
			print ("RIDING WALL SLOWLY, CONSIDERING CORRECTION");
			if ((m_LeftWalled) && (horizontalInput < 0)) 
			{
				if (steepnessAngle >= m_TractionLossMaxAngle) { //If the wall surface the player is running
					print ("Wall steepness of " + steepnessAngle + " was too steep for speed " + this.GetSpeed () + ", stopping.");
					//FighterState.Vel = Vector2.zero;
					m_LeftWallBlocked = true;
					horizontalInput = 0;
					m_SurfaceCling = false;
				}
			} 
			else if ((m_RightWalled) && (horizontalInput > 0)) 
			{
				print ("Trying to run up right wall slowly.");
				if (steepnessAngle >= m_TractionLossMaxAngle) { //If the wall surface the player is running
					print ("Wall steepness of " + steepnessAngle + " was too steep for speed " + this.GetSpeed () + ", stopping.");
					//FighterState.Vel = Vector2.zero;
					m_RightWallBlocked = true;
					horizontalInput = 0;
					m_SurfaceCling = false;
				}
			} 
			else 
			{
				print ("Not trying to move up a wall; Continue as normal.");
			}
		}


		//print("Wall Steepness Angle:"+steepnessAngle);

		///////////////////
		// Movement code //
		///////////////////

		if(m_SurfaceCling)
		{
			if(FighterState.Vel.y > 0)
			{
				FighterState.Vel = ChangeSpeedLinear(FighterState.Vel,-0.8f);
			}
			else if(FighterState.Vel.y <= 0)
			{
				if( (horizontalInput<0 && m_LeftWalled) || (horizontalInput>0 && m_RightWalled) )
				{
					FighterState.Vel = ChangeSpeedLinear(FighterState.Vel,0.1f);
				}
				else
				{
					FighterState.Vel = ChangeSpeedLinear(FighterState.Vel,1f);
				}
			}
		}
		else
		{
			if(FighterState.Vel.y > 0)
			{
				if( (horizontalInput<0 && m_LeftWalled) || (horizontalInput>0 && m_RightWalled) ) // If pressing key toward wall direction.
				{
					FighterState.Vel.y -= 0.8f; //Decelerate slower.
				}
				else if((horizontalInput>0 && m_LeftWalled) || (horizontalInput<0 && m_RightWalled)) // If pressing key opposite wall direction.
				{
					FighterState.Vel.y -= 1.2f; //Decelerate faster.
				}
				else // If no input.
				{
					FighterState.Vel.y -= 1f; 	//Decelerate.
				}
			}
			else if(FighterState.Vel.y <= 0)
			{
				if( (horizontalInput<0 && m_LeftWalled) || (horizontalInput>0 && m_RightWalled) ) // If pressing key toward wall direction.
				{
					FighterState.Vel.y -= 0.1f; //Accelerate downward slower.
				}
				else if((horizontalInput>0 && m_LeftWalled) || (horizontalInput<0 && m_RightWalled)) // If pressing key opposite wall direction.
				{
					FighterState.Vel.y -= 1.2f; //Accelerate downward faster.
				}
				else // If no input.
				{
					FighterState.Vel.y -= 1f; 	//Accelerate downward.
				}
			}
		}
	}

	protected bool ToLeftWall(RaycastHit2D leftCheck) 
	{ //Sets the new position of the fighter and their m_LeftNormal.

		//print ("We've hit LeftWall, sir!!");
		//print ("leftCheck.normal=" + leftCheck.normal);
		//print("preleftwall Pos:" + this.transform.position);

		if (m_Airborne)
		{
			if(d_SendCollisionMessages)
			{
				print("Airborne before impact.");
			}
			m_Impact = true;
			//m_Landing = true;
		}

		Breakable hitBreakable = leftCheck.collider.transform.GetComponent<Breakable>();

		if(hitBreakable!=null&&m_Spd > 3)
		{
			print("hit a hitbreakable!");
			if(hitBreakable.RecieveHit(this)){return false;}
		}

		//m_Impact = true;
		m_LeftWalled = true;
		Vector2 setCharPos = leftCheck.point;
		setCharPos.x += (m_LeftSideLength-m_MinEmbed); //Embed slightly in wall to ensure raycasts still hit wall.
		//setCharPos.y -= m_MinEmbed;
		//print("Sent to Pos:" + setCharPos);

		this.transform.position = setCharPos;

		//print ("Final Position:  " + this.transform.position);

		RaycastHit2D leftCheck2 = Physics2D.Raycast(this.transform.position, Vector2.left, m_LeftSideLength, mask);
		if (leftCheck2) 
		{
			
		}
		else
		{
			//m_LeftWalled = false;
		}

		m_LeftNormal = leftCheck2.normal;

//		if(m_Grounded)
//		{
//			if(d_SendCollisionMessages)
//			{
//				print("LeftGroundWedge detected during left collision.");
//			}
//			OmniWedge(0,2);
//		}
//
//		if(m_Ceilinged)
//		{
//			if(d_SendCollisionMessages)
//			{
//				print("LeftCeilingWedge detected during left collision.");
//			}
//			OmniWedge(2,1);
//		}
//
//		if(m_RightWalled)
//		{
//			if(d_SendCollisionMessages)
//			{
//				print("THERE'S PROBLEMS.");
//			}
//			//OmniWedge(2,3);
//		}
		return true;
		//print ("Final Position2:  " + this.transform.position);
	}

	protected bool ToRightWall(RaycastHit2D rightCheck) 
	{ //Sets the new position of the fighter and their m_RightNormal.

		if(d_SendCollisionMessages)
		{
			print ("We've hit RightWall, sir!!");
		}
		//print ("groundCheck.normal=" + groundCheck.normal);
		//print("prerightwall Pos:" + this.transform.position);

		if (m_Airborne)
		{
			if(d_SendCollisionMessages)
			{
				print("Airborne before impact.");
			}
			m_Impact = true;
		}

		Breakable hitBreakable = rightCheck.collider.transform.GetComponent<Breakable>();

		if(hitBreakable!=null&&m_Spd > 3)
		{
			print("hit a hitbreakable!");
			if(hitBreakable.RecieveHit(this)){return false;}
		}
			
		rightSideContact = true;
		m_RightWalled = true;
		Vector2 setCharPos = rightCheck.point;
		setCharPos.x -= (m_RightSideLength-m_MinEmbed); //Embed slightly in wall to ensure raycasts still hit wall.
		//setCharPos.y -= m_MinEmbed;  //Embed slightly in ground to ensure raycasts still hit ground.

		//print("Sent to Pos:" + setCharPos);
		//print("Sent to normal:" + groundCheck.normal);

		this.transform.position = setCharPos;

		//print ("Final Position:  " + this.transform.position);

		RaycastHit2D rightCheck2 = Physics2D.Raycast(this.transform.position, Vector2.right, m_RightSideLength, mask);
		if (rightCheck2) 
		{
			
		}
		else
		{
			m_RightWalled = false;
		}

		m_RightNormal = rightCheck2.normal;
		//print ("Final Position2:  " + this.transform.position);

		if(m_Grounded)
		{
			//print("RightGroundWedge detected during right collision.");
			OmniWedge(0,3);
		}

		if(m_LeftWalled)
		{
			print("THERE'S PROBLEMS.");
			//OmniWedge(2,3);
		}

		if(m_Ceilinged)
		{
			//print("RightCeilingWedge detected during right collision.");
			OmniWedge(3,1);
		}
		return true;
	}

	protected bool ToGround(RaycastHit2D groundCheck) 
	{ //Sets the new position of the fighter and their ground normal.
		//print ("m_Grounded=" + m_Grounded);

		if (m_Airborne)
		{
			//print("Airborne before impact.");
			m_Impact = true;
			//m_Landing = true;
		}

		Breakable hitBreakable = groundCheck.collider.transform.GetComponent<Breakable>();

		if(hitBreakable!=null&&m_Spd > 3)
		{
			print("hit a hitbreakable!");
			if(hitBreakable.RecieveHit(this)){return false;}
		}

		//m_Impact = true;
		m_Grounded = true;
		Vector2 setCharPos = groundCheck.point;
		setCharPos.y = setCharPos.y+m_GroundFootLength-m_MinEmbed; //Embed slightly in ground to ensure raycasts still hit ground.
		this.transform.position = setCharPos;

		//print("Sent to Pos:" + setCharPos);
		//print("Sent to normal:" + groundCheck.normal);

		RaycastHit2D groundCheck2 = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength, mask);
		if (groundCheck2) 
		{
			//			if(antiTunneling){
			//				Vector2 surfacePosition = groundCheck2.point;
			//				surfacePosition.y += m_GroundFootLength-m_MinEmbed;
			//				this.transform.position = surfacePosition;
			//				print("Antitunneling executed during impact.");
			//			}
		}
		else
		{
			//print ("Impact Pos:  " + groundCheck.point);
			//print("Reflected back into the air!");
			//print("Transform position: " + this.transform.position);
			//print("RB2D position: " + o_Rigidbody2D.position);
			//print("Velocity : " + FighterState.Vel);
			//print("Speed : " + FighterState.Vel.magnitude);
			//print(" ");
			//print(" ");	
			m_Grounded = false;
		}

		if(groundCheck.normal.y == 0f)
		{//If vertical surface
			//throw new Exception("Existence is suffering");
			if(d_SendCollisionMessages)
			{
				print("GtG VERTICAL :O");
			}
		}

		m_GroundNormal = groundCheck2.normal;

//		if(m_Ceilinged)
//		{
//			if(d_SendCollisionMessages)
//			{
//				print("CeilGroundWedge detected during ground collision.");
//			}
//			OmniWedge(0,1);
//		}
//
//		if(m_LeftWalled)
//		{
//			if(d_SendCollisionMessages)
//			{
//				print("LeftGroundWedge detected during ground collision.");
//			}
//			OmniWedge(0,2);
//		}
//
//		if(m_RightWalled)
//		{
//			if(d_SendCollisionMessages)
//			{
//				print("RightGroundWedge detected during groundcollision.");
//			}
//			OmniWedge(0,3);
//		}

		//print ("Final Position2:  " + this.transform.position);
		return true;
	}

	protected bool ToCeiling(RaycastHit2D ceilingCheck) 
	{ //Sets the new position of the fighter when they hit the ceiling.

		//float testNumber = ceilingCheck.normal.y/ceilingCheck.normal.x;
		//print(testNumber);
		//print ("We've hit ceiling, sir!!");
		//print ("ceilingCheck.normal=" + ceilingCheck.normal);

		if (m_Airborne)
		{
			if(d_SendCollisionMessages)
			{
				print("Airborne before impact.");
			}
			//			m_Landing = true;
			m_Impact = true;
		}


		Breakable hitBreakable = ceilingCheck.collider.transform.GetComponent<Breakable>();

		if(hitBreakable!=null&&m_Spd > 3)
		{
			print("hit a hitbreakable!");
			if(hitBreakable.RecieveHit(this)){return false;}
		}

		//m_Impact = true;
		m_Ceilinged = true;
		Vector2 setCharPos = ceilingCheck.point;
		setCharPos.y -= (m_GroundFootLength-m_MinEmbed); //Embed slightly in ceiling to ensure raycasts still hit ceiling.
		this.transform.position = setCharPos;

		RaycastHit2D ceilingCheck2 = Physics2D.Raycast(this.transform.position, Vector2.up, m_GroundFootLength, mask);
		if (ceilingCheck2) 
		{
			//			if(antiTunneling){
			//				Vector2 surfacePosition = ceilingCheck2.point;
			//				surfacePosition.y -= (m_CeilingFootLength-m_MinEmbed);
			//				this.transform.position = surfacePosition;
			//			}
		}
		else
		{
			if(d_SendCollisionMessages)
			{
				print("Ceilinged = false?");
			}
			m_Ceilinged = false;
		}

		if(ceilingCheck.normal.y == 0f)
		{//If vertical surface
			//throw new Exception("Existence is suffering");
			if(d_SendCollisionMessages)
			{
				print("CEILING VERTICAL :O");
			}
		}

		m_CeilingNormal = ceilingCheck2.normal;

		if(m_Grounded)
		{
			if(d_SendCollisionMessages)
			{
				print("CeilGroundWedge detected during ceiling collision.");
			}
			OmniWedge(0,1);
		}

		if(m_LeftWalled)
		{
			if(d_SendCollisionMessages)
			{
				print("LeftCeilWedge detected during ceiling collision.");
			}
			OmniWedge(2,1);
		}

		if(m_RightWalled)
		{
			if(d_SendCollisionMessages)
			{
				print("RightGroundWedge detected during ceiling collision.");
			}
			OmniWedge(3,1);
		}
		//print ("Final Position2:  " + this.transform.position);
		return true;
	}

	protected bool CollideWithFighter(FighterChar fighterCollidedWith) 
	{ // Handles collisions with another Fighter.
		if(!IsVelocityPunching()){return false;}

		Vector2 velocitee = GetVelocity();
		float speed = velocitee.magnitude;

		//this.gameObject.SetActive(false);

		fighterCollidedWith.InstantForce(velocitee, this.GetSpeed()*0.75f);
		fighterCollidedWith.TakeDamage((int)(25+(75*(speed/1000))));
		print("Fighter recieved a blow of force: "+speed+", dealing damage of: "+(int)(25+(75*(speed/1000))));
		m_Impact = true;
		this.SetSpeed(this.GetSpeed()*0.25f);

		return true;
	}

	protected Vector2 ChangeSpeedMult(Vector2 inputVelocity, float multiplier)
	{
		Vector2 newVelocity;
		float speed = inputVelocity.magnitude*multiplier;
		Vector2 direction = inputVelocity.normalized;
		newVelocity = direction * speed;
		return newVelocity;
	}

	protected Vector2 ChangeSpeedLinear(Vector2 inputVelocity, float changeAmount)
	{
		Vector2 newVelocity;
		float speed = inputVelocity.magnitude+changeAmount;
		Vector2 direction = inputVelocity.normalized;
		newVelocity = direction * speed;
		return newVelocity;
	}

	protected void DirectionChange(Vector2 newNormal)
	{
		//print("DirectionChange");
		m_ExpiredNormal = new Vector2(0,0); //Used for wallslides. This resets the surface normal that wallcling is set to ignore.

		Vector2 initialDirection = FighterState.Vel.normalized;
		Vector2 newPerp = Perp(newNormal);
		Vector2 AdjustedVel;

		float initialSpeed = FighterState.Vel.magnitude;
		//print("Speed before : " + initialSpeed);
		float testNumber = newPerp.y/newPerp.x;
		//print(testNumber);
		//print("newPerp="+newPerp);
		//print("initialDirection="+initialDirection);
		if(float.IsNaN(testNumber))
		{
			print("IT'S NaN BRO LoLoLOL XD");
			//throw new Exception("NaN value.");
			//print("X = "+ newNormal.x +", Y = " + newNormal.y);
		}

		if((initialDirection == newPerp)||initialDirection == Vector2.zero)
		{
			//print("same angle");
			return;
		}

		float impactAngle = Vector2.Angle(initialDirection,newPerp);
		//print("TrueimpactAngle: " +impactAngle);
		//print("InitialDirection: "+initialDirection);
		//print("GroundDirection: "+newPerp);


		impactAngle = (float)Math.Round(impactAngle,2);

		if(impactAngle >= 180)
		{
			impactAngle = 180f - impactAngle;
		}

		if(impactAngle > 90)
		{
			impactAngle = 180f - impactAngle;
		}

		//print("impactAngle: " +impactAngle);

		float projectionVal;
		if(newPerp.sqrMagnitude == 0)
		{
			projectionVal = 0;
		}
		else
		{
			projectionVal = Vector2.Dot(FighterState.Vel, newPerp)/newPerp.sqrMagnitude;
		}


		//print("P"+projectionVal);
		AdjustedVel = newPerp * projectionVal;
		//print("A"+AdjustedVel);

		if(FighterState.Vel == Vector2.zero)
		{
			//FighterState.Vel = new Vector2(h, FighterState.Vel.y);
		}
		else
		{
			try
			{
				FighterState.Vel = AdjustedVel;
				//print("FighterState.Vel====>"+FighterState.Vel);

			}
			catch(Exception e)
			{
				print(e);
				print("newPerp="+newPerp);
				print("projectionVal"+projectionVal);
				print("adjustedVel"+AdjustedVel);
			}
		}

		//Speed loss from impact angle handling beyond this point

		float speedLossMult = 1; // The % of speed retained, based on sharpness of impact angle. A direct impact = full stop.

		if(impactAngle <= m_ImpactDecelMinAngle)
		{ // Angle lower than min, no speed penalty.
			speedLossMult = 1;
		}
		else if(impactAngle < m_ImpactDecelMaxAngle)
		{ // In the midrange, administering momentum loss on a curve leading from min to max.
			speedLossMult = 1-Mathf.Pow((impactAngle-m_ImpactDecelMinAngle)/(m_ImpactDecelMaxAngle-m_ImpactDecelMinAngle),2); // See Workflowy notes section for details on this formula.
		}
		else
		{ // Angle beyond max, momentum halted. 
			speedLossMult = 0;
			m_Impact = true;
		}

		if(initialSpeed <= 2f)
		{ // If the fighter is near stationary, do not remove any velocity because there is no impact!
			speedLossMult = 1;
		}

		//print("SPLMLT " + speedLossMult);

		SetSpeed(FighterState.Vel, initialSpeed*speedLossMult);
		//print("Final Vel " + FighterState.Vel);
		//print ("DirChange Vel:  " + FighterState.Vel);
	}

	protected void OmniWedge(int lowerContact, int upperContact)
	{//Executes when the fighter is moving into a corner and there isn't enough room to fit them. It halts the fighter's momentum and sets off a blocked-direction flag.

		print("OmniWedge("+lowerContact+","+upperContact+")");

		RaycastHit2D lowerHit;
		Vector2 lowerDirection = Vector2.down;
		float lowerLength = m_GroundFootLength;

		RaycastHit2D upperHit;
		Vector2 upperDirection = Vector2.up;
		float upperLength = m_CeilingFootLength;


		switch(lowerContact)
		{
		case 0: //lowercontact is ground
			{
				lowerDirection = Vector2.down;
				lowerLength = m_GroundFootLength;
				break;
			}
		case 1: //lowercontact is ceiling
			{
				throw new Exception("ERROR: Ceiling cannot be lower contact.");
			}
		case 2: //lowercontact is left
			{
				print("Omniwedge: lowercontact is left");
				lowerDirection = Vector2.left;
				lowerLength = m_LeftSideLength;
				break;
			}
		case 3: //lowercontact is right
			{
				print("Omniwedge: lowercontact is right");
				lowerDirection = Vector2.right;
				lowerLength = m_RightSideLength;
				break;
			}
		default:
			{
				throw new Exception("ERROR: DEFAULTED ON LOWERHIT.");
			}
		}

		lowerHit = Physics2D.Raycast(this.transform.position, lowerDirection, lowerLength, mask);

		float embedDepth;
		Vector2 gPara; //lowerpara, aka groundparallel
		Vector2 cPara; //upperpara, aka ceilingparallel
		Vector2 correctionVector = new Vector2(0,0);

		if(!lowerHit)
		{
			//throw new Exception("Bottom not wedged!");
			print("Bottom not wedged!");
			//gPara.x = m_GroundNormal.x;
			//gPara.y = m_GroundNormal.y;
			return;
		}
		else
		{
			gPara = Perp(lowerHit.normal);
			Vector2 groundPosition = lowerHit.point;
			if(lowerContact == 0) //ground contact
			{
				groundPosition.y += (m_GroundFootLength-m_MinEmbed);
			}
			else if(lowerContact == 1) //ceiling contact
			{
				throw new Exception("CEILINGCOLLIDER CAN'T BE LOWER CONTACT");
			}
			else if(lowerContact == 2) //left contact
			{
				groundPosition.x += (m_LeftSideLength-m_MinEmbed);
			}
			else if(lowerContact == 3) //right contact
			{
				groundPosition.x -= (m_RightSideLength-m_MinEmbed);
			}

			this.transform.position = groundPosition;
			//print("Hitting bottom, shifting up!");
		}

		switch(upperContact)
		{
		case 0: //uppercontact is ground
			{
				throw new Exception("FLOORCOLLIDER CAN'T BE UPPER CONTACT");
			}
		case 1: //uppercontact is ceiling
			{
				upperDirection = Vector2.up;
				upperLength = m_CeilingFootLength;
				break;
			}
		case 2: //uppercontact is left
			{
				print("Omniwedge: uppercontact is left");
				upperDirection = Vector2.left;
				upperLength = m_LeftSideLength;
				break;
			}
		case 3: //uppercontact is right
			{
				print("Omniwedge: uppercontact is right");
				upperDirection = Vector2.right;
				upperLength = m_RightSideLength;
				break;
			}
		default:
			{
				throw new Exception("ERROR: DEFAULTED ON UPPERHIT.");
			}
		}

		upperHit = Physics2D.Raycast(this.transform.position, upperDirection, upperLength, mask);
		embedDepth = upperLength-upperHit.distance;

		if(!upperHit)
		{
			//throw new Exception("Top not wedged!");
			cPara = Perp(upperHit.normal);
			if(d_SendCollisionMessages)
			{
				print("Top not wedged!");
			}
			return;
		}
		else
		{
			//print("Hitting top, superunwedging..."); 
			cPara = Perp(upperHit.normal);
		}

		//print("Embedded ("+embedDepth+") units into the ceiling");

		//Rounding the perpendiculars to 4 decimal places to eliminate error prone edge-cases and floating point imprecision.
		gPara.x = Mathf.Round(gPara.x * 10000f) / 10000f;
		gPara.y = Mathf.Round(gPara.y * 10000f) / 10000f;

		cPara.x = Mathf.Round(cPara.x * 10000f) / 10000f;
		cPara.y = Mathf.Round(cPara.y * 10000f) / 10000f;

		float cornerAngle = Vector2.Angle(cPara,gPara);

		//print("Ground Perp = " + gPara);
		//print("Ceiling Perp = " + cPara);
		//print("cornerAngle = " + cornerAngle);

		Vector2 cParaTest = cPara;
		Vector2 gParaTest = gPara;

		if(cParaTest.x < 0)
		{
			cParaTest *= -1;
		}
		if(gParaTest.x < 0)
		{
			gParaTest *= -1;
		}

		//print("gParaTest = " + gParaTest);
		//print("cParaTest= " + cParaTest);

		float convergenceValue = cParaTest.y-gParaTest.y;
		//print("ConvergenceValue =" + convergenceValue);

		if(lowerContact == 2 || upperContact == 2){convergenceValue = 1;}; // PLACEHOLDER CODE! It just sets it to converging left when touching left contact.
		if(lowerContact == 3 || upperContact == 3){convergenceValue =-1;}; // PLACEHOLDER CODE! It just sets it to converging right when touching right contact.

		if(cornerAngle >= 90f)
		{
			if(convergenceValue > 0)
			{
				print("Left wedge!");
				correctionVector = SuperUnwedger(cPara, gPara, true, embedDepth);
				m_LeftWallBlocked = true;
			}
			else if(convergenceValue < 0)
			{
				//print("Right wedge!");
				correctionVector = SuperUnwedger(cPara, gPara, false, embedDepth);
				m_RightWallBlocked = true;
			}
			else
			{
				throw new Exception("CONVERGENCE VALUE OF ZERO ON CORNER!");
			}
			FighterState.Vel = new Vector2(0f, 0f);
		}
		else
		{
			print("Obtuse wedge angle detected!");
			correctionVector = (upperDirection*(-(embedDepth-m_MinEmbed)));
		}

		this.transform.position = new Vector2((this.transform.position.x + correctionVector.x), (this.transform.position.y + correctionVector.y));
	}

	protected Vector2 Perp(Vector2 input)
	{
		Vector2 output;
		output.x = input.y;
		output.y = -input.x;
		return output;
	}		

	protected void UpdateContactNormals(bool posCorrection) // UCN - Updates the present-time state of the player's contact with surrounding world geometry. Corrects the player's position if it is embedded in geometry, and gathers information about where the player can move.
	{
		m_Grounded = false;
		m_Ceilinged = false;
		m_LeftWalled = false;
		m_RightWalled = false;
		m_Airborne = false;

		groundContact = false;
		ceilingContact = false;
		leftSideContact = false;
		rightSideContact = false;

		m_GroundLine.endColor = Color.red;
		m_GroundLine.startColor = Color.red;
		m_CeilingLine.endColor = Color.red;
		m_CeilingLine.startColor = Color.red;
		m_LeftSideLine.endColor = Color.red;
		m_LeftSideLine.startColor = Color.red;
		m_RightSideLine.endColor = Color.red;
		m_RightSideLine.startColor = Color.red;

		RaycastHit2D[] directionContacts = new RaycastHit2D[4];
		directionContacts[0] = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength, mask); 	// Ground
		directionContacts[1] = Physics2D.Raycast(this.transform.position, Vector2.up, m_CeilingFootLength, mask);  	// Ceiling
		directionContacts[2] = Physics2D.Raycast(this.transform.position, Vector2.left, m_LeftSideLength, mask); 	// Left
		directionContacts[3] = Physics2D.Raycast(this.transform.position, Vector2.right, m_RightSideLength, mask);	// Right  

		if (directionContacts[0]) 
		{
			groundContact = true;
			m_GroundLine.endColor = Color.green;
			m_GroundLine.startColor = Color.green;
			m_GroundNormal = directionContacts[0].normal;
			m_Grounded = true;
		} 

		if (directionContacts[1]) 
		{
			ceilingContact = true;
			m_CeilingLine.endColor = Color.green;
			m_CeilingLine.startColor = Color.green;
			m_CeilingNormal = directionContacts[1].normal;
			m_Ceilinged = true;
		} 


		if (directionContacts[2])
		{
			m_LeftNormal = directionContacts[2].normal;
			leftSideContact = true;
			m_LeftSideLine.endColor = Color.green;
			m_LeftSideLine.startColor = Color.green;
			m_LeftWalled = true;
		} 

		if (directionContacts[3])
		{
			m_RightNormal = directionContacts[3].normal;
			rightSideContact = true;
			m_RightSideLine.endColor = Color.green;
			m_RightSideLine.startColor = Color.green;
			m_RightWalled = true;
		} 

		if(!(m_Grounded&&m_Ceilinged))
		{
			if(!m_RightWalled)
			{
				m_RightWallBlocked = false;
			}
			if(!m_LeftWalled)
			{
				m_LeftWallBlocked = false;
			}
		}

		if(antiTunneling&&posCorrection)
		{
			AntiTunneler(directionContacts);
		}
		if(!(m_Grounded||m_Ceilinged||m_LeftWalled||m_RightWalled))
		{
			m_Airborne = true;	
		}
	}

	protected void DebugUCN() //Like normal UCN but doesn't affect anything and prints results.
	{
		print("DEBUG_UCN");
		m_GroundLine.endColor = Color.red;
		m_GroundLine.startColor = Color.red;
		m_CeilingLine.endColor = Color.red;
		m_CeilingLine.startColor = Color.red;
		m_LeftSideLine.endColor = Color.red;
		m_LeftSideLine.startColor = Color.red;
		m_RightSideLine.endColor = Color.red;
		m_RightSideLine.startColor = Color.red;

		RaycastHit2D[] directionContacts = new RaycastHit2D[4];
		directionContacts[0] = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength, mask); 	// Ground
		directionContacts[1] = Physics2D.Raycast(this.transform.position, Vector2.up, m_CeilingFootLength, mask);  	// Ceiling
		directionContacts[2] = Physics2D.Raycast(this.transform.position, Vector2.left, m_LeftSideLength, mask); 	// Left
		directionContacts[3] = Physics2D.Raycast(this.transform.position, Vector2.right, m_RightSideLength, mask);	// Right  

		if (directionContacts[0]) 
		{
			m_GroundLine.endColor = Color.green;
			m_GroundLine.startColor = Color.green;
		} 

		if (directionContacts[1]) 
		{
			m_CeilingLine.endColor = Color.green;
			m_CeilingLine.startColor = Color.green;
		} 


		if (directionContacts[2])
		{
			m_LeftSideLine.endColor = Color.green;
			m_LeftSideLine.startColor = Color.green;
		} 

		if (directionContacts[3])
		{
			m_RightSideLine.endColor = Color.green;
			m_RightSideLine.startColor = Color.green;
		} 

		bool[] isEmbedded = {false, false, false, false};
		int contactCount = 0;
		if(groundContact){contactCount++;}
		if(ceilingContact){contactCount++;}
		if(leftSideContact){contactCount++;}
		if(rightSideContact){contactCount++;}

		int embedCount = 0;
		if(groundContact && ((m_GroundFootLength-directionContacts[0].distance)>=0.011f))	{ print("Embedded in grnd by amount: "+((m_GroundFootLength-directionContacts[0].distance)-m_MinEmbed)); embedCount++;} //If embedded too deep in this surface.
		if(ceilingContact && ((m_CeilingFootLength-directionContacts[1].distance)>=0.011f))	{ print("Embedded in ceil by amount: "+((m_CeilingFootLength-directionContacts[1].distance)-m_MinEmbed)); embedCount++;} //If embedded too deep in this surface.
		if(leftSideContact && ((m_LeftSideLength-directionContacts[2].distance)>=0.011f))	{ print("Embedded in left by amount: "+((m_LeftSideLength-directionContacts[2].distance)-m_MinEmbed)); embedCount++;} //If embedded too deep in this surface.
		if(rightSideContact && ((m_RightSideLength-directionContacts[3].distance)>=0.011f))	{ print("Embedded in rigt by amount: "+((m_RightSideLength-directionContacts[3].distance)-m_MinEmbed)); embedCount++;} //If embedded too deep in this surface.

		print(contactCount+" sides touching, "+embedCount+" sides embedded");
	}


	protected void AntiTunneler(RaycastHit2D[] contacts)
	{
		bool[] isEmbedded = {false, false, false, false};
		int contactCount = 0;
		if(groundContact){contactCount++;}
		if(ceilingContact){contactCount++;}
		if(leftSideContact){contactCount++;}
		if(rightSideContact){contactCount++;}

		int embedCount = 0;
		if(groundContact && ((m_GroundFootLength-contacts[0].distance)>=0.011f))	{isEmbedded[0]=true; embedCount++;} //If embedded too deep in this surface.
		if(ceilingContact && ((m_CeilingFootLength-contacts[1].distance)>=0.011f))	{isEmbedded[1]=true; embedCount++;} //If embedded too deep in this surface.
		if(leftSideContact && ((m_LeftSideLength-contacts[2].distance)>=0.011f))	{isEmbedded[2]=true; embedCount++;} //If embedded too deep in this surface.
		if(rightSideContact && ((m_RightSideLength-contacts[3].distance)>=0.011f))	{isEmbedded[3]=true; embedCount++;} //If embedded too deep in this surface.

		switch(contactCount)
		{
		case 0: //No embedded contacts. Save this position as the most recent valid one and move on.
			{
				//print("No embedding! :)");
				lastSafePosition = this.transform.position;
				break;
			}
		case 1: //One side is embedded. Simply push out to remove it.
			{
				//print("One side embed!");
				if(isEmbedded[0])
				{
					Vector2 surfacePosition = contacts[0].point;
					surfacePosition.y += (m_GroundFootLength-m_MinEmbed);
					this.transform.position = surfacePosition;
				}
				else if(isEmbedded[1])
				{
					Vector2 surfacePosition = contacts[1].point;
					surfacePosition.y -= (m_CeilingFootLength-m_MinEmbed);
					this.transform.position = surfacePosition;
				}
				else if(isEmbedded[2])
				{
					Vector2 surfacePosition = contacts[2].point;
					surfacePosition.x += ((m_LeftSideLength)-m_MinEmbed);
					this.transform.position = surfacePosition;
				}
				else if(isEmbedded[3])
				{
					Vector2 surfacePosition = contacts[3].point;
					surfacePosition.x -= ((m_RightSideLength)-m_MinEmbed);
					this.transform.position = surfacePosition;
				}
				else
				{
					lastSafePosition = this.transform.position;
				}
				break;
			}
		case 2: //Two sides are touching. Use the 2-point unwedging algorithm to resolve.
			{
				if(groundContact&&ceilingContact)
				{
					//if(m_GroundNormal != m_CeilingNormal)
					{
						if(d_SendCollisionMessages)
						{
							print("Antitunneling omniwedge executed");		
						}
						OmniWedge(0,1);
					}
				}
				else if(groundContact&&leftSideContact)
				{
					if(m_GroundNormal != m_LeftNormal)
					{
						OmniWedge(0,2);
					}
					else
					{
						//print("Same surface, 1-point unwedging.");
						Vector2 surfacePosition = contacts[0].point;
						surfacePosition.y += (m_GroundFootLength-m_MinEmbed);
						this.transform.position = surfacePosition;
					}
				}
				else if(groundContact&&rightSideContact)
				{
					if(m_GroundNormal != m_RightNormal)
					{
						OmniWedge(0,3);
					}
					else
					{
						//print("Same surface, 1-point unwedging.");
						Vector2 surfacePosition = contacts[0].point;
						surfacePosition.y += (m_GroundFootLength-m_MinEmbed);
						this.transform.position = surfacePosition;
					}
				}
				else if(ceilingContact&&leftSideContact)
				{
					//if(m_CeilingNormal != m_LeftNormal)
					{
						OmniWedge(2,1);
					}
				}
				else if(ceilingContact&&rightSideContact)
				{
					//if(m_CeilingNormal != m_RightNormal)
					{
						OmniWedge(3,1);
					}
				}
				else if(leftSideContact&&rightSideContact)
				{
					throw new Exception("Unhandled horizontal wedge detected.");
					//OmniWedge(3,2);
				}
				break;
			}
		case 3: //Three sides are embedded. Not sure how to handle this yet besides reverting.
			{
				if(d_SendCollisionMessages)
				{
					print("Triple Embed.");
				}
				break;
			}
		case 4:
			{
				if(d_SendCollisionMessages)
				{
					print("FULL embedding!");
				}
				if(recoverFromFullEmbed)
				{
					this.transform.position = lastSafePosition;
				}
				break;
			}
		default:
			{
				if(d_SendCollisionMessages)
				{
					print("ERROR: DEFAULTED ON ANTITUNNELER.");
				}
				break;
			}
		}

	}



	protected Vector2 SuperUnwedger(Vector2 cPara, Vector2 gPara, bool cornerIsLeft, float embedDistance)
	{
		print("Ground Perp = ("+gPara.x+", "+gPara.y+")");
		print("Ceiling Perp = ("+cPara.x+", "+cPara.y+")");

		if(!cornerIsLeft)
		{// Setting up variables	
			//print("Resolving right wedge.");


			if(gPara.x>0)
			{// Ensure both perpendicular vectors are pointing left, out of the corner the fighter is lodged in.
				gPara *= -1;
			}

			if(cPara.x>=0)
			{// Ensure both perpendicular vectors are pointing left, out of the corner the fighter is lodged in.
				//print("("+cPara.x+", "+cPara.y+") is cPara, inverting this...");
				cPara *= -1;
			}

			if(cPara.x != -1)
			{// Multiply/Divide the top vector so that its x = -1.
				//if(Math.Abs(cPara.x) < 1)
				if(Math.Abs(cPara.x) == 0)
				{
					return new Vector2(0, -embedDistance);
				}
				else
				{
					cPara /= Mathf.Abs(cPara.x);
				}
			}

			if(gPara.x != -1)
			{// Multiply/Divide the bottom vector so that its x = -1.
				if(gPara.x == 0)
				{
					//throw new Exception("Your ground has no horizontality. What are you even doing?");
					return new Vector2(0, embedDistance);
				}
				else
				{
					gPara /= Mathf.Abs(gPara.x);
				}
			}
		}
		else
		{
			print("Resolving left wedge.");

			if(gPara.x<0)
			{// Ensure both surface-parallel vectors are pointing right, out of the corner the fighter is lodged in.
				gPara *= -1;
			}

			if(cPara.x<0)
			{// Ensure both surface-parallel vectors are pointing left, out of the corner the fighter is lodged in.
				cPara *= -1;
			}

			if(cPara.x != 1)
			{// Multiply/Divide the top vector so that its x = 1.
				if(Math.Abs(cPara.x) == 0)
				{
					print("It's a wall, bro");
					//return new Vector2(0, -embedDistance);
					return new Vector2(embedDistance-m_MinEmbed,0);
				}
				else
				{
					cPara /= cPara.x;
				}
			}

			if(gPara.x != -1)
			{// Multiply/Divide the bottom vector so that its x = -1.
				if(gPara.x == 0)
				{
					//throw new Exception("Your ground has no horizontality. What are you even doing?");
					return new Vector2(0, -embedDistance);
				}
				else
				{
					gPara /= gPara.x;
				}
			}
		}

		print("Adapted Ground Perp = " + gPara);
		print("Adapted Ceiling Perp = " + cPara);

		//
		// Now, the equation for repositioning two points that are embedded in a corner, so that both points are touching the lines that comprise the corner
		// In other words, here is the glorious UNWEDGER algorithm.
		//
		float B = gPara.y;
		float A = cPara.y;
		float H = embedDistance; //Reduced so the fighter stays just embedded enough for the raycast to detect next frame.
		float DivX;
		float DivY;
		float X;
		float Y;

		print("(A, B)=("+ A +", "+ B +").");

		if(B <= 0)
		{
			print("B <= 0, using normal eqn.");
			DivX = B-A;
			DivY = -(DivX/B);
		}
		else
		{
			print("B >= 0, using alternate eqn.");
			DivX = 1f/(B-A);
			DivY = -(A*DivX);
		}

		if(DivX != 0)
		{
			X = H/DivX;
		}
		else
		{
			X = 0;
		}

		if(DivY != 0)
		{
			Y = H/DivY;
		}
		else
		{
			Y = 0;
		}

		if((cornerIsLeft)&&(X<0))
		{
			X = -X;
		}

		if((!cornerIsLeft)&&(X>0))
		{
			X = -X;
		}

		//print("Adding the movement: ("+ X +", "+Y+").");
		if(Math.Abs(X) >= 1000 || Math.Abs(Y) >= 1000)
		{
			print("ERROR: HYPERMASSIVE CORRECTION OF ("+X+","+Y+")");
			//return new Vector2 (0, 0);
		}
		print("SuperUnwedger push of: ("+X+","+Y+")");
		return new Vector2(X,Y); // Returns the distance the object must move to resolve wedging.
	}

	protected void Jump(float horizontalInput)
	{
		if(m_Grounded&&m_Ceilinged)
		{
			if(d_SendCollisionMessages)
			{
				print("Grounded and Ceilinged, nowhere to jump!");
			}
			//FighterState.JumpKey = false;
		}
		else if(m_Grounded)
		{
			//m_LeftWallBlocked = false;
			//m_RightWallBlocked = false;

			if(FighterState.Vel.y >= 0)
			{
				FighterState.Vel = new Vector2(FighterState.Vel.x+(m_HJumpForce*horizontalInput), FighterState.Vel.y+m_VJumpForce);
			}
			else
			{
				FighterState.Vel = new Vector2(FighterState.Vel.x+(m_HJumpForce*horizontalInput), m_VJumpForce);
			}
			o_FighterAudio.JumpSound();
			//FighterState.JumpKey = false;
		}
		else if(m_LeftWalled)
		{
			if(d_SendCollisionMessages)
			{
				print("Leftwalljumping!");
			}
			if(FighterState.Vel.y < 0)
			{
				FighterState.Vel = new Vector2(m_WallHJumpForce, m_WallVJumpForce);
			}
			else if(FighterState.Vel.y <= (2*m_WallVJumpForce))
			{
				FighterState.Vel = new Vector2(m_WallHJumpForce, FighterState.Vel.y+m_WallVJumpForce);
			}
			else
			{
				FighterState.Vel = new Vector2(m_WallHJumpForce, FighterState.Vel.y);
			}
			o_FighterAudio.JumpSound();
			//FighterState.JumpKey = false;
			m_LeftWalled = false;
		}
		else if(m_RightWalled)
		{
			if(d_SendCollisionMessages)
			{
				print("Rightwalljumping!");
			}
			if(FighterState.Vel.y < 0)
			{
				FighterState.Vel = new Vector2(-m_WallHJumpForce, m_WallVJumpForce);
			}
			else if(FighterState.Vel.y <= m_WallVJumpForce)
			{
				FighterState.Vel = new Vector2(-m_WallHJumpForce, FighterState.Vel.y+m_WallVJumpForce);
			}
			else
			{
				FighterState.Vel = new Vector2(-m_WallHJumpForce, FighterState.Vel.y);
			}

			o_FighterAudio.JumpSound();
			//FighterState.JumpKey = false;
			m_RightWalled = false;
		}
		else if(m_Ceilinged)
		{
			if(FighterState.Vel.y <= 0)
			{
				FighterState.Vel = new Vector2(FighterState.Vel.x+(m_HJumpForce*horizontalInput), FighterState.Vel.y -m_VJumpForce);
			}
			else
			{
				FighterState.Vel = new Vector2(FighterState.Vel.x+(m_HJumpForce*horizontalInput), -m_VJumpForce);
			}
			o_FighterAudio.JumpSound();
			//FighterState.JumpKey = false;
			m_Ceilinged = false;
		}
		else
		{
			//print("Can't jump, airborne!");
		}
	}

	#endregion
	//###################################################################################################################################
	// PUBLIC FUNCTIONS
	//###################################################################################################################################
	#region PUBLIC FUNCTIONS

	public bool IsVelocityPunching()
	{
		return g_VelocityPunching;
	}

	public bool isAlive()
	{
		return !FighterState.Dead;
	}

	public void InstantForce(Vector2 newDirection, float speed)
	{
		//newDirection.Normalize();
		SetSpeed(newDirection, speed);
		//DirectionChange(newDirection);
		print("Changing direction to" +newDirection);
		//
	}

	public void InstantForce(Vector2 newDirection)
	{
		DirectionChange(newDirection);
	}

	public void PunchConnect(GameObject victim, Vector2 aimDirection)
	{
		if((isAPlayer&&!isLocalPlayer)|| !isAPlayer&&!isServer){return;}
		FighterChar enemyFighter = null;

		if(victim != null)
		{
			enemyFighter = victim.GetComponent<FighterChar>();
		}
		if(enemyFighter != null)
		{
			enemyFighter.FighterState.CurHealth -= 5;
			enemyFighter.FighterState.Vel += aimDirection.normalized*5;
			o_FighterAudio.PunchHitSound();

			float Magnitude = 1f;
			float Roughness = 20f;
			float FadeOutTime = 0.6f;
			float FadeInTime = 0.0f;
			float posX = 0.5f*aimDirection.normalized.x;
			float posY = 0.5f*aimDirection.normalized.y;
			Vector3 RotInfluence = new Vector3(0,0,0);
			Vector3 PosInfluence = new Vector3(posX,posY,0);
			CameraShaker.Instance.ShakeOnce(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);

			//print("Punch connected locally");
		}

		CmdPunchConnect(victim, aimDirection);
	}

	[Command] public void CmdPunchConnect(GameObject victim, Vector2 aimDirection)
	{
		RpcPunchConnect(victim, aimDirection);
	}

	[ClientRpc] public void RpcPunchConnect(GameObject victim, Vector2 aimDirection)
	{
		if(isLocalPlayer){return;}
		FighterChar enemyFighter = null;
		if(victim != null)
		{
			enemyFighter = victim.GetComponent<FighterChar>();
		}
		if(enemyFighter != null)
		{
			enemyFighter.FighterState.CurHealth -= 5;
			enemyFighter.FighterState.Vel += aimDirection.normalized*5;
			o_FighterAudio.PunchHitSound();
			//print("Punch connected remotely");
		}
	}


	public bool IsDisabled()
	{
		if(FighterState.Dead||(g_CurFallStun>0))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public float GetInstantGForce()
	{
		return m_IGF;
	}

	public float GetContinuousGForce()
	{
		return m_CGF;
	}

	public Vector2 GetVelocity()
	{
		return FighterState.Vel;
	}

	public Vector2 GetPosition()
	{
		return FighterState.FinalPos;
	}

	public float GetSpeed()
	{
		return m_Spd;
	}

	public void SetZonLevel(int zonLevel)
	{
		g_ZonLevel = zonLevel;
	}

	public int GetZonLevel()
	{
		return g_ZonLevel;
	}

	public int GetZonStance()
	{
		return g_ZonStance; //-1 is none, 0 is kneeling, 1 is AD.
	}

	public void DissipateZon()
	{
		// Executes when the fighter leaves zon stance without using power.
	}

	public void SetSpeed(Vector2 inputVelocity, float speed)
	{
		//print("SetSpeed");
		Vector2 newVelocity;
		Vector2 direction = inputVelocity.normalized;
		newVelocity = direction * speed;
		FighterState.Vel = newVelocity;
	}

	public void SetSpeed(float speed)
	{
		//print("SetSpeed");
		Vector2 newVelocity;
		Vector2 direction = FighterState.Vel.normalized;
		newVelocity = direction * speed;
		FighterState.Vel = newVelocity;
	}

	public void TakeDamage(int dmgAmount)
	{
		FighterState.CurHealth -= dmgAmount;
	}

	public bool IsPlayer()
	{
		return isAPlayer;
	}

	#endregion
}

[System.Serializable] public struct FighterState
{
	[SerializeField] public bool DevMode;											// Turns on all dev cheats.
	[SerializeField][ReadOnlyAttribute]public int CurHealth;						// Current health.
	[SerializeField][ReadOnlyAttribute]public bool Dead;							// True when the fighter's health reaches 0 and they die.
	[SerializeField][ReadOnlyAttribute]public bool JumpKey;
	[SerializeField][ReadOnlyAttribute]public bool LeftClick;
	[SerializeField][ReadOnlyAttribute]public bool LeftClickHold;
	[SerializeField][ReadOnlyAttribute]public bool LeftClickRelease;
	[SerializeField][ReadOnlyAttribute]public bool RightClick;
	[SerializeField][ReadOnlyAttribute]public bool LeftKey;
	[SerializeField][ReadOnlyAttribute]public bool RightKey;
	[SerializeField][ReadOnlyAttribute]public bool UpKey;
	[SerializeField][ReadOnlyAttribute]public bool DownKey;
	[SerializeField][ReadOnlyAttribute]public bool ZonKey;
	[SerializeField][ReadOnlyAttribute]public bool DisperseKey;
	[SerializeField][ReadOnlyAttribute]public Vector2 MouseWorldPos;				// Mouse position in world coordinates.
	[SerializeField][ReadOnlyAttribute]public Vector2 PlayerMouseVector;			// Vector pointing from the player to their mouse position.
	[SerializeField][ReadOnlyAttribute]public Vector2 Vel;							//Current (x,y) velocity.
	[SerializeField][ReadOnlyAttribute]public Vector2 FinalPos;						//The final position of the character at the end of the physics frame.
}