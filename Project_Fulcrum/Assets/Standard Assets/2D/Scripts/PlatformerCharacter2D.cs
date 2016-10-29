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
            // Setting up references.
            m_GroundCheck = transform.Find("GroundCheck");
			m_LeftFoot = transform.Find("LeftFoot");
			m_LeftLine = m_LeftFoot.GetComponent<LineRenderer>();

			m_MidFoot = transform.Find("MidFoot");
			m_MidLine = m_MidFoot.GetComponent<LineRenderer>();

			m_DebugLine = GetComponent<LineRenderer>();
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
			m_LeftLine.SetColors (Color.red, Color.red);
			m_RightLine.SetColors (Color.red, Color.red);

			RaycastHit2D LeftHit = Physics2D.Raycast (m_LeftFoot.position, Vector2.down, 0.75f, mask);
			if (LeftHit.collider != null)
			{
				leftcontact = true;
				m_LeftLine.SetColors (Color.green, Color.green);
			} 
			else
			{
				leftcontact = false;
			}

			RaycastHit2D MidHit = Physics2D.Raycast (m_MidFoot.position, Vector2.down, 0.75f, mask);
			if (MidHit.collider != null) 
			{
				midcontact = true;
				m_MidLine.SetColors (Color.green, Color.green);
			} 
			else 
			{
				midcontact = false;
			}
				
			RaycastHit2D RightHit = Physics2D.Raycast (m_RightFoot.position, Vector2.down, 0.75f, mask);
			if (RightHit.collider != null) 
			{
				rightcontact = true;
				m_RightLine.SetColors (Color.green, Color.green);
			} 
			else 
			{
				rightcontact = false;
			}


			if (rightcontact && leftcontact) 
			{
				m_Grounded = true;
			} 
			else 
			{
				m_Grounded = false;
			}


			if (m_Grounded) //RightHit.distance <= 0.1f || LeftHit.distance <= 0.1f
			{

				if(m_Rigidbody2D.velocity.y >= 0)
				{
					if (LeftHit.distance >= RightHit.distance) 
					{
						GroundNormal = LeftHit.normal;
						//print(LeftHit.normal);
						//m_Rigidbody2D.position = new Vector2 (m_Rigidbody2D.position.x, m_Rigidbody2D.position.y+(0.5f - LeftHit.distance));
					} 
					else 
					{
						GroundNormal = RightHit.normal;
						//print(RightHit.normal);
						//m_Rigidbody2D.position = new Vector2 (m_Rigidbody2D.position.x, m_Rigidbody2D.position.y+(0.5f - RightHit.distance));
					}
				}
				else
				{
					if (LeftHit.distance <= RightHit.distance) 
					{
						GroundNormal = LeftHit.normal;
						//print(LeftHit.normal);
						//m_Rigidbody2D.position = new Vector2 (m_Rigidbody2D.position.x, m_Rigidbody2D.position.y+(0.5f - LeftHit.distance));
					} 
					else 
					{
						GroundNormal = RightHit.normal;
						//print(RightHit.normal);
						//m_Rigidbody2D.position = new Vector2 (m_Rigidbody2D.position.x, m_Rigidbody2D.position.y+(0.5f - RightHit.distance));
					}	
				}
				/*
				if(RightHit.distance <= 0.25f || LeftHit.distance <= 0.25f)
				{
					if (LeftHit.distance <= RightHit.distance) 
					{
						GroundNormal = LeftHit.normal;
						//print(LeftHit.normal);
						//m_Rigidbody2D.position = new Vector2 (m_Rigidbody2D.position.x, m_Rigidbody2D.position.y+(0.75f - LeftHit.distance));
					} 
					else 
					{
						GroundNormal = RightHit.normal;
						//print(RightHit.normal);
						//m_Rigidbody2D.position = new Vector2 (m_Rigidbody2D.position.x, m_Rigidbody2D.position.y+(0.75f - RightHit.distance));
					}
				}
				else
				{
					if (LeftHit.distance >= RightHit.distance) 
					{
						GroundNormal = LeftHit.normal;
						//print(LeftHit.normal);
						//m_Rigidbody2D.position = new Vector2 (m_Rigidbody2D.position.x, m_Rigidbody2D.position.y+(0.5f - LeftHit.distance));
					} 
					else 
					{
						GroundNormal = RightHit.normal;
						//print(RightHit.normal);
						//m_Rigidbody2D.position = new Vector2 (m_Rigidbody2D.position.x, m_Rigidbody2D.position.y+(0.5f - RightHit.distance));
					}
				}
				*/

			
				GroundNormal = MidHit.normal;
			}

			if (!m_Grounded) 
			{
				m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_Rigidbody2D.velocity.y - 1);
				//PVel.y -= 1;
			} else {
				//PVel.y = 0;
			}
				
			if (m_Grounded) 
			{
				Vector2 groundperp;
				Vector2 AdjustedVel;

			//	if(m_FacingRight)
			//	{
					groundperp.x = GroundNormal.y;
					groundperp.y = -GroundNormal.x;
			//	}
			//	else
			//	{
			//		groundperp.x = -GroundNormal.y;
			//		groundperp.y = GroundNormal.x;
			//	}

				//m_Rigidbody2D.velocity.x += h;
				//m_Rigidbody2D.velocity = new Vector2( m_Rigidbody2D.velocity.x + h*10 ,m_Rigidbody2D.velocity.y);

				//groundperp.x = GroundNormal.y;
				//groundperp.y = -GroundNormal.x;

				//float test = Vector2.Dot(m_Rigidbody2D.velocity, groundperp);
				//print(test);
				float projectionVal = Vector2.Dot(m_Rigidbody2D.velocity, groundperp)/groundperp.sqrMagnitude;
				//print(projectionVal);
				AdjustedVel = groundperp * projectionVal;
				//print(AdjustedVel);

				if(AdjustedVel.normalized.x < 0)
				{
					h = -h;
				}

				if(m_Rigidbody2D.velocity == Vector2.zero)
				{
					m_Rigidbody2D.velocity = new Vector2(h, m_Rigidbody2D.velocity.y);
					print("FUCKYOU");
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
					

				//m_Rigidbody2D.velocity = new Vector2 (m_Rigidbody2D.velocity.x, PVel.y); //remove later
			}


			print(GroundNormal);

			m_DebugLine.SetPosition(1, m_Rigidbody2D.velocity);
			//m_DebugLine.SetPosition(1, GroundNormal);


        }

		private void Update()
		{
			if (!m_Jump && m_Grounded)
			{
				// Read the jump input in Update so button presses aren't missed.
				m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
			}
		}
    }
}