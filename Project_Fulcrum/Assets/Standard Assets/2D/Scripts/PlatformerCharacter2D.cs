// WELCOME! When you left off you were working on line 400 on directioncorrection specific collision code, and were contemplating killing yourself. Have fun!



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
		[SerializeField] private float m_JumpForce = 10f;                  	// Amount of force added when the player jumps.
		[SerializeField] private float tractionChangeSpeed = 100f;			// Where movement changes from exponential to linear acceleration.
		[SerializeField] private bool m_AirControl = false;                 // Whether or not a player can steer while jumping;
        [SerializeField] private LayerMask m_WhatIsGround;                  // A mask determining what is ground to the character
		[SerializeField] private Vector2 GroundNormal;
		[SerializeField] private bool m_Grounded;    
		[SerializeField] private LayerMask mask;
	
	
		[SerializeField] private bool showVelocityIndicator;


		//##################################
		// PLAYER COMPONENTS
		//###################################

        private Transform m_GroundCheck;    // A position marking where to check if the player is grounded.
        const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
      	private Transform m_CeilingCheck;   // A position marking where to check for ceilings
        const float k_CeilingRadius = .01f; // Radius of the overlap circle to determine if the player can stand up
        private Animator m_Anim;            // Reference to the player's animator component.
        private Rigidbody2D m_Rigidbody2D;
		private SpriteRenderer m_SpriteRenderer;

		//##################################
		// TRANSFORM STUFF
		//###################################
		private Transform m_LeftFoot;
		private Transform m_RightFoot;
		private Transform m_MidFoot;
		private Transform m_LeftSide;
		private Transform m_RightSide;

		private Transform m_Collider1;
		private Transform m_Collider2;
		private Transform m_Collider3;
		private Transform m_Collider4;

		private bool leftcontact;
		private bool midcontact;
		private bool rightcontact;

		//##################################
		// PLAYER INPUT VARIABLES
		//###################################
		private bool m_Jump;
		private bool m_KeyLeft;
		private bool m_KeyRight;
		private int CtrlH; 					// Tracks horizontal keys pressed. Values are -1 (left), 0 (none), or 1 (right). 
		private bool facingDirection; 		// true means right (the direction), false means left.

		//##################################
		// DEBUGGING VARIABLES
		//##################################
		private int errorDetectingRecursionCount; //Iterates each time recursive trajectory correction executes on the current frame.
		[SerializeField] private bool autorun; // When set to true, the player will run even after the key is released.
		private LineRenderer m_DebugLine; // Shows Velocity.
		private LineRenderer m_LeftLine; 
		private LineRenderer m_MidLine;
		private LineRenderer m_RightLine;


        private void Awake()
        {
			m_DebugLine = GetComponent<LineRenderer>();
			if(!showVelocityIndicator){
				m_DebugLine.enabled = false;
			}

            m_GroundCheck = transform.Find("GroundCheck");

			m_LeftFoot = transform.Find("LeftFoot");
			m_LeftLine = m_LeftFoot.GetComponent<LineRenderer>();

			m_MidFoot = transform.Find("MidFoot");
			m_MidLine = m_MidFoot.GetComponent<LineRenderer>();

			m_RightFoot = transform.Find("RightFoot");
			m_RightLine = m_RightFoot.GetComponent<LineRenderer>();

			m_Collider1 = transform.Find("Collider1");
			m_Collider2 = transform.Find("Collider2");
			m_Collider3 = transform.Find("Collider3");
			m_Collider4 = transform.Find("Collider4");


            m_CeilingCheck = transform.Find("CeilingCheck");
            m_Anim = GetComponent<Animator>();
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
			m_SpriteRenderer = GetComponent<SpriteRenderer>();
        }


        private void FixedUpdate()
		{
			m_KeyLeft = CrossPlatformInputManager.GetButton("Left");
			//print("LEFT="+m_KeyLeft);
			m_KeyRight = CrossPlatformInputManager.GetButton("Right");
			//print("RIGHT="+m_KeyRight);
			if((!m_KeyLeft && !m_KeyRight) || (m_KeyLeft && m_KeyRight))
			{
				//print("BOTH/NEITHER");
				if(!autorun)
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
				
			print("CTRLH=" + CtrlH);

			#region raytesting

			m_Grounded = false;
			m_SpriteRenderer.color = Color.white;
			m_LeftLine.endColor = Color.red;
			m_LeftLine.startColor = Color.red;
			m_RightLine.endColor = Color.red;
			m_RightLine.startColor = Color.red;
			m_MidLine.endColor = Color.red;
			m_MidLine.startColor = Color.red;

			if (CtrlH < 0) 
			{
				facingDirection = false; //true means right (the direction), false means left.
			} 
			else if (CtrlH > 0)
			{
				facingDirection = true; //true means right (the direction), false means left.
			}

			RaycastHit2D LeftHit = Physics2D.Raycast (m_LeftFoot.position, Vector2.down, 0.42f, mask);
			if (LeftHit.collider != null)
			{
				leftcontact = true;
				m_LeftLine.endColor = Color.green;
				m_LeftLine.startColor = Color.green;
			} 
			else
			{
				leftcontact = false;
			}

			RaycastHit2D MidHit = Physics2D.Raycast (m_MidFoot.position, Vector2.down, 0.42f, mask);
			if (MidHit.collider != null) 
			{
				midcontact = true;
				m_MidLine.endColor = Color.green;
				m_MidLine.startColor = Color.green;
				GroundNormal = MidHit.normal;
			} 
			else 
			{
				midcontact = false;
			}
				
			RaycastHit2D RightHit = Physics2D.Raycast (m_RightFoot.position, Vector2.down, 0.42f, mask);
			if (RightHit.collider != null) 
			{
				rightcontact = true;
				m_RightLine.endColor = Color.green;
				m_RightLine.startColor = Color.green;
			} 
			else 
			{
				rightcontact = false;
			}

			#endregion

			if (midcontact) 
			{
				//print ("Midcontact");
				m_Grounded = true;
			}
			else 
			{
				m_Grounded = false;
			}

			print("Starting velocity: " + m_Rigidbody2D.velocity);

			//Midair Physics!
			if (!m_Grounded) 
			{
				m_Rigidbody2D.velocity = new Vector2 (m_Rigidbody2D.velocity.x, m_Rigidbody2D.velocity.y - 1);
			}
			else
			{
				Traction(CtrlH);
			}

			errorDetectingRecursionCount = 0; //Used for DirectionCorrection();

			DirectionCorrection();
			//Collision();

			if (m_Grounded) //Handles velocity along ground surface.
			{
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
				//print("Groundperp="+groundperp);
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

				/*
				if(AdjustedVel.normalized.x < 0)
				{
					CtrlH = -CtrlH;
				}
				*/

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

				if(m_Jump)
				{
					m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_Rigidbody2D.velocity.y + m_JumpForce);
					m_Jump = false;
				}
			}

			//Collision();

			print("Velocity at end of physics frame: "+m_Rigidbody2D.velocity);

			//print(GroundNormal);
			Vector2 offset = new Vector2(0, 0.62f);

			#region Animator Controls
			//
			//Animator Controls
			//

			if (!facingDirection) //If facing left
			{
				//print("FACING LEFT!   "+h);
				Vector2 debugLineInverted = ((m_Rigidbody2D.velocity*Time.deltaTime)-offset);
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
				m_DebugLine.SetPosition(1, (m_Rigidbody2D.velocity*Time.deltaTime)-offset);
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
			if (!m_Jump && m_Grounded)
			{
				// Read the jump input in Update so button presses aren't missed.
				m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
			}
		}
   
		private void DirectionCorrection()
		{
			print("DC Executing");
			errorDetectingRecursionCount++;

			if(errorDetectingRecursionCount >= 5)
			{
				throw new Exception("Your recursion code is fucked!");
				//return;
			}

			//Vector2 surfacePosition = MidHit.point;
			//surfacePosition.y += 0.61f;
			//this.transform.position = surfacePosition;
			//print ("MIDHIT NORMAL INITIAL:    " + MidHit.normal);

			//print ("Player Pos:  " + this.transform.position);
			//print ("velocity:  " + m_Rigidbody2D.velocity);

			//print ("predOrigin:  " + predOrigin);
			//print ("MidFoot:  " + m_MidFoot.position);

			Vector2 adjustedBot = m_MidFoot.position; // AdjustedBot marks the bottom of the middle raycast, but 0.02 higher.
			adjustedBot.y = adjustedBot.y - 0.40f;
			RaycastHit2D groundCheck = Physics2D.Raycast(m_MidFoot.position, Vector2.down, 0.42f, mask);
			RaycastHit2D predictedLoc = Physics2D.Raycast(adjustedBot, m_Rigidbody2D.velocity, m_Rigidbody2D.velocity.magnitude*Time.deltaTime, mask);

			Vector2 collider1Position;


			if(m_Rigidbody2D.velocity.x < 0)
			{
				print("              MOVING LEFT!");
				collider1Position.x = m_Collider1.position.x-0.01f;
			}
			else if(m_Rigidbody2D.velocity.x > 0)
			{
				print("              MOVING RIGHT!");
				collider1Position.x = m_Collider1.position.x+0.01f;
			}
			else
			{
				print("alternate");
				collider1Position.x = m_Collider1.position.x;
			}

			//collider1Position.x = m_Collider1.position.x;
			collider1Position.y = m_Collider1.position.y;
			RaycastHit2D collider1Check = Physics2D.Raycast(collider1Position, m_Rigidbody2D.velocity, m_Rigidbody2D.velocity.magnitude*Time.deltaTime, mask);


			if (predictedLoc.collider != null) 
			{//If you're going to hit something.
				collider1Check = Physics2D.Raycast(collider1Position, m_Rigidbody2D.velocity, predictedLoc.distance, mask); //Collider checks go only as far as the first angle correction, so they don't mistake the slope of the floor for an obstacle.

				//print("Velocity before impact: "+m_Rigidbody2D.velocity);
				//print("Velocity after impact: "+m_Rigidbody2D.velocity);

				Vector2 invertedImpactNormal;//This is done in case one of the raycasts is inside the collider, which would cause it to return an inverted normal value.
				invertedImpactNormal.x = -predictedLoc.normal.y; //Inverting the normal.
				invertedImpactNormal.y = predictedLoc.normal.x; //Inverting the normal.

				if(collider1Check.collider != null) //if((collider1Check.collider != null) && (collider1Check.normal != predictedLoc.normal) && (collider1Check.normal != invertedImpactNormal))
				{// If player hits an obstacle that is too high for its feet to take care of.
					//print("Obstacle");
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
				collider1Position.x = m_Collider1.position.x-0.01f;
			}
			else if(m_Rigidbody2D.velocity.x > 0)
			{
				//print("              MOVING RIGHT!");
				collider1Position.x = m_Collider1.position.x+0.01f;
			}
			else
			{
				//print("alternate");
				collider1Position.x = m_Collider1.position.x;
			}

			//collider1Position.x = m_Collider1.position.x;
			collider1Position.y = m_Collider1.position.y;
			//Vector2 futureMove = m_Rigidbody2D.velocity*Time.deltaTime
			//futureColliderPos.x += futureMove

			RaycastHit2D collider1Check = Physics2D.Raycast(collider1Position, m_Rigidbody2D.velocity, m_Rigidbody2D.velocity.magnitude*Time.deltaTime, mask);

			if(collider1Check.collider != null)
			{
				if(collider1Check.collider.OverlapPoint(collider1Position+(m_Rigidbody2D.velocity*Time.deltaTime)))
				{
					//Vector2 setCharPos = predictedLoc.point;
					//setCharPos.y = setCharPos.y + 0.61f;
					//setCharPos.x = setCharPos.x-0.01f;
					//this.transform.position = setCharPos;
					//m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
				}
				else
				{
					//print("Raycasthit punched completely through collider!");
					//return false;
				}
				//print("IMPACT");

				if(m_Rigidbody2D.velocity.x < 0)
				{
					this.transform.position =  new Vector2(collider1Check.point.x+0.01f, collider1Check.point.y+0.2f);
				}
				else if(m_Rigidbody2D.velocity.x > 0)
				{
					this.transform.position =  new Vector2(collider1Check.point.x-0.01f, collider1Check.point.y+0.2f);
				}

				//this.transform.position =  new Vector2(collider1Check.point.x+0.01f, collider1Check.point.y+0.2f);
				m_Rigidbody2D.velocity = new Vector2(0f, m_Rigidbody2D.velocity.y);
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
			//print ("We've hit slope, sir!!");
			//print ("groundCheck.normal=" + groundCheck.normal);
			//print ("inverseGround.normal=" + invertedGroundNormal);
			//print ("predictedLoc.normal=" + predictedLoc.normal);

			//print ("Starting velocity:  " + m_Rigidbody2D.velocity);
			Vector2 expendedVel = predictedLoc.point;
			expendedVel.x -= this.transform.position.x;
			expendedVel.y -= this.transform.position.y;
			//print ("Player Pos:  " + this.transform.position);
			//print ("Impact Pos:  " + predictedLoc.point);
			//print ("Expended velocity:  " + expendedVel);
			Vector2 remainingVel = m_Rigidbody2D.velocity-expendedVel;
			//print ("Remaining velocity:  " + remainingVel);

			m_Grounded = true;
			Vector2 setCharPos = predictedLoc.point;
			setCharPos.y = setCharPos.y + 0.61f;
			this.transform.position = setCharPos;
			//print ("Final Position:  " + this.transform.position);
			m_Rigidbody2D.velocity = remainingVel;
			//TEST CODE AFTER THIS POINT
			//Vector2 newTestRayOrigin = this.transform.position;
			//newTestRayOrigin.y = newTestRayOrigin.y - 0.20f;
			RaycastHit2D groundCheck2 = Physics2D.Raycast(m_MidFoot.position, Vector2.down, 0.42f, mask);
			if (groundCheck2.collider != null) 
			{
				//print(
				//Vector2 surfacePosition = groundCheck2.point;
				//print(groundCheck2.point);
				//surfacePosition.y += 0.61f;
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
			//DirectionCorrection();
			//Collision();

			//print ("Final Position2:  " + this.transform.position);
		}

		private void AirToGround(RaycastHit2D predictedLoc)
		{
			//print ("Starting velocity:  " + m_Rigidbody2D.velocity);
			Vector2 expendedVel = predictedLoc.point;
			expendedVel.x -= this.transform.position.x;
			expendedVel.y -= this.transform.position.y;
			//print ("Player Pos:  " + this.transform.position);
			//print ("Impact Pos:  " + predictedLoc.point);
			//print ("Expended velocity:  " + expendedVel);
			Vector2 remainingVel = m_Rigidbody2D.velocity-expendedVel;
			//print ("Remaining velocity:  " + remainingVel);

			m_Grounded = true;
			Vector2 setCharPos = predictedLoc.point;
			setCharPos.y += 0.61f;
			this.transform.position = setCharPos;
			//print ("Final Position:  " + this.transform.position);
			m_Rigidbody2D.velocity = remainingVel;
			//TEST CODE AFTER THIS POINT
			//Vector2 newTestRayOrigin = this.transform.position;
			//newTestRayOrigin.y = newTestRayOrigin.y - 0.20f;
			RaycastHit2D groundCheck2 = Physics2D.Raycast(m_MidFoot.position, Vector2.down, 0.42f, mask);
			if (groundCheck2.collider != null) {

				//print ("groundCheck2.normal=" + groundCheck2.normal);
				//print ("groundCheck2.collider=" + groundCheck2.transform.gameObject);
				GroundNormal = groundCheck2.normal;
				Traction(CtrlH);
				//DirectionCorrection();
			} 
			else 
			{
				//print ("GroundCheck2=null!");
			}
			//Collision();


			//print ("Final Position2:  " + this.transform.position);
		}
	}
}