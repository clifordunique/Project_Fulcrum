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
	[SerializeField] public GameObject p_AirPunchPrefab;	// Reference to the air punch prefab.
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
	protected void Start()
	{
		if(!isLocalPlayer){return;}
		o_MainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
		o_MainCamera.transform.parent.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -10f);
		o_MainCamera.transform.parent.SetParent(this.transform);
		o_Speedometer = GameObject.Find("Speedometer").GetComponent<Text>();
	}

	protected override void FixedUpdate()
	{
		FixedUpdateInput();
		FixedUpdatePhysics();
		i_RightClick = false;
		i_LeftClick = false;
		i_ZonKey = false;
		FixedUpdateAnimation();
	}

	protected override void Update()
	{
		UpdateInput();
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
		GameObject newAirPunch = (GameObject)Instantiate(p_AirPunchPrefab, this.transform.position, punchAngle,this.transform);

		if(randomness1>0)
		{
			yTransform = -1f;
			newAirPunch.GetComponentInChildren<SpriteRenderer>().sortingLayerName = "Background";	
		}

		newAirPunch.transform.localScale = new Vector3 (xTransform, yTransform, 1f);
		newAirPunch.transform.Translate(new Vector3(randomness1,randomness2, 0));
		newAirPunch.transform.Rotate(new Vector3(0,0,randomness1));

		o_FighterAudio.PunchSound();
	}

	[Command]protected void CmdSetFacingDirection(bool isFacingRight)
	{
		RpcSetFacingDirection(isFacingRight);
	}
	[ClientRpc]protected void RpcSetFacingDirection(bool isFacingRight)
	{
		if(!isLocalPlayer)
		{
			o_SpriteRenderer.flipX = !isFacingRight;
			facingDirection = isFacingRight;
		}
		else
		{
			//print("DID NOT CHANGE FACING ON LOCAL PLAYER!");
		}
	}

	protected override void FixedUpdateInput()
	{
		m_Impact = false;
		m_Landing = false;
		m_Kneeling = false;
		g_ZonStance = -1;

		i_PlayerMouseVector = i_MouseWorldPos-Vec2(this.transform.position);
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

		if(i_LeftClick&&!(d_DevMode||d_ClickToKnockPlayer)&&!m_Kneeling)
		{
			if(!(i_LeftKey&&(i_PlayerMouseVector.normalized.x>0))&&!(i_RightKey&&(i_PlayerMouseVector.normalized.x<0))) // If trying to run opposite your punch direction, do not punch.
			{
				ThrowPunch(i_PlayerMouseVector.normalized);
			}
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

	}

	protected override void UpdateInput()
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

	protected override void FixedUpdateAnimation()
	{
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
			facingDirection = true;
			//o_CharSprite.transform.localPosition = new Vector3(0.13f, 0f,0f);
		}

		if(m_RightWalled&&!m_Grounded)
		{
			o_Anim.SetBool("Walled", true);
			facingDirection = false;
			//o_CharSprite.transform.localPosition = new Vector3(-0.13f, 0f,0f);
		}

		if(m_Grounded || !(m_RightWalled||m_LeftWalled))
		{
			//o_CharSprite.transform.localPosition = new Vector3(0f,0f,0f);
		}

		if (!facingDirection) //If facing left
		{
			//print("FACING LEFT!");
			//o_CharSprite.transform.localScale = new Vector3 (-1f, 1f, 1f);
			o_SpriteRenderer.flipX = true;
			//print(o_SpriteRenderer.flipX);
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
			//print("FACING RIGHT!");

			//o_CharSprite.transform.localScale = new Vector3 (1f, 1f, 1f);
			o_SpriteRenderer.flipX = false;
			//print(o_SpriteRenderer.flipX);
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
			}
			else
			{
				facingDirection = true;
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

		CmdSetFacingDirection(facingDirection);
	}

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
	}
		
	protected void ZonJump(Vector2 jumpNormal)
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