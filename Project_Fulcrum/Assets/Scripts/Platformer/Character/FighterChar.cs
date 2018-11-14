using UnityEngine.UI;
using System;
using UnityEngine;

using EZCameraShake;
using UnityEngine.Networking;
using AK.Wwise;
#if UNITY_EDITOR
using UnityEditor;
#endif
/*
 * AUTHOR'S NOTES:
 * 
 * FighterChar is the basis class for all humanoid combatants. NPC and Player classes extend this class. 
 * Most animation, collision code, and physics is conducted in this base class. Input is processed in FighterChar, but the input is recieved from respective sources set in NPC and Player.
 * A FighterChar, if spawned, would behave simply as an uncontrolled dummy. Spawn players or NPCs instead, and use FighterChar for any code that would affect both of them. 
 * 
 * Naming conventions:
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
 *
*/

[System.Serializable]
public class FighterChar : NetworkBehaviour
{        
	[Header("Debug Variables:")]
	[Tooltip("Debug Variables")][SerializeField] public DebugVars d;

	[Header("Movement Variables:")]
	[Tooltip("Movement Variables")][SerializeField] public MovementVars m;

	[Header("Object References:")]
	[Tooltip("Object References")][SerializeField] public ObjectRefs o;

	[Header("Physics Variables:")]
	[Tooltip("Physics Variables")][SerializeField] public PhysicsVars phys;

	[Header("Audio/Visual Variables:")]
	[Tooltip("Audio/Visual Variables")][SerializeField] public AudioVisualVars v;

	//############################################################################################################################################################################################################
	// KINEMATIC VARIABLES
	//###########################################################################################################################################################################
	#region KINEMATIC VARIABLES
	[Header("Kinematic Components:")]
	[SerializeField] protected bool k_IsKinematic; 						//Dictates whether the player is moving in physical fighterchar mode or in some sort of specially controlled fashion, such as in cutscenes or strand jumps
	[SerializeField] protected int k_KinematicAnim; 					//Designates the kinematic animation being played. 0 is strandjumping.
	[SerializeField] protected float k_StrandJumpSlowdownM = 0.33f; 		//Percent of momentum retained per frame when hitting a strand.
	[SerializeField] protected float k_StrandJumpSlowdownLinear = 5f; 	//Set amount of momentum lost per frame when hitting a strand.
	#endregion
	//############################################################################################################################################################################################################
	// OBJECT REFERENCES
	//###########################################################################################################################################################################
	#region OBJECT REFERENCES
	[Header("Prefab References:")]
	[SerializeField] public GameObject p_EtherPulse;			// Reference to the Ether Pulse prefab, a pulsewave that emanates from the fighter when they disperse ether force.
	[SerializeField] public GameObject p_AirPunchPrefab;		// Reference to the air punch attack prefab.
	[SerializeField] public GameObject p_DebugMarker;			// Reference to a sprite prefab used to mark locations ingame during development.
	[SerializeField] public GameObject p_ShockEffectPrefab;		// Reference to the shock visual effect prefab.
	[SerializeField] public GameObject p_StrandJumpPrefab;		// Reference to the strand jump visual effect prefab.
	[SerializeField] public GameObject p_AirBurstPrefab;		// Reference to the air burst prefab, which is a radial windforce.
	[SerializeField] public GameObject p_DustEffectPrefab;		// Reference to the dust visual effect prefab.
	[SerializeField] public GameObject p_SparkEffectPrefab;		// Reference to the spark visual effect prefab.
	[SerializeField] public GameObject p_ExplosionEffectPrefab;	// Reference to the explosion visual effect prefab.

	#endregion
	//############################################################################################################################################################################################################
	// PHYSICS&RAYCASTING
	//###########################################################################################################################################################################
	#region PHYSICS&RAYCASTING

	// Physics relies on four directional raycasts set up in a cross formation. The vertical raycasts are longer than the horizontal ones. 
	// This results in a diamond shaped hitbox. When multiple raycasts are contacting the ground, priority is chosen based on which one impacts the deepest, or, when equal, the angle of terrain.

	[SerializeField] public LayerMask m_TerrainMask;	// Mask used for terrain collisions.
	[SerializeField] public LayerMask m_FighterMask;	// Mask used for fighter collisions.

	[HideInInspector]public Transform m_GroundFoot; 			// Ground collider.
	[HideInInspector]public Vector2 m_GroundFootOffset; 		// Ground raycast endpoint.
	[ReadOnlyAttribute]public float m_GroundFootLength;		// Ground raycast length.

	[HideInInspector]public Transform m_CeilingFoot; 		// Ceiling collider, middle.
	[HideInInspector]public Vector2 m_CeilingFootOffset;		// Ceiling raycast endpoint.
	[ReadOnlyAttribute]public float m_CeilingFootLength;		// Ceiling raycast length.

	[HideInInspector]public Transform m_LeftSide; 			// LeftWall collider.
	[HideInInspector]public Vector2 m_LeftSideOffset;		// LeftWall raycast endpoint.
	[ReadOnlyAttribute]public float m_LeftSideLength;			// LeftWall raycast length.

	[HideInInspector]public Transform m_RightSide;  			// RightWall collider.
	[HideInInspector]public Vector2 m_RightSideOffset;		// RightWall raycast endpoint.
	[ReadOnlyAttribute]public float m_RightSideLength;			// RightWall raycast length.

	[ReadOnlyAttribute]public Vector2 m_GroundNormal;			// Vector holding the slope of Ground.
	[ReadOnlyAttribute]public Vector2 m_CeilingNormal;		// Vector holding the slope of Ceiling.
	[ReadOnlyAttribute]public Vector2 m_LeftNormal;			// Vector holding the slope of LeftWall.
	[ReadOnlyAttribute]public Vector2 m_RightNormal;			// Vector holding the slope of RightWall.

	[ReadOnlyAttribute]public RaycastHit2D[] directionContacts;
	[ReadOnlyAttribute] public RaycastHit2D closeToGroundContact; // Seperate from the contact raycasts, this detects if the ground is *near* below the fighter (but not neccessarily touching)


	#endregion
	//##########################################################################################################################################################################
	// FIGHTER INPUT VARIABLES
	//###########################################################################################################################################################################
	#region FIGHTERINPUT
	[Header("Networked Variables:")]
	[SerializeField] protected FighterState FighterState;// Struct holding all networked fighter info.
	[Header("Input:")]
	protected int CtrlH; 													// Tracks horizontal keys pressed. Values are -1 (left), 0 (none), or 1 (right). 
	protected int CtrlV; 													// Tracks vertical keys pressed. Values are -1 (down), 0 (none), or 1 (up).
	#endregion
	//############################################################################################################################################################################################################
	// GAMEPLAY VARIABLES
	//###########################################################################################################################################################################
	#region GAMEPLAY VARIABLES
	[Header("Gameplay:")]
	[SerializeField] protected int g_Team = 0;							// What team the NPC is on. Team 0 = ally, team 1 = enemy.

	[SerializeField] protected bool g_VelocityPunching;					// True when fighter is channeling a velocity fuelled punch.
	[SerializeField] protected float g_VelocityPunchChargeTime = 0.5f;			// Min duration the fighter can be stunned from slamming the ground.

	[SerializeField] protected int g_MaxVigor = 100;					// Max health.
	[SerializeField] protected int g_MinSlamDMG = 5;					// Min damage a slam impact can deal.
	[SerializeField] protected int g_MaxSlamDMG = 30;					// Max damage a slam impact can deal.	
	[SerializeField] protected int g_MinCrtrDMG = 30;					// Min damage a crater impact can deal.
	[SerializeField] protected int g_MaxCrtrDMG = 60;					// Max damage a crater impact can deal.
	[SerializeField] protected float g_MinSlamStun = 0.5f;				// Min duration the fighter can be stunned from slamming the ground or being attacked.
	[SerializeField] protected float g_MaxSlamStun = 1.5f;				// Max duration the fighter can be stunned from slamming the ground or being attacked.
	[SerializeField] protected float g_MinCrtrStun = 1.5f;				// Max duration the fighter can be stunned from smashing the ground hard or being attacked.
	[SerializeField] protected float g_MaxCrtrStun = 3f;				// Max duration the fighter can be stunned from smashing the ground hard or being attacked.
	[SerializeField] protected bool g_Stunned = false;					// True when the fighter loses control after a hard impact from the ground or a player.
	[SerializeField] protected bool g_Staggered = false;				// True when the fighter loses control after a hard impact from the ground or a player.
	[SerializeField] public int g_IsInGrass;							// True when greater than 1. The number equates to how many grass tiles the fighter is touching.
	[SerializeField] public bool g_FighterCollision = true;				// While true, this fighter will collide with other fighters
	[SerializeField][ReadOnlyAttribute] public float g_FighterCollisionCD;					// Time after a fightercollision that further collision is disabled. This is the current time remaining.
	[SerializeField] public float g_FighterCollisionCDLength = 0.25f;		// Time after a fightercollision that further collision is disabled. This is the duration to wait. Later this should be modified to only affect one fighter.
	[SerializeField][ReadOnlyAttribute]protected float g_CurStun = 0;	// How much longer the fighter is stunned after a fall. When this value is > 0  the fighter is stunned.
	[SerializeField] protected float g_MaxClashDisparity = 500;			// The speed difference at which the damage a clash deals reaches its max. For example, if set to 500, one player must be going 500 Kph faster than their opponent to deal 100% damage. If value is lost, try starting with 500 to test.
	[SerializeField] protected float g_MaxClashDamage = 300;			// Max damage dealt by any clash of fighters. (A clash is when two fighters collide in attack stance)
	[SerializeField] protected float g_MaxClashDamageForce = 1000;		// Combined force at which max damage is dealt by any clash of fighters. (A clash is when two fighters collide in attack stance)
	[SerializeField] protected float g_MaxGuardClassDamageForce = 2000;	// Combined force at which max damage is dealt by any clash of fighters. (A clash is when two fighters collide in attack stance)

	[SerializeField][Range(0,1)] protected float g_MinAttackStaggerT = 0.1f;			// Min damage required to be staggered by the fighter during a clash.
	[SerializeField][Range(0,1)] protected float g_MinAttackStunT = 0.25f;				// Damage required to reach max stagger time from a clash. Min damage required to stun the fighter during a clash.
	[SerializeField][Range(0,1)] protected float g_MaxAttackStunT = 0.4f;				// Damage required to reach max stun time from a clash.

	[SerializeField][Range(0,1)] protected float g_MinNeutStaggerT = 0.05f;				// Min damage required to be staggered by the enemy when in neutral stance.
	[SerializeField][Range(0,1)] protected float g_MinNeutStunT = 0.125f;				// Damage required to reach max stagger time from any enemy strike when in neutral stance; Min damage required to stun from any enemy strike when in neutral stance.
	[SerializeField][Range(0,1)] protected float g_MaxNeutStunT = 0.2f;					// Damage required to reach max stun time when hit by the enemy when in neutral stance.

	[SerializeField][Range(0,1)] protected float g_MinGuardStaggerT = 0.2f;				// Min damage required to be staggered by the enemy when in guard stance.
	[SerializeField][Range(0,1)] protected float g_MinGuardStunT = 0.5f;				// Damage required to reach max stagger time from any enemy strike when in guard stance; Min damage required to stun from any enemy strike when in guard stance.
	[SerializeField][Range(0,1)] protected float g_MaxGuardStunT = 0.8f;				// Damage required to reach max stun time from a strike on a guard stance.

	protected bool isAPlayer;
	[SerializeField] protected int p_DefaultShoeID;      		// Reference to the character's starting shoe

	#endregion 
	//############################################################################################################################################################################################################
	// NAVIGATION VARIABLES
	//###########################################################################################################################################################################
	#region NAVIGATION
	[Header("Navigation:")]
	[SerializeField][ReadOnlyAttribute] public NavSurface n_CurrentSurf; // Surface the fighter is standing on.
	[SerializeField][ReadOnlyAttribute] public int n_CurrentSurfID; // ID of surface the fighter is standing on.
	[SerializeField] protected float n_MaxSurfLineDist = 0.5f; // Max distance from the current surface. Outside of this, it is considered off the surface.

