// WELCOME! When you left off you were working on line 400 on directioncorrection specific collision code, and were contemplating killing yourself. Have fun!


using UnityEngine.UI;
using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
namespace UnityStandardAssets._2D
{
    public class PlatformerCharacter2D : MonoBehaviour
    {                
		[SerializeField] private float m_MinSpeed = 10f; 					// The instant starting speed while moving
		[SerializeField] private float maxRunSpeed;							// The fastest the player can travel in the x axis.
		[Range(0,2)][SerializeField] private float m_Acceleration = 1f;    	// Speed the player accelerates at
		[SerializeField] private float m_JumpForce = 20f;                  	// Amount of force added when the player jumps.
		[SerializeField] private float tractionChangeSpeed = 100f;			// Where movement changes from exponential to linear acceleration.
		[SerializeField] private bool m_AirControl = false;                 // Whether or not a player can steer while jumping;
        [SerializeField] private LayerMask m_WhatIsGround;                  // A mask determining what is ground to the character
		[SerializeField] private LayerMask mask;
	
	
		[SerializeField] private bool showVelocityIndicator;


		//############################################################################################################################################################################################################
		// PLAYER COMPONENTS
		//###########################################################################################################################################################################
		#region PLAYERCOMPONENTS
		[SerializeField] private Text m_Speedometer;      
		[SerializeField] private Camera m_MainCamera;
		private float cameraZoom;
        private Animator m_Anim;            // Reference to the player's animator component.
        private Rigidbody2D m_Rigidbody2D;
		private SpriteRenderer m_SpriteRenderer;
		#endregion
		//############################################################################################################################################################################################################
		// PHYSICS&RAYCASTING
		//###########################################################################################################################################################################
		#region PHYSICS&RAYCASTING
		private Transform m_LeftFoot; // Floor collider, left.
		private Vector2 m_LeftFootOffset;
		private float m_LeftFootLength;

		private Transform m_RightFoot; // Floor collider, right.
		private Vector2 m_RightFootOffset;
		private float m_RightFootLength;

		private Transform m_MidFoot; // Floor collider, middle.
		private Vector2 m_MidFootOffset; 
		private float m_MidFootLength;

		private Transform m_CeilingFoot; // Ceiling collider, middle.
		private Vector2 m_CeilingFootOffset;
		private float m_CeilingFootLength;

		private Transform m_LeftSide; // Unused for now.
		private Transform m_RightSide; // Unused for now.

		private Vector2 GroundNormal;   
		private Vector2 CeilingNormal;
		[SerializeField] private Transform[] m_Colliders;


		private bool leftContact;
		private bool midContact;
		private bool rightContact;
		private bool ceilingContact;

		[SerializeField] private bool m_Grounded; 
		[SerializeField] private bool m_Ceilinged; 
		[SerializeField] private bool m_LeftWalled; 
		[SerializeField] private bool m_RightWalled; 
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
		[SerializeField] private bool autoRun; // When set to true, the player will run even after the key is released.
		[SerializeField] private bool autoJump;
		[SerializeField] private bool antiTunneling; // When set to true, the player will be pushed up out of objects they are stuck in.
		[SerializeField] private bool noGravity; 
		private LineRenderer m_DebugLine; // Shows Velocity.
		private LineRenderer m_LeftLine; 
		private LineRenderer m_MidLine;
		private LineRenderer m_RightLine;
		private LineRenderer m_CeilingLine;
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
			m_LeftFootLength = m_LeftFootOffset.magnitude;

			m_RightFoot = transform.Find("RightFoot");
			m_RightLine = m_RightFoot.GetComponent<LineRenderer>();
			m_RightFootOffset.x = m_RightFoot.position.x-playerOrigin.x;
			m_RightFootOffset.y = m_RightFoot.position.y-playerOrigin.y;
			m_RightFootLength = m_RightFootOffset.magnitude;

			m_MidFoot = transform.Find("MidFoot");
			m_MidLine = m_MidFoot.GetComponent<LineRenderer>();
			m_MidFootOffset.x = m_MidFoot.position.x-playerOrigin.x;
			m_MidFootOffset.y = m_MidFoot.position.y-playerOrigin.y;
			m_MidFootLength = m_MidFootOffset.magnitude;

