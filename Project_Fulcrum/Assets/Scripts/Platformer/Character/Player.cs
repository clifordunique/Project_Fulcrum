using UnityEngine.UI;
using System;
using UnityEngine;
using EZCameraShake;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
	[SerializeField] private Text o_Speedometer;      			// Reference to the speed indicator (dev tool).
	[SerializeField] private Text o_ZonCounter;      			// Reference to the level of zon power (dev tool).
	[SerializeField] private Camera o_MainCamera;				// Reference to the main camera.
	[SerializeField] private CameraShaker o_CamShaker;			// Reference to the main camera's shaking controller.
	[SerializeField] public Spooler o_Spooler;					// Reference to the character's spooler object, which handles power charging gameplay.
	[SerializeField] public Healthbar o_Healthbar;				// Reference to the Healthbar UI element.
	[SerializeField] private ProximityLiner o_ProximityLiner;	// Reference to the proximity line handler object. This handles the little lines indicating the direction of offscreen enemies.
	[SerializeField] private GameObject p_ZonPulse;				// Reference to the Zon Pulse prefab, a pulsewave that emanates from the fighter when they disperse zon power.
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
	[SerializeField] public Queue<FighterState> inputBuffer;
	[SerializeField][ReadOnlyAttribute]public bool i_DevKey1;
	[SerializeField][ReadOnlyAttribute]public bool i_DevKey2;
	[SerializeField][ReadOnlyAttribute]public bool i_DevKey3;
	[SerializeField][ReadOnlyAttribute]public bool i_DevKey4;
	[SerializeField][ReadOnlyAttribute]public float i_LeftClickHoldDuration;
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
	//############################################################################################################################################################################################################
	// NETWORKING VARIABLES
	//###########################################################################################################################################################################
	#region NETWORKING VARIABLES
	[SerializeField]private bool sceneIsReady = false;
	#endregion 
	//########################################################################################################################################
	// CORE FUNCTIONS
	//########################################################################################################################################
	#region CORE FUNCTIONS
	protected override void Awake()
	{
		inputBuffer = new Queue<FighterState>();
		isAPlayer = true;
		FighterState.DevMode = true;
		FighterAwake();
	}

	void OnDestroy()
	{
		SceneManager.sceneLoaded -= SceneLoadPlayer;
	}

    void OnEnable()
    {
		SceneManager.sceneLoaded += SceneLoadPlayer;
    }

    void OnDisable()
    {
		SceneManager.sceneLoaded -= SceneLoadPlayer;
    }

	protected void SceneLoadPlayer(Scene scene, LoadSceneMode mode)
	{
		sceneIsReady = true;
		if(!isLocalPlayer||!isClient){return;}
		print("Executing post-scenelaunch code!");
		o_MainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
		o_MainCamera.transform.parent.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -10f);
		o_MainCamera.transform.parent.SetParent(this.transform);
		o_Speedometer = GameObject.Find("Speedometer").GetComponent<Text>();
		o_ZonCounter = GameObject.Find("Zon Counter").GetComponent<Text>();
		o_Healthbar = GameObject.Find("Healthbar").GetComponent<Healthbar>();

	}

	protected void Start()
	{
		o_ProximityLiner = this.GetComponent<ProximityLiner>();
		this.FighterState.FinalPos = this.transform.position;
		if(SceneManager.GetActiveScene().isLoaded)
		{
			SceneLoadPlayer(SceneManager.GetActiveScene(), LoadSceneMode.Single);
		}
	}

	protected override void FixedUpdate()
	{
		if(!sceneIsReady){return;}
		if(isLocalPlayer)
		{
			inputBuffer.Enqueue(FighterState);
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
				FighterState = inputBuffer.Dequeue();
			}
		}

		FixedUpdateProcessInput();
		FixedUpdatePhysics(); 		// Change this to take time.deltatime as an input so you can implement time dilation.
		FixedUpdateLogic();			// Deals with variables such as life and zon power
		FixedUpdateAnimation();		// Animates the character based on movement and input.
		FighterState.RightClick = false;
		FighterState.LeftClick = false;
		FighterState.ZonKey = false;
		FighterState.DisperseKey = false;
		FighterState.JumpKey = false;
	}

	protected override void Update()
	{
		if(!sceneIsReady){return;}
		if(!isLocalPlayer){return;}
		UpdateInput();

		if(o_Speedometer != null)
		{
			o_Speedometer.text = ""+Math.Round(FighterState.Vel.magnitude,0);
		}
		if(o_ZonCounter != null)
		{
			o_ZonCounter.text = ""+g_ZonLevel;
		}
	}

	protected override void LateUpdate()
	{
		if(!sceneIsReady){return;}
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

	[Command]protected void CmdThrowPunch(Vector2 aimDirection)
	{
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

		newAirPunch.GetComponentInChildren<AirPunch>().aimDirection = aimDirection;
		newAirPunch.GetComponentInChildren<AirPunch>().punchThrower = this;
	}

	[Command(channel=1)]protected void CmdSendInput(FighterState[] theInput)
	{
		RpcSendInput(theInput);
	}

	[ClientRpc(channel=1)]protected void RpcSendInput(FighterState[]  theInput)
	{
		if(!isLocalPlayer)
		{
			foreach (FighterState i in theInput)
			{
				this.inputBuffer.Enqueue(i);
			}
		}
	}

	protected override void FixedUpdateProcessInput()
	{
		m_Impact = false;
		m_Landing = false;
		m_Kneeling = false;
		g_ZonStance = -1;

		if(FighterState.RightClick&&(FighterState.DevMode))
		{
			GameObject newAirBurst = (GameObject)Instantiate(p_AirBurstPrefab, FighterState.MouseWorldPos, Quaternion.identity);
			newAirBurst.GetComponentInChildren<AirBurst>().Create(true, 30+70, 0.2f, 250); 

			FighterState.RightClick = false;
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

		if(FighterState.LeftClick&&(FighterState.DevMode||d_ClickToKnockFighter))
		{
			FighterState.Vel += FighterState.PlayerMouseVector*10;
			//print("Knocking the fighter.");
			FighterState.LeftClick = false;
		}	

		if(i_DevKey1)
		{
			if(FighterState.DevMode)
			{
				FighterState.DevMode = false;
			}
			else
			{
				o_ProximityLiner.DetectAllFighters();
				FighterState.DevMode = true;
			}
			i_DevKey1 = false;
		}


		if(i_DevKey2)
		{
			this.Respawn();
			i_DevKey2 = false;
		}

		if(i_DevKey3)
		{
			if(autoRunLeft==false)
			{
				autoRunLeft = true;
			}
			else
			{
				autoRunLeft = false;
			}
			i_DevKey3 = false;
		}

		if(i_DevKey4)
		{
			FighterState.CurHealth -= 10;
			i_DevKey4 = false;
		}

		if(IsDisabled())
		{
			FighterState.RightClick = false;
			FighterState.LeftClick = false;
			FighterState.LeftClickRelease = false;
			FighterState.LeftClickHold = false;
			FighterState.UpKey = false;
			FighterState.LeftKey = false;
			FighterState.DownKey = false;
			FighterState.RightKey = false;
			FighterState.JumpKey = false;
			FighterState.ZonKey = false;
			FighterState.DisperseKey = false;
		}

		//#################################################################################
		//### ALL INPUT AFTER THIS POINT IS DISABLED WHEN THE FIGHTER IS INCAPACITATED. ###
		//#################################################################################

		FighterState.PlayerMouseVector = FighterState.MouseWorldPos-Vec2(this.transform.position);
		if(!(FighterState.LeftKey||FighterState.RightKey) || (FighterState.LeftKey && FighterState.RightKey))
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
		else if(FighterState.LeftKey)
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
		if(FighterState.DownKey&&m_Grounded)
		{
			m_Kneeling = true;
			CtrlH = 0;
			g_ZonStance = 0; // Kneeling stance.
		}
		else
		{
			g_ZonJumpCharge=0;
		}
			
		if(FighterState.JumpKey&&(m_Grounded||m_Ceilinged||m_LeftWalled||m_RightWalled))
		{
			FighterState.JumpKey = false;
			if(m_Kneeling)
			{
				ZonJump(FighterState.PlayerMouseVector.normalized);
			}
			else
			{
				Jump(CtrlH);
			}
		}
			
		if(FighterState.LeftClickRelease&&!(FighterState.DevMode||d_ClickToKnockFighter)&&!m_Kneeling)
		{
			if(!(FighterState.LeftKey&&(FighterState.PlayerMouseVector.normalized.x>0))&&!(FighterState.RightKey&&(FighterState.PlayerMouseVector.normalized.x<0))) // If trying to run opposite your punch direction, do not punch.
			{
				ThrowPunch(FighterState.PlayerMouseVector.normalized);
			}
			//print("Leftclick detected");
			//g_VelocityPunchExpended = false;
			FighterState.LeftClickRelease = false;
		}	

		if(FighterState.LeftClickHold)
		{
			i_LeftClickHoldDuration += Time.fixedDeltaTime;
			if(g_VelocityPunching)
			{
				if(FighterState.Vel.magnitude <= 70||g_VelocityPunchExpended)
				{
					g_VelocityPunching = false;
					o_VelocityPunch.inUse = false;
					g_VelocityPunchExpended = true;
				}
			}
			else
			{
				if((FighterState.Vel.magnitude > 70)&&(!g_VelocityPunchExpended)&&(i_LeftClickHoldDuration>=0.5f)) //If going fast enough and holding click for long enough.
				{
					g_VelocityPunching = true;
					o_VelocityPunch.inUse = true;
				}
			}
		}
		else
		{
			i_LeftClickHoldDuration = 0;
			g_VelocityPunching = false;
			o_VelocityPunch.inUse = false;
			g_VelocityPunchExpended = false;
		}
			
		if(FighterState.DisperseKey)
		{
			ZonPulse();
			FighterState.DisperseKey = false;
		}
	}

	protected override void UpdateInput()
	{
		if(!isLocalPlayer){return;}

		//
		// Individual keydown presses
		//
		if(Input.GetMouseButtonDown(0))
		{
			FighterState.LeftClick = true;
		}
		if(Input.GetMouseButtonDown(1))
		{
			FighterState.RightClick = true;
		}
		if(Input.GetButtonDown("Spooling"))
		{
			FighterState.ZonKey = true;				
		}
		if(Input.GetButtonDown("Disperse"))
		{
			FighterState.DisperseKey = true;				
		}
		if(Input.GetButtonDown("Jump"))
		{
			FighterState.JumpKey = true;				
		}
		if(Input.GetButtonDown("F1"))
		{
			i_DevKey1 = true;				
		}
		if(Input.GetButtonDown("F2"))
		{
			i_DevKey2  = true;				
		}
		if(Input.GetButtonDown("F3"))
		{
			i_DevKey3  = true;				
		}
		if(Input.GetButtonDown("F4"))
		{
			i_DevKey4  = true;				
		}

		//
		// Key-Up Unpresses
		//
		FighterState.LeftClickRelease = Input.GetMouseButtonUp(0);
			
		//
		// Key Hold-Downs
		//
		FighterState.LeftKey = Input.GetButton("Left");
		FighterState.RightKey = Input.GetButton("Right");
		FighterState.UpKey = Input.GetButton("Up");
		FighterState.DownKey = Input.GetButton("Down");
		FighterState.LeftClickHold = Input.GetMouseButton(0);

		// Mouse position in world space
		Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		FighterState.MouseWorldPos = Vec2(mousePoint);

		// Automatic input options.
		if(autoJump)
		{
			FighterState.JumpKey = true;
		}
	}

	protected override void FixedUpdateAnimation()
	{
		v_FighterGlow = g_ZonLevel;
		if (v_FighterGlow > 7){v_FighterGlow = 7;}

		if(v_FighterGlow>0)
		{
			o_TempLight.color = new Color(1,1,0,1);
			o_TempLight.intensity = (v_FighterGlow)+(UnityEngine.Random.Range(0,1f));
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
			if(FighterState.Vel.x > 0 && m_Spd >= v_ReversingSlideT)
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
			if(FighterState.Vel.x < 0 && m_Spd >= v_ReversingSlideT)
			{
				o_Anim.SetBool("Crouch", true);
			}
			else
			{
				o_Anim.SetBool("Crouch", false);
			}
		}

		if(m_Kneeling||(g_CurFallStun>0))
		{
			o_Anim.SetBool("Crouch", true);

			if((FighterState.MouseWorldPos.x-this.transform.position.x)<0)
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

		o_Anim.SetFloat("Speed", FighterState.Vel.magnitude);

		if(FighterState.Vel.magnitude >= m_TractionChangeT )
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

		if(FighterState.Vel.magnitude > 20.0f)
		{
			multiplier = ((FighterState.Vel.magnitude - 20) / 20)+1;
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

		if(isLocalPlayer)
		{
			o_Healthbar.SetCurHealth(FighterState.CurHealth);
		}
	}

	protected void CameraControl()
	{
		#region zoom
		if(!o_MainCamera){return;}
		v_CameraZoom = Mathf.Lerp(v_CameraZoom, FighterState.Vel.magnitude, 0.1f);
		//v_CameraZoom = FighterState.Vel.magnitude;
		float zoomChange = 0;
		if((0.15f*v_CameraZoom)>=5f)
		{
			zoomChange = (0.15f*v_CameraZoom)-5f;
		}
//		if((v_CameraZoom)>=5f)
//		{
//			zoomChange = (v_CameraZoom)-5f;
//		}
		//o_MainCamera.orthographicSize = 5f+(0.15f*v_CameraZoom);
		if(8f+zoomChange >= 50f)
		{
			o_MainCamera.orthographicSize = 50f;
		}
		else
		{
			o_MainCamera.orthographicSize = 8f+zoomChange;
		}

		//o_MainCamera.orthographicSize = 100f; // REMOVE THIS WHEN NOT DEBUGGING.

		#endregion
	}
		
	protected void ZonJump(Vector2 jumpNormal)
	{
		g_ZonJumpCharge = g_ZonLevel;
		if(g_ZonLevel > 0)
		{
			g_ZonLevel--;
		}
		FighterState.Vel = FighterState.Vel+(jumpNormal*(m_ZonJumpForceBase+(m_ZonJumpForcePerCharge*g_ZonJumpCharge)));
		g_ZonJumpCharge = 0;		
		o_FighterAudio.JumpSound();
	}

	protected void ZonPulse()
	{
		if(g_ZonLevel <= 0)
		{
			return;
		}

		g_ZonLevel--;
		o_ProximityLiner.ClearAllFighters();
		GameObject newZonPulse = (GameObject)Instantiate(p_ZonPulse, this.transform.position, Quaternion.identity);
		newZonPulse.GetComponentInChildren<ZonPulse>().originPlayer = this;
		newZonPulse.GetComponentInChildren<ZonPulse>().pulseRange = 150+(g_ZonLevel*50);
		//o_ProximityLiner.outerRange = 100+(g_ZonLevel*25);
		o_FighterAudio.ZonPulseSound();
	}
		

	#endregion
	//###################################################################################################################################
	// PUBLIC FUNCTIONS
	//###################################################################################################################################
	#region PUBLIC FUNCTIONS

	public Vector2 GetPosition()
	{
		return FighterState.FinalPos;
	}

	public void PulseHit(FighterChar theFighter)
	{
		o_ProximityLiner.AddFighter(theFighter);
	}

	#endregion
}