	//NavConnection AutoGeneration variables. Used for recording the players movements for the NPCs to mimic.
	[SerializeField] protected bool n_AutoGenerateNavCon; // When true, any traversals made between surfaces will be recorded for use by the AI.
	[SerializeField][ReadOnlyAttribute] protected bool n_Jumped; // Set to true when the player started the traversal with a jump.
	[SerializeField][ReadOnlyAttribute] protected bool n_PlayerTraversing; // Set to true when the player is traversing between surfaces.
	[SerializeField][ReadOnlyAttribute] protected float n_PlayerTraversalTime; // Time the player took to complete the traversal. Updated each frame until the destination is reached.
	[SerializeField][ReadOnlyAttribute] protected NavConnection n_TempNavCon; // Set to true when the player is traversing between surfaces.
	[SerializeField][ReadOnlyAttribute] protected NavSurface n_LastSurface; // Surface the player was last standing on.
	[SerializeField][ReadOnlyAttribute] protected bool n_SpecialAction; // Set to true when the player traverses using a method the NPC cannot, such as superjumps.



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
		//EditorApplication.isPaused = true; //Used for debugging.
		d.tickCounter++;
		d.tickCounter = (d.tickCounter > 60) ? 0 : d.tickCounter; // Rolls back to zero when hitting 60
		UpdateCurrentNavSurf();
		if(k_IsKinematic)
		{
			FixedUpdateKinematic();	
		}
		else
		{
			FixedUpdateProcessInput();
			FixedUpdatePhysics();
		}
		FixedUpdateLogic();
		FixedUpdateAnimation();
		FixedUpdateWwiseAudio();
//		FighterState.RightClick = false;
//		FighterState.LeftClick = false;
//		FighterState.EtherKey = false;
//		FighterState.DisperseKey = false;
	}

	protected virtual void Update()
	{
		UpdatePlayerInput();
		UpdateAnimation();
	}

	protected virtual void LateUpdate()
	{
		// Declared for override purposes
	}


	#endregion
	//###################################################################################################################################
	// CUSTOM FUNCTIONS
	//###################################################################################################################################
	#region CUSTOM FUNCTIONS

	public void Teleport(Vector2 destination, bool resetVelocity)
	{
		this.FighterState.FinalPos = destination;
		this.GetComponent<InterpolatedTransform>().ForgetPreviousTransforms();
		if(resetVelocity)
		{
			this.FighterState.Vel = Vector2.zero;
		}
	}

	public void Teleport(Vector3 destination, bool resetVelocity)
	{
		this.FighterState.FinalPos = (Vector2)destination;
		this.GetComponent<InterpolatedTransform>().ForgetPreviousTransforms();
		if(resetVelocity)
		{
			this.FighterState.Vel = Vector2.zero;
		}
	}

	protected void FlashEffect(float myDuration, Color myColor)
	{
		v.flashDuration = myDuration;
		v.flashTimer = myDuration;
		v.flashColour = myColor;
	}

	public bool IsDevMode()
	{
		return this.FighterState.DevMode;
	}

	protected void UpdateCurrentNavSurf()
	{
		if(n_PlayerTraversing)
			n_PlayerTraversalTime += Time.fixedDeltaTime;

		n_CurrentSurf = null;
		n_CurrentSurfID = -1;

		Vector3 contactTransform;

		if(v.truePrimarySurface==0)
		{
			contactTransform = this.m_GroundFoot.position;
		}
		else if(v.truePrimarySurface==1)
		{
			contactTransform = this.m_CeilingFoot.position;
		}
		else if(v.truePrimarySurface==2)
		{
			contactTransform = this.m_LeftSide.position;
		}
		else
		{
			contactTransform = this.m_RightSide.position;
		}
		NavSurface[] surfaceList = o.navMaster.GetSurfaces();
		for(int i = 0; i<surfaceList.Length; i++)
		{
			if( surfaceList[i].DistFromLine(this.m_GroundFoot.position)<=n_MaxSurfLineDist && surfaceList[i].surfaceType == v.truePrimarySurface)
			{
				n_CurrentSurf = surfaceList[i];
				n_CurrentSurfID = n_CurrentSurf.id;
				n_LastSurface = n_CurrentSurf;
				if(n_PlayerTraversing&&v.truePrimarySurface!=-1) // If touching a new surface and not airborne, set that as the destination of your traversal.
					EndPlayerTraverse();
				break;
			}
		}

		if(n_CurrentSurf==null&&n_AutoGenerateNavCon&&!n_PlayerTraversing&&!n_Jumped)
		{
			StartPlayerTraverse();
		}
	}

	protected void StartPlayerTraverse()
	{
		print("["+d.tickCounter+"]:Starting Player traversal recording");
		//n_AutoGenerateNavCon = false;
		n_PlayerTraversalTime = 0;
		n_PlayerTraversing = true;
		n_TempNavCon = new NavConnection();
		n_TempNavCon.exitPosition = n_LastSurface.WorldToLinPos(this.transform.position);
		n_TempNavCon.edgeWeight = 1;
		n_TempNavCon.exitVel = this.FighterState.Vel;
		n_TempNavCon.orig = n_LastSurface;
		n_TempNavCon.traversaltimeout = 2;
		n_TempNavCon.exitPositionRange = 0.5f;
		n_TempNavCon.exitVelRange = 0.5f;

		if(n_CurrentSurf.surfaceType>=2)
		{
			n_TempNavCon.exitPositionRange = 1f;
			n_TempNavCon.exitVelRange = 5f;
		}

		if(n_Jumped)
		{	
			n_Jumped = false;
			n_TempNavCon.traverseType = 1;
		}
		else
			n_TempNavCon.traverseType = 0;
	}

	protected void EndPlayerTraverse()
	{
		n_PlayerTraversing = false;

		if(n_CurrentSurf.id==n_TempNavCon.orig.id)
		{
			print("["+d.tickCounter+"]: "+n_TempNavCon.orig.id+" and "+n_CurrentSurf.id+" are same surface, not recording.");
			n_TempNavCon = null;
			return;
		}
		n_TempNavCon.dest = n_CurrentSurf;
		n_TempNavCon.destPosition = n_CurrentSurf.WorldToLinPos(this.transform.position);
		n_TempNavCon.averageTraversalTime = n_PlayerTraversalTime;
		n_TempNavCon.orig.AddNavConnection(n_TempNavCon);
		print("["+d.tickCounter+"]:Saved new navconnection between surfaces "+n_TempNavCon.orig.id+" and "+n_TempNavCon.dest.id+".");
	}
	
	protected void FighterAwake()
	{
		o.timeManager = GameObject.Find("PFGameManager").GetComponent<TimeManager>();
		o.itemHandler = GameObject.Find("PFGameManager").GetComponent<ItemHandler>();
		o.itemHandler = GameObject.Find("PFGameManager").GetComponent<ItemHandler>();
		o.navMaster = GameObject.Find("NavMaster").GetComponent<NavMaster>();

		v.triggerGenderChange = true; // Marks the gender attribute as needing to be set by WWise.

		//v.terrainType = new string[]{ "Concrete", "Concrete", "Concrete", "Concrete" };
		directionContacts = new RaycastHit2D[4];
		closeToGroundContact = new RaycastHit2D();

		FighterState.CurVigor = 100;					// Current health.
		FighterState.Dead = false;						// True when the fighter's health reaches 0 and they die.
		Vector2 fighterOrigin = new Vector2(this.transform.position.x, this.transform.position.y);

		o.velocityPunch = GetComponentInChildren<VelocityPunch>();
		o.spriteTransform = transform.Find("SpriteTransform");
		o.debugAngleDisplay = transform.Find("DebugAngleDisplay");
		o.dustSpawnTransform = transform.Find("SpriteTransform/DustEffectTransform");
		o.anim = o.spriteTransform.GetComponentInChildren<Animator>();
		o.spriteRenderer = o.spriteTransform.GetComponent<SpriteRenderer>();
		o.fighterAudio = this.GetComponent<FighterAudio>();
		o.rigidbody2D = GetComponent<Rigidbody2D>();

		if(o.dustSpawnTransform==null)
		{
			print("dusttransform is the issue");
		}

		if(p_SparkEffectPrefab==null)
		{
			print("p_SparkEffectPrefab is the issue");
		}
			
		o.sparkThrower = (GameObject)Instantiate(p_SparkEffectPrefab, o.dustSpawnTransform.position, Quaternion.identity, this.transform);
		ParticleSystem.EmissionModule em = o.sparkThrower.GetComponent<ParticleSystem>().emission;
		em.enabled = false;



		m_GroundFoot = transform.Find("MidFoot");
		d.groundLine = m_GroundFoot.GetComponent<LineRenderer>();
		m_GroundFootOffset.x = m_GroundFoot.position.x-fighterOrigin.x;
		m_GroundFootOffset.y = m_GroundFoot.position.y-fighterOrigin.y;
		m_GroundFootLength = m_GroundFootOffset.magnitude;

		m_CeilingFoot = transform.Find("CeilingFoot");
		d.ceilingLine = m_CeilingFoot.GetComponent<LineRenderer>();
		m_CeilingFootOffset.x = m_CeilingFoot.position.x-fighterOrigin.x;
		m_CeilingFootOffset.y = m_CeilingFoot.position.y-fighterOrigin.y;
		m_CeilingFootLength = m_CeilingFootOffset.magnitude;

		m_LeftSide = transform.Find("LeftSide");
		d.leftSideLine = m_LeftSide.GetComponent<LineRenderer>();
		m_LeftSideOffset.x = m_LeftSide.position.x-fighterOrigin.x;
		m_LeftSideOffset.y = m_LeftSide.position.y-fighterOrigin.y;
		m_LeftSideLength = m_LeftSideOffset.magnitude;

		m_RightSide = transform.Find("RightSide");
		d.rightSideLine = m_RightSide.GetComponent<LineRenderer>();
		m_RightSideOffset.x = m_RightSide.position.x-fighterOrigin.x;
		m_RightSideOffset.y = m_RightSide.position.y-fighterOrigin.y;
		m_RightSideLength = m_RightSideOffset.magnitude;

		d.debugLine = GetComponent<LineRenderer>();

		v.defaultColor = o.spriteRenderer.color;
		phys.lastSafePosition = new Vector2(0,0);
		phys.remainingMovement = new Vector2(0,0);
		phys.remainingVelM = 1f;

		Shoe startingShoe = Instantiate(o.itemHandler.shoes[p_DefaultShoeID], this.transform.position, Quaternion.identity).GetComponent<Shoe>();
		EquipItem(startingShoe);


		if(!(d.showVelocityIndicator||FighterState.DevMode)){
			d.debugLine.enabled = false;
		}

		if(!(d.showContactIndicators||FighterState.DevMode))
		{
			d.ceilingLine.enabled = false;
			d.groundLine.enabled = false;
			d.rightSideLine.enabled = false;
			d.leftSideLine.enabled = false;
		}
	}

	public virtual void UnequipShoe()
	{
		if(o.equippedShoe == null){return;}
		if(o.equippedShoe.shoeID==0)
		{
			print("Cannot unequip feet!");
			o.equippedShoe.DestroyThis();
			return;
		}
		o.equippedShoe.Drop();
	}

	public virtual void UnequipWeapon()
	{
//		if(o.equippedShoe == null){return;}
//		if(o.equippedShoe.itemID==0)
//		{
//			print("Cannot unequip feet!");
//			o.equippedShoe.DestroyThis();
//			return;
//		}
//		o.equippedShoe.Drop();
	}

	public virtual void UnequipGadget()
	{
		//		if(o.equippedShoe == null){return;}
		//		if(o.equippedShoe.itemID==0)
		//		{
		//			print("Cannot unequip feet!");
		//			o.equippedShoe.DestroyThis();
		//			return;
		//		}
		//		o.equippedShoe.Drop();
	}

	public virtual void EquipItem(Item item)
	{
		print("Generic Item not supported! Item is a template class!");
	}

	public virtual void EquipItem(Shoe shoe) // Equip a shoe object. Set argument to null to equip barefoot.
	{		
		if(shoe==null)
		{
			shoe = Instantiate(o.itemHandler.shoes[0], this.transform.position, Quaternion.identity).GetComponent<Shoe>();
		}

		UnequipShoe(); // Drop old shoes.

		///
		/// Movestat code
		///

		this.m.minSpeed = shoe.m.minSpeed;					
		this.m.maxRunSpeed = shoe.m.maxRunSpeed;				
		this.m.startupAccelRate = shoe.m.startupAccelRate;  			

		this.m.vJumpForce = shoe.m.vJumpForce;               
		this.m.hJumpForce = shoe.m.hJumpForce;  				
		this.m.wallVJumpForce = shoe.m.wallVJumpForce;           
		this.m.wallHJumpForce = shoe.m.wallHJumpForce;  			
		this.m.etherJumpForcePerCharge = shoe.m.etherJumpForcePerCharge; 	
		this.m.etherJumpForceBase = shoe.m.etherJumpForceBase; 		

		this.m.tractionChangeT = shoe.m.tractionChangeT;			
		this.m.wallTractionT = shoe.m.wallTractionT;			
		this.m.linearStopRate = shoe.m.linearStopRate; 			
		this.m.linearSlideRate = shoe.m.linearSlideRate;			
		this.m.linearOverSpeedRate = shoe.m.linearOverSpeedRate;		
		this.m.linearAccelRate = shoe.m.linearAccelRate;			
		this.m.impactDecelMinAngle = shoe.m.impactDecelMinAngle;
		this.m.impactDecelMaxAngle = shoe.m.impactDecelMaxAngle;
		this.m.tractionLossMinAngle = shoe.m.tractionLossMinAngle; 
		this.m.tractionLossMaxAngle = shoe.m.tractionLossMaxAngle;
		this.m.slippingAcceleration = shoe.m.slippingAcceleration;  	
		this.m.surfaceClingTime = shoe.m.surfaceClingTime;
		this.m.clingReqGForce = shoe.m.clingReqGForce;

		this.m.slamT = shoe.m.slamT;					
		this.m.craterT = shoe.m.craterT; 					
		this.m.guardSlamT = shoe.m.guardSlamT; 				
		this.m.guardCraterT = shoe.m.guardCraterT;				

		this.m.strandJumpSpeedLossM = shoe.m.strandJumpSpeedLossM;
		this.m.widestStrandJumpAngle = shoe.m.widestStrandJumpAngle;

		///
		/// Non movestat code
		///

		o.equippedShoe = shoe;
		shoe.PickedUpBy(this);

		if(shoe.shoeID!=0)
		{
			o.fighterAudio.EquipSound();
		}
	}

	protected void ThrowPunch(Vector2 aimDirection)
	{
		float randomness1 = UnityEngine.Random.Range(-0.2f,0.2f);
		float randomness2 = UnityEngine.Random.Range(-0.2f,0.2f);
		float xTransform = 1f;
		float yTransform = 1f;

		if(aimDirection.x<0)
		{
			v.facingDirection = false;
			xTransform = -1f;
		}
		else
		{
			v.facingDirection = true;
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
		v.triggerAtkHit = true;
		o.fighterAudio.PunchSound();
	}

	protected virtual void Death()
	{
		if(FighterState.DevMode)
		{
			FighterState.CurVigor = 100;
			return;
		}
		FighterState.Dead = true;
		o.anim.SetBool("Dead", true);
		v.currentColor = Color.red;
		o.spriteRenderer.color = v.currentColor;
	}

	protected virtual void Respawn()
	{

		FighterState.Dead = false;
		FighterState.CurVigor = g_MaxVigor;
		o.anim.SetBool("Dead", false);
		v.currentColor = v.defaultColor;
		o.spriteRenderer.color = v.currentColor;
	}

//	protected virtual void SpawnSparkEffect()
//	{
//		Vector3 spawnPos;
//		if(o.dustSpawnTransform)
//		{
//			spawnPos = o.dustSpawnTransform.position;
//		}
//		else
//		{
//			spawnPos = m_GroundFoot.position;
//		}
//		Instantiate(p_SparkEffectPrefab, spawnPos, Quaternion.identity);
//	}
//		
//	protected virtual void SpawnSparkEffect(Vector2 spawnPos)
//	{
//		Vector3 spawnPosV3 = (Vector3)spawnPos;
//		Instantiate(p_SparkEffectPrefab, spawnPosV3, Quaternion.identity);
//	}
//		
	protected virtual void SpawnDustEffect()
	{
		Vector3 spawnPos;
		if(o.dustSpawnTransform)
		{
			spawnPos = o.dustSpawnTransform.position;
		}
		else
		{
			spawnPos = m_GroundFoot.position;
		}
		Instantiate(p_DustEffectPrefab, spawnPos, Quaternion.identity);
	}

	protected virtual void SpawnExplosionEffect(float explosionForce)
	{
		Vector3 spawnPos;
		float rotation;
		if(v.primarySurface <= 0)
		{
			spawnPos = m_GroundFoot.position;
			rotation = Get2DAngle(Perp(m_GroundNormal));
		}
		else if(v.primarySurface == 1)
		{
			spawnPos = m_CeilingFoot.position;
			rotation = Get2DAngle(Perp(m_CeilingNormal));
		}
		else if(v.primarySurface == 2)
		{
			spawnPos = m_LeftSide.position;
			rotation = Get2DAngle(Perp(m_LeftNormal));
		}
		else
		{
			spawnPos = m_RightSide.position;
			rotation = Get2DAngle(Perp(m_RightNormal));
		}

		Quaternion spawnRotation = new Quaternion();
		spawnRotation.eulerAngles = new Vector3(0, 0, rotation);

		GameObject newExplosion = (GameObject)Instantiate(p_ExplosionEffectPrefab, spawnPos, spawnRotation);
		newExplosion.GetComponent<ExplosionEffect>().craterForce = explosionForce;
	}

	protected virtual void SpawnDustEffect(Vector2 spawnPos)
	{
		Vector3 spawnPosV3 = (Vector3)spawnPos;
		Instantiate(p_DustEffectPrefab, spawnPosV3, Quaternion.identity);
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

	protected virtual void FixedUpdatePhysics() //FUP
	{
		this.transform.position = FighterState.FinalPos;
		phys.distanceTravelled = Vector2.zero;

		phys.initialVel = FighterState.Vel;
		v.wallSliding = false; // Set to false, and changed to true in WallTraction().



		if(phys.grounded)
		{//Locomotion!
			Traction(CtrlH, CtrlV);
			m.airborneDelayTimer = m.airborneDelay;
			v.primarySurface = 0;
			v.truePrimarySurface = 0;
		}
		else if(phys.leftWalled)
		{//Wallsliding!
			//print("Walltraction!");
			WallTraction(CtrlH, CtrlV, m_LeftNormal);
			m.airborneDelayTimer = m.airborneDelay;
			v.primarySurface = 2;
			v.truePrimarySurface = 2;
		}
		else if(phys.rightWalled)
		{//Wallsliding!
			WallTraction(CtrlH, CtrlV, m_RightNormal);
			m.airborneDelayTimer = m.airborneDelay;
			v.primarySurface = 3;
			v.truePrimarySurface = 3;
		}
//		else if(phys.ceilinged)
//		{
//			WallTraction(CtrlH, m_CeilingNormal);
//		}
		else if(d.gravityEnabled)
		{ // Airborne with gravity!
			if (!phys.closeToGround)
			{
				m.airborneDelayTimer = -1; // If ground is too far away, disable airborne delay.
			}
			if(m.airborneDelayTimer>0)
			{
				m.airborneDelayTimer -= Time.fixedDeltaTime;
			}
			else
			{			
				v.primarySurface = -1;
			}
			v.truePrimarySurface = -1;
			AirControl(CtrlH);
			FighterState.Vel = new Vector2 (FighterState.Vel.x, FighterState.Vel.y - 1f);
			//phys.ceilinged = false; ??? reenable if buggy loopdeloops
		}	
			

		d.errorDetectingRecursionCount = 0; //Used for WorldCollizion(); (note: colliZion is used to help searches for the keyword 'collision' by filtering out extraneous matches)

		//print("Velocity before Collizion: "+FighterState.Vel);
		//print("Position before Collizion: "+this.transform.position);

		phys.remainingVelM = 1f;
		phys.remainingMovement = FighterState.Vel*Time.fixedDeltaTime;
		Vector2 startingPos = this.transform.position;

		//print("phys.remainingMovement before collision: "+phys.remainingMovement);

		if(g_FighterCollision && g_FighterCollisionCD <= 0)
		{
			DynamicCollision();
		}

		WorldCollision();

		//print("Per frame velocity at end of Collizion() "+FighterState.Vel*Time.fixedDeltaTime);
		//print("Velocity at end of Collizion() "+FighterState.Vel);
		//print("Per frame velocity at end of updatecontactnormals "+FighterState.Vel*Time.fixedDeltaTime);
		//print("phys.remainingMovement after collision: "+phys.remainingMovement);

		phys.distanceTravelled = new Vector2(this.transform.position.x-startingPos.x,this.transform.position.y-startingPos.y);
		//print("phys.distanceTravelled: "+phys.distanceTravelled);
		//print("phys.remainingMovement: "+phys.remainingMovement);
		//print("phys.remainingMovement after removing phys.distanceTravelled: "+phys.remainingMovement);

		if(phys.initialVel.magnitude>0)
		{
			phys.remainingVelM = (((phys.initialVel.magnitude*Time.fixedDeltaTime)-phys.distanceTravelled.magnitude)/(phys.initialVel.magnitude*Time.fixedDeltaTime));
		}
		else
		{
			phys.remainingVelM = 1f;
		}

		//print("phys.remainingVelM: "+phys.remainingVelM);
		//print("movement after distance travelled: "+phys.remainingMovement);
		//print("Speed this frame: "+FighterState.Vel.magnitude);

		phys.remainingMovement = FighterState.Vel*phys.remainingVelM*Time.fixedDeltaTime;

		//print("Corrected remaining movement: "+phys.remainingMovement);

		Vector2 deltaV = FighterState.Vel-phys.initialVel;
		phys.IGF = deltaV.magnitude;
		phys.CGF += phys.IGF;
		if(phys.CGF>=1){phys.CGF --;}
		if(phys.CGF>=10){phys.CGF -= (phys.CGF/10);}

		if(phys.worldImpact)
		{
			float craterThreshold;
			float slamThreshold;
			float velPunchThreshold;
			AkSoundEngine.SetRTPCValue("GForce_Instant", phys.IGF, this.gameObject);


			if(FighterState.Stance == 2) // More resistant to landing damage if guarding.
			{
				craterThreshold = m.guardCraterT;
				slamThreshold = m.guardSlamT;
				velPunchThreshold = m.velPunchT;
			}
			else
			{
				craterThreshold = m.craterT;
				slamThreshold = m.slamT;
				velPunchThreshold = m.velPunchT;
			}

			if(phys.IGF >= craterThreshold)
			{
				//Time.timeScale = 0.25f;
				float impactStrengthM = ((phys.IGF-craterThreshold)/(1000f-craterThreshold));
				if(impactStrengthM > 1){impactStrengthM = 1;}

				Crater(phys.IGF);

				float damagedealt = g_MinCrtrDMG+((g_MaxCrtrDMG-g_MinCrtrDMG)*impactStrengthM); // Damage dealt scales linearly from minDMG to maxDMG, reaching max damage at a 1000 kph impact.
				float stunTime = g_MinCrtrStun+((g_MaxCrtrStun-g_MinCrtrStun)*impactStrengthM); // Stun duration scales linearly from ...

				g_CurStun = stunTime;				 			// Stunned for stunTime.
				g_Stunned = true;
				TakeDamage((int)damagedealt);		// Damaged by fall.
				if(FighterState.CurVigor < 0){FighterState.CurVigor = 0;}
			}
			else if(phys.IGF >= slamThreshold)
			{
				float impactStrengthM = ((phys.IGF-slamThreshold)/(craterThreshold-slamThreshold)); // Linear scaling between slamThreshold and craterThreshold, value between 0 and 1.

				Slam(phys.IGF);

				float damagedealt = g_MinSlamDMG+((g_MaxSlamDMG-g_MinSlamDMG)*impactStrengthM); // Damage dealt scales linearly from minDMG to maxDMG, as you go from the min Slam Threshold to min Crater Threshold (impact speed)
				float stunTime = g_MinSlamStun+((g_MaxSlamStun-g_MinSlamStun)*impactStrengthM); // Stun duration scales linearly from ...

				g_CurStun = stunTime;				 // Stunned for stunTime.
				g_Staggered = true;
				if(damagedealt >= 0)
				{
					TakeDamage((int)damagedealt);		 // Damaged by fall.
				}
				if(FighterState.CurVigor < 0){FighterState.CurVigor = 0;}
			}
			else if(FighterState.Stance == 1)
			{
				
				float impactStrengthM = ((phys.IGF-velPunchThreshold)/(craterThreshold-velPunchThreshold));

				float damagedealt;
				g_CurStun = 0.1f;	// Stunned for stunTime.
				g_Staggered = true;

				if(phys.IGF>=slamThreshold)
				{
					Slam(phys.IGF);
					damagedealt = g_MinSlamDMG+((g_MaxSlamDMG-g_MinSlamDMG)*impactStrengthM); // Damage dealt scales linearly from minDMG to maxDMG, as you go from the min Slam Threshold to min Crater Threshold (impact speed)
				}
				else
				{
					o.fighterAudio.LandingSound(phys.IGF);
					damagedealt = 0;
				}

				if(damagedealt >= 0)
				{
					TakeDamage((int)damagedealt);		 // Damaged by fall.
				}
				if(FighterState.CurVigor < 0){FighterState.CurVigor = 0;}
			}
			else if(FighterState.Stance == 2) // Guardroll if guard stance mitigated fall damage. More resistant to landing damage.
			{
				v.triggerRollOut = true;
			}
			else
			{
				o.fighterAudio.LandingSound(phys.IGF);
			}
		}

//		FighterState.FinalPos = new Vector2(this.transform.position.x+phys.remainingMovement.x, this.transform.position.y+phys.remainingMovement.y);
//
//		this.transform.position = FighterState.FinalPos;
//		UpdateContactNormals(true);
//
//		DebugUCN();

		this.transform.position = new Vector2(this.transform.position.x+phys.remainingMovement.x, this.transform.position.y+phys.remainingMovement.y);

		UpdateContactNormals(true);

		FighterState.FinalPos = this.transform.position;
	
		//this.GetComponent<Rigidbody2D>().velocity = (Vector3)FighterState.Vel;

		if(FighterState.DevMode&&d.sendCollisionMessages)
		{
			DebugUCN();
		}

		//print("Per frame velocity at end of physics frame: "+FighterState.Vel*Time.fixedDeltaTime);
		//print("phys.remainingMovement at end of physics frame: "+phys.remainingMovement);
		//print("Pos at end of physics frame: "+this.transform.position);
		//print("##############################################################################################");
		//print("FinaL Pos: " + this.transform.position);
		//print("FinaL Vel: " + FighterState.Vel);
		//print("Speed at end of frame: " + FighterState.Vel.magnitude);

	}

	protected virtual void FixedUpdateProcessInput() //FUPI
	{
		phys.worldImpact = false;
		phys.kneeling = false;

		FighterState.PlayerMouseVector = FighterState.MouseWorldPos-Vec2(this.transform.position);
		if(FighterState.LeftClickPress&&(FighterState.DevMode||d.clickToKnockFighter))
		{
			FighterState.Vel += FighterState.PlayerMouseVector*10;
			print("Leftclick detected");
			FighterState.LeftClickPress = false;
		}

		// Once the input has been processed, set the press inputs to false so they don't run several times before being changed by update() again. 
		// FixedUpdate can run multiple times before Update refreshes, so a keydown input can be registered as true multiple times before update changes it back to false, instead of just the intended one time.
		FighterState.LeftClickPress = false; 	
		FighterState.RightClickPress = false;
		FighterState.EtherKeyPress = false;
		FighterState.ShiftKeyPress = false;
		FighterState.DisperseKeyPress = false;				
		FighterState.JumpKeyPress = false;				
		FighterState.LeftKeyPress = false;
		FighterState.RightKeyPress = false;
		FighterState.UpKeyPress = false;
		FighterState.DownKeyPress = false;
	}

	protected virtual void FixedUpdateKinematic() //FUK
	{
		switch (k_KinematicAnim)
		{
		case -1:
			{
				k_IsKinematic = false;
				break;
			}
		case 0: // Strand Jump
			{
				StrandJumpKinematic();
				break; 
			}
		default:
			{
				k_IsKinematic = false;
				break;
			}
		}
	}

	protected virtual void FixedUpdateLogic() //FUL
	{

		if(v.primarySurface==-1)
		{
			m.critJumpFrameTimer = m.critJumpFrameWindow;
			m.critJumpTimer = m.critJumpWindow;
		}
		else
		{
			if(m.critJumpTimer>=0)
			{
				m.critJumpTimer -= Time.fixedDeltaTime;
			}
			if(m.critJumpFrameTimer>=0)
			{
				m.critJumpFrameTimer -= Time.frameCount;
			}
		}

		if( (m.critJumpTimer>0 || m.critJumpFrameTimer>0) && (this.IsPlayer()) ) // If fighter is a player, and has recently hit the ground, crit jump is ready.
		{
//			print("CritJumpTimer:"+m.critJumpTimer);
//			print("CritJumpFrameTimer:"+m.critJumpFrameTimer);
			m.critJumpReady = true;
		}
		else
		{
			m.critJumpReady = false;
		}


		if(g_FighterCollisionCD>0)
		{
			g_FighterCollisionCD -= Time.fixedDeltaTime;
		}

		if(g_CurStun>0)
		{
			g_CurStun -= Time.deltaTime;
			if(g_CurStun<0)
			{
				g_CurStun = 0;
				g_Stunned = false;
				g_Staggered = false;
			}
		}
		else
		{
			g_Stunned = false;
		}
			
		if(FighterState.CurVigor <= 0)
		{
			if(!FighterState.Dead)
			{
				Death();
			}
			FighterState.Dead = true;
			o.anim.SetBool("Dead", true);
			v.currentColor = Color.red;
			o.spriteRenderer.color = v.currentColor;
		}
		else
		{
			FighterState.Dead = false;
			o.anim.SetBool("Dead", false);
		}
	}

	protected virtual void UpdateAnimation()
	{
		o.sparkThrower.transform.position = o.dustSpawnTransform.position;
		ParticleSystem.EmissionModule em = o.sparkThrower.GetComponent<ParticleSystem>().emission;
		if(v.sliding)
		{
			if(v.distFromLastDust<=0)
			{
				if(o.equippedShoe.soundType==1)
				{
					//SpawnSparkEffect();
					em.enabled = true;

				}
				else
				{
					em.enabled = false;
					SpawnDustEffect();
				}

				v.distFromLastDust = v.distBetweenDust;
			}
			else
			{
				v.distFromLastDust -= this.GetSpeed()*Time.deltaTime;
			}
		}
		else
		{
			em.enabled = false;
		}
	}

	protected virtual void FixedUpdateWwiseAudio() // FUWA
	{
		if(v.triggerGenderChange)
		{
			if(v.gender)
			{
				AkSoundEngine.PostEvent("Set_Gender_Male", gameObject);
			}
			else
			{
				AkSoundEngine.PostEvent("Set_Gender_Female", gameObject);
			}
		}
		AkSoundEngine.SetRTPCValue("Vigor", FighterState.CurVigor, this.gameObject);
		AkSoundEngine.SetRTPCValue("Speed", FighterState.Vel.magnitude, this.gameObject);
		AkSoundEngine.SetRTPCValue("WindForce", FighterState.Vel.magnitude, this.gameObject);
		AkSoundEngine.SetRTPCValue("Velocity_X", FighterState.Vel.x, this.gameObject);
		AkSoundEngine.SetRTPCValue("Velocity_Y", FighterState.Vel.y, this.gameObject);
		AkSoundEngine.SetRTPCValue("GForce_Continuous", phys.CGF, this.gameObject);

		//Bools
		AkSoundEngine.SetRTPCValue("Sliding", Convert.ToSingle(isSliding()), this.gameObject);
		AkSoundEngine.SetRTPCValue("Contact_Airborne", Convert.ToSingle(phys.airborne), this.gameObject);
		AkSoundEngine.SetRTPCValue("Contact_Ceiling", Convert.ToSingle(phys.ceilinged), this.gameObject);
		AkSoundEngine.SetRTPCValue("Contact_Ground", Convert.ToSingle(phys.grounded), this.gameObject);
		AkSoundEngine.SetRTPCValue("Contact_Leftwall", Convert.ToSingle(phys.leftWalled), this.gameObject);
		AkSoundEngine.SetRTPCValue("Contact_Rightwall", Convert.ToSingle(phys.rightWalled), this.gameObject);

		//Switches
		if(g_IsInGrass>0)
		{
			AkSoundEngine.SetSwitch("TerrainType", "Grass", gameObject);
		}
	}

	protected virtual void FixedUpdateAnimation() //FUA
	{
		
		v.sliding = false;
		o.anim.SetBool("WallSlide", false);
		o.anim.SetBool("Crouch", false);

		if(g_Staggered && !phys.airborne)
		{
			o.anim.SetBool("Crouch", true);
		}
			
		if(phys.kneeling && !phys.airborne)
		{
			v.sliding = true;
			o.anim.SetBool("Crouch", true);
		}

		if(g_Stunned)
		{
			if(FighterState.Vel.x>0)
			{
				v.facingDirection = true;
			}
			if(FighterState.Vel.x<0)
			{
				v.facingDirection = false;
			}
		}
			
		float fighterGlow = FighterState.EtherLevel;
		if (fighterGlow > 7){fighterGlow = 7;}

		if(fighterGlow>0)
		{
			o.spriteRenderer.material.SetFloat("_Magnitude", fighterGlow);
			//v.currentColor = new Color(1,1,(1f-(fighterGlow/7f)),1);
			v.currentColor = Color.Lerp(v.defaultColor, v.chargedColor, fighterGlow/7);
			o.spriteRenderer.color = v.currentColor;
		}
		else
		{
			o.spriteRenderer.material.SetFloat("_Magnitude", 0);
			v.currentColor = v.defaultColor;
			o.spriteRenderer.color = v.currentColor;
		}

		if(v.flashTimer>0)
		{
			v.flashTimer -= Time.fixedDeltaTime;
			float flashMultiplier = (v.flashTimer/v.flashDuration);
			o.spriteRenderer.color = Color.Lerp(v.currentColor, v.flashColour, flashMultiplier);
		}

		if(v.primarySurface==0)
			FixedGroundAnimation();
		else if(v.primarySurface==1)
		{} 
		else if(v.primarySurface>=2)
			FixedWallAnimation();
		else
			FixedAirAnimation();

		#region sprite rotation code
		bool disableWindLean = false;
		float surfaceLeanM = GetSpeed()/v.speedForMaxLean; // Player leans more the faster they're going. At max speed, the player model rotates so the ground is directly below them.
		surfaceLeanM = (surfaceLeanM<1) ? surfaceLeanM : 1; // If greater than 1, clamp to 1.

		float spriteAngle;
		float testAngle = 0;

		if((v.primarySurface != -1) && (Mathf.Abs(GetVelocity().magnitude)>=v.highSpeedModeT)) // If airborne and moving fast
		{
			v.highSpeedMode = true;  // if player is moving fast, change their animations.
		}

		if(Mathf.Abs(GetVelocity().magnitude)<v.highSpeedModeT)
		{
			v.highSpeedMode = false; // if player is moving slow, change their animation to normal.
		}

		if(v.primarySurface == 0)
		{
			spriteAngle = Get2DAngle(Perp(m_GroundNormal));
		}
		else if(v.primarySurface == 1)
		{
			spriteAngle = Get2DAngle(Perp(m_CeilingNormal));
		}
		else if(v.primarySurface == 2)
		{
			spriteAngle = Get2DAngle(Perp(m_LeftNormal));
			//if(v.wallSliding)
			surfaceLeanM = 1;
			disableWindLean = true;
		}
		else if(v.primarySurface == 3)
		{
			spriteAngle = Get2DAngle(Perp(m_RightNormal));
			//if(v.wallSliding)
			surfaceLeanM = 1;
			disableWindLean = true;
		}
		else
		{
			disableWindLean = true;
			spriteAngle = Get2DAngle(GetVelocity(), 0);
			testAngle = spriteAngle;

			if(GetVelocity().y<0)
			{
				if(spriteAngle>0)
				{
					spriteAngle = -180+spriteAngle;
				}
				else
				{
					spriteAngle = 180+spriteAngle;
				}
				spriteAngle *= 0.5f;
			}


			//print("spriteAngle: "+spriteAngle);
			float angleScaling = 1;

			angleScaling = Mathf.Abs(GetVelocity().y/50);//*Mathf.Abs(GetVelocity().y/20); // Parabola approaching zero at y = 0, and extending to positive infinity on either side.

			angleScaling = (angleScaling>1) ? 1 : angleScaling; // Clamp at 1.

			surfaceLeanM = angleScaling;

			if(v.highSpeedMode)
			{
				surfaceLeanM = 1; 
				spriteAngle = Get2DAngle(GetVelocity(), 0);
			}

			//print("testAngle: "+testAngle+", finalangle: "+(surfaceLeanM*spriteAngle));
		}

		if(o.anim.GetBool("Crouch"))
		{
			surfaceLeanM = 1;
			//print("CROUCHING!!!!");
			disableWindLean = true;
		}
		if(!disableWindLean)
		{
			float leanIntoWindAngle = Get2DAngle(GetVelocity(), 0);
			//print("leanIntoWindAngle"+leanIntoWindAngle);

			if(FighterState.Vel.magnitude>(v.highSpeedModeT-25) && FighterState.Vel.magnitude<v.highSpeedModeT) // Approaching projectile mode
			{
				float fadein = (FighterState.Vel.magnitude-(v.highSpeedModeT-25))/(25);
				spriteAngle = ((leanIntoWindAngle*fadein)+(spriteAngle*2))/3;
				//print("SpriteAngle: "+spriteAngle);
			}
			else if(FighterState.Vel.magnitude>=v.highSpeedModeT && FighterState.Vel.magnitude<(v.highSpeedModeT+25)) // in projectile mode
			{
				float leanOutOfWindAng = Get2DAngle(-GetVelocity(), 0);
				if(Math.Abs(leanOutOfWindAng+spriteAngle)<(Mathf.Abs(leanOutOfWindAng)+Mathf.Abs(spriteAngle))) // If one angle is negative while the other is positive, invert the sign of leanoutofwindang so they match.
				{
					leanOutOfWindAng *= -1;
				}
				float fadeOut = (FighterState.Vel.magnitude-v.highSpeedModeT)/(25);
				if(fadeOut>1)
					fadeOut = 1;
				spriteAngle = ((leanOutOfWindAng*(1-fadeOut))+(spriteAngle*2))/3;
				print("SpriteAngle: "+spriteAngle);
				print("leanOutOfWindAng: "+leanOutOfWindAng);
				print("fadein: "+fadeOut);
			}
		}
		v.leanAngle = spriteAngle*surfaceLeanM; //remove this and enable lerp.
		Quaternion finalAngle = new Quaternion();
		finalAngle.eulerAngles = new Vector3(0,0, v.leanAngle);
		o.spriteTransform.localRotation = finalAngle;
		//
		// End of sprite transform positioning code
		//
		#endregion

		float relativeAimDirection = -Get2DAngle((Vector2)FighterState.MouseWorldPos-(Vector2)this.transform.position, -v.leanAngle);
		if(phys.kneeling&&!phys.airborne)
		{
			//print("RAD = "+relativeAimDirection+", LeanAngle = "+v.leanAngle);
			if(this.IsPlayer())
			{
				if(relativeAimDirection < 0)
				{
					v.facingDirection = false;
				}
				else
				{
					v.facingDirection = true;
				}
			}
		}


		//
		// Debug collision visualization code.
		//
		if( isLocalPlayer )
		{
			if( FighterState.DevMode )
			{
				o.debugAngleDisplay.gameObject.SetActive(true);
				Quaternion debugQuaternion = new Quaternion();
				debugQuaternion.eulerAngles = new Vector3(0, 0, testAngle);
				o.debugAngleDisplay.localRotation = debugQuaternion;
			}
			else
			{
				o.debugAngleDisplay.gameObject.SetActive(false);
			}
		}
		Vector3[] debugLineVector = new Vector3[3];

		debugLineVector[0].x = -phys.distanceTravelled.x;
		debugLineVector[0].y = -(phys.distanceTravelled.y+(m_GroundFootLength-m.maxEmbed));
		debugLineVector[0].z = 0f;

		debugLineVector[1].x = 0f;
		debugLineVector[1].y = -(m_GroundFootLength-m.maxEmbed);
		debugLineVector[1].z = 0f;

		debugLineVector[2].x = phys.remainingMovement.x;
		debugLineVector[2].y = (phys.remainingMovement.y)-(m_GroundFootLength-m.maxEmbed);
		debugLineVector[2].z = 0f;

		d.debugLine.SetPositions(debugLineVector);

		if(FighterState.Vel.magnitude >= m.tractionChangeT )
		{
			d.debugLine.endColor = Color.white;
			d.debugLine.startColor = Color.white;
		}   
		else
		{   
			d.debugLine.endColor = Color.blue;
			d.debugLine.startColor = Color.blue;
		}

		//
		// End of debug line code
		//

		//
		// Mecanim variable assignment. Last step of animation code.
		//

		if(!v.facingDirection) //If facing left
		{
			o.anim.SetBool("IsFacingRight", false);
			o.spriteTransform.localScale = new Vector3(-1f, 1f, 1f);
		}
		else
		{
			o.anim.SetBool("IsFacingRight", true);
			o.spriteTransform.localScale = new Vector3(1f, 1f, 1f);
		}
			
		if(phys.kneeling)
		{
			o.anim.SetFloat("AimAngle", relativeAimDirection);
		}
		else
		{
			float stoppingAngle = FighterState.Vel.magnitude*2;
			if(stoppingAngle>60)
				stoppingAngle = 60;
			if(stoppingAngle<30)
				stoppingAngle = 30;
			
			o.anim.SetFloat("AimAngle", stoppingAngle);
		}

		o.anim.SetInteger("PrimarySurface", v.primarySurface);
		o.anim.SetFloat("Speed", FighterState.Vel.magnitude);
		o.anim.SetFloat("hSpeed", Math.Abs(FighterState.Vel.x));
		o.anim.SetFloat("vSpeed", Math.Abs(FighterState.Vel.y));
		o.anim.SetFloat("hVelocity", FighterState.Vel.x);
		o.anim.SetFloat("vVelocity", FighterState.Vel.y);
		o.anim.SetInteger("Stance", FighterState.Stance);
		o.anim.SetBool("Stunned", g_Stunned);
		o.anim.SetBool("Staggered", g_Staggered);
		o.anim.SetFloat("ProjectileMode", Convert.ToSingle(v.highSpeedMode));

		if(v.triggerAtkHit)
		{
			v.triggerAtkHit = false;
			o.anim.SetBool("TriggerPunchHit", true);
		}
		else if(v.triggerRollOut)
		{
			v.triggerRollOut = false;
			o.anim.SetBool("TriggerRollOut", true);
		}
		else if(v.triggerFlinched)
		{
			v.triggerFlinched = false;
			o.anim.SetBool("TriggerFlinched", true);
		}


		float multiplier = 1; // Animation playspeed multiplier that increases with higher velocity

		if(FighterState.Vel.magnitude > 20.0f)
			multiplier = ((FighterState.Vel.magnitude - 20) / 20)+1;
		
		o.anim.SetFloat("Multiplier", multiplier);

		//
		// Surface-material-type sound code
		//


		if(v.primarySurface != -1)
		{
			if(directionContacts[v.primarySurface])
			{
				String terrainType = "Concrete";
				RaycastHit2D surfaceHit = directionContacts[v.primarySurface];
				if(surfaceHit.collider.sharedMaterial!=null)
				{
					terrainType = surfaceHit.collider.sharedMaterial.name;
				}
				if(terrainType==null || terrainType=="")
				{
					terrainType = "Concrete";
				} 
				AkSoundEngine.SetSwitch("TerrainType", terrainType, gameObject);
			}
		}
	}

	protected virtual void FixedGroundAnimation()
	{		
		if (!v.facingDirection) //If facing left
		{
//			o.anim.SetBool("IsFacingRight", false);
//			o.spriteTransform.localScale = new Vector3 (-1f, 1f, 1f);
			if(FighterState.Vel.x > 0 && !phys.airborne) // && FighterState.Vel.magnitude >= v.reversingSlideT 
			{
				o.anim.SetBool("Crouch", true);
				v.sliding = true;
			}
		} 
		else //If facing right
		{
//			o.anim.SetBool("IsFacingRight", true);
//			o.spriteTransform.localScale = new Vector3 (1f, 1f, 1f);
			if(FighterState.Vel.x < 0 && !phys.airborne) // && FighterState.Vel.magnitude >= v.reversingSlideT 
			{
				o.anim.SetBool("Crouch", true);
				v.sliding = true;
			}
		}
	}

	protected virtual void FixedAirAnimation()
	{
		
	}

	protected virtual void FixedWallAnimation()
	{
		if(v.wallSliding)
		{
			o.anim.SetBool("WallSlide", true);
			v.sliding = true;
		}

		if (!v.facingDirection) //If facing left
		{
//			o.anim.SetBool("IsFacingRight", false);
//			o.spriteTransform.localScale = new Vector3 (-1f, 1f, 1f);
			if(v.primarySurface == 3 && FighterState.Vel.y > 0 && !phys.airborne) // If facing down and moving up, go into crouch stance.
			{
				//print("Running down on rightwall!");
				o.anim.SetBool("Crouch", true);
				v.sliding = true;
			}
		} 
		else //If facing right
		{
//			o.anim.SetBool("IsFacingRight", true);
//			o.spriteTransform.localScale = new Vector3 (1f, 1f, 1f);
			if(v.primarySurface == 2 && FighterState.Vel.y > 0 && !phys.airborne) // If facing down and moving up, go into crouch stance.
			{
				//print("Running down on leftwall!");
				o.anim.SetBool("Crouch", true);
				v.sliding = true;
			}
		}
			
	}

	protected virtual void UpdatePlayerInput()
	{
		// Extended by Player class.
	}

	protected Vector2 Vec2(Vector3 inputVector)
	{
		return new Vector2(inputVector.x, inputVector.y);
	}

	protected void StrandJumpKinematic() //SJK
	{
		d.errorDetectingRecursionCount = 0; //Used for WorldCollizion(); (note: colliZion is used to help searches for the keyword 'collision' by filtering out extraneous matches)
		phys.distanceTravelled = Vector2.zero;
		phys.remainingMovement = FighterState.Vel*Time.fixedDeltaTime;
		Vector2 startingPos = this.transform.position;


		FighterState.Vel *= k_StrandJumpSlowdownM;
//		if(FighterState.Vel.magnitude>=k_StrandJumpSlowdownLinear)
//		{
//			FighterState.Vel -= FighterState.Vel.normalized*k_StrandJumpSlowdownLinear;
//		}

		if(FighterState.Vel.magnitude<=1f)
		{ // If stopped, reflect in the other direction now.
			k_IsKinematic = false;
			k_KinematicAnim = -1;
			InstantForce(m.strandJumpReflectDir, m.strandJumpReflectSpd);
		}

		if(g_FighterCollision)
		{
			DynamicCollision();
		}

		WorldCollision();

		//print("phys.remainingVelM: "+phys.remainingVelM);
		//print("movement after distance travelled: "+phys.remainingMovement);
		//print("Speed this frame: "+FighterState.Vel.magnitude);

		phys.remainingMovement = FighterState.Vel*phys.remainingVelM*Time.fixedDeltaTime;

		//print("Corrected remaining movement: "+phys.remainingMovement);

		this.transform.position = new Vector2(this.transform.position.x+phys.remainingMovement.x, this.transform.position.y+phys.remainingMovement.y);

		FighterState.FinalPos = this.transform.position;

		if(FighterState.DevMode&&d.sendCollisionMessages)
		{
			DebugUCN();
		}
	}

	public void Crater(float impactForce) // Triggered when character impacts anything REALLY hard.
	{
		float impactStrengthM = ((impactForce-m.craterT)/(1000f-m.craterT));
		if(impactStrengthM > 1){impactStrengthM = 1;}
		float camShakeM = (impactForce+m.craterT)/(2*m.craterT);

		if(camShakeM >=2){camShakeM = 2;}
		float Magnitude = camShakeM;
		float Roughness = 10f;
		float FadeOutTime = 2.5f;
		float FadeInTime = 0f;
		Vector3 RotInfluence = new Vector3(1,1,1);
		Vector3 PosInfluence = new Vector3(0.15f,0.15f,0.15f);
		CameraShaker.Instance.ShakeOnce(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);

		AkSoundEngine.SetRTPCValue("GForce_Instant", phys.IGF, this.gameObject);
		//o.fighterAudio.CraterSound(impactForce, m.craterT, 1000f);

		SpawnShockEffect(this.phys.initialVel.normalized);
		SpawnExplosionEffect(impactForce);

//		GameObject newAirBurst = (GameObject)Instantiate(p_AirBurstPrefab, this.transform.position, Quaternion.identity);
		//		newAirBurst.GetComponentInChildren<AirBurst>().Create(true, 30+70*linScaleModifier, 0.4f, impactForce); 					//Set the parameters of the shockwave.
//		newAirBurst.name = "Shockwave";
		GameObject newWindGust = (GameObject)Instantiate(p_AirBurstPrefab, this.transform.position, Quaternion.identity);
		newWindGust.GetComponentInChildren<AirBurst>().Create(false, 0, 30+70*impactStrengthM, 0.8f, impactStrengthM*3, impactForce); 		//Set the parameters of the afterslam wind.
		newWindGust.name = "AirGust";
	}

	public void ShakeFighter(float mag)
	{
		if(!this.IsPlayer()){return;} 
		float Magnitude = mag;
		float Roughness = 10f;
		float FadeOutTime = 2.5f;
		float FadeInTime = 0f;
		Vector3 RotInfluence = new Vector3(1,1,1);
		Vector3 PosInfluence = new Vector3(0.15f,0.15f,0.15f);
		CameraShaker.Instance.ShakeOnce(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);
	}

	public void ShakeFighter(float myMagnitude, float myRoughness, float myFadeInTime, float myFadeOutTime, Vector3 myPosInfluence, Vector3 myRotInfluence)
	{
		if(!this.IsPlayer()){return;} 
		float Magnitude = myMagnitude;
		float Roughness = myRoughness;
		float FadeInTime = myFadeInTime;
		float FadeOutTime = myFadeOutTime;
		Vector3 PosInfluence = myPosInfluence;
		Vector3 RotInfluence = myRotInfluence;
		CameraShaker.Instance.ShakeOnce(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);

	}

	public void Slam(float impactForce) // Triggered when character impacts anything too hard.
	{
		float impactStrengthM = ((impactForce-m.slamT)/(m.craterT-m.slamT));
		float camShakeM = (impactForce+m.slamT)/(2*m.slamT);
		if(camShakeM >=2){camShakeM = 2;}
		float Magnitude = 0.5f;
		float Roughness = 20f;
		float FadeOutTime = 0.6f*camShakeM;
		float FadeInTime = 0f;
		float posM = 0.3f*camShakeM;
		Vector3 RotInfluence = new Vector3(0,0,0);
		Vector3 PosInfluence = new Vector3(posM,posM,0);
		CameraShaker.Instance.ShakeOnce(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);

		AkSoundEngine.SetRTPCValue("GForce_Instant", phys.IGF, this.gameObject);
		o.fighterAudio.SlamSound(impactForce, m.slamT, m.craterT);

		if(g_VelocityPunching)
		{
			SpawnShockEffect(this.phys.initialVel.normalized);
		}
		GameObject newAirBurst = (GameObject)Instantiate(p_AirBurstPrefab, this.transform.position, Quaternion.identity);
		newAirBurst.GetComponentInChildren<AirBurst>().Create(true, 30*impactStrengthM, impactStrengthM*0.8f, impactForce*2); //Set the parameters of the shockwave.
	}

	protected void DynamicCollision()
	{
		#region fightercollisions
		if(!this.isAlive()){return;}
		// FIGHTER-FIGHTER COLLISION TESTING IS SEPERATE AND PRECEDES WORLD COLLISIONS
		float crntSpeed = FighterState.Vel.magnitude*Time.fixedDeltaTime; //Current speed.
		RaycastHit2D[] fighterCollision = Physics2D.RaycastAll(this.transform.position, FighterState.Vel, crntSpeed, m_FighterMask);

		foreach(RaycastHit2D hit in fighterCollision)
		{
			if(hit.collider.gameObject != this.gameObject)
			{
				//print("HIT: "+hit.collider.transform.gameObject);//GetComponent<>());
				if(hit.collider.GetComponent<FighterChar>())
				{
					if(hit.collider.GetComponent<FighterChar>().isAlive())
					{
						bool isFighterCol = FighterCollision(hit.collider.GetComponent<FighterChar>());
						if(isFighterCol)
						{
							v.facingDirection = (hit.collider.transform.position.x-this.transform.position.x < 0) ? false : true; // If enemy is to your left, face left. Otherwise, right.
							hit.collider.GetComponent<FighterChar>().v.facingDirection = !v.facingDirection;
						}
					}
				}
			}
		}
		#endregion
	}

	protected void WorldCollision()	// Handles all collisions with terrain geometry (and fighters).
	{
		//print ("Collision->phys.grounded=" + phys.grounded);
		float crntSpeed = FighterState.Vel.magnitude*Time.fixedDeltaTime; //Current speed.
		//print("DC Executing");
		d.errorDetectingRecursionCount++;

		if(d.errorDetectingRecursionCount >= 5)
		{
			throw new Exception("Your recursion code is not working!");
			//return;
		}

		if(FighterState.Vel.x > 0.001f)
		{
			phys.leftWallBlocked = false;
		}

		if(FighterState.Vel.x < -0.001f)
		{
			phys.rightWallBlocked = false;
		}

		#region worldcollision raytesting

		Vector2 adjustedBot = m_GroundFoot.position; // AdjustedBot marks the end of the ground raycast, but 0.02 shorter.
		adjustedBot.y += m.maxEmbed;

		Vector2 adjustedTop = m_CeilingFoot.position; // AdjustedTop marks the end of the ceiling raycast, but 0.02 shorter.
		adjustedTop.y -= m.maxEmbed;

		Vector2 adjustedLeft = m_LeftSide.position; // AdjustedLeft marks the end of the left wall raycast, but 0.02 shorter.
		adjustedLeft.x += m.maxEmbed;

		Vector2 adjustedRight = m_RightSide.position; // AdjustedRight marks the end of the right wall raycast, but 0.02 shorter.
		adjustedRight.x -= m.maxEmbed;

		//RaycastHit2D groundCheck = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength, mask);
		RaycastHit2D[] predictedLoc = new RaycastHit2D[4];
		//These raycasts fire from the 4 edges of the player collider in the direction of travel, effectively forming a projection of the player. This is a form of continuous collision detection.
		predictedLoc[0] = Physics2D.Raycast(adjustedBot, FighterState.Vel, crntSpeed, m_TerrainMask); 	// Ground
		predictedLoc[1] = Physics2D.Raycast(adjustedTop, FighterState.Vel, crntSpeed, m_TerrainMask); 	// Ceiling
		predictedLoc[2] = Physics2D.Raycast(adjustedLeft, FighterState.Vel, crntSpeed, m_TerrainMask); 	// Left
		predictedLoc[3] = Physics2D.Raycast(adjustedRight, FighterState.Vel, crntSpeed, m_TerrainMask);	// Right  

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
				if ((moveDirectionNormal != predictedLoc[0].normal) && (invertedDirectionNormal != predictedLoc[0].normal)) 
				{ // If the slope you're hitting is different than your current slope.

					//
					// Start of Ascendant Snag detection
					// This is when the player's foot hits a surface that is less vertical than its current trajectory, and gets pulled downward onto the flatter slope. 
					// The code detects this circumstance by comparing the new surface's y value to the y value of the player trajectory. If the player's y value is higher, they are hitting the surface from the underside with their foot collider, which should be ignored.

					Vector2 predictedPerp = Perp(predictedLoc[0].normal);
					if(predictedPerp.x==0)
					{
						predictedPerp = Perp(predictedPerp);
					}

					if((predictedPerp.x<0 && FighterState.Vel.x>0) || (predictedPerp.x>0 && FighterState.Vel.x<0))
					{
						//print("flipping predictedPerp...");
						predictedPerp *= -1;
					}

					if(FighterState.Vel.normalized.y>predictedPerp.y)
					{
						if(d.sendCollisionMessages)
						{
							print("MD: "+FighterState.Vel.normalized);
							print("PDL: "+predictedPerp);
							print("Ascendant snag detected.");
						}
						return;
					}
					//
					// End of Ascendant Snag detection
					//
					if(ToGround(predictedLoc[0]))
					{
						DirectionChange(m_GroundNormal);
					}


					return;
				}
				else // When the slope you're hitting is the same as your current slope, no action is needed.
				{
					if(invertedDirectionNormal == predictedLoc[0].normal&&d.sendCollisionMessages)
					{
						throw new Exception("INVERTED GROUND IMPACT NORMAL DETECTED!");
					}
					return;
				}
			}
		case 1: //Ceiling collision
			{
				if ((moveDirectionNormal != predictedLoc[1].normal) && (invertedDirectionNormal != predictedLoc[1].normal)) 
				{ // If the slope you're hitting is different than your current slope.
					//print("CEILINm_Impact");
					if(ToCeiling(predictedLoc[1]))
					{
						DirectionChange(m_CeilingNormal);
					}
					return;
				}
				else // When the slope you're hitting is the same as your current slope, no action is needed.
				{
					if(invertedDirectionNormal == predictedLoc[1].normal&&d.sendCollisionMessages)
					{
						throw new Exception("INVERTED CEILING IMPACT NORMAL DETECTED!");
					}
					return;
				}
			}
		case 2: //Left contact collision
			{
				if ((moveDirectionNormal != predictedLoc[2].normal) && (invertedDirectionNormal != predictedLoc[2].normal)) 
				{ // If the slope you're hitting is different than your current slope.
					//print("LEFT_IMPACT");
					if(ToLeftWall(predictedLoc[2]))
					{
						DirectionChange(m_LeftNormal);
					}
					return;
				}
				else // When the slope you're hitting is the same as your current slope, no action is needed.
				{
					if(invertedDirectionNormal == predictedLoc[2].normal&&d.sendCollisionMessages)
					{
						throw new Exception("INVERTED LEFT IMPACT NORMAL DETECTED!");
					}
					return;
				}
			}
		case 3: //Right contact collision
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
				else // When the slope you're hitting is the same as your current slope, no action is needed.
				{
					if(invertedDirectionNormal == predictedLoc[3].normal&&d.sendCollisionMessages)
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

	protected virtual void EtherPulse()
	{
		if(FighterState.EtherLevel <= 0)
		{
			return;
		}

		FighterState.EtherLevel--;
		//o_ProximityLiner.ClearAllFighters();
		GameObject newEtherPulse = (GameObject)Instantiate(p_EtherPulse, this.transform.position, Quaternion.identity);
		newEtherPulse.GetComponentInChildren<EtherPulse>().originFighter = this;
		newEtherPulse.GetComponentInChildren<EtherPulse>().pulseRange = 150+(FighterState.EtherLevel*50);
		//o_ProximityLiner.outerRange = 100+(FighterState.EtherLevel*25);
		o.fighterAudio.EtherPulseSound();
	}

	protected float GetSteepness(Vector2 vectorPara)
	{
		return 0f;
	}

	protected void Traction(float horizontalInput, float inputV)
	{
		Vector2 groundPara = Perp(m_GroundNormal);
		if(d.sendTractionMessages){print("Traction");}

		float linAccel = this.m.linearAccelRate;
		float fastAccel = this.m.startupAccelRate;
		float topSpeed = this.m.maxRunSpeed;

		if(FighterState.Stance==2) // If in guard stance
		{
			linAccel /= 2;
			fastAccel /= 2;
			topSpeed /= 2;
		}

		// This block of code makes the player treat very steep left and right surfaces as walls when they aren't going fast enough to reasonably climb them. 
		// This aims to prevent a jittering effect when the player build small amounts of speed, then hits the steeper slope and starts sliding down again 
		// as frequently as 60 times a second.
		if (this.GetSpeed() <= 0.0001f) 
		{
			//print("Hitting wall slowly, considering correction.");
			float wallSteepnessAngle;

			if ((phys.leftWalled) && (horizontalInput < 0)) 
			{
				//print("Trying to run up left wall slowly.");
				Vector2 wallPara = Perp (m_LeftNormal);
				wallSteepnessAngle = Vector2.Angle (Vector2.up, wallPara);
				if (wallSteepnessAngle == 180) 
				{
					wallSteepnessAngle = 0;
				}
				if (wallSteepnessAngle >= m.tractionLossMaxAngle) 
				{ //If the wall surface the player is running
					//print("Wall steepness of "+wallSteepnessAngle+" was too steep for speed "+this.GetSpeed()+", stopping.");
					FighterState.Vel = Vector2.zero;
					phys.leftWallBlocked = true;
				}
			} 
			else if ((phys.rightWalled) && (horizontalInput > 0)) 
			{
				//print("Trying to run up right wall slowly.");
				Vector2 wallPara = Perp(m_RightNormal);
				wallSteepnessAngle = Vector2.Angle (Vector2.up, wallPara);
				wallSteepnessAngle = 180f - wallSteepnessAngle;
				if (wallSteepnessAngle == 180) 
				{
					wallSteepnessAngle = 0;
				}
				if (wallSteepnessAngle >= m.tractionLossMaxAngle) 
				{ //If the wall surface the player is running
					//print("Wall steepness of "+wallSteepnessAngle+" was too steep for speed "+this.GetSpeed()+", stopping.");
					FighterState.Vel = Vector2.zero;
					phys.rightWallBlocked = true;
				}
			}
			else 
			{
				//print("Only hitting groundcontact, test ground steepness.");
			}

		}
		// End of anti-slope-jitter code.

		if(inputV<0)
		{
			phys.kneeling = true;
			horizontalInput = 0;
//			if(GetEtherLevel()>0)
//			{
//				v.cameraMode = 2;
//			}
		}
//		else
//		{
//			v.cameraMode = v.defaultCameraMode;
//		}

		if(groundPara.x > 0)
		{
			groundPara *= -1;
		}

		if(d.sendTractionMessages){print("gp="+groundPara);}

		float steepnessAngle = Vector2.Angle(Vector2.left,groundPara);

		steepnessAngle = (float)Math.Round(steepnessAngle,2);
		if(d.sendTractionMessages){print("SteepnessAngle:"+steepnessAngle);}

		float slopeMultiplier = 0;

		if(steepnessAngle > m.tractionLossMinAngle)
		{
			if(steepnessAngle >= m.tractionLossMaxAngle)
			{
				if(d.sendTractionMessages){print("MAXED OUT!");}
				slopeMultiplier = 1;
			}
			else
			{
				slopeMultiplier = ((steepnessAngle-m.tractionLossMinAngle)/(m.tractionLossMaxAngle-m.tractionLossMinAngle));
			}

			if(d.sendTractionMessages){print("slopeMultiplier: "+slopeMultiplier);}
			//print("groundParaY: "+groundParaY+", slopeT: "+slopeT);
		}


		if(((phys.leftWallBlocked)&&(horizontalInput < 0)) || ((phys.rightWallBlocked)&&(horizontalInput > 0)))
		{// If running at an obstruction you're up against.
			//print("Running against a wall.");
			horizontalInput = 0;
		}

		//print("Traction executing");
		float rawSpeed = FighterState.Vel.magnitude;
		if(d.sendTractionMessages){print("FighterState.Vel.magnitude: "+FighterState.Vel.magnitude);}

		if (horizontalInput == 0||phys.kneeling) 
		{//if not pressing any move direction, slow to zero linearly.
			if(d.sendTractionMessages){print("No input, slowing...");}
			if(phys.kneeling) // Decelerate faster if crouching
			{
				if(rawSpeed <= 0.5f)
				{
					FighterState.Vel = Vector2.zero;	
				}
				else
				{
					FighterState.Vel = ChangeSpeedLinear(FighterState.Vel, -m.linearStopRate);
				}
			}
			else 
			{
				if(rawSpeed <= 0.5f)
				{
					FighterState.Vel = Vector2.zero;	
				}
				else
				{
					FighterState.Vel = ChangeSpeedLinear(FighterState.Vel, -m.linearSlideRate);
				}
			}
		}
		else if((horizontalInput > 0 && FighterState.Vel.x >= 0) || (horizontalInput < 0 && FighterState.Vel.x <= 0))
		{//if pressing same button as move direction, move to MAXSPEED.
			if(d.sendTractionMessages){print("Moving with keypress");}
			if(rawSpeed < topSpeed)
			{
				if(d.sendTractionMessages){print("Rawspeed("+rawSpeed+") less than max");}
				if(rawSpeed > m.tractionChangeT)
				{
					if(d.sendTractionMessages){print("LinAccel-> " + rawSpeed);}
					if(FighterState.Vel.y > 0)
					{ 	// If climbing, recieve uphill movement penalty.
						FighterState.Vel = ChangeSpeedLinear(FighterState.Vel, linAccel*(1-slopeMultiplier));
					}
					else
					{
						FighterState.Vel = ChangeSpeedLinear(FighterState.Vel, linAccel);
					}
				}
				else if(rawSpeed < 0.001f)
				{
//					if(slopeMultiplier<0.5)
//					{
//						FighterState.Vel = new Vector2((m_Acceleration)*horizontalInput*(1-slopeMultiplier), 0);
//					}
//					else
//					{
//						if(d.sendTractionMessages){print("Too steep!");}
//					}
//					if(d.sendTractionMessages){print("Starting motion. Adding " + m_Acceleration);}
					if(d.sendTractionMessages){print("HardAccel-> " + rawSpeed);}
					if(FighterState.Vel.y > 0)
					{ 	// If climbing, recieve uphill movement penalty.
						FighterState.Vel = new Vector2(fastAccel*(1-slopeMultiplier)*horizontalInput, 0);
					}
					else
					{
						FighterState.Vel = new Vector2(fastAccel*horizontalInput, 0);
					}
				}
				else
				{
//					//print("ExpAccel-> " + rawSpeed);
//					float eqnX = (1+Mathf.Abs((1/m.tractionChangeT )*rawSpeed));
//					float curveMultiplier = 1+(1/(eqnX*eqnX)); // Goes from 1/4 to 1, increasing as speed approaches 0.
//
//					float addedSpeed = curveMultiplier*(m_Acceleration);
//					if(FighterState.Vel.y > 0)
//					{ // If climbing, recieve uphill movement penalty.
//						addedSpeed = curveMultiplier*(m_Acceleration)*(1-slopeMultiplier);
//					}
//					if(d.sendTractionMessages){print("Addedspeed:"+addedSpeed);}
//					FighterState.Vel = (FighterState.Vel.normalized)*(rawSpeed+addedSpeed);
//					if(d.sendTractionMessages){print("FighterState.Vel:"+FighterState.Vel);}
					if(d.sendTractionMessages){print("HardAccel-> " + rawSpeed);}
					if(FighterState.Vel.y > 0)
					{ 	// If climbing, recieve uphill movement penalty.
						FighterState.Vel = ChangeSpeedLinear(FighterState.Vel, fastAccel*(1-slopeMultiplier));
					}
					else
					{
						FighterState.Vel = ChangeSpeedLinear(FighterState.Vel, fastAccel);
					}
				}
			}
			else
			{
				if(rawSpeed < topSpeed+1)
				{
					rawSpeed = topSpeed;
					SetSpeed(FighterState.Vel,topSpeed);
				}
				else
				{
					if(d.sendTractionMessages){print("Rawspeed("+rawSpeed+") more than max.");}
					FighterState.Vel = ChangeSpeedLinear (FighterState.Vel, -m.linearOverSpeedRate);
				}
			}
		}
		else if((horizontalInput > 0 && FighterState.Vel.x < 0) || (horizontalInput < 0 && FighterState.Vel.x > 0))
		{//if pressing button opposite of move direction, slow to zero quickly.
			if(d.sendTractionMessages){print("LinDecel");}
			FighterState.Vel = ChangeSpeedLinear (FighterState.Vel, -m.linearStopRate);

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

		FighterState.Vel += downSlope*m.slippingAcceleration*slopeMultiplier;

		//	TESTINGSLOPES
		if(d.sendTractionMessages){print("downSlope="+downSlope);}
		if(d.sendTractionMessages){print("m.slippingAcceleration="+m.slippingAcceleration);}
		if(d.sendTractionMessages){print("slopeMultiplier="+slopeMultiplier);}

		//ChangeSpeedLinear(FighterState.Vel, );
		if(d.sendTractionMessages){print("PostTraction velocity: "+FighterState.Vel);}
	}

	protected void AirControl(float horizontalInput)
	{
		FighterState.Vel += new Vector2(horizontalInput/20, 0);
	}
		
	protected void WallTraction(float hInput, float vInput, Vector2 wallSurface)
	{
		if(vInput>0)
		{
			//print("FALLIN OFF YO!");
			AirControl(vInput);
			return;
		}
		else if(vInput<0)
		{
			phys.kneeling = true;
			hInput = 0;
//			if(v.defaultCameraMode==3)
//			{
//				v.cameraMode = 2;
//			}
		}

		if(phys.leftWalled) 	// If going up the left side wall, reverse horizontal input. This makes it so when control scheme is rotated 90 degrees, the key facing the wall will face up always. 
		{					// On walls the horizontal movekeys control vertical movement.
			hInput *= -1;
		}

		////////////////////
		// Variable Setup //
		////////////////////
		Vector2 wallPara = Perp(wallSurface);

		//print("hInput="+hInput);


		if(wallPara.x > 0)
		{
			wallPara *= -1;
		}

		float steepnessAngle = Vector2.Angle	(Vector2.up,wallPara);

		if(phys.rightWalled)
		{
			steepnessAngle = 180f - steepnessAngle;
		}

		if(steepnessAngle == 180)
		{
			steepnessAngle=0;
		}

		if(steepnessAngle > 90 && (wallSurface != m.expiredNormal)) //If the sliding surface is upside down, and hasn't already been clung to.
		{
			if(!phys.surfaceCling)
			{
				m.timeSpentHanging = 0;
				m.maxTimeHanging = 0;
				phys.surfaceCling = true;
				if(phys.CGF >= m.clingReqGForce)
				{
					m.maxTimeHanging = m.surfaceClingTime;
				}
				else
				{
					m.maxTimeHanging = m.surfaceClingTime*(phys.CGF/m.clingReqGForce);
				}
				//print("m.maxTimeHanging="+m.maxTimeHanging);
			}
			else
			{
				m.timeSpentHanging += Time.fixedDeltaTime;
				//print("time=("+m.timeSpentHanging+"/"+m.maxTimeHanging+")");
				if(m.timeSpentHanging>=m.maxTimeHanging)
				{
					phys.surfaceCling = false;
					m.expiredNormal = wallSurface;
					//print("EXPIRED!");
				}
			}
		}
		else
		{
			phys.surfaceCling = false;
			m.timeSpentHanging = 0;
			m.maxTimeHanging = 0;
		}


		//
		// This code block is likely unnecessary
		// Anti-Jitter code for transitioning to a steep slope that is too steep to climb.
		//
		//		if (this.GetSpeed () <= 0.0001f) 
		//		{
		//			print ("RIDING WALL SLOWLY, CONSIDERING CORRECTION");
		//			if ((phys.leftWalled) && (hInput < 0)) 
		//			{
		//				if (steepnessAngle >= m.tractionLossMaxAngle) { //If the wall surface the player is running
		//					print ("Wall steepness of " + steepnessAngle + " was too steep for speed " + this.GetSpeed () + ", stopping.");
		//					//FighterState.Vel = Vector2.zero;
		//					phys.leftWallBlocked = true;
		//					hInput = 0;
		//					phys.surfaceCling = false;
		//				}
		//			} 
		//			else if ((phys.rightWalled) && (hInput > 0)) 
		//			{
		//				print ("Trying to run up right wall slowly.");
		//				if (steepnessAngle >= m.tractionLossMaxAngle) { //If the wall surface the player is running
		//					print ("Wall steepness of " + steepnessAngle + " was too steep for speed " + this.GetSpeed () + ", stopping.");
		//					//FighterState.Vel = Vector2.zero;
		//					phys.rightWallBlocked = true;
		//					hInput = 0;
		//					phys.surfaceCling = false;
		//				}
		//			} 
		//			else 
		//			{
		//				print ("Not trying to move up a wall; Continue as normal.");
		//			}
		//		}


		//print("Wall Steepness Angle:"+steepnessAngle);

		///////////////////
		// Movement code //
		///////////////////

		if(phys.surfaceCling)
		{
			//print("SURFACECLING!");
			if(FighterState.Vel.y > 0)
			{
				FighterState.Vel = ChangeSpeedLinear(FighterState.Vel,-0.8f);
			}
			else if(FighterState.Vel.y <= 0)
			{
				if( (hInput<0 && phys.leftWalled) || (hInput>0 && phys.rightWalled) )
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
			if(FighterState.Vel.y>0) 	// If ascending...
			{		
				if(phys.leftWalled)
					v.facingDirection = false;
				if(phys.rightWalled)
					v.facingDirection = true;

				if(hInput>0) 				// ...and pressing key upward...
				{
					FighterState.Vel.y -= 0.8f; // ... then decelerate slower.
				}
				else if(hInput<0) 			// ...and pressing key downward...
				{
					FighterState.Vel.y -= 1.2f; // ...decelerate quickly.
					if(phys.leftWalled)
						v.facingDirection = true;
					if(phys.rightWalled)
						v.facingDirection = false;
				}
				else 						// ...and pressing nothing...
				{
					FighterState.Vel.y -= 1f; 	// ...decelerate.
				}
			}
			else if(FighterState.Vel.y<=0) // If descending...
			{
				if(phys.leftWalled)
					v.facingDirection = true;
				if(phys.rightWalled)
					v.facingDirection = false;

				if(hInput>0) 					// ...and pressing key upward...
				{
					FighterState.Vel.y -= 0.1f; 	// ...then wallslide.
					v.wallSliding = true;
				}
				else if(hInput<0) 				// ...and pressing key downward...
				{
					FighterState.Vel.y -= 1.2f; 	// ...accelerate downward quickly.
				}
				else 							// ...and pressing nothing...
				{
					FighterState.Vel.y -= 1f; 		// ...accelerate downward.
					v.wallSliding = true;
				}
			}
		}
	}


	protected bool ToLeftWall(RaycastHit2D leftCheck) 
	{ //Sets the new position of the fighter and their m_LeftNormal.

		if(d.sendCollisionMessages){print("We've hit LeftWall, sir!!");}

		if (phys.airborne)
		{
			if(d.sendCollisionMessages){print("Airborne before impact.");}
			phys.worldImpact = true;
		}

		Breakable hitBreakable = leftCheck.collider.transform.GetComponent<Breakable>();

		if(hitBreakable!=null&&FighterState.Vel.magnitude > 3)
		{
			if(d.sendCollisionMessages){print("hit a hitbreakable!");}
			if(hitBreakable.RecieveHit(this)){return false;}
		}
		//phys.leftSideContact = true;
		phys.leftWalled = true;

		if(!phys.grounded&&!phys.ceilinged)
		{
			v.primarySurface = 2;
			v.truePrimarySurface = 2;
		}

		Vector2 setCharPos = leftCheck.point;
		setCharPos.x += (m_LeftSideLength-m.minEmbed); //Embed slightly in wall to ensure raycasts still hit wall.
		//setCharPos.y -= m.minEmbed;
		//print("Sent to Pos:" + setCharPos);

		this.transform.position = setCharPos;

		//print ("Final Position:  " + this.transform.position);

		RaycastHit2D leftCheck2 = Physics2D.Raycast(this.transform.position, Vector2.left, m_LeftSideLength, m_TerrainMask);
		if (leftCheck2) 
		{
			m_LeftNormal = leftCheck2.normal;
		}
		else
		{
			m_LeftNormal = leftCheck.normal;
		}
			
		if(Mathf.Abs(m_LeftNormal.x)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
		{
			m_LeftNormal.x = 0;
		}

		if(Mathf.Abs(m_LeftNormal.y)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
		{
			m_LeftNormal.y = 0;
		}

//		if(phys.grounded)
//		{
//			if(d.sendCollisionMessages)
//			{
//				print("LeftGroundWedge detected during left collision.");
//			}
//			OmniWedge(0,2);
//		}
//
//		if(phys.ceilinged)
//		{
//			if(d.sendCollisionMessages)
//			{
//				print("LeftCeilingWedge detected during left collision.");
//			}
//			OmniWedge(2,1);
//		}
//
//		if(phys.rightWalled)
//		{
//			if(d.sendCollisionMessages)
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

		if(d.sendCollisionMessages){print("We've hit RightWall, sir!!");}
		//print ("groundCheck.normal=" + groundCheck.normal);
		//print("prerightwall Pos:" + this.transform.position);

		if (phys.airborne)
		{
			if(d.sendCollisionMessages){print("Airborne before impact.");}
			phys.worldImpact = true;
		}

		Breakable hitBreakable = rightCheck.collider.transform.GetComponent<Breakable>();

		if(hitBreakable!=null&&FighterState.Vel.magnitude > 3)
		{
			print("hit a hitbreakable!");
			if(hitBreakable.RecieveHit(this)){return false;}
		}
		phys.rightSideContact = true;
		phys.rightWalled = true;

		if(!phys.grounded && !phys.ceilinged)
		{
			v.primarySurface = 3;
			v.truePrimarySurface = 3;
		}

		Vector2 setCharPos = rightCheck.point;
		setCharPos.x -= (m_RightSideLength-m.minEmbed); //Embed slightly in wall to ensure raycasts still hit wall.

		//print("Sent to Pos:" + setCharPos);
		//print("Sent to normal:" + groundCheck.normal);

		this.transform.position = setCharPos;

		//print ("Final Position:  " + this.transform.position);

		RaycastHit2D rightCheck2 = Physics2D.Raycast(this.transform.position, Vector2.right, m_RightSideLength, m_TerrainMask);
		if (rightCheck2) 
		{
			m_RightNormal = rightCheck2.normal;
			//print("rightCheck2.normal="+rightCheck2.normal);
		}
		else
		{
			m_RightNormal = rightCheck.normal;
			//print("rightCheck1.normal="+rightCheck.normal);
		}

		if(Mathf.Abs(m_RightNormal.x)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
		{
			m_RightNormal.x = 0;
		}

		if(Mathf.Abs(m_RightNormal.y)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
		{
			m_RightNormal.y = 0;
		}


		//print ("Final Position2:  " + this.transform.position);

//		if(phys.grounded)
//		{
//			//print("RightGroundWedge detected during right collision.");
//			OmniWedge(0,3);
//		}
//
//		if(phys.leftWalled)
//		{
//			print("THERE'S PROBLEMS.");
//			//OmniWedge(2,3);
//		}
//
//		if(phys.ceilinged)
//		{
//			//print("RightCeilingWedge detected during right collision.");
//			OmniWedge(3,1);
//		}
		return true;
	}

	protected bool ToGround(RaycastHit2D groundCheck) 
	{ //Sets the new position of the fighter and their ground normal.
		//print ("phys.grounded=" + phys.grounded);

		if (phys.airborne)
		{
			phys.worldImpact = true;
		}

		Breakable hitBreakable = groundCheck.collider.transform.GetComponent<Breakable>();

		if(hitBreakable!=null&&this.GetSpeed() > 3)
		{
			print("hit a hitbreakable!");
			if(hitBreakable.RecieveHit(this)){return false;}
		}
			
		phys.grounded = true;
		v.primarySurface = 0;
		v.truePrimarySurface = 0;

		Vector2 setCharPos = groundCheck.point;
		setCharPos.y = setCharPos.y+m_GroundFootLength-m.minEmbed; //Embed slightly in ground to ensure raycasts still hit ground.
		this.transform.position = setCharPos;

		//print("Sent to Pos:" + setCharPos);
		//print("Sent to normal:" + groundCheck.normal);

		RaycastHit2D groundCheck2 = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength, m_TerrainMask);

		if(groundCheck.normal.y == 0f)
		{//If vertical surface
			//throw new Exception("Existence is suffering");
			if(d.sendCollisionMessages)
			{
				//print("GtG VERTICAL :O");
			}
		}

//		if(phys.ceilinged)
//		{
//			if(d.sendCollisionMessages)
//			{
//				print("CeilGroundWedge detected during ground collision.");
//			}
//			OmniWedge(0,1);
//		}
//
//		if(phys.leftWalled)
//		{
//			if(d.sendCollisionMessages)
//			{
//				print("LeftGroundWedge detected during ground collision.");
//			}
//			OmniWedge(0,2);
//		}
//
//		if(phys.rightWalled)
//		{
//			if(d.sendCollisionMessages)
//			{
//				print("RightGroundWedge detected during groundcollision.");
//			}
//			OmniWedge(0,3);
//		}

		if((GetSteepness(groundCheck2.normal)>=((m.tractionLossMaxAngle+m.tractionLossMinAngle)/2)) && this.GetSpeed()<=0.001f) 
		{ //If going slow and hitting a steep slope, don't move to the new surface, and treat the new surface as a wall on that side.
			if(this.GetVelocity().x>0)
			{
				print("Positive slope ground acting as right wall due to steepness.");
				phys.rightWallBlocked = true;
			}
			else
			{
				print("Negative slope ground acting as left wall due to steepness.");
				phys.leftWallBlocked = true;
			}
			return false;
		}
		else
		{
			m_GroundNormal = groundCheck2.normal;
			return true;
		}



		//print ("Final Position2:  " + this.transform.position);
	}

	protected bool ToCeiling(RaycastHit2D ceilingCheck) 
	{ //Sets the new position of the fighter when they hit the ceiling.
		//print ("We've hit ceiling, sir!!");
		//print ("ceilingCheck.normal=" + ceilingCheck.normal);

		if (phys.airborne)
		{
			phys.worldImpact = true;
		}


		Breakable hitBreakable = ceilingCheck.collider.transform.GetComponent<Breakable>();

		if(hitBreakable!=null&&FighterState.Vel.magnitude > 3)
		{
			print("hit a hitbreakable!");
			if(hitBreakable.RecieveHit(this)){return false;}
		}

		//m_Impact = true;
		phys.ceilinged = true;
		Vector2 setCharPos = ceilingCheck.point;
		setCharPos.y -= (m_GroundFootLength-m.minEmbed); //Embed slightly in ceiling to ensure raycasts still hit ceiling.
		this.transform.position = setCharPos;

		RaycastHit2D ceilingCheck2 = Physics2D.Raycast(this.transform.position, Vector2.up, m_GroundFootLength, m_TerrainMask);
		if (ceilingCheck2) 
		{
			//			if(d.antiTunneling){
			//				Vector2 surfacePosition = ceilingCheck2.point;
			//				surfacePosition.y -= (m_CeilingFootLength-m.minEmbed);
			//				this.transform.position = surfacePosition;
			//			}
		}
		else
		{
			if(d.sendCollisionMessages)
			{
				print("Ceilinged = false?");
			}
			phys.ceilinged = false;
		}

		if(ceilingCheck.normal.y == 0f)
		{//If vertical surface
			//throw new Exception("Existence is suffering");
			if(d.sendCollisionMessages)
			{
				print("CEILING VERTICAL :O");
			}
		}

		m_CeilingNormal = ceilingCheck2.normal;

//		if(phys.grounded)
//		{
//			if(d.sendCollisionMessages)
//			{
//				print("CeilGroundWedge detected during ceiling collision.");
//			}
//			OmniWedge(0,1);
//		}
//
//		if(phys.leftWalled)
//		{
//			if(d.sendCollisionMessages)
//			{
//				print("LeftCeilWedge detected during ceiling collision.");
//			}
//			OmniWedge(2,1);
//		}
//
//		if(phys.rightWalled)
//		{
//			if(d.sendCollisionMessages)
//			{
//				print("RightGroundWedge detected during ceiling collision.");
//			}
//			OmniWedge(3,1);
//		}
		//print ("Final Position2:  " + this.transform.position);
		return true;
	}

	protected bool FighterCollision(FighterChar fighterCollidedWith) //FC
	{ // Handles collisions with another Fighter.
		if(FighterState.Stance == 0) // If neutral, check if other fighter is attacking.
		{
			if(fighterCollidedWith.FighterState.Stance==0)
			{
				return false;
			}
			else if(fighterCollidedWith.FighterState.Stance==1)
			{
				fighterCollidedWith.FighterStruck(this);
				return true;
			}
			else
			{
				return false;
			}
		}
		else if(FighterState.Stance==1) // If attacking, see what stance enemy is, and decide type of impact.
		{
			if(fighterCollidedWith.FighterState.Stance==0)
			{
				FighterStruck(fighterCollidedWith);
			}
			else if(fighterCollidedWith.FighterState.Stance==1)
			{
				FighterClash(fighterCollidedWith);
			}
			else
			{
				FighterGuardStruck(fighterCollidedWith);
			}
			return true; //All paths in attack stance lead to a collision.
		}
		else
		{
			if(fighterCollidedWith.FighterState.Stance==0)
			{
				return false;
			}
			else if(fighterCollidedWith.FighterState.Stance==1)
			{
				fighterCollidedWith.FighterGuardStruck(this);
				return true;
			}
			else
			{
				return false;
			}
		}
	}

	protected void FighterStruck(FighterChar opponent) //Executes when a fighter in attack stance collides with an opponent in neutral stance.
	{
		// Calculating impact information.
		Vector2 myVelocity = this.GetVelocity();
		Vector2 yourVelocity = opponent.GetVelocity();

		float mySpeed = this.GetSpeed();
		float yourSpeed = opponent.GetSpeed();

		float combinedSpeed = mySpeed + yourSpeed;

		Vector2 lineFromMeToOpponent = opponent.GetPosition()-this.GetPosition();
		Vector2 lineFromOpponentToMe = -lineFromMeToOpponent;

		Vector2 myImpactVelocity = Proj(myVelocity, lineFromMeToOpponent);
		Vector2 yourImpactVelocity = Proj(yourVelocity, lineFromOpponentToMe);

		float myImpactForce = myImpactVelocity.magnitude;
		float yourImpactForce = yourImpactVelocity.magnitude;
		float combinedForce = myImpactForce+yourImpactForce;

		if(combinedForce<1f)
		{
			return; // Placeholder
		}
			
		float impactDamageM = (combinedForce/g_MaxClashDamageForce);
	
		//
		// Dealing damage
		//
		int myTotalDamageDealt = (int)(impactDamageM*g_MaxClashDamage);
		opponent.TakeDamage(myTotalDamageDealt);

		//
		//  Applying status effects
		//
		if(impactDamageM>opponent.g_MaxGuardStunT) 
		{
			opponent.g_CurStun = 3f;
			opponent.g_Stunned = true;	
		}
		else if(impactDamageM>opponent.g_MinNeutStunT)
		{
			//opponent.g_CurStun = 2*(myCombinedDmgM-g_ClashStaggerStunT)/(g_MaxClashStunT-g_ClashStaggerStunT);
			opponent.g_CurStun = 1f+2*(impactDamageM-g_MinNeutStunT)/(g_MaxGuardStunT-g_MinNeutStunT);
			opponent.g_Stunned = true;
		}
		else if(impactDamageM>opponent.g_MinGuardStaggerT)
		{
			opponent.g_CurStun = 1f;
			opponent.g_Staggered = true;
		}

		//
		// Visual/Audio effects
		//
		v.triggerAtkHit = true;
		o.fighterAudio.PunchHitSound();
		opponent.v.triggerFlinched = true;
		if((this.IsPlayer() || opponent.IsPlayer())&&(impactDamageM>v.punchStrengthSlowmoT))
		{
			o.timeManager.TimeDilation(0.1f, 0.75f+0.75f*impactDamageM);
		}
		if(combinedSpeed >= m.craterT)
		{
			//opponent.Crater(combinedSpeed);
			Crater(combinedSpeed);
		}
		else if(combinedSpeed >= m.velPunchT)
		{
			//opponent.Slam(combinedSpeed);
			Slam(combinedSpeed);
		}
		Vector2 inbetween = (opponent.GetPosition()+this.GetPosition())/2;
		SpawnDustEffect(inbetween);
		//
		// Setting new player velocities.
		//
		opponent.InstantForce(myVelocity, combinedSpeed*0.6f);
		this.InstantForce(yourVelocity, combinedSpeed*0.2f);

		//this.FighterState.Vel.y += 20*impactDamageM;
		//opponent.FighterState.Vel.y += 20*impactDamageM;

		//print("Opponent struck!\nOpponent got knocked in direction "+yourVelocity+"\nI got knocked in direction "+myVelocity);
		//print("Opponent took "+myTotalDamageDealt+" damage");

		// Placeholder. Adding a delay to prevent a double impact when the other player's physics executes.
		g_FighterCollisionCD = g_FighterCollisionCDLength;
		opponent.g_FighterCollisionCD = g_FighterCollisionCDLength;
	}

	protected void FighterGuardStruck(FighterChar opponent) // FGS - Executes when a fighter in attack stance collides with an opponent in guard stance.
	{
		// Calculating impact information.
		Vector2 myVelocity = this.GetVelocity();
		Vector2 yourVelocity = opponent.GetVelocity();

		float mySpeed = this.GetSpeed();
		float yourSpeed = opponent.GetSpeed();

		float combinedSpeed = mySpeed + yourSpeed;

		Vector2 lineFromMeToOpponent = opponent.GetPosition()-this.GetPosition();
		Vector2 lineFromOpponentToMe = -lineFromMeToOpponent;

		Vector2 myImpactVelocity = Proj(myVelocity, lineFromMeToOpponent);
		Vector2 yourImpactVelocity = Proj(yourVelocity, lineFromOpponentToMe);

		float myImpactForce = myImpactVelocity.magnitude;
		float yourImpactForce = yourImpactVelocity.magnitude;
		float combinedForce = myImpactForce+yourImpactForce;

		if(combinedForce<1f)
		{
			return; // Placeholder
		}

		float forceInequality = Math.Abs(myImpactForce-yourImpactForce);
		float inequalityM = forceInequality/g_MaxClashDisparity; // This number is larger the bigger the difference in velocities is. This is used to make equal-speed collisions less harmful for both parties.
		inequalityM = 0.25f + 0.75f*inequalityM; //Inequality multiplier scales from 25% to 100%.

		float impactDamageM = (combinedForce/g_MaxClashDamageForce);


		//
		// Dealing damage
		//
		float myDamageDealtM = inequalityM*(myImpactForce/combinedForce);
		//float yourDamageDealtM = inequalityM*(yourImpactForce/combinedForce);

		float myCombinedDmgM = myDamageDealtM*impactDamageM;
		//float yourCombinedDmgM = yourDamageDealtM*impactDamageM;

		int myTotalDamageDealt = (int)(myCombinedDmgM*g_MaxClashDamage);
		//int yourTotalDamageDealt = (int)(yourCombinedDmgM*g_MaxClashDamage);

		opponent.TakeDamage(myTotalDamageDealt);
		//this.TakeDamage(yourTotalDamageDealt);

		//
		//  Applying status effects
		//
		if(myCombinedDmgM>g_MaxNeutStunT) 
		{
			opponent.g_CurStun = 3f;
			opponent.g_Stunned = true;	
		}
		else if(myCombinedDmgM>g_MinNeutStunT)
		{
			opponent.g_CurStun = 1f+2*(myCombinedDmgM-g_MinNeutStunT)/(g_MaxNeutStunT-g_MinNeutStunT);
			opponent.g_Stunned = true;
		}
		else if(myCombinedDmgM>g_MinNeutStaggerT)
		{
			opponent.g_CurStun = 1f;
			opponent.g_Staggered = true;
		}

		//
		// Special effects
		//
		if((this.IsPlayer() || opponent.IsPlayer())&&(impactDamageM>v.punchStrengthSlowmoT))
		{
			o.timeManager.TimeDilation(0.1f, 0.75f+0.75f*impactDamageM);
		}
		if(combinedSpeed >= m.craterT)
		{
			print("Fighter crater successful");
			Crater(combinedSpeed);
		}
		else if(combinedSpeed >= m.slamT)
		{
			print("Fighter slam successful");
			Slam(combinedSpeed);
		}
		v.triggerAtkHit = true;
		o.fighterAudio.PunchHitSound();

		//
		// Setting new player velocities.
		//
		opponent.InstantForce(myVelocity, combinedSpeed*0.2f);
		this.InstantForce(yourVelocity, combinedSpeed*0.1f);

		print("Guard struck!\nOpponent got knocked in direction "+yourVelocity.normalized+"\nI got knocked in direction "+myVelocity.normalized);
		print("Opponent took "+myTotalDamageDealt+" damage");
		// Placeholder. Adding a delay to prevent a double impact when the other player's physics executes.
		g_FighterCollisionCD = g_FighterCollisionCDLength;
		opponent.g_FighterCollisionCD = g_FighterCollisionCDLength;
	}

	protected void FighterClash(FighterChar opponent) // FC - Executes when two fighters collide in attack stance, clashing weapons.
	{
		// Calculating impact information.
		Vector2 myVelocity = this.GetVelocity();
		Vector2 yourVelocity = opponent.GetVelocity();

		float mySpeed = this.GetSpeed();
		float yourSpeed = opponent.GetSpeed();

		float combinedSpeed = mySpeed + yourSpeed;

		Vector2 lineFromMeToOpponent = opponent.GetPosition()-this.GetPosition();
		Vector2 lineFromOpponentToMe = -lineFromMeToOpponent;

		Vector2 myImpactVelocity = Proj(myVelocity, lineFromMeToOpponent);
		Vector2 yourImpactVelocity = Proj(yourVelocity, lineFromOpponentToMe);

		float myImpactForce = myImpactVelocity.magnitude;
		float yourImpactForce = yourImpactVelocity.magnitude;
		float combinedForce = myImpactForce+yourImpactForce;

		if(combinedForce<1f)
		{
			return; // Placeholder
		}

		float forceInequality = Math.Abs(myImpactForce-yourImpactForce);
		float inequalityM = forceInequality/g_MaxClashDisparity; // This number is larger the bigger the difference in velocities is. This is used to make equal-speed collisions less harmful for both parties.
		inequalityM = 0.25f + 0.75f*inequalityM; //Inequality multiplier scales from 25% to 100%.

		float impactDamageM = (combinedForce/g_MaxClashDamageForce);


		//
		// Dealing damage
		//
		float myDamageDealtM = inequalityM*(myImpactForce/combinedForce);
		float yourDamageDealtM = inequalityM*(yourImpactForce/combinedForce);

		float myCombinedDmgM = myDamageDealtM*impactDamageM;
		float yourCombinedDmgM = yourDamageDealtM*impactDamageM;

		int myTotalDamageDealt = (int)(myCombinedDmgM*g_MaxClashDamage);
		int yourTotalDamageDealt = (int)(yourCombinedDmgM*g_MaxClashDamage);

		opponent.TakeDamage(myTotalDamageDealt);
		this.TakeDamage(yourTotalDamageDealt);

		//
		//  Applying status effects
		//
		if(myCombinedDmgM>g_MaxAttackStunT) 
		{
			opponent.g_CurStun = 3f;
			opponent.g_Stunned = true;	
		}
		else if(myCombinedDmgM>g_MinAttackStunT)
		{
			//opponent.g_CurStun = 2*(myCombinedDmgM-g_ClashStaggerStunT)/(g_MaxClashStunT-g_ClashStaggerStunT);
			opponent.g_CurStun = 1f+2*(myCombinedDmgM-g_MinAttackStunT)/(g_MaxAttackStunT-g_MinAttackStunT);
			opponent.g_Stunned = true;
		}
		else if(myCombinedDmgM>g_MinAttackStaggerT)
		{
			opponent.g_CurStun = 1f;
			opponent.g_Staggered = true;
		}

		if(yourCombinedDmgM>g_MaxAttackStunT)
		{
			this.g_CurStun = 3f;
			g_Stunned = true;	
		}
		else if(yourCombinedDmgM>g_MinAttackStunT)
		{
			this.g_CurStun = 1f+2*(yourCombinedDmgM-g_MinAttackStunT)/(g_MaxAttackStunT-g_MinAttackStunT);
			g_Stunned = true;
		}
		else if(yourCombinedDmgM>g_MinAttackStaggerT)
		{
			this.g_CurStun = 1f;
			g_Staggered = true;
		}

		//
		// Special effects
		//
		opponent.v.triggerAtkHit = true;
		opponent.o.fighterAudio.PunchHitSound();

		v.triggerAtkHit = true;
		o.fighterAudio.PunchHitSound();


		if((this.IsPlayer() || opponent.IsPlayer())&&(impactDamageM>v.punchStrengthSlowmoT))
		{
			o.timeManager.TimeDilation(0.1f, 0.75f+0.75f*impactDamageM);
		}

		if(combinedSpeed >= m.craterT)
		{
			//print("Fighter crater successful");
			opponent.Crater(combinedSpeed);
			Crater(combinedSpeed);
		}
		else if(combinedSpeed >= m.slamT)
		{
			//print("Fighter slam successful");
			opponent.Slam(combinedSpeed);
			Slam(combinedSpeed);
		}
		//
		// Setting new player velocities.
		//
		float repulsion;
		if(combinedSpeed*0.5f<15)
		{
			repulsion = 15;
		}
		else
		{
			repulsion = combinedSpeed*0.5f;
		}
		opponent.InstantForce(myVelocity, repulsion);
		this.InstantForce(yourVelocity, repulsion);

		//print("Fighters Clashed!\nOpponent got knocked in direction "+yourVelocity.normalized+"\nI got knocked in direction "+myVelocity.normalized);
		//print("Opponent took "+myTotalDamageDealt+" damage, and I took "+yourTotalDamageDealt+" damage.");

		// Placeholder. Adding a delay to prevent a double impact when the other player's physics executes.
		g_FighterCollisionCD = g_FighterCollisionCDLength;
		opponent.g_FighterCollisionCD = g_FighterCollisionCDLength;
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
		m.expiredNormal = new Vector2(0,0); //Used for wallslides. This resets the surface normal that wallcling is set to ignore.

		Vector2 initialDirection = FighterState.Vel.normalized;
		Vector2 newPara = Perp(newNormal);
		Vector2 AdjustedVel;

		float initialSpeed = FighterState.Vel.magnitude;
		float testNumber = newPara.y/newPara.x;

		if(float.IsNaN(testNumber))
		{
			if(d.sendCollisionMessages){print("NaN value found on DirectionChange");}
		}

		if((initialDirection == newPara)||initialDirection == Vector2.zero)
		{
			//print("same angle");
			return;
		}

		float impactAngle = Vector2.Angle(initialDirection,newPara);
		//print("TrueimpactAngle: " +impactAngle);
		//print("InitialDirection: "+initialDirection);
		//print("GroundDirection: "+newPara);

		impactAngle = (float)Math.Round(impactAngle,2);

		if(impactAngle >= 180)
		{
			impactAngle = 180f - impactAngle;
		}

		if(impactAngle > 90)
		{
			impactAngle = 180f - impactAngle;
		}

		AdjustedVel = Proj(FighterState.Vel, newPara);

		FighterState.Vel = AdjustedVel;

		//Speed loss from impact angle handling beyond this point. The player loses speed based on projection angle, but that is purely mathematical. The proceeding code is intended to simulate ground traction.

		float speedRetentionMult = 1; // The % of speed retained, based on sharpness of impact angle. A direct impact = full stop.

		if(impactAngle <= m.impactDecelMinAngle)
		{ // Angle lower than min, no speed penalty.
			speedRetentionMult = 1;
		}
		else if(impactAngle < m.impactDecelMaxAngle)
		{ // In the midrange, administering momentum loss on a curve leading from min to max.
			speedRetentionMult = 1-Mathf.Pow((impactAngle-m.impactDecelMinAngle)/(m.impactDecelMaxAngle-m.impactDecelMinAngle),2); // See Workflowy notes section for details on this formula.
		}
		else
		{ // Angle beyond max, momentum halted. 
			speedRetentionMult = 0;
			phys.worldImpact = true;
		}

		if(initialSpeed <= 2f)
		{ // If the fighter is near stationary, do not remove any velocity because there is no impact!
			speedRetentionMult = 1;
		}
			
		//print("SPLMLT " + speedRetentionMult);

		SetSpeed(FighterState.Vel, initialSpeed*speedRetentionMult);
		//print("Final Vel " + FighterState.Vel);
		//print ("DirChange Vel:  " + FighterState.Vel);
	}

	protected void OmniWedge(int lowerContact, int upperContact)
	{//Executes when the fighter is moving into a corner and there isn't enough room to fit them. It halts the fighter's momentum and sets off a blocked-direction flag.

		if(d.sendCollisionMessages){print("OmniWedge("+lowerContact+","+upperContact+")");}

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
				if(d.sendCollisionMessages){print("Omniwedge: lowercontact is left");}
				lowerDirection = Vector2.left;
				lowerLength = m_LeftSideLength;
				break;
			}
		case 3: //lowercontact is right
			{
				if(d.sendCollisionMessages){print("Omniwedge: lowercontact is right");}
				lowerDirection = Vector2.right;
				lowerLength = m_RightSideLength;
				break;
			}
		default:
			{
				throw new Exception("ERROR: DEFAULTED ON LOWERHIT.");
			}
		}

		lowerHit = Physics2D.Raycast(this.transform.position, lowerDirection, lowerLength, m_TerrainMask);

		float embedDepth;
		Vector2 gPara; //lowerpara, aka groundparallel
		Vector2 cPara; //upperpara, aka ceilingparallel
		Vector2 correctionVector = new Vector2(0,0);

		if(!lowerHit)
		{
			//throw new Exception("Bottom not wedged!");
			if(d.sendCollisionMessages){print("Bottom not wedged!");}
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
				groundPosition.y += (m_GroundFootLength-m.minEmbed);
			}
			else if(lowerContact == 1) //ceiling contact
			{
				throw new Exception("CEILINGCOLLIDER CAN'T BE LOWER CONTACT");
			}
			else if(lowerContact == 2) //left contact
			{
				groundPosition.x += (m_LeftSideLength-m.minEmbed);
			}
			else if(lowerContact == 3) //right contact
			{
				groundPosition.x -= (m_RightSideLength-m.minEmbed);
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
				if(d.sendCollisionMessages){print("Omniwedge: uppercontact is left");}
				upperDirection = Vector2.left;
				upperLength = m_LeftSideLength;
				break;
			}
		case 3: //uppercontact is right
			{
				if(d.sendCollisionMessages){print("Omniwedge: uppercontact is right");}
				upperDirection = Vector2.right;
				upperLength = m_RightSideLength;
				break;
			}
		default:
			{
				throw new Exception("ERROR: DEFAULTED ON UPPERHIT.");
			}
		}

		upperHit = Physics2D.Raycast(this.transform.position, upperDirection, upperLength, m_TerrainMask);
		embedDepth = upperLength-upperHit.distance;

		if(!upperHit)
		{
			//throw new Exception("Top not wedged!");
			cPara = Perp(upperHit.normal);
			if(d.sendCollisionMessages)
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

		//print("Ground Para = " + gPara);
		//print("Ceiling Para = " + cPara);
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

		if(cornerAngle > 90f)
		{
			if(convergenceValue > 0)
			{
				if(d.sendCollisionMessages){print("Left wedge!");}
				correctionVector = SuperUnwedger(cPara, gPara, true, embedDepth);
				if(d.sendCollisionMessages){print("correctionVector:"+correctionVector);}
				phys.leftWallBlocked = true;
			}
			else if(convergenceValue < 0)
			{
				//print("Right wedge!");
				correctionVector = SuperUnwedger(cPara, gPara, false, embedDepth);
				phys.rightWallBlocked = true;
			}
			else
			{
				throw new Exception("CONVERGENCE VALUE OF ZERO ON CORNER!");
			}
			FighterState.Vel = new Vector2(0f, 0f);
		}
		else
		{
			if(d.sendCollisionMessages){print("Obtuse wedge angle detected!");}
			correctionVector = (upperDirection*(-(embedDepth-m.minEmbed)));
		}

		this.transform.position = new Vector2((this.transform.position.x + correctionVector.x), (this.transform.position.y + correctionVector.y));
	}

	protected Vector2 Perp(Vector2 input) //Perpendicularizes the vector.
	{
		Vector2 output;
		output.x = input.y;
		output.y = -input.x;
		return output;
	}

	protected Vector2 Proj(Vector2 A, Vector2 B) //Projects vector A onto vector B.
	{
		float component = Vector2.Dot(A,B)/B.magnitude;
		return component*B.normalized;
	}		

	protected void UpdateContactNormals(bool posCorrection) // UCN - Updates the present-time state of the player's contact with surrounding world geometry. Corrects the player's position if it is embedded in geometry, and gathers information about where the player can move.
	{
		phys.grounded = false;
		phys.ceilinged = false;
		phys.leftWalled = false;
		phys.rightWalled = false;
		phys.airborne = false;

		if(m.jumpBufferG>0){m.jumpBufferG--;}
		if(m.jumpBufferC>0){m.jumpBufferC--;}
		if(m.jumpBufferL>0){m.jumpBufferL--;}
		if(m.jumpBufferR>0){m.jumpBufferR--;}

		phys.groundContact = false;
		phys.ceilingContact = false;
		phys.leftSideContact = false;
		phys.rightSideContact = false;
		phys.closeToGround = false;

		d.groundLine.endColor = Color.red;
		d.groundLine.startColor = Color.red;
		d.ceilingLine.endColor = Color.red;
		d.ceilingLine.startColor = Color.red;
		d.leftSideLine.endColor = Color.red;
		d.leftSideLine.startColor = Color.red;
		d.rightSideLine.endColor = Color.red;
		d.rightSideLine.startColor = Color.red;

		directionContacts[0] = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength, m_TerrainMask); 	// Ground
		directionContacts[1] = Physics2D.Raycast(this.transform.position, Vector2.up, m_CeilingFootLength, m_TerrainMask);  	// Ceiling
		directionContacts[2] = Physics2D.Raycast(this.transform.position, Vector2.left, m_LeftSideLength, m_TerrainMask); 	// Left
		directionContacts[3] = Physics2D.Raycast(this.transform.position, Vector2.right, m_RightSideLength, m_TerrainMask); // Right  

		closeToGroundContact = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength+m.airborneCutoffLength, m_TerrainMask);

		if (directionContacts[0]) 
		{
			m_GroundNormal = directionContacts[0].normal;
			phys.groundContact = true;
			d.groundLine.endColor = Color.green;
			d.groundLine.startColor = Color.green;
			phys.grounded = true;
			m.jumpBufferG = m.jumpBufferFrameAmount;
			if(Mathf.Abs(m_GroundNormal.x)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
				m_GroundNormal.x = 0;
			if(Mathf.Abs(m_GroundNormal.y)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
				m_GroundNormal.y = 0;
		} 

		if (directionContacts[1]) 
		{
			m_CeilingNormal = directionContacts[1].normal;
			phys.ceilingContact = true;
			d.ceilingLine.endColor = Color.green;
			d.ceilingLine.startColor = Color.green;
			phys.ceilinged = true;
			m.jumpBufferC = m.jumpBufferFrameAmount;
			if(Mathf.Abs(m_CeilingNormal.x)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
				m_CeilingNormal.x = 0;
			if(Mathf.Abs(m_CeilingNormal.y)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
				m_CeilingNormal.y = 0;
		} 


		if (directionContacts[2])
		{
			m_LeftNormal = directionContacts[2].normal;
			phys.leftSideContact = true;
			d.leftSideLine.endColor = Color.green;
			d.leftSideLine.startColor = Color.green;
			phys.leftWalled = true;
			m.jumpBufferL = m.jumpBufferFrameAmount;
			if(Mathf.Abs(m_LeftNormal.x)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
				m_LeftNormal.x = 0;
			if(Mathf.Abs(m_LeftNormal.y)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
				m_LeftNormal.y = 0;

		} 

		if (directionContacts[3])
		{
			m_RightNormal = directionContacts[3].normal;
			phys.rightSideContact = true;
			d.rightSideLine.endColor = Color.green;
			d.rightSideLine.startColor = Color.green;
			phys.rightWalled = true;
			m.jumpBufferR = m.jumpBufferFrameAmount;
			if(Mathf.Abs(m_RightNormal.x)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
				m_RightNormal.x = 0;
			if(Mathf.Abs(m_RightNormal.y)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
				m_RightNormal.y = 0;
		}

		if (closeToGroundContact)
		{
			phys.closeToGround = true;
		}
		

		if (!(phys.grounded&&phys.ceilinged)) //Resets wall blocker flags if the player isn't touching a blocking surface.
		{
			if(!phys.rightWalled)
			{
				phys.rightWallBlocked = false;
			}
			if(!phys.leftWalled)
			{
				phys.leftWallBlocked = false;
			}
		}

		if(d.antiTunneling&&posCorrection)
		{
			AntiTunneler(directionContacts);
		}
		if(!(phys.grounded||phys.ceilinged||phys.leftWalled||phys.rightWalled))
		{
			phys.airborne = true;
			phys.surfaceCling = false;
		}
	}

	protected void DebugUCN() //Like normal UCN but doesn't affect anything and prints results.
	{
		print("DEBUG_UCN");
		d.groundLine.endColor = Color.red;
		d.groundLine.startColor = Color.red;
		d.ceilingLine.endColor = Color.red;
		d.ceilingLine.startColor = Color.red;
		d.leftSideLine.endColor = Color.red;
		d.leftSideLine.startColor = Color.red;
		d.rightSideLine.endColor = Color.red;
		d.rightSideLine.startColor = Color.red;

		directionContacts[0] = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength, m_TerrainMask); 	// Ground
		directionContacts[1] = Physics2D.Raycast(this.transform.position, Vector2.up, m_CeilingFootLength, m_TerrainMask);  	// Ceiling
		directionContacts[2] = Physics2D.Raycast(this.transform.position, Vector2.left, m_LeftSideLength, m_TerrainMask); 	// Left
		directionContacts[3] = Physics2D.Raycast(this.transform.position, Vector2.right, m_RightSideLength, m_TerrainMask); // Right  

		closeToGroundContact = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength+m.airborneCutoffLength, m_TerrainMask);


		if (directionContacts[0]) 
		{
			d.groundLine.endColor = Color.green;
			d.groundLine.startColor = Color.green;
		} 

		if (directionContacts[1]) 
		{
			d.ceilingLine.endColor = Color.green;
			d.ceilingLine.startColor = Color.green;
		} 


		if (directionContacts[2])
		{
			d.leftSideLine.endColor = Color.green;
			d.leftSideLine.startColor = Color.green;
		} 

		if (directionContacts[3])
		{
			d.rightSideLine.endColor = Color.green;
			d.rightSideLine.startColor = Color.green;
		}


		if (closeToGroundContact)
		{

		}


		int contactCount = 0;
		if(phys.groundContact){contactCount++;}
		if(phys.ceilingContact){contactCount++;}
		if(phys.leftSideContact){contactCount++;}
		if(phys.rightSideContact){contactCount++;}

		int embedCount = 0;
		if(d.sendCollisionMessages&&phys.groundContact && ((m_GroundFootLength-directionContacts[0].distance)>=0.011f))	{ print("Embedded in grnd by amount: "+((m_GroundFootLength-directionContacts[0].distance)-m.minEmbed)); embedCount++;} //If embedded too deep in this surface.
		if(d.sendCollisionMessages&&phys.ceilingContact && ((m_CeilingFootLength-directionContacts[1].distance)>=0.011f))	{ print("Embedded in ceil by amount: "+((m_CeilingFootLength-directionContacts[1].distance)-m.minEmbed)); embedCount++;} //If embedded too deep in this surface.
		if(d.sendCollisionMessages&&phys.leftSideContact && ((m_LeftSideLength-directionContacts[2].distance)>=0.011f))	{ print("Embedded in left by amount: "+((m_LeftSideLength-directionContacts[2].distance)-m.minEmbed)); embedCount++;} //If embedded too deep in this surface.
		if(d.sendCollisionMessages&&phys.rightSideContact && ((m_RightSideLength-directionContacts[3].distance)>=0.011f))	{ print("Embedded in rigt by amount: "+((m_RightSideLength-directionContacts[3].distance)-m.minEmbed)); embedCount++;} //If embedded too deep in this surface.

		if(d.sendCollisionMessages){print(contactCount+" sides touching, "+embedCount+" sides embedded");}
	}


	protected void AntiTunneler(RaycastHit2D[] contacts)
	{
		bool[] isEmbedded = {false, false, false, false};
		int contactCount = 0;
		if(phys.groundContact){contactCount++;}
		if(phys.ceilingContact){contactCount++;}
		if(phys.leftSideContact){contactCount++;}
		if(phys.rightSideContact){contactCount++;}

		int embedCount = 0;
		if(phys.groundContact && ((m_GroundFootLength-contacts[0].distance)>=0.011f))	{isEmbedded[0]=true; embedCount++;} //If embedded too deep in this surface.
		if(phys.ceilingContact && ((m_CeilingFootLength-contacts[1].distance)>=0.011f))	{isEmbedded[1]=true; embedCount++;} //If embedded too deep in this surface.
		if(phys.leftSideContact && ((m_LeftSideLength-contacts[2].distance)>=0.011f))	{isEmbedded[2]=true; embedCount++;} //If embedded too deep in this surface.
		if(phys.rightSideContact && ((m_RightSideLength-contacts[3].distance)>=0.011f))	{isEmbedded[3]=true; embedCount++;} //If embedded too deep in this surface.

		switch(contactCount)
		{
		case 0: //No embedded contacts. Save this position as the most recent valid one and move on.
			{
				//print("No embedding! :)");
				phys.lastSafePosition = this.transform.position;
				break;
			}
		case 1: //One side is embedded. Simply push out to remove it.
			{
				//print("One side embed!");
				if(isEmbedded[0])
				{
					Vector2 surfacePosition = contacts[0].point;
					surfacePosition.y += (m_GroundFootLength-m.minEmbed);
					this.transform.position = surfacePosition;
				}
				else if(isEmbedded[1])
				{
					Vector2 surfacePosition = contacts[1].point;
					surfacePosition.y -= (m_CeilingFootLength-m.minEmbed);
					this.transform.position = surfacePosition;
				}
				else if(isEmbedded[2])
				{
					Vector2 surfacePosition = contacts[2].point;
					surfacePosition.x += ((m_LeftSideLength)-m.minEmbed);
					this.transform.position = surfacePosition;
				}
				else if(isEmbedded[3])
				{
					Vector2 surfacePosition = contacts[3].point;
					surfacePosition.x -= ((m_RightSideLength)-m.minEmbed);
					this.transform.position = surfacePosition;
				}
				else
				{
					phys.lastSafePosition = this.transform.position;
				}
				break;
			}
		case 2: //Two sides are touching. Use the 2-point unwedging algorithm to resolve.
			{
				if(phys.groundContact&&phys.ceilingContact)
				{
					//if(m_GroundNormal != m_CeilingNormal)
					{
						if(d.sendCollisionMessages)
						{
							print("Antitunneling omniwedge executed");		
						}
						OmniWedge(0,1);
					}
				}
				else if(phys.groundContact&&phys.leftSideContact)
				{
					if(m_GroundNormal != m_LeftNormal)
					{
						OmniWedge(0,2);
					}
					else
					{
						//print("Same surface, 1-point unwedging.");
						Vector2 surfacePosition = contacts[0].point;
						surfacePosition.y += (m_GroundFootLength-m.minEmbed);
						this.transform.position = surfacePosition;
					}
				}
				else if(phys.groundContact&&phys.rightSideContact)
				{
					if(m_GroundNormal != m_RightNormal)
					{
						OmniWedge(0,3);
					}
					else
					{
						//print("Same surface, 1-point unwedging.");
						Vector2 surfacePosition = contacts[0].point;
						surfacePosition.y += (m_GroundFootLength-m.minEmbed);
						this.transform.position = surfacePosition;
					}
				}
				else if(phys.ceilingContact&&phys.leftSideContact)
				{
					//if(m_CeilingNormal != m_LeftNormal)
					{
						OmniWedge(2,1);
					}
				}
				else if(phys.ceilingContact&&phys.rightSideContact)
				{
					//if(m_CeilingNormal != m_RightNormal)
					{
						OmniWedge(3,1);
					}
				}
				else if(phys.leftSideContact&&phys.rightSideContact)
				{
					throw new Exception("Unhandled horizontal wedge detected.");
					//OmniWedge(3,2);
				}
				break;
			}
		case 3: //Three sides are embedded. Not sure how to handle this yet besides reverting.
			{
				if(d.sendCollisionMessages)
				{
					print("Triple Embed.");
				}
				break;
			}
		case 4:
			{
				if(d.sendCollisionMessages)
				{
					print("FULL embedding!");
				}
				if(d.recoverFromFullEmbed)
				{
					this.transform.position = phys.lastSafePosition;
				}
				break;
			}
		default:
			{
				if(d.sendCollisionMessages)
				{
					print("ERROR: DEFAULTED ON ANTITUNNELER.");
				}
				break;
			}
		}

	}



	protected Vector2 SuperUnwedger(Vector2 cPara, Vector2 gPara, bool cornerIsLeft, float embedDistance)
	{
		if(d.sendCollisionMessages)
		{
			print("Ground Para = ("+gPara.x+", "+gPara.y+")");
			print("Ceiling Para = ("+cPara.x+", "+cPara.y+")");
		}
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
			if(d.sendCollisionMessages){print("Resolving left wedge.");}

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
					if(d.sendCollisionMessages){print("It's a wall, bro");}
					//return new Vector2(0, -embedDistance);
					return new Vector2(embedDistance-m.minEmbed,0);
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

		if(d.sendCollisionMessages)
		{
			print("Adapted Ground Para = "+gPara);
			print("Adapted Ceiling Para = "+cPara);
		}
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

		if(d.sendCollisionMessages){print("(A, B)=("+A+", "+B+").");}

		if(B <= 0)
		{
			if(d.sendCollisionMessages){print("B <= 0, using normal eqn.");}
			DivX = B-A;
			DivY = -(DivX/B);
		}
		else
		{
			if(d.sendCollisionMessages){print("B >= 0, using alternate eqn.");}
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
			return new Vector2 (0, 0);
		}
		if(d.sendCollisionMessages){print("SuperUnwedger push of: ("+X+","+Y+")");}
		return new Vector2(X,Y); // Returns the distance the object must move to resolve wedging.
	}

	protected void Jump(float horizontalInput)
	{
		Vector2 preJumpVelocity = FighterState.Vel;
		float jumpVelBonusM = 1;

		if(m.critJumpReady)
		{
			FlashEffect(0.2f, Color.yellow);
			o.fighterAudio.CritJumpSound();
			jumpVelBonusM = m.critJumpBonusM;
		}

		if(phys.grounded&&phys.ceilinged)
		{
			if(d.sendCollisionMessages)
			{print("Grounded and Ceilinged, nowhere to jump!");}
			//FighterState.JumpKey = false;
		}
		else if(m.jumpBufferG>0)
		{
			//phys.leftWallBlocked = false;
			//phys.rightWallBlocked = false;

			n_Jumped = true;
			if(n_AutoGenerateNavCon&&!n_PlayerTraversing) 
			{
				StartPlayerTraverse(); // Generating a jump-type nav connection between the start and end of this jump
			}

			if(FighterState.Vel.y >= 0) // If falling, jump will nullify downward momentum.
			{
				FighterState.Vel = new Vector2(FighterState.Vel.x+(m.hJumpForce*horizontalInput*jumpVelBonusM), FighterState.Vel.y+(m.vJumpForce*jumpVelBonusM));
			}
			else
			{
				FighterState.Vel = new Vector2(FighterState.Vel.x+(m.hJumpForce*horizontalInput*jumpVelBonusM), (m.vJumpForce*jumpVelBonusM));
			}
			o.fighterAudio.JumpSound();
			phys.grounded = false; // Watch this.
			v.primarySurface = -1;
			m.airborneDelayTimer = -1;
			v.truePrimarySurface = -1;
		}
		else if(m.jumpBufferL>0)
		{
			if(d.sendCollisionMessages)
			{
				print("Leftwalljumping!");
			}
			if(FighterState.Vel.y < 0)
			{
				FighterState.Vel = new Vector2( (m.wallHJumpForce*jumpVelBonusM), (m.wallVJumpForce*jumpVelBonusM) );
			}
			else if(FighterState.Vel.y <= (2*m.wallVJumpForce)) // If not ascending too fast, add vertical jump power to jump.
			{
				FighterState.Vel = new Vector2( (m.wallHJumpForce*jumpVelBonusM), FighterState.Vel.y+(m.wallVJumpForce*jumpVelBonusM) );
			}
			else // If ascending too fast, add no more vertical speed and just add horizontal.
			{
				FighterState.Vel = new Vector2( (m.wallHJumpForce*jumpVelBonusM), FighterState.Vel.y);
			}
			o.fighterAudio.JumpSound();
			//FighterState.JumpKey = false;
			phys.leftWalled = false;
			n_Jumped = true;
			if(n_AutoGenerateNavCon&&!n_PlayerTraversing)
			{
				StartPlayerTraverse();
			}
			v.primarySurface = -1;
			m.airborneDelayTimer = -1;
			v.truePrimarySurface = -1;
		}
		else if(m.jumpBufferR>0)
		{
			if(d.sendCollisionMessages)
			{
				print("Rightwalljumping!");
			}
			if(FighterState.Vel.y < 0)
			{
				FighterState.Vel = new Vector2( (-m.wallHJumpForce*jumpVelBonusM), (m.wallVJumpForce*jumpVelBonusM) );
			}
			else if(FighterState.Vel.y <= m.wallVJumpForce)
			{
				FighterState.Vel = new Vector2( (-m.wallHJumpForce*jumpVelBonusM), FighterState.Vel.y+(m.wallVJumpForce*jumpVelBonusM) );
			}
			else
			{
				FighterState.Vel = new Vector2( (-m.wallHJumpForce*jumpVelBonusM), FighterState.Vel.y );
			}

			o.fighterAudio.JumpSound();
			//FighterState.JumpKey = false;
			phys.rightWalled = false;
			n_Jumped = true;
			if(n_AutoGenerateNavCon&&!n_PlayerTraversing)
			{
				StartPlayerTraverse();
			}
			v.primarySurface = -1;
			m.airborneDelayTimer = -1;
			v.truePrimarySurface = -1;
		}
		else if(m.jumpBufferC>0)
		{
			if(FighterState.Vel.y <= 0)
			{
				FighterState.Vel = new Vector2(FighterState.Vel.x+(m.hJumpForce*horizontalInput*jumpVelBonusM), FighterState.Vel.y-(m.vJumpForce*jumpVelBonusM));
			}
			else
			{
				FighterState.Vel = new Vector2(FighterState.Vel.x+(m.hJumpForce*horizontalInput*jumpVelBonusM), -(m.vJumpForce*jumpVelBonusM));
			}
			o.fighterAudio.JumpSound();
			//FighterState.JumpKey = false;
			n_Jumped = true;
			if(n_AutoGenerateNavCon&&!n_PlayerTraversing)
			{
				StartPlayerTraverse();
			}
			phys.ceilinged = false;
			v.primarySurface = -1;
			m.airborneDelayTimer = -1;
			v.truePrimarySurface = -1;
		}
		else
		{
			//print("Can't jump, airborne!");
		}
			
		m.jumpBufferG = 0;
		m.jumpBufferC = 0;
		m.jumpBufferL = 0;
		m.jumpBufferR = 0;
	}

	protected void EtherJump(Vector2 jumpNormal)
	{
		if(FighterState.EtherLevel > 0)
		{
			FighterState.EtherLevel--;
		}
		FighterState.Vel = FighterState.Vel+(jumpNormal*(m.etherJumpForceBase+(m.etherJumpForcePerCharge*FighterState.EtherLevel)));	

		o.fighterAudio.JumpSound();
		v.primarySurface = -1;
		v.truePrimarySurface = -1;
		m.airborneDelayTimer = -1;

		phys.ceilinged = false;
		phys.grounded = false;
		phys.leftWalled = false;
		phys.rightWalled = false;

		m.jumpBufferG = 0;
		m.jumpBufferC = 0;
		m.jumpBufferL = 0;
		m.jumpBufferR = 0;

	}

	protected void StrandJumpTypeA(float horizontalInput, float verticalInput) //SJTA
	{
		float numberOfInputs = Math.Abs(horizontalInput)+Math.Abs(verticalInput);
		if(FighterState.Vel.magnitude>=20&&FighterState.EtherLevel > 0&&numberOfInputs > 0)
		{
			//print("STRANDJUMP!");
			Vector2 oldDirection = FighterState.Vel.normalized;
			//print("olddir:("+oldDirection.x+","+oldDirection.y+")");
			Vector2 newDirection = new Vector2(horizontalInput, verticalInput);
			//print("newdir:("+newDirection.x+","+newDirection.y+")");

			float bounceAngle = Vector2.Angle(oldDirection,newDirection);
			//print("TrueimpactAngle: " +impactAngle);
			//print("InitialDirection: "+initialDirection);
			//print("GroundDirection: "+newPara);


			bounceAngle = (float)Math.Round(bounceAngle,2);

			if(bounceAngle > 180)
			{
				bounceAngle = 180f - bounceAngle;
			}
				
			if(bounceAngle<m.widestStrandJumpAngle)
			{
				//print("Not Steep enough! Angle: "+bounceAngle);
				return;
			}
			else
			{
				//print("Angle: "+bounceAngle);
			}



		//	print("sum:("+(newDirection.x+oldDirection.x)+","+(newDirection.y+oldDirection.y)+")");
			Vector2 reflectionVector;
			if((oldDirection+newDirection)==Vector2.zero)
			{
				//print("equal!");
				reflectionVector = Perp(oldDirection);
			}
			else
			{
				reflectionVector = Perp((oldDirection+newDirection)/2); // Reflection surface is perpendicular to the vector perfectly between the impact angle and the reflected angle, which is found by averaging the two vectors.
			}

			Quaternion ImpactAngle = Quaternion.LookRotation(reflectionVector);
			ImpactAngle.x = 0;
			ImpactAngle.y = 0;


			if(reflectionVector.y == 0) //Duct tape fix
			{
				if(reflectionVector.x < 0)
				{
					ImpactAngle.eulerAngles = new Vector3(0, 0, -90);
				}
				else if(reflectionVector.x > 0)
				{
					ImpactAngle.eulerAngles = new Vector3(0, 0, 90);
				}
				else
				{
					//print("ERROR: IMPACT DIRECTION OF (0,0)");
				}
			}
				
			GameObject newStrandJumpEffect = (GameObject)Instantiate(p_StrandJumpPrefab, this.transform.position, ImpactAngle);
			Vector3 theLocalScale = new Vector3(1f, 1f, 1f);
			float strandScaleM = (FighterState.Vel.magnitude/200);
			strandScaleM = (strandScaleM<1) ? 1 : strandScaleM; // if strandscale multiplier less than 1, set it to one.
			strandScaleM = (strandScaleM>5) ? 5 : strandScaleM; // if strandscale multiplier greater than 5, set it to five.
			newStrandJumpEffect.transform.localScale = theLocalScale*strandScaleM;
			k_IsKinematic = true;
			k_KinematicAnim = 0;

			newStrandJumpEffect.GetComponentInChildren<StrandJumpEffect>().SetFighterChar(this);
			//InstantForce(newDirection, FighterState.Vel.magnitude*(1-m.strandJumpSpeedLossM));	
			m.strandJumpReflectSpd = FighterState.Vel.magnitude*(1-m.strandJumpSpeedLossM);
			m.strandJumpReflectDir = newDirection;

			FighterState.EtherLevel--;
			o.fighterAudio.StrandJumpSound();
		}
	}

	#endregion
	//###################################################################################################################################
	// PUBLIC FUNCTIONS
	//###################################################################################################################################
	#region PUBLIC FUNCTIONS

	public float Get2DAngle(Vector2 vector2, float degOffset) // Get angle, from -180 to +180 degrees. Degree offset shifts the origin from up, clockwise, by the amount of degrees specified. For example, 90 degrees shifts the origin to horizontal right.
	{
		float angle = Mathf.Atan2(vector2.x, vector2.y)*Mathf.Rad2Deg;
		angle = degOffset-angle;
		if(angle>180)
			angle = -360+angle;
		if(angle<-180)
			angle = 360+angle;
		return angle;
	}

	public void ChargeBackfire(int chargeAmount)
	{
		this.TakeDamage(chargeAmount*5);
		g_Stunned = true;
		g_CurStun = 2+chargeAmount/10;
		Vector2 jumpNormal = -FighterState.PlayerMouseVector.normalized;
		if(FighterState.EtherLevel > 0)
		{
			FighterState.EtherLevel--;
		}

		FighterState.Vel = FighterState.Vel+(jumpNormal*(m.etherJumpForceBase+(m.etherJumpForcePerCharge*(chargeAmount*3))));
		SpawnExplosionEffect(FighterState.Vel.magnitude*2);
		//o.fighterAudio.CraterSound(FighterState.Vel.magnitude*5, m.craterT, 1000f);
	}

	public float Get2DAngle(Vector2 vector2) // Get angle, from -180 to +180 degrees. Degree offset to horizontal right.
	{
		float angle = Mathf.Atan2(vector2.x, vector2.y)*Mathf.Rad2Deg;
		angle = 90-angle;
		if(angle>180)
			angle = -360+angle;
		return angle;
	}

	public float Get2DAngle(Vector3 vector3, float degOffset)
	{
		float angle = Mathf.Atan2(vector3.x, vector3.y)*Mathf.Rad2Deg;
		angle = degOffset-angle;
		if(angle>180)
			angle = -360+angle;
		return angle;
	}

	public float Get2DAngle(Vector3 vector3)
	{
		float angle = Mathf.Atan2(vector3.x, vector3.y)*Mathf.Rad2Deg;
		angle = 90-angle;
		if(angle>180)
			angle = -360+angle;
		return angle;
	}

	public bool IsVelocityPunching()
	{
		return g_VelocityPunching;
	}

	public bool isAlive()
	{
		return !FighterState.Dead;
	}

	public bool isSliding()
	{
		return v.sliding;
	}

	public void InstantForce(Vector2 newDirection, float speed)
	{
		//newDirection.Normalize();
		SetSpeed(newDirection, speed);
		//DirectionChange(newDirection);
		//print("Changing direction to" +newDirection);
	}

	public void LightPunchConnect(GameObject victim, Vector2 aimDirection)
	{// This is for simple one-click punches. Velocity punches use a different function.
		if((isAPlayer&&!isLocalPlayer)|| !isAPlayer&&!isServer){return;}
		FighterChar enemyFighter = null;

		if(victim != null)
		{
			enemyFighter = victim.GetComponent<FighterChar>();
		}
		if(enemyFighter != null)
		{
			enemyFighter.TakeDamage(5);
			enemyFighter.v.triggerFlinched = true;
			enemyFighter.v.facingDirection = (this.transform.position.x-enemyFighter.transform.position.x < 0) ? false : true; // If you are to their left, face left. Otherwise, right.

			if(!enemyFighter.IsPlayer())
			{
				enemyFighter.g_CurStun = 0.2f;
				enemyFighter.g_Staggered = true;
			}

			enemyFighter.FighterState.Vel += aimDirection.normalized*5;
			o.fighterAudio.PunchHitSound();

			float Magnitude = 1f;
			float Roughness = 20f;
			float FadeOutTime = 0.6f;
			float FadeInTime = 0.0f;
			float posX = 0.5f*aimDirection.normalized.x;
			float posY = 0.5f*aimDirection.normalized.y;
			Vector3 RotInfluence = new Vector3(0,0,0);
			Vector3 PosInfluence = new Vector3(posX,posY,0);
			CameraShaker.Instance.ShakeOnce(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);
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
			enemyFighter.TakeDamage(5);
			enemyFighter.FighterState.Vel += aimDirection.normalized*5;
			o.fighterAudio.PunchHitSound();
			//print("Punch connected remotely");
		}
	}


	public bool IsDisabled()
	{
		if(FighterState.Dead||(g_CurStun>0))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public bool IsKinematic()
	{
		return k_IsKinematic;
	}


	public float GetInstantGForce()
	{
		return phys.IGF;
	}

	public float GetContinuousGForce()
	{
		return phys.CGF;
	}

	public Vector2 GetVelocity()
	{
		return FighterState.Vel;
	}

	public Vector2 GetPosition()
	{
		return FighterState.FinalPos;
	}

	public Vector2 GetFootPosition()
	{
		return m_GroundFoot.position;
	}

	public float GetSpeed()
	{
		return FighterState.Vel.magnitude;
	}

	public void SetEtherLevel(int etherLevel)
	{
		FighterState.EtherLevel = etherLevel;
	}

	public int GetEtherLevel()
	{
		return FighterState.EtherLevel;
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
		if(dmgAmount==0||d.invincible){return;}
		FlashEffect(0.2f, Color.red);
		FighterState.CurVigor -= dmgAmount;
		if(dmgAmount>15)
		{
			o.fighterAudio.PainSound();
		}
	}

	public bool IsPlayer()
	{
		return isAPlayer;
	}

	public bool IsEnemyOf(int team)
	{
		if(team==this.g_Team)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	public int GetTeam()
	{
		return g_Team;
	}



	#endregion
}

[System.Serializable] public class AudioVisualVars
{

	[SerializeField][ReadOnlyAttribute]public bool facingDirection; 	// True means right, false means left.
	[SerializeField][ReadOnlyAttribute]public int facingDirectionV; 	// 1 means up, -1 means down, and 0 means horizontal.
	[SerializeField][Range(0,10)]public float reversingSlideT;		// How fast the fighter must be going to go into a slide posture when changing directions.
	[SerializeField][Range(0,1)] public int cameraMode; 			 	// What camera control type is in use.
	[SerializeField][Range(0,1)] public int defaultCameraMode;		// What camera control type to default to in normal gameplay.
	[SerializeField][Range(0,1)] public float cameraXLeashM; 			// How close the player can get to the edge of the screen horizontally. 1 is at the edge, whereas 0 is locked to the center of the screen.
	[SerializeField][Range(0,1)] public float cameraYLeashM; 			// How close the player can get to the edge of the screen horizontally. 1 is at the edge, whereas 0 is locked to the center of the screen.
	[SerializeField][Range(0,1)] public float cameraXLeashLim; 	 	// MUST BE SET HIGHER THAN LEASHM. Same as above, except when it reaches this threshold it instantly stops the camera at the edge rather than interpolating it there.
	[SerializeField][Range(0,1)] public float cameraYLeashLim; 	 	// MUST BE SET HIGHER THAN LEASHM. Same as above, except when it reaches this threshold it instantly stops the camera at the edge rather than interpolating it there.
	[SerializeField][Range(0.01f,20)] public float dustMoteFrequency;	// Amount of dustmotes generated behind the player per second.
	[SerializeField][ReadOnlyAttribute] public float dustMoteTimer; 	// Records the time between dust cloud spawns.
	[SerializeField][ReadOnlyAttribute] public float distFromLastDust;// Records the distance from the last dust cloud produced;
	[SerializeField][Range(0,200)] public float distBetweenDust; 		// Sets the max distance between dust clouds.
	[SerializeField][ReadOnlyAttribute] public Color defaultColor; 	// Set to the colour selected on the object's spriterenderer component.
	[SerializeField][ReadOnlyAttribute] public Color currentColor; 	// Set to the colour selected on the object's spriterenderer component.
	[SerializeField] public Color chargedColor; 						// Colour the fighter will be when fully charged.
	[SerializeField][ReadOnlyAttribute] public bool triggerAtkHit;	// Set to true to activate the attack hit animation.
	[SerializeField][ReadOnlyAttribute] public bool triggerRollOut;	// Set to true to activate the guard roll animation.
	[SerializeField][ReadOnlyAttribute] public bool triggerFlinched;	// Set to true to activate the flinch animation.
	[SerializeField][Range(0,1)]public float punchStrengthSlowmoT;	// Percent of maximum clash power at which a player's attack will activate slow motion.
	[SerializeField] public bool gender;								// Used for character audio.
	[SerializeField] public bool triggerGenderChange;					// Used for character audio.
	[SerializeField][Range(0, 1000)]public float speedForMaxLean;		// The speed at which the player's sprite is fully rotated to match the ground angle. Used to improve animation realism by leaning against GForces and wind drag. 
	[SerializeField][ReadOnlyAttribute]public float leanAngle;		// The angle the sprite is rotated to simulate leaning. Used to improve animation realism by leaning against GForces and wind drag. 
	[SerializeField][ReadOnlyAttribute]public int primarySurface;		// The main surface the player is running on. -1 is airborne, 0 is ground, 1 is ceiling, 2 is leftwall, 3 is rightwall. Lingers for a moment before going airborne, in order to hide microbumps in the terrain which would cause animation stuttering.
	[SerializeField][ReadOnlyAttribute]public int truePrimarySurface;	// The main surface the player is running on. -1 is airborne, 0 is ground, 1 is ceiling, 2 is leftwall, 3 is rightwall. More accurate version of primary surface that does not linger for a moment upon leaving a surface. 
	[SerializeField][ReadOnlyAttribute]public bool wallSliding;		// Whether or not the player is wallsliding.
	[SerializeField][ReadOnlyAttribute]public bool sliding;			// Whether or not the player is sliding.
	[SerializeField][ReadOnlyAttribute]public string[] terrainType;	// Type of terrain the player is stepping on. Used for audio like footsteps.
	[SerializeField][ReadOnlyAttribute]public bool highSpeedMode;		// True when the player hits a certain speed threshold and changes animations.
	[SerializeField][ReadOnlyAttribute]public float highSpeedModeT;	// Speed at which the player becomes a human projectile and switches to different animations.
	[SerializeField][ReadOnlyAttribute]public float flashTimer; 		// Remaining time on a flash effect.
	[SerializeField][ReadOnlyAttribute]public float flashDuration; 	// Duration that a flash effect lasts for.
	[SerializeField][ReadOnlyAttribute]public Color flashColour; 		// Colour of the player flash.


	public void SetDefaults()
	{
		reversingSlideT = 5;			// How fast the fighter must be going to go into a slide posture when changing directions.
		defaultCameraMode = 1;		// What camera control type to default to in normal gameplay.
		cameraXLeashM = 0.5f; 		// How close the player can get to the edge of the screen horizontally. 1 is at the edge, whereas 0 is locked to the center of the screen.
		cameraYLeashM = 0.5f; 		// How close the player can get to the edge of the screen horizontally. 1 is at the edge, whereas 0 is locked to the center of the screen.
		cameraXLeashLim = 0.8f; 	 	// MUST BE SET HIGHER THAN LEASHM. Same as above, except when it reaches this threshold it instantly stops the camera at the edge rather than interpolating it there.
		cameraYLeashLim = 0.8f;; 	 	// MUST BE SET HIGHER THAN LEASHM. Same as above, except when it reaches this threshold it instantly stops the camera at the edge rather than interpolating it there.
		dustMoteFrequency = 10;		// Amount of dustmotes generated behind the player per second.
		distBetweenDust = 0.1f; 		// Sets the max distance between dust clouds.
		chargedColor = Color.white; 	// Colour the fighter will be when fully charged.
		punchStrengthSlowmoT = 0.5f;	// Percent of maximum clash power at which a player's attack will activate slow motion.
		gender = true;				// Used for character audio.
		triggerGenderChange = true;	// Used for character audio.
		speedForMaxLean = 100;		// The speed at which the player's sprite is fully rotated to match the ground angle. Used to improve animation realism by leaning against GForces and wind drag. 
		highSpeedModeT = 100;			// Speed at which the player becomes a human projectile and switches to different animations.
	}
}

[System.Serializable] public struct PhysicsVars
{
	[SerializeField][ReadOnlyAttribute] public float IGF; 					//"Instant G-Force" of the impact this frame.
	[SerializeField][ReadOnlyAttribute] public float CGF; 					//"Continuous G-Force" over time.
	[SerializeField][ReadOnlyAttribute] public float remainingVelM;		//Remaining velocity proportion after an impact. Range: 0-1.
	[SerializeField][ReadOnlyAttribute] public Vector2 initialVel;			//Velocity at the start of the physics frame.
	[SerializeField][ReadOnlyAttribute] public Vector2 distanceTravelled;	//(x,y) distance travelled on current frame. Inversely proportional to remainingMovement.
	[SerializeField][ReadOnlyAttribute] public Vector2 remainingMovement; 	//Remaining (x,y) movement after impact.
	[SerializeField][ReadOnlyAttribute] public bool groundContact;			//True when touching surface.
	[SerializeField][ReadOnlyAttribute] public bool ceilingContact;			//True when touching surface.
	[SerializeField][ReadOnlyAttribute] public bool leftSideContact;			//True when touching surface.
	[SerializeField][ReadOnlyAttribute] public bool rightSideContact;           //True when touching surface.
	[SerializeField] [ReadOnlyAttribute] public bool closeToGround;         //True when the ground is at most [airborneCutoffLength] distance below the player. Can be true when groundcontact is false.

	[Space(10)]						    
	[SerializeField][ReadOnlyAttribute] public bool grounded;				// True when making contact with this direction.
	[SerializeField][ReadOnlyAttribute] public bool ceilinged; 			// True when making contact with this direction.
	[SerializeField][ReadOnlyAttribute] public bool leftWalled; 			// True when making contact with this direction.
	[SerializeField][ReadOnlyAttribute] public bool rightWalled;			// True when making contact with this direction.
	[Space(10)]						    
	[SerializeField][ReadOnlyAttribute] public bool groundBlocked;		// True when the player cannot move in this direction and movement input towards it is ignored.
	[SerializeField][ReadOnlyAttribute] public bool ceilingBlocked; 		// True when the player cannot move in this direction and movement input towards it is ignored.
	[SerializeField][ReadOnlyAttribute] public bool leftWallBlocked; 		// True when the player cannot move in this direction and movement input towards it is ignored.
	[SerializeField][ReadOnlyAttribute] public bool rightWallBlocked; 	// True when the player cannot move in this direction and movement input towards it is ignored.
	[Space(10)]						    
	[SerializeField][ReadOnlyAttribute] public bool surfaceCling;			//True when the player is clinging to an upside down surface. Whenever the player hits an upside down surface they have a grace period before gravity pulls them off.
	[SerializeField][ReadOnlyAttribute] public bool airborne;
	[SerializeField][ReadOnlyAttribute] public bool kneeling;				//True when fighter kneeling.
	[SerializeField][ReadOnlyAttribute] public bool worldImpact;			//True when the fighter has hit terrain on the current frame.
	[SerializeField][ReadOnlyAttribute] public Vector3 lastSafePosition;	//Used to revert player position if they get totally stuck in something.
}

[System.Serializable] public struct DebugVars //Debug variables.
{
	[SerializeField] public int errorDetectingRecursionCount; 	//Iterates each time recursive trajectory correction executes on the current frame. Not currently used.
	[SerializeField] public bool autoPressLeft; 				// When true, fighter will behave as if the left key is pressed.
	[SerializeField] public bool autoPressRight; 				// When true, fighter will behave as if the right key is pressed.	
	[SerializeField] public bool autoPressDown; 				// When true, fighter will behave as if the left key is pressed.
	[SerializeField] public bool autoPressUp; 					// When true, fighter will behave as if the right key is pressed.
	[SerializeField] public bool autoJump;						// When true, fighter jumps instantly on every surface.
	[SerializeField] public bool autoLeftClick;					// When true, fighter will behave as if left click is pressed.
	[SerializeField] public bool antiTunneling;					// When true, fighter will be pushed out of objects they are stuck in.
	[SerializeField] public bool gravityEnabled;				// Enables gravity.
	[SerializeField] public bool showVelocityIndicator;			// Shows a line tracing the character's movement path.
	[SerializeField] public bool showContactIndicators;			// Shows fighter's surface-contact raycasts, which turn green when touching something.
	[SerializeField] public bool recoverFromFullEmbed;			// When true and the fighter is fully stuck in something, teleports fighter to last good position.
	[SerializeField] public bool clickToKnockFighter;			// When true and you left click, the fighter is propelled toward where you clicked.
	[SerializeField] public bool sendCollisionMessages;			// When true, the console prints messages related to collision detection
	[SerializeField] public bool sendTractionMessages;			// When true, the console prints messages related to collision detection
	[SerializeField] public bool invincible;					// When true, the fighter does not take damage of any kind.	[SerializeField]private int d.tickCounter; 								// Counts which game logic tick the game is on. Rolls over at 60.
	[SerializeField] public int tickCounter; 					// Counts which game logic tick the game is on. Rolls over at 60
	[SerializeField] public LineRenderer debugLine; 			// Part of above indicators.
	[SerializeField] public LineRenderer groundLine;			// Part of above indicators.		
	[SerializeField] public LineRenderer ceilingLine;			// Part of above indicators.		
	[SerializeField] public LineRenderer leftSideLine;			// Part of above indicators.		
	[SerializeField] public LineRenderer rightSideLine;			// Part of above indicators.
}
[System.Serializable] public struct MovementVars 
{
	[Tooltip("The instant starting speed while moving")]
	[SerializeField] public float minSpeed; 						

	[Tooltip("The fastest the fighter can travel along land.")]
	[SerializeField] public float maxRunSpeed;					

	[Tooltip("Speed the fighter accelerates within the traction change threshold. (acceleration while changing direction)")]
	[Range(0,2)][SerializeField] public float startupAccelRate;   

	[Tooltip("How fast the fighter accelerates with input.")]
	[Range(0,5)][SerializeField] public float linearAccelRate;		

	[Tooltip("Amount of vertical force added when the fighter jumps.")]
	[SerializeField] public float vJumpForce;                  		

	[Tooltip("Amount of horizontal force added when the fighter jumps.")]
	[SerializeField] public float hJumpForce;  						

	[Tooltip("Amount of vertical force added when the fighter walljumps.")]
	[SerializeField] public float wallVJumpForce;                  	

	[Tooltip("Amount of horizontal force added when the fighter walljumps.")]
	[SerializeField] public float wallHJumpForce;  					

	[Tooltip("Threshold where movement changes from exponential to linear acceleration.")]
	[SerializeField] public float tractionChangeT;					

	[Tooltip("Speed threshold at which wallsliding traction changes.")]	
	[SerializeField] public float wallTractionT;						

	[Tooltip("How fast the fighter decelerates when changing direction.")]
	[Range(0,5)][SerializeField] public float linearStopRate; 		

	[Tooltip("How fast the fighter decelerates with no input.")]
	[Range(0,5)][SerializeField] public float linearSlideRate;		

	[Tooltip("How fast the fighter decelerates when running too fast.")]
	[Range(0,5)][SerializeField] public float linearOverSpeedRate;	

	[Tooltip("Any impacts at sharper angles than this will start to slow the fighter down.")]
	[Range(1,89)][SerializeField] public float impactDecelMinAngle;	

	[Tooltip("Any impacts at sharper angles than this will result in a full halt.")]
	[Range(1,89)][SerializeField] public float impactDecelMaxAngle;	

	[Tooltip("Changes the angle at which steeper angles start to linearly lose traction")]
	[Range(1,89)][SerializeField] public float tractionLossMinAngle; 

	[Tooltip("Changes the angle at which fighter loses ALL traction")][Range(45,90)]
	[SerializeField] public float tractionLossMaxAngle;

	[Tooltip("Changes how fast the fighter slides down overly steep slopes.")]
	[Range(0,2)][SerializeField] public float slippingAcceleration;  	

	[Tooltip("How long the fighter can cling to walls before gravity takes over.")]
	[Range(0.5f,3)][SerializeField] public float surfaceClingTime; 	

	[Tooltip("This is the amount of impact GForce required for a full-duration ceiling cling.")]
	[Range(20,70)][SerializeField] public float clingReqGForce;		

	[Tooltip("This is the normal of the last surface clung to, to make sure the fighter doesn't repeatedly cling the same surface after clingtime expires.")]
	[ReadOnlyAttribute]public Vector2 expiredNormal;						

	[Tooltip("Amount of time the fighter has been clung to a wall.")]
	[ReadOnlyAttribute]public float timeSpentHanging;					

	[Tooltip("Max time the fighter can cling to a wall.")]
	[ReadOnlyAttribute]public float maxTimeHanging;					

	[Tooltip("How deep into objects the character can be before actually colliding with the ")]
	[Range(0,0.5f)][SerializeField]public float maxEmbed;			

	[Tooltip("How deep into objects the character will sit by default. A value of zero will cause physics errors because the fighter is not technically *touching* the surface.")]
	[Range(0.01f,0.4f)][SerializeField]public float minEmbed; 

	[Space(10)]

	[Tooltip("")][SerializeField] public float etherJumpForcePerCharge; 				// How much force does each Ether Charge add to the jump power?
	[Tooltip("")][SerializeField] public float etherJumpForceBase; 					// How much force does a no-power Ether jump have?

	[Space(10)]

	[Tooltip("")][SerializeField] public float velPunchT; 							// Impact threshold for Velocity Punch trigger
	[Tooltip("")][SerializeField] public float slamT; 							// Impact threshold for slam
	[Tooltip("")][SerializeField] public float craterT; 							// Impact threshold for crater
	[Tooltip("")][SerializeField] public float guardSlamT; 						// Guarded Impact threshold for slam
	[Tooltip("")][SerializeField] public float guardCraterT; 					// Guarded Impact threshold for crater

	[Space(10)]

	[Tooltip("")][SerializeField][ReadOnlyAttribute]public int jumpBufferG; //Provides an n frame buffer to allow players to jump after leaving the ground.
	[Tooltip("")][SerializeField][ReadOnlyAttribute]public int jumpBufferC; //Provides an n frame buffer to allow players to jump after leaving the ceiling.
	[Tooltip("")][SerializeField][ReadOnlyAttribute]public int jumpBufferL; //Provides an n frame buffer to allow players to jump after leaving the leftwall.
	[Tooltip("")][SerializeField][ReadOnlyAttribute]public int jumpBufferR; //Provides an n frame buffer to allow players to jump after leaving the rightwall.
	[Tooltip("")][SerializeField][Range(1,600)] public int jumpBufferFrameAmount; //Dictates the duration of the jump buffer (in physics frames). IMPORTANT: Do not merge this with airborne delay because you need to jump after going over sheer cliffs.

	[Tooltip("")][SerializeField][Range(0,2)] public float airborneDelay; //Amount of time after leaving the ground that the player behaves as if they are airborne. Prevents jittering caused by small bumps in the environment.
	[Tooltip("")][SerializeField][ReadOnlyAttribute] public float airborneDelayTimer; //Time remaining before the player is treated as airborne upon leaving a surface. Negative 1 when immediately canceled.
	[Tooltip("")] [SerializeField][Range(0,1)] public float airborneCutoffLength; //Max distance the fighter can be from the ground before airborne delay is canceled. Stops the player from airwalking when far above the ground.

	[Space(10)]

	[Tooltip("")][SerializeField][Range(0,1)] public float strandJumpSpeedLossM; //Percent of speed lost with each strand Jump
	[Tooltip("")][SerializeField][ReadOnlyAttribute] public float strandJumpReflectSpd;
	[Tooltip("")][SerializeField][ReadOnlyAttribute] public Vector2 strandJumpReflectDir;
	[Tooltip("")][SerializeField][Range(0f,180f)] public float widestStrandJumpAngle;

	[Space(10)]

	[Tooltip("True when a jump command will result in a critical jump.")]
	[SerializeField][ReadOnlyAttribute] public bool critJumpReady;

	[Tooltip("Starting duration of the critical jump window.")]
	[SerializeField] public float critJumpWindow;

	[Tooltip("Amount of time the player has after landing before the crit jump window closes.")]
	[SerializeField][ReadOnlyAttribute] public float critJumpTimer;

	[Tooltip("Starting duration of the critical jump frame window.")]
	[SerializeField] public int critJumpFrameWindow;

	[Tooltip("An extra fallback for low FPS players, the minimum number of frames that must play after landing before the crit jump window closes.")]
	[SerializeField][ReadOnlyAttribute] public int critJumpFrameTimer;

	[Tooltip("Amount of bonus force from a crit jump.")]
	[SerializeField][Range(1f,2f)] public float critJumpBonusM;

	public void SetDefaults()
	{
		airborneDelayTimer = 0;
		critJumpFrameTimer = 0;
		critJumpTimer = 0;
		strandJumpReflectDir = Vector2.zero;
		critJumpReady = false;
		expiredNormal = Vector2.zero;
		jumpBufferC = 0;
		jumpBufferG = 0;
		jumpBufferL = 0;
		jumpBufferR = 0;

		minSpeed = 10f; 						
		maxRunSpeed = 200f;					
		startupAccelRate = 0.8f;   
		linearAccelRate = 0.4f;		
		vJumpForce = 40f;                  		
		hJumpForce = 5f;  						
		wallVJumpForce = 20f;                  	
		wallHJumpForce = 10f;  					
		tractionChangeT = 20f;					
		wallTractionT = 20f;						
		linearStopRate = 2f; 		
		linearSlideRate = 0.35f;		
		linearOverSpeedRate = 0.1f;	
		impactDecelMinAngle = 20f;	
		impactDecelMaxAngle = 80f;	
		tractionLossMinAngle = 45f; 
		tractionLossMaxAngle = 78f;
		slippingAcceleration = 1f;  	
		surfaceClingTime = 1f; 	
		clingReqGForce = 50f;		
		timeSpentHanging = 0f;					
		maxTimeHanging = 0f;					
		maxEmbed = 0.02f;			
		minEmbed = 0.01f; 
		etherJumpForcePerCharge = 5f; 			// How much force does each Ether Charge add to the jump power?
		etherJumpForceBase = 40f; 				// How much force does a no-power Ether jump have?
		velPunchT = 60f; 						// Impact threshold for Velocity Punch trigger
		slamT = 100f; 							// Impact threshold for slam
		craterT = 200f; 						// Impact threshold for crater
		guardSlamT = 200f; 						// Guarded Impact threshold for slam
		guardCraterT = 400f; 					// Guarded Impact threshold for crater
		jumpBufferFrameAmount = 10; 			//Dictates the duration of the jump buffer (in physics frames).
		airborneDelay = 0.5f; 					//Amount of time after leaving the ground that the player behaves as if they are airborne. Prevents jittering caused by small bumps in the environment.
		strandJumpSpeedLossM = 0; 				//Percent of speed lost with each strand Jump
		strandJumpReflectSpd = 0;
		widestStrandJumpAngle = 45;
		critJumpWindow = 0.17f;
		critJumpFrameWindow = 2;
		critJumpBonusM = 1.33f; 
	}
}
[System.Serializable] public struct ObjectRefs 
{
	[SerializeField][ReadOnlyAttribute] public FighterAudio fighterAudio;			// Reference to the character's audio handler.
	[SerializeField][ReadOnlyAttribute] public VelocityPunch velocityPunch;		// Reference to the velocity punch visual effect entity attached to the character.
	[SerializeField][ReadOnlyAttribute] public Transform spriteTransform;			// Reference to the velocity punch visual effect entity attached to the character.
	[SerializeField][ReadOnlyAttribute] public Transform dustSpawnTransform;		// Reference to the velocity punch visual effect entity attached to the character.
	[SerializeField][ReadOnlyAttribute] public TimeManager timeManager;     	// Reference to the game level's timescale manager.
	[SerializeField][ReadOnlyAttribute] public SpriteRenderer spriteRenderer;	// Reference to the character's sprite renderer.
	[SerializeField][ReadOnlyAttribute] public ItemHandler itemHandler;		// Reference to the itemhandler, which acts as an authority on item stats and indexes.
	[SerializeField][ReadOnlyAttribute] public Shoe equippedShoe;      		// Reference to the player's currently equipped shoe.
	[SerializeField][ReadOnlyAttribute] public GameObject sparkThrower;      	// Reference to the player's currently equipped shoe.
	[SerializeField][ReadOnlyAttribute] public Animator anim;           		// Reference to the character's animator component.
	[SerializeField][ReadOnlyAttribute] public Rigidbody2D rigidbody2D;		// Reference to the character's physics body.
	[SerializeField][ReadOnlyAttribute] public Transform debugAngleDisplay;	// Reference to a transform of an angle display child transform of the player.
	[SerializeField][ReadOnlyAttribute] public NavMaster navMaster;			// Global navmesh handler for the level.
}
[System.Serializable] public struct FighterState
{
	[SerializeField][ReadOnlyAttribute]public int EtherLevel;					// Level of fighter Ether Power.
	[SerializeField][ReadOnlyAttribute]public bool DevMode;					// Turns on all dev cheats.
	[SerializeField][ReadOnlyAttribute]public int CurVigor;				// Current health.
	[SerializeField][ReadOnlyAttribute]public bool Dead;					// True when the fighter's health reaches 0 and they die.
	[SerializeField][ReadOnlyAttribute]public int Stance;					// Combat stance which dictates combat actions and animations. 0 = neutral, 1 = attack(leftmouse), 2 = guard(rightclick). 

	[SerializeField][ReadOnlyAttribute]public bool JumpKeyPress;
	[SerializeField][ReadOnlyAttribute]public bool ShiftKeyPress;
	[SerializeField][ReadOnlyAttribute]public bool LeftClickPress;
	[SerializeField][ReadOnlyAttribute]public bool RightClickPress;
	[SerializeField][ReadOnlyAttribute]public bool LeftKeyPress;
	[SerializeField][ReadOnlyAttribute]public bool RightKeyPress;
	[SerializeField][ReadOnlyAttribute]public bool UpKeyPress;
	[SerializeField][ReadOnlyAttribute]public bool DownKeyPress;

	[SerializeField][ReadOnlyAttribute]public float ScrollWheel;

	[SerializeField][ReadOnlyAttribute]public bool LeftClickHold;
	[SerializeField][ReadOnlyAttribute]public bool RightClickHold;
	[SerializeField][ReadOnlyAttribute]public bool ShiftKeyHold;
	[SerializeField][ReadOnlyAttribute]public bool LeftKeyHold;
	[SerializeField][ReadOnlyAttribute]public bool RightKeyHold;
	[SerializeField][ReadOnlyAttribute]public bool UpKeyHold;
	[SerializeField][ReadOnlyAttribute]public bool DownKeyHold;

	[SerializeField][ReadOnlyAttribute]public bool RightClickRelease;
	[SerializeField][ReadOnlyAttribute]public bool LeftClickRelease;
	[SerializeField][ReadOnlyAttribute]public bool ShiftKeyRelease;
	[SerializeField][ReadOnlyAttribute]public bool LeftKeyRelease;
	[SerializeField][ReadOnlyAttribute]public bool RightKeyRelease;
	[SerializeField][ReadOnlyAttribute]public bool UpKeyRelease;
	[SerializeField][ReadOnlyAttribute]public bool DownKeyRelease;

	[SerializeField][ReadOnlyAttribute]public bool LeftKeyDoubleTapReady;
	[SerializeField][ReadOnlyAttribute]public bool RightKeyDoubleTapReady;
	[SerializeField][ReadOnlyAttribute]public bool UpKeyDoubleTapReady;
	[SerializeField][ReadOnlyAttribute]public bool DownKeyDoubleTapReady;

	[SerializeField][ReadOnlyAttribute]public float LeftKeyDoubleTapDelay;
	[SerializeField][ReadOnlyAttribute]public float RightKeyDoubleTapDelay;
	[SerializeField][ReadOnlyAttribute]public float UpKeyDoubleTapDelay;
	[SerializeField][ReadOnlyAttribute]public float DownKeyDoubleTapDelay;



	[SerializeField][ReadOnlyAttribute]public bool EtherKeyPress;
	[SerializeField][ReadOnlyAttribute]public bool DisperseKeyPress;
	[SerializeField][ReadOnlyAttribute]public bool DevkeyTilde;
	[SerializeField][ReadOnlyAttribute]public bool DevKey1;
	[SerializeField][ReadOnlyAttribute]public bool DevKey2;
	[SerializeField][ReadOnlyAttribute]public bool DevKey3;
	[SerializeField][ReadOnlyAttribute]public bool DevKey4;
	[SerializeField][ReadOnlyAttribute]public bool DevKey5;
	[SerializeField][ReadOnlyAttribute]public bool DevKey6;
	[SerializeField][ReadOnlyAttribute]public bool DevKey7;
	[SerializeField][ReadOnlyAttribute]public bool DevKey8;
	[SerializeField][ReadOnlyAttribute]public bool DevKey9;
	[SerializeField][ReadOnlyAttribute]public bool DevKey10;
	[SerializeField][ReadOnlyAttribute]public bool DevKey11;
	[SerializeField][ReadOnlyAttribute]public bool DevKey12;
	[SerializeField][ReadOnlyAttribute]public float LeftClickHoldDuration;
	[SerializeField][ReadOnlyAttribute]public Vector2 MouseWorldPos;				// Mouse position in world coordinates.
	[SerializeField][ReadOnlyAttribute]public Vector2 PlayerMouseVector;			// Vector pointing from the player to their mouse position.
	[SerializeField][ReadOnlyAttribute]public Vector2 Vel;							//Current (x,y) velocity.
	[SerializeField]public Vector2 FinalPos;						//The final position of the character at the end of the physics frame.
}