			m_CeilingFoot = transform.Find("CeilingFoot");
			m_CeilingLine = m_CeilingFoot.GetComponent<LineRenderer>();
			m_CeilingFootOffset.x = m_CeilingFoot.position.x-playerOrigin.x;
			m_CeilingFootOffset.y = m_CeilingFoot.position.y-playerOrigin.y;
			m_CeilingFootLength = m_CeilingFootOffset.magnitude;

            m_Anim = GetComponent<Animator>();
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
			m_SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void FixedUpdate()
		{
			//print("Initial Pos: " + this.transform.position);

			m_KeyLeft = CrossPlatformInputManager.GetButton("Left");
			//print("LEFT="+m_KeyLeft);
			m_KeyRight = CrossPlatformInputManager.GetButton("Right");
			//print("RIGHT="+m_KeyRight);
			if((!m_KeyLeft && !m_KeyRight) || (m_KeyLeft && m_KeyRight))
			{
				//print("BOTH/NEITHER");
				if(!autoRun)
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

			m_SpriteRenderer.color = Color.white;
			m_LeftLine.endColor = Color.red;
			m_LeftLine.startColor = Color.red;
			m_RightLine.endColor = Color.red;
			m_RightLine.startColor = Color.red;
			m_MidLine.endColor = Color.red;
			m_MidLine.startColor = Color.red;
			m_CeilingLine.endColor = Color.red;
			m_CeilingLine.startColor = Color.red;

			if (CtrlH < 0) 
			{
				facingDirection = false; //true means right (the direction), false means left.
			} 
			else if (CtrlH > 0)
			{
				facingDirection = true; //true means right (the direction), false means left.
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

			RaycastHit2D MidHit = Physics2D.Raycast(this.transform.position, Vector2.down, m_MidFootLength, mask);
			if (MidHit.collider != null) 
			{
				//m_Grounded = true;
				midContact = true;
				m_MidLine.endColor = Color.green;
				m_MidLine.startColor = Color.green;
				GroundNormal = MidHit.normal;
			} 
			else 
			{
				//m_Grounded = false;
				midContact = false;
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

			RaycastHit2D CeilingHit = Physics2D.Raycast (this.transform.position, Vector2.up,  m_CeilingFootLength, mask);
			if (CeilingHit.collider != null) 
			{
				//m_Ceilinged = true;
				ceilingContact = true;
				m_CeilingLine.endColor = Color.green;
				m_CeilingLine.startColor = Color.green;
				CeilingNormal = CeilingHit.normal;
			} 
			else 
			{
				//m_Ceilinged = false;
				ceilingContact = false;
			}



			#endregion


			if (ceilingContact) 
			{
				if(!midContact && antiTunneling)
				{
					Vector2 surfacePosition = CeilingHit.point;
					surfacePosition.y -= (m_CeilingFootLength+0.01f);
					this.transform.position = surfacePosition;
					//print ("HEAD IMPACTED IN SURFACE " + CeilingHit.normal);
				}
				m_Ceilinged = true;
			}
			else 
			{
				m_Ceilinged = false;
			}

			if (midContact) 
			{
				if(!ceilingContact && antiTunneling)
				{
					Vector2 surfacePosition = MidHit.point;
					surfacePosition.y += m_MidFootLength-0.01f;
					this.transform.position = surfacePosition;
					//print ("MIDHIT NORMAL INITIAL:    " + MidHit.normal);
				}
				m_Grounded = true;
			}
			else 
			{
				m_Grounded = false;
			}

			if(m_Ceilinged&&m_Grounded)
			{
				Wedged();
				m_Ceilinged = false;
			}
				
			if(m_Jump)
			{
				if(m_Grounded){
					if(m_Rigidbody2D.velocity.y >= 0)
					{
						m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_Rigidbody2D.velocity.y+m_JumpForce);
					}
					else
					{
						m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_JumpForce);
					}
					m_Jump = false;
					m_Grounded = false;
				}
				else if(m_Ceilinged)
				{
					if(m_Rigidbody2D.velocity.y <= 0)
					{
						m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_Rigidbody2D.velocity.y -m_JumpForce);
					}
					else
					{
						m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, -m_JumpForce);
					}

					m_Jump = false;
					m_Ceilinged = false;
				}
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

			if(m_Ceilinged)
			{
				CeilingTraversal();
			}

			if (m_Grounded) //Handles velocity along ground surface.
			{
				GroundTraversal();
			}

			//print("Speed this frame: "+m_Rigidbody2D.velocity.magnitude);
			//print("Per frame velocity at end of physics frame: "+m_Rigidbody2D.velocity*Time.deltaTime);
			//print("Pos at end of physics frame: "+this.transform.position);
			//print("##############################################################################################");

			//print(GroundNormal);

			#region Animator Controls

			//
			//Animator Controls
			//

			if (!facingDirection) //If facing left
			{
				//print("FACING LEFT!   "+h);
				Vector2 debugLineInverted = ((m_Rigidbody2D.velocity*Time.deltaTime));
				debugLineInverted = new Vector2(-debugLineInverted.x, debugLineInverted.y);
				m_DebugLine.SetPosition (1, debugLineInverted);
				this.transform.localScale = new Vector3 (-1f, 1f, 1f);
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
				m_DebugLine.SetPosition(1, (m_Rigidbody2D.velocity*Time.deltaTime));
				this.transform.localScale = new Vector3 (1f, 1f, 1f);
				if(m_Rigidbody2D.velocity.x < 0)
				{
					m_Anim.SetBool("Crouch", true);
				}
				else
				{
					m_Anim.SetBool("Crouch", false);
				}
			}
				
			m_Anim.SetFloat("Speed", m_Rigidbody2D.velocity.magnitude);

			if(m_Rigidbody2D.velocity.magnitude >= tractionChangeSpeed)
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

			if (!m_Grounded) 
			{
				//print ("Flying");
			}
			m_Anim.SetBool("Ground", m_Grounded);

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
			//print("DC Executing");
			errorDetectingRecursionCount++;

			if(errorDetectingRecursionCount >= 5)
			{
				throw new Exception("Your recursion code is fucked!");
				return;
			}

			if(m_Rigidbody2D.velocity.magnitude != 0)
			{
				m_LeftWalled = false;
				m_RightWalled = false;
			}

			//print ("Player Pos:  " + this.transform.position);
			//print ("velocity:  " + m_Rigidbody2D.velocity);

			Vector2 adjustedBot = m_MidFoot.position; // AdjustedBot marks the end of the middle floor raycast, but 0.02 higher.
			adjustedBot.y += 0.02f;

			Vector2 adjustedTop = m_CeilingFoot.position; // AdjustedTop marks the end of the middle ceiling raycast, but 0.02 lower.
			adjustedTop.y -= 0.02f;

			RaycastHit2D groundCheck = Physics2D.Raycast(this.transform.position, Vector2.down, m_MidFootLength, mask);
			RaycastHit2D predictedLoc = Physics2D.Raycast(adjustedBot, m_Rigidbody2D.velocity, m_Rigidbody2D.velocity.magnitude*Time.deltaTime, mask);

			RaycastHit2D collider1Check = Physics2D.Raycast(adjustedTop, m_Rigidbody2D.velocity, m_Rigidbody2D.velocity.magnitude*Time.deltaTime, mask);

			if (predictedLoc.collider != null) 
			{//If you're going to hit something.
				print("Impact!");
				collider1Check = Physics2D.Raycast(adjustedTop, m_Rigidbody2D.velocity, predictedLoc.distance, mask); //Collider checks go only as far as the first angle correction, so they don't mistake the slope of the floor for an obstacle.

				//print("Velocity before impact: "+m_Rigidbody2D.velocity);

				Vector2 invertedImpactNormal;//This is done in case one of the raycasts is inside the collider, which would cause it to return an inverted normal value.
				invertedImpactNormal.x = -predictedLoc.normal.y; //Inverting the normal.
				invertedImpactNormal.y = -predictedLoc.normal.x; //Inverting the normal.

				if(collider1Check.collider != null) //if((collider1Check.collider != null) && (collider1Check.normal != predictedLoc.normal) && (collider1Check.normal != invertedImpactNormal))
				{// If player hits an obstacle that is too high for its feet to take care of.
					print("Obstacle");
					Collision();
					//print("Velocity after impact: "+m_Rigidbody2D.velocity);
					//DirectionCorrection();
				}
				else if (groundCheck.collider == null) 
				{ // If player is airborne beforehand.
					AirToGround(predictedLoc);
					return;
				} 
				else if (groundCheck.normal != predictedLoc.normal && groundCheck.normal != invertedImpactNormal) 
				{ // If the slope you're hitting is different than your current slope.
					ChangeSlope(predictedLoc);
					return;
				}
				else 
				{
					//print ("Same surface, no trajectory change needed");
					return;
				}
			} 
			else if(collider1Check.collider != null)
			{
				//print("Free move into obstacle.");
				Collision();
				//print("Velocity after impact: "+m_Rigidbody2D.velocity);
			}
			else
			{
				//print("No collisions detected");
			}
		}
			
