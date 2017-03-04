using UnityEngine.UI;
using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[System.Serializable]
public class PlatformerCharacter2D : MonoBehaviour
{                
	//############################################################################################################################################################################################################
	// HANDLING CHARACTERISTICS
	//###########################################################################################################################################################################
	#region HANDLINGCHARACTERISTICS
	[Header("Movement Tuning:")]
	[SerializeField] private float m_MinSpeed = 10f; 							// The instant starting speed while moving
	[SerializeField] private float maxRunSpeed;									// The fastest the player can travel in the x axis.
	[Range(0,2)][SerializeField] private float m_Acceleration = 1f;    			// Speed the player accelerates at
	[SerializeField] private float m_VJumpForce = 40f;                  		// Amount of vertical force added when the player jumps.
	[SerializeField] private float m_HJumpForce = 5f;  							// Amount of horizontal force added when the player jumps.
	[SerializeField] private float tractionChangeThreshold = 20f;				// Threshold where movement changes from exponential to linear acceleration.  
	[Range(0,1)][SerializeField] private float m_LinearStopRate = 1f; 			// How fast the player decelerates when changing direction.
	[Range(0,1)][SerializeField] private float m_LinearSlideRate = 0.20f;		// How fast the player decelerates with no input.
	[Range(0,1)][SerializeField] private float m_LinearAccelRate = 0.35f;		// How fast the player accelerates with input.
	[Range(1,8)][SerializeField] private float m_StationaryBoostMultiplier = 2f;// Governs how much the player accelerates on the very first frame from stationary.
	[Range(1,89)][SerializeField] private float m_AngleSpeedLossMin = 20f; 		// Any impacts at sharper angles than this will start to slow the player down. Reaches full halt at m_AngleSpeedLossMax.
	[Range(1,89)][SerializeField] private float m_AngleSpeedLossMax = 80f; 		// Any impacts at sharper angles than this will result in a full halt. DO NOT SET THIS LOWER THAN m_AngleSpeedLossMin!!
	[Range(1,89)][SerializeField] private float m_TractionLossAngle = 45f; 		// Changes the angle at which steeper angles start to linearly lose traction, and eventually starts slipping back down. Default equates to 45 degrees.
	[Range(0,2)][SerializeField] private float m_SlippingAcceleration = 1f;  
	[Range(0.5f,3)][SerializeField] private float m_SteepSurfaceHangTime = 1f; 	// How long the player can cling to walls before gravity takes over.
	private float timeSpentHanging = 0f;										// Amount of time the player has been in walljump stance.
	[Range(0,0.5f)][SerializeField] private float m_MaxEmbed = 0.02f;			// How deep into objects the character can be before actually colliding with them. MUST BE GREATER THAN m_MinEmbed!!!
	[Range(0.01f,0.4f)][SerializeField] private float m_MinEmbed = 0.01f; 		// How deep into objects the character will sit by default. A value of zero will cause physics errors because the player is not technically *touching* the surface.

	#endregion
	//############################################################################################################################################################################################################
	// PLAYER COMPONENTS
	//###########################################################################################################################################################################
	#region PLAYERCOMPONENTS
	[Header("Player Components:")]
	[SerializeField] private Text m_Speedometer;      
	[SerializeField] private Camera m_MainCamera;
	[SerializeField] private GameObject m_PlayerSprite;
	private float cameraZoom;
    private Animator m_Anim;            // Reference to the player's animator component.
    private Rigidbody2D m_Rigidbody2D;
	private SpriteRenderer m_SpriteRenderer;
	#endregion
	//############################################################################################################################################################################################################
	// PHYSICS&RAYCASTING
	//###########################################################################################################################################################################
	#region PHYSICS&RAYCASTING
	[SerializeField] private LayerMask mask; // A mask determining what is ground to the character

	private Transform m_LeftFoot; // Floor collider, left.
	private Vector2 m_LeftFootOffset;
	private float m_LeftFootLength;

	private Transform m_RightFoot; // Floor collider, right.
	private Vector2 m_RightFootOffset;
	private float m_RightFootLength;

	private Transform m_FloorFoot; // Floor collider, middle.
	private Vector2 m_FloorFootOffset; 
	private float m_FloorFootLength;

	private Transform m_CeilingFoot; // Ceiling collider, middle.
	private Vector2 m_CeilingFootOffset;
	private float m_CeilingFootLength;

	private Transform m_LeftSide; //  Wall collider, left.
	private Vector2 m_LeftSideOffset;
	private float m_LeftSideLength;

	private Transform m_RightSide;  //  Wall collider, right.
	private Vector2 m_RightSideOffset;
	private float m_RightSideLength;

	private Vector2 groundNormal;
	private Vector2 ceilingNormal;
	private Vector2 leftNormal;
	private Vector2 rightNormal;

	[Header("Player State:")]

	[SerializeField][ReadOnlyAttribute]private float remainingVelMult;
	[SerializeField][ReadOnlyAttribute]private Vector2 pVel;
	[SerializeField][ReadOnlyAttribute]private Vector2 pVelPerFrame;
	[SerializeField][ReadOnlyAttribute]private Vector2 remainingMovement;
	[SerializeField][ReadOnlyAttribute]private bool leftFootContact;
	[SerializeField][ReadOnlyAttribute]private bool rightFootContact;
	[SerializeField][ReadOnlyAttribute]private bool floorContact;
	[SerializeField][ReadOnlyAttribute]private bool ceilingContact;
	[SerializeField][ReadOnlyAttribute]private bool leftSideContact;
	[SerializeField][ReadOnlyAttribute]private bool rightSideContact;
	[Space(10)]
	[SerializeField][ReadOnlyAttribute]private bool m_Grounded;
	[SerializeField][ReadOnlyAttribute]private bool m_Ceilinged; 
	[SerializeField][ReadOnlyAttribute]private bool m_LeftWalled; 
	[SerializeField][ReadOnlyAttribute]private bool m_RightWalled;
	[Space(10)]
	[SerializeField][ReadOnlyAttribute]private bool m_GroundBlocked;
	[SerializeField][ReadOnlyAttribute]private bool m_CeilingBlocked; 
	[SerializeField][ReadOnlyAttribute]private bool m_LeftWallBlocked; 
	[SerializeField][ReadOnlyAttribute]private bool m_RightWallBlocked; 

