//using UnityEngine.UI;
//using System;
//using UnityEngine;
//using EZCameraShake;
//
///*
// * AUTHOR'S NOTES:
// * 
// * Naming Shorthand:
// * If a variable ends with: A, it is short for:B
// * 		A	|	B
// * --------------------------
// * 		T	| 	Threshold 	(Value at which something changes)
// * 		M	| 	Multiplier 	(Used to scale things. Usually a value between 0 and 1)
// * 		H	|	Horizontal 	(Axis)
// * 		V	|	Vertical	(Axis)
// * 		G	|	Ground		(Direction)
// * 		C	|	Ceiling		(Direction)
// * 		L	|	Left		(Direction)
// * 		R	|	Right		(Direction)
//*/
//
//[System.Serializable]
//public class PlatformerCharacter2D : MonoBehaviour
//{        	
//	//############################################################################################################################################################################################################
//	// HANDLING VARIABLES
//	//###########################################################################################################################################################################
//	#region MOVEMENT HANDLING
//	[Header("Movement Tuning:")]
//	[SerializeField] private float m.minSpeed = 10f; 							// The instant starting speed while moving
//	[SerializeField] private float m.maxRunSpeed;								// The fastest the player can travel along land.
//	[Range(0,2)][SerializeField] private float m_Acceleration = 1f;    			// Speed the player accelerates at
//	[SerializeField] private float m.vJumpForce = 40f;                  		// Amount of vertical force added when the player jumps.
//	[SerializeField] private float m.hJumpForce = 5f;  							// Amount of horizontal force added when the player jumps.
//	[SerializeField] private float m.wallVJumpForce = 20f;                  	// Amount of vertical force added when the player walljumps.
//	[SerializeField] private float m.wallHJumpForce = 10f;  					// Amount of horizontal force added when the player walljumps.
//	[SerializeField] private float m.tractionChangeT = 20f;						// Threshold where movement changes from exponential to linear acceleration.  
//	[SerializeField] private float m.wallTractionT = 20f;						// Speed threshold at which wallsliding traction changes.
//	[Range(0,5)][SerializeField] private float m.linearStopRate = 1f; 			// How fast the player decelerates when changing direction.
//	[Range(0,5)][SerializeField] private float m.linearSlideRate = 0.20f;		// How fast the player decelerates with no input.
//	[Range(0,5)][SerializeField] private float m.linearOverSpeedRate = 0.10f;	// How fast the player decelerates when running too fast.
//	[Range(0,5)][SerializeField] private float m.linearAccelRate = 0.35f;		// How fast the player accelerates with input.
//	[Range(1,89)][SerializeField] private float m.impactDecelMinAngle = 20f;	// Any impacts at sharper angles than this will start to slow the player down. Reaches full halt at m.impactDecelMaxAngle.
//	[Range(1,89)][SerializeField] private float m.impactDecelMaxAngle = 80f;	// Any impacts at sharper angles than this will result in a full halt. DO NOT SET THIS LOWER THAN m.impactDecelMinAngle!!
//	[Range(1,89)][SerializeField] private float m.tractionLossMinAngle = 45f; 	// Changes the angle at which steeper angles start to linearly lose traction, and eventually starts slipping back down. Default of 45 degrees.
//	[Range(45,90)][SerializeField] private float m.tractionLossMaxAngle = 90f; 	// Changes the angle at which player loses ALL traction, and starts slipping back down. Default of 90 degrees.
//	[Range(0,2)][SerializeField] private float m.slippingAcceleration = 1f;  	// Changes how fast the player slides down overly steep slopes.
//	[Range(0.5f,3)][SerializeField] private float m.surfaceClingTime = 1f; 		// How long the player can cling to walls before gravity takes over.
//	[Range(20,70)][SerializeField] private float m.clingReqGForce = 50f;		// This is the amount of impact GForce required for a full-duration ceiling cling.
//	[ReadOnlyAttribute]private Vector2 m.expiredNormal;							// This is the normal of the last surface clung to, to make sure the player doesn't repeatedly cling the same surface after clingtime expires.
//	[ReadOnlyAttribute]private float m.timeSpentHanging = 0f;					// Amount of time the player has been clung to a wall.
//	[ReadOnlyAttribute]private float m.maxTimeHanging = 0f;						// Max time the player can cling to current wall.
//	[Range(0,0.5f)][SerializeField] private float m.maxEmbed = 0.02f;			// How deep into objects the character can be before actually colliding with them. MUST BE GREATER THAN m.minEmbed!!!
//	[Range(0.01f,0.4f)][SerializeField] private float m.minEmbed = 0.01f; 		// How deep into objects the character will sit by default. A value of zero will cause physics errors because the player is not technically *touching* the surface.
//	[Space(10)]
//	[SerializeField] private float m.etherJumpForcePerCharge = 10f; 				// How much force does each Ether Charge add to the jump power?
//	[SerializeField] private float m.etherJumpForceBase = 40f; 					// How much force does a no-power Ether jump have?
//	[Space(10)]
//	[SerializeField] private float m.slamT = 100f; 								// Impact threshold for slam
//	[SerializeField] private float m.craterT = 200f; 							// Impact threshold for crater
//	[SerializeField] private float m.guardSlamT = 100f; 						// Guarded Impact threshold for slam
//	[SerializeField] private float m.guardCraterT = 200f; 						// Guarded Impact threshold for crater
//
//
//	#endregion
//	//############################################################################################################################################################################################################
//	// OBJECT REFERENCES
//	//###########################################################################################################################################################################
//	#region OBJECT REFERENCES
//	[Header("Player Components:")]
//
//	[SerializeField] private Text o_Speedometer;      		// Reference to the speed indicator (dev tool).
//	[SerializeField] private Light o_TempLight;      		// Reference to a spotlight attached to the character.
//	[SerializeField] private Camera o_MainCamera;			// Reference to the main camera.
//	[SerializeField] private CameraShaker o_CamShaker;		// Reference to the main camera's shaking controller.
//	[SerializeField] private GameObject o_CharSprite;		// Reference to the character's sprite.
//	[SerializeField] public CharacterAudio o_CharAudio;		// Reference to the character's audio handler.
//	[SerializeField] public Spooler o_Spooler;				// Reference to the character's spooler object, which handles power charging gameplay.
//	[SerializeField] public GameObject p_DebugMarker;		// Reference to a sprite prefab used to mark locations ingame during development.
//	[SerializeField] public Healthbar o_Healthbar;			// Reference to the Healthbar UI element.
//	private Animator o.anim;           						// Reference to the character's animator component.
//    private Rigidbody2D o.rigidbody2D;						// Reference to the character's physics body.
//	private SpriteRenderer o.spriteRenderer;				// Reference to the character's sprite renderer.
//	#endregion
//	//############################################################################################################################################################################################################
//	// PHYSICS&RAYCASTING
//	//###########################################################################################################################################################################
//	#region PHYSICS&RAYCASTING
//	[SerializeField] private LayerMask mask;// A mask determining what collides with the character.
//
//	private Transform m_GroundFoot; 		// Ground collider.
//	private Vector2 m_GroundFootOffset; 	// Ground raycast endpoint.
//	private float m_GroundFootLength;		// Ground raycast length.
//
//	private Transform m_CeilingFoot; 		// Ceiling collider, middle.
//	private Vector2 m_CeilingFootOffset;	// Ceiling raycast endpoint.
//	private float m_CeilingFootLength;		// Ceiling raycast length.
//
//	private Transform m_LeftSide; 			// LeftWall collider.
//	private Vector2 m_LeftSideOffset;		// LeftWall raycast endpoint.
//	private float m_LeftSideLength;			// LeftWall raycast length.
//
//	private Transform m_RightSide;  		// RightWall collider.
//	private Vector2 m_RightSideOffset;		// RightWall raycast endpoint.
//	private float m_RightSideLength;		// RightWall raycast length.
//
//	private Vector2 m_GroundNormal;			// Vector with slope of Ground.
//	private Vector2 m_CeilingNormal;		// Vector with slope of Ceiling.
//	private Vector2 m_LeftNormal;			// Vector with slope of LeftWall.
//	private Vector2 m_RightNormal;			// Vector with slope of RightWall.
//
//	[Header("Player State:")]
//	private Vector3 phys.lastSafePosition;										//Used to revert player position if they get totally stuck in something.
//	[SerializeField][ReadOnlyAttribute]private float phys.IGF; 				//"Instant G-Force" of the impact this frame.
//	[SerializeField][ReadOnlyAttribute]private float phys.CGF; 				//"Continuous G-Force" over time.
//	[SerializeField][ReadOnlyAttribute]private float phys.remainingVelM;		//Remaining velocity proportion after an impact. Range: 0-1.
//	[SerializeField][ReadOnlyAttribute]public float m_Spd;					//Current speed.
//	[SerializeField][ReadOnlyAttribute]public Vector2 m_Vel;				//Current (x,y) velocity.
//	[SerializeField][ReadOnlyAttribute]private Vector2 phys.remainingMovement; //Remaining (x,y) movement after impact.
//	[SerializeField][ReadOnlyAttribute]private bool phys.groundContact;			//True when touching surface.
//	[SerializeField][ReadOnlyAttribute]private bool phys.ceilingContact;			//True when touching surface.
//	[SerializeField][ReadOnlyAttribute]private bool phys.leftSideContact;		//True when touching surface.
//	[SerializeField][ReadOnlyAttribute]private bool phys.rightSideContact;		
//	[Space(10)]
//	[SerializeField][ReadOnlyAttribute]private bool phys.grounded;
//	[SerializeField][ReadOnlyAttribute]private bool phys.ceilinged; 
//	[SerializeField][ReadOnlyAttribute]private bool phys.leftWalled; 
//	[SerializeField][ReadOnlyAttribute]private bool phys.rightWalled;
//	[Space(10)]
//	[SerializeField][ReadOnlyAttribute]private bool phys.groundBlocked;
//	[SerializeField][ReadOnlyAttribute]private bool phys.ceilingBlocked; 
//	[SerializeField][ReadOnlyAttribute]private bool phys.leftWallBlocked; 
//	[SerializeField][ReadOnlyAttribute]private bool phys.rightWallBlocked; 
//	[Space(10)]
//	[SerializeField][ReadOnlyAttribute]private bool phys.surfaceCling;
//	[SerializeField][ReadOnlyAttribute]private bool phys.airborne;
//	[SerializeField][ReadOnlyAttribute]private bool m_Landing;
//	[SerializeField][ReadOnlyAttribute]private bool phys.kneeling;
//	[SerializeField][ReadOnlyAttribute]private bool m_Impact;
//
//	#endregion
//	//##########################################################################################################################################################################
//	// PLAYER INPUT VARIABLES
//	//###########################################################################################################################################################################
//	#region PLAYERINPUT
//	private bool i_JumpKey;
//	private bool i_LeftClick;
//	private bool i_RightClick;
//	private bool i_LeftKey;
//	private bool i_RightKey;
//	private bool i_UpKey;
//	private bool i_DownKey;
//	private bool i_EtherKey;
//	private int CtrlH; 					// Tracks horizontal keys pressed. Values are -1 (left), 0 (none), or 1 (right). 
//	private int CtrlV; 					// Tracks vertical keys pressed. Values are -1 (down), 0 (none), or 1 (up).
//	private bool v.facingDirection; 		// True means right, false means left.
//	private Vector2 i_MouseWorldPos;	// Mouse position in world coordinates.
//	private Vector2 i_PlayerMouseVector;// Vector pointing from the player to their mouse position.
//	#endregion
//	//############################################################################################################################################################################################################
//	// DEBUGGING VARIABLES
//	//##########################################################################################################################################################################
//	#region DEBUGGING
//	private int errorDetectingRecursionCount; 				//Iterates each time recursive trajectory correction executes on the current frame. Not currently used.
//	[Header("Debug:")]
//	[SerializeField] private bool autoRunLeft; 				// When true, player will behave as if the left key is pressed.
//	[SerializeField] private bool autoRunRight; 			// When true, player will behave as if the right key is pressed.
//	[SerializeField] private bool autoJump;					// When true, player jumps instantly on every surface.
//	[SerializeField] private bool antiTunneling; 			// When true, player will be pushed out of objects they are stuck in.
//	[SerializeField] private bool noGravity;				// Disable gravity.
//	[SerializeField] private bool showVelocityIndicator;	// Shows a line tracing the character's movement path.
//	[SerializeField] private bool showContactIndicators;	// Shows player's surface-contact raycasts, which turn green when touching something.
//	[SerializeField] private bool recoverFromFullEmbed;		// When true and the player is fully stuck in something, teleports player to last good position.
//	[SerializeField] private bool d_ClickToKnockPlayer;		// When true and you left click, the player is propelled toward where you clicked.
//	[SerializeField] public  bool d_DevMode;				// Turns on all dev cheats.
//	private LineRenderer m_DebugLine; 						// Part of above indicators.
//	private LineRenderer m_GroundLine;						// Part of above indicators.		
//	private LineRenderer m_CeilingLine;						// Part of above indicators.		
//	private LineRenderer m_LeftSideLine;					// Part of above indicators.		
//	private LineRenderer m_RightSideLine;					// Part of above indicators.		
//	#endregion
//	//############################################################################################################################################################################################################
//	// VISUAL&SOUND VARIABLES
//	//###########################################################################################################################################################################
//	#region VISUALS&SOUND
//	[Header("Visuals And Sound:")]
//	[SerializeField]private int v_EtherLevel;							//	Level of player Ether Power.
//	[SerializeField][Range(0,10)]private float v.reversingSlideT; 	// How fast the player must be going to go into a slide posture when changing directions.
//	[SerializeField]private float v_CameraZoom; 					// Amount of camera zoom.
//	[SerializeField][Range(0,3)]private int v_PlayerGlow;			// Amount of player "energy glow" effect.
//	#endregion 
//	//############################################################################################################################################################################################################
//	// GAMEPLAY VARIABLES
//	//###########################################################################################################################################################################
//	#region GAMEPLAY VARIABLES
//	[Header("Gameplay:")]
//	[SerializeField]private int g_EtherJumpCharge;					//	Level of power channelled into current jump.
//	[SerializeField][ReadOnlyAttribute] private int g_EtherStance;	// Which stance is the player in? -1 = no stance.
//	[SerializeField] private int g_CurHealth;						// Current health.
//	[SerializeField] private int g_MaxHealth;						// Max health.
//	[SerializeField] private int g_MinSlamDMG;						// Min damage a slam impact can deal.
//	[SerializeField] private int g_MaxSlamDMG;						// Max damage a slam impact can deal.	
//	[SerializeField] private int g_MinCrtrDMG;						// Min damage a crater impact can deal.
//	[SerializeField] private int g_MaxCrtrDMG;						// Max damage a crater impact can deal.
//						
//
//
//
//	#endregion 
//
//
//	//########################################################################################################################################
//	// CORE FUNCTIONS
//	//########################################################################################################################################
//	#region CORE FUNCTIONS
//    private void Awake()
//    {
//		Vector2 playerOrigin = new Vector2(this.transform.position.x, this.transform.position.y);
//		m_DebugLine = GetComponent<LineRenderer>();
//
//		m_GroundFoot = transform.Find("MidFoot");
//		m_GroundLine = m_GroundFoot.GetComponent<LineRenderer>();
//		m_GroundFootOffset.x = m_GroundFoot.position.x-playerOrigin.x;
//		m_GroundFootOffset.y = m_GroundFoot.position.y-playerOrigin.y;
//		m_GroundFootLength = m_GroundFootOffset.magnitude;
//
//		m_CeilingFoot = transform.Find("CeilingFoot");
//		m_CeilingLine = m_CeilingFoot.GetComponent<LineRenderer>();
//		m_CeilingFootOffset.x = m_CeilingFoot.position.x-playerOrigin.x;
//		m_CeilingFootOffset.y = m_CeilingFoot.position.y-playerOrigin.y;
//		m_CeilingFootLength = m_CeilingFootOffset.magnitude;
//
//		m_LeftSide = transform.Find("LeftSide");
//		m_LeftSideLine = m_LeftSide.GetComponent<LineRenderer>();
//		m_LeftSideOffset.x = m_LeftSide.position.x-playerOrigin.x;
//		m_LeftSideOffset.y = m_LeftSide.position.y-playerOrigin.y;
//		m_LeftSideLength = m_LeftSideOffset.magnitude;
//
//		m_RightSide = transform.Find("RightSide");
//		m_RightSideLine = m_RightSide.GetComponent<LineRenderer>();
//		m_RightSideOffset.x = m_RightSide.position.x-playerOrigin.x;
//		m_RightSideOffset.y = m_RightSide.position.y-playerOrigin.y;
//		m_RightSideLength = m_RightSideOffset.magnitude;
//
//
//		o.anim = o_CharSprite.GetComponent<Animator>();
//        o.rigidbody2D = GetComponent<Rigidbody2D>();
//		o.spriteRenderer = o_CharSprite.GetComponent<SpriteRenderer>();
//
//		phys.lastSafePosition = new Vector2(0,0);
//		phys.remainingMovement = new Vector2(0,0);
//		phys.remainingVelM = 1f;
//		//print(phys.remainingMovement);
//
//		o_CamShaker = o_MainCamera.GetComponent<CameraShaker>();
//
//		if(!(showVelocityIndicator||d_DevMode)){
//			m_DebugLine.enabled = false;
//		}
//
//		if(!(showContactIndicators||d_DevMode))
//		{
//			m_CeilingLine.enabled = false;
//			m_GroundLine.enabled = false;
//			m_RightSideLine.enabled = false;
//			m_LeftSideLine.enabled = false;
//		}
//    }
//
//    private void FixedUpdate()
//	{
//		Vector2 finalPos = new Vector2(this.transform.position.x+phys.remainingMovement.x, this.transform.position.y+phys.remainingMovement.y);
//		this.transform.position = finalPos;
//
//		UpdateContactNormals(true);
//
//		Vector2 phys.initialVel = m_Vel;
//		i_PlayerMouseVector =  i_MouseWorldPos-Vec2(this.transform.position);
//
//		m_Impact = false;
//		m_Landing = false;
//		phys.kneeling = false;
//		g_EtherStance = -1;
//
//		//print("Initial Pos: " + startingPos);
//		//print("Initial Vel: " +  m_Vel);
//
//		#region playerinput
//
//		if(!(i_LeftKey||i_RightKey) || (i_LeftKey && i_RightKey))
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
//		else if(i_LeftKey)
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
//			v.facingDirection = false; //true means right (the direction), false means left.
//		} 
//		else if (CtrlH > 0)
//		{
//			v.facingDirection = true; //true means right (the direction), false means left.
//		}
//
//		//print("CTRLH=" + CtrlH);
//		if(i_DownKey&&phys.grounded)
//		{
//			phys.kneeling = true;
//			CtrlH = 0;
//			g_EtherStance = 0; // Kneeling stance.
//		}
//		else
//		{
//			g_EtherJumpCharge=0;
//		}
//
//		if(i_JumpKey)
//		{
//			if(phys.kneeling)
//			{
//				EtherJump(i_PlayerMouseVector.normalized);
//			}
//			else
//			{
//				Jump(CtrlH);
//			}
//		}
//
//		if(i_LeftClick&&(d_DevMode||d_ClickToKnockPlayer))
//		{
//			m_Vel += i_PlayerMouseVector*10;
//			print("Leftclick detected");
//			i_LeftClick = false;
//		}	
//
//		if(i_RightClick&&(d_DevMode))
//		{
//			//GameObject newMarker = (GameObject)Instantiate(o_DebugMarker);
//			//newMarker.name = "DebugMarker";
//			//newMarker.transform.position = i_MouseWorldPos;
//			i_RightClick = false;
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
//		#endregion
//
//		if(phys.grounded)
//		{//Locomotion!
//			Traction(CtrlH);
//		}
//		else if(phys.rightWalled)
//		{//Wallsliding!
//			WallTraction(CtrlH,m_RightNormal);
//		}
//		else if(phys.leftWalled)
//		{
//			WallTraction(CtrlH,m_LeftNormal);
//		}
//		else if(!noGravity)
//		{//Gravity!
//			m_Vel = new Vector2 (m_Vel.x, m_Vel.y - 1);
//			phys.ceilinged = false;
//		}
//		
//			
//		errorDetectingRecursionCount = 0; //Used for Collizion();
//
//		//print("Velocity before Coll ision: "+m_Vel);
//		//print("Position before Coll ision: "+this.transform.position);
//
//		phys.remainingVelM = 1f;
//		phys.remainingMovement = m_Vel*Time.fixedDeltaTime;
//		Vector2 startingPos = this.transform.position;
//
//		//print("phys.remainingMovement before collision: "+phys.remainingMovement);
//
//		Collision();
//	
//		//print("Per frame velocity at end of Collizion() "+m_Vel*Time.fixedDeltaTime);
//		//print("Velocity at end of Collizion() "+m_Vel);
//		//print("Per frame velocity at end of updatecontactnormals "+m_Vel*Time.fixedDeltaTime);
//		//print("phys.remainingMovement after collision: "+phys.remainingMovement);
//
//		Vector2 distanceTravelled = new Vector2(this.transform.position.x-startingPos.x,this.transform.position.y-startingPos.y);
//		//print("distanceTravelled: "+distanceTravelled);
//		//print("phys.remainingMovement: "+phys.remainingMovement);
//		//print("phys.remainingMovement after removing distancetravelled: "+phys.remainingMovement);
//
//		if(phys.initialVel.magnitude>0)
//		{
//			phys.remainingVelM = (((phys.initialVel.magnitude*Time.fixedDeltaTime)-distanceTravelled.magnitude)/(phys.initialVel.magnitude*Time.fixedDeltaTime));
//		}
//		else
//		{
//			phys.remainingVelM = 1f;
//		}
//
//		//print("phys.remainingVelM: "+phys.remainingVelM);
//		//print("movement after distance travelled: "+phys.remainingMovement);
//		//print("Speed this frame: "+m_Vel.magnitude);
//
//		phys.remainingMovement = m_Vel*phys.remainingVelM*Time.fixedDeltaTime;
//
//		//print("Corrected remaining movement: "+phys.remainingMovement);
//
//		m_Spd = m_Vel.magnitude;
//
//		Vector2 deltaV = m_Vel-phys.initialVel;
//		phys.IGF = deltaV.magnitude;
//		phys.CGF += phys.IGF;
//		if(phys.CGF>=1){phys.CGF --;}
//		if(phys.CGF>=10){phys.CGF -= (phys.CGF/10);}
//
//		//if(phys.CGF>=200)
//		//{
//		//	//phys.CGF = 0f;
//		//	print("phys.CGF over limit!!");	
//		//}
//
//		if(m_Impact)
//		{
//			if(phys.IGF >= m.craterT)
//			{
//				Crater();
//			}
//			else if(phys.IGF >= m.slamT)
//			{
//				Slam();
//			}
//			else
//			{
//				o_CharAudio.LandingSound(phys.IGF);
//			}
//		}
//		//print("Per frame velocity at end of physics frame: "+m_Vel*Time.fixedDeltaTime);
//		//print("phys.remainingMovement at end of physics frame: "+phys.remainingMovement);
//		//print("Pos at end of physics frame: "+this.transform.position);
//		//print("##############################################################################################");
//		//print("FinaL Pos: " + this.transform.position);
//		//print("FinaL Vel: " + m_Vel);
//		//print("Speed at end of frame: " + m_Vel.magnitude);
//
////		#region audio
////		if(m_Landing)
////		{
////			o_CharAudio.LandingSound(phys.IGF); // Makes a landing sound when the player hits ground, using the impact force to determine loudness.
////		}
////		#endregion
//
//		#region Animator
//
//		//
//		//Animator Controls
//		//
//
//		v_PlayerGlow = v_EtherLevel;
//		if (v_PlayerGlow > 7){v_PlayerGlow = 7;}
//
//		if(v_PlayerGlow>2)
//		{
//			o_TempLight.color = new Color(1,1,0,1);
//			o_TempLight.intensity = (v_PlayerGlow)+(UnityEngine.Random.Range(-1f,1f));
//		}
//		else
//		{
//			o_TempLight.color = new Color(1,1,1,1);
//			o_TempLight.intensity = 2;
//		}
//
//		o.anim.SetBool("Walled", false);
//
//		if(phys.leftWalled&&!phys.grounded)
//		{
//			o.anim.SetBool("Walled", true);
//			v.facingDirection = false;
//			o_CharSprite.transform.localPosition = new Vector3(0.13f, 0f,0f);
//		}
//
//		if(phys.rightWalled&&!phys.grounded)
//		{
//			o.anim.SetBool("Walled", true);
//			v.facingDirection = true;
//			o_CharSprite.transform.localPosition = new Vector3(-0.13f, 0f,0f);
//		}
//
//		if(phys.grounded || !(phys.rightWalled||phys.leftWalled))
//		{
//			o_CharSprite.transform.localPosition = new Vector3(0f,0f,0f);
//		}
//
//		if (!v.facingDirection) //If facing left
//		{
//			//print("FACING LEFT!   "+h)
//			o_CharSprite.transform.localScale = new Vector3 (-1f, 1f, 1f);
//			if(m_Vel.x > 0 && m_Spd >= v.reversingSlideT)
//			{
//				o.anim.SetBool("Crouch", true);
//			}
//			else
//			{
//				o.anim.SetBool("Crouch", false);
//			}
//		} 
//		else //If facing right
//		{
//			//print("FACING RIGHT!   "+h);
//
//			o_CharSprite.transform.localScale = new Vector3 (1f, 1f, 1f);
//			if(m_Vel.x < 0 && m_Spd >= v.reversingSlideT)
//			{
//				o.anim.SetBool("Crouch", true);
//			}
//			else
//			{
//				o.anim.SetBool("Crouch", false);
//			}
//		}
//			
//		if(phys.kneeling)
//		{
//			o.anim.SetBool("Crouch", true);
//
//			if((i_MouseWorldPos.x-this.transform.position.x)<0)
//			{
//				v.facingDirection = false;
//				o_CharSprite.transform.localScale = new Vector3 (-1f, 1f, 1f);
//			}
//			else
//			{
//				v.facingDirection = true;
//				o_CharSprite.transform.localScale = new Vector3 (1f, 1f, 1f);
//			}
//		}
//
//		Vector3[] debugLineVector = new Vector3[3];
//
//		debugLineVector[0].x = -distanceTravelled.x;
//		debugLineVector[0].y = -(distanceTravelled.y+(m_GroundFootLength-m.maxEmbed));
//		debugLineVector[0].z = 0f;
//
//		debugLineVector[1].x = 0f;
//		debugLineVector[1].y = -(m_GroundFootLength-m.maxEmbed);
//		debugLineVector[1].z = 0f;
//
//		debugLineVector[2].x = phys.remainingMovement.x;
//		debugLineVector[2].y = (phys.remainingMovement.y)-(m_GroundFootLength-m.maxEmbed);
//		debugLineVector[2].z = 0f;
//
//		m_DebugLine.SetPositions(debugLineVector);
//
//		o.anim.SetFloat("Speed", m_Vel.magnitude);
//
//		if(m_Vel.magnitude >= m.tractionChangeT )
//		{
//			m_DebugLine.endColor = Color.white;
//			m_DebugLine.startColor = Color.white;
//		}   
//		else
//		{   
//			m_DebugLine.endColor = Color.blue;
//			m_DebugLine.startColor = Color.blue;
//		}
//
//		float multiplier = 1; // Animation playspeed multiplier that increases with higher velocity
//
//		if(m_Vel.magnitude > 20.0f)
//		{
//			multiplier = ((m_Vel.magnitude - 20) / 20)+1;
//		}
//
//		o.anim.SetFloat("Multiplier", multiplier);
//
//		if (!phys.grounded&&!phys.leftWalled&!phys.rightWalled) 
//		{
//			o.anim.SetBool("Ground", false);
//		}
//		else
//		{
//			o.anim.SetBool("Ground", true);
//		}
//		#endregion
//
//		i_RightClick = false;
//		i_LeftClick = false;
//		i_EtherKey = false;
//
//    }
//
//	private void Update()
//	{
//		if(Input.GetMouseButtonDown(0))
//		{
//			i_LeftClick = true;
//		}
//
//		if(Input.GetMouseButtonDown(1))
//		{
//			i_RightClick = true;
//		}
//			
//		if(Input.GetButtonDown("Spooling"))
//		{
//			i_EtherKey = true;				
//		}
//
//		Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//		i_MouseWorldPos = Vec2(mousePoint);
//
//		i_LeftKey = Input.GetButton("Left");
//		i_RightKey = Input.GetButton("Right");
//		i_UpKey = Input.GetButton("Up");
//		i_DownKey = Input.GetButton("Down");
//
//		if(o_Speedometer != null)
//		{
//			o_Speedometer.text = ""+Math.Round(m_Vel.magnitude,0);
//		}
//
//		if (!i_JumpKey && (phys.grounded||phys.ceilinged||phys.leftWalled||phys.rightWalled))
//		{
//			// Read the jump input in Update so button presses aren't missed.
//
//			i_JumpKey = Input.GetButtonDown("Jump");
//			if(autoJump)
//			{
//				i_JumpKey = true;
//			}
//		}
//	}
//
//	private void LateUpdate()
//	{
//		CameraControl();
//	}
//	#endregion
//	//###################################################################################################################################
//	// CUSTOM FUNCTIONS
//	//###################################################################################################################################
//	#region CUSTOM FUNCTIONS
//
//	private Vector2 Vec2(Vector3 inputVector)
//	{
//		return new Vector2(inputVector.x, inputVector.y);
//	}
//
//	private void Crater() // Triggered when character impacts anything REALLY hard.
//	{
//		float Multiplier = (phys.IGF+m.craterT)/(2*m.craterT);
//		//print(Multiplier);
//		if(Multiplier >=2){Multiplier = 2;}
//		float Magnitude = Multiplier;
//		float Roughness = 10f;
//		float FadeOutTime = 2.5f;
//		float FadeInTime = 0f;
//		Vector3 RotInfluence = new Vector3(1,1,1);
//		Vector3 PosInfluence = new Vector3(0.15f,0.15f,0.15f);
//		CameraShaker.Instance.ShakeOnce(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);
//
//		o_CharAudio.CraterSound(phys.IGF, m.craterT, 1000f);
//
//		float damagedealt = g_MinCrtrDMG+((g_MaxCrtrDMG-g_MinCrtrDMG)*((phys.IGF-m.craterT)/(1000f-m.craterT))); // Damage dealt scales linearly from minDMG to maxDMG, reaching max damage at a 1000 kph impact.
//		g_CurHealth -= (int)damagedealt;
//		if(g_CurHealth < 0){g_CurHealth = 100;}
//		o_Healthbar.SetCurHealth(g_CurHealth);
//		print("CRATERED!");
//	}
//
//	private void Slam() // Triggered when character impacts anything too hard.
//	{
//		float Multiplier = (phys.IGF+m.slamT)/(2*m.slamT);
//		//print(Multiplier);
//		if(Multiplier >=2){Multiplier = 2;}
//		float Magnitude = 0.5f;
//		float Roughness = 20f;
//		float FadeOutTime = 0.6f*Multiplier;
//		float FadeInTime = 0f;
//		float posM = 0.3f*Multiplier;
//		Vector3 RotInfluence = new Vector3(0,0,0);
//		Vector3 PosInfluence = new Vector3(posM,posM,0);
//		CameraShaker.Instance.ShakeOnce(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);
//		print("SLAMMIN!");
//
//		o_CharAudio.SlamSound(phys.IGF, m.slamT, m.craterT);
//
//		float damagedealt = g_MinSlamDMG+((g_MaxSlamDMG-g_MinSlamDMG)*((phys.IGF-m.slamT)/(m.craterT-m.slamT))); // Damage dealt scales linearly from minDMG to maxDMG, as you go from the min Slam Threshold to min Crater Threshold (impact speed)
//		g_CurHealth -= (int)damagedealt;
//		if(g_CurHealth < 0){g_CurHealth = 100;}
//		o_Healthbar.SetCurHealth(g_CurHealth);
//	}
//
//	private void Collision()	// Handles all collisions with terrain geometry.
//	{
//
//		//print ("Collision->phys.grounded=" + phys.grounded);
//		float crntSpeed = m_Vel.magnitude*Time.fixedDeltaTime; //Current speed.
//		//print("DC Executing");
//		errorDetectingRecursionCount++;
//
//		if(errorDetectingRecursionCount >= 5)
//		{
//			throw new Exception("Your recursion code is not working!");
//			//return;
//		}
//			
//		if(m_Vel.x > 0.001f)
//		{
//			phys.leftWallBlocked = false;
//		}
//
//		if(m_Vel.x < -0.001f)
//		{
//			phys.rightWallBlocked = false;
//		}
//	
//		#region collision raytesting
//
//		Vector2 adjustedBot = m_GroundFoot.position; // AdjustedBot marks the end of the ground raycast, but 0.02 shorter.
//		adjustedBot.y += m.maxEmbed;
//
//		Vector2 adjustedTop = m_CeilingFoot.position; // AdjustedTop marks the end of the ceiling raycast, but 0.02 shorter.
//		adjustedTop.y -= m.maxEmbed;
//
//		Vector2 adjustedLeft = m_LeftSide.position; // AdjustedLeft marks the end of the left wall raycast, but 0.02 shorter.
//		adjustedLeft.x += m.maxEmbed;
//
//		Vector2 adjustedRight = m_RightSide.position; // AdjustedRight marks the end of the right wall raycast, but 0.02 shorter.
//		adjustedRight.x -= m.maxEmbed;
//
//		//RaycastHit2D groundCheck = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength, mask);
//		RaycastHit2D[] predictedLoc = new RaycastHit2D[4];
//
//		predictedLoc[0] = Physics2D.Raycast(adjustedBot, m_Vel, crntSpeed, mask); 	// Ground
//		predictedLoc[1] = Physics2D.Raycast(adjustedTop, m_Vel, crntSpeed, mask); 	// Ceiling
//		predictedLoc[2] = Physics2D.Raycast(adjustedLeft, m_Vel, crntSpeed, mask); // Left
//		predictedLoc[3] = Physics2D.Raycast(adjustedRight, m_Vel, crntSpeed, mask);// Right  
//
//		float[] rayDist = new float[4];
//		rayDist[0] = predictedLoc[0].distance; // Ground dist
//		rayDist[1] = predictedLoc[1].distance; // Ceiling dist
//		rayDist[2] = predictedLoc[2].distance; // Left dist
//		rayDist[3] = predictedLoc[3].distance; // Right dist
//
//
//		int shortestVertical = -1;
//		int shortestHorizontal = -1;
//		int shortestRaycast = -1;
//
//		//Shortest non-zero vertical collision.
//		if(rayDist[0] != 0 && rayDist[1]  != 0)
//		{
//			if(rayDist[0] <= rayDist[1])
//			{
//				shortestVertical = 0;
//			}
//			else
//			{
//				shortestVertical = 1;
//			}
//		}
//		else if(rayDist[0] != 0)
//		{
//			shortestVertical = 0;
//		}
//		else if(rayDist[1] != 0)
//		{
//			shortestVertical = 1;
//		}
//
//		//Shortest non-zero horizontal collision.
//		if(rayDist[2] != 0 && rayDist[3]  != 0)
//		{
//			if(rayDist[2] <= rayDist[3])
//			{
//				shortestHorizontal = 2;
//			}
//			else
//			{
//				shortestHorizontal = 3;
//			}
//		}
//		else if(rayDist[2] != 0)
//		{
//			shortestHorizontal = 2;
//		}
//		else if(rayDist[3] != 0)
//		{
//			shortestHorizontal = 3;
//		}
//
//		//non-zero shortest of all four colliders.
//		if(shortestVertical >= 0 && shortestHorizontal >= 0)
//		{
//			//print("Horiz dist="+shortestHorizontal);
//			//print("Verti dist="+shortestVertical);
//			//print("Verti-horiz="+(shortestVertical-shortestHorizontal));
//			if(rayDist[shortestVertical] < rayDist[shortestHorizontal])
//			{
//				shortestRaycast = shortestVertical;
//			}
//			else
//			{
//				shortestRaycast = shortestHorizontal;
//			}
//		}
//		else if(shortestVertical >= 0)
//		{
//			//print("Shortest is vertical="+shortestVertical);
//			shortestRaycast = shortestVertical;
//		}
//		else if(shortestHorizontal >= 0)
//		{
//			//print("Shortest is horizontal="+shortestHorizontal);
//			shortestRaycast = shortestHorizontal;
//		}
//		else
//		{
//			//print("NOTHING?");	
//		}
//			
//		//print("G="+gDist+" C="+cDist+" R="+rDist+" L="+lDist);
//		//print("VDist: "+shortestDistV);
//		//print("HDist: "+shortestDistH);
//
//
//		//print("shortestDist: "+rayDist[shortestRaycast]);
//
//		int collisionNum = 0;
//
//		if(predictedLoc[0])
//		{
//			collisionNum++;
//		}
//		if(predictedLoc[1])
//		{
//			collisionNum++;
//		}
//		if(predictedLoc[2])
//		{
//			collisionNum++;
//		}
//		if(predictedLoc[3])
//		{
//			collisionNum++;
//		}
//
//		if(collisionNum>0)
//		{
//			//print("TOTAL COLLISIONS: "+collisionNum);
//		}
//
//		#endregion
//
//		Vector2 moveDirectionNormal = Perp(m_Vel.normalized);
//		Vector2 invertedDirectionNormal = -moveDirectionNormal;//This is made in case one of the raycasts is inside the collider, which would cause it to return an inverted normal value.
//
//		switch (shortestRaycast)
//		{
//			case -1:
//			{
//				//print("No collision!");
//				break;
//			}
//			case 0://Ground collision with feet
//			{
//				//If you're going to hit something with your feet.
//				print("FOOT_IMPACT");
//				//print("Velocity before impact: "+m_Vel);
//
//				//print("GroundDist"+predictedLoc[0].distance);
//				//print("RightDist"+predictedLoc[3].distance);
//
//				if ((moveDirectionNormal != predictedLoc[0].normal) && (invertedDirectionNormal != predictedLoc[0].normal)) 
//				{ // If the slope you're hitting is different than your current slope.
//					ToGround(predictedLoc[0]);
//					DirectionChange(m_GroundNormal);
//					return;
//				}
//				else 
//				{
//					if(invertedDirectionNormal == predictedLoc[0].normal)
//					{
//						throw new Exception("INVERTED GROUND IMPACT NORMAL DETECTED!");
//					}
//					return;
//				}
//			}
//			case 1:
//			{
//
//				if ((moveDirectionNormal != predictedLoc[1].normal) && (invertedDirectionNormal != predictedLoc[1].normal)) 
//				{ // If the slope you're hitting is different than your current slope.
//					//print("CEILINm_Impact");
//					ToCeiling(predictedLoc[1]);
//					DirectionChange(m_CeilingNormal);
//					return;
//				}
//				else 
//				{
//					if(invertedDirectionNormal == predictedLoc[1].normal)
//					{
//						throw new Exception("INVERTED CEILING IMPACT NORMAL DETECTED!");
//					}
//					return;
//				}
//				break;
//			}
//			case 2:
//			{
//				if ((moveDirectionNormal != predictedLoc[2].normal) && (invertedDirectionNormal != predictedLoc[2].normal)) 
//				{ // If the slope you're hitting is different than your current slope.
//					print("LEFT_IMPACT");
//					ToLeftWall(predictedLoc[2]);
//					DirectionChange(m_LeftNormal);
//					return;
//				}
//				else 
//				{
//					if(invertedDirectionNormal == predictedLoc[2].normal)
//					{
//						throw new Exception("INVERTED LEFT IMPACT NORMAL DETECTED!");
//					}
//					return;
//				}
//			}
//			case 3:
//			{
//				if ((moveDirectionNormal != predictedLoc[3].normal) && (invertedDirectionNormal != predictedLoc[3].normal)) 
//				{ // If the slope you're hitting is different than your current slope.
//					//print("RIGHT_IMPACT");
//					//print("predictedLoc[3].normal=("+predictedLoc[3].normal.x+","+predictedLoc[3].normal.y+")");
//					//print("moveDirectionNormal=("+moveDirectionNormal.x+","+moveDirectionNormal.y+")");
//					//print("moveDirectionNormal="+moveDirectionNormal);
//					ToRightWall(predictedLoc[3]);
//					DirectionChange(m_RightNormal);
//					return;
//				}
//				else 
//				{
//					if(invertedDirectionNormal == predictedLoc[3].normal)
//					{
//						throw new Exception("INVERTED RIGHT IMPACT NORMAL DETECTED!");
//					}
//					return;
//				}
//			}
//			default:
//			{
//				print("ERROR: DEFAULTED.");
//				break;
//			}
//		}
//	}
//		
//	private void Traction(float horizontalInput)
//	{
//		Vector2 groundPerp = Perp(m_GroundNormal);
//
//		//print("Traction");
//		//print("gp="+groundPerp);
//
//		if(groundPerp.x > 0)
//		{
//			groundPerp *= -1;
//		}
//
//		float steepnessAngle = Vector2.Angle(Vector2.left,groundPerp);
//
//		steepnessAngle = (float)Math.Round(steepnessAngle,2);
//		//print("SteepnessAngle:"+steepnessAngle);
//
//		float slopeMultiplier = 0;
//	
//		if(steepnessAngle > m.tractionLossMinAngle)
//		{
//			if(steepnessAngle >= m.tractionLossMaxAngle)
//			{
//				//print("MAXED OUT!");
//				slopeMultiplier = 1;
//			}
//			else
//			{
//				slopeMultiplier = ((steepnessAngle-m.tractionLossMinAngle)/(m.tractionLossMaxAngle-m.tractionLossMinAngle));
//			}
//
//			//print("slopeMultiplier: "+ slopeMultiplier);
//			//print("groundPerpY: "+groundPerpY+", slopeT: "+slopeT);
//		}
//
//
//		if(((phys.leftWallBlocked)&&(horizontalInput < 0)) || ((phys.rightWallBlocked)&&(horizontalInput > 0)))
//		{// If running at an obstruction you're up against.
//			//print("Running against a wall.");
//			horizontalInput = 0;
//		}
//
//		//print("Traction executing");
//		float rawSpeed = m_Vel.magnitude;
//		//print("m_Vel.magnitude"+m_Vel.magnitude);
//
//		if (horizontalInput == 0) 
//		{//if not pressing any move direction, slow to zero linearly.
//			//print("No input, slowing...");
//			if(rawSpeed <= 0.5f)
//			{
//				m_Vel = Vector2.zero;	
//			}
//			else
//			{
//				m_Vel = ChangeSpeedLinear (m_Vel, -m.linearSlideRate);
//			}
//		}
//		else if((horizontalInput > 0 && m_Vel.x >= 0) || (horizontalInput < 0 && m_Vel.x <= 0))
//		{//if pressing same button as move direction, move to MAXSPEED.
//			//print("Moving with keypress");
//			if(rawSpeed < m.maxRunSpeed)
//			{
//				//print("Rawspeed("+rawSpeed+") less than max");
//				if(rawSpeed > m.tractionChangeT)
//				{
//					//print("LinAccel-> " + rawSpeed);
//					if(m_Vel.y > 0)
//					{ 	// If climbing, recieve uphill movement penalty.
//						m_Vel = ChangeSpeedLinear(m_Vel, m.linearAccelRate*(1-slopeMultiplier));
//					}
//					else
//					{
//						m_Vel = ChangeSpeedLinear(m_Vel, m.linearAccelRate);
//					}
//				}
//				else if(rawSpeed < 0.001)
//				{
//					m_Vel = new Vector2((m_Acceleration)*horizontalInput*(1-slopeMultiplier), 0);
//					//print("Starting motion. Adding " + m_Acceleration);
//				}
//				else
//				{
//					//print("ExpAccel-> " + rawSpeed);
//					float eqnX = (1+Mathf.Abs((1/m.tractionChangeT )*rawSpeed));
//					float curveMultiplier = 1+(1/(eqnX*eqnX)); // Goes from 1/4 to 1, increasing as speed approaches 0.
//
//					float addedSpeed = curveMultiplier*(m_Acceleration);
//					if(m_Vel.y > 0)
//					{ // If climbing, recieve uphill movement penalty.
//						addedSpeed = curveMultiplier*(m_Acceleration)*(1-slopeMultiplier);
//					}
//					//print("Addedspeed:"+addedSpeed);
//					m_Vel = (m_Vel.normalized)*(rawSpeed+addedSpeed);
//					//print("m_Vel:"+m_Vel);
//				}
//			}
//			else
//			{
//				if(rawSpeed < m.maxRunSpeed+1)
//				{
//					rawSpeed = m.maxRunSpeed;
//					m_Vel = SetSpeed(m_Vel,m.maxRunSpeed);
//				}
//				else
//				{
//					//print("Rawspeed("+rawSpeed+") more than max.");
//					m_Vel = ChangeSpeedLinear (m_Vel, -m.linearOverSpeedRate);
//				}
//			}
//		}
//		else if((horizontalInput > 0 && m_Vel.x < 0) || (horizontalInput < 0 && m_Vel.x > 0))
//		{//if pressing button opposite of move direction, slow to zero exponentially.
//			if(rawSpeed > m.tractionChangeT )
//			{
//				//print("LinDecel");
//				m_Vel = ChangeSpeedLinear (m_Vel, -m.linearStopRate);
//			}
//			else
//			{
//				//print("Decelerating");
//				float eqnX = (1+Mathf.Abs((1/m.tractionChangeT )*rawSpeed));
//				float curveMultiplier = 1+(1/(eqnX*eqnX)); // Goes from 1/4 to 1, increasing as speed approaches 0.
//				float addedSpeed = curveMultiplier*(m_Acceleration-slopeMultiplier);
//				m_Vel = (m_Vel.normalized)*(rawSpeed-2*addedSpeed);
//			}
//
//			//float modifier = Mathf.Abs(m_Vel.x/m_Vel.y);
//			//print("SLOPE MODIFIER: " + modifier);
//			//m_Vel = m_Vel/(1.25f);
//		}
//
//		Vector2 downSlope = m_Vel.normalized; // Normal vector pointing down the current slope!
//		if (downSlope.y > 0) //Make sure the vector is descending.
//		{
//			downSlope *= -1;
//		}
//
//
//
//		if(downSlope == Vector2.zero)
//		{
//			downSlope = Vector2.down;
//		}
//
//		m_Vel += downSlope*m.slippingAcceleration*slopeMultiplier;
//
//		//	TESTINGSLOPES
//		//print("downSlope="+downSlope);
//		//print("m.slippingAcceleration="+m.slippingAcceleration);
//		//print("slopeMultiplier="+slopeMultiplier);
//
//			//ChangeSpeedLinear(m_Vel, );
//		//print("PostTraction velocity: "+m_Vel);
//	}
//
//	private void WallTraction(float horizontalInput, Vector2 wallSurface)
//	{
//		////////////////////
//		// Variable Setup //
//		////////////////////
//		Vector2 wallPerp = Perp(wallSurface);
//
//		//print("horizontalInput="+horizontalInput);
//
//		if(wallPerp.x > 0)
//		{
//			wallPerp *= -1;
//		}
//
//		float steepnessAngle = Vector2.Angle(Vector2.up,wallPerp);
//
//		if(phys.rightWalled)
//		{
//			steepnessAngle = 180f - steepnessAngle;
//		}
//
//		if(steepnessAngle == 180)
//		{
//			steepnessAngle=0;
//		}
//
//		if(steepnessAngle > 90 && (wallSurface != m.expiredNormal)) //If the sliding surface is upside down, and hasn't already been clung to.
//		{
//			if(!phys.surfaceCling)
//			{
//				m.timeSpentHanging = 0;
//				m.maxTimeHanging = 0;
//				phys.surfaceCling = true;
//				if(phys.CGF >= m.clingReqGForce)
//				{
//					m.maxTimeHanging = m.surfaceClingTime;
//				}
//				else
//				{
//					m.maxTimeHanging = m.surfaceClingTime*(phys.CGF/m.clingReqGForce);
//				}
//				//print("m.maxTimeHanging="+m.maxTimeHanging);
//			}
//			else
//			{
//				m.timeSpentHanging += Time.fixedDeltaTime;
//				//print("time=("+m.timeSpentHanging+"/"+m.maxTimeHanging+")");
//				if(m.timeSpentHanging>=m.maxTimeHanging)
//				{
//					phys.surfaceCling = false;
//					m.expiredNormal = wallSurface;
//					//print("EXPIRED!");
//				}
//			}
//		}
//		else
//		{
//			phys.surfaceCling = false;
//			m.timeSpentHanging = 0;
//			m.maxTimeHanging = 0;
//		}
//
//		//print("Wall Steepness Angle:"+steepnessAngle);
//
//		///////////////////
//		// Movement code //
//		///////////////////
//
//		if(phys.surfaceCling)
//		{
//			if(m_Vel.y > 0)
//			{
//				m_Vel = ChangeSpeedLinear(m_Vel,-0.8f);
//			}
//			else if(m_Vel.y <= 0)
//			{
//				if( (horizontalInput<0 && phys.leftWalled) || (horizontalInput>0 && phys.rightWalled) )
//				{
//					m_Vel = ChangeSpeedLinear(m_Vel,0.1f);
//				}
//				else
//				{
//					m_Vel = ChangeSpeedLinear(m_Vel,1f);
//				}
//			}
//		}
//		else
//		{
//			if(m_Vel.y > 0)
//			{
//				if( (horizontalInput<0 && phys.leftWalled) || (horizontalInput>0 && phys.rightWalled) ) // If pressing key toward wall direction.
//				{
//					m_Vel.y -= 0.8f; //Decelerate slower.
//				}
//				else if((horizontalInput>0 && phys.leftWalled) || (horizontalInput<0 && phys.rightWalled)) // If pressing key opposite wall direction.
//				{
//					m_Vel.y -= 1.2f; //Decelerate faster.
//				}
//				else // If no input.
//				{
//					m_Vel.y -= 1f; 	//Decelerate.
//				}
//			}
//			else if(m_Vel.y <= 0)
//			{
//				if( (horizontalInput<0 && phys.leftWalled) || (horizontalInput>0 && phys.rightWalled) ) // If pressing key toward wall direction.
//				{
//					m_Vel.y -= 0.1f; //Accelerate downward slower.
//				}
//				else if((horizontalInput>0 && phys.leftWalled) || (horizontalInput<0 && phys.rightWalled)) // If pressing key opposite wall direction.
//				{
//					m_Vel.y -= 1.2f; //Accelerate downward faster.
//				}
//				else // If no input.
//				{
//					m_Vel.y -= 1f; 	//Accelerate downward.
//				}
//			}
//		}
//	}
//		
//	private void ToLeftWall(RaycastHit2D leftCheck) 
//	{ //Sets the new position of the player and their m_LeftNormal.
//
//		//print ("We've hit LeftWall, sir!!");
//		//print ("leftCheck.normal=" + leftCheck.normal);
//		//print("preleftwall Pos:" + this.transform.position);
//
//		if (phys.airborne)
//		{
//			print("Airborne before impact.");
//			m_Impact = true;
//			//m_Landing = true;
//		}
//
//		//m_Impact = true;
//		phys.leftWalled = true;
//		Vector2 setCharPos = leftCheck.point;
//		setCharPos.x += (m_LeftSideLength-m.minEmbed); //Embed slightly in wall to ensure raycasts still hit wall.
//		//setCharPos.y -= m.minEmbed;
//		//print("Sent to Pos:" + setCharPos);
//
//		this.transform.position = setCharPos;
//
//		//print ("Final Position:  " + this.transform.position);
//
//		RaycastHit2D leftCheck2 = Physics2D.Raycast(this.transform.position, Vector2.left, m_LeftSideLength, mask);
//		if (leftCheck2) 
//		{
////			if(antiTunneling){
////				Vector2 surfacePosition = leftCheck2.point;
////				surfacePosition.x += m_LeftSideLength-m.minEmbed;
////				//print("Sent to Pos:" + surfacePosition);
////				this.transform.position = surfacePosition;
////			}
//		}
//		else
//		{
//			phys.leftWalled = false;
//		}
//
//		m_LeftNormal = leftCheck2.normal;
//
//		if(phys.grounded)
//		{
//			print("LeftGroundWedge detected during left collision.");
//			OmniWedge(0,2);
//		}
//
//		if(phys.ceilinged)
//		{
//			print("LeftCeilingWedge detected during left collision.");
//			OmniWedge(2,1);
//		}
//
//		if(phys.rightWalled)
//		{
//			print("THERE'S PROBLEMS.");
//			//OmniWedge(2,3);
//		}
//
//		//print ("Final Position2:  " + this.transform.position);
//	}
//
//	private void ToRightWall(RaycastHit2D rightCheck) 
//	{ //Sets the new position of the player and their m_RightNormal.
//
//		print ("We've hit RightWall, sir!!");
//		//print ("groundCheck.normal=" + groundCheck.normal);
//		//print("prerightwall Pos:" + this.transform.position);
//
//		if (phys.airborne)
//		{
//			print("Airborne before impact.");
//			m_Impact = true;
//			//m_Landing = true;
//		}
//
//		//m_Impact = true;
//		phys.rightSideContact = true;
//		phys.rightWalled = true;
//		Vector2 setCharPos = rightCheck.point;
//		setCharPos.x -= (m_RightSideLength-m.minEmbed); //Embed slightly in wall to ensure raycasts still hit wall.
//		//setCharPos.y -= m.minEmbed;  //Embed slightly in ground to ensure raycasts still hit ground.
//
//		//print("Sent to Pos:" + setCharPos);
//		//print("Sent to normal:" + groundCheck.normal);
//
//		this.transform.position = setCharPos;
//
//		//print ("Final Position:  " + this.transform.position);
//
//		RaycastHit2D rightCheck2 = Physics2D.Raycast(this.transform.position, Vector2.right, m_RightSideLength, mask);
//		if (rightCheck2) 
//		{
////			if(antiTunneling){
////				Vector2 surfacePosition = rightCheck2.point;
////				surfacePosition.x -= (m_RightSideLength-m.minEmbed);
////				//print("Sent to Pos:" + surfacePosition);
////				this.transform.position = surfacePosition;
////			}
//		}
//		else
//		{
//			phys.rightWalled = false;
//		}
//
//		m_RightNormal = rightCheck2.normal;
//		//print ("Final Position2:  " + this.transform.position);
//
//		if(phys.grounded)
//		{
//			print("RightGroundWedge detected during right collision.");
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
//			print("RightCeilingWedge detected during right collision.");
//			OmniWedge(3,1);
//		}
//
//	}
//		
//	private void ToGround(RaycastHit2D groundCheck) 
//	{ //Sets the new position of the player and their ground normal.
//		print ("phys.grounded=" + phys.grounded);
//
//		if (phys.airborne)
//		{
//			print("Airborne before impact.");
//			m_Impact = true;
//			//m_Landing = true;
//		}
//
//		//m_Impact = true;
//		phys.grounded = true;
//		Vector2 setCharPos = groundCheck.point;
//		setCharPos.y = setCharPos.y+m_GroundFootLength-m.minEmbed; //Embed slightly in ground to ensure raycasts still hit ground.
//		this.transform.position = setCharPos;
//
//		//print("Sent to Pos:" + setCharPos);
//		//print("Sent to normal:" + groundCheck.normal);
//
//		RaycastHit2D groundCheck2 = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength, mask);
//		if (groundCheck2) 
//		{
////			if(antiTunneling){
////				Vector2 surfacePosition = groundCheck2.point;
////				surfacePosition.y += m_GroundFootLength-m.minEmbed;
////				this.transform.position = surfacePosition;
////				print("Antitunneling executed during impact.");
////			}
//		}
//		else
//		{
//			//print ("Impact Pos:  " + groundCheck.point);
//			//print("Reflected back into the air!");
//			//print("Transform position: " + this.transform.position);
//			//print("RB2D position: " + o.rigidbody2D.position);
//			//print("Velocity : " + m_Vel);
//			//print("Speed : " + m_Vel.magnitude);
//			//print(" ");
//			//print(" ");	
//			phys.grounded = false;
//		}
//
//		if(groundCheck.normal.y == 0f)
//		{//If vertical surface
//			//throw new Exception("Existence is suffering");
//			print("GtG VERTICAL :O");
//		}
//
//		m_GroundNormal = groundCheck2.normal;
//
//		if(phys.ceilinged)
//		{
//			print("CeilGroundWedge detected during ground collision.");
//			OmniWedge(0,1);
//		}
//
//		if(phys.leftWalled)
//		{
//			print("LeftGroundWedge detected during ground collision.");
//			OmniWedge(0,2);
//		}
//
//		if(phys.rightWalled)
//		{
//			print("RightGroundWedge detected during groundcollision.");
//			OmniWedge(0,3);
//		}
//
//		//print ("Final Position2:  " + this.transform.position);
//	}
//
//	private void ToCeiling(RaycastHit2D ceilingCheck) 
//	{ //Sets the new position of the player when they hit the ceiling.
//
//		//float testNumber = ceilingCheck.normal.y/ceilingCheck.normal.x;
//		//print(testNumber);
//		//print ("We've hit ceiling, sir!!");
//		//print ("ceilingCheck.normal=" + ceilingCheck.normal);
//
//		if (phys.airborne)
//		{
//			print("Airborne before impact.");
////			m_Landing = true;
//			m_Impact = true;
//		}
//
//		//m_Impact = true;
//		phys.ceilinged = true;
//		Vector2 setCharPos = ceilingCheck.point;
//		setCharPos.y -= (m_GroundFootLength-m.minEmbed); //Embed slightly in ceiling to ensure raycasts still hit ceiling.
//		this.transform.position = setCharPos;
//
//		RaycastHit2D ceilingCheck2 = Physics2D.Raycast(this.transform.position, Vector2.up, m_GroundFootLength, mask);
//		if (ceilingCheck2) 
//		{
////			if(antiTunneling){
////				Vector2 surfacePosition = ceilingCheck2.point;
////				surfacePosition.y -= (m_CeilingFootLength-m.minEmbed);
////				this.transform.position = surfacePosition;
////			}
//		}
//		else
//		{
//			print("Ceilinged = false?");
//			phys.ceilinged = false;
//		}
//
//		if(ceilingCheck.normal.y == 0f)
//		{//If vertical surface
//			//throw new Exception("Existence is suffering");
//			print("CEILING VERTICAL :O");
//		}
//
//		m_CeilingNormal = ceilingCheck2.normal;
//
//		if(phys.grounded)
//		{
//			print("CeilGroundWedge detected during ceiling collision.");
//			OmniWedge(0,1);
//		}
//
//		if(phys.leftWalled)
//		{
//			print("LeftCeilWedge detected during ceiling collision.");
//			OmniWedge(2,1);
//		}
//
//		if(phys.rightWalled)
//		{
//			print("RightGroundWedge detected during ceiling collision.");
//			OmniWedge(3,1);
//		}
//		//print ("Final Position2:  " + this.transform.position);
//	}
//
//	private Vector2 ChangeSpeedMult(Vector2 inputVelocity, float multiplier)
//	{
//		Vector2 newVelocity;
//		float speed = inputVelocity.magnitude*multiplier;
//		Vector2 direction = inputVelocity.normalized;
//		newVelocity = direction * speed;
//		return newVelocity;
//	}
//
//	private Vector2 ChangeSpeedLinear(Vector2 inputVelocity, float changeAmount)
//	{
//		Vector2 newVelocity;
//		float speed = inputVelocity.magnitude+changeAmount;
//		Vector2 direction = inputVelocity.normalized;
//		newVelocity = direction * speed;
//		return newVelocity;
//	}
//
//	private Vector2 SetSpeed(Vector2 inputVelocity, float speed)
//	{
//		//print("SetSpeed");
//		Vector2 newVelocity;
//		Vector2 direction = inputVelocity.normalized;
//		newVelocity = direction * speed;
//		return newVelocity;
//	}
//
//	private void DirectionChange(Vector2 newNormal)
//	{
//		//print("DirectionChange");
//		m.expiredNormal = new Vector2(0,0); //Used for wallslides. This resets the surface normal that wallcling is set to ignore.
//
//		Vector2 initialDirection = m_Vel.normalized;
//		Vector2 newPerp = Perp(newNormal);
//		Vector2 AdjustedVel;
//
//		float initialSpeed = m_Vel.magnitude;
//		//print("Speed before : " + initialSpeed);
//		float testNumber = newPerp.y/newPerp.x;
//		//print(testNumber);
//		//print("newPerp="+newPerp);
//		//print("initialDirection="+initialDirection);
//		if(float.IsNaN(testNumber))
//		{
//			print("IT'S NaN BRO LoLoLOL XD");
//			//throw new Exception("NaN value.");
//			//print("X = "+ newNormal.x +", Y = " + newNormal.y);
//		}
//
//		if((initialDirection == newPerp)||initialDirection == Vector2.zero)
//		{
//			//print("same angle");
//			return;
//		}
//
//		float impactAngle = Vector2.Angle(initialDirection,newPerp);
//		//print("TrueimpactAngle: " +impactAngle);
//		//print("InitialDirection: "+initialDirection);
//		//print("GroundDirection: "+newPerp);
//
//
//		impactAngle = (float)Math.Round(impactAngle,2);
//
//		if(impactAngle >= 180)
//		{
//			impactAngle = 180f - impactAngle;
//		}
//
//		if(impactAngle > 90)
//		{
//			impactAngle = 180f - impactAngle;
//		}
//
//		//print("impactAngle: " +impactAngle);
//
//		float projectionVal;
//		if(newPerp.sqrMagnitude == 0)
//		{
//			projectionVal = 0;
//		}
//		else
//		{
//			projectionVal = Vector2.Dot(m_Vel, newPerp)/newPerp.sqrMagnitude;
//		}
//
//
//		//print("P"+projectionVal);
//		AdjustedVel = newPerp * projectionVal;
//		//print("A"+AdjustedVel);
//
//		if(m_Vel == Vector2.zero)
//		{
//			//m_Vel = new Vector2(h, m_Vel.y);
//		}
//		else
//		{
//			try
//			{
//				m_Vel = AdjustedVel;
//				//print("m_Vel====>"+m_Vel);
//
//			}
//			catch(Exception e)
//			{
//				print(e);
//				print("newPerp="+newPerp);
//				print("projectionVal"+projectionVal);
//				print("adjustedVel"+AdjustedVel);
//			}
//		}
//
//		//Speed loss from impact angle handling beyond this point
//
//		float speedLossMult = 1; // The % of speed retained, based on sharpness of impact angle. A direct impact = full stop.
//
//		if(impactAngle <= m.impactDecelMinAngle)
//		{ // Angle lower than min, no speed penalty.
//			speedLossMult = 1;
//		}
//		else if(impactAngle < m.impactDecelMaxAngle)
//		{ // In the midrange, administering momentum loss on a curve leading from min to max.
//			speedLossMult = 1-Mathf.Pow((impactAngle-m.impactDecelMinAngle)/(m.impactDecelMaxAngle-m.impactDecelMinAngle),2); // See Workflowy notes section for details on this formula.
//		}
//		else
//		{ // Angle beyond max, momentum halted. 
//			speedLossMult = 0;
//		}
//
//		if(initialSpeed <= 2f)
//		{ // If the player is near stationary, do not remove any velocity because there is no impact!
//			speedLossMult = 1;
//		}
//
//		//print("SPLMLT " + speedLossMult);
//
//		m_Vel = SetSpeed(m_Vel, initialSpeed*speedLossMult);
//		//print("Final Vel " + m_Vel);
//		//print ("DirChange Vel:  " + m_Vel);
//	}
//
//	private void CameraControl()
//	{
//		#region zoom
//		v_CameraZoom = Mathf.Lerp(v_CameraZoom, m_Vel.magnitude, 0.1f);
//		//v_CameraZoom = m_Vel.magnitude;
//		float zoomChange = 0;
//		if((0.15f*v_CameraZoom)>=5f)
//		{
//			zoomChange = (0.15f*v_CameraZoom)-5f;
//		}
//		//o_MainCamera.orthographicSize = 5f+(0.15f*v_CameraZoom);
//		if(8f+zoomChange >= 50f)
//		{
//			o_MainCamera.orthographicSize = 50f;
//		}
//		else
//		{
//			o_MainCamera.orthographicSize = 8f+zoomChange;
//		}
//		//o_MainCamera.orthographicSize = 25f;
//
//		#endregion
//
//		#region position
//
//		#endregion
//	}
//	private void OmniWedge(int lowerContact, int upperContact)
//	{//Executes when the player is moving into a corner and there isn't enough room to fit them. It halts the player's momentum and sets off a blocked-direction flag.
//
//		//print("OmniWedge!");
//
//		RaycastHit2D lowerHit;
//		Vector2 lowerDirection = Vector2.down;
//		float lowerLength = m_GroundFootLength;
//
//		RaycastHit2D upperHit;
//		Vector2 upperDirection = Vector2.up;
//		float upperLength = m_CeilingFootLength;
//
//
//		switch(lowerContact)
//		{
//		case 0: //lowercontact is ground
//			{
//				lowerDirection = Vector2.down;
//				lowerLength = m_GroundFootLength;
//				break;
//			}
//		case 1: //lowercontact is ceiling
//			{
//				throw new Exception("ERROR: Ceiling cannot be lower contact.");
//				break;
//			}
//		case 2: //lowercontact is left
//			{
//				lowerDirection = Vector2.left;
//				lowerLength = m_LeftSideLength;
//				break;
//			}
//		case 3: //lowercontact is right
//			{
//				lowerDirection = Vector2.right;
//				lowerLength = m_RightSideLength;
//				break;
//			}
//		default:
//			{
//				throw new Exception("ERROR: DEFAULTED ON LOWERHIT.");
//				break;
//			}
//		}
//
//		lowerHit = Physics2D.Raycast(this.transform.position, lowerDirection, lowerLength, mask);
//
//		float embedDepth;
//		Vector2 gPerp; //lowerperp, aka groundperp
//		Vector2 cPerp; //upperperp, aka ceilingperp
//		Vector2 moveAmount = new Vector2(0,0);
//
//		if(!lowerHit)
//		{
//			//throw new Exception("Bottom not wedged!");
//			print("Bottom not wedged!");
//			gPerp.x = m_GroundNormal.x;
//			gPerp.y = m_GroundNormal.y;
//			return;
//		}
//		else
//		{
//			gPerp = Perp(lowerHit.normal);
//			Vector2 groundPosition = lowerHit.point;
//			if(lowerContact == 0) //ground contact
//			{
//				groundPosition.y += (m_GroundFootLength-m.minEmbed);
//			}
//			else if(lowerContact == 1) //ceiling contact
//			{
//				throw new Exception("CEILINGCOLLIDER CAN'T BE LOWER CONTACT");
//			}
//			else if(lowerContact == 2) //left contact
//			{
//				groundPosition.x += (m_LeftSideLength-m.minEmbed);
//			}
//			else if(lowerContact == 3) //right contact
//			{
//				groundPosition.x -= (m_RightSideLength-m.minEmbed);
//			}
//
//			this.transform.position = groundPosition;
//			//print("Hitting bottom, shifting up!");
//		}
//
//		switch(upperContact)
//		{
//		case 0: //uppercontact is ground
//			{
//				throw new Exception("FLOORCOLLIDER CAN'T BE UPPER CONTACT");
//				break;
//			}
//		case 1: //uppercontact is ceiling
//			{
//				upperDirection = Vector2.up;
//				upperLength = m_CeilingFootLength;
//				break;
//			}
//		case 2: //uppercontact is left
//			{
//				upperDirection = Vector2.left;
//				upperLength = m_LeftSideLength;
//				break;
//			}
//		case 3: //uppercontact is right
//			{
//				upperDirection = Vector2.right;
//				upperLength = m_RightSideLength;
//				break;
//			}
//		default:
//			{
//				throw new Exception("ERROR: DEFAULTED ON UPPERHIT.");
//				break;
//			}
//		}
//
//		upperHit = Physics2D.Raycast(this.transform.position, upperDirection, upperLength, mask);
//		embedDepth = upperLength-upperHit.distance;
//
//		if(!upperHit)
//		{
//			//throw new Exception("Top not wedged!");
//			cPerp = Perp(upperHit.normal);
//			print("Top not wedged!");
//			return;
//		}
//		else
//		{
//			//print("Hitting top, superunwedging..."); 
//			cPerp = Perp(upperHit.normal);
//		}
//			
//		//print("Embedded ("+embedDepth+") units into the ceiling");
//
//		float cornerAngle = Vector2.Angle(cPerp,gPerp);
//
//		//print("Ground Perp = " + gPerp);
//		//print("Ceiling Perp = " + cPerp);
//		//print("cornerAngle = " + cornerAngle);
//		bool convergingLeft = false;
//
//		Vector2 cPerpTest = cPerp;
//		Vector2 gPerpTest = gPerp;
//
//		if(cPerpTest.x < 0)
//		{
//			cPerpTest *= -1;
//		}
//		if(gPerpTest.x < 0)
//		{
//			gPerpTest *= -1;
//		}
//
//		//print("gPerpTest = " + gPerpTest);
//		//print("cPerpTest= " + cPerpTest);
//
//		float convergenceValue = cPerpTest.y-gPerpTest.y;
//
//		if(lowerContact == 2 || upperContact == 2){convergenceValue = 1;}; // THIS IS BAD, PLACEHOLDER CODE!
//		if(lowerContact == 3 || upperContact == 3){convergenceValue =-1;}; // THIS IS BAD, PLACEHOLDER CODE!
//
//		if(cornerAngle > 90f)
//		{
//			if(convergenceValue > 0)
//			{
//				moveAmount = SuperUnwedger(cPerp, gPerp, true, embedDepth);
//				//print("Left wedge!");
//				phys.leftWallBlocked = true;
//			}
//			else if(convergenceValue < 0)
//			{
//				moveAmount = SuperUnwedger(cPerp, gPerp, false, embedDepth);
//				//print("Right wedge!");
//				phys.rightWallBlocked = true;
//			}
//			else
//			{
//				throw new Exception("CONVERGENCE VALUE OF ZERO ON CORNER!");
//			}
//			m_Vel = new Vector2(0f, 0f);
//		}
//		else
//		{
//			moveAmount = (upperDirection*(-embedDepth));
//		}
//			
//		this.transform.position = new Vector2((this.transform.position.x + moveAmount.x), (this.transform.position.y + moveAmount.y));
//	}
//		
//	private Vector2	Perp(Vector2 input)
//	{
//		Vector2 output;
//		output.x = input.y;
//		output.y = -input.x;
//		return output;
//	}		
//
//	private void UpdateContactNormals(bool posCorrection)
//	{
//		phys.grounded = false;
//		phys.ceilinged = false;
//		phys.leftWalled = false;
//		phys.rightWalled = false;
//		phys.airborne = false;
//
//		phys.groundContact = false;
//		phys.ceilingContact = false;
//		phys.leftSideContact = false;
//		phys.rightSideContact = false;
//
//		//m_GroundNormal = new Vector2(0,0);
//		//m_CeilingNormal = new Vector2(0,0);
//		//m_LeftNormal = new Vector2(0,0);
//		//m_RightNormal = new Vector2(0,0);
//
//		m_GroundLine.endColor = Color.red;
//		m_GroundLine.startColor = Color.red;
//		m_CeilingLine.endColor = Color.red;
//		m_CeilingLine.startColor = Color.red;
//		m_LeftSideLine.endColor = Color.red;
//		m_LeftSideLine.startColor = Color.red;
//		m_RightSideLine.endColor = Color.red;
//		m_RightSideLine.startColor = Color.red;
//
//		RaycastHit2D[] directionContacts = new RaycastHit2D[4];
//		directionContacts[0] = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength, mask); 	// Ground
//		directionContacts[1] = Physics2D.Raycast(this.transform.position, Vector2.up, m_CeilingFootLength, mask);  	// Ceiling
//		directionContacts[2] = Physics2D.Raycast(this.transform.position, Vector2.left, m_LeftSideLength, mask); 	// Left
//		directionContacts[3] = Physics2D.Raycast(this.transform.position, Vector2.right, m_RightSideLength, mask);	// Right  
//
//		if (directionContacts[0]) 
//		{
//			phys.groundContact = true;
//			m_GroundLine.endColor = Color.green;
//			m_GroundLine.startColor = Color.green;
//			m_GroundNormal = directionContacts[0].normal;
//			phys.grounded = true;
//		} 
//			
//		if (directionContacts[1]) 
//		{
//			phys.ceilingContact = true;
//			m_CeilingLine.endColor = Color.green;
//			m_CeilingLine.startColor = Color.green;
//			m_CeilingNormal = directionContacts[1].normal;
//			phys.ceilinged = true;
//		} 
//
//
//		if (directionContacts[2])
//		{
//			m_LeftNormal = directionContacts[2].normal;
//			phys.leftSideContact = true;
//			m_LeftSideLine.endColor = Color.green;
//			m_LeftSideLine.startColor = Color.green;
//			phys.leftWalled = true;
//		} 
//
//		if (directionContacts[3])
//		{
//			m_RightNormal = directionContacts[3].normal;
//			phys.rightSideContact = true;
//			m_RightSideLine.endColor = Color.green;
//			m_RightSideLine.startColor = Color.green;
//			phys.rightWalled = true;
//		} 
//
//		if(!(phys.grounded&&phys.ceilinged))
//		{
//			if(!phys.rightWalled)
//			{
//				phys.rightWallBlocked = false;
//			}
//			if(!phys.leftWalled)
//			{
//				phys.leftWallBlocked = false;
//			}
//		}
//
//		if(antiTunneling&&posCorrection)
//		{
//			AntiTunneler(directionContacts);
//		}
//		if(!(phys.grounded||phys.ceilinged||phys.leftWalled||phys.rightWalled))
//		{
//			phys.airborne = true;	
//		}
//	}
//
//	private void AntiTunneler(RaycastHit2D[] contacts)
//	{
//		bool[] isEmbedded = {false, false, false, false};
//		int contactCount = 0;
//		if(phys.groundContact){contactCount++;}
//		if(phys.ceilingContact){contactCount++;}
//		if(phys.leftSideContact){contactCount++;}
//		if(phys.rightSideContact){contactCount++;}
//
//		int embedCount = 0;
//		if(phys.groundContact && ((m_GroundFootLength-contacts[0].distance)>=0.011f))	{isEmbedded[0]=true; embedCount++;} //If embedded too deep in this surface.
//		if(phys.ceilingContact && ((m_CeilingFootLength-contacts[1].distance)>=0.011f))	{isEmbedded[1]=true; embedCount++;} //If embedded too deep in this surface.
//		if(phys.leftSideContact && ((m_LeftSideLength-contacts[2].distance)>=0.011f))	{isEmbedded[2]=true; embedCount++;} //If embedded too deep in this surface.
//		if(phys.rightSideContact && ((m_RightSideLength-contacts[3].distance)>=0.011f))	{isEmbedded[3]=true; embedCount++;} //If embedded too deep in this surface.
//
//		switch(contactCount)
//		{
//			case 0: //No embedded contacts. Save this position as the most recent valid one and move on.
//			{
//				//print("No embedding! :)");
//				phys.lastSafePosition = this.transform.position;
//				break;
//			}
//			case 1: //One side is embedded. Simply push out to remove it.
//			{
//				if(isEmbedded[0])
//				{
//					Vector2 surfacePosition = contacts[0].point;
//					surfacePosition.y += (m_GroundFootLength-m.minEmbed);
//					this.transform.position = surfacePosition;
//				}
//				else if(isEmbedded[1])
//				{
//					Vector2 surfacePosition = contacts[1].point;
//					surfacePosition.y -= (m_CeilingFootLength-m.minEmbed);
//					this.transform.position = surfacePosition;
//				}
//				else if(isEmbedded[2])
//				{
//					Vector2 surfacePosition = contacts[2].point;
//					surfacePosition.x += ((m_LeftSideLength)-m.minEmbed);
//					this.transform.position = surfacePosition;
//				}
//				else if(isEmbedded[3])
//				{
//					Vector2 surfacePosition = contacts[3].point;
//					surfacePosition.x -= ((m_RightSideLength)-m.minEmbed);
//					this.transform.position = surfacePosition;
//				}
//				else
//				{
//					phys.lastSafePosition = this.transform.position;
//				}
//				break;
//			}
//			case 2: //Two sides are touching. Use the 2-point unwedging algorithm to resolve.
//			{
//				if(phys.groundContact&&phys.ceilingContact)
//				{
//					//if(m_GroundNormal != m_CeilingNormal)
//					{
//						print("Antitunneling omniwedge executed");			
//						OmniWedge(0,1);
//					}
//				}
//				else if(phys.groundContact&&phys.leftSideContact)
//				{
//					if(m_GroundNormal != m_LeftNormal)
//					{
//						OmniWedge(0,2);
//					}
//					else
//					{
//						//print("Same surface, 1-point unwedging.");
//						Vector2 surfacePosition = contacts[0].point;
//						surfacePosition.y += (m_GroundFootLength-m.minEmbed);
//						this.transform.position = surfacePosition;
//					}
//				}
//				else if(phys.groundContact&&phys.rightSideContact)
//				{
//					if(m_GroundNormal != m_RightNormal)
//					{
//						OmniWedge(0,3);
//					}
//					else
//					{
//						//print("Same surface, 1-point unwedging.");
//						Vector2 surfacePosition = contacts[0].point;
//						surfacePosition.y += (m_GroundFootLength-m.minEmbed);
//						this.transform.position = surfacePosition;
//					}
//				}
//				else if(phys.ceilingContact&&phys.leftSideContact)
//				{
//					//if(m_CeilingNormal != m_LeftNormal)
//					{
//						OmniWedge(2,1);
//					}
//				}
//				else if(phys.ceilingContact&&phys.rightSideContact)
//				{
//					//if(m_CeilingNormal != m_RightNormal)
//					{
//						OmniWedge(3,1);
//					}
//				}
//				else if(phys.leftSideContact&&phys.rightSideContact)
//				{
//					throw new Exception("Unhandled horizontal wedge detected.");
//					//OmniWedge(3,2);
//				}
//				break;
//			}
//			case 3: //Three sides are embedded. Not sure how to handle this yet besides reverting.
//			{
//				print("Triple Embed.");
//				break;
//			}
//			case 4:
//			{
//				print("FULL embedding! :C");
//				if(recoverFromFullEmbed)
//				{
//					this.transform.position = phys.lastSafePosition;
//				}
//				break;
//			}
//			default:
//			{
//				print("ERROR: DEFAULTED.");
//				break;
//			}
//		}
//
//	}
//
//	private Vector2 SuperUnwedger(Vector2 cPerp, Vector2 gPerp, bool cornerIsLeft, float embedDistance)
//	{
//		if(!cornerIsLeft)
//		{// Setting up variables	
//			//print("Resolving right wedge.");
//			if(gPerp.x>0)
//			{// Ensure both perpendicular vectors are pointing left, out of the corner the player is lodged in.
//				gPerp *= -1;
//			}
//
//			if(cPerp.x>0)
//			{// Ensure both perpendicular vectors are pointing left, out of the corner the player is lodged in.
//				cPerp *= -1;
//			}
//
//			if(cPerp.x != -1)
//			{// Multiply/Divide the top vector so that its x = -1.
//				if(cPerp.x == 0)
//				{
//					//print("You're unwedging from a wall, that's not allowed. Wall corners aren't wedgy enough.");
//					return new Vector2(0, -embedDistance);
//				}
//				else
//				{
//					cPerp /= Mathf.Abs(cPerp.x);
//				}
//			}
//
//			if(gPerp.x != -1)
//			{// Multiply/Divide the bottom vector so that its x = -1.
//				if(gPerp.x == 0)
//				{
//					//throw new Exception("Your ground has no horizontality. What are you even doing?");
//					return new Vector2(0, embedDistance);
//				}
//				else
//				{
//					gPerp /= Mathf.Abs(gPerp.x);
//				}
//			}
//		}
//		else
//		{
//			//print("Resolving left wedge.");
//			// Ensure both perpendicular vectors are pointing right, out of the corner the player is lodged in.
//			if(gPerp.x<0)
//			{
//				gPerp *= -1;
//			}
//
//			if(cPerp.x<0)
//			{// Ensure both perpendicular vectors are pointing left, out of the corner the player is lodged in.
//				cPerp *= -1;
//			}
//
//			if(cPerp.x != 1)
//			{// Multiply/Divide the top vector so that its x = -1.
//				if(cPerp.x == 0)
//				{
//					//throw new Exception("You're unwedging from a wall, that's not allowed. Wall corners aren't wedgy enough.");
//					return new Vector2(0, -embedDistance);
//				}
//				else
//				{
//					cPerp /= cPerp.x;
//				}
//			}
//
//			if(gPerp.x != -1)
//			{// Multiply/Divide the bottom vector so that its x = -1.
//				if(gPerp.x == 0)
//				{
//					//throw new Exception("Your ground has no horizontality. What are you even doing?");
//					return new Vector2(0, -embedDistance);
//				}
//				else
//				{
//					gPerp /= gPerp.x;
//				}
//			}
//		}
//			
//		//print("Adapted Ground Perp = " + gPerp);
//		//print("Adapted Ceiling Perp = " + cPerp);
//
//		//
//		// Now, the equation for repositioning a vertical line that is embedded in a corner:
//		//
//
//		float B = gPerp.y;
//		float A = cPerp.y;
//		float H = embedDistance; //Reduced so the player stays just embedded enough for the raycast to detect next frame.
//		float DivX;
//		float DivY;
//		float X;
//		float Y;
//
//		//print("(A, B)=("+ A +", "+ B +").");
//
//		if(B <= 0)
//		{
//			//print("B <= 0, using normal eqn.");
//			DivX = B-A;
//			DivY = -(DivX/B);
//		}
//		else
//		{
//			//print("B >= 0, using alternate eqn.");
//			DivX = 1f/(B-A);
//			DivY = -(A*DivX);
//		}
//
//		if(DivX != 0)
//		{
//			X = H/DivX;
//		}
//		else
//		{
//			X = 0;
//		}
//
//		if(DivY != 0)
//		{
//			Y = H/DivY;
//		}
//		else
//		{
//			Y = 0;
//		}
//
//		if((cornerIsLeft)&&(X<0))
//		{
//			X = -X;
//		}
//
//		if((!cornerIsLeft)&&(X>0))
//		{
//			X = -X;
//		}
//
//		//print("Adding the movement: ("+ X +", "+Y+").");
//		return new Vector2(X,Y); // Returns the distance the object must move to resolve wedging.
//	}
//
//	private void Jump(float horizontalInput)
//	{
//		if(phys.grounded&&phys.ceilinged)
//		{
//			print("Grounded and Ceilinged, nowhere to jump!");
//			i_JumpKey = false;
//		}
//		else if(phys.grounded)
//		{
//			if(m_Vel.y >= 0)
//			{
//				m_Vel = new Vector2(m_Vel.x+(m.hJumpForce*horizontalInput), m_Vel.y+m.vJumpForce);
//			}
//			else
//			{
//				m_Vel = new Vector2(m_Vel.x+(m.hJumpForce*horizontalInput), m.vJumpForce);
//			}
//			o_CharAudio.JumpSound();
//			i_JumpKey = false;
//		}
//		else if(phys.leftWalled)
//		{
//			print("Leftwalljumping!");
//			if(m_Vel.y < 0)
//			{
//				m_Vel = new Vector2(m.wallHJumpForce, m.wallVJumpForce);
//			}
//			else if(m_Vel.y <= (2*m.wallVJumpForce))
//			{
//				m_Vel = new Vector2(m.wallHJumpForce, m_Vel.y+m.wallVJumpForce);
//			}
//			else
//			{
//				m_Vel = new Vector2(m.wallHJumpForce, m_Vel.y);
//			}
//			o_CharAudio.JumpSound();
//			i_JumpKey = false;
//			phys.leftWalled = false;
//		}
//		else if(phys.rightWalled)
//		{
//			print("Rightwalljumping!");
//			if(m_Vel.y < 0)
//			{
//				m_Vel = new Vector2(-m.wallHJumpForce, m.wallVJumpForce);
//			}
//			else if(m_Vel.y <= m.wallVJumpForce)
//			{
//				m_Vel = new Vector2(-m.wallHJumpForce, m_Vel.y+m.wallVJumpForce);
//			}
//			else
//			{
//				m_Vel = new Vector2(-m.wallHJumpForce, m_Vel.y);
//			}
//
//			o_CharAudio.JumpSound();
//			i_JumpKey = false;
//			phys.rightWalled = false;
//		}
//		else if(phys.ceilinged)
//		{
//			if(m_Vel.y <= 0)
//			{
//				m_Vel = new Vector2(m_Vel.x+(m.hJumpForce*horizontalInput), m_Vel.y -m.vJumpForce);
//			}
//			else
//			{
//				m_Vel = new Vector2(m_Vel.x+(m.hJumpForce*horizontalInput), -m.vJumpForce);
//			}
//			o_CharAudio.JumpSound();
//			i_JumpKey = false;
//			phys.ceilinged = false;
//		}
//		else
//		{
//			//print("Can't jump, airborne!");
//		}
//	}
//
//	private void EtherJump(Vector2 jumpNormal)
//	{
//		g_EtherJumpCharge = o_Spooler.GetTotalPower();
//		m_Vel = jumpNormal*(m.etherJumpForceBase+(m.etherJumpForcePerCharge*g_EtherJumpCharge));
//		g_EtherJumpCharge = 0;		
//		i_JumpKey = false;
//		o_CharAudio.JumpSound();
//		o_Spooler.Reset();
//	}
//
//
//
//	#endregion
//	//###################################################################################################################################
//	// PUBLIC FUNCTIONS
//	//###################################################################################################################################
//	#region PUBLIC FUNCTIONS
//	public float GetInstantGForce()
//	{
//		return phys.IGF;
//	}
//
//	public float GetContinuousGForce()
//	{
//		return phys.CGF;
//	}
//
//	public Vector2 GetVelocity()
//	{
//		return m_Vel;
//	}
//
//	public float GetSpeed()
//	{
//		return m_Spd;
//	}
//
//	public void SetEtherLevel(int EtherLevel)
//	{
//		v_EtherLevel = EtherLevel;
//	}
//
//	public int GetEtherLevel()
//	{
//		return v_EtherLevel;
//	}
//
//	public int GetEtherStance()
//	{
//		return g_EtherStance; //-1 is none, 0 is kneeling, 1 is AD.
//	}
//
//	public void DissipateEther()
//	{
//		// Executes when the player leaves Ether stance without using power.
//	}
//
//	#endregion
//}