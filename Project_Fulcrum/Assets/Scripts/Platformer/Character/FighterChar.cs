using UnityEngine.UI;
using System;
using UnityEngine;
using EZCameraShake;

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
public class FighterChar : MonoBehaviour
{        	
	//############################################################################################################################################################################################################
	// HANDLING VARIABLES
	//###########################################################################################################################################################################
	#region MOVEMENT HANDLING
	[Header("Movement Tuning:")]
	[SerializeField] protected float m_MinSpeed = 10f; 							// The instant starting speed while moving
	[SerializeField] protected float m_MaxRunSpeed = 200f;						// The fastest the player can travel along land.
	[Range(0,2)][SerializeField] protected float m_Acceleration = 0.6f;    			// Speed the player accelerates at
	[SerializeField] protected float m_VJumpForce = 40f;                  		// Amount of vertical force added when the player jumps.
	[SerializeField] protected float m_HJumpForce = 5f;  							// Amount of horizontal force added when the player jumps.
	[SerializeField] protected float m_WallVJumpForce = 20f;                  	// Amount of vertical force added when the player walljumps.
	[SerializeField] protected float m_WallHJumpForce = 10f;  					// Amount of horizontal force added when the player walljumps.
	[SerializeField] protected float m_TractionChangeT = 20f;						// Threshold where movement changes from exponential to linear acceleration.  
	[SerializeField] protected float m_WallTractionT = 20f;						// Speed threshold at which wallsliding traction changes.
	[Range(0,5)][SerializeField] protected float m_LinearStopRate = 2f; 			// How fast the player decelerates when changing direction.
	[Range(0,5)][SerializeField] protected float m_LinearSlideRate = 0.35f;		// How fast the player decelerates with no input.
	[Range(0,5)][SerializeField] protected float m_LinearOverSpeedRate = 0.1f;	// How fast the player decelerates when running too fast.
	[Range(0,5)][SerializeField] protected float m_LinearAccelRate = 0.4f;		// How fast the player accelerates with input.
	[Range(1,89)][SerializeField] protected float m_ImpactDecelMinAngle = 20f;	// Any impacts at sharper angles than this will start to slow the player down. Reaches full halt at m_ImpactDecelMaxAngle.
	[Range(1,89)][SerializeField] protected float m_ImpactDecelMaxAngle = 80f;	// Any impacts at sharper angles than this will result in a full halt. DO NOT SET THIS LOWER THAN m_ImpactDecelMinAngle!!
	[Range(1,89)][SerializeField] protected float m_TractionLossMinAngle = 45f; 	// Changes the angle at which steeper angles start to linearly lose traction, and eventually starts slipping back down. Default of 45 degrees.
	[Range(45,90)][SerializeField] protected float m_TractionLossMaxAngle = 78f; 	// Changes the angle at which player loses ALL traction, and starts slipping back down. Default of 90 degrees.
	[Range(0,2)][SerializeField] protected float m_SlippingAcceleration = 1f;  	// Changes how fast the player slides down overly steep slopes.
	[Range(0.5f,3)][SerializeField] protected float m_SurfaceClingTime = 1f; 		// How long the player can cling to walls before gravity takes over.
	[Range(20,70)][SerializeField] protected float m_ClingReqGForce = 50f;		// This is the amount of impact GForce required for a full-duration ceiling cling.
	[ReadOnlyAttribute]protected Vector2 m_ExpiredNormal;							// This is the normal of the last surface clung to, to make sure the player doesn't repeatedly cling the same surface after clingtime expires.
	[ReadOnlyAttribute]protected float m_TimeSpentHanging = 0f;					// Amount of time the player has been clung to a wall.
	[ReadOnlyAttribute]protected float m_MaxTimeHanging = 0f;						// Max time the player can cling to current wall.
	[Range(0,0.5f)][SerializeField] protected float m_MaxEmbed = 0.02f;			// How deep into objects the character can be before actually colliding with them. MUST BE GREATER THAN m_MinEmbed!!!
	[Range(0.01f,0.4f)][SerializeField] protected float m_MinEmbed = 0.01f; 		// How deep into objects the character will sit by default. A value of zero will cause physics errors because the player is not technically *touching* the surface.
	[Space(10)]
	[SerializeField] protected float m_ZonJumpForcePerCharge = 5f; 				// How much force does each Zon Charge add to the jump power?
	[SerializeField] protected float m_ZonJumpForceBase = 40f; 					// How much force does a no-power Zon jump have?
	[Space(10)]
	[SerializeField] protected float m_SlamT = 100f; 								// Impact threshold for slam
	[SerializeField] protected float m_CraterT = 200f; 							// Impact threshold for crater
	[SerializeField] protected float m_GuardSlamT = 200f; 						// Guarded Impact threshold for slam
	[SerializeField] protected float m_GuardCraterT = 400f; 						// Guarded Impact threshold for crater


	#endregion
	//############################################################################################################################################################################################################
	// OBJECT REFERENCES
	//###########################################################################################################################################################################
	#region OBJECT REFERENCES
	[Header("Player Components:")]

	//[SerializeField] protected Text o_Speedometer;      		// Reference to the speed indicator (dev tool).
	[SerializeField] protected Light o_TempLight;      		// Reference to a spotlight attached to the character.
	//[SerializeField] protected Camera o_MainCamera;			// Reference to the main camera.
	//[SerializeField] protected CameraShaker o_CamShaker;		// Reference to the main camera's shaking controller.
	[SerializeField] protected GameObject o_CharSprite;		// Reference to the character's sprite.
	[SerializeField] public FighterAudio o_FighterAudio;		// Reference to the character's audio handler.
	//[SerializeField] public Spooler o_Spooler;				// Reference to the character's spooler object, which handles power charging gameplay.
	[SerializeField] public GameObject p_DebugMarker;		// Reference to a sprite prefab used to mark locations ingame during development.
	//[SerializeField] public Healthbar o_Healthbar;			// Reference to the Healthbar UI element.
	protected Animator o_Anim;           						// Reference to the character's animator component.
	protected Rigidbody2D o_Rigidbody2D;						// Reference to the character's physics body.
	protected SpriteRenderer o_SpriteRenderer;				// Reference to the character's sprite renderer.
	#endregion
	//############################################################################################################################################################################################################
	// PHYSICS&RAYCASTING
	//###########################################################################################################################################################################
	#region PHYSICS&RAYCASTING
	[SerializeField] protected LayerMask mask;// A mask determining what collides with the character.

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