	#endregion
	//##########################################################################################################################################################################
	// PLAYER INPUT VARIABLES
	//###########################################################################################################################################################################
	#region PLAYERINPUT
	private bool m_Jump;
	private bool m_KeyLeft;
	private bool m_KeyRight;
	private int CtrlH; 					// Tracks horizontal keys pressed. Values are -1 (left), 0 (none), or 1 (right). 
	private bool facingDirection; 		// true means right (the direction), false means left.
	#endregion
	//############################################################################################################################################################################################################
	// DEBUGGING VARIABLES
	//##########################################################################################################################################################################
	#region DEBUGGING
	private int errorDetectingRecursionCount; //Iterates each time recursive trajectory correction executes on the current frame.
	[Header("Debug:")]
	[SerializeField] private bool autoSprint; // When set to true, the player will run even after the key is released.
	[SerializeField] private bool autoJump;
	[SerializeField] private bool antiTunneling; // When set to true, the player will be pushed up out of objects they are stuck in.
	[SerializeField] private bool noGravity; 
	[SerializeField] private bool showVelocityIndicator;
	[SerializeField] private bool showContactIndicators;
	private LineRenderer m_DebugLine; // Shows Velocity.
	private LineRenderer m_LeftLine; 
	private LineRenderer m_FloorLine;
	private LineRenderer m_RightLine;
	private LineRenderer m_CeilingLine;
	private LineRenderer m_LeftSideLine;
	private LineRenderer m_RightSideLine;
	#endregion
	//########################################################################################################################################
	// MAIN FUNCTIONS
	//########################################################################################################################################

    private void Awake()
    {
		Vector2 playerOrigin = new Vector2(this.transform.position.x, this.transform.position.y);
		m_DebugLine = GetComponent<LineRenderer>();
		if(!showVelocityIndicator){
			m_DebugLine.enabled = false;
		}

		m_LeftFoot = transform.Find("LeftFoot");
		m_LeftLine = m_LeftFoot.GetComponent<LineRenderer>();
		m_LeftFootOffset.x = m_LeftFoot.position.x-playerOrigin.x;
		m_LeftFootOffset.y = m_LeftFoot.position.y-playerOrigin.y;
		m_LeftFootLength = m_LeftFootOffset.y; // Cheaty way of getting only their vertical length, since the sidefoot offset is technically diagonal.

		m_RightFoot = transform.Find("RightFoot");
		m_RightLine = m_RightFoot.GetComponent<LineRenderer>();
		m_RightFootOffset.x = m_RightFoot.position.x-playerOrigin.x;
		m_RightFootOffset.y = m_RightFoot.position.y-playerOrigin.y;
		m_RightFootLength = m_RightFootOffset.y; // Cheaty way of getting only their vertical length, since the sidefoot offset is technically diagonal.

		m_FloorFoot = transform.Find("MidFoot");
		m_FloorLine = m_FloorFoot.GetComponent<LineRenderer>();
		m_FloorFootOffset.x = m_FloorFoot.position.x-playerOrigin.x;
		m_FloorFootOffset.y = m_FloorFoot.position.y-playerOrigin.y;
		m_FloorFootLength = m_FloorFootOffset.magnitude;

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


		m_Anim = m_PlayerSprite.GetComponent<Animator>();
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
		m_SpriteRenderer = m_PlayerSprite.GetComponent<SpriteRenderer>();

		remainingMovement = new Vector2(0,0);
		remainingVelMult = 1f;
		//print(remainingMovement);

		if(!showContactIndicators)
		{
			m_CeilingLine.enabled = false;
			m_LeftLine.enabled = false;
			m_FloorLine.enabled = false;
			m_RightLine.enabled = false;
			m_RightSideLine.enabled = false;
			m_LeftSideLine.enabled = false;
		}


    }