		private void Traction(float horizontalInput)
		{
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
					m_Rigidbody2D.velocity = ChangeSpeedLinear (m_Rigidbody2D.velocity, -0.25f);
				}
			}
			else if((horizontalInput > 0 && m_Rigidbody2D.velocity.x >= 0) || (horizontalInput < 0 && m_Rigidbody2D.velocity.x <= 0))
			{//if pressing same button as move direction, move to MAXSPEED.
				//print("Running with momentum.");
				//print("Moving with keypress");
				if(rawSpeed <= maxRunSpeed)
				{
					//print("Rawspeed("+rawSpeed+") less than max");
					if(rawSpeed > tractionChangeSpeed)
					{
						//print("LinAccel-> " + rawSpeed);
						m_Rigidbody2D.velocity = ChangeSpeedLinear (m_Rigidbody2D.velocity, 0.25f);
					}
					else if(rawSpeed == 0)
					{
						m_Rigidbody2D.velocity = new Vector2(m_Acceleration*horizontalInput, 0);
						//print("Starting motion. Adding " + m_Acceleration);
					}
					else
					{
						//print("Accelerating");
						float eqnX = (1+Mathf.Abs((1/tractionChangeSpeed)*rawSpeed));
						float curveMultiplier = 1+(1/(eqnX*eqnX)); // Goes from 1/4 to 1, increasing as speed approaches 0.
						float addedSpeed = curveMultiplier*m_Acceleration;
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
				

				if(rawSpeed > tractionChangeSpeed)
				{
					//print("LinDecel");
					m_Rigidbody2D.velocity = ChangeSpeedLinear (m_Rigidbody2D.velocity, -0.5f);
				}
				else
				{
					//print("Decelerating");
					float eqnX = (1+Mathf.Abs((1/tractionChangeSpeed)*rawSpeed));
					float curveMultiplier = 1+(1/(eqnX*eqnX)); // Goes from 1/4 to 1, increasing as speed approaches 0.
					float addedSpeed = curveMultiplier*m_Acceleration;
					m_Rigidbody2D.velocity = (m_Rigidbody2D.velocity.normalized)*(rawSpeed-2*addedSpeed);
				}

				//float modifier = Mathf.Abs(m_Rigidbody2D.velocity.x/m_Rigidbody2D.velocity.y);
				//print("SLOPE MODIFIER: " + modifier);
				//m_Rigidbody2D.velocity = m_Rigidbody2D.velocity/(1.25f);
			}
			//print("PostTraction velocity: "+m_Rigidbody2D.velocity);
		}

		private bool Collision()
		{
			Vector2 collider1Position;

			if(m_Rigidbody2D.velocity.x < 0)
			{
				//print("              MOVING LEFT!");
				collider1Position.x = m_CeilingFoot.position.x;//-0.01f;
			}
			else if(m_Rigidbody2D.velocity.x > 0)
			{
				//print("              MOVING RIGHT!");
				collider1Position.x = m_CeilingFoot.position.x;//+0.01f;
			}
			else
			{
				//print("alternate");
				collider1Position.x = m_CeilingFoot.position.x;
			}

			//collider1Position.x = m_CeilingFoot.position.x;
			collider1Position.y = m_CeilingFoot.position.y;
			//Vector2 futureMove = m_Rigidbody2D.velocity*Time.deltaTime
			//futureColliderPos.x += futureMove

			RaycastHit2D collider1Check = Physics2D.Raycast(collider1Position, m_Rigidbody2D.velocity, m_Rigidbody2D.velocity.magnitude*Time.deltaTime, mask);
			//print("Collision normal: "+ collider1Check.normal);


			if(collider1Check.collider != null)
			{
				print("Head collision.");
				//print("Original Velocity: "+ m_Rigidbody2D.velocity);
				//print("Original location: "+ collider1Position);
				//print("Predicted location: "+ collider1Check.point);
				//print("Struck object: " + collider1Check.collider.transform.gameObject);
				//collider1Check.collider.transform.gameObject.SetActive(false);

				Vector2 originalPos = this.transform.position;

				float hOffset = 0;
				float vOffset = -m_CeilingFootLength;

				if(m_Rigidbody2D.velocity.y != 0)
				{
					vOffset = vOffset - 0.01f;
				}
				else
				{
					vOffset = vOffset - 0.01f;
				}


				if(m_Rigidbody2D.velocity.x < 0)
				{
					//print("HOFFSET NEG VELO");
					hOffset = -0.01f;
				}
				else if(m_Rigidbody2D.velocity.x > 0)
				{
					//print("HOFFSET POS VELO");
					hOffset = 0.01f;
				}
				else
				{
					//print("HOFFSET 0");
					hOffset = 0;
				}

				if(collider1Check.normal.y == 0)
				{//If hitting a vertical wall.
					//print("normal.y == 0");

					if(m_Rigidbody2D.velocity.x < 0)
					{
						m_LeftWalled = true;
						this.transform.position =  new Vector2(collider1Check.point.x+0.02f, collider1Check.point.y-m_CeilingFootLength);
					}
					else if(m_Rigidbody2D.velocity.x > 0)
					{
						m_RightWalled = true;
						this.transform.position =  new Vector2(collider1Check.point.x-0.02f, collider1Check.point.y-m_CeilingFootLength);
					}
					else
					{
						print("Hit a wall without moving horizontally, somehow.");
					}
					float expendedYVelocity = m_Rigidbody2D.velocity.y - (this.transform.position.y - originalPos.y);
					m_Rigidbody2D.velocity = new Vector2(0.0f, expendedYVelocity);
				}
				else
				{//If hitting a downward facing surface
					this.transform.position =  new Vector2(collider1Check.point.x+hOffset, collider1Check.point.y+vOffset);//+vOffset);
					//print("normal.y != 0");

					//Vector2 adjustedTop = m_CeilingFoot.position; // AdjustedBot marks the top of the middle ceiling raycast.
					//adjustedTop.y -= 0.02f;

					m_Ceilinged = true;
					CeilingNormal = collider1Check.normal;
					if(m_Grounded)
					{
						Wedged();
					}

					print("After collision, Ceilinged = true, Grounded = " + m_Grounded);

					/*
					Vector2 expendedVel = collider1Check.point;
					expendedVel.x -= collider1Position.x;
					expendedVel.y -= collider1Position.y;
					Vector2 remainingVel = m_Rigidbody2D.velocity-expendedVel;
					//SEARCHMARKER: lotus

					//print("Expended Velocity: "+ expendedVel);
					//print("Remaining Velocity: "+ remainingVel);
					m_Rigidbody2D.velocity = remainingVel;
					*/

					/*
					RaycastHit2D ceilingCheck2 = Physics2D.Raycast(adjustedBot, Vector2.up, 0.42f, mask);
					if (ceilingCheck2.collider != null) {
						print("THIS CODE RUNS I GUESS HEH");
						m_Ceilinged = true;
						//print ("ceilingCheck2.normal=" + ceilingCheck2.normal);
						//print ("ceilingCheck2.collider=" + ceilingCheck2.transform.gameObject);
						CeilingNormal  = ceilingCheck2.normal;
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

				//this.transform.position =  new Vector2(collider1Check.point.x+0.01f, collider1Check.point.y+0.2f);
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

		private void ChangeSlope(RaycastHit2D predictedLoc)
		{
			float testNumber = predictedLoc.normal.y/predictedLoc.normal.x;
			//print(testNumber);
			print ("We've hit slope, sir!!");
			//print ("predictedLoc.normal=" + predictedLoc.normal);

			/*
			//SEARCHMARKER: Lotus
			//print ("Starting velocity:  " + m_Rigidbody2D.velocity);
			Vector2 expendedVel = predictedLoc.point;
			expendedVel.x -= this.transform.position.x;
			expendedVel.y -= this.transform.position.y;
			//print ("Player Pos:  " + this.transform.position);
			//print ("Impact Pos:  " + predictedLoc.point);
			//print ("Expended velocity:  " + expendedVel);
			Vector2 remainingVel = m_Rigidbody2D.velocity-expendedVel;
			//print ("Remaining velocity:  " + remainingVel);
			m_Rigidbody2D.velocity = remainingVel;

			*/

			m_Grounded = true;
			Vector2 setCharPos = predictedLoc.point;
			setCharPos.y = setCharPos.y+m_MidFootLength-0.01f; //Embed slightly in floor to ensure raycasts still hit floor.
			this.transform.position = setCharPos;
			//print ("Final Position:  " + this.transform.position);

			RaycastHit2D groundCheck2 = Physics2D.Raycast(this.transform.position, Vector2.down, m_MidFootLength, mask);
			if (groundCheck2.collider != null) 
			{
				//print(
				//Vector2 surfacePosition = groundCheck2.point;
				//print(groundCheck2.point);
				//surfacePosition.y += m_MidFootLength;
				//this.transform.position = surfacePosition;
				//print ("ASCENDING!");
			}
			else
			{
				//print ("Impact Pos:  " + predictedLoc.point);
				//print ("Expended velocity:  " + expendedVel);
				//print("Reflected back into the air!");
				//print("Transform position: " + this.transform.position);
				//print("RB2D position: " + m_Rigidbody2D.position);
				//print("Velocity : " + m_Rigidbody2D.velocity);
				//print("Speed : " + m_Rigidbody2D.velocity.magnitude);
				//print(" ");
				//print(" ");	
				m_Grounded = false;
			}
			GroundNormal = groundCheck2.normal;
			//print ("Final Position2:  " + this.transform.position);
		}

		private void AirToGround(RaycastHit2D predictedLoc)
		{
			print("AirToGround");
			//print ("Starting velocity:  " + m_Rigidbody2D.velocity);

			/*
			//SEARCHMARKER: Lotus
			Vector2 expendedVel = predictedLoc.point;
			expendedVel.x -= this.transform.position.x;
			expendedVel.y -= this.transform.position.y;
			//print ("Player Pos:  " + this.transform.position);
			//print ("Impact Pos:  " + predictedLoc.point);
			//print ("Expended velocity:  " + expendedVel);
			Vector2 remainingVel = m_Rigidbody2D.velocity-expendedVel;
			//print ("Remaining velocity:  " + remainingVel);
			m_Rigidbody2D.velocity = remainingVel;
			*/

			Vector2 setCharPos = predictedLoc.point;
			setCharPos.y += m_MidFootLength;
			this.transform.position = setCharPos;
			//print ("Final Position:  " + this.transform.position);

			//TEST CODE AFTER THIS POINT
			RaycastHit2D groundCheck2 = Physics2D.Raycast(this.transform.position, Vector2.down, m_MidFootLength, mask);
			if (groundCheck2.collider != null) {

				m_Grounded = true;
				//print ("groundCheck2.normal=" + groundCheck2.normal);
				//print ("groundCheck2.collider=" + groundCheck2.transform.gameObject);
				GroundNormal = groundCheck2.normal;
			} 
			else 
			{
				m_Grounded = false;
				//print ("GroundCheck2=null!");
			}
			//Collision();


			//print ("Final Position2:  " + this.transform.position);
		}

		private void GroundTraversal()
		{
			//print("GroundTraversal");
			float testNumber = GroundNormal.y/GroundNormal.x;
			if(float.IsNaN(testNumber))
			{
				print("IT'S NaN BRO LoLoLOL XD");
				//print("X = "+ GroundNormal.x +", Y = " + GroundNormal.y);
			}

			Vector2 groundperp;
			Vector2 AdjustedVel;

			groundperp.x = GroundNormal.y;
			groundperp.y = -GroundNormal.x;

			float projectionVal;
			if(groundperp.sqrMagnitude == 0)
			{
				projectionVal = 0;
			}
			else
			{
				projectionVal = Vector2.Dot(m_Rigidbody2D.velocity, groundperp)/groundperp.sqrMagnitude;
			}
			//print("P"+projectionVal);
			AdjustedVel = groundperp * projectionVal;
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
					print("Groundperp="+groundperp);
					print("projectionVal"+projectionVal);
					print("adjustedVel"+AdjustedVel);
				}
			}
		}
	
		private void CeilingTraversal()
		{
			print("CeilingTraversal");
			float testNumber = CeilingNormal.y/CeilingNormal.x;
			if(float.IsNaN(testNumber))
			{
				//print("IT'S NaN BRO LoLoLOL XD");
			}

			Vector2 ceilingPerp;
			Vector2 adjustedVel;

			ceilingPerp.x = CeilingNormal.y;
			ceilingPerp.y = -CeilingNormal.x;

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

		private void CameraControl()
		{
			cameraZoom = Mathf.Lerp(cameraZoom, m_Rigidbody2D.velocity.magnitude, 0.1f);
			m_MainCamera.orthographicSize = 5f+(0.15f*cameraZoom);
		}

		private void Wedged()
		{//Executes when the player is moving into a corner and there isn't enough headroom to go any further. It halts the player's momentum and sets off a wall-touching flag.

			print("Wedged!");
			RaycastHit2D ceilingHit;
			RaycastHit2D floorHit = Physics2D.Raycast(this.transform.position, Vector2.down, m_MidFootLength, mask);
			float embedDepth;
			Vector2 gPerp;
			Vector2 cPerp;
			Vector2 moveAmount = new Vector2(0,0);

			if(floorHit.collider == null)
			{
				print("Bottom not wedged, using GroundNormal!");
				gPerp.x = GroundNormal.x;
				gPerp.y = GroundNormal.y;
			}
			else
			{
				gPerp.x = floorHit.normal.y;
				gPerp.y = -floorHit.normal.x;
				Vector2 floorPosition = floorHit.point;
				floorPosition.y += m_MidFootLength-0.01f;
				this.transform.position = floorPosition;
				print("Hitting bottom, shifting up!");
			}

			ceilingHit = Physics2D.Raycast(this.transform.position, Vector2.up, m_CeilingFootLength, mask);

			if(ceilingHit.collider == null)
			{
				print("Top not wedged!");
				return;
				cPerp.x = CeilingNormal.x;
				cPerp.y = CeilingNormal.y;
			}
			else
			{
				print("Hitting top, superunwedging...");
				cPerp.x = ceilingHit.normal.y;
				cPerp.y = -ceilingHit.normal.x;
			}
				
			embedDepth = m_CeilingFootLength-ceilingHit.distance;
			print("Embedded ("+embedDepth+") units into the ceiling");

			print("Ground Perp = " + gPerp);
			print("Ceiling Perp = " + cPerp);

			if(cPerp.y > 0)
			{
				if(m_Rigidbody2D.velocity.x > 0)
				{
					moveAmount = SuperUnwedger(cPerp, gPerp, false, embedDepth);
					m_Rigidbody2D.velocity = new Vector2(0f, 0f);
					m_RightWalled = true;
					print("Right wedge!");
					print("cPerp: "+cPerp);
					print("gPerp: "+gPerp);
				}
			}
			else if(cPerp.y < 0)
			{
				if(m_Rigidbody2D.velocity.x < 0)
				{
					moveAmount = SuperUnwedger(cPerp, gPerp, true, embedDepth);
					print("Left wedge!");
					print("cPerp: "+cPerp);
					print("gPerp: "+gPerp);
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
				print("Resolving right wedge.");
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
				print("Resolving left wedge.");
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
				
			print("Adapted Ground Perp = " + gPerp);
			print("Adapted Ceiling Perp = " + cPerp);

			//
			// Now, the equation for repositioning a vertical line that is embedded in a corner:
			//

			float B = gPerp.y;
			float A = cPerp.y;
			float H = embedDistance-0.01f; //Reduced so the player stays just embedded enough for the raycast to detect next frame.
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

			print("Adding the movement: ("+ X +", "+Y+").");
			return new Vector2(X,Y); // Returns the distance the object must move to resolve wedging.
		}
	}
}