	[Header("Player State:")]
	protected Vector3 lastSafePosition;										//Used to revert player position if they get totally stuck in something.
	[SerializeField][ReadOnlyAttribute]protected float m_IGF; 				//"Instant G-Force" of the impact this frame.
	[SerializeField][ReadOnlyAttribute]protected float m_CGF; 				//"Continuous G-Force" over time.
	[SerializeField][ReadOnlyAttribute]protected float m_RemainingVelM;		//Remaining velocity proportion after an impact. Range: 0-1.
	[SerializeField][ReadOnlyAttribute]public float m_Spd;					//Current speed.
	[SerializeField][ReadOnlyAttribute]public Vector2 m_Vel;				//Current (x,y) velocity.
	[SerializeField][ReadOnlyAttribute]protected Vector2 m_RemainingMovement; //Remaining (x,y) movement after impact.
	[SerializeField][ReadOnlyAttribute]protected bool groundContact;			//True when touching surface.
	[SerializeField][ReadOnlyAttribute]protected bool ceilingContact;			//True when touching surface.
	[SerializeField][ReadOnlyAttribute]protected bool leftSideContact;		//True when touching surface.
	[SerializeField][ReadOnlyAttribute]protected bool rightSideContact;		
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

	#endregion
	//##########################################################################################################################################################################
	// PLAYER INPUT VARIABLES
	//###########################################################################################################################################################################
	#region PLAYERINPUT
	protected bool i_JumpKey;
	protected bool i_LeftClick;
	protected bool i_RightClick;
	protected bool i_LeftKey;
	protected bool i_RightKey;
	protected bool i_UpKey;
	protected bool i_DownKey;
	protected bool i_ZonKey;
	protected int CtrlH; 					// Tracks horizontal keys pressed. Values are -1 (left), 0 (none), or 1 (right). 
	protected int CtrlV; 					// Tracks vertical keys pressed. Values are -1 (down), 0 (none), or 1 (up).
	protected bool facingDirection; 		// True means right, false means left.
	protected Vector2 i_MouseWorldPos;	// Mouse position in world coordinates.
	protected Vector2 i_PlayerMouseVector;// Vector pointing from the player to their mouse position.
	#endregion
	//############################################################################################################################################################################################################
	// DEBUGGING VARIABLES
	//##########################################################################################################################################################################
	#region DEBUGGING
	protected int errorDetectingRecursionCount; 				//Iterates each time recursive trajectory correction executes on the current frame. Not currently used.
	[Header("Debug:")]
	[SerializeField] protected bool autoRunLeft; 				// When true, player will behave as if the left key is pressed.
	[SerializeField] protected bool autoRunRight; 			// When true, player will behave as if the right key is pressed.
	[SerializeField] protected bool autoJump;					// When true, player jumps instantly on every surface.
	[SerializeField] protected bool antiTunneling = true;		// When true, player will be pushed out of objects they are stuck in.
	[SerializeField] protected bool noGravity;				// Disable gravity.
	[SerializeField] protected bool showVelocityIndicator;	// Shows a line tracing the character's movement path.
	[SerializeField] protected bool showContactIndicators;	// Shows player's surface-contact raycasts, which turn green when touching something.
	[SerializeField] protected bool recoverFromFullEmbed=true;// When true and the player is fully stuck in something, teleports player to last good position.
	[SerializeField] protected bool d_ClickToKnockPlayer;		// When true and you left click, the player is propelled toward where you clicked.
	[SerializeField] public  bool d_DevMode;				// Turns on all dev cheats.
	protected LineRenderer m_DebugLine; 						// Part of above indicators.
	protected LineRenderer m_GroundLine;						// Part of above indicators.		
	protected LineRenderer m_CeilingLine;						// Part of above indicators.		
	protected LineRenderer m_LeftSideLine;					// Part of above indicators.		
	protected LineRenderer m_RightSideLine;					// Part of above indicators.		
	#endregion
	//############################################################################################################################################################################################################
	// VISUAL&SOUND VARIABLES
	//###########################################################################################################################################################################
	#region VISUALS&SOUND
	[Header("Visuals And Sound:")]
	[SerializeField]protected int v_ZonLevel;							//	Level of player Zon Power.
	[SerializeField][Range(0,10)]protected float v_ReversingSlideT =5;// How fast the player must be going to go into a slide posture when changing directions.
	//[SerializeField]protected float v_CameraZoom; 					// Amount of camera zoom.
	[SerializeField][Range(0,3)]protected int v_PlayerGlow;			// Amount of player "energy glow" effect.
	#endregion 
	//############################################################################################################################################################################################################
	// GAMEPLAY VARIABLES
	//###########################################################################################################################################################################
	#region GAMEPLAY VARIABLES
	[Header("Gameplay:")]
	[SerializeField]protected int g_ZonJumpCharge;					//	Level of power channelled into current jump.
	[SerializeField][ReadOnlyAttribute] protected int g_ZonStance;	// Which stance is the player in? -1 = no stance.
	[SerializeField] protected int g_CurHealth = 100;					// Current health.
	[SerializeField] protected int g_MaxHealth = 100;					// Max health.
	[SerializeField] protected int g_MinSlamDMG = 5;					// Min damage a slam impact can deal.
	[SerializeField] protected int g_MaxSlamDMG = 30;					// Max damage a slam impact can deal.	
	[SerializeField] protected int g_MinCrtrDMG = 30;					// Min damage a crater impact can deal.
	[SerializeField] protected int g_MaxCrtrDMG = 60;					// Max damage a crater impact can deal.
	#endregion 


	//########################################################################################################################################
	// CORE FUNCTIONS
	//########################################################################################################################################
	#region CORE FUNCTIONS
	protected void Awake()
	{
		Vector2 playerOrigin = new Vector2(this.transform.position.x, this.transform.position.y);
		m_DebugLine = GetComponent<LineRenderer>();

		m_GroundFoot = transform.Find("MidFoot");
		m_GroundLine = m_GroundFoot.GetComponent<LineRenderer>();
		m_GroundFootOffset.x = m_GroundFoot.position.x-playerOrigin.x;
		m_GroundFootOffset.y = m_GroundFoot.position.y-playerOrigin.y;
		m_GroundFootLength = m_GroundFootOffset.magnitude;

		m_CeilingFoot = transform.Find("CeilingFoot");
		m_CeilingLine = m_CeilingFoot.GetComponent<LineRenderer>();
		m_CeilingFootOffset.x = m_CeilingFoot.position.x-playerOrigin.x;
		m_CeilingFootOffset.y = m_CeilingFoot.position.y-playerOrigin.y;
		m_CeilingFootLength = m_CeilingFootOffset.magnitude;

		m_LeftSide = transform.Find("LeftSide");
		m_LeftSideLine = m_LeftSide.GetComponent<LineRenderer>();
		m_LeftSideOffset.x = m_LeftSide.position.x-playerOrigin.x;
		m_LeftSideOffset.y = m_LeftSide.position.y-playerOrigin.y;
		m_LeftSideLength = m_LeftSideOffset.magnitude;

		m_RightSide = transform.Find("RightSide");
		m_RightSideLine = m_RightSide.GetComponent<LineRenderer>();
		m_RightSideOffset.x = m_RightSide.position.x-playerOrigin.x;
		m_RightSideOffset.y = m_RightSide.position.y-playerOrigin.y;
		m_RightSideLength = m_RightSideOffset.magnitude;


		o_Anim = o_CharSprite.GetComponent<Animator>();
		o_Rigidbody2D = GetComponent<Rigidbody2D>();
		o_SpriteRenderer = o_CharSprite.GetComponent<SpriteRenderer>();

		lastSafePosition = new Vector2(0,0);
		m_RemainingMovement = new Vector2(0,0);
		m_RemainingVelM = 1f;
		//print(m_RemainingMovement);


		if(!(showVelocityIndicator||d_DevMode)){
			m_DebugLine.enabled = false;
		}

		if(!(showContactIndicators||d_DevMode))
		{
			m_CeilingLine.enabled = false;
			m_GroundLine.enabled = false;
			m_RightSideLine.enabled = false;
			m_LeftSideLine.enabled = false;
		}
	}