    private void FixedUpdate()
	{



		Vector2 finalPos = new Vector2(this.transform.position.x+remainingMovement.x, this.transform.position.y+remainingMovement.y);
		this.transform.position = finalPos;

		//m_Rigidbody2D.MovePosition(finalPos);
		//Vector2 finalPos = new Vector2(this.transform.position.x+(pVelPerFrame.x), this.transform.position.y+(pVelPerFrame.y));
		//print("Initial Pos: " + startingPos);
		//print("Initial Vel: " +  pVel);

		m_KeyLeft = CrossPlatformInputManager.GetButton("Left");
		//print("LEFT="+m_KeyLeft);
		m_KeyRight = CrossPlatformInputManager.GetButton("Right");
		//print("RIGHT="+m_KeyRight);
		if((!m_KeyLeft && !m_KeyRight) || (m_KeyLeft && m_KeyRight))
		{
			//print("BOTH/NEITHER");
			if(!autoSprint)
			{
				CtrlH = 0;
			}
		}
		else if(m_KeyLeft)
		{
			//print("LEFT");
			CtrlH = -1;
		}
		else
		{
			//print("RIGHT");
			CtrlH = 1;
		}
			
		//print("CTRLH=" + CtrlH);

		UpdateContactNormals(true);

		if(m_Ceilinged&&m_Grounded)
		{
			if(pVel.magnitude != 0)
			{
				Wedged();
			}
			m_Ceilinged = false; // Remove when safe
		}
			
		if(m_Jump)
		{
			Jump(CtrlH);
		}

		if(m_Grounded)
		{//Locomotion!
			Traction(CtrlH);
		}else
		{//Gravity!
			pVel = new Vector2 (pVel.x, pVel.y - 1);
			m_Ceilinged = false;
		}
			
		errorDetectingRecursionCount = 0; //Used for Colli sion();

		//print("Velocity before Coll ision: "+pVel);
		//print("Position before Coll ision: "+this.transform.position);

		pVelPerFrame = pVel*Time.fixedDeltaTime;
		remainingVelMult = 1f;
		remainingMovement = pVelPerFrame;
		Vector2 startingPos = this.transform.position;

		Collision();
	
		//print("Per frame velocity at end of Coll ision() "+pVelPerFrame);

		//UpdateContactNormals(true);
	
		//print("Per frame velocity at end of updatecontactnormals "+pVelPerFrame);

		Vector2 distanceTravelled = new Vector2(this.transform.position.x-startingPos.x,this.transform.position.y-startingPos.y);
		//print("Distance moved to complete colli sion: "+distanceTravelled);
		//print("remainingMovement: "+remainingMovement);

		remainingMovement -= distanceTravelled;

		if(pVelPerFrame.magnitude>0)
		{
			remainingVelMult = (pVelPerFrame.magnitude-distanceTravelled.magnitude)/pVelPerFrame.magnitude;
		}
		else
		{
			remainingVelMult = 1f;
		}
		//print("remainingVelMult: "+remainingVelMult);

		//print("movement after distance travelled: "+remainingMovement);

		if(m_LeftWalled)
		{
			if(m_Grounded)
			{
				//print("Both!");
				if(pVel.y > 0)
				{
					float leftSteepness = Vector2.Angle(Vector2.right, Perp(leftNormal));
					//print("LS="+leftSteepness);
					float groundSteepness = Vector2.Angle(Vector2.right, Perp(groundNormal));
					//print("GS="+groundSteepness);
					if(leftSteepness >= groundSteepness)
					{
						DirectionChange(leftNormal);
					}
					else
					{
						DirectionChange(groundNormal);
					}
				}
				else if(pVel.y < 0)
				{
					//print("DESCENDING IMPACT");
					float leftSteepness = Vector2.Angle(Vector2.right, Perp(leftNormal));
					//print("LS="+leftSteepness);
					float groundSteepness = Vector2.Angle(Vector2.right, Perp(groundNormal));
					//print("GS="+groundSteepness);
					if(leftSteepness <= groundSteepness)
					{
						//print("Chose left");
						DirectionChange(leftNormal);
					}
					else
					{
						//print("Chose ground");
						DirectionChange(groundNormal);
					}
				}
				else
				{
					DirectionChange(leftNormal);
				}
			}
			else
			{
				DirectionChange(leftNormal);
			}

			//print("LeftWallMovement");
			//DirectionChange(leftNormal);
		}

		if(m_RightWalled)
		{
			if(m_Grounded)
			{
				//print("Both!");
				if(pVel.y > 0)
				{
					float rightSteepness = Vector2.Angle(Vector2.right, Perp(rightNormal));
					//print("LS="+rightSteepness);
					float groundSteepness = Vector2.Angle(Vector2.right, Perp(groundNormal));
					//print("GS="+groundSteepness);
					if(rightSteepness >= groundSteepness)
					{
						DirectionChange(rightNormal);
					}
					else
					{
						DirectionChange(groundNormal);
					}
				}
				else if(pVel.y < 0)
				{
					//print("DESCENDING IMPACT");
					float rightSteepness = Vector2.Angle(Vector2.right, Perp(rightNormal));
					//print("LS="+rightSteepness);
					float groundSteepness = Vector2.Angle(Vector2.right, Perp(groundNormal));
					//print("GS="+groundSteepness);
					if(rightSteepness <= groundSteepness)
					{
						//print("Chose right");
						DirectionChange(rightNormal);
					}
					else
					{
						//print("Chose ground");
						DirectionChange(groundNormal);
					}
				}
				else
				{
					DirectionChange(rightNormal);
				}
			}
			else
			{
				DirectionChange(rightNormal);
			}

			//print("rightWallMovement");
			//DirectionChange(rightNormal);
		}

		if(m_Ceilinged&&!m_RightWalled&&!m_LeftWalled)
		{
			//print("CeilingMovement");
			DirectionChange(ceilingNormal);
		}

		if (m_Grounded&&!m_RightWalled&&!m_LeftWalled) //Handles velocity along ground surface.
		{
			//print("GroundMovement");
			DirectionChange(groundNormal);
		}


		//print("Speed this frame: "+pVel.magnitude);
	
		//print("Per frame velocity at end of physics frame: "+pVelPerFrame);
		//print("Pos at end of physics frame: "+this.transform.position);
		//print("##############################################################################################");

		//print("FinaL Pos: " + this.transform.position);
		//print("FinaL Vel: " + pVel);
		//print("Speed at end of frame: " + pVel.magnitude);


		#region Animator Controls

		//
		//Animator Controls
		//

		m_Anim.SetBool("Walled", false);

		if(m_LeftWalled&&!m_Grounded)
		{
			m_Anim.SetBool("Walled", true);
			facingDirection = false;
		}

		if(m_RightWalled&&!m_Grounded)
		{
			m_Anim.SetBool("Walled", true);
			facingDirection = true;
		}

		if (!facingDirection) //If facing left
		{
			//print("FACING LEFT!   "+h)
			m_PlayerSprite.transform.localScale = new Vector3 (-1f, 1f, 1f);
			if(pVel.x > 0)
			{
				m_Anim.SetBool("Crouch", true);
			}
			else
			{
				m_Anim.SetBool("Crouch", false);
			}
		} 
		else //If facing right
		{
			//print("FACING RIGHT!   "+h);

			m_PlayerSprite.transform.localScale = new Vector3 (1f, 1f, 1f);
			if(pVel.x < 0)
			{
				m_Anim.SetBool("Crouch", true);
			}
			else
			{
				m_Anim.SetBool("Crouch", false);
			}
		}

		/*
		Vector2 debugLineVector = ((pVelPerFrame));
		debugLineVector.y -= (m_FloorFootLength-m_MaxEmbed);
		m_DebugLine.SetPosition(1, debugLineVector);
		*/
		Vector3[] debugLineVector = new Vector3[3];

		debugLineVector[0].x = -distanceTravelled.x;
		debugLineVector[0].y = -(distanceTravelled.y+(m_FloorFootLength-m_MaxEmbed));
		debugLineVector[0].z = 0f;

		debugLineVector[1].x = 0f;
		debugLineVector[1].y = -(m_FloorFootLength-m_MaxEmbed);
		debugLineVector[1].z = 0f;

		//Vector2 debugLineVector[2] = distanceTravelled+remainingMovement;
		//debugLineVector[2].x = (pVelPerFrame.x);
		//debugLineVector[2].y = (pVelPerFrame.y)-(m_FloorFootLength-m_MaxEmbed);
		//debugLineVector[2].z = 0f;

		debugLineVector[2].x = remainingMovement.x;
		debugLineVector[2].y = (remainingMovement.y)-(m_FloorFootLength-m_MaxEmbed);
		debugLineVector[2].z = 0f;

		m_DebugLine.SetPositions(debugLineVector);

		m_Anim.SetFloat("Speed", pVel.magnitude);

		if(pVel.magnitude >= tractionChangeThreshold )
		{
			m_DebugLine.endColor = Color.white;
			m_DebugLine.startColor = Color.white;
		}   
		else
		{   
			m_DebugLine.endColor = Color.red;
			m_DebugLine.startColor = Color.red;
		}

		float multiplier = 1; // Animation playspeed multiplier that increases with higher velocity

		if(pVel.magnitude > 20.0f)
		{
			multiplier = ((pVel.magnitude - 20) / 20)+1;
		}

		m_Anim.SetFloat("Multiplier", multiplier);

		if (!m_Grounded&&!m_LeftWalled&!m_RightWalled) 
		{
			m_Anim.SetBool("Ground", false);
		}
		else
		{
			m_Anim.SetBool("Ground", true);
		}
		#endregion



    }

	private void Update()
	{
		m_Speedometer.text = ""+Math.Round(pVel.magnitude,0);
		if (!m_Jump && (m_Grounded||m_Ceilinged))
		{
			// Read the jump input in Update so button presses aren't missed.

			m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
			if(autoJump&&m_Grounded)
			{
				m_Jump = true;
			}
		}
	}

	private void LateUpdate()
	{
		//CameraControl();
	}

	//###################################################################################################################################
	// CUSTOM FUNCTIONS
	//###################################################################################################################################

