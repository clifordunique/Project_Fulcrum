using UnityEngine.UI;
using System;
using UnityEngine;
using EZCameraShake;
using UnityEngine.Networking;
using System.Collections.Generic;

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
	public int inputBufferSize = 2;
	[SerializeField] public Queue<FighterInput> inputBuffer;
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
	protected void Awake()
	{
		inputBuffer = new Queue<FighterInput>();
		FighterAwake();
	}

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
		if(isLocalPlayer)
		{
			inputBuffer.Enqueue(fighterInput);
			if(inputBuffer.Count >= inputBufferSize)
			{
				CmdSendInput(inputBuffer.ToArray());
				//print("Sending...");
				inputBuffer.Clear();
			}
		}
		else
		{
			if(inputBuffer.Count > 0)
			{
				fighterInput = inputBuffer.Dequeue();
				print("Inputbuffer count  = "+inputBuffer.Count);
				//print("Jumpkey state  = "+fighterInput.JumpKey);
			}
		}

		FixedUpdateProcessInput();
		FixedUpdatePhysics(); // Change this to take time.deltatime as an input so you can implement time dilation.
		FixedUpdateAnimation();
		fighterInput.RightClick = false;
		fighterInput.LeftClick = false;
		fighterInput.ZonKey = false;
		fighterInput.JumpKey = false;
	}

	protected override void Update()
	{
		if(!isLocalPlayer){return;}
		UpdateInput();
	}

	protected override void LateUpdate()
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
		if(!isLocalPlayer){return;}
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
		GameObject newAirPunch = (GameObject)Instantiate(p_AirPunchPrefab, this.transform.position, punchAngle, this.transform);

		if(randomness1>0)
		{
			yTransform = -1f;
			newAirPunch.GetComponentInChildren<SpriteRenderer>().sortingLayerName = "Background";	
		}

		Vector3 theLocalScale = new Vector3 (xTransform, yTransform, 1f);
		Vector3 theLocalTranslate = new Vector3(randomness1,randomness2, 0);

		newAirPunch.transform.localScale = theLocalScale;
		newAirPunch.transform.Translate(theLocalTranslate);
		newAirPunch.transform.Rotate(new Vector3(0,0,randomness1));

		CmdThrowPunch(aimDirection);

		o_FighterAudio.PunchSound();
	}

	[Command]protected void CmdThrowPunch(Vector2 aimDirection)
	{
		if(isServer&&!isLocalPlayer)
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
			GameObject newAirPunch = (GameObject)Instantiate(p_AirPunchPrefab, this.transform.position, punchAngle, this.transform);

			//NetworkServer.Spawn(newAirPunch);
			if(randomness1>0)
			{
				yTransform = -1f;
				newAirPunch.GetComponentInChildren<SpriteRenderer>().sortingLayerName = "Background";	
			}

			Vector3 theLocalScale = new Vector3 (xTransform, yTransform, 1f);
			Vector3 theLocalTranslate = new Vector3(randomness1,randomness2, 0);

			newAirPunch.transform.localScale = theLocalScale;
			newAirPunch.transform.Translate(theLocalTranslate);
			newAirPunch.transform.Rotate(new Vector3(0,0,randomness1));
		}
		//RpcThrowPunch(newAirPunch, theLocalScale, theLocalTranslate);
		RpcThrowPunch(aimDirection);
	}

	[ClientRpc]protected void RpcThrowPunch(Vector2 aimDirection)
	{
		if(isLocalPlayer){return;}
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
		GameObject newAirPunch = (GameObject)Instantiate(p_AirPunchPrefab, this.transform.position, punchAngle, this.transform);

		//NetworkServer.Spawn(newAirPunch);
		if(randomness1>0)
		{
			yTransform = -1f;
			newAirPunch.GetComponentInChildren<SpriteRenderer>().sortingLayerName = "Background";	
		}

		Vector3 theLocalScale = new Vector3 (xTransform, yTransform, 1f);
		Vector3 theLocalTranslate = new Vector3(randomness1,randomness2, 0);

		newAirPunch.transform.localScale = theLocalScale;
		newAirPunch.transform.Translate(theLocalTranslate);
		newAirPunch.transform.Rotate(new Vector3(0,0,randomness1));
	}

	[Command(channel=1)]protected void CmdSendInput(FighterInput[] theInput)
	{
//		if(!isLocalPlayer)
//		{
//			//this.fighterInput = theInput[0];
//			//this.inputBuffer.Clear();
//			foreach (FighterInput i in theInput)
//			{
//				this.inputBuffer.Enqueue(i);
//			}
//		}
		RpcSendInput(theInput);
	}

	[ClientRpc(channel=1)]protected void RpcSendInput(FighterInput[]  theInput)
	{
		if(!isLocalPlayer)
		{
			//this.inputBuffer.Clear();
			foreach (FighterInput i in theInput)
			{
				this.inputBuffer.Enqueue(i);
			}
			//this.fighterInput = theInput;
		}
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

	protected override void FixedUpdateProcessInput()
	{
		m_Impact = false;
		m_Landing = false;
		m_Kneeling = false;
		g_ZonStance = -1;

		fighterInput.PlayerMouseVector = fighterInput.MouseWorldPos-Vec2(this.transform.position);
		if(!(fighterInput.LeftKey||fighterInput.RightKey) || (fighterInput.LeftKey && fighterInput.RightKey))
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
		else if(fighterInput.LeftKey)
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
		if(fighterInput.DownKey&&m_Grounded)
		{
			m_Kneeling = true;
			CtrlH = 0;
			g_ZonStance = 0; // Kneeling stance.
		}
		else
		{
			g_ZonJumpCharge=0;
		}
			
		if(fighterInput.JumpKey&&(m_Grounded||m_Ceilinged||m_LeftWalled||m_RightWalled))
		{
			if(m_Kneeling)
			{
				ZonJump(fighterInput.PlayerMouseVector.normalized);
			}
			else
			{
				Jump(CtrlH);
			}
		}

		if(fighterInput.LeftClick&&(d_DevMode||d_ClickToKnockPlayer))
		{
			m_Vel += fighterInput.PlayerMouseVector*10;
			//print("Leftclick detected");
			fighterInput.LeftClick = false;
		}	

		if(fighterInput.LeftClick&&!(d_DevMode||d_ClickToKnockPlayer)&&!m_Kneeling)
		{
			if(!(fighterInput.LeftKey&&(fighterInput.PlayerMouseVector.normalized.x>0))&&!(fighterInput.RightKey&&(fighterInput.PlayerMouseVector.normalized.x<0))) // If trying to run opposite your punch direction, do not punch.
			{
				ThrowPunch(fighterInput.PlayerMouseVector.normalized);
			}
			//print("Leftclick detected");
			fighterInput.LeftClick = false;
		}	

		if(fighterInput.RightClick&&(d_DevMode))
		{
			//GameObject newMarker = (GameObject)Instantiate(o_DebugMarker);
			//newMarker.name = "DebugMarker";
			//newMarker.transform.position = fighterInput.MouseWorldPos;
			fighterInput.RightClick = false;
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
			fighterInput.LeftClick = true;
		}

		if(Input.GetMouseButtonDown(1))
		{
			fighterInput.RightClick = true;
		}

		if(Input.GetButtonDown("Spooling"))
		{
			fighterInput.ZonKey = true;				
		}

		if(Input.GetButtonDown("Jump"))
		{
			fighterInput.JumpKey = true;				
		}
		if(autoJump)
		{
			fighterInput.JumpKey = true;
		}

		Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		fighterInput.MouseWorldPos = Vec2(mousePoint);

		fighterInput.LeftKey = Input.GetButton("Left");
		fighterInput.RightKey = Input.GetButton("Right");
		fighterInput.UpKey = Input.GetButton("Up");
		fighterInput.DownKey = Input.GetButton("Down");


		if(o_Speedometer != null)
		{
			o_Speedometer.text = ""+Math.Round(m_Vel.magnitude,0);
		}

//		if(!fighterInput.JumpKey && (m_Grounded||m_Ceilinged||m_LeftWalled||m_RightWalled))
//		{
//			// Read the jump input in Update so button presses aren't missed.
//
//			fighterInput.JumpKey = Input.GetButtonDown("Jump");
//			if(autoJump)
//			{
//				fighterInput.JumpKey = true;
//			}
//		}
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

			if((fighterInput.MouseWorldPos.x-this.transform.position.x)<0)
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

		//CmdSetFacingDirection(facingDirection);
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
		fighterInput.JumpKey = false;
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