	protected void FixedUpdate()
	{
		Vector2 finalPos = new Vector2(this.transform.position.x+m_RemainingMovement.x, this.transform.position.y+m_RemainingMovement.y);
		this.transform.position = finalPos;
		print("fixedupdate on fighterchar activating!");
		UpdateContactNormals(true);

		Vector2 initialVel = m_Vel;
		i_PlayerMouseVector = i_MouseWorldPos-Vec2(this.transform.position);

		m_Impact = false;
		m_Landing = false;
		m_Kneeling = false;
		g_ZonStance = -1;

		//print("Initial Pos: " + startingPos);
		//print("Initial Vel: " +  m_Vel);


		#region playerinput
		/*
		if(!(i_LeftKey||i_RightKey) || (i_LeftKey && i_RightKey))
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
		else if(i_LeftKey)
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
		if(i_DownKey&&m_Grounded)
		{
			m_Kneeling = true;
			CtrlH = 0;
			g_ZonStance = 0; // Kneeling stance.
		}
		else
		{
			g_ZonJumpCharge=0;
		}

		if(0==1)//(i_JumpKey)
		{
			if(m_Kneeling)
			{
				ZonJump(i_PlayerMouseVector.normalized);
			}
			else
			{
				Jump(CtrlH);
			}
		}
		*/
		if(i_LeftClick&&(d_DevMode||d_ClickToKnockPlayer))
		{
			m_Vel += i_PlayerMouseVector*10;
			print("Leftclick detected");
			i_LeftClick = false;
		}	
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

		#endregion


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
			m_Vel = new Vector2 (m_Vel.x, m_Vel.y - 1);
			m_Ceilinged = false;
		}


		errorDetectingRecursionCount = 0; //Used for Collizion();

		//print("Velocity before Coll ision: "+m_Vel);
		//print("Position before Coll ision: "+this.transform.position);

		m_RemainingVelM = 1f;
		m_RemainingMovement = m_Vel*Time.fixedDeltaTime;
		Vector2 startingPos = this.transform.position;

		//print("m_RemainingMovement before collision: "+m_RemainingMovement);

		Collision();

		//print("Per frame velocity at end of Collizion() "+m_Vel*Time.fixedDeltaTime);
		//print("Velocity at end of Collizion() "+m_Vel);
		//print("Per frame velocity at end of updatecontactnormals "+m_Vel*Time.fixedDeltaTime);
		//print("m_RemainingMovement after collision: "+m_RemainingMovement);

		Vector2 distanceTravelled = new Vector2(this.transform.position.x-startingPos.x,this.transform.position.y-startingPos.y);
		//print("distanceTravelled: "+distanceTravelled);
		//print("m_RemainingMovement: "+m_RemainingMovement);
		//print("m_RemainingMovement after removing distancetravelled: "+m_RemainingMovement);

		if(initialVel.magnitude>0)
		{
			m_RemainingVelM = (((initialVel.magnitude*Time.fixedDeltaTime)-distanceTravelled.magnitude)/(initialVel.magnitude*Time.fixedDeltaTime));
		}
		else
		{
			m_RemainingVelM = 1f;
		}

		//print("m_RemainingVelM: "+m_RemainingVelM);
		//print("movement after distance travelled: "+m_RemainingMovement);
		//print("Speed this frame: "+m_Vel.magnitude);

		m_RemainingMovement = m_Vel*m_RemainingVelM*Time.fixedDeltaTime;

		//print("Corrected remaining movement: "+m_RemainingMovement);

		m_Spd = m_Vel.magnitude;