	private void Collision()
	{
		float crntSpeed = pVelPerFrame.magnitude; //Current speed.
		//print("DC Executing");
		errorDetectingRecursionCount++;

		if(errorDetectingRecursionCount >= 5)
		{
			throw new Exception("Your recursion code is fucked!");
			return;
		}
			
		if(pVel.x > 0.001f)
		{
			m_LeftWalled = false;
			m_LeftWallBlocked = false;
		}

		if(pVel.x < -0.001f)
		{
			m_RightWalled = false;
			m_RightWallBlocked = false;
		}
	
		#region collision raytesting

		Vector2 adjustedBot = m_FloorFoot.position; // AdjustedBot marks the end of the floor raycast, but 0.02 shorter.
		adjustedBot.y += m_MaxEmbed;

		Vector2 adjustedTop = m_CeilingFoot.position; // AdjustedTop marks the end of the ceiling raycast, but 0.02 shorter.
		adjustedTop.y -= m_MaxEmbed;

		Vector2 adjustedLeft = m_LeftSide.position; // AdjustedLeft marks the end of the left wall raycast, but 0.02 shorter.
		adjustedLeft.x += m_MaxEmbed;

		Vector2 adjustedRight = m_RightSide.position; // AdjustedRight marks the end of the right wall raycast, but 0.02 shorter.
		adjustedRight.x -= m_MaxEmbed;

		//RaycastHit2D groundCheck = Physics2D.Raycast(this.transform.position, Vector2.down, m_FloorFootLength, mask);
		RaycastHit2D[] predictedLoc = new RaycastHit2D[4];

		predictedLoc[0] = Physics2D.Raycast(adjustedBot, pVel, crntSpeed, mask); 	// Ground
		predictedLoc[1] = Physics2D.Raycast(adjustedTop, pVel, crntSpeed, mask); 	// Ceiling
		predictedLoc[2] = Physics2D.Raycast(adjustedLeft, pVel, crntSpeed, mask); // Left
		predictedLoc[3] = Physics2D.Raycast(adjustedRight, pVel, crntSpeed, mask);// Right  

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
			shortestRaycast = shortestVertical;
		}
		else if(shortestHorizontal >= 0)
		{
			shortestRaycast = shortestHorizontal;
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
			print("TOTAL COLLISIONS: "+collisionNum);
		}

		#endregion

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
				Vector2 testperp = Perp(predictedLoc[0].normal);
				float steepnessAngle = Vector2.Angle(Vector2.right,testperp);

				#region collision debug
				//print("groundAngle:"+steepnessAngle);
				//GameObject collisionPoint = new GameObject("Impact");
				//collisionPoint.transform.position = new Vector2(groundCheck.point.x, groundCheck.point.y);
				//GameObject rayStartPoint = new GameObject("Ray");
				//rayStartPoint.transform.position = new Vector2(adjustedBot.x, adjustedBot.y);
				//print("Impact!");
				//print ("Player Pos:  " + this.transform.position);
				//print ("velocity:  " + pVel);
				#endregion

				//ceilingCheck = Physics2D.Raycast(adjustedTop, pVel, groundCheck.distance, mask); //Collider checks go only as far as the first angle correction, so they don't mistake the slope of the floor for an obstacle.

				//print("Velocity before impact: "+pVel);
				Vector2 moveDirectionNormal = Perp(pVel.normalized);

				Vector2 invertedImpactNormal;//This is done in case one of the raycasts is inside the collider, which would cause it to return an inverted normal value.
				invertedImpactNormal.x = -predictedLoc[0].normal.x; //Inverting the normal.
				invertedImpactNormal.y = -predictedLoc[0].normal.y; //Inverting the normal.

