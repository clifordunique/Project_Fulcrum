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

	[SerializeField][ReadOnlyAttribute]private bool leftContact;
	[SerializeField][ReadOnlyAttribute]private bool floorContact;
	[SerializeField][ReadOnlyAttribute]private bool rightContact;
	[SerializeField][ReadOnlyAttribute]private bool ceilingContact;
	[SerializeField][ReadOnlyAttribute]private bool leftSideContact;
	[SerializeField][ReadOnlyAttribute]private bool rightSideContact;
	[Space(10)]
	[SerializeField][ReadOnlyAttribute]private bool m_Grounded;
	[SerializeField][ReadOnlyAttribute]private bool m_NearGround; 
	[SerializeField][ReadOnlyAttribute]private bool m_Ceilinged; 
	[SerializeField][ReadOnlyAttribute]private bool m_NearCeiling; 
	[SerializeField][ReadOnlyAttribute]private bool m_LeftWalled; 
	[SerializeField][ReadOnlyAttribute]private bool m_RightWalled; 

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
		//print("Initial Pos: " + this.transform.position);
		//print("Initial Vel: " + m_Rigidbody2D.velocity);

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

		#region raytesting

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

		RaycastHit2D LeftSideHit = Physics2D.Raycast (this.transform.position, Vector2.left, m_LeftSideLength, mask);
		if (LeftSideHit.collider != null)
		{
			leftNormal = LeftSideHit.normal;
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
			//print ("leftSideContact" + LeftSideHit.normal);
			if(!rightSideContact && antiTunneling)
			{
				Vector2 surfacePosition = LeftSideHit.point;
				surfacePosition.x += ((m_LeftSideLength)-m_MinEmbed);
				this.transform.position = surfacePosition;
				//print ("ANTIWALLING " + (2*m_LeftSideLength));
			}
			m_LeftWalled = true;
		}
		else 
		{
			m_LeftWalled = false;
		}

		RaycastHit2D RightSideHit = Physics2D.Raycast (this.transform.position, Vector2.right, m_RightSideLength, mask);
		if (RightSideHit.collider != null)
		{
			rightNormal = RightSideHit.normal;
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
			//print ("leftSideContact" + LeftSideHit.normal);
			if(!leftSideContact && antiTunneling)
			{
				Vector2 surfacePosition = RightSideHit.point;
				surfacePosition.x -= ((m_RightSideLength)-m_MinEmbed);
				this.transform.position = surfacePosition;
				//print ("ANTIWALLING " + (2*m_RightSideLength));
			}
			m_RightWalled = true;
		}
		else 
		{
			m_RightWalled = false;
		}

		RaycastHit2D LeftHit = Physics2D.Raycast (this.transform.position, Vector2.down, m_LeftFootLength, mask);
		if (LeftHit.collider != null)
		{
			leftContact = true;
			m_LeftLine.endColor = Color.green;
			m_LeftLine.startColor = Color.green;
		} 
		else
		{
			leftContact = false;
		}

		RaycastHit2D floorHit = Physics2D.Raycast(this.transform.position, Vector2.down, m_FloorFootLength, mask);
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
			//m_Grounded = false;
			floorContact = false;
		}

		if (floorContact) 
		{
			if(!ceilingContact && antiTunneling)
			{
				Vector2 surfacePosition = floorHit.point;
				surfacePosition.y += (m_FloorFootLength-m_MinEmbed);
				this.transform.position = surfacePosition;
				//print ("floorHit NORMAL INITIAL:    " + floorHit.normal);
			}
			m_Grounded = true;
		}
		else 
		{
			m_Grounded = false;
		}
			
		RaycastHit2D RightHit = Physics2D.Raycast (this.transform.position, Vector2.down,  m_RightFootLength, mask);
		if (RightHit.collider != null) 
		{
			rightContact = true;
			m_RightLine.endColor = Color.green;
			m_RightLine.startColor = Color.green;
		} 
		else 
		{
			rightContact = false;
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
			if(!floorContact && antiTunneling)
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



		#endregion


		if(m_Ceilinged&&m_Grounded)
		{
			if(m_Rigidbody2D.velocity.magnitude != 0)
			{
				Wedged();
			}
			m_Ceilinged = false;
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
			m_Rigidbody2D.velocity = new Vector2 (m_Rigidbody2D.velocity.x, m_Rigidbody2D.velocity.y - 1);
			m_Ceilinged = false;
		}

		errorDetectingRecursionCount = 0; //Used for DirectionCorrection();

		//print("Velocity before directioncorrection: "+m_Rigidbody2D.velocity);
		//print("Position before directioncorrection: "+this.transform.position);

		DirectionCorrection();

		if(m_LeftWalled&&!m_Ceilinged&&!m_Grounded)
		{
			//print("LeftWallMovement");
			Traversal(leftNormal);
		}
		else
		{
			//print("SKIPPEDSKIPPEDSKIPPEDSKIPPED");
		}

		if(m_RightWalled&&!m_Ceilinged&&!m_Grounded)
		{
			//print("RightWallMovement");
			Traversal(rightNormal);
		}

		if(m_Ceilinged)
		{
			//print("CeilingMovement");
			Traversal(ceilingNormal);
		}

		if (m_Grounded) //Handles velocity along ground surface.
		{
			//print("GroundMovement");
			Traversal(groundNormal);
		}

		//print("Speed this frame: "+m_Rigidbody2D.velocity.magnitude);
		//print("Per frame velocity at end of physics frame: "+m_Rigidbody2D.velocity*Time.fixedDeltaTime);
		//print("Pos at end of physics frame: "+this.transform.position);
		//print("##############################################################################################");



		#region Animator Controls

		//
		//Animator Controls
		//

		m_Anim.SetBool("Walled", false);

		if(m_LeftWalled)
		{
			m_Anim.SetBool("Walled", true);
			facingDirection = false;
		}

		if(m_RightWalled)
		{
			m_Anim.SetBool("Walled", true);
			facingDirection = true;
		}

		if (!facingDirection) //If facing left
		{
			//print("FACING LEFT!   "+h)
			m_PlayerSprite.transform.localScale = new Vector3 (-1f, 1f, 1f);
			if(m_Rigidbody2D.velocity.x > 0)
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
			if(m_Rigidbody2D.velocity.x < 0)
			{
				m_Anim.SetBool("Crouch", true);
			}
			else
			{
				m_Anim.SetBool("Crouch", false);
			}
		}
			
		Vector2 debugLineVector = ((m_Rigidbody2D.velocity*Time.fixedDeltaTime));
		debugLineVector.y -= m_FloorFootLength-m_MaxEmbed;
		m_DebugLine.SetPosition(1, debugLineVector);

		m_Anim.SetFloat("Speed", m_Rigidbody2D.velocity.magnitude);

		if(m_Rigidbody2D.velocity.magnitude >= tractionChangeThreshold )
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

		if(m_Rigidbody2D.velocity.magnitude > 20.0f)
		{
			multiplier = ((m_Rigidbody2D.velocity.magnitude - 20) / 20)+1;
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
	
		//print("FinaL Pos: " + this.transform.position);
		//print("FinaL Vel: " + m_Rigidbody2D.velocity);
		
		//print("Speed at end of frame: " + m_Rigidbody2D.velocity.magnitude);
		#endregion
    }

	private void Update()
	{
		m_Speedometer.text = ""+Math.Round(m_Rigidbody2D.velocity.magnitude,0);
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

	private void DirectionCorrection()
	{
		float crntSpeed = m_Rigidbody2D.velocity.magnitude*Time.fixedDeltaTime; //Current speed.
		//print("DC Executing");
		errorDetectingRecursionCount++;

		if(errorDetectingRecursionCount >= 5)
		{
			throw new Exception("Your recursion code is fucked!");
			return;
		}


		if(m_Rigidbody2D.velocity.x > 0)
		{
			m_LeftWalled = false;
		}

		if(m_Rigidbody2D.velocity.x < 0)
		{
			m_RightWalled = false;
		}

	
		Vector2 adjustedBot = m_FloorFoot.position; // AdjustedBot marks the end of the floor raycast, but 0.02 shorter.
		adjustedBot.y += m_MaxEmbed;

		Vector2 adjustedTop = m_CeilingFoot.position; // AdjustedTop marks the end of the ceiling raycast, but 0.02 shorter.
		adjustedTop.y -= m_MaxEmbed;

		Vector2 adjustedLeft = m_LeftSide.position; // AdjustedLeft marks the end of the left wall raycast, but 0.02 shorter.
		adjustedLeft.y += m_MaxEmbed;

		Vector2 adjustedRight = m_RightSide.position; // AdjustedRight marks the end of the right wall raycast, but 0.02 shorter.
		adjustedRight.y -= m_MaxEmbed;

		//RaycastHit2D groundCheck = Physics2D.Raycast(this.transform.position, Vector2.down, m_FloorFootLength, mask);
		RaycastHit2D groundCheck = Physics2D.Raycast(adjustedBot, m_Rigidbody2D.velocity, crntSpeed, mask);
		RaycastHit2D ceilingCheck = Physics2D.Raycast(adjustedTop, m_Rigidbody2D.velocity, crntSpeed, mask);
		RaycastHit2D leftCheck = Physics2D.Raycast(adjustedLeft, m_Rigidbody2D.velocity, crntSpeed, mask);
		RaycastHit2D rightCheck = Physics2D.Raycast(adjustedRight, m_Rigidbody2D.velocity, crntSpeed, mask);

		float gDist = groundCheck.distance;
		float cDist = ceilingCheck.distance;
		float lDist = leftCheck.distance;
		float rDist = rightCheck.distance;

		float shortestDistV = 0;
		float shortestDistH = 0;
		float shortestDist = 0;


		//Shortest non-zero vertical collision.
		if(gDist != 0 && cDist != 0)
		{
			shortestDistV = Math.Min(gDist, cDist);
		}
		else if(gDist != 0)
		{
			shortestDistV = gDist;
		}
		else if(cDist != 0)
		{
			shortestDistV = cDist;
		}

		//Shortest non-zero horizontal collision.
		if(lDist != 0 && rDist != 0)
		{
			shortestDistH = Math.Min(lDist, rDist);
		}
		else if(lDist != 0)
		{
			shortestDistH = lDist;
		}
		else if(rDist != 0)
		{
			shortestDistH = rDist;
		}

		//Total non-zero shortest of all four colliders.
		if(shortestDistV != 0 && shortestDistH != 0)
		{
			shortestDist = Math.Min(shortestDistV, shortestDistH);
		}
		else if(shortestDistV != 0)
		{
			shortestDist = shortestDistV;
		}
		else if(shortestDistH != 0)
		{
			shortestDist = shortestDistH;
		}

		//print("G="+gDist+" C="+cDist+" R="+rDist+" L="+lDist);
		//print("VDist: "+shortestDistV);
		//print("HDist: "+shortestDistH);

		// THIS SYSTEM IS NOT OPTIMAL! REPLACE IT WITH SOMETHING ELSE LATER. SOMETHING THAT JUST CHOOSES FROM THE ORIGINAL RAYCASTS.
		groundCheck = Physics2D.Raycast(adjustedBot, m_Rigidbody2D.velocity, shortestDist, mask); 	
		ceilingCheck = Physics2D.Raycast(adjustedTop, m_Rigidbody2D.velocity, shortestDist, mask);
		leftCheck = Physics2D.Raycast(adjustedLeft, m_Rigidbody2D.velocity, shortestDist, mask);
		rightCheck = Physics2D.Raycast(adjustedRight, m_Rigidbody2D.velocity, shortestDist, mask);

		int collisionNum = 0;

		if(leftCheck)
		{
			collisionNum++;
		}
		if(rightCheck)
		{
			collisionNum++;
		}
		if(ceilingCheck)
		{
			collisionNum++;
		}
		if(groundCheck)
		{
			collisionNum++;
		}

		if(collisionNum>0)
		{
			//print("TOTAL COLLISIONS: "+collisionNum);
		}

		if(leftCheck&&rightCheck)
		{
			throw new Exception("ERROR: Vertical wedge fix not yet implemented. Unhandled physics interaction.");
		}
			

		//Find shortest of the 4 checks. If 2 are both very small, choose the one with the steepest vertical angle.

		if(leftCheck)
		{
			print("ToLeftWall");
			ToLeftWall(leftCheck);
			return;
			if(groundCheck) //If hitting wall and ground.
			{
				

			}
			else if(ceilingCheck) //If hitting wall and ceiling.
			{
				
			}
			else
			{ //If simply hitting wall.
				
				//return;
			}
		}

		if (groundCheck) 
		{//If you're going to hit something with your feet.

			Vector2 testperp = new Vector2(groundCheck.normal.y,-groundCheck.normal.x);

			float steepnessAngle = Vector2.Angle(Vector2.right,testperp);

			//print("groundAngle:"+steepnessAngle);
			#region collision debug
			//GameObject collisionPoint = new GameObject("Impact");
			//collisionPoint.transform.position = new Vector2(groundCheck.point.x, groundCheck.point.y);
			//GameObject rayStartPoint = new GameObject("Ray");
			//rayStartPoint.transform.position = new Vector2(adjustedBot.x, adjustedBot.y);
			//print("Impact!");
			//print ("Player Pos:  " + this.transform.position);
			//print ("velocity:  " + m_Rigidbody2D.velocity);
			#endregion

			//ceilingCheck = Physics2D.Raycast(adjustedTop, m_Rigidbody2D.velocity, groundCheck.distance, mask); //Collider checks go only as far as the first angle correction, so they don't mistake the slope of the floor for an obstacle.

			//print("Velocity before impact: "+m_Rigidbody2D.velocity);
			Vector2 moveDirectionNormal;
			moveDirectionNormal.x = m_Rigidbody2D.velocity.normalized.y;
			moveDirectionNormal.y = -m_Rigidbody2D.velocity.normalized.x;

			Vector2 invertedImpactNormal;//This is done in case one of the raycasts is inside the collider, which would cause it to return an inverted normal value.
			invertedImpactNormal.x = -groundCheck.normal.x; //Inverting the normal.
			invertedImpactNormal.y = -groundCheck.normal.y; //Inverting the normal.

			if(ceilingCheck.collider != null)
			{// If player hits something with their head.
				AirToCeiling(); //ATTEND TO THIS! SHOULD BYPASS A2C AND GO STRAIGHT TO WEDGE!
				return;
			}
			else if (groundCheck.collider == null) 
			{ // If player is airborne beforehand.
				AirToGround(groundCheck);
				return;
			} 
			else if ((moveDirectionNormal != groundCheck.normal) && (moveDirectionNormal != invertedImpactNormal)) 
			{ // If the slope you're hitting is different than your current slope.
				GroundToGround(groundCheck);
				return;
			}
			else 
			{
				//print ("("+moveDirectionNormal+")!=("+groundCheck.normal+")");
				//print ("("+moveDirectionNormal+")!=("+invertedImpactNormal+")");
				//print ("Same surface, no trajectory change needed");
				return;
			}
		} 
		else if(ceilingCheck.collider != null)
		{
			Vector2 testPerp2 = new Vector2(ceilingCheck.normal.y,-ceilingCheck.normal.x);

			float steepnessAngle2 = Vector2.Angle(Vector2.left,testPerp2);
			//print("ceilingAngle:"+steepnessAngle2);
			//print("Obstacle");
			//print("Free move into obstacle.");
			//print ("Player Pos:  " + this.transform.position);
			//print ("velocity:  " + m_Rigidbody2D.velocity);

			AirToCeiling();
			//print("Velocity after impact: "+m_Rigidbody2D.velocity);
		}
		else
		{
			//print("No collisions detected");
		}
	}
		
	private void Traction(float horizontalInput)
	{
		Vector2 groundPerp;
		groundPerp.x = groundNormal.y;
		groundPerp.y = -groundNormal.x;

		if(groundPerp.x > 0)
		{
			groundPerp*=-1;
		}

		float steepnessAngle = Vector2.Angle(Vector2.left,groundPerp);
		//print("SteepnessAngle:"+steepnessAngle);

		float slopeMultiplier = 0;
	
		if(steepnessAngle > m_TractionLossAngle)
		{
			slopeMultiplier = ((steepnessAngle-m_TractionLossAngle)/(90f-m_TractionLossAngle));

			//print("Ding! slopeMultiplier: "+ slopeMultiplier);
			//print("groundPerpY: "+groundPerpY+", slopeThreshold: "+slopeThreshold);
		}

		//print("Traction");
		if( ((m_LeftWalled)&&(horizontalInput < 0)) || ((m_RightWalled)&&(horizontalInput > 0)) )
		{// If running at a wall you're up against.
			horizontalInput = 0;
		}
		else
		{
			if((m_LeftWalled)&&(horizontalInput > 0))
			{
				m_LeftWalled = false;
			}
			if((m_RightWalled)&&(horizontalInput < 0))
			{
				m_RightWalled = false;
			}
		}

		//print("Traction executing");
		float rawSpeed = m_Rigidbody2D.velocity.magnitude;
		if (horizontalInput == 0) 
		{//if not pressing any move direction, slow to zero linearly.
			//print("No input, slowing...");
			if(rawSpeed <= 2)
			{
				m_Rigidbody2D.velocity = Vector2.zero;	
			}
			else
			{
				m_Rigidbody2D.velocity = ChangeSpeedLinear (m_Rigidbody2D.velocity, -m_LinearSlideRate);
			}
		}
		else if((horizontalInput > 0 && m_Rigidbody2D.velocity.x >= 0) || (horizontalInput < 0 && m_Rigidbody2D.velocity.x <= 0))
		{//if pressing same button as move direction, move to MAXSPEED.
			//print("Running with momentum.");
			//print("Moving with keypress");
			if(rawSpeed <= maxRunSpeed)
			{
				//print("Rawspeed("+rawSpeed+") less than max");
				if(rawSpeed > tractionChangeThreshold )
				{
					//print("LinAccel-> " + rawSpeed);
					if(m_Rigidbody2D.velocity.y > 0)
					{ 	// If climbing, recieve uphill movement penalty.
						m_Rigidbody2D.velocity = ChangeSpeedLinear(m_Rigidbody2D.velocity, m_LinearAccelRate*(1-slopeMultiplier));
					}
					else
					{
						m_Rigidbody2D.velocity = ChangeSpeedLinear(m_Rigidbody2D.velocity, m_LinearAccelRate);
					}
				}
				else if(rawSpeed == 0)
				{
					m_Rigidbody2D.velocity = new Vector2((m_Acceleration)*horizontalInput*(1-slopeMultiplier), 0);
					//print("Starting motion. Adding " + m_Acceleration);
				}
				else
				{
					//print("Accelerating");
					float eqnX = (1+Mathf.Abs((1/tractionChangeThreshold )*rawSpeed));
					float curveMultiplier = 1+(1/(eqnX*eqnX)); // Goes from 1/4 to 1, increasing as speed approaches 0.

					float addedSpeed = curveMultiplier*(m_Acceleration);
					if(m_Rigidbody2D.velocity.y > 0)
					{ // If climbing, recieve uphill movement penalty.
						addedSpeed = curveMultiplier*(m_Acceleration)*(1-slopeMultiplier);
					}
					m_Rigidbody2D.velocity = (m_Rigidbody2D.velocity.normalized)*(rawSpeed+addedSpeed);
				}
			}
			else
			{
				//print("Rawspeed("+rawSpeed+") more than max???");
			}
		}
		else if((horizontalInput > 0 && m_Rigidbody2D.velocity.x < 0) || (horizontalInput < 0 && m_Rigidbody2D.velocity.x > 0))
		{//if pressing button opposite of move direction, slow to zero exponentially.
			if(rawSpeed > tractionChangeThreshold )
			{
				//print("LinDecel");
				m_Rigidbody2D.velocity = ChangeSpeedLinear (m_Rigidbody2D.velocity, -m_LinearStopRate);
			}
			else
			{
				//print("Decelerating");
				float eqnX = (1+Mathf.Abs((1/tractionChangeThreshold )*rawSpeed));
				float curveMultiplier = 1+(1/(eqnX*eqnX)); // Goes from 1/4 to 1, increasing as speed approaches 0.
				float addedSpeed = curveMultiplier*(m_Acceleration-slopeMultiplier);
				m_Rigidbody2D.velocity = (m_Rigidbody2D.velocity.normalized)*(rawSpeed-2*addedSpeed);
			}

			//float modifier = Mathf.Abs(m_Rigidbody2D.velocity.x/m_Rigidbody2D.velocity.y);
			//print("SLOPE MODIFIER: " + modifier);
			//m_Rigidbody2D.velocity = m_Rigidbody2D.velocity/(1.25f);
		}

		Vector2 downSlope = m_Rigidbody2D.velocity.normalized; // Normal vector pointing down the current slope!
		if (downSlope.y > 0) //Make sure the vector is descending.
		{
			downSlope *= -1;
		}

		if(downSlope == Vector2.zero)
		{
			downSlope = Vector2.down;
		}

		m_Rigidbody2D.velocity += downSlope*m_SlippingAcceleration*slopeMultiplier;
			//ChangeSpeedLinear(m_Rigidbody2D.velocity, );
		//print("PostTraction velocity: "+m_Rigidbody2D.velocity);
	}

	private void ToLeftWall(RaycastHit2D leftCheck) 
	{ //Sets the new position of the player and their leftNormal.

		print ("We've hit LeftWall, sir!!");
		//print ("groundCheck.normal=" + groundCheck.normal);
		//print("preleftwall Pos:" + this.transform.position);

		m_LeftWalled = true;
		Vector2 setCharPos = leftCheck.point;
		setCharPos.x += m_LeftSideLength-m_MinEmbed; //Embed slightly in wall to ensure raycasts still hit wall.
		setCharPos.y -= m_MinEmbed;
		//print("Sent to Pos:" + setCharPos);
		//print("Sent to normal:" + groundCheck.normal);

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
			//print("Velocity : " + m_Rigidbody2D.velocity);
			//print("Speed : " + m_Rigidbody2D.velocity.magnitude);
			//print(" ");
			//print(" ");	
			m_LeftWalled = false;
		}

		if(leftCheck.normal.y == 0f)
		{//If vertical surface
			//throw new Exception("Existence is suffering");
			//print("LEFT VERTICAL");
		}
		//groundNormal = groundCheck2.normal;
		//roundNormal = groundCheck.normal;
		leftNormal = leftCheck2.normal;
		//print ("Final Position2:  " + this.transform.position);
	}

	private bool AirToCeiling()
	{
		//print("Collision");
		Vector2 ceilingPosition;

		if(m_Rigidbody2D.velocity.x < 0)
		{
			//print("              MOVING LEFT!");
			ceilingPosition.x = m_CeilingFoot.position.x;
		}
		else if(m_Rigidbody2D.velocity.x > 0)
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
		//Vector2 futureMove = m_Rigidbody2D.velocity*Time.fixedDeltaTime
		//futureColliderPos.x += futureMove

		RaycastHit2D ceilingCheck = Physics2D.Raycast(ceilingPosition, m_Rigidbody2D.velocity, m_Rigidbody2D.velocity.magnitude*Time.fixedDeltaTime, mask);
		//print("Collision normal: "+ ceilingCheck.normal);


		if(ceilingCheck.collider != null)
		{
			//print("Head collision.");
			//print("Original Velocity: "+ m_Rigidbody2D.velocity);
			//print("Original location: "+ ceilingPosition);
			//print("Predicted location: "+ ceilingCheck.point);
			//print("Struck object: " + ceilingCheck.collider.transform.gameObject);
			//ceilingCheck.collider.transform.gameObject.SetActive(false);

			Vector2 originalPos = this.transform.position;

			float hOffset = 0;
			float vOffset = -m_CeilingFootLength;

			if(m_Rigidbody2D.velocity.y != 0)
			{
				vOffset = vOffset - m_MinEmbed;
			}
			else
			{
				vOffset = vOffset - m_MinEmbed;
			}


			if(m_Rigidbody2D.velocity.x < 0)
			{
				//print("HOFFSET NEG VELO");
				hOffset = -m_MinEmbed;
			}
			else if(m_Rigidbody2D.velocity.x > 0)
			{
				//print("HOFFSET POS VELO");
				hOffset = m_MinEmbed;
			}

			if(ceilingCheck.normal.y == 0)
			{//If hitting a vertical wall.
				print("HITING WALL WITH HEAD INSTEAD OF SIDE!!");

				if(m_Rigidbody2D.velocity.x < 0)
				{
					m_LeftWalled = true;
					this.transform.position =  new Vector2(ceilingCheck.point.x+(m_LeftSideLength-m_MinEmbed), ceilingCheck.point.y-m_CeilingFootLength);
				}
				else if(m_Rigidbody2D.velocity.x > 0)
				{
					m_RightWalled = true;
					this.transform.position =  new Vector2(ceilingCheck.point.x-(m_RightSideLength-m_MinEmbed), ceilingCheck.point.y-m_CeilingFootLength);
				}
				else
				{
					print("Hit a wall without moving horizontally, somehow.");
				}
				m_Rigidbody2D.velocity = new Vector2(0f, m_Rigidbody2D.velocity.y);
			}
			else
			{//If hitting a downward facing surface
				this.transform.position =  new Vector2(ceilingCheck.point.x+hOffset, ceilingCheck.point.y+vOffset);//+vOffset);
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
					//DirectionCorrection();
				} 
				else 
				{
					m_Ceilinged = false;
					//print ("GroundCheck2=null!");
				}
				*/
			}

			//this.transform.position =  new Vector2(ceilingCheck.point.x+m_MinEmbed, ceilingCheck.point.y+0.2f);
			//m_Rigidbody2D.velocity = new Vector2(0f, m_Rigidbody2D.velocity.y);
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

		//print("Sent to Pos:" + setCharPos);
		//print("Sent to normal:" + groundCheck.normal);

		this.transform.position = setCharPos;
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
			//print("Velocity : " + m_Rigidbody2D.velocity);
			//print("Speed : " + m_Rigidbody2D.velocity.magnitude);
			//print(" ");
			//print(" ");	
			m_Grounded = false;
		}

		if(groundCheck.normal.y == 0f)
		{//If vertical surface
			//throw new Exception("Existence is suffering");
			//print("GtG VERTICAL :O");
		}
		//groundNormal = groundCheck2.normal;
		//roundNormal = groundCheck.normal;
		groundNormal = groundCheck2.normal;
		//print ("Final Position2:  " + this.transform.position);
	}

	private void AirToGround(RaycastHit2D groundCheck)
	{
		//print("AirToGround");
		//print ("Starting velocity:  " + m_Rigidbody2D.velocity);

		Vector2 setCharPos = groundCheck.point;
		setCharPos.y += m_FloorFootLength-m_MinEmbed;
		this.transform.position = setCharPos;
		//print ("Final Position:  " + this.transform.position);

		m_Grounded = true;

		//TEST CODE AFTER THIS POINT
		RaycastHit2D groundCheck2 = Physics2D.Raycast(this.transform.position, Vector2.down, m_FloorFootLength, mask);
		if (groundCheck2.collider != null) {
			if(antiTunneling){
				Vector2 surfacePosition = groundCheck2.point;
				surfacePosition.y += m_FloorFootLength-m_MinEmbed;
				this.transform.position = surfacePosition;
			}
		} 
		else 
		{
			m_Grounded = false;
			//print ("GroundCheck2=null!");
		}
		//AirToCeiling();
		//print ("A2G Vel:  " + m_Rigidbody2D.velocity);
		groundNormal = groundCheck.normal;
		//print ("Final Position2:  " + this.transform.position);
	}

	private void Traversal(Vector2 newNormal)
	{
		//print("traversal");
		Vector2 initialDirection = m_Rigidbody2D.velocity.normalized;


		float initialSpeed = m_Rigidbody2D.velocity.magnitude;
		//print("Speed before : " + m_Rigidbody2D.velocity.magnitude);
		//print("GroundTraversal");
		float testNumber = newNormal.y/newNormal.x;
		if(float.IsNaN(testNumber))
		{
			//print("IT'S NaN BRO LoLoLOL XD");
			throw new Exception("NaN value.");
			//print("X = "+ newNormal.x +", Y = " + newNormal.y);
		}



		Vector2 newPerp;
		Vector2 AdjustedVel;

		newPerp.x = newNormal.y;
		newPerp.y = -newNormal.x;


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


		//if (initialDirection.x < 0)
		//{
		//	impactAngle = 180f - impactAngle;
		//}
		//else if((initialDirection.x == 0) && (impactAngle > 90))
		//{
		//	impactAngle = 180f - impactAngle;
		//}

		if(impactAngle >= 180)
		{
			impactAngle = 180f - impactAngle;
		}

		if(impactAngle > 90)
		{
			impactAngle = 180f - impactAngle;
		}

		print("impactAngle: " +impactAngle);

		float projectionVal;
		if(newPerp.sqrMagnitude == 0)
		{
			projectionVal = 0;
		}
		else
		{
			projectionVal = Vector2.Dot(m_Rigidbody2D.velocity, newPerp)/newPerp.sqrMagnitude;
		}
		//print("P"+projectionVal);
		AdjustedVel = newPerp * projectionVal;
		//	print("A"+AdjustedVel);

		if(m_Rigidbody2D.velocity == Vector2.zero)
		{
			//m_Rigidbody2D.velocity = new Vector2(h, m_Rigidbody2D.velocity.y);
		}
		else
		{
			//m_Rigidbody2D.velocity = AdjustedVel + AdjustedVel.normalized*h;
			try
			{
				m_Rigidbody2D.velocity = AdjustedVel;
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

		print("SPLMLT " + speedLossMult);
		m_Rigidbody2D.velocity = SetSpeed(m_Rigidbody2D.velocity , initialSpeed*speedLossMult);

		//print ("GT Vel:  " + m_Rigidbody2D.velocity);
	}

	/*
	private void CeilingTraversal()
	{
		print("CeilingTraversal");
		float testNumber = ceilingNormal.y/ceilingNormal.x;
		if(float.IsNaN(testNumber))
		{
			//print("IT'S NaN BRO LoLoLOL XD");
		}

		Vector2 ceilingPerp;
		Vector2 adjustedVel;

		ceilingPerp.x = ceilingNormal.y;
		ceilingPerp.y = -ceilingNormal.x;

		float projectionVal;
		if(ceilingPerp.sqrMagnitude == 0)
		{
			projectionVal = 0;
		}
		else
		{
			projectionVal = Vector2.Dot(m_Rigidbody2D.velocity, ceilingPerp)/ceilingPerp.sqrMagnitude;
		}
		//print("P"+projectionVal);
		adjustedVel = ceilingPerp * projectionVal;
		//	print("A"+AdjustedVel);

		try
		{
			m_Rigidbody2D.velocity = adjustedVel;
			//print("Velocity after ceiling traversal: "+ m_Rigidbody2D.velocity);
		}
		catch(Exception e)
		{
			print(e);
			//print("CeilingPerp="+ceilingPerp);
			//print("projectionVal"+projectionVal);
			//print("adjustedVel"+adjustedVel);
		}
	}
	*/

	private void CameraControl()
	{
		cameraZoom = Mathf.Lerp(cameraZoom, m_Rigidbody2D.velocity.magnitude, 0.1f);
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
			//print("Top not wedged!");
			return;
			cPerp.x = ceilingNormal.x;
			cPerp.y = ceilingNormal.y;
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
			if(m_Rigidbody2D.velocity.x > 0)
			{
				moveAmount = SuperUnwedger(cPerp, gPerp, false, embedDepth);
				m_Rigidbody2D.velocity = new Vector2(0f, 0f);
				m_RightWalled = true;
				//print("Right wedge!");
				//print("cPerp: "+cPerp);
				//print("gPerp: "+gPerp);
			}
		}
		else if(cPerp.y < 0)
		{
			if(m_Rigidbody2D.velocity.x < 0)
			{
				moveAmount = SuperUnwedger(cPerp, gPerp, true, embedDepth);
				//print("Left wedge!");
				//print("cPerp: "+cPerp);
				//print("gPerp: "+gPerp);
				m_LeftWalled = true;
				m_Rigidbody2D.velocity = new Vector2(0f, 0f);
			}
		}
		else
		{
			//throw new Exception("Ceiling is vertical! CEILINGS CAN'T BE VERTICAL!");
		}

		this.transform.position = new Vector2((this.transform.position.x + moveAmount.x), (this.transform.position.y + moveAmount.y));
		//m_Ceilinged = false;

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
			if(m_Rigidbody2D.velocity.y >= 0)
			{
				m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x+(m_HJumpForce*horizontalInput), m_Rigidbody2D.velocity.y+m_VJumpForce);
			}
			else
			{
				m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x+(m_HJumpForce*horizontalInput), m_VJumpForce);
			}
			m_Jump = false;
			m_Grounded = false;
		}
		else if(m_Ceilinged)
		{
			if(m_Rigidbody2D.velocity.y <= 0)
			{
				m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x+(m_HJumpForce*horizontalInput), m_Rigidbody2D.velocity.y -m_VJumpForce);
			}
			else
			{
				m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x+(m_HJumpForce*horizontalInput), -m_VJumpForce);
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