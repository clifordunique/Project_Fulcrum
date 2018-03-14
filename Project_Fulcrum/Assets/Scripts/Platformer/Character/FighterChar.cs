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

	//############################################################################################################################################################################################################
	// DEBUGGING VARIABLES
	//##########################################################################################################################################################################
	#region DEBUGGING
	protected int errorDetectingRecursionCount; 			//Iterates each time recursive trajectory correction executes on the current frame. Not currently used.
	[Header("Debug:")]
	[SerializeField] protected bool autoPressLeft; 			// When true, fighter will behave as if the left key is pressed.
	[SerializeField] protected bool autoPressRight; 		// When true, fighter will behave as if the right key is pressed.	
	[SerializeField] protected bool autoPressDown; 			// When true, fighter will behave as if the left key is pressed.
	[SerializeField] protected bool autoPressUp; 			// When true, fighter will behave as if the right key is pressed.
	[SerializeField] protected bool autoJump;				// When true, fighter jumps instantly on every surface.
	[SerializeField] protected bool autoLeftClick;			// When true, fighter will behave as if left click is pressed.
	[SerializeField] protected bool antiTunneling = true;	// When true, fighter will be pushed out of objects they are stuck in.
	[SerializeField] protected bool noGravity;				// Disable gravity.
	[SerializeField] protected bool showVelocityIndicator;	// Shows a line tracing the character's movement path.
	[SerializeField] protected bool showContactIndicators;	// Shows fighter's surface-contact raycasts, which turn green when touching something.
	[SerializeField] protected bool recoverFromFullEmbed=true;// When true and the fighter is fully stuck in something, teleports fighter to last good position.
	[SerializeField] protected bool d_ClickToKnockFighter;	// When true and you left click, the fighter is propelled toward where you clicked.
	[SerializeField] protected bool d_SendCollisionMessages;// When true, the console prints messages related to collision detection
	[SerializeField] protected bool d_SendTractionMessages;	// When true, the console prints messages related to collision detection
	[SerializeField] protected bool d_Invincible;			// When true, the fighter does not take damage of any kind.	[SerializeField]private int d_TickCounter; 								// Counts which game logic tick the game is on. Rolls over at 60.
	[SerializeField] protected int d_TickCounter; 								// Counts which game logic tick the game is on. Rolls over at 60
	protected LineRenderer m_DebugLine; 					// Part of above indicators.
	protected LineRenderer m_GroundLine;					// Part of above indicators.		
	protected LineRenderer m_CeilingLine;					// Part of above indicators.		
	protected LineRenderer m_LeftSideLine;					// Part of above indicators.		
	protected LineRenderer m_RightSideLine;					// Part of above indicators.		
	#endregion
	//############################################################################################################################################################################################################
	// MOVEMENT HANDLING VARIABLES
	//###########################################################################################################################################################################
	#region MOVEMENT HANDLING
	[Header("Movement Tuning:")]
	[Tooltip("The instant starting speed while moving")][SerializeField] 
	protected float m_MinSpeed = 10f; 							// The instant starting speed while moving
	[Tooltip("The fastest the fighter can travel along land.")][SerializeField] 
	protected float m_MaxRunSpeed = 200f;						// The fastest the fighter can travel along land.
	[Tooltip("Speed the fighter accelerates within the traction change threshold. (Changing directions acceleration)")][Range(0,2)][SerializeField] 
	protected float m_StartupAccelRate = 0.8f;    		// Speed the fighter accelerates within the traction change threshold. (Changing directions acceleration)
	[Tooltip("How fast the fighter accelerates with input.")][Range(0,5)][SerializeField] 
	protected float m_LinearAccelRate = 0.4f;		// How fast the fighter accelerates with input.
	[Tooltip("Amount of vertical force added when the fighter jumps.")][SerializeField] 
	protected float m_VJumpForce = 40f;                  		// Amount of vertical force added when the fighter jumps.
	[Tooltip("Amount of horizontal force added when the fighter jumps.")][SerializeField] 
	protected float m_HJumpForce = 5f;  						// Amount of horizontal force added when the fighter jumps.
	[Tooltip("Amount of vertical force added when the fighter walljumps.")][SerializeField] 
	protected float m_WallVJumpForce = 20f;                  	// Amount of vertical force added when the fighter walljumps.
	[Tooltip("Amount of horizontal force added when the fighter walljumps.")][SerializeField] 
	protected float m_WallHJumpForce = 10f;  					// Amount of horizontal force added when the fighter walljumps.
	[Tooltip("Threshold where movement changes from exponential to linear acceleration.")][SerializeField] 
	protected float m_TractionChangeT = 20f;					// Threshold where movement changes from exponential to linear acceleration.  
	[Tooltip("Speed threshold at which wallsliding traction changes.")]	[SerializeField] 
	protected float m_WallTractionT = 20f;						// Speed threshold at which wallsliding traction changes.
	[Tooltip("How fast the fighter decelerates when changing direction.")][Range(0,5)][SerializeField] 
	protected float m_LinearStopRate = 2f; 		// How fast the fighter decelerates when changing direction.
	[Tooltip("How fast the fighter decelerates with no input.")][Range(0,5)][SerializeField] 
	protected float m_LinearSlideRate = 0.35f;		// How fast the fighter decelerates with no input.
	[Tooltip("How fast the fighter decelerates when running too fast.")][Range(0,5)][SerializeField] 
	protected float m_LinearOverSpeedRate = 0.1f;	// How fast the fighter decelerates when running too fast.
	[Tooltip("Any impacts at sharper angles than this will start to slow the fighter down.")][Range(1,89)][SerializeField] 
	protected float m_ImpactDecelMinAngle = 20f;	// Any impacts at sharper angles than this will start to slow the fighter down. Reaches full halt at m_ImpactDecelMaxAngle.
	[Tooltip("Any impacts at sharper angles than this will result in a full halt.")][Range(1,89)][SerializeField] 
	protected float m_ImpactDecelMaxAngle = 80f;	// Any impacts at sharper angles than this will result in a full halt. DO NOT SET THIS LOWER THAN m_ImpactDecelMinAngle!!
	[Tooltip("Changes the angle at which steeper angles start to linearly lose traction")][Range(1,89)][SerializeField] 
	protected float m_TractionLossMinAngle = 45f; // Changes the angle at which steeper angles start to linearly lose traction, and eventually starts slipping back down. Default of 45 degrees.
	[Tooltip("Changes the angle at which fighter loses ALL traction")][Range(45,90)][SerializeField] 
	protected float m_TractionLossMaxAngle = 78f;// Changes the angle at which fighter loses ALL traction, and starts slipping back down. Default of 90 degrees.
	[Tooltip("Changes how fast the fighter slides down overly steep slopes.")][Range(0,2)][SerializeField] 
	protected float m_SlippingAcceleration = 1f;  	// Changes how fast the fighter slides down overly steep slopes.
	[Tooltip("How long the fighter can cling to walls before gravity takes over.")][Range(0.5f,3)][SerializeField] 
	protected float m_SurfaceClingTime = 1f; 	// How long the fighter can cling to walls before gravity takes over.
	[Tooltip("This is the amount of impact GForce required for a full-duration ceiling cling.")][Range(20,70)][SerializeField] 
	protected float m_ClingReqGForce = 50f;		// This is the amount of impact GForce required for a full-duration ceiling cling.
	[Tooltip("This is the normal of the last surface clung to, to make sure the fighter doesn't repeatedly cling the same surface after clingtime expires.")][ReadOnlyAttribute]
	protected Vector2 m_ExpiredNormal;						// This is the normal of the last surface clung to, to make sure the fighter doesn't repeatedly cling the same surface after clingtime expires.
	[Tooltip("Amount of time the fighter has been clung to a wall.")][ReadOnlyAttribute]
	protected float m_TimeSpentHanging = 0f;					// Amount of time the fighter has been clung to a wall.
	[Tooltip("Max time the fighter can cling to a wall.")][ReadOnlyAttribute]
	protected float m_MaxTimeHanging = 0f;					// Max time the fighter can cling to current wall.
	[Tooltip("How deep into objects the character can be before actually colliding with them. ")][Range(0,0.5f)][SerializeField]
	protected float m_MaxEmbed = 0.02f;			// How deep into objects the character can be before actually colliding with them. MUST BE GREATER THAN m_MinEmbed!!!
	[Tooltip("How deep into objects the character will sit by default. A value of zero will cause physics errors because the fighter is not technically *touching* the surface.")][Range(0.01f,0.4f)][SerializeField]
	protected float m_MinEmbed = 0.01f; 	// How deep into objects the character will sit by default. A value of zero will cause physics errors because the fighter is not technically *touching* the surface.

	[Space(10)]

	[Tooltip("")][SerializeField] protected float m_ZonJumpForcePerCharge = 5f; 				// How much force does each Zon Charge add to the jump power?
	[Tooltip("")][SerializeField] protected float m_ZonJumpForceBase = 40f; 					// How much force does a no-power Zon jump have?

	[Space(10)]

	[Tooltip("")][SerializeField] public float m_VelPunchT = 60f; 							// Impact threshold for Velocity Punch trigger
	[Tooltip("")][SerializeField] protected float m_SlamT = 100f; 							// Impact threshold for slam
	[Tooltip("")][SerializeField] protected float m_CraterT = 200f; 							// Impact threshold for crater
	[Tooltip("")][SerializeField] protected float m_GuardSlamT = 200f; 						// Guarded Impact threshold for slam
	[Tooltip("")][SerializeField] protected float m_GuardCraterT = 400f; 					// Guarded Impact threshold for crater

	[Space(10)]

	[Tooltip("")][SerializeField][ReadOnlyAttribute]protected int m_JumpBufferG; //Provides a _ frame buffer to allow players to jump after leaving the ground.
	[Tooltip("")][SerializeField][ReadOnlyAttribute]protected int m_JumpBufferC; //Provides a _ frame buffer to allow players to jump after leaving the ceiling.
	[Tooltip("")][SerializeField][ReadOnlyAttribute]protected int m_JumpBufferL; //Provides a _ frame buffer to allow players to jump after leaving the leftwall.
	[Tooltip("")][SerializeField][ReadOnlyAttribute]protected int m_JumpBufferR; //Provides a _ frame buffer to allow players to jump after leaving the rightwall.
	[Tooltip("")][SerializeField][Range(1,600)] protected int m_JumpBufferFrameAmount; //Dictates the duration of the jump buffer in physics frames.

	[Space(10)]

	[Tooltip("")][SerializeField][Range(0,1)] protected float m_StrandJumpSpeedLossM; //Percent of speed lost with each strand Jump
	[Tooltip("")][SerializeField] protected float m_StrandJumpReflectSpd;
	[Tooltip("")][SerializeField] protected Vector2 m_StrandJumpReflectDir;
	[Tooltip("")][SerializeField][Range(0f,180f)] protected float m_WidestStrandJumpAngle;
	#endregion

	//############################################################################################################################################################################################################
	// KINEMATIC VARIABLES
	//###########################################################################################################################################################################
	#region KINEMATIC VARIABLES
	[SerializeField] protected bool k_IsKinematic; 						//Dictates whether the player is moving in physical fighterchar mode or in some sort of specially controlled fashion, such as in cutscenes or strand jumps
	[SerializeField] protected int k_KinematicAnim; 					//Designates the kinematic animation being played. 0 is strandjumping.
	[SerializeField] protected float k_StrandJumpSlowdownM = 0.33f; 		//Percent of momentum retained per frame when hitting a strand.
	[SerializeField] protected float k_StrandJumpSlowdownLinear = 5f; 	//Set amount of momentum lost per frame when hitting a strand.
	#endregion
	//############################################################################################################################################################################################################
	// OBJECT REFERENCES
	//###########################################################################################################################################################################
	#region OBJECT REFERENCES
	[Header("Character Components:")]
	[SerializeField][ReadOnlyAttribute] public FighterAudio o_FighterAudio;			// Reference to the character's audio handler.
	[SerializeField][ReadOnlyAttribute] public VelocityPunch o_VelocityPunch;		// Reference to the velocity punch visual effect entity attached to the character.
	[SerializeField][ReadOnlyAttribute] public Transform o_SpriteTransform;			// Reference to the velocity punch visual effect entity attached to the character.
	[SerializeField][ReadOnlyAttribute] public Transform o_DustSpawnTransform;		// Reference to the velocity punch visual effect entity attached to the character.
	[SerializeField][ReadOnlyAttribute] protected TimeManager o_TimeManager;     	// Reference to the game level's timescale manager.
	[SerializeField][ReadOnlyAttribute] protected SpriteRenderer o_SpriteRenderer;	// Reference to the character's sprite renderer.
	[SerializeField][ReadOnlyAttribute] protected ItemHandler o_ItemHandler;		// Reference to the itemhandler, which acts as an authority on item stats and indexes.
	[SerializeField][ReadOnlyAttribute] protected Shoe o_EquippedShoe;      		// Reference to the player's currently equipped shoe.
	[SerializeField][ReadOnlyAttribute] protected GameObject o_SparkThrower;      	// Reference to the player's currently equipped shoe.
	[SerializeField][ReadOnlyAttribute] protected Animator o_Anim;           		// Reference to the character's animator component.
	[SerializeField][ReadOnlyAttribute] protected Rigidbody2D o_Rigidbody2D;		// Reference to the character's physics body.
	[SerializeField][ReadOnlyAttribute] protected Transform o_DebugAngleDisplay;	// Reference to a transform of an angle display child transform of the player.
	[SerializeField][ReadOnlyAttribute] protected NavMaster o_NavMaster;			// Global navmesh handler for the level.

	[SerializeField] public GameObject p_ZonPulse;				// Reference to the Zon Pulse prefab, a pulsewave that emanates from the fighter when they disperse zon power.
	[SerializeField] public GameObject p_AirPunchPrefab;		// Reference to the air punch attack prefab.
	[SerializeField] public GameObject p_DebugMarker;			// Reference to a sprite prefab used to mark locations ingame during development.
	[SerializeField] public GameObject p_ShockEffectPrefab;		// Reference to the shock visual effect prefab.
	[SerializeField] public GameObject p_StrandJumpPrefab;		// Reference to the strand jump visual effect prefab.
	[SerializeField] public GameObject p_AirBurstPrefab;		// Reference to the air burst prefab, which is a radial windforce.
	[SerializeField] public GameObject p_DustEffectPrefab;		// Reference to the dust visual effect prefab.
	[SerializeField] public GameObject p_SparkEffectPrefab;		// Reference to the spark visual effect prefab.


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

	[Header("Fighter State:")]
	[SerializeField][ReadOnlyAttribute]protected float m_IGF; 					//"Instant G-Force" of the impact this frame.
	[SerializeField][ReadOnlyAttribute]protected float m_CGF; 					//"Continuous G-Force" over time.
	[SerializeField][ReadOnlyAttribute]protected float m_RemainingVelM;		//Remaining velocity proportion after an impact. Range: 0-1.
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
	[SerializeField][ReadOnlyAttribute]protected bool m_Kneeling;
	[SerializeField][ReadOnlyAttribute]protected bool m_WorldImpact;
	protected Vector3 lastSafePosition;										//Used to revert player position if they get totally stuck in something.
	#endregion
	//##########################################################################################################################################################################
	// FIGHTER INPUT VARIABLES
	//###########################################################################################################################################################################
	#region FIGHTERINPUT
	[Header("Input:")]
	[SerializeField] protected FighterState FighterState;// Struct holding all networked fighter info.
	protected int CtrlH; 													// Tracks horizontal keys pressed. Values are -1 (left), 0 (none), or 1 (right). 
	protected int CtrlV; 													// Tracks vertical keys pressed. Values are -1 (down), 0 (none), or 1 (up).
	protected bool facingDirection; 										// True means right, false means left.
	protected int facingDirectionV; 										// 1 means up, -1 means down, and 0 means horizontal.

	#endregion
	//############################################################################################################################################################################################################
	// VISUAL&SOUND VARIABLES
	//###########################################################################################################################################################################
	#region VISUALS&SOUND
	[Header("Visuals And Sound:")]
	[SerializeField][Range(0,10)]protected float v_ReversingSlideT = 5;		// How fast the fighter must be going to go into a slide posture when changing directions.
	[SerializeField][Range(0,3)] protected int v_FighterGlow;			 	// Amount of fighter "energy glow" effect.
	[SerializeField][ReadOnlyAttribute] protected float v_CameraZoom; 	 	// Amount of camera zoom.
	[SerializeField][Range(0,1)] protected int v_CameraMode; 			 	// What camera control type is in use.
	[SerializeField][Range(0,1)] protected int v_DefaultCameraMode = 1;		// What camera control type to default to in normal gameplay.
	[SerializeField][Range(0,1)] protected float v_CameraXLeashM; 			// How close the player can get to the edge of the screen horizontally. 1 is at the edge, whereas 0 is locked to the center of the screen.
	[SerializeField][Range(0,1)] protected float v_CameraYLeashM; 			// How close the player can get to the edge of the screen horizontally. 1 is at the edge, whereas 0 is locked to the center of the screen.
	[SerializeField][Range(0,1)] protected float v_CameraXLeashLim; 	 	// MUST BE SET HIGHER THAN LEASHM. Same as above, except when it reaches this threshold it instantly stops the camera at the edge rather than interpolating it there.
	[SerializeField][Range(0,1)] protected float v_CameraYLeashLim; 	 	// MUST BE SET HIGHER THAN LEASHM. Same as above, except when it reaches this threshold it instantly stops the camera at the edge rather than interpolating it there.
	[SerializeField][Range(0.01f,20)] protected float v_DustMoteFrequency;	// Amount of dustmotes generated behind the player per second.
	[SerializeField][ReadOnlyAttribute] protected float v_DustMoteTimer; 	// Records the time between dust cloud spawns.
	[SerializeField][ReadOnlyAttribute] protected float v_DistFromLastDust; // Records the distance from the last dust cloud produced;
	[SerializeField][Range(0,200)] protected float v_DistBetweenDust; 		// Sets the max distance between dust clouds.
	[SerializeField][ReadOnlyAttribute] protected Color v_DefaultColor; 	// Set to the colour selected on the object's spriterenderer component.
	[SerializeField][ReadOnlyAttribute] public bool v_TriggerAtkHit;		// Set to true to activate the attack hit animation.
	[SerializeField][ReadOnlyAttribute] public bool v_TriggerRollOut;		// Set to true to activate the guard roll animation.

	[SerializeField][ReadOnlyAttribute] protected float v_AirForgiveness;	// Amount of time the player can be in the air without animating as airborne. Useful for micromovements. NEEDS TO BE IMPLEMENTED
	[SerializeField][Range(0,1)]protected float v_PunchStrengthSlowmoT=0.5f;// Percent of maximum clash power at which a player's attack will activate slow motion.
	[SerializeField] protected bool v_Gender;								// Used for character audio.
	[SerializeField][Range(0, 1000)]protected float v_SpeedForMaxLean = 100;// The speed at which the player's sprite is fully rotated to match the ground angle. Used to simulate gforce effects changing required body leaning direction. 
	[SerializeField][ReadOnlyAttribute]protected float v_LeanAngle;			// The speed at which the player's sprite is fully rotated to match the ground angle. Used to simulate gforce effects changing required body leaning direction. 
	[SerializeField][ReadOnlyAttribute]protected int v_PrimarySurface;		// The main surface the player is running on. -1 is airborne, 0 is ground, 1 is ceiling, 2 is leftwall, 3 is rightwall.
	[SerializeField][ReadOnlyAttribute]protected bool v_WallSliding;		// Whether or not the player is wallsliding.
	[SerializeField][ReadOnlyAttribute]protected bool v_Sliding;			// Whether or not the player is sliding.
	[SerializeField][ReadOnlyAttribute]protected string[] v_TerrainType;	// Type of terrain the player is stepping on. Used for audio like footsteps.
	#endregion 
	//############################################################################################################################################################################################################
	// GAMEPLAY VARIABLES
	//###########################################################################################################################################################################
	#region GAMEPLAY VARIABLES
	[Header("Gameplay:")]
	[SerializeField] protected bool g_VelocityPunching;					// True when fighter is channeling a velocity fuelled punch.
	[SerializeField] protected float g_VelocityPunchChargeTime;			// Min duration the fighter can be stunned from slamming the ground.

	[SerializeField] protected int g_MaxHealth = 100;					// Max health.
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
	[SerializeField] public float g_FighterCollisionCDLength = 1f;		// Time after a fightercollision that further collision is disabled. This is the duration to wait. Later this should be modified to only affect one fighter.
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
		d_TickCounter++;
		d_TickCounter = (d_TickCounter > 60) ? 0 : d_TickCounter; // Rolls back to zero when hitting 60
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
//		FighterState.ZonKey = false;
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

	protected void UpdateCurrentNavSurf()
	{
		if(n_PlayerTraversing)
			n_PlayerTraversalTime += Time.fixedDeltaTime;

		n_CurrentSurf = null;
		n_CurrentSurfID = -1;

		Vector3 contactTransform;

		if(v_PrimarySurface==0)
		{
			contactTransform = this.m_GroundFoot.position;
		}
		else if(v_PrimarySurface==1)
		{
			contactTransform = this.m_CeilingFoot.position;
		}
		else if(v_PrimarySurface==2)
		{
			contactTransform = this.m_LeftSide.position;
		}
		else
		{
			contactTransform = this.m_RightSide.position;
		}
		NavSurface[] surfaceList = o_NavMaster.GetSurfaces();
		for(int i = 0; i<surfaceList.Length; i++)
		{
			if( surfaceList[i].DistFromLine(this.m_GroundFoot.position)<=n_MaxSurfLineDist && surfaceList[i].surfaceType == v_PrimarySurface)
			{
				n_CurrentSurf = surfaceList[i];
				n_CurrentSurfID = n_CurrentSurf.id;
				n_LastSurface = n_CurrentSurf;
				if(n_PlayerTraversing&&v_PrimarySurface!=-1) // If touching a new surface and not airborne, set that as the destination of your traversal.
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
		print("["+d_TickCounter+"]:Starting Player traversal recording");
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
			print("["+d_TickCounter+"]: "+n_TempNavCon.orig.id+" and "+n_CurrentSurf.id+" are same surface, not recording.");
			n_TempNavCon = null;
			return;
		}
		n_TempNavCon.dest = n_CurrentSurf;
		n_TempNavCon.destPosition = n_CurrentSurf.WorldToLinPos(this.transform.position);
		n_TempNavCon.averageTraversalTime = n_PlayerTraversalTime;
		n_TempNavCon.orig.AddNavConnection(n_TempNavCon);
		print("["+d_TickCounter+"]:Saved new navconnection between surfaces "+n_TempNavCon.orig.id+" and "+n_TempNavCon.dest.id+".");
	}
	
	protected virtual void FighterAwake()
	{
		o_TimeManager = GameObject.Find("PFGameManager").GetComponent<TimeManager>();
		o_ItemHandler = GameObject.Find("PFGameManager").GetComponent<ItemHandler>();
		o_ItemHandler = GameObject.Find("PFGameManager").GetComponent<ItemHandler>();
		o_NavMaster = GameObject.Find("NavMaster").GetComponent<NavMaster>();

		//v_TerrainType = new string[]{ "Concrete", "Concrete", "Concrete", "Concrete" };
		directionContacts = new RaycastHit2D[4];


		FighterState.CurHealth = 100;					// Current health.
		FighterState.Dead = false;						// True when the fighter's health reaches 0 and they die.
		Vector2 fighterOrigin = new Vector2(this.transform.position.x, this.transform.position.y);

		o_VelocityPunch = GetComponentInChildren<VelocityPunch>();
		o_SpriteTransform = transform.Find("SpriteTransform");
		o_DebugAngleDisplay = transform.Find("DebugAngleDisplay");
		o_DustSpawnTransform = transform.Find("SpriteTransform/DustEffectTransform");
		o_Anim = o_SpriteTransform.GetComponentInChildren<Animator>();
		o_SpriteRenderer = o_SpriteTransform.GetComponent<SpriteRenderer>();
		o_FighterAudio = this.GetComponent<FighterAudio>();
		o_Rigidbody2D = GetComponent<Rigidbody2D>();

		if(o_DustSpawnTransform.position==null)
		{
			print("dusttransform is the issue");
		}

		if(p_SparkEffectPrefab==null)
		{
			print("p_SparkEffectPrefab is the issue");
		}


		o_SparkThrower = (GameObject)Instantiate(p_SparkEffectPrefab, o_DustSpawnTransform.position, Quaternion.identity, this.transform);
		o_SparkThrower.GetComponent<ParticleSystem>().enableEmission = false;



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

		m_DebugLine = GetComponent<LineRenderer>();

		v_DefaultColor = o_SpriteRenderer.color;
		lastSafePosition = new Vector2(0,0);
		m_RemainingMovement = new Vector2(0,0);
		m_RemainingVelM = 1f;

		Shoe startingShoe = Instantiate(o_ItemHandler.shoes[p_DefaultShoeID], this.transform.position, Quaternion.identity).GetComponent<Shoe>();
		EquipShoe(startingShoe);


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

	public virtual void UnequipShoe()
	{
		if(o_EquippedShoe == null){return;}
		if(o_EquippedShoe.shoeID==0)
		{
			print("Cannot unequip feet!");
			o_EquippedShoe.DestroyThis();
			return;
		}
		o_EquippedShoe.Drop();
	}

	public virtual void EquipShoe(Shoe shoe) // Equip a shoe object. Set argument to null to equip barefoot.
	{		
		if(shoe==null)
		{
			shoe = Instantiate(o_ItemHandler.shoes[0], this.transform.position, Quaternion.identity).GetComponent<Shoe>();
		}

		UnequipShoe(); // Drop old shoes.

		///
		/// Movestat code
		///

		this.m_MinSpeed = shoe.m_MinSpeed;					
		this.m_MaxRunSpeed = shoe.m_MaxRunSpeed;				
		this.m_StartupAccelRate = shoe.m_StartupAccelRate;  			

		this.m_VJumpForce = shoe.m_VJumpForce;               
		this.m_HJumpForce = shoe.m_HJumpForce;  				
		this.m_WallVJumpForce = shoe.m_WallVJumpForce;           
		this.m_WallHJumpForce = shoe.m_WallHJumpForce;  			
		this.m_ZonJumpForcePerCharge = shoe.m_ZonJumpForcePerCharge; 	
		this.m_ZonJumpForceBase = shoe.m_ZonJumpForceBase; 		

		this.m_TractionChangeT = shoe.m_TractionChangeT;			
		this.m_WallTractionT = shoe.m_WallTractionT;			
		this.m_LinearStopRate = shoe.m_LinearStopRate; 			
		this.m_LinearSlideRate = shoe.m_LinearSlideRate;			
		this.m_LinearOverSpeedRate = shoe.m_LinearOverSpeedRate;		
		this.m_LinearAccelRate = shoe.m_LinearAccelRate;			
		this.m_ImpactDecelMinAngle = shoe.m_ImpactDecelMinAngle;
		this.m_ImpactDecelMaxAngle = shoe.m_ImpactDecelMaxAngle;
		this.m_TractionLossMinAngle = shoe.m_TractionLossMinAngle; 
		this.m_TractionLossMaxAngle = shoe.m_TractionLossMaxAngle;
		this.m_SlippingAcceleration = shoe.m_SlippingAcceleration;  	
		this.m_SurfaceClingTime = shoe.m_SurfaceClingTime;
		this.m_ClingReqGForce = shoe.m_ClingReqGForce;

		this.m_SlamT = shoe.m_SlamT;					
		this.m_CraterT = shoe.m_CraterT; 					
		this.m_GuardSlamT = shoe.m_GuardSlamT; 				
		this.m_GuardCraterT = shoe.m_GuardCraterT;				

		this.m_StrandJumpSpeedLossM = shoe.m_StrandJumpSpeedLossM;
		this.m_WidestStrandJumpAngle = shoe.m_WidestStrandJumpAngle;

		///
		/// Non movestat code
		///

		o_EquippedShoe = shoe;
		shoe.PickedUpBy(this);

		if(shoe.shoeID!=0)
		{
			o_FighterAudio.EquipSound();
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
		v_TriggerAtkHit = true;
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
		o_SpriteRenderer.color = v_DefaultColor;
	}

//	protected virtual void SpawnSparkEffect()
//	{
//		Vector3 spawnPos;
//		if(o_DustSpawnTransform)
//		{
//			spawnPos = o_DustSpawnTransform.position;
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
		if(o_DustSpawnTransform)
		{
			spawnPos = o_DustSpawnTransform.position;
		}
		else
		{
			spawnPos = m_GroundFoot.position;
		}
		Instantiate(p_DustEffectPrefab, spawnPos, Quaternion.identity);
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
		m_DistanceTravelled = Vector2.zero;

		initialVel = FighterState.Vel;
		v_WallSliding = false; // Set to false, and changed to true in WallTraction().

		if(m_Grounded)
		{//Locomotion!
			Traction(CtrlH, CtrlV);
			v_PrimarySurface = 0;
		}
		else if(m_LeftWalled)
		{//Wallsliding!
			//print("Walltraction!");
			WallTraction(CtrlH, CtrlV, m_LeftNormal);
			v_PrimarySurface = 2;
		}
		else if(m_RightWalled)
		{//Wallsliding!
			WallTraction(CtrlH, CtrlV, m_RightNormal);
			v_PrimarySurface = 3;
		}
//		else if(m_Ceilinged)
//		{
//			WallTraction(CtrlH, m_CeilingNormal);
//		}
		else if(!noGravity)
		{//Gravity!
			v_PrimarySurface = -1;
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

		if(g_FighterCollision && g_FighterCollisionCD <= 0)
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

		Vector2 deltaV = FighterState.Vel-initialVel;
		m_IGF = deltaV.magnitude;
		m_CGF += m_IGF;
		if(m_CGF>=1){m_CGF --;}
		if(m_CGF>=10){m_CGF -= (m_CGF/10);}

		if(m_WorldImpact)
		{
			float craterThreshold;
			float slamThreshold;
			float velPunchThreshold;
			AkSoundEngine.SetRTPCValue("GForce_Instant", m_IGF, this.gameObject);


			if(FighterState.Stance == 2) // If guarding, more resistant to landing damage.
			{
				craterThreshold = m_GuardCraterT;
				slamThreshold = m_GuardSlamT;
				velPunchThreshold = m_VelPunchT;
			}
			else
			{
				craterThreshold = m_CraterT;
				slamThreshold = m_SlamT;
				velPunchThreshold = m_VelPunchT;
			}

			if(m_IGF >= craterThreshold)
			{
				//Time.timeScale = 0.25f;
				float impactStrengthM = ((m_IGF-craterThreshold)/(1000f-craterThreshold));
				if(impactStrengthM > 1){impactStrengthM = 1;}

				Crater(m_IGF);

				float damagedealt = g_MinCrtrDMG+((g_MaxCrtrDMG-g_MinCrtrDMG)*impactStrengthM); // Damage dealt scales linearly from minDMG to maxDMG, reaching max damage at a 1000 kph impact.
				float stunTime = g_MinCrtrStun+((g_MaxCrtrStun-g_MinCrtrStun)*impactStrengthM); // Stun duration scales linearly from ...

				g_CurStun = stunTime;				 			// Stunned for stunTime.
				g_Stunned = true;
				TakeDamage((int)damagedealt);		// Damaged by fall.
				if(FighterState.CurHealth < 0){FighterState.CurHealth = 0;}
			}
			else if(m_IGF >= slamThreshold)
			{
				float impactStrengthM = ((m_IGF-slamThreshold)/(craterThreshold-slamThreshold)); // Linear scaling between slamThreshold and craterThreshold, value between 0 and 1.

				Slam(m_IGF);

				float damagedealt = g_MinSlamDMG+((g_MaxSlamDMG-g_MinSlamDMG)*impactStrengthM); // Damage dealt scales linearly from minDMG to maxDMG, as you go from the min Slam Threshold to min Crater Threshold (impact speed)
				float stunTime = g_MinSlamStun+((g_MaxSlamStun-g_MinSlamStun)*impactStrengthM); // Stun duration scales linearly from ...

				g_CurStun = stunTime;				 // Stunned for stunTime.
				g_Staggered = true;
				if(damagedealt >= 0)
				{
					TakeDamage((int)damagedealt);		 // Damaged by fall.
				}
				if(FighterState.CurHealth < 0){FighterState.CurHealth = 0;}

			}
			else if(FighterState.Stance == 1)
			{
				
				float impactStrengthM = ((m_IGF-velPunchThreshold)/(craterThreshold-velPunchThreshold));

				float damagedealt;
				g_CurStun = 0.1f;	// Stunned for stunTime.
				g_Staggered = true;

				if(m_IGF>=slamThreshold)
				{
					Slam(m_IGF);
					damagedealt = g_MinSlamDMG+((g_MaxSlamDMG-g_MinSlamDMG)*impactStrengthM); // Damage dealt scales linearly from minDMG to maxDMG, as you go from the min Slam Threshold to min Crater Threshold (impact speed)
				}
				else
				{
					o_FighterAudio.LandingSound(m_IGF);
					damagedealt = 0;
				}

				if(damagedealt >= 0)
				{
					TakeDamage((int)damagedealt);		 // Damaged by fall.
				}
				if(FighterState.CurHealth < 0){FighterState.CurHealth = 0;}
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
	
		//this.GetComponent<Rigidbody2D>().velocity = (Vector3)FighterState.Vel;

		if(FighterState.DevMode&&d_SendCollisionMessages)
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

	protected virtual void FixedUpdateProcessInput() //FUPI
	{
		m_WorldImpact = false;
		m_Kneeling = false;

		FighterState.PlayerMouseVector = FighterState.MouseWorldPos-Vec2(this.transform.position);
		if(FighterState.LeftClickPress&&(FighterState.DevMode||d_ClickToKnockFighter))
		{
			FighterState.Vel += FighterState.PlayerMouseVector*10;
			print("Leftclick detected");
			FighterState.LeftClickPress = false;
		}

		// Once the input has been processed, set the press inputs to false so they don't run several times before being changed by update() again. 
		// FixedUpdate can run multiple times before Update refreshes, so a keydown input can be registered as true multiple times before update changes it back to false, instead of just the intended one time.
		FighterState.LeftClickPress = false; 	
		FighterState.RightClickPress = false;
		FighterState.ZonKeyPress = false;
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
			
		if(FighterState.CurHealth <= 0)
		{
			if(!FighterState.Dead)
			{
				Death();
			}
			FighterState.Dead = true;
			o_Anim.SetBool("Dead", true);
			o_SpriteRenderer.color = Color.red;
		}
		else
		{
			FighterState.Dead = false;
			o_Anim.SetBool("Dead", false);
		}
	}

	protected virtual void UpdateAnimation()
	{
		o_SparkThrower.transform.position = o_DustSpawnTransform.position;
		if(v_Sliding)
		{
			if(v_DistFromLastDust<=0)
			{
				if(o_EquippedShoe.soundType==1)
				{
					//SpawnSparkEffect();
					o_SparkThrower.GetComponent<ParticleSystem>().enableEmission = true;

				}
				else
				{
					o_SparkThrower.GetComponent<ParticleSystem>().enableEmission = false;
					SpawnDustEffect();
				}

				v_DistFromLastDust = v_DistBetweenDust;
			}
			else
			{
				v_DistFromLastDust -= this.GetSpeed()*Time.deltaTime;
			}
		}
		else
		{
			o_SparkThrower.GetComponent<ParticleSystem>().enableEmission = false;
		}
	}

	protected virtual void FixedUpdateWwiseAudio() // FUWA
	{
		AkSoundEngine.SetRTPCValue("Health", FighterState.CurHealth, this.gameObject);
		AkSoundEngine.SetRTPCValue("Speed", FighterState.Vel.magnitude, this.gameObject);
		AkSoundEngine.SetRTPCValue("WindForce", FighterState.Vel.magnitude, this.gameObject);
		AkSoundEngine.SetRTPCValue("Velocity_X", FighterState.Vel.x, this.gameObject);
		AkSoundEngine.SetRTPCValue("Velocity_Y", FighterState.Vel.y, this.gameObject);
		AkSoundEngine.SetRTPCValue("GForce_Continuous", m_CGF, this.gameObject);


		//Bools
		AkSoundEngine.SetRTPCValue("Sliding", Convert.ToSingle(isSliding()), this.gameObject);
		AkSoundEngine.SetRTPCValue("Contact_Airborne", Convert.ToSingle(m_Airborne), this.gameObject);
		AkSoundEngine.SetRTPCValue("Contact_Ceiling", Convert.ToSingle(m_Ceilinged), this.gameObject);
		AkSoundEngine.SetRTPCValue("Contact_Ground", Convert.ToSingle(m_Grounded), this.gameObject);
		AkSoundEngine.SetRTPCValue("Contact_Leftwall", Convert.ToSingle(m_LeftWalled), this.gameObject);
		AkSoundEngine.SetRTPCValue("Contact_Rightwall", Convert.ToSingle(m_RightWalled), this.gameObject);

		//Switches
		if(g_IsInGrass>0)
		{
			AkSoundEngine.SetSwitch("TerrainType", "Grass", gameObject);
		}
	}

	protected virtual void FixedUpdateAnimation() //FUA
	{
		v_Sliding = false;
		o_Anim.SetBool("WallSlide", false);
		o_Anim.SetBool("Crouch", false);

		if(g_Staggered && !m_Airborne)
		{
			o_Anim.SetBool("Crouch", true);
		}
			
		if(m_Kneeling && !m_Airborne)
		{
			v_Sliding = true;
			o_Anim.SetBool("Crouch", true);
		}

		if(g_Stunned)
		{
			if(FighterState.Vel.x>0)
			{
				facingDirection = true;
			}
			if(FighterState.Vel.x<0)
			{
				facingDirection = false;
			}
		}
			
		v_FighterGlow = FighterState.ZonLevel;
		if (v_FighterGlow > 7){v_FighterGlow = 7;}

		if(v_FighterGlow>2)
		{
			o_SpriteRenderer.color = new Color(1,1,(1f-(v_FighterGlow/7f)),1);
		}
		else
		{
			o_SpriteRenderer.color = v_DefaultColor;
		}

		//
		//Sprite rotation code - SRC
		//
		float surfaceLeanM = GetSpeed()/v_SpeedForMaxLean; // Player leans more the faster they're going. At max speed, the player model rotates so the ground is directly below them.
		surfaceLeanM = (surfaceLeanM<1) ? surfaceLeanM : 1; // If greater than 1, clamp to 1.

		float spriteAngle;
		float testAngle = 0;

		if(v_PrimarySurface == 0)
		{
			spriteAngle = Get2DAngle(Perp(m_GroundNormal));
		}
		else if(v_PrimarySurface == 1)
		{
			spriteAngle = Get2DAngle(Perp(m_CeilingNormal));
		}
		else if(v_PrimarySurface == 2)
		{
			spriteAngle = Get2DAngle(Perp(m_LeftNormal));
			//if(v_WallSliding)
				surfaceLeanM = 1;
		}
		else if(v_PrimarySurface == 3)
		{
			spriteAngle = Get2DAngle(Perp(m_RightNormal));
			//if(v_WallSliding)
			surfaceLeanM = 1;
		}
		else
		{
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

//			angleScaling = GetVelocity().y/100;
//
//			angleScaling = (angleScaling>1) ? 1 : angleScaling;
//			angleScaling = (angleScaling<-1) ? -1 : angleScaling;

		//	angleScaling = GetVelocity().normalized.y;

			angleScaling = Mathf.Abs(GetVelocity().y/50);//*Mathf.Abs(GetVelocity().y/20); // Parabola approaching zero at y = 0, and extending to positive infinity on either side.

			angleScaling = (angleScaling>1) ? 1 : angleScaling; // Clamp at 1.

//			float horizontalScaling = (Mathf.Abs(GetVelocity().x)+50)/200;
//			if(GetVelocity().x<50)
//			{
//				horizontalScaling = Mathf.Abs(GetVelocity().x)/50;
//			}
//
//			horizontalScaling = (horizontalScaling>1) ? 1 : horizontalScaling; // Clamp at 1.
//
//			angleScaling *= horizontalScaling;
//			if(angleScaling<0)
//			{
//				spriteAngle = -spriteAngle
//			}
			//angleScaling = (angleScaling<0) ? angleScaling*0.5f : angleScaling;

//			if(GetVelocity().x>GetVelocity().y)
//			{
//				
//			}

			surfaceLeanM = angleScaling;

			if(Mathf.Abs(GetVelocity().x)>100)
			{
				surfaceLeanM = 1; 
				spriteAngle = Get2DAngle(GetVelocity(), 0);
			}

			//print("testAngle: "+testAngle+", finalangle: "+(surfaceLeanM*spriteAngle));
		}

		if(o_Anim.GetBool("Crouch"))
			surfaceLeanM = 1;

		//v_LeanAngle = Mathf.Lerp(v_LeanAngle, spriteAngle, Time.fixedDeltaTime*100);
		v_LeanAngle = spriteAngle*surfaceLeanM; //remove this and enable lerp.
		Quaternion finalAngle = new Quaternion();
		finalAngle.eulerAngles = new Vector3(0,0, v_LeanAngle);
		o_SpriteTransform.localRotation = finalAngle;
		//
		// End of sprite transform positioning code
		//

		float relativeAimDirection = -Get2DAngle((Vector2)FighterState.MouseWorldPos-(Vector2)this.transform.position, -v_LeanAngle);
		if(m_Kneeling&&!m_Airborne)
		{
			//print("RAD = "+relativeAimDirection+", LeanAngle = "+v_LeanAngle);
			if(this.IsPlayer())
			{
				if(relativeAimDirection < 0)
				{
					facingDirection = false;
				}
				else
				{
					facingDirection = true;
				}
			}
		}

		if(v_PrimarySurface==0)
			FixedGroundAnimation();
		else if(v_PrimarySurface==1)
		{} 
		else if(v_PrimarySurface>=2)
			FixedWallAnimation();
		else
			FixedAirAnimation();

		//
		// Debug collision visualization code.
		//
		if( isLocalPlayer )
		{
			if( FighterState.DevMode )
			{
				o_DebugAngleDisplay.gameObject.SetActive(true);
				Quaternion debugQuaternion = new Quaternion();
				debugQuaternion.eulerAngles = new Vector3(0, 0, testAngle);
				o_DebugAngleDisplay.localRotation = debugQuaternion;
			}
			else
			{
				o_DebugAngleDisplay.gameObject.SetActive(false);
			}
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

		//
		// End of debug line code
		//

		//
		// Mecanim variable assignment. Last step of animation code.
		//

		if(!facingDirection) //If facing left
		{
			o_Anim.SetBool("IsFacingRight", false);
			o_SpriteTransform.localScale = new Vector3(-1f, 1f, 1f);
		}
		else
		{
			o_Anim.SetBool("IsFacingRight", true);
			o_SpriteTransform.localScale = new Vector3(1f, 1f, 1f);
		}
			
		if(m_Kneeling)
		{
			o_Anim.SetFloat("AimAngle", relativeAimDirection);
		}
		else
		{
			float stoppingAngle = FighterState.Vel.magnitude*2;
			if(stoppingAngle>60)
				stoppingAngle = 60;
			if(stoppingAngle<30)
				stoppingAngle = 30;
			
			o_Anim.SetFloat("AimAngle", stoppingAngle);
		}

		o_Anim.SetInteger("PrimarySurface", v_PrimarySurface);
		o_Anim.SetFloat("Speed", FighterState.Vel.magnitude);
		o_Anim.SetFloat("hSpeed", Math.Abs(FighterState.Vel.x));
		o_Anim.SetFloat("vSpeed", Math.Abs(FighterState.Vel.y));
		o_Anim.SetFloat("hVelocity", FighterState.Vel.x);
		o_Anim.SetFloat("vVelocity", FighterState.Vel.y);
		o_Anim.SetInteger("Stance", FighterState.Stance);
		o_Anim.SetBool("Stunned", g_Stunned);
		o_Anim.SetBool("Staggered", g_Staggered);

		if(v_TriggerAtkHit)
		{
			v_TriggerAtkHit = false;
			o_Anim.SetBool("TriggerPunchHit", true);
		}
		if(v_TriggerRollOut)
		{
			v_TriggerRollOut = false;
			o_Anim.SetBool("TriggerRollOut", true);
		}

		float multiplier = 1; // Animation playspeed multiplier that increases with higher velocity

		if(FighterState.Vel.magnitude > 20.0f)
			multiplier = ((FighterState.Vel.magnitude - 20) / 20)+1;
		
		o_Anim.SetFloat("Multiplier", multiplier);

		//
		// Surface-material-type sound code
		//


		if(v_PrimarySurface != -1)
		{
			if(directionContacts[v_PrimarySurface])
			{
				String terrainType = "Concrete";
				RaycastHit2D surfaceHit = directionContacts[v_PrimarySurface];
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
		if (!facingDirection) //If facing left
		{
//			o_Anim.SetBool("IsFacingRight", false);
//			o_SpriteTransform.localScale = new Vector3 (-1f, 1f, 1f);
			if(FighterState.Vel.x > 0 && !m_Airborne) // && FighterState.Vel.magnitude >= v_ReversingSlideT 
			{
				o_Anim.SetBool("Crouch", true);
				v_Sliding = true;
			}
		} 
		else //If facing right
		{
//			o_Anim.SetBool("IsFacingRight", true);
//			o_SpriteTransform.localScale = new Vector3 (1f, 1f, 1f);
			if(FighterState.Vel.x < 0 && !m_Airborne) // && FighterState.Vel.magnitude >= v_ReversingSlideT 
			{
				o_Anim.SetBool("Crouch", true);
				v_Sliding = true;
			}
		}
	}

	protected virtual void FixedAirAnimation()
	{
		
	}

	protected virtual void FixedWallAnimation()
	{
		if(v_WallSliding)
		{
			o_Anim.SetBool("WallSlide", true);
			v_Sliding = true;
		}

		if (!facingDirection) //If facing left
		{
//			o_Anim.SetBool("IsFacingRight", false);
//			o_SpriteTransform.localScale = new Vector3 (-1f, 1f, 1f);
			if(v_PrimarySurface == 3 && FighterState.Vel.y > 0 && !m_Airborne) // If facing down and moving up, go into crouch stance.
			{
				//print("Running down on rightwall!");
				o_Anim.SetBool("Crouch", true);
				v_Sliding = true;
			}
		} 
		else //If facing right
		{
//			o_Anim.SetBool("IsFacingRight", true);
//			o_SpriteTransform.localScale = new Vector3 (1f, 1f, 1f);
			if(v_PrimarySurface == 2 && FighterState.Vel.y > 0 && !m_Airborne) // If facing down and moving up, go into crouch stance.
			{
				//print("Running down on leftwall!");
				o_Anim.SetBool("Crouch", true);
				v_Sliding = true;
			}
		}
			
	}

	protected virtual void UpdatePlayerInput()
	{
		if(Input.GetMouseButtonDown(0))
		{
			FighterState.LeftClickPress = true;
		}

		if(Input.GetMouseButtonDown(1))
		{
			//FighterState.RightClick = true;
		}

		//Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		//FighterState.MouseWorldPos = Vec2(mousePoint);
	}

	protected Vector2 Vec2(Vector3 inputVector)
	{
		return new Vector2(inputVector.x, inputVector.y);
	}

	protected void StrandJumpKinematic() //SJK
	{
		errorDetectingRecursionCount = 0; //Used for WorldCollizion(); (note: colliZion is used to help searches for the keyword 'collision' by filtering out extraneous matches)
		m_DistanceTravelled = Vector2.zero;
		m_RemainingMovement = FighterState.Vel*Time.fixedDeltaTime;
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
			InstantForce(m_StrandJumpReflectDir, m_StrandJumpReflectSpd);
		}

		if(g_FighterCollision)
		{
			DynamicCollision();
		}

		WorldCollision();

		//print("m_RemainingVelM: "+m_RemainingVelM);
		//print("movement after distance travelled: "+m_RemainingMovement);
		//print("Speed this frame: "+FighterState.Vel.magnitude);

		m_RemainingMovement = FighterState.Vel*m_RemainingVelM*Time.fixedDeltaTime;

		//print("Corrected remaining movement: "+m_RemainingMovement);

		this.transform.position = new Vector2(this.transform.position.x+m_RemainingMovement.x, this.transform.position.y+m_RemainingMovement.y);

		FighterState.FinalPos = this.transform.position;

		if(FighterState.DevMode&&d_SendCollisionMessages)
		{
			DebugUCN();
		}
	}

	public void Crater(float impactForce) // Triggered when character impacts anything REALLY hard.
	{
		float impactStrengthM = ((impactForce-m_CraterT)/(1000f-m_CraterT));
		if(impactStrengthM > 1){impactStrengthM = 1;}
		float camShakeM = (impactForce+m_CraterT)/(2*m_CraterT);

		if(camShakeM >=2){camShakeM = 2;}
		float Magnitude = camShakeM;
		float Roughness = 10f;
		float FadeOutTime = 2.5f;
		float FadeInTime = 0f;
		Vector3 RotInfluence = new Vector3(1,1,1);
		Vector3 PosInfluence = new Vector3(0.15f,0.15f,0.15f);
		CameraShaker.Instance.ShakeOnce(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);

		AkSoundEngine.SetRTPCValue("GForce_Instant", m_IGF, this.gameObject);
		o_FighterAudio.CraterSound(impactForce, m_CraterT, 1000f);

		SpawnShockEffect(this.initialVel.normalized);

//		GameObject newAirBurst = (GameObject)Instantiate(p_AirBurstPrefab, this.transform.position, Quaternion.identity);
		//		newAirBurst.GetComponentInChildren<AirBurst>().Create(true, 30+70*linScaleModifier, 0.4f, impactForce); 					//Set the parameters of the shockwave.
//		newAirBurst.name = "Shockwave";
		GameObject newWindGust = (GameObject)Instantiate(p_AirBurstPrefab, this.transform.position, Quaternion.identity);
		newWindGust.GetComponentInChildren<AirBurst>().Create(false, 0, 30+70*impactStrengthM, 0.8f, impactStrengthM*3, impactForce); 		//Set the parameters of the afterslam wind.
		newWindGust.name = "AirGust";
	}

	public void Slam(float impactForce) // Triggered when character impacts anything too hard.
	{
		float impactStrengthM = ((impactForce-m_SlamT)/(m_CraterT-m_SlamT));
		float camShakeM = (impactForce+m_SlamT)/(2*m_SlamT);
		if(camShakeM >=2){camShakeM = 2;}
		float Magnitude = 0.5f;
		float Roughness = 20f;
		float FadeOutTime = 0.6f*camShakeM;
		float FadeInTime = 0f;
		float posM = 0.3f*camShakeM;
		Vector3 RotInfluence = new Vector3(0,0,0);
		Vector3 PosInfluence = new Vector3(posM,posM,0);
		CameraShaker.Instance.ShakeOnce(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);

		AkSoundEngine.SetRTPCValue("GForce_Instant", m_IGF, this.gameObject);
		o_FighterAudio.SlamSound(impactForce, m_SlamT, m_CraterT);

		if(g_VelocityPunching)
		{
			SpawnShockEffect(this.initialVel.normalized);
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
							facingDirection = (hit.collider.transform.position.x-this.transform.position.x < 0) ? false : true; // If enemy is to your left, face left. Otherwise, right.
							hit.collider.GetComponent<FighterChar>().facingDirection = !facingDirection;
						}
					}
				}
			}
		}
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
						if(d_SendCollisionMessages)
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
					if(invertedDirectionNormal == predictedLoc[0].normal&&d_SendCollisionMessages)
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
					if(invertedDirectionNormal == predictedLoc[1].normal&&d_SendCollisionMessages)
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
					if(invertedDirectionNormal == predictedLoc[2].normal&&d_SendCollisionMessages)
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

	protected virtual void ZonPulse()
	{
		if(FighterState.ZonLevel <= 0)
		{
			return;
		}

		FighterState.ZonLevel--;
		//o_ProximityLiner.ClearAllFighters();
		GameObject newZonPulse = (GameObject)Instantiate(p_ZonPulse, this.transform.position, Quaternion.identity);
		newZonPulse.GetComponentInChildren<ZonPulse>().originFighter = this;
		newZonPulse.GetComponentInChildren<ZonPulse>().pulseRange = 150+(FighterState.ZonLevel*50);
		//o_ProximityLiner.outerRange = 100+(FighterState.ZonLevel*25);
		o_FighterAudio.ZonPulseSound();
	}

	protected float GetSteepness(Vector2 vectorPara)
	{
		return 0f;
	}

	protected void Traction(float horizontalInput, float inputV)
	{
		Vector2 groundPara = Perp(m_GroundNormal);
		if(d_SendTractionMessages){print("Traction");}


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
				Vector2 wallPara = Perp (m_LeftNormal);
				wallSteepnessAngle = Vector2.Angle (Vector2.up, wallPara);
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
				Vector2 wallPara = Perp(m_RightNormal);
				wallSteepnessAngle = Vector2.Angle (Vector2.up, wallPara);
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

		if(inputV<0)
		{
			m_Kneeling = true;
			horizontalInput = 0;
			v_CameraMode = 2;
		}
		else
		{
			v_CameraMode = v_DefaultCameraMode;
		}

		if(groundPara.x > 0)
		{
			groundPara *= -1;
		}

		if(d_SendTractionMessages){print("gp="+groundPara);}

		float steepnessAngle = Vector2.Angle(Vector2.left,groundPara);

		steepnessAngle = (float)Math.Round(steepnessAngle,2);
		if(d_SendTractionMessages){print("SteepnessAngle:"+steepnessAngle);}

		float slopeMultiplier = 0;

		if(steepnessAngle > m_TractionLossMinAngle)
		{
			if(steepnessAngle >= m_TractionLossMaxAngle)
			{
				if(d_SendTractionMessages){print("MAXED OUT!");}
				slopeMultiplier = 1;
			}
			else
			{
				slopeMultiplier = ((steepnessAngle-m_TractionLossMinAngle)/(m_TractionLossMaxAngle-m_TractionLossMinAngle));
			}

			if(d_SendTractionMessages){print("slopeMultiplier: "+slopeMultiplier);}
			//print("groundParaY: "+groundParaY+", slopeT: "+slopeT);
		}


		if(((m_LeftWallBlocked)&&(horizontalInput < 0)) || ((m_RightWallBlocked)&&(horizontalInput > 0)))
		{// If running at an obstruction you're up against.
			//print("Running against a wall.");
			horizontalInput = 0;
		}

		//print("Traction executing");
		float rawSpeed = FighterState.Vel.magnitude;
		if(d_SendTractionMessages){print("FighterState.Vel.magnitude: "+FighterState.Vel.magnitude);}

		if (horizontalInput == 0) 
		{//if not pressing any move direction, slow to zero linearly.
			if(d_SendTractionMessages){print("No input, slowing...");}
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
			if(d_SendTractionMessages){print("Moving with keypress");}
			if(rawSpeed < m_MaxRunSpeed)
			{
				if(d_SendTractionMessages){print("Rawspeed("+rawSpeed+") less than max");}
				if(rawSpeed > m_TractionChangeT)
				{
					if(d_SendTractionMessages){print("LinAccel-> " + rawSpeed);}
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
//					if(slopeMultiplier<0.5)
//					{
//						FighterState.Vel = new Vector2((m_Acceleration)*horizontalInput*(1-slopeMultiplier), 0);
//					}
//					else
//					{
//						if(d_SendTractionMessages){print("Too steep!");}
//					}
//					if(d_SendTractionMessages){print("Starting motion. Adding " + m_Acceleration);}
					if(d_SendTractionMessages){print("HardAccel-> " + rawSpeed);}
					if(FighterState.Vel.y > 0)
					{ 	// If climbing, recieve uphill movement penalty.
						FighterState.Vel = new Vector2(m_StartupAccelRate*(1-slopeMultiplier)*horizontalInput, 0);
					}
					else
					{
						FighterState.Vel = new Vector2(m_StartupAccelRate*horizontalInput, 0);
					}
				}
				else
				{
//					//print("ExpAccel-> " + rawSpeed);
//					float eqnX = (1+Mathf.Abs((1/m_TractionChangeT )*rawSpeed));
//					float curveMultiplier = 1+(1/(eqnX*eqnX)); // Goes from 1/4 to 1, increasing as speed approaches 0.
//
//					float addedSpeed = curveMultiplier*(m_Acceleration);
//					if(FighterState.Vel.y > 0)
//					{ // If climbing, recieve uphill movement penalty.
//						addedSpeed = curveMultiplier*(m_Acceleration)*(1-slopeMultiplier);
//					}
//					if(d_SendTractionMessages){print("Addedspeed:"+addedSpeed);}
//					FighterState.Vel = (FighterState.Vel.normalized)*(rawSpeed+addedSpeed);
//					if(d_SendTractionMessages){print("FighterState.Vel:"+FighterState.Vel);}
					if(d_SendTractionMessages){print("HardAccel-> " + rawSpeed);}
					if(FighterState.Vel.y > 0)
					{ 	// If climbing, recieve uphill movement penalty.
						FighterState.Vel = ChangeSpeedLinear(FighterState.Vel, m_StartupAccelRate*(1-slopeMultiplier));
					}
					else
					{
						FighterState.Vel = ChangeSpeedLinear(FighterState.Vel, m_StartupAccelRate);
					}
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
					if(d_SendTractionMessages){print("Rawspeed("+rawSpeed+") more than max.");}
					FighterState.Vel = ChangeSpeedLinear (FighterState.Vel, -m_LinearOverSpeedRate);
				}
			}
		}
		else if((horizontalInput > 0 && FighterState.Vel.x < 0) || (horizontalInput < 0 && FighterState.Vel.x > 0))
		{//if pressing button opposite of move direction, slow to zero quickly.
			if(d_SendTractionMessages){print("LinDecel");}
			FighterState.Vel = ChangeSpeedLinear (FighterState.Vel, -m_LinearStopRate);

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
		if(d_SendTractionMessages){print("downSlope="+downSlope);}
		if(d_SendTractionMessages){print("m_SlippingAcceleration="+m_SlippingAcceleration);}
		if(d_SendTractionMessages){print("slopeMultiplier="+slopeMultiplier);}

		//ChangeSpeedLinear(FighterState.Vel, );
		if(d_SendTractionMessages){print("PostTraction velocity: "+FighterState.Vel);}
	}

	protected void AirControl(float horizontalInput)
	{
		FighterState.Vel += new Vector2(horizontalInput/20, 0);
	}

//	protected void WallTraction(float hInput, float vInput, Vector2 wallSurface)
//	{
//		if(m_LeftWalled && !m_RightWalled) // If pressing input away from wall, detach from it.
//		{
//			if(hInput>0)
//			{
//				//print("FALLIN OFF YO!");
//				AirControl(hInput);
//				v_CameraMode = v_DefaultCameraMode;
//				return;
//			}
//			else if(hInput<0)
//			{
//				m_Kneeling = true;
//				vInput = 0;
//				v_CameraMode = 2;
//			}
//			else
//			{
//				v_CameraMode = v_DefaultCameraMode;
//			}
//		}
//		else if(m_RightWalled && !m_LeftWalled)  // If pressing input away from wall, detach from it.
//		{
//			if(hInput<0)
//			{
//				//print("FALLIN OFF YO!");
//				AirControl(hInput);
//				v_CameraMode = v_DefaultCameraMode;
//				return;
//			}
//			else if(hInput>0)
//			{
//				m_Kneeling = true;
//				vInput = 0;
//				v_CameraMode = 2;
//			}
//			else
//			{
//				v_CameraMode = v_DefaultCameraMode;
//			}
//
//		}
//
//		////////////////////
//		// Variable Setup //
//		////////////////////
//		Vector2 wallPara = Perp(wallSurface);
//
//		//print("hInput="+hInput);
//
//
//		if(wallPara.x > 0)
//		{
//			wallPara *= -1;
//		}
//
//		float steepnessAngle = Vector2.Angle	(Vector2.up,wallPara);
//
//		if(m_RightWalled)
//		{
//			steepnessAngle = 180f - steepnessAngle;
//		}
//
//		if(steepnessAngle == 180)
//		{
//			steepnessAngle=0;
//		}
//
//		if(steepnessAngle > 90 && (wallSurface != m_ExpiredNormal)) //If the sliding surface is upside down, and hasn't already been clung to.
//		{
//			if(!m_SurfaceCling)
//			{
//				m_TimeSpentHanging = 0;
//				m_MaxTimeHanging = 0;
//				m_SurfaceCling = true;
//				if(m_CGF >= m_ClingReqGForce)
//				{
//					m_MaxTimeHanging = m_SurfaceClingTime;
//				}
//				else
//				{
//					m_MaxTimeHanging = m_SurfaceClingTime*(m_CGF/m_ClingReqGForce);
//				}
//				//print("m_MaxTimeHanging="+m_MaxTimeHanging);
//			}
//			else
//			{
//				m_TimeSpentHanging += Time.fixedDeltaTime;
//				//print("time=("+m_TimeSpentHanging+"/"+m_MaxTimeHanging+")");
//				if(m_TimeSpentHanging>=m_MaxTimeHanging)
//				{
//					m_SurfaceCling = false;
//					m_ExpiredNormal = wallSurface;
//					//print("EXPIRED!");
//				}
//			}
//		}
//		else
//		{
//			m_SurfaceCling = false;
//			m_TimeSpentHanging = 0;
//			m_MaxTimeHanging = 0;
//		}
//
//
//		//
//		// This code block is likely unnecessary
//		// Anti-Jitter code for transitioning to a steep slope that is too steep to climb.
//		//
////		if (this.GetSpeed () <= 0.0001f) 
////		{
////			print ("RIDING WALL SLOWLY, CONSIDERING CORRECTION");
////			if ((m_LeftWalled) && (hInput < 0)) 
////			{
////				if (steepnessAngle >= m_TractionLossMaxAngle) { //If the wall surface the player is running
////					print ("Wall steepness of " + steepnessAngle + " was too steep for speed " + this.GetSpeed () + ", stopping.");
////					//FighterState.Vel = Vector2.zero;
////					m_LeftWallBlocked = true;
////					hInput = 0;
////					m_SurfaceCling = false;
////				}
////			} 
////			else if ((m_RightWalled) && (hInput > 0)) 
////			{
////				print ("Trying to run up right wall slowly.");
////				if (steepnessAngle >= m_TractionLossMaxAngle) { //If the wall surface the player is running
////					print ("Wall steepness of " + steepnessAngle + " was too steep for speed " + this.GetSpeed () + ", stopping.");
////					//FighterState.Vel = Vector2.zero;
////					m_RightWallBlocked = true;
////					hInput = 0;
////					m_SurfaceCling = false;
////				}
////			} 
////			else 
////			{
////				print ("Not trying to move up a wall; Continue as normal.");
////			}
////		}
//
//
//		//print("Wall Steepness Angle:"+steepnessAngle);
//
//		///////////////////
//		// Movement code //
//		///////////////////
//
//		if(m_SurfaceCling)
//		{
//			if(FighterState.Vel.y > 0)
//			{
//				FighterState.Vel = ChangeSpeedLinear(FighterState.Vel,-0.8f);
//			}
//			else if(FighterState.Vel.y <= 0)
//			{
//				if( (hInput<0 && m_LeftWalled) || (hInput>0 && m_RightWalled) )
//				{
//					FighterState.Vel = ChangeSpeedLinear(FighterState.Vel,0.1f);
//				}
//				else
//				{
//					FighterState.Vel = ChangeSpeedLinear(FighterState.Vel,1f);
//				}
//			}
//		}
//		else
//		{
//			if(FighterState.Vel.y>0)
//			{		
//				if(m_LeftWalled)
//					facingDirection = false;
//				if(m_RightWalled)
//					facingDirection = true;
//				
//				if(vInput>0) // If pressing key upward.
//				{
//					FighterState.Vel.y -= 0.8f; //Decelerate slower.
//				}
//				else if(vInput<0) // If pressing key downward
//				{
//					if(m_LeftWalled)
//						facingDirection = true;
//					if(m_RightWalled)
//						facingDirection = false;
//					FighterState.Vel.y -= 1.2f; //Decelerate faster.
//				}
//				else // If no input.
//				{
//					FighterState.Vel.y -= 1f; 	//Decelerate.
//				}
//			}
//			else if(FighterState.Vel.y<=0)
//			{
//				if(m_LeftWalled)
//					facingDirection = true;
//				if(m_RightWalled)
//					facingDirection = false;
//				
//				if((hInput<0 && m_LeftWalled) || (hInput>0 && m_RightWalled) || vInput>0) // If pressing up or against wall.
//				{
//					FighterState.Vel.y -= 0.1f; //Wallslide
//					m_WallSliding = true;
//				}
//				else if(vInput<0) // If pressing down
//				{
//					FighterState.Vel.y -= 1.2f; //Accelerate downward faster.
//				}
//				else // If no input.
//				{
//					FighterState.Vel.y -= 1f; 	//Accelerate downward.
//					m_WallSliding = true;
//				}
//			}
//		}
//	}
//

	protected void WallTraction(float hInput, float vInput, Vector2 wallSurface)
	{
		v_CameraMode = v_DefaultCameraMode;

		if(vInput>0)
		{
			//print("FALLIN OFF YO!");
			AirControl(vInput);
			return;
		}
		else if(vInput<0)
		{
			m_Kneeling = true;
			hInput = 0;
			v_CameraMode = 2;
		}

		if(m_LeftWalled) 	// If going up the left side wall, reverse horizontal input. This makes it so when control scheme is rotated 90 degrees, the key facing the wall will face up always. 
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
		//		if (this.GetSpeed () <= 0.0001f) 
		//		{
		//			print ("RIDING WALL SLOWLY, CONSIDERING CORRECTION");
		//			if ((m_LeftWalled) && (hInput < 0)) 
		//			{
		//				if (steepnessAngle >= m_TractionLossMaxAngle) { //If the wall surface the player is running
		//					print ("Wall steepness of " + steepnessAngle + " was too steep for speed " + this.GetSpeed () + ", stopping.");
		//					//FighterState.Vel = Vector2.zero;
		//					m_LeftWallBlocked = true;
		//					hInput = 0;
		//					m_SurfaceCling = false;
		//				}
		//			} 
		//			else if ((m_RightWalled) && (hInput > 0)) 
		//			{
		//				print ("Trying to run up right wall slowly.");
		//				if (steepnessAngle >= m_TractionLossMaxAngle) { //If the wall surface the player is running
		//					print ("Wall steepness of " + steepnessAngle + " was too steep for speed " + this.GetSpeed () + ", stopping.");
		//					//FighterState.Vel = Vector2.zero;
		//					m_RightWallBlocked = true;
		//					hInput = 0;
		//					m_SurfaceCling = false;
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

		if(m_SurfaceCling)
		{
			//print("SURFACECLING!");
			if(FighterState.Vel.y > 0)
			{
				FighterState.Vel = ChangeSpeedLinear(FighterState.Vel,-0.8f);
			}
			else if(FighterState.Vel.y <= 0)
			{
				if( (hInput<0 && m_LeftWalled) || (hInput>0 && m_RightWalled) )
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
				if(m_LeftWalled)
					facingDirection = false;
				if(m_RightWalled)
					facingDirection = true;

				if(hInput>0) 				// ...and pressing key upward...
				{
					FighterState.Vel.y -= 0.8f; // ... then decelerate slower.
				}
				else if(hInput<0) 			// ...and pressing key downward...
				{
					FighterState.Vel.y -= 1.2f; // ...decelerate quickly.
					if(m_LeftWalled)
						facingDirection = true;
					if(m_RightWalled)
						facingDirection = false;
				}
				else 						// ...and pressing nothing...
				{
					FighterState.Vel.y -= 1f; 	// ...decelerate.
				}
			}
			else if(FighterState.Vel.y<=0) // If descending...
			{
				if(m_LeftWalled)
					facingDirection = true;
				if(m_RightWalled)
					facingDirection = false;

				if(hInput>0) 					// ...and pressing key upward...
				{
					FighterState.Vel.y -= 0.1f; 	// ...then wallslide.
					v_WallSliding = true;
				}
				else if(hInput<0) 				// ...and pressing key downward...
				{
					FighterState.Vel.y -= 1.2f; 	// ...accelerate downward quickly.
				}
				else 							// ...and pressing nothing...
				{
					FighterState.Vel.y -= 1f; 		// ...accelerate downward.
					v_WallSliding = true;
				}
			}
		}
	}


	protected bool ToLeftWall(RaycastHit2D leftCheck) 
	{ //Sets the new position of the fighter and their m_LeftNormal.

		if(d_SendCollisionMessages){print("We've hit LeftWall, sir!!");}

		if (m_Airborne)
		{
			if(d_SendCollisionMessages){print("Airborne before impact.");}
			m_WorldImpact = true;
		}

		Breakable hitBreakable = leftCheck.collider.transform.GetComponent<Breakable>();

		if(hitBreakable!=null&&FighterState.Vel.magnitude > 3)
		{
			if(d_SendCollisionMessages){print("hit a hitbreakable!");}
			if(hitBreakable.RecieveHit(this)){return false;}
		}
		//leftSideContact = true;
		m_LeftWalled = true;

		if(!m_Grounded&&!m_Ceilinged)
		{
			v_PrimarySurface = 2;
		}

		Vector2 setCharPos = leftCheck.point;
		setCharPos.x += (m_LeftSideLength-m_MinEmbed); //Embed slightly in wall to ensure raycasts still hit wall.
		//setCharPos.y -= m_MinEmbed;
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

		if(d_SendCollisionMessages){print("We've hit RightWall, sir!!");}
		//print ("groundCheck.normal=" + groundCheck.normal);
		//print("prerightwall Pos:" + this.transform.position);

		if (m_Airborne)
		{
			if(d_SendCollisionMessages){print("Airborne before impact.");}
			m_WorldImpact = true;
		}

		Breakable hitBreakable = rightCheck.collider.transform.GetComponent<Breakable>();

		if(hitBreakable!=null&&FighterState.Vel.magnitude > 3)
		{
			print("hit a hitbreakable!");
			if(hitBreakable.RecieveHit(this)){return false;}
		}
		rightSideContact = true;
		m_RightWalled = true;

		if(!m_Grounded&&!m_Ceilinged)
			v_PrimarySurface = 3;

		Vector2 setCharPos = rightCheck.point;
		setCharPos.x -= (m_RightSideLength-m_MinEmbed); //Embed slightly in wall to ensure raycasts still hit wall.

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

//		if(m_Grounded)
//		{
//			//print("RightGroundWedge detected during right collision.");
//			OmniWedge(0,3);
//		}
//
//		if(m_LeftWalled)
//		{
//			print("THERE'S PROBLEMS.");
//			//OmniWedge(2,3);
//		}
//
//		if(m_Ceilinged)
//		{
//			//print("RightCeilingWedge detected during right collision.");
//			OmniWedge(3,1);
//		}
		return true;
	}

	protected bool ToGround(RaycastHit2D groundCheck) 
	{ //Sets the new position of the fighter and their ground normal.
		//print ("m_Grounded=" + m_Grounded);

		if (m_Airborne)
		{
			m_WorldImpact = true;
		}

		Breakable hitBreakable = groundCheck.collider.transform.GetComponent<Breakable>();

		if(hitBreakable!=null&&this.GetSpeed() > 3)
		{
			print("hit a hitbreakable!");
			if(hitBreakable.RecieveHit(this)){return false;}
		}
			
		m_Grounded = true;
		v_PrimarySurface = 0;

		Vector2 setCharPos = groundCheck.point;
		setCharPos.y = setCharPos.y+m_GroundFootLength-m_MinEmbed; //Embed slightly in ground to ensure raycasts still hit ground.
		this.transform.position = setCharPos;

		//print("Sent to Pos:" + setCharPos);
		//print("Sent to normal:" + groundCheck.normal);

		RaycastHit2D groundCheck2 = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength, m_TerrainMask);

		if(groundCheck.normal.y == 0f)
		{//If vertical surface
			//throw new Exception("Existence is suffering");
			if(d_SendCollisionMessages)
			{
				//print("GtG VERTICAL :O");
			}
		}

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

		if((GetSteepness(groundCheck2.normal)>=((m_TractionLossMaxAngle+m_TractionLossMinAngle)/2)) && this.GetSpeed()<=0.001f) 
		{ //If going slow and hitting a steep slope, don't move to the new surface, and treat the new surface as a wall on that side.
			if(this.GetVelocity().x>0)
			{
				print("Positive slope ground acting as right wall due to steepness.");
				m_RightWallBlocked = true;
			}
			else
			{
				print("Negative slope ground acting as left wall due to steepness.");
				m_LeftWallBlocked = true;
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

		if (m_Airborne)
		{
			m_WorldImpact = true;
		}


		Breakable hitBreakable = ceilingCheck.collider.transform.GetComponent<Breakable>();

		if(hitBreakable!=null&&FighterState.Vel.magnitude > 3)
		{
			print("hit a hitbreakable!");
			if(hitBreakable.RecieveHit(this)){return false;}
		}

		//m_Impact = true;
		m_Ceilinged = true;
		Vector2 setCharPos = ceilingCheck.point;
		setCharPos.y -= (m_GroundFootLength-m_MinEmbed); //Embed slightly in ceiling to ensure raycasts still hit ceiling.
		this.transform.position = setCharPos;

		RaycastHit2D ceilingCheck2 = Physics2D.Raycast(this.transform.position, Vector2.up, m_GroundFootLength, m_TerrainMask);
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

//		if(m_Grounded)
//		{
//			if(d_SendCollisionMessages)
//			{
//				print("CeilGroundWedge detected during ceiling collision.");
//			}
//			OmniWedge(0,1);
//		}
//
//		if(m_LeftWalled)
//		{
//			if(d_SendCollisionMessages)
//			{
//				print("LeftCeilWedge detected during ceiling collision.");
//			}
//			OmniWedge(2,1);
//		}
//
//		if(m_RightWalled)
//		{
//			if(d_SendCollisionMessages)
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
		v_TriggerAtkHit = true;
		o_FighterAudio.PunchHitSound();
		if((this.IsPlayer() || opponent.IsPlayer())&&(impactDamageM>v_PunchStrengthSlowmoT))
		{
			o_TimeManager.TimeDilation(0.1f, 0.75f+0.75f*impactDamageM);
		}
		if(combinedSpeed >= m_CraterT)
		{
			//opponent.Crater(combinedSpeed);
			Crater(combinedSpeed);
		}
		else if(combinedSpeed >= m_VelPunchT)
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
		if((this.IsPlayer() || opponent.IsPlayer())&&(impactDamageM>v_PunchStrengthSlowmoT))
		{
			o_TimeManager.TimeDilation(0.1f, 0.75f+0.75f*impactDamageM);
		}
		if(combinedSpeed >= m_CraterT)
		{
			print("Fighter crater successful");
			Crater(combinedSpeed);
		}
		else if(combinedSpeed >= m_SlamT)
		{
			print("Fighter slam successful");
			Slam(combinedSpeed);
		}
		v_TriggerAtkHit = true;
		o_FighterAudio.PunchHitSound();

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
		opponent.v_TriggerAtkHit = true;
		opponent.o_FighterAudio.PunchHitSound();

		v_TriggerAtkHit = true;
		o_FighterAudio.PunchHitSound();


		if((this.IsPlayer() || opponent.IsPlayer())&&(impactDamageM>v_PunchStrengthSlowmoT))
		{
			o_TimeManager.TimeDilation(0.1f, 0.75f+0.75f*impactDamageM);
		}

		if(combinedSpeed >= m_CraterT)
		{
			//print("Fighter crater successful");
			opponent.Crater(combinedSpeed);
			Crater(combinedSpeed);
		}
		else if(combinedSpeed >= m_SlamT)
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
		m_ExpiredNormal = new Vector2(0,0); //Used for wallslides. This resets the surface normal that wallcling is set to ignore.

		Vector2 initialDirection = FighterState.Vel.normalized;
		Vector2 newPara = Perp(newNormal);
		Vector2 AdjustedVel;

		float initialSpeed = FighterState.Vel.magnitude;
		float testNumber = newPara.y/newPara.x;

		if(float.IsNaN(testNumber))
		{
			if(d_SendCollisionMessages){print("NaN value found on DirectionChange");}
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

		if(impactAngle <= m_ImpactDecelMinAngle)
		{ // Angle lower than min, no speed penalty.
			speedRetentionMult = 1;
		}
		else if(impactAngle < m_ImpactDecelMaxAngle)
		{ // In the midrange, administering momentum loss on a curve leading from min to max.
			speedRetentionMult = 1-Mathf.Pow((impactAngle-m_ImpactDecelMinAngle)/(m_ImpactDecelMaxAngle-m_ImpactDecelMinAngle),2); // See Workflowy notes section for details on this formula.
		}
		else
		{ // Angle beyond max, momentum halted. 
			speedRetentionMult = 0;
			m_WorldImpact = true;
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

		if(d_SendCollisionMessages){print("OmniWedge("+lowerContact+","+upperContact+")");}

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
				if(d_SendCollisionMessages){print("Omniwedge: lowercontact is left");}
				lowerDirection = Vector2.left;
				lowerLength = m_LeftSideLength;
				break;
			}
		case 3: //lowercontact is right
			{
				if(d_SendCollisionMessages){print("Omniwedge: lowercontact is right");}
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
			if(d_SendCollisionMessages){print("Bottom not wedged!");}
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
				if(d_SendCollisionMessages){print("Omniwedge: uppercontact is left");}
				upperDirection = Vector2.left;
				upperLength = m_LeftSideLength;
				break;
			}
		case 3: //uppercontact is right
			{
				if(d_SendCollisionMessages){print("Omniwedge: uppercontact is right");}
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
				if(d_SendCollisionMessages){print("Left wedge!");}
				correctionVector = SuperUnwedger(cPara, gPara, true, embedDepth);
				if(d_SendCollisionMessages){print("correctionVector:"+correctionVector);}
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
			if(d_SendCollisionMessages){print("Obtuse wedge angle detected!");}
			correctionVector = (upperDirection*(-(embedDepth-m_MinEmbed)));
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
		m_Grounded = false;
		m_Ceilinged = false;
		m_LeftWalled = false;
		m_RightWalled = false;
		m_Airborne = false;

		if(m_JumpBufferG>0){m_JumpBufferG--;}
		if(m_JumpBufferC>0){m_JumpBufferC--;}
		if(m_JumpBufferL>0){m_JumpBufferL--;}
		if(m_JumpBufferR>0){m_JumpBufferR--;}

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

		directionContacts[0] = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength, m_TerrainMask); 	// Ground
		directionContacts[1] = Physics2D.Raycast(this.transform.position, Vector2.up, m_CeilingFootLength, m_TerrainMask);  	// Ceiling
		directionContacts[2] = Physics2D.Raycast(this.transform.position, Vector2.left, m_LeftSideLength, m_TerrainMask); 	// Left
		directionContacts[3] = Physics2D.Raycast(this.transform.position, Vector2.right, m_RightSideLength, m_TerrainMask);	// Right  

		if (directionContacts[0]) 
		{
			m_GroundNormal = directionContacts[0].normal;
			groundContact = true;
			m_GroundLine.endColor = Color.green;
			m_GroundLine.startColor = Color.green;
			m_Grounded = true;
			m_JumpBufferG = m_JumpBufferFrameAmount;
			if(Mathf.Abs(m_GroundNormal.x)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
				m_GroundNormal.x = 0;
			if(Mathf.Abs(m_GroundNormal.y)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
				m_GroundNormal.y = 0;
		} 

		if (directionContacts[1]) 
		{
			m_CeilingNormal = directionContacts[1].normal;
			ceilingContact = true;
			m_CeilingLine.endColor = Color.green;
			m_CeilingLine.startColor = Color.green;
			m_Ceilinged = true;
			m_JumpBufferC = m_JumpBufferFrameAmount;
			if(Mathf.Abs(m_CeilingNormal.x)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
				m_CeilingNormal.x = 0;
			if(Mathf.Abs(m_CeilingNormal.y)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
				m_CeilingNormal.y = 0;
		} 


		if (directionContacts[2])
		{
			m_LeftNormal = directionContacts[2].normal;
			leftSideContact = true;
			m_LeftSideLine.endColor = Color.green;
			m_LeftSideLine.startColor = Color.green;
			m_LeftWalled = true;
			m_JumpBufferL = m_JumpBufferFrameAmount;
			if(Mathf.Abs(m_LeftNormal.x)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
				m_LeftNormal.x = 0;
			if(Mathf.Abs(m_LeftNormal.y)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
				m_LeftNormal.y = 0;

		} 

		if (directionContacts[3])
		{
			m_RightNormal = directionContacts[3].normal;
			rightSideContact = true;
			m_RightSideLine.endColor = Color.green;
			m_RightSideLine.startColor = Color.green;
			m_RightWalled = true;
			m_JumpBufferR = m_JumpBufferFrameAmount;
			if(Mathf.Abs(m_RightNormal.x)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
				m_RightNormal.x = 0;
			if(Mathf.Abs(m_RightNormal.y)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
				m_RightNormal.y = 0;
		} 

		if(!(m_Grounded&&m_Ceilinged)) //Resets wall blocker flags if the player isn't touching a blocking surface.
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
			m_SurfaceCling = false;
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
		directionContacts[0] = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength, m_TerrainMask); 	// Ground
		directionContacts[1] = Physics2D.Raycast(this.transform.position, Vector2.up, m_CeilingFootLength, m_TerrainMask);  	// Ceiling
		directionContacts[2] = Physics2D.Raycast(this.transform.position, Vector2.left, m_LeftSideLength, m_TerrainMask); 	// Left
		directionContacts[3] = Physics2D.Raycast(this.transform.position, Vector2.right, m_RightSideLength, m_TerrainMask);	// Right  

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

		int contactCount = 0;
		if(groundContact){contactCount++;}
		if(ceilingContact){contactCount++;}
		if(leftSideContact){contactCount++;}
		if(rightSideContact){contactCount++;}

		int embedCount = 0;
		if(d_SendCollisionMessages&&groundContact && ((m_GroundFootLength-directionContacts[0].distance)>=0.011f))	{ print("Embedded in grnd by amount: "+((m_GroundFootLength-directionContacts[0].distance)-m_MinEmbed)); embedCount++;} //If embedded too deep in this surface.
		if(d_SendCollisionMessages&&ceilingContact && ((m_CeilingFootLength-directionContacts[1].distance)>=0.011f))	{ print("Embedded in ceil by amount: "+((m_CeilingFootLength-directionContacts[1].distance)-m_MinEmbed)); embedCount++;} //If embedded too deep in this surface.
		if(d_SendCollisionMessages&&leftSideContact && ((m_LeftSideLength-directionContacts[2].distance)>=0.011f))	{ print("Embedded in left by amount: "+((m_LeftSideLength-directionContacts[2].distance)-m_MinEmbed)); embedCount++;} //If embedded too deep in this surface.
		if(d_SendCollisionMessages&&rightSideContact && ((m_RightSideLength-directionContacts[3].distance)>=0.011f))	{ print("Embedded in rigt by amount: "+((m_RightSideLength-directionContacts[3].distance)-m_MinEmbed)); embedCount++;} //If embedded too deep in this surface.

		if(d_SendCollisionMessages){print(contactCount+" sides touching, "+embedCount+" sides embedded");}
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
		if(d_SendCollisionMessages)
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
			if(d_SendCollisionMessages){print("Resolving left wedge.");}

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
					if(d_SendCollisionMessages){print("It's a wall, bro");}
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

		if(d_SendCollisionMessages)
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

		if(d_SendCollisionMessages){print("(A, B)=("+A+", "+B+").");}

		if(B <= 0)
		{
			if(d_SendCollisionMessages){print("B <= 0, using normal eqn.");}
			DivX = B-A;
			DivY = -(DivX/B);
		}
		else
		{
			if(d_SendCollisionMessages){print("B >= 0, using alternate eqn.");}
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
		if(d_SendCollisionMessages){print("SuperUnwedger push of: ("+X+","+Y+")");}
		return new Vector2(X,Y); // Returns the distance the object must move to resolve wedging.
	}

	protected void Jump(float horizontalInput)
	{
		Vector2 preJumpVelocity = FighterState.Vel;
		if(m_Grounded&&m_Ceilinged)
		{
			if(d_SendCollisionMessages)
			{print("Grounded and Ceilinged, nowhere to jump!");}
			//FighterState.JumpKey = false;
		}
		//	else if(m_Grounded)
		else if(m_JumpBufferG>0)
		{
			//m_LeftWallBlocked = false;
			//m_RightWallBlocked = false;

			n_Jumped = true;
			if(n_AutoGenerateNavCon&&!n_PlayerTraversing) 
			{
				StartPlayerTraverse(); // Generating a jump-type nav connection between the start and end of this jump
			}

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
			m_Grounded = false; // Watch this.
			v_PrimarySurface = -1;
		}
		//else if(m_LeftWalled)m_JumpBufferG>0
		else if(m_JumpBufferL>0)
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
			n_Jumped = true;
			if(n_AutoGenerateNavCon&&!n_PlayerTraversing)
			{
				StartPlayerTraverse();
			}
			v_PrimarySurface = -1;
		}
		//else if(m_RightWalled)
		else if(m_JumpBufferR>0)
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
			n_Jumped = true;
			if(n_AutoGenerateNavCon&&!n_PlayerTraversing)
			{
				StartPlayerTraverse();
			}
			v_PrimarySurface = -1;
		}
		//else if(m_Ceilinged)
		else if(m_JumpBufferC>0)
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
			n_Jumped = true;
			if(n_AutoGenerateNavCon&&!n_PlayerTraversing)
			{
				StartPlayerTraverse();
			}
			m_Ceilinged = false;
			v_PrimarySurface = -1;
		}
		else
		{
			//print("Can't jump, airborne!");
		}
			
		m_JumpBufferG = 0;
		m_JumpBufferC = 0;
		m_JumpBufferL = 0;
		m_JumpBufferR = 0;
	}

	protected void ZonJump(Vector2 jumpNormal)
	{
		if(FighterState.ZonLevel > 0)
		{
			FighterState.ZonLevel--;
		}
		FighterState.Vel = FighterState.Vel+(jumpNormal*(m_ZonJumpForceBase+(m_ZonJumpForcePerCharge*FighterState.ZonLevel)));	
		o_FighterAudio.JumpSound();
	}

	protected void StrandJumpTypeA(float horizontalInput, float verticalInput) //SJTA
	{
		float numberOfInputs = Math.Abs(horizontalInput)+Math.Abs(verticalInput);
		if(FighterState.Vel.magnitude>=20&&FighterState.ZonLevel > 0&&numberOfInputs > 0)
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
				
			if(bounceAngle<m_WidestStrandJumpAngle)
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
			//InstantForce(newDirection, FighterState.Vel.magnitude*(1-m_StrandJumpSpeedLossM));	
			m_StrandJumpReflectSpd = FighterState.Vel.magnitude*(1-m_StrandJumpSpeedLossM);
			m_StrandJumpReflectDir = newDirection;

			FighterState.ZonLevel--;
			o_FighterAudio.StrandJumpSound();
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
		return v_Sliding;
	}

	public void InstantForce(Vector2 newDirection, float speed)
	{
		//newDirection.Normalize();
		SetSpeed(newDirection, speed);
		//DirectionChange(newDirection);
		//print("Changing direction to" +newDirection);
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
			enemyFighter.TakeDamage(5);
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
			enemyFighter.TakeDamage(5);
			enemyFighter.FighterState.Vel += aimDirection.normalized*5;
			o_FighterAudio.PunchHitSound();
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

	public Vector2 GetFootPosition()
	{
		return m_GroundFoot.position;
	}

	public float GetSpeed()
	{
		return FighterState.Vel.magnitude;
	}

	public void SetZonLevel(int zonLevel)
	{
		FighterState.ZonLevel = zonLevel;
	}

	public int GetZonLevel()
	{
		return FighterState.ZonLevel;
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
		if(d_Invincible){return;}
		FighterState.CurHealth -= dmgAmount;
		if(dmgAmount>15)
		{
			o_FighterAudio.PainSound();
		}
	}

	public bool IsPlayer()
	{
		return isAPlayer;
	}

	#endregion
}

[System.Serializable] public struct FighterState
{
	[SerializeField][ReadOnlyAttribute]public int ZonLevel;					// Level of fighter Zon Power.
	[SerializeField][ReadOnlyAttribute]public bool DevMode;					// Turns on all dev cheats.
	[SerializeField][ReadOnlyAttribute]public int CurHealth;				// Current health.
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



	[SerializeField][ReadOnlyAttribute]public bool ZonKeyPress;
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