		Vector2 deltaV = m_Vel-initialVel;
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
				Crater();
			}
			else if(m_IGF >= m_SlamT)
			{
				Slam();
			}
			else
			{
				o_FighterAudio.LandingSound(m_IGF);
			}
		}
		//print("Per frame velocity at end of physics frame: "+m_Vel*Time.fixedDeltaTime);
		//print("m_RemainingMovement at end of physics frame: "+m_RemainingMovement);
		//print("Pos at end of physics frame: "+this.transform.position);
		//print("##############################################################################################");
		//print("FinaL Pos: " + this.transform.position);
		//print("FinaL Vel: " + m_Vel);
		//print("Speed at end of frame: " + m_Vel.magnitude);

		//		#region audio
		//		if(m_Landing)
		//		{
		//			o_FighterAudio.LandingSound(m_IGF); // Makes a landing sound when the player hits ground, using the impact force to determine loudness.
		//		}
		//		#endregion

		#region Animator

		//
		//Animator Controls
		//

		v_PlayerGlow = v_ZonLevel;
		if (v_PlayerGlow > 7){v_PlayerGlow = 7;}

		if(v_PlayerGlow>2)
		{
			o_TempLight.color = new Color(1,1,0,1);
			o_TempLight.intensity = (v_PlayerGlow)+(UnityEngine.Random.Range(-1f,1f));
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
			o_CharSprite.transform.localPosition = new Vector3(0.13f, 0f,0f);
		}

		if(m_RightWalled&&!m_Grounded)
		{
			o_Anim.SetBool("Walled", true);
			facingDirection = true;
			o_CharSprite.transform.localPosition = new Vector3(-0.13f, 0f,0f);
		}

		if(m_Grounded || !(m_RightWalled||m_LeftWalled))
		{
			o_CharSprite.transform.localPosition = new Vector3(0f,0f,0f);
		}

		if (!facingDirection) //If facing left
		{
			//print("FACING LEFT!   "+h)
			o_CharSprite.transform.localScale = new Vector3 (-1f, 1f, 1f);
			if(m_Vel.x > 0 && m_Spd >= v_ReversingSlideT)
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

			o_CharSprite.transform.localScale = new Vector3 (1f, 1f, 1f);
			if(m_Vel.x < 0 && m_Spd >= v_ReversingSlideT)
			{
				o_Anim.SetBool("Crouch", true);
			}
			else
			{
				o_Anim.SetBool("Crouch", false);
			}
		}

		if(m_Kneeling)
		{
			o_Anim.SetBool("Crouch", true);

			/*
			if((i_MouseWorldPos.x-this.transform.position.x)<0)
			{
				facingDirection = false;
				o_CharSprite.transform.localScale = new Vector3 (-1f, 1f, 1f);
			}
			else
			{
				facingDirection = true;
				o_CharSprite.transform.localScale = new Vector3 (1f, 1f, 1f);
			}
			*/
		}

		Vector3[] debugLineVector = new Vector3[3];

		debugLineVector[0].x = -distanceTravelled.x;
		debugLineVector[0].y = -(distanceTravelled.y+(m_GroundFootLength-m_MaxEmbed));
		debugLineVector[0].z = 0f;

		debugLineVector[1].x = 0f;
		debugLineVector[1].y = -(m_GroundFootLength-m_MaxEmbed);
		debugLineVector[1].z = 0f;

		debugLineVector[2].x = m_RemainingMovement.x;
		debugLineVector[2].y = (m_RemainingMovement.y)-(m_GroundFootLength-m_MaxEmbed);
		debugLineVector[2].z = 0f;

		m_DebugLine.SetPositions(debugLineVector);

		o_Anim.SetFloat("Speed", m_Vel.magnitude);

		if(m_Vel.magnitude >= m_TractionChangeT )
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

		if(m_Vel.magnitude > 20.0f)
		{
			multiplier = ((m_Vel.magnitude - 20) / 20)+1;
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
		#endregion

		i_RightClick = false;
		i_LeftClick = false;
		//i_ZonKey = false;

	}

	protected void Update()
	{
		
		if(Input.GetMouseButtonDown(0))
		{
			i_LeftClick = true;
		}

		if(Input.GetMouseButtonDown(1))
		{
			//i_RightClick = true;
		}
		/*
		if(Input.GetButtonDown("Spooling"))
		{
			//i_ZonKey = true;				
		}
		*/
		Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		i_MouseWorldPos = Vec2(mousePoint);
		/*
		//i_LeftKey = Input.GetButton("Left");
		i_RightKey = Input.GetButton("Right");
		i_UpKey = Input.GetButton("Up");
		i_DownKey = Input.GetButton("Down");

		if(o_Speedometer != null)
		{
			o_Speedometer.text = ""+Math.Round(m_Vel.magnitude,0);
		}

		if(0==1) //(!i_JumpKey && (m_Grounded||m_Ceilinged||m_LeftWalled||m_RightWalled))
		{
			// Read the jump input in Update so button presses aren't missed.

			//i_JumpKey = Input.GetButtonDown("Jump");
			if(autoJump)
			{
				//i_JumpKey = true;
			}
		}
		*/
	}

	protected void LateUpdate()
	{
		//CameraControl();
	}
	#endregion
	//###################################################################################################################################
	// CUSTOM FUNCTIONS
	//###################################################################################################################################
	#region CUSTOM FUNCTIONS

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

		float damagedealt = g_MinCrtrDMG+((g_MaxCrtrDMG-g_MinCrtrDMG)*((m_IGF-m_CraterT)/(1000f-m_CraterT))); // Damage dealt scales linearly from minDMG to maxDMG, reaching max damage at a 1000 kph impact.
		g_CurHealth -= (int)damagedealt;
		if(g_CurHealth < 0){g_CurHealth = 100;}
		//o_Healthbar.SetCurHealth(g_CurHealth);
		print("CRATERED!");
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
		print("SLAMMIN!");

		o_FighterAudio.SlamSound(m_IGF, m_SlamT, m_CraterT);

		float damagedealt = g_MinSlamDMG+((g_MaxSlamDMG-g_MinSlamDMG)*((m_IGF-m_SlamT)/(m_CraterT-m_SlamT))); // Damage dealt scales linearly from minDMG to maxDMG, as you go from the min Slam Threshold to min Crater Threshold (impact speed)
		g_CurHealth -= (int)damagedealt;
		if(g_CurHealth < 0){g_CurHealth = 100;}
		//o_Healthbar.SetCurHealth(g_CurHealth);
	}

	protected void Collision()	// Handles all collisions with terrain geometry.
	{

		//print ("Collision->m_Grounded=" + m_Grounded);
		float crntSpeed = m_Vel.magnitude*Time.fixedDeltaTime; //Current speed.
		//print("DC Executing");
		errorDetectingRecursionCount++;

		if(errorDetectingRecursionCount >= 5)
		{
			throw new Exception("Your recursion code is not working!");
			//return;
		}

		if(m_Vel.x > 0.001f)
		{
			m_LeftWallBlocked = false;
		}

		if(m_Vel.x < -0.001f)
		{
			m_RightWallBlocked = false;
		}

		#region collision raytesting

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

		predictedLoc[0] = Physics2D.Raycast(adjustedBot, m_Vel, crntSpeed, mask); 	// Ground
		predictedLoc[1] = Physics2D.Raycast(adjustedTop, m_Vel, crntSpeed, mask); 	// Ceiling
		predictedLoc[2] = Physics2D.Raycast(adjustedLeft, m_Vel, crntSpeed, mask); // Left
		predictedLoc[3] = Physics2D.Raycast(adjustedRight, m_Vel, crntSpeed, mask);// Right  

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

		//non-zero shortest of all four colliders.
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

		Vector2 moveDirectionNormal = Perp(m_Vel.normalized);
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
				print("FOOT_IMPACT");
				//print("Velocity before impact: "+m_Vel);

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
					if(invertedDirectionNormal == predictedLoc[0].normal)
					{
						throw new Exception("INVERTED GROUND IMPACT NORMAL DETECTED!");
					}
					return;
				}
				break;
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
					if(invertedDirectionNormal == predictedLoc[1].normal)
					{
						throw new Exception("INVERTED CEILING IMPACT NORMAL DETECTED!");
					}
					return;
				}
				break;
			}
		case 2:
			{
				if ((moveDirectionNormal != predictedLoc[2].normal) && (invertedDirectionNormal != predictedLoc[2].normal)) 
				{ // If the slope you're hitting is different than your current slope.
					print("LEFT_IMPACT");
					ToLeftWall(predictedLoc[2]);
					DirectionChange(m_LeftNormal);
					return;
				}
				else 
				{
					if(invertedDirectionNormal == predictedLoc[2].normal)
					{
						throw new Exception("INVERTED LEFT IMPACT NORMAL DETECTED!");
					}
					return;
				}
				break;
			}
		case 3:
			{
				if ((moveDirectionNormal != predictedLoc[3].normal) && (invertedDirectionNormal != predictedLoc[3].normal)) 
				{ // If the slope you're hitting is different than your current slope.
					//print("RIGHT_IMPACT");
					//print("predictedLoc[3].normal=("+predictedLoc[3].normal.x+","+predictedLoc[3].normal.y+")");
					//print("moveDirectionNormal=("+moveDirectionNormal.x+","+moveDirectionNormal.y+")");
					//print("moveDirectionNormal="+moveDirectionNormal);
					ToRightWall(predictedLoc[3]);
					DirectionChange(m_RightNormal);
					return;
				}
				else 
				{
					if(invertedDirectionNormal == predictedLoc[3].normal)
					{
						throw new Exception("INVERTED RIGHT IMPACT NORMAL DETECTED!");
					}
					return;
				}
				break;
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
		float rawSpeed = m_Vel.magnitude;
		//print("m_Vel.magnitude"+m_Vel.magnitude);

		if (horizontalInput == 0) 
		{//if not pressing any move direction, slow to zero linearly.
			//print("No input, slowing...");
			if(rawSpeed <= 0.5f)
			{
				m_Vel = Vector2.zero;	
			}
			else
			{
				m_Vel = ChangeSpeedLinear (m_Vel, -m_LinearSlideRate);
			}
		}
		else if((horizontalInput > 0 && m_Vel.x >= 0) || (horizontalInput < 0 && m_Vel.x <= 0))
		{//if pressing same button as move direction, move to MAXSPEED.
			//print("Moving with keypress");
			if(rawSpeed < m_MaxRunSpeed)
			{
				//print("Rawspeed("+rawSpeed+") less than max");
				if(rawSpeed > m_TractionChangeT)
				{
					//print("LinAccel-> " + rawSpeed);
					if(m_Vel.y > 0)
					{ 	// If climbing, recieve uphill movement penalty.
						m_Vel = ChangeSpeedLinear(m_Vel, m_LinearAccelRate*(1-slopeMultiplier));
					}
					else
					{
						m_Vel = ChangeSpeedLinear(m_Vel, m_LinearAccelRate);
					}
				}
				else if(rawSpeed < 0.001)
				{
					m_Vel = new Vector2((m_Acceleration)*horizontalInput*(1-slopeMultiplier), 0);
					//print("Starting motion. Adding " + m_Acceleration);
				}
				else
				{
					//print("ExpAccel-> " + rawSpeed);
					float eqnX = (1+Mathf.Abs((1/m_TractionChangeT )*rawSpeed));
					float curveMultiplier = 1+(1/(eqnX*eqnX)); // Goes from 1/4 to 1, increasing as speed approaches 0.

					float addedSpeed = curveMultiplier*(m_Acceleration);
					if(m_Vel.y > 0)
					{ // If climbing, recieve uphill movement penalty.
						addedSpeed = curveMultiplier*(m_Acceleration)*(1-slopeMultiplier);
					}
					//print("Addedspeed:"+addedSpeed);
					m_Vel = (m_Vel.normalized)*(rawSpeed+addedSpeed);
					//print("m_Vel:"+m_Vel);
				}
			}
			else
			{
				if(rawSpeed < m_MaxRunSpeed+1)
				{
					rawSpeed = m_MaxRunSpeed;
					m_Vel = SetSpeed(m_Vel,m_MaxRunSpeed);
				}
				else
				{
					//print("Rawspeed("+rawSpeed+") more than max.");
					m_Vel = ChangeSpeedLinear (m_Vel, -m_LinearOverSpeedRate);
				}
			}
		}
		else if((horizontalInput > 0 && m_Vel.x < 0) || (horizontalInput < 0 && m_Vel.x > 0))
		{//if pressing button opposite of move direction, slow to zero exponentially.
			if(rawSpeed > m_TractionChangeT )
			{
				//print("LinDecel");
				m_Vel = ChangeSpeedLinear (m_Vel, -m_LinearStopRate);
			}
			else
			{
				//print("Decelerating");
				float eqnX = (1+Mathf.Abs((1/m_TractionChangeT )*rawSpeed));
				float curveMultiplier = 1+(1/(eqnX*eqnX)); // Goes from 1/4 to 1, increasing as speed approaches 0.
				float addedSpeed = curveMultiplier*(m_Acceleration-slopeMultiplier);
				m_Vel = (m_Vel.normalized)*(rawSpeed-2*addedSpeed);
			}

			//float modifier = Mathf.Abs(m_Vel.x/m_Vel.y);
			//print("SLOPE MODIFIER: " + modifier);
			//m_Vel = m_Vel/(1.25f);
		}

		Vector2 downSlope = m_Vel.normalized; // Normal vector pointing down the current slope!
		if (downSlope.y > 0) //Make sure the vector is descending.
		{
			downSlope *= -1;
		}



		if(downSlope == Vector2.zero)
		{
			downSlope = Vector2.down;
		}

		m_Vel += downSlope*m_SlippingAcceleration*slopeMultiplier;

		//	TESTINGSLOPES
		//print("downSlope="+downSlope);
		//print("m_SlippingAcceleration="+m_SlippingAcceleration);
		//print("slopeMultiplier="+slopeMultiplier);

		//ChangeSpeedLinear(m_Vel, );
		//print("PostTraction velocity: "+m_Vel);
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

		//print("Wall Steepness Angle:"+steepnessAngle);

		///////////////////
		// Movement code //
		///////////////////

		if(m_SurfaceCling)
		{
			if(m_Vel.y > 0)
			{
				m_Vel = ChangeSpeedLinear(m_Vel,-0.8f);
			}
			else if(m_Vel.y <= 0)
			{
				if( (horizontalInput<0 && m_LeftWalled) || (horizontalInput>0 && m_RightWalled) )
				{
					m_Vel = ChangeSpeedLinear(m_Vel,0.1f);
				}
				else
				{
					m_Vel = ChangeSpeedLinear(m_Vel,1f);
				}
			}
		}
		else
		{
			if(m_Vel.y > 0)
			{
				if( (horizontalInput<0 && m_LeftWalled) || (horizontalInput>0 && m_RightWalled) ) // If pressing key toward wall direction.
				{
					m_Vel.y -= 0.8f; //Decelerate slower.
				}
				else if((horizontalInput>0 && m_LeftWalled) || (horizontalInput<0 && m_RightWalled)) // If pressing key opposite wall direction.
				{
					m_Vel.y -= 1.2f; //Decelerate faster.
				}
				else // If no input.
				{
					m_Vel.y -= 1f; 	//Decelerate.
				}
			}
			else if(m_Vel.y <= 0)
			{
				if( (horizontalInput<0 && m_LeftWalled) || (horizontalInput>0 && m_RightWalled) ) // If pressing key toward wall direction.
				{
					m_Vel.y -= 0.1f; //Accelerate downward slower.
				}
				else if((horizontalInput>0 && m_LeftWalled) || (horizontalInput<0 && m_RightWalled)) // If pressing key opposite wall direction.
				{
					m_Vel.y -= 1.2f; //Accelerate downward faster.
				}
				else // If no input.
				{
					m_Vel.y -= 1f; 	//Accelerate downward.
				}
			}
		}
	}

	protected void ToLeftWall(RaycastHit2D leftCheck) 
	{ //Sets the new position of the player and their m_LeftNormal.

		//print ("We've hit LeftWall, sir!!");
		//print ("leftCheck.normal=" + leftCheck.normal);
		//print("preleftwall Pos:" + this.transform.position);

		if (m_Airborne)
		{
			print("Airborne before impact.");
			m_Impact = true;
			//m_Landing = true;
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
			//			if(antiTunneling){
			//				Vector2 surfacePosition = leftCheck2.point;
			//				surfacePosition.x += m_LeftSideLength-m_MinEmbed;
			//				//print("Sent to Pos:" + surfacePosition);
			//				this.transform.position = surfacePosition;
			//			}
		}
		else
		{
			m_LeftWalled = false;
		}

		m_LeftNormal = leftCheck2.normal;

		if(m_Grounded)
		{
			print("LeftGroundWedge detected during left collision.");
			OmniWedge(0,2);
		}

		if(m_Ceilinged)
		{
			print("LeftCeilingWedge detected during left collision.");
			OmniWedge(2,1);
		}

		if(m_RightWalled)
		{
			print("THERE'S PROBLEMS.");
			//OmniWedge(2,3);
		}

		//print ("Final Position2:  " + this.transform.position);
	}

	protected void ToRightWall(RaycastHit2D rightCheck) 
	{ //Sets the new position of the player and their m_RightNormal.

		print ("We've hit RightWall, sir!!");
		//print ("groundCheck.normal=" + groundCheck.normal);
		//print("prerightwall Pos:" + this.transform.position);

		if (m_Airborne)
		{
			print("Airborne before impact.");
			m_Impact = true;
			//m_Landing = true;
		}

		//m_Impact = true;
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
			//			if(antiTunneling){
			//				Vector2 surfacePosition = rightCheck2.point;
			//				surfacePosition.x -= (m_RightSideLength-m_MinEmbed);
			//				//print("Sent to Pos:" + surfacePosition);
			//				this.transform.position = surfacePosition;
			//			}
		}
		else
		{
			m_RightWalled = false;
		}

		m_RightNormal = rightCheck2.normal;
		//print ("Final Position2:  " + this.transform.position);

		if(m_Grounded)
		{
			print("RightGroundWedge detected during right collision.");
			OmniWedge(0,3);
		}

		if(m_LeftWalled)
		{
			print("THERE'S PROBLEMS.");
			//OmniWedge(2,3);
		}

		if(m_Ceilinged)
		{
			print("RightCeilingWedge detected during right collision.");
			OmniWedge(3,1);
		}

	}

	protected void ToGround(RaycastHit2D groundCheck) 
	{ //Sets the new position of the player and their ground normal.
		print ("m_Grounded=" + m_Grounded);

		if (m_Airborne)
		{
			print("Airborne before impact.");
			m_Impact = true;
			//m_Landing = true;
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
			//print("Velocity : " + m_Vel);
			//print("Speed : " + m_Vel.magnitude);
			//print(" ");
			//print(" ");	
			m_Grounded = false;
		}

		if(groundCheck.normal.y == 0f)
		{//If vertical surface
			//throw new Exception("Existence is suffering");
			print("GtG VERTICAL :O");
		}

		m_GroundNormal = groundCheck2.normal;

		if(m_Ceilinged)
		{
			print("CeilGroundWedge detected during ground collision.");
			OmniWedge(0,1);
		}

		if(m_LeftWalled)
		{
			print("LeftGroundWedge detected during ground collision.");
			OmniWedge(0,2);
		}

		if(m_RightWalled)
		{
			print("RightGroundWedge detected during groundcollision.");
			OmniWedge(0,3);
		}

		//print ("Final Position2:  " + this.transform.position);
	}

	protected void ToCeiling(RaycastHit2D ceilingCheck) 
	{ //Sets the new position of the player when they hit the ceiling.

		//float testNumber = ceilingCheck.normal.y/ceilingCheck.normal.x;
		//print(testNumber);
		//print ("We've hit ceiling, sir!!");
		//print ("ceilingCheck.normal=" + ceilingCheck.normal);

		if (m_Airborne)
		{
			print("Airborne before impact.");
			//			m_Landing = true;
			m_Impact = true;
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
			print("Ceilinged = false?");
			m_Ceilinged = false;
		}

		if(ceilingCheck.normal.y == 0f)
		{//If vertical surface
			//throw new Exception("Existence is suffering");
			print("CEILING VERTICAL :O");
		}

		m_CeilingNormal = ceilingCheck2.normal;

		if(m_Grounded)
		{
			print("CeilGroundWedge detected during ceiling collision.");
			OmniWedge(0,1);
		}

		if(m_LeftWalled)
		{
			print("LeftCeilWedge detected during ceiling collision.");
			OmniWedge(2,1);
		}

		if(m_RightWalled)
		{
			print("RightGroundWedge detected during ceiling collision.");
			OmniWedge(3,1);
		}
		//print ("Final Position2:  " + this.transform.position);
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

	protected Vector2 SetSpeed(Vector2 inputVelocity, float speed)
	{
		//print("SetSpeed");
		Vector2 newVelocity;
		Vector2 direction = inputVelocity.normalized;
		newVelocity = direction * speed;
		return newVelocity;
	}

	protected void DirectionChange(Vector2 newNormal)
	{
		//print("DirectionChange");
		m_ExpiredNormal = new Vector2(0,0); //Used for wallslides. This resets the surface normal that wallcling is set to ignore.

		Vector2 initialDirection = m_Vel.normalized;
		Vector2 newPerp = Perp(newNormal);
		Vector2 AdjustedVel;

		float initialSpeed = m_Vel.magnitude;
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
			projectionVal = Vector2.Dot(m_Vel, newPerp)/newPerp.sqrMagnitude;
		}


		//print("P"+projectionVal);
		AdjustedVel = newPerp * projectionVal;
		//print("A"+AdjustedVel);

		if(m_Vel == Vector2.zero)
		{
			//m_Vel = new Vector2(h, m_Vel.y);
		}
		else
		{
			try
			{
				m_Vel = AdjustedVel;
				//print("m_Vel====>"+m_Vel);

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
		}

		if(initialSpeed <= 2f)
		{ // If the player is near stationary, do not remove any velocity because there is no impact!
			speedLossMult = 1;
		}

		//print("SPLMLT " + speedLossMult);

		m_Vel = SetSpeed(m_Vel, initialSpeed*speedLossMult);
		//print("Final Vel " + m_Vel);
		//print ("DirChange Vel:  " + m_Vel);
	}

	/*
	protected void CameraControl()
	{
		#region zoom
		v_CameraZoom = Mathf.Lerp(v_CameraZoom, m_Vel.magnitude, 0.1f);
		//v_CameraZoom = m_Vel.magnitude;
		float zoomChange = 0;
		if((0.15f*v_CameraZoom)>=5f)
		{
			zoomChange = (0.15f*v_CameraZoom)-5f;
		}
		//o_MainCamera.orthographicSize = 5f+(0.15f*v_CameraZoom);
		if(8f+zoomChange >= 50f)
		{
			o_MainCamera.orthographicSize = 50f;
		}
		else
		{
			o_MainCamera.orthographicSize = 8f+zoomChange;
		}
		//o_MainCamera.orthographicSize = 25f;

		#endregion

		#region position

		#endregion
	}
	*/

	protected void OmniWedge(int lowerContact, int upperContact)
	{//Executes when the player is moving into a corner and there isn't enough room to fit them. It halts the player's momentum and sets off a blocked-direction flag.

		//print("OmniWedge!");

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
				break;
			}
		case 2: //lowercontact is left
			{
				lowerDirection = Vector2.left;
				lowerLength = m_LeftSideLength;
				break;
			}
		case 3: //lowercontact is right
			{
				lowerDirection = Vector2.right;
				lowerLength = m_RightSideLength;
				break;
			}
		default:
			{
				throw new Exception("ERROR: DEFAULTED ON LOWERHIT.");
				break;
			}
		}

		lowerHit = Physics2D.Raycast(this.transform.position, lowerDirection, lowerLength, mask);

		float embedDepth;
		Vector2 gPerp; //lowerperp, aka groundperp
		Vector2 cPerp; //upperperp, aka ceilingperp
		Vector2 moveAmount = new Vector2(0,0);

		if(!lowerHit)
		{
			//throw new Exception("Bottom not wedged!");
			print("Bottom not wedged!");
			gPerp.x = m_GroundNormal.x;
			gPerp.y = m_GroundNormal.y;
			return;
		}
		else
		{
			gPerp = Perp(lowerHit.normal);
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
				break;
			}
		case 1: //uppercontact is ceiling
			{
				upperDirection = Vector2.up;
				upperLength = m_CeilingFootLength;
				break;
			}
		case 2: //uppercontact is left
			{
				upperDirection = Vector2.left;
				upperLength = m_LeftSideLength;
				break;
			}
		case 3: //uppercontact is right
			{
				upperDirection = Vector2.right;
				upperLength = m_RightSideLength;
				break;
			}
		default:
			{
				throw new Exception("ERROR: DEFAULTED ON UPPERHIT.");
				break;
			}
		}

		upperHit = Physics2D.Raycast(this.transform.position, upperDirection, upperLength, mask);
		embedDepth = upperLength-upperHit.distance;

		if(!upperHit)
		{
			//throw new Exception("Top not wedged!");
			cPerp = Perp(upperHit.normal);
			print("Top not wedged!");
			return;
		}
		else
		{
			//print("Hitting top, superunwedging..."); 
			cPerp = Perp(upperHit.normal);
		}

		//print("Embedded ("+embedDepth+") units into the ceiling");

		float cornerAngle = Vector2.Angle(cPerp,gPerp);

		//print("Ground Perp = " + gPerp);
		//print("Ceiling Perp = " + cPerp);
		//print("cornerAngle = " + cornerAngle);
		bool convergingLeft = false;

		Vector2 cPerpTest = cPerp;
		Vector2 gPerpTest = gPerp;

		if(cPerpTest.x < 0)
		{
			cPerpTest *= -1;
		}
		if(gPerpTest.x < 0)
		{
			gPerpTest *= -1;
		}

		//print("gPerpTest = " + gPerpTest);
		//print("cPerpTest= " + cPerpTest);

		float convergenceValue = cPerpTest.y-gPerpTest.y;

		if(lowerContact == 2 || upperContact == 2){convergenceValue = 1;}; // THIS IS BAD, PLACEHOLDER CODE!
		if(lowerContact == 3 || upperContact == 3){convergenceValue =-1;}; // THIS IS BAD, PLACEHOLDER CODE!

		if(cornerAngle > 90f)
		{
			if(convergenceValue > 0)
			{
				moveAmount = SuperUnwedger(cPerp, gPerp, true, embedDepth);
				//print("Left wedge!");
				m_LeftWallBlocked = true;
			}
			else if(convergenceValue < 0)
			{
				moveAmount = SuperUnwedger(cPerp, gPerp, false, embedDepth);
				//print("Right wedge!");
				m_RightWallBlocked = true;
			}
			else
			{
				throw new Exception("CONVERGENCE VALUE OF ZERO ON CORNER!");
			}
			m_Vel = new Vector2(0f, 0f);
		}
		else
		{
			moveAmount = (upperDirection*(-embedDepth));
		}

		this.transform.position = new Vector2((this.transform.position.x + moveAmount.x), (this.transform.position.y + moveAmount.y));
	}

	protected Vector2	Perp(Vector2 input)
	{
		Vector2 output;
		output.x = input.y;
		output.y = -input.x;
		return output;
	}		

	protected void UpdateContactNormals(bool posCorrection)
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
						print("Antitunneling omniwedge executed");			
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
				print("Triple Embed.");
				break;
			}
		case 4:
			{
				print("FULL embedding! :C");
				if(recoverFromFullEmbed)
				{
					this.transform.position = lastSafePosition;
				}
				break;
			}
		default:
			{
				print("ERROR: DEFAULTED.");
				break;
			}
		}

	}

	protected Vector2 SuperUnwedger(Vector2 cPerp, Vector2 gPerp, bool cornerIsLeft, float embedDistance)
	{
		if(!cornerIsLeft)
		{// Setting up variables	
			//print("Resolving right wedge.");
			if(gPerp.x>0)
			{// Ensure both perpendicular vectors are pointing left, out of the corner the player is lodged in.
				gPerp *= -1;
			}

			if(cPerp.x>0)
			{// Ensure both perpendicular vectors are pointing left, out of the corner the player is lodged in.
				cPerp *= -1;
			}

			if(cPerp.x != -1)
			{// Multiply/Divide the top vector so that its x = -1.
				if(cPerp.x == 0)
				{
					//print("You're unwedging from a wall, that's not allowed. Wall corners aren't wedgy enough.");
					return new Vector2(0, -embedDistance);
				}
				else
				{
					cPerp /= Mathf.Abs(cPerp.x);
				}
			}

			if(gPerp.x != -1)
			{// Multiply/Divide the bottom vector so that its x = -1.
				if(gPerp.x == 0)
				{
					//throw new Exception("Your ground has no horizontality. What are you even doing?");
					return new Vector2(0, embedDistance);
				}
				else
				{
					gPerp /= Mathf.Abs(gPerp.x);
				}
			}
		}
		else
		{
			//print("Resolving left wedge.");
			// Ensure both perpendicular vectors are pointing right, out of the corner the player is lodged in.
			if(gPerp.x<0)
			{
				gPerp *= -1;
			}

			if(cPerp.x<0)
			{// Ensure both perpendicular vectors are pointing left, out of the corner the player is lodged in.
				cPerp *= -1;
			}

			if(cPerp.x != 1)
			{// Multiply/Divide the top vector so that its x = -1.
				if(cPerp.x == 0)
				{
					//throw new Exception("You're unwedging from a wall, that's not allowed. Wall corners aren't wedgy enough.");
					return new Vector2(0, -embedDistance);
				}
				else
				{
					cPerp /= cPerp.x;
				}
			}

			if(gPerp.x != -1)
			{// Multiply/Divide the bottom vector so that its x = -1.
				if(gPerp.x == 0)
				{
					//throw new Exception("Your ground has no horizontality. What are you even doing?");
					return new Vector2(0, -embedDistance);
				}
				else
				{
					gPerp /= gPerp.x;
				}
			}
		}

		//print("Adapted Ground Perp = " + gPerp);
		//print("Adapted Ceiling Perp = " + cPerp);

		//
		// Now, the equation for repositioning a vertical line that is embedded in a corner:
		//

		float B = gPerp.y;
		float A = cPerp.y;
		float H = embedDistance; //Reduced so the player stays just embedded enough for the raycast to detect next frame.
		float DivX;
		float DivY;
		float X;
		float Y;

		//print("(A, B)=("+ A +", "+ B +").");

		if(B <= 0)
		{
			//print("B <= 0, using normal eqn.");
			DivX = B-A;
			DivY = -(DivX/B);
		}
		else
		{
			//print("B >= 0, using alternate eqn.");
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
		return new Vector2(X,Y); // Returns the distance the object must move to resolve wedging.
	}

	protected void Jump(float horizontalInput)
	{
		if(m_Grounded&&m_Ceilinged)
		{
			print("Grounded and Ceilinged, nowhere to jump!");
			i_JumpKey = false;
		}
		else if(m_Grounded)
		{
			if(m_Vel.y >= 0)
			{
				m_Vel = new Vector2(m_Vel.x+(m_HJumpForce*horizontalInput), m_Vel.y+m_VJumpForce);
			}
			else
			{
				m_Vel = new Vector2(m_Vel.x+(m_HJumpForce*horizontalInput), m_VJumpForce);
			}
			o_FighterAudio.JumpSound();
			i_JumpKey = false;
		}
		else if(m_LeftWalled)
		{
			print("Leftwalljumping!");
			if(m_Vel.y < 0)
			{
				m_Vel = new Vector2(m_WallHJumpForce, m_WallVJumpForce);
			}
			else if(m_Vel.y <= (2*m_WallVJumpForce))
			{
				m_Vel = new Vector2(m_WallHJumpForce, m_Vel.y+m_WallVJumpForce);
			}
			else
			{
				m_Vel = new Vector2(m_WallHJumpForce, m_Vel.y);
			}
			o_FighterAudio.JumpSound();
			i_JumpKey = false;
			m_LeftWalled = false;
		}
		else if(m_RightWalled)
		{
			print("Rightwalljumping!");
			if(m_Vel.y < 0)
			{
				m_Vel = new Vector2(-m_WallHJumpForce, m_WallVJumpForce);
			}
			else if(m_Vel.y <= m_WallVJumpForce)
			{
				m_Vel = new Vector2(-m_WallHJumpForce, m_Vel.y+m_WallVJumpForce);
			}
			else
			{
				m_Vel = new Vector2(-m_WallHJumpForce, m_Vel.y);
			}

			o_FighterAudio.JumpSound();
			i_JumpKey = false;
			m_RightWalled = false;
		}
		else if(m_Ceilinged)
		{
			if(m_Vel.y <= 0)
			{
				m_Vel = new Vector2(m_Vel.x+(m_HJumpForce*horizontalInput), m_Vel.y -m_VJumpForce);
			}
			else
			{
				m_Vel = new Vector2(m_Vel.x+(m_HJumpForce*horizontalInput), -m_VJumpForce);
			}
			o_FighterAudio.JumpSound();
			i_JumpKey = false;
			m_Ceilinged = false;
		}
		else
		{
			//print("Can't jump, airborne!");
		}
	}

	protected void ZonJump(Vector2 jumpNormal)
	{
		//g_ZonJumpCharge = o_Spooler.GetTotalPower();
		//m_Vel = jumpNormal*(m_ZonJumpForceBase+(m_ZonJumpForcePerCharge*g_ZonJumpCharge));
		//g_ZonJumpCharge = 0;		
		//i_JumpKey = false;
		//o_FighterAudio.JumpSound();
		//o_Spooler.Reset();
	}



	#endregion
	//###################################################################################################################################
	// PUBLIC FUNCTIONS
	//###################################################################################################################################
	#region PUBLIC FUNCTIONS
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
		return m_Vel;
	}

	public float GetSpeed()
	{
		return m_Spd;
	}

	public void SetZonLevel(int zonLevel)
	{
		v_ZonLevel = zonLevel;
	}

	public int GetZonLevel()
	{
		return v_ZonLevel;
	}

	public int GetZonStance()
	{
		return g_ZonStance; //-1 is none, 0 is kneeling, 1 is AD.
	}

	public void DissipateZon()
	{
		// Executes when the player leaves zon stance without using power.
	}

	#endregion
}