				if (predictedLoc[0].collider == null) 
				{ // If player is airborne beforehand.
					print("A2G");
					GroundToGround(predictedLoc[0]);
					return;
				} 
				else if ((moveDirectionNormal != predictedLoc[0].normal) && (moveDirectionNormal != invertedImpactNormal)) 
				{ // If the slope you're hitting is different than your current slope.
					print("G2G");
					GroundToGround(predictedLoc[0]);
					return;
				}
				else 
				{
					if(moveDirectionNormal == invertedImpactNormal)
					{
						print("INVERTED GROUND IMPACT NORMAL DETECTED!");
					}
					//print ("("+moveDirectionNormal+")!=("+predictedLoc[0].normal+")");
					//print ("("+moveDirectionNormal+")!=("+invertedImpactNormal+")");
					//print ("Same surface, no trajectory change needed");
					return;
				}
				break;
			}
			case 1:
			{
				print("CEILING_IMPACT");
				AirToCeiling();
				break;
			}
			case 2:
			{
				print("LEFT_IMPACT");
				if(predictedLoc[0]) //If hitting wall and ground.
				{
					ToLeftWall(predictedLoc[2]);
				}
				else if(predictedLoc[1]) //If hitting wall and ceiling.
				{
					ToLeftWall(predictedLoc[2]);
				}
				else  //If simply hitting wall.
				{
					ToLeftWall(predictedLoc[2]);
				}
				break;
			}
			case 3:
			{
				print("RIGHT_IMPACT");
				ToRightWall(predictedLoc[3]);
				break;
			}
			default:
			{
				print("ERROR: DEFAULTED.");
				break;
			}
		}
	}
		
	private void Traction(float horizontalInput)
	{
		Vector2 groundPerp = Perp(groundNormal);

		//print("gp="+groundPerp);

		if(groundPerp.x > 0)
		{
			groundPerp *= -1;
		}

		float steepnessAngle = Vector2.Angle(Vector2.left,groundPerp);
		//print("SteepnessAngle:"+steepnessAngle);

		float slopeMultiplier = 0;
	
		if(steepnessAngle > m_TractionLossAngle)
		{
			slopeMultiplier = ((steepnessAngle-m_TractionLossAngle)/(90f-m_TractionLossAngle));

			//print("slopeMultiplier: "+ slopeMultiplier);
			//print("groundPerpY: "+groundPerpY+", slopeThreshold: "+slopeThreshold);
		}

		//print("Traction");
		if( ((m_LeftWallBlocked)&&(horizontalInput < 0)) || ((m_RightWallBlocked)&&(horizontalInput > 0)) )
		{// If running at an obstruction you're up against.
			print("Running against a wall.");
			horizontalInput = 0;
		}
		/*else
		{
			if((m_LeftWalled)&&(horizontalInput > 0))
			{
				//print("leftwalled removed from traction");
				m_LeftWalled = false;
			}
			if((m_RightWalled)&&(horizontalInput < 0))
			{
				m_RightWalled = false;
			}
		}*/

		//print("Traction executing");
		float rawSpeed = pVel.magnitude;
		//print("pVel.magnitude"+pVel.magnitude);
		if (horizontalInput == 0) 
		{//if not pressing any move direction, slow to zero linearly.
			//print("No input, slowing...");
			if(rawSpeed <= 1)
			{
				pVel = Vector2.zero;	
			}
			else
			{
				pVel = ChangeSpeedLinear (pVel, -m_LinearSlideRate);
			}
		}
		else if((horizontalInput > 0 && pVel.x >= 0) || (horizontalInput < 0 && pVel.x <= 0))
		{//if pressing same button as move direction, move to MAXSPEED.
			//print("Moving with keypress");
			if(rawSpeed <= maxRunSpeed)
			{
				//print("Rawspeed("+rawSpeed+") less than max");
				if(rawSpeed > tractionChangeThreshold )
				{
					//print("LinAccel-> " + rawSpeed);
					if(pVel.y > 0)
					{ 	// If climbing, recieve uphill movement penalty.
						pVel = ChangeSpeedLinear(pVel, m_LinearAccelRate*(1-slopeMultiplier));
					}
					else
					{
						pVel = ChangeSpeedLinear(pVel, m_LinearAccelRate);
					}
				}
				else if(rawSpeed < 0.001)
				{
					pVel = new Vector2((m_Acceleration)*horizontalInput*(1-slopeMultiplier), 0);
					//print("Starting motion. Adding " + m_Acceleration);
				}
				else
				{
					//print("ExpAccel-> " + rawSpeed);
					float eqnX = (1+Mathf.Abs((1/tractionChangeThreshold )*rawSpeed));
					float curveMultiplier = 1+(1/(eqnX*eqnX)); // Goes from 1/4 to 1, increasing as speed approaches 0.

					float addedSpeed = curveMultiplier*(m_Acceleration);
					if(pVel.y > 0)
					{ // If climbing, recieve uphill movement penalty.
						addedSpeed = curveMultiplier*(m_Acceleration)*(1-slopeMultiplier);
					}
					//print("Addedspeed:"+addedSpeed);
					pVel = (pVel.normalized)*(rawSpeed+addedSpeed);
					//print("pVel:"+pVel);
				}
			}
			else
			{
				print("Rawspeed("+rawSpeed+") more than max???");
			}
		}
		else if((horizontalInput > 0 && pVel.x < 0) || (horizontalInput < 0 && pVel.x > 0))
		{//if pressing button opposite of move direction, slow to zero exponentially.
			if(rawSpeed > tractionChangeThreshold )
			{
				//print("LinDecel");
				pVel = ChangeSpeedLinear (pVel, -m_LinearStopRate);
			}
			else
			{
				//print("Decelerating");
				float eqnX = (1+Mathf.Abs((1/tractionChangeThreshold )*rawSpeed));
				float curveMultiplier = 1+(1/(eqnX*eqnX)); // Goes from 1/4 to 1, increasing as speed approaches 0.
				float addedSpeed = curveMultiplier*(m_Acceleration-slopeMultiplier);
				pVel = (pVel.normalized)*(rawSpeed-2*addedSpeed);
			}

			//float modifier = Mathf.Abs(pVel.x/pVel.y);
			//print("SLOPE MODIFIER: " + modifier);
			//pVel = pVel/(1.25f);
		}

		Vector2 downSlope = pVel.normalized; // Normal vector pointing down the current slope!
		if (downSlope.y > 0) //Make sure the vector is descending.
		{
			downSlope *= -1;
		}



		if(downSlope == Vector2.zero)
		{
			downSlope = Vector2.down;
		}

		pVel += downSlope*m_SlippingAcceleration*slopeMultiplier;
			//ChangeSpeedLinear(pVel, );
		//print("PostTraction velocity: "+pVel);
	}

	private void ToLeftWall(RaycastHit2D leftCheck) 
	{ //Sets the new position of the player and their leftNormal.

		//print ("We've hit LeftWall, sir!!");
		//print ("leftCheck.normal=" + leftCheck.normal);
		//print("preleftwall Pos:" + this.transform.position);

		m_LeftWalled = true;
		Vector2 setCharPos = leftCheck.point;
		setCharPos.x += (m_LeftSideLength-m_MinEmbed); //Embed slightly in wall to ensure raycasts still hit wall.
		//setCharPos.y -= m_MinEmbed;
		//print("Sent to Pos:" + setCharPos);

		this.transform.position = setCharPos;

		//print ("Final Position:  " + this.transform.position);

		RaycastHit2D leftCheck2 = Physics2D.Raycast(this.transform.position, Vector2.left, m_LeftSideLength, mask);
		if (leftCheck2.collider != null) 
		{
			if(antiTunneling){
				Vector2 surfacePosition = leftCheck2.point;
				surfacePosition.x += m_LeftSideLength-m_MinEmbed;
				//print("Sent to Pos:" + surfacePosition);
				this.transform.position = surfacePosition;
			}
		}
		else
		{
			//print ("Impact Pos:  " + groundCheck.point);
			//print("Reflected back into the air!");
			//print("Transform position: " + this.transform.position);
			//print("RB2D position: " + m_Rigidbody2D.position);
			//print("Velocity : " + pVel);
			//print("Speed : " + pVel.magnitude);
			//print(" ");
			//print(" ");	
			m_LeftWalled = false;
		}
			
		//groundNormal = groundCheck2.normal;
		//roundNormal = groundCheck.normal;


		leftNormal = leftCheck2.normal;

		//print ("Final Position2:  " + this.transform.position);
	}

	private void ToRightWall(RaycastHit2D rightCheck) 
	{ //Sets the new position of the player and their rightNormal.

		print ("We've hit RightWall, sir!!");
		//print ("groundCheck.normal=" + groundCheck.normal);
		//print("prerightwall Pos:" + this.transform.position);

		m_RightWalled = true;
		Vector2 setCharPos = rightCheck.point;
		setCharPos.x -= m_RightSideLength-m_MinEmbed; //Embed slightly in wall to ensure raycasts still hit wall.
		//setCharPos.y -= m_MinEmbed;  //Embed slightly in floor to ensure raycasts still hit floor.

		//print("Sent to Pos:" + setCharPos);
		//print("Sent to normal:" + groundCheck.normal);

		this.transform.position = setCharPos;

		//print ("Final Position:  " + this.transform.position);

		RaycastHit2D rightCheck2 = Physics2D.Raycast(this.transform.position, Vector2.right, m_RightSideLength, mask);
		if (rightCheck2.collider != null) 
		{
			if(antiTunneling){
				Vector2 surfacePosition = rightCheck2.point;
				surfacePosition.x -= m_RightSideLength-m_MinEmbed;
				//print("Sent to Pos:" + surfacePosition);
				this.transform.position = surfacePosition;
			}
		}
		else
		{
			//print ("Impact Pos:  " + groundCheck.point);
			//print("Reflected back into the air!");
			//print("Transform position: " + this.transform.position);
			//print("RB2D position: " + m_Rigidbody2D.position);
			//print("Velocity : " + pVel);
			//print("Speed : " + pVel.magnitude);
			//print(" ");
			//print(" ");	
			m_RightWalled = false;
		}

		/*
		if(rightCheck.normal.y == 0f)
		{//If vertical surface
			if(pVel.x > 0)
			{
				pVel = new Vector2(0, pVel.y);
			}
			//throw new Exception("Existence is suffering");
			print("LEFT VERTICAL");
		}
		*/

		//groundNormal = groundCheck2.normal;
		//roundNormal = groundCheck.normal;


		rightNormal = rightCheck2.normal;
		//print ("Final Position2:  " + this.transform.position);
	}
		
	private bool AirToCeiling()
	{
		//print("Collision");
		Vector2 ceilingPosition;

		if(pVel.x < 0)
		{
			//print("              MOVING LEFT!");
			ceilingPosition.x = m_CeilingFoot.position.x;
		}
		else if(pVel.x > 0)
		{
			//print("              MOVING RIGHT!");
			ceilingPosition.x = m_CeilingFoot.position.x;
		}
		else
		{
			//print("alternate");
			ceilingPosition.x = m_CeilingFoot.position.x;
		}

		//ceilingPosition.x = m_CeilingFoot.position.x;
		ceilingPosition.y = m_CeilingFoot.position.y;
		//Vector2 futureMove = pVelPerFrame
		//futureColliderPos.x += futureMove

		RaycastHit2D ceilingCheck = Physics2D.Raycast(ceilingPosition, pVel, pVelPerFrame.magnitude, mask);
		//print("Collision normal: "+ ceilingCheck.normal);


		if(ceilingCheck.collider != null)
		{
			//print("Head collision.");
			//print("Original Velocity: "+ pVel);
			//print("Original location: "+ ceilingPosition);
			//print("Predicted location: "+ ceilingCheck.point);
			//print("Struck object: " + ceilingCheck.collider.transform.gameObject);
			//ceilingCheck.collider.transform.gameObject.SetActive(false);

			Vector2 originalPos = this.transform.position;

			float hOffset = 0;
			float vOffset = -m_CeilingFootLength;

			if(pVel.y != 0)
			{
				vOffset = vOffset - m_MinEmbed;
			}
			else
			{
				vOffset = vOffset - m_MinEmbed;
			}


			if(pVel.x < 0)
			{
				//print("HOFFSET NEG VELO");
				hOffset = -m_MinEmbed;
			}
			else if(pVel.x > 0)
			{
				//print("HOFFSET POS VELO");
				hOffset = m_MinEmbed;
			}

			if(ceilingCheck.normal.y == 0)
			{//If hitting a vertical wall.

				print("Head hit wall. Ow!");
				return false;

				if(pVel.x < 0)
				{
					m_LeftWalled = true;
					this.transform.position =  new Vector2(ceilingCheck.point.x+(m_LeftSideLength-m_MinEmbed), ceilingCheck.point.y-m_CeilingFootLength);
				}
				else if(pVel.x > 0)
				{
					m_RightWalled = true;
					this.transform.position =  new Vector2(ceilingCheck.point.x-(m_RightSideLength-m_MinEmbed), ceilingCheck.point.y-m_CeilingFootLength);
				}
				else
				{
					print("Hit a wall without moving horizontally, somehow.");
				}
				pVel = new Vector2(0f, pVel.y);
			}
			else
			{//If hitting a downward facing surface
				this.transform.position = new Vector2(ceilingCheck.point.x+hOffset, ceilingCheck.point.y+vOffset);//+vOffset);
				//print("normal.y != 0");

				//Vector2 adjustedTop = m_CeilingFoot.position; // AdjustedBot marks the top of the middle ceiling raycast.
				//adjustedTop.y -= m_MaxEmbed;

				m_Ceilinged = true;
				ceilingNormal = ceilingCheck.normal;
				if(m_Grounded)
				{
					Wedged();
				}

				//print("After collision, Ceilinged = true, Grounded = " + m_Grounded);

				/*
				RaycastHit2D ceilingCheck2 = Physics2D.Raycast(adjustedBot, Vector2.up, 0.42f, mask);
				if (ceilingCheck2.collider != null) {
					print("THIS CODE RUNS I GUESS HEH");
					m_Ceilinged = true;
					//print ("ceilingCheck2.normal=" + ceilingCheck2.normal);
					//print ("ceilingCheck2.collider=" + ceilingCheck2.transform.gameObject);
					ceilingNormal  = ceilingCheck2.normal;
					//Traction(CtrlH);
					//Collision();
				} 
				else 
				{
					m_Ceilinged = false;
					//print ("GroundCheck2=null!");
				}
				*/
			}

			//this.transform.position =  new Vector2(ceilingCheck.point.x+m_MinEmbed, ceilingCheck.point.y+0.2f);
			//pVel = new Vector2(0f, pVel.y);
			//Debug.Break();
			return true;
		}
		else
		{
			return false;
		}
	}

	private Vector2 ChangeSpeedMult(Vector2 inputVelocity, float multiplier)
	{
		Vector2 newVelocity;
		float speed = inputVelocity.magnitude*multiplier;
		Vector2 direction = inputVelocity.normalized;
		newVelocity = direction * speed;
		return newVelocity;
	}

	private Vector2 ChangeSpeedLinear(Vector2 inputVelocity, float changeAmount)
	{
		Vector2 newVelocity;
		float speed = inputVelocity.magnitude+changeAmount;
		Vector2 direction = inputVelocity.normalized;
		newVelocity = direction * speed;
		return newVelocity;
	}

	private Vector2 SetSpeed(Vector2 inputVelocity, float speed)
	{
		//print("SetSpeed");
		Vector2 newVelocity;
		Vector2 direction = inputVelocity.normalized;
		newVelocity = direction * speed;
		return newVelocity;
	}

	private void GroundToGround(RaycastHit2D groundCheck) 
	{ //Sets the new position of the player and their ground normal.
		
		//float testNumber = groundCheck.normal.y/groundCheck.normal.x;
		//print(testNumber);
		//print ("We've hit slope, sir!!");
		//print ("groundCheck.normal=" + groundCheck.normal);

		m_Grounded = true;
		Vector2 setCharPos = groundCheck.point;
		setCharPos.y = setCharPos.y+m_FloorFootLength-m_MinEmbed; //Embed slightly in floor to ensure raycasts still hit floor.
		this.transform.position = setCharPos;

		//print("Sent to Pos:" + setCharPos);
		//print("Sent to normal:" + groundCheck.normal);

	
		//print ("Final Position:  " + this.transform.position);

		RaycastHit2D groundCheck2 = Physics2D.Raycast(this.transform.position, Vector2.down, m_FloorFootLength, mask);
		if (groundCheck2.collider != null) 
		{
			if(antiTunneling){
				Vector2 surfacePosition = groundCheck2.point;
				surfacePosition.y += m_FloorFootLength-m_MinEmbed;
				this.transform.position = surfacePosition;
			}
		}
		else
		{
			//print ("Impact Pos:  " + groundCheck.point);
			//print("Reflected back into the air!");
			//print("Transform position: " + this.transform.position);
			//print("RB2D position: " + m_Rigidbody2D.position);
			//print("Velocity : " + pVel);
			//print("Speed : " + pVel.magnitude);
			//print(" ");
			//print(" ");	
			m_Grounded = false;
		}

		if(groundCheck.normal.y == 0f)
		{//If vertical surface
			//throw new Exception("Existence is suffering");
			print("GtG VERTICAL :O");
		}

		groundNormal = groundCheck2.normal;
		//print ("Final Position2:  " + this.transform.position);
	}

	private void DirectionChange(Vector2 newNormal)
	{
		//print("DirectionChange");
		Vector2 initialDirection = pVel.normalized;
		Vector2 newPerp = Perp(newNormal);
		Vector2 AdjustedVel;

		float initialSpeed = pVel.magnitude;
		//print("Speed before : " + initialSpeed);
		float testNumber = newPerp.y/newPerp.x;
		//print(testNumber);
		//print("newPerp="+newPerp);
		if(float.IsNaN(testNumber))
		{
			print("IT'S NaN BRO LoLoLOL XD");
			//throw new Exception("NaN value.");
			//print("X = "+ newNormal.x +", Y = " + newNormal.y);
		}
			
		//newPerp.x = newNormal.y;
		//newPerp.y = -newNormal.x;

		if((initialDirection == newPerp)||initialDirection == Vector2.zero)
		{
			//print("same angle BITCH");
			return;
		}
		else
		{
			//print("Different lul. Init: "+initialDirection+", gPerp: "+newPerp+".");
		}

		//print("InitialDirection: "+initialDirection);
		//print("GroundDirection: "+newPerp);

		float impactAngle = Vector2.Angle(initialDirection,newPerp);
		//print("TrueimpactAngle: " +impactAngle);

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
			projectionVal = Vector2.Dot(pVel, newPerp)/newPerp.sqrMagnitude;
		}
		//print("P"+projectionVal);
		AdjustedVel = newPerp * projectionVal;
		//	print("A"+AdjustedVel);

		if(pVel == Vector2.zero)
		{
			//pVel = new Vector2(h, pVel.y);
		}
		else
		{
			//pVel = AdjustedVel + AdjustedVel.normalized*h;
			try
			{
				pVel = AdjustedVel;
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

		if(impactAngle <= m_AngleSpeedLossMin)
		{ // Angle lower than min, no speed penalty.
			speedLossMult = 1;
		}
		else if(impactAngle < m_AngleSpeedLossMax)
		{ // In the midrange, administering momentum loss on a curve leading from min to max.
			speedLossMult = 1-Mathf.Pow((impactAngle-m_AngleSpeedLossMin)/(m_AngleSpeedLossMax-m_AngleSpeedLossMin),2); // See Workflowy notes section for details on this formula.
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
		pVel = SetSpeed(pVel , initialSpeed*speedLossMult);
		remainingMovement = pVel*remainingVelMult*Time.fixedDeltaTime;
		//print ("GT Vel:  " + pVel);
	}

	private void CameraControl()
	{
		cameraZoom = Mathf.Lerp(cameraZoom, pVel.magnitude, 0.1f);
		m_MainCamera.orthographicSize = 5f+(0.15f*cameraZoom);
	}

	private void Wedged()
	{//Executes when the player is moving into a corner and there isn't enough headroom to go any further. It halts the player's momentum and sets off a wall-touching flag.

		print("Wedged!");
		RaycastHit2D ceilingHit;
		RaycastHit2D floorHit = Physics2D.Raycast(this.transform.position, Vector2.down, m_FloorFootLength, mask);
		float embedDepth;
		Vector2 gPerp;
		Vector2 cPerp;
		Vector2 moveAmount = new Vector2(0,0);

		if(floorHit.collider == null)
		{
			//print("Bottom not wedged, using newNormal!");
			gPerp.x = groundNormal.x;
			gPerp.y = groundNormal.y;
		}
		else
		{
			gPerp.x = floorHit.normal.y;
			gPerp.y = -floorHit.normal.x;
			Vector2 floorPosition = floorHit.point;
			floorPosition.y += m_FloorFootLength-m_MinEmbed;
			this.transform.position = floorPosition;
			//print("Hitting bottom, shifting up!");
		}

		ceilingHit = Physics2D.Raycast(this.transform.position, Vector2.up, m_CeilingFootLength, mask);

		if(ceilingHit.collider == null)
		{
			throw new Exception("Top not wedged!");
			return;
		}
		else
		{
			//print("Hitting top, superunwedging..."); 
			cPerp.x = ceilingHit.normal.y;
			cPerp.y = -ceilingHit.normal.x;
		}
			
		embedDepth = m_CeilingFootLength-ceilingHit.distance;
		//print("Embedded ("+embedDepth+") units into the ceiling");

		//print("Ground Perp = " + gPerp);
		//print("Ceiling Perp = " + cPerp);

		if(cPerp.y > 0)
		{
			if(pVel.x > 0)
			{
				moveAmount = SuperUnwedger(cPerp, gPerp, false, embedDepth);
				pVel = new Vector2(0f, 0f);
				m_RightWallBlocked = true;
				//print("Right wedge!");
				//print("cPerp: "+cPerp);
				//print("gPerp: "+gPerp);
			}
		}
		else if(cPerp.y < 0)
		{
			if(pVel.x < 0)
			{
				moveAmount = SuperUnwedger(cPerp, gPerp, true, embedDepth);
				//print("Left wedge!");
				//print("cPerp: "+cPerp);
				//print("gPerp: "+gPerp);
				m_LeftWallBlocked = true;
				pVel = new Vector2(0f, 0f);
			}
		}
		else
		{
			//throw new Exception("Ceiling is vertical! CEILINGS CAN'T BE VERTICAL!");
		}

		this.transform.position = new Vector2((this.transform.position.x + moveAmount.x), (this.transform.position.y + moveAmount.y));
		//m_Ceilinged = false;

	}

	private Vector2	Perp(Vector2 input)
	{
		Vector2 output;
		output.x = input.y;
		output.y = -input.x;
		return output;
	}		

	private void UpdateContactNormals(bool posCorrection)
	{
		m_Grounded = false;
		m_Ceilinged = false;

		//m_SpriteRenderer.color = Color.white;
		m_LeftLine.endColor = Color.red;
		m_LeftLine.startColor = Color.red;
		m_RightLine.endColor = Color.red;
		m_RightLine.startColor = Color.red;
		m_FloorLine.endColor = Color.red;
		m_FloorLine.startColor = Color.red;
		m_CeilingLine.endColor = Color.red;
		m_CeilingLine.startColor = Color.red;
		m_LeftSideLine.endColor = Color.red;
		m_LeftSideLine.startColor = Color.red;
		m_RightSideLine.endColor = Color.red;
		m_RightSideLine.startColor = Color.red;

		if (CtrlH < 0) 
		{
			facingDirection = false; //true means right (the direction), false means left.
		} 
		else if (CtrlH > 0)
		{
			facingDirection = true; //true means right (the direction), false means left.
		}


		RaycastHit2D leftSideHit = Physics2D.Raycast (this.transform.position, Vector2.left, m_LeftSideLength, mask);
		if (leftSideHit.collider != null)
		{
			leftNormal = leftSideHit.normal;
			leftSideContact = true;
			m_LeftSideLine.endColor = Color.green;
			m_LeftSideLine.startColor = Color.green;
		} 
		else
		{
			leftSideContact = false;
		}

		if (leftSideContact) 
		{
			//print ("leftSideContact" + leftSideHit.normal);
			if(!rightSideContact && antiTunneling && posCorrection && ((m_LeftSideLength-leftSideHit.distance)>=0.011f))
			{

				Vector2 surfacePosition = leftSideHit.point;
				surfacePosition.x += ((m_LeftSideLength)-m_MinEmbed);
				this.transform.position = surfacePosition;
				//print("DISTANCE TO HIT:"+leftSideHit.distance);
				//print("Remaining distance:" + (m_LeftSideLength-leftSideHit.distance));
				//print ("ANTIWALLING " + this.transform.position.y);
				//print ("ANTIWALLING " + this.transform.position.x);
			}
			print("leftwalled added");
			m_LeftWalled = true;
		}
		else 
		{
			m_LeftWalled = false;
		}

		RaycastHit2D rightSideHit = Physics2D.Raycast (this.transform.position, Vector2.right, m_RightSideLength, mask);
		if (rightSideHit.collider != null)
		{
			rightNormal = rightSideHit.normal;
			rightSideContact = true;
			m_RightSideLine.endColor = Color.green;
			m_RightSideLine.startColor = Color.green;
		} 
		else
		{
			rightSideContact = false;
		}

		if (rightSideContact) 
		{
			//print ("leftSideContact" + leftSideHit.normal);
			if(!leftSideContact && antiTunneling && posCorrection && ((m_RightSideLength-rightSideHit.distance)>=0.011f))
			{
				Vector2 surfacePosition = rightSideHit.point;
				surfacePosition.x -= ((m_RightSideLength)-m_MinEmbed);
				surfacePosition.y -= m_MinEmbed;
				this.transform.position = surfacePosition;

				//print ("ANTIWALLING " + (2*m_RightSideLength));
			}
			m_RightWalled = true;
		}
		else 
		{
			m_RightWalled = false;
		}
			
		RaycastHit2D floorHit = Physics2D.Raycast(this.transform.position, Vector2.down, m_FloorFootLength, mask);

		//print("Floormal="+Perp(floorHit.normal).y);
		//print("pVel.n.y="+pVel.normalized.y);
		//searchkey: neotokyo


		if (floorHit.collider != null) 
		{
			//m_Grounded = true;
			floorContact = true;
			m_FloorLine.endColor = Color.green;
			m_FloorLine.startColor = Color.green;
			groundNormal = floorHit.normal;
		} 
		else 
		{
			floorContact = false;
		}

		if (floorContact) 
		{
			if(!ceilingContact && antiTunneling && posCorrection && ((m_FloorFootLength-floorHit.distance)>=0.011f))
			{
				Vector2 surfacePosition = floorHit.point;
				surfacePosition.y += (m_FloorFootLength-m_MinEmbed);
				this.transform.position = surfacePosition;
				//print ("floorHit NORMAL INITIAL:    " + floorHit.normal);
				//print ("ANTIFLOORING " + (m_FloorFootLength-floorHit.distance));
			}
			m_Grounded = true;
		}
		else 
		{
			m_Grounded = false;
		}


		RaycastHit2D ceilingHit = Physics2D.Raycast (this.transform.position, Vector2.up,  m_CeilingFootLength, mask);
		if (ceilingHit.collider != null) 
		{
			//m_Ceilinged = true;
			ceilingContact = true;
			m_CeilingLine.endColor = Color.green;
			m_CeilingLine.startColor = Color.green;
			ceilingNormal = ceilingHit.normal;
		} 
		else 
		{
			//m_Ceilinged = false;
			ceilingContact = false;
		}

		if (ceilingContact) 
		{
			if(!floorContact && antiTunneling && posCorrection && ((m_CeilingFootLength-ceilingHit.distance)>=0.011f))
			{
				Vector2 surfacePosition = ceilingHit.point;
				surfacePosition.y -= (m_CeilingFootLength-m_MinEmbed);
				this.transform.position = surfacePosition;
				//print ("HEAD IMPACTED IN SURFACE " + ceilingHit.normal);
			}
			m_Ceilinged = true;
		}
		else 
		{
			m_Ceilinged = false;
		}

		RaycastHit2D RightFootHit = Physics2D.Raycast (this.transform.position, Vector2.down,  m_RightFootLength, mask);
		if (RightFootHit.collider != null) 
		{
			rightFootContact = true;
			m_RightLine.endColor = Color.green;
			m_RightLine.startColor = Color.green;
		} 
		else 
		{
			rightFootContact = false;
		}

		RaycastHit2D LeftFootHit = Physics2D.Raycast (this.transform.position, Vector2.down, m_LeftFootLength, mask);
		if (LeftFootHit.collider != null)
		{
			leftFootContact = true;
			m_LeftLine.endColor = Color.green;
			m_LeftLine.startColor = Color.green;
		} 
		else
		{
			leftFootContact = false;
		}

	}

	private Vector2 SuperUnwedger(Vector2 cPerp, Vector2 gPerp, bool cornerIsLeft, float embedDistance)
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
					throw new Exception("You're unwedging from a wall, that's not allowed. Wall corners aren't wedgy enough.");
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
					throw new Exception("Your floor has no horizontality. What are you even doing?");
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
					throw new Exception("You're unwedging from a wall, that's not allowed. Wall corners aren't wedgy enough.");
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
					throw new Exception("Your floor has no horizontality. What are you even doing?");
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

	private void Jump(float horizontalInput)
	{
		if(m_Grounded&&m_Ceilinged)
		{
			print("Grounded and Ceilinged, nowhere to jump!");
		}
		else if(m_Grounded)
		{
			if(pVel.y >= 0)
			{
				pVel = new Vector2(pVel.x+(m_HJumpForce*horizontalInput), pVel.y+m_VJumpForce);
			}
			else
			{
				pVel = new Vector2(pVel.x+(m_HJumpForce*horizontalInput), m_VJumpForce);
			}
			m_Jump = false;
			m_Grounded = false;
		}
		else if(m_Ceilinged)
		{
			if(pVel.y <= 0)
			{
				pVel = new Vector2(pVel.x+(m_HJumpForce*horizontalInput), pVel.y -m_VJumpForce);
			}
			else
			{
				pVel = new Vector2(pVel.x+(m_HJumpForce*horizontalInput), -m_VJumpForce);
			}

			m_Jump = false;
			m_Ceilinged = false;
		}
		else
		{
			print("Can't jump, airborne!");
		}
	}
}