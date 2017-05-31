using UnityEngine.UI;
using System;
using UnityEngine;
using EZCameraShake;
using UnityEngine.Networking;

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
public class Player : FighterChar
{        	
	//############################################################################################################################################################################################################
	// HANDLING VARIABLES
	//###########################################################################################################################################################################
	#region MOVEMENT HANDLING
	#endregion
	//############################################################################################################################################################################################################
	// OBJECT REFERENCES
	//###########################################################################################################################################################################
	#region OBJECT REFERENCES
	[Header("Player Components:")]

	[SerializeField] private Text o_Speedometer;      		// Reference to the speed indicator (dev tool).
	[SerializeField] private Camera o_MainCamera;			// Reference to the main camera.
	[SerializeField] private CameraShaker o_CamShaker;		// Reference to the main camera's shaking controller.
	[SerializeField] public Spooler o_Spooler;				// Reference to the character's spooler object, which handles power charging gameplay.
	[SerializeField] public Healthbar o_Healthbar;			// Reference to the Healthbar UI element.
	#endregion
	//############################################################################################################################################################################################################
	// PHYSICS&RAYCASTING
	//###########################################################################################################################################################################
	#region PHYSICS&RAYCASTING
	[Header("Player State:")]
	#endregion
	//##########################################################################################################################################################################
	// PLAYER INPUT VARIABLES
	//###########################################################################################################################################################################
	#region PLAYERINPUT
	#endregion
	//############################################################################################################################################################################################################
	// DEBUGGING VARIABLES
	//##########################################################################################################################################################################
	#region DEBUGGING
	#endregion
	//############################################################################################################################################################################################################
	// VISUAL&SOUND VARIABLES
	//###########################################################################################################################################################################
	#region VISUALS&SOUND
	[SerializeField]private float v_CameraZoom; 					// Amount of camera zoom.
	#endregion 
	//############################################################################################################################################################################################################
	// GAMEPLAY VARIABLES
	//###########################################################################################################################################################################
	#region GAMEPLAY VARIABLES
	#endregion 


	//########################################################################################################################################
	// CORE FUNCTIONS
	//########################################################################################################################################
	#region CORE FUNCTIONS
	private void Awake()
	{
		FighterAwake();
	}

	private void Start()
	{
		if(!isLocalPlayer){return;}
		o_MainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
		o_MainCamera.transform.parent.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -10f);
		o_MainCamera.transform.parent.SetParent(this.transform);

		o_Speedometer = GameObject.Find("Speedometer").GetComponent<Text>();

	}

	private void FixedUpdate()
	{
		Vector2 distanceTravelled = Vector3.zero;
		if(!isLocalPlayer){return;}
		Vector2 finalPos = new Vector2(this.transform.position.x+m_RemainingMovement.x, this.transform.position.y+m_RemainingMovement.y);
		this.transform.position = finalPos;
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

		if(i_JumpKey)
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
	
		if(i_LeftClick&&(d_DevMode||d_ClickToKnockPlayer))
		{
			m_Vel += i_PlayerMouseVector*10;
			print("Leftclick detected");
			i_LeftClick = false;
		}	
		
				if(i_RightClick&&(d_DevMode))
				{
					//GameObject newMarker = (GameObject)Instantiate(o_DebugMarker);
					//newMarker.name = "DebugMarker";
					//newMarker.transform.position = i_MouseWorldPos;
					i_RightClick = false;
					float Magnitude = 2f;
					//float Magnitude = 0.5f;
					float Roughness = 10f;
					//float FadeOutTime = 0.6f;
					float FadeOutTime = 5f;
					float FadeInTime = 0f;
					//Vector3 RotInfluence = new Vector3(0,0,0);
					//Vector3 PosInfluence = new Vector3(1,1,0);
					Vector3 RotInfluence = new Vector3(1,1,1);
					Vector3 PosInfluence = new Vector3(0.15f,0.15f,0.15f);
					CameraShaker.Instance.ShakeOnce(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);
				}	

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

		distanceTravelled = new Vector2(this.transform.position.x-startingPos.x,this.transform.position.y-startingPos.y);
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

		if(m_Grounded)
		{
			this.GetComponent<NetworkTransform>().grounded = true;
		}
		else
		{
			this.GetComponent<NetworkTransform>().grounded = false;
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
		i_ZonKey = false;
	}

	private void Update()
	{
		if(!isLocalPlayer)
		{
			return;
		}
		if(Input.GetMouseButtonDown(0))
		{
			i_LeftClick = true;
		}

		if(Input.GetMouseButtonDown(1))
		{
			i_RightClick = true;
		}

		if(Input.GetButtonDown("Spooling"))
		{
			i_ZonKey = true;				
		}

		Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		i_MouseWorldPos = Vec2(mousePoint);

		i_LeftKey = Input.GetButton("Left");
		i_RightKey = Input.GetButton("Right");
		i_UpKey = Input.GetButton("Up");
		i_DownKey = Input.GetButton("Down");

		if(o_Speedometer != null)
		{
			o_Speedometer.text = ""+Math.Round(m_Vel.magnitude,0);
		}

		if(!i_JumpKey && (m_Grounded||m_Ceilinged||m_LeftWalled||m_RightWalled))
		{
			// Read the jump input in Update so button presses aren't missed.

			i_JumpKey = Input.GetButtonDown("Jump");
			if(autoJump)
			{
				i_JumpKey = true;
			}
		}

	}

	private void LateUpdate()
	{
		if(isLocalPlayer)
		{
			CameraControl();
		}
	}
	#endregion
	//###################################################################################################################################
	// CUSTOM FUNCTIONS
	//###################################################################################################################################
	#region CUSTOM FUNCTIONS

	private void CameraControl()
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
	}
		
	private void ZonJump(Vector2 jumpNormal)
	{
		g_ZonJumpCharge = o_Spooler.GetTotalPower();
		m_Vel = jumpNormal*(m_ZonJumpForceBase+(m_ZonJumpForcePerCharge*g_ZonJumpCharge));
		g_ZonJumpCharge = 0;		
		i_JumpKey = false;
		o_FighterAudio.JumpSound();
		o_Spooler.Reset();
	}
		

	#endregion
	//###################################################################################################################################
	// PUBLIC FUNCTIONS
	//###################################################################################################################################
	#region PUBLIC FUNCTIONS

	#endregion
}