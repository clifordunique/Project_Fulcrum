using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
namespace UnityStandardAssets._2D
{
    public class PlatformerCharacter2D : MonoBehaviour
    {
        [SerializeField] private float m_MaxSpeed = 10f;                    // The fastest the player can travel in the x axis.
        [SerializeField] private float m_JumpForce = 10f;                  // Amount of force added when the player jumps.
        [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;  // Amount of maxSpeed applied to crouching movement. 1 = 100%
        [SerializeField] private bool m_AirControl = false;                 // Whether or not a player can steer while jumping;
        [SerializeField] private LayerMask m_WhatIsGround;                  // A mask determining what is ground to the character
		[SerializeField] private Vector2 GroundNormal;
		[SerializeField] private bool m_Grounded;    
		[SerializeField] private LayerMask mask;
		[SerializeField] private float maxRunSpeed;

        private Transform m_GroundCheck;    // A position marking where to check if the player is grounded.
        const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
      	private Transform m_CeilingCheck;   // A position marking where to check for ceilings
        const float k_CeilingRadius = .01f; // Radius of the overlap circle to determine if the player can stand up
        private Animator m_Anim;            // Reference to the player's animator component.
        private Rigidbody2D m_Rigidbody2D;
		private SpriteRenderer m_SpriteRenderer;
		private Transform m_LeftFoot;
		private Transform m_RightFoot;
		private Transform m_MidFoot;
		private LineRenderer m_DebugLine;
		private LineRenderer m_LeftLine;
		private LineRenderer m_MidLine;
		private LineRenderer m_RightLine;
		private Vector2 PVel;
		private bool leftcontact;
		private bool midcontact;
		private bool rightcontact;
		private bool m_Jump;



        private void Awake()
        {
			m_DebugLine = GetComponent<LineRenderer>();

            m_GroundCheck = transform.Find("GroundCheck");

			m_LeftFoot = transform.Find("LeftFoot");
			m_LeftLine = m_LeftFoot.GetComponent<LineRenderer>();

			m_MidFoot = transform.Find("MidFoot");
			m_MidLine = m_MidFoot.GetComponent<LineRenderer>();

			m_RightFoot = transform.Find("RightFoot");
			m_RightLine = m_RightFoot.GetComponent<LineRenderer>();

            m_CeilingCheck = transform.Find("CeilingCheck");
            m_Anim = GetComponent<Animator>();
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
			m_SpriteRenderer = GetComponent<SpriteRenderer>();
        }


        private void FixedUpdate()
		{
			float h = CrossPlatformInputManager.GetAxis("Horizontal");
			m_Grounded = false;
			m_SpriteRenderer.color = Color.white;
			m_LeftLine.endColor = Color.red;
			m_LeftLine.startColor = Color.red;
			m_RightLine.endColor = Color.red;
			m_RightLine.startColor = Color.red;
			m_MidLine.endColor = Color.red;
			m_MidLine.startColor = Color.red;

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
				Vector2 surfacePosition = MidHit.point;
				surfacePosition.y += 0.61f;
				this.transform.position = surfacePosition;
				//print ("MIDHIT NORMAL INITIAL:    " + MidHit.normal);
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
				

			if (midcontact) 
			{
				m_Grounded = true;
			} 
			else 
			{
				m_Grounded = false;
			}

			/*
			if (m_Grounded) //RightHit.distance <= 0.1f || LeftHit.distance <= 0.1f
			{

				if(m_Rigidbody2D.velocity.y >= 0)
				{
					if (LeftHit.distance >= RightHit.distance) 
					{
						GroundNormal = LeftHit.normal;
						//print(LeftHit.normal);
						//this.transform.position = new Vector2 (this.transform.position.x, this.transform.position.y+(0.5f - LeftHit.distance));
					} 
					else 
					{
						GroundNormal = RightHit.normal;
						//print(RightHit.normal);
						//this.transform.position = new Vector2 (this.transform.position.x, this.transform.position.y+(0.5f - RightHit.distance));
					}
				}
				else
				{
					if (LeftHit.distance <= RightHit.distance) 
					{
						GroundNormal = LeftHit.normal;
						//print(LeftHit.normal);
						//this.transform.position = new Vector2 (this.transform.position.x, this.transform.position.y+(0.5f - LeftHit.distance));
					} 
					else 
					{
						GroundNormal = RightHit.normal;
						//print(RightHit.normal);
						//this.transform.position = new Vector2 (this.transform.position.x, this.transform.position.y+(0.5f - RightHit.distance));
					}	
				}
					
				GroundNormal = MidHit.normal;
			}
			*/


			/*
			//Midair Physics!
			if (!m_Grounded) 
			{
				m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_Rigidbody2D.velocity.y - 1);
				Vector2 adjBot = m_MidFoot.position;
				adjBot.y = adjBot.y - 0.42f;
				RaycastHit2D PredictedLoc = Physics2D.Raycast(adjBot, m_Rigidbody2D.velocity, m_Rigidbody2D.velocity.magnitude*Time.deltaTime, mask);

				if (PredictedLoc.collider != null) 
				{
					m_Rigidbody2D.velocity = DirectionCorrection(PredictedLoc);
					//m_Rigidbody2D.velocity = Vector2.zero;	
					//print ("We've hit land, captain!!");
					m_Grounded = true;
					Vector2 setCharPos = PredictedLoc.point;
					setCharPos.y = setCharPos.y + 0.61f;
					this.transform.position = setCharPos;
					//this.transform.position
				} 
			}
			*/

			//Midair Physics!
			if (!m_Grounded) 
			{
				//print ("FLIGHT!");
				m_Rigidbody2D.velocity = new Vector2 (m_Rigidbody2D.velocity.x, m_Rigidbody2D.velocity.y - 1);
			}

			DirectionCorrection ();

			if (m_Grounded) //Handles velocity along ground surface.
			{
				Vector2 groundperp;
				Vector2 AdjustedVel;
			
				groundperp.x = GroundNormal.y;
				groundperp.y = -GroundNormal.x;
				//print("Groundperp="+groundperp);
				float projectionVal = Vector2.Dot(m_Rigidbody2D.velocity, groundperp)/groundperp.sqrMagnitude;
				//print("P"+projectionVal);
				AdjustedVel = groundperp * projectionVal;
				//print("A"+AdjustedVel);

				if(AdjustedVel.normalized.x < 0)
				{
					h = -h;
				}

				if(m_Rigidbody2D.velocity == Vector2.zero)
				{
					m_Rigidbody2D.velocity = new Vector2(h, m_Rigidbody2D.velocity.y);
					//print("FUCKYOU");
				}
				else
				{
					m_Rigidbody2D.velocity = AdjustedVel + AdjustedVel.normalized*h;
				}

				if(m_Jump)
				{
					m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_Rigidbody2D.velocity.y + m_JumpForce);
					m_Jump = false;
				}

			}


			//print(GroundNormal);
			Vector2 offset = new Vector2(0, 0.62f);

			m_DebugLine.SetPosition(1, (m_Rigidbody2D.velocity*Time.deltaTime)-offset);

			//
			//Animator Controls
			//

			if (m_Rigidbody2D.velocity.x < 0) 
			{
				this.transform.localScale = new Vector3 (-1f, 1f, 1f);
			} 
			else 
			{
				this.transform.localScale = new Vector3 (1f, 1f, 1f);
			}

			m_Anim.SetFloat("Speed", m_Rigidbody2D.velocity.magnitude); //m_Rigidbody2D.velocity.magnitude

			float multiplier = 1;
			if(m_Rigidbody2D.velocity.magnitude > 20.0f)
			{
				multiplier = ((m_Rigidbody2D.velocity.magnitude - 20) / 20)+1;
			}

			m_Anim.SetFloat("Multiplier", multiplier);

			if (!m_Grounded) 
			{
				print ("Flying");
			}
			m_Anim.SetBool("Ground", m_Grounded);
			//print ("Final Position4  " + this.transform.position);
			//m_Rigidbody2D.velocity = new Vector2 (m_Rigidbody2D.velocity.x, PVel.y); //remove later

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

			Vector2 adjustedBot = m_MidFoot.position;
			adjustedBot.y = adjustedBot.y - 0.40f;
			RaycastHit2D groundCheck = Physics2D.Raycast(m_MidFoot.position, Vector2.down, 0.42f, mask);
			RaycastHit2D predictedLoc = Physics2D.Raycast(adjustedBot, m_Rigidbody2D.velocity, m_Rigidbody2D.velocity.magnitude*Time.deltaTime, mask);
			Vector2 invertedGroundNormal;//This is done in case one of the raycasts is inside the collider, which would cause it to return an inverted normal value.
			invertedGroundNormal.x = -groundCheck.normal.y;
			invertedGroundNormal.y = groundCheck.normal.x;

			//print("MIDFOOT POSITION=" + m_MidFoot.position);
			//print("THEBODY POSITION=" + this.transform.position);
			//print("THETRAN POSITION=" + this.transform.position);

			if (predictedLoc.collider != null) 
			{
				if (groundCheck.collider == null) {
					//print ("We've hit land, captain!!");
					//
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
					if (groundCheck2.collider != null) {
						
						//print ("groundCheck2.normal=" + groundCheck2.normal);
						//print ("groundCheck2.collider=" + groundCheck2.transform.gameObject);
						GroundNormal = groundCheck2.normal;
					} 
					else 
					{
						//print ("GroundCheck2=null!");
					}


					//DirectionCorrection();
					//print ("Final Position2:  " + this.transform.position);
					return;
				} 
				else if (groundCheck.normal != predictedLoc.normal && invertedGroundNormal != predictedLoc.normal) 
				{
					
					//print ("We've hit slope, sir!!");
					//print ("groundCheck.normal=" + groundCheck.normal);
					//print ("inverseGround.normal=" + invertedGroundNormal);
					//print ("predictedLoc.normal=" + predictedLoc.normal);

					if (groundCheck.normal == predictedLoc.normal || invertedGroundNormal == predictedLoc.normal) {
						//print ("They're equal.");
					} 
					else 
					{
						//print ("They're not equal.");

					}


					//
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
						//print ("something is happens!");
						//print ("groundCheck2.normal=" + groundCheck2.normal);
						//print ("groundCheck2.collider=" + groundCheck2.transform.gameObject);
					}
					GroundNormal = groundCheck2.normal;
					//DirectionCorrection();
					//print ("Final Position2:  " + this.transform.position);
					return;
				}
				else 
				{
					//print ("Same surface, no trajectory change needed");
					return;
				}
			} 
		}

		private void Traction()
		{
			if (h = 0) 
			{//if not pressing any move direction, slow to zero linearly.
				
				if (m_Rigidbody2D.velocity.x > 0) {
					m_Rigidbody2D.velocity.x -= 1;
				} 
				else 
				{
					m_Rigidbody2D.velocity.x += 1;
				}

				if (m_Rigidbody2D.velocity.y > 0) {
					m_Rigidbody2D.velocity.y -= 1;
				} 
				else 
				{
					m_Rigidbody2D.velocity.y += 1;
				}

			}
			else if((h > 0 && m_Rigidbody2D.velocity.x > 0) || (h < 0 && m_Rigidbody2D.velocity.x < 0))
			{//if pressing same button as move direction, slow to MAXSPEED.
				if(m_Rigidbody2D.velocity.magnitude >= maxRunSpeed)
				{
					m_Rigidbody2D.velocity = m_Rigidbody2D.velocity
				}
			}
			else if((h > 0 && m_Rigidbody2D.velocity.x < 0) || (h < 0 && m_Rigidbody2D.velocity.x > 0))
			{//if pressing button opposite of move direction, slow to zero exponentially.
				m_Rigidbody2D.velocity = m_Rigidbody2D.velocity/2;
			}
		}
	}
}