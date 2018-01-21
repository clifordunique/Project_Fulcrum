using UnityEngine.UI;
using System;
using UnityEngine;

using EZCameraShake;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
	[SerializeField][ReadOnlyAttribute] private Text o_Speedometer;      			// Reference to the speed indicator (dev tool).
	[SerializeField][ReadOnlyAttribute] private Reporter o_Reporter;      			// Reference to the console (dev tool).
	[SerializeField][ReadOnlyAttribute] private Text o_ZonCounter;      			// Reference to the level of zon power (dev tool).
	[SerializeField][ReadOnlyAttribute] private Camera o_MainCamera;				// Reference to the main camera.
	[SerializeField][ReadOnlyAttribute] private Transform o_MainCameraTransform;	// Reference to the main camera's parent's transform, used to move it.
	[SerializeField][ReadOnlyAttribute] private CameraShaker o_CamShaker;			// Reference to the main camera's shaking controller.
	[SerializeField] public Spooler o_Spooler;					// Reference to the character's spooler object, which handles power charging gameplay.
	[SerializeField][ReadOnlyAttribute] public Healthbar o_Healthbar;				// Reference to the Healthbar UI element.
	[SerializeField][ReadOnlyAttribute] private ProximityLiner o_ProximityLiner;	// Reference to the proximity line handler object. This handles the little lines indicating the direction of offscreen enemies.
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
	[SerializeField][ReadOnlyAttribute]public bool i_DevkeyTilde;
	[SerializeField][ReadOnlyAttribute]public bool i_DevKey1;
	[SerializeField][ReadOnlyAttribute]public bool i_DevKey2;
	[SerializeField][ReadOnlyAttribute]public bool i_DevKey3;
	[SerializeField][ReadOnlyAttribute]public bool i_DevKey4;
	[SerializeField][ReadOnlyAttribute]public bool i_DevKey5;
	[SerializeField][ReadOnlyAttribute]public bool i_DevKey6;
	[SerializeField][ReadOnlyAttribute]public bool i_DevKey7;
	[SerializeField][ReadOnlyAttribute]public bool i_DevKey8;
	[SerializeField][ReadOnlyAttribute]public bool i_DevKey9;
	[SerializeField][ReadOnlyAttribute]public bool i_DevKey10;
	[SerializeField][ReadOnlyAttribute]public bool i_DevKey11;
	[SerializeField][ReadOnlyAttribute]public bool i_DevKey12;
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
		//FighterState.DevMode = true;
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
		o_MainCameraTransform = o_MainCamera.transform.parent.transform;
//		if(v_CameraMode==0)
//		{
//			o_MainCameraTransform.position = new Vector3(this.transform.position.x, this.transform.position.y, -10f);
//			o_MainCamera.transform.parent.SetParent(this.transform);
//		}
		o_Speedometer = GameObject.Find("Speedometer").GetComponent<Text>();
		o_ZonCounter = GameObject.Find("Zon Counter").GetComponent<Text>();
		o_Healthbar = GameObject.Find("Healthbar").GetComponent<Healthbar>();

	}

	protected void Start()
	{
		o_ProximityLiner = this.GetComponent<ProximityLiner>();
		o_Reporter = FindObjectOfType<Reporter>();
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

		if(k_IsKinematic)
		{
			FixedUpdateKinematic();	//If the player is in kinematic mode, physics are disabled while animations are played.
		}
		else
		{
			FixedUpdatePhysics(); // Change this to take time.deltatime as an input so you can implement time dilation.
		}

		FixedUpdateLogic();			// Deals with variables such as life and zon power
		FixedUpdateAnimation();		// Animates the character based on movement and input.
		FighterState.RightClick = false;
		FighterState.LeftClick = false;
		FighterState.ZonKey = false;
		FighterState.DisperseKey = false;
		FighterState.JumpKey = false;

		if(isLocalPlayer)
		{
			if(v_CameraMode==0)
			{
				CameraControlTypeA(); //Player-locked velocity size-reactive camera
			}
			else
			{
				CameraControlTypeB(); //Mouse Directed Camera
			}
		}
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
		g_Stance = 0;
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

		// Automatic input options.
		if(autoJump)
		{
			FighterState.JumpKey = true;
		}
		if(autoLeftClick)
		{
			FighterState.LeftClick = true;
			FighterState.LeftClickHold = true;
		}
		if(i_DevkeyTilde)
		{
			o_Reporter.doShow();
			i_DevkeyTilde = false;
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
			if(autoPressLeft==false)
			{
				autoPressLeft = true;
			}
			else
			{
				autoPressLeft = false;
			}
			i_DevKey3 = false;
		}

		if(i_DevKey4)
		{
			FighterState.CurHealth -= 10;
			g_ZonLevel = 8;
			i_DevKey4 = false;
		}
		if(i_DevKey5)
		{
			v_CameraMode++;
			if(v_CameraMode>1)
			{
				v_CameraMode = 0;
			}
			i_DevKey5 = false;
		}
		if(i_DevKey6)
		{
			i_DevKey6 = false;
		}
		if(i_DevKey7)
		{
			i_DevKey7 = false;
		}
		if(i_DevKey8)
		{
			i_DevKey8 = false;
		}
		if(i_DevKey9)
		{
			i_DevKey9 = false;
		}
		if(i_DevKey10)
		{
			i_DevKey10 = false;
		}
		if(i_DevKey11)
		{
			i_DevKey11 = false;
		}
		if(i_DevKey12)
		{
			i_DevKey12 = false;
			#if UNITY_EDITOR
			EditorApplication.isPaused = true;
			#endif
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
		//Horizontal button pressing
		FighterState.PlayerMouseVector = FighterState.MouseWorldPos-Vec2(this.transform.position);
		if((FighterState.LeftKey && FighterState.RightKey) || !(FighterState.LeftKey||FighterState.RightKey))
		{
			//print("BOTH OR NEITHER");
			if(!(autoPressLeft||autoPressRight))
			{
				CtrlH = 0;
			}
			else if(autoPressLeft)
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

		//Vertical button pressing
		if((FighterState.DownKey && FighterState.UpKey) || !(FighterState.UpKey||FighterState.DownKey))
		{
			//print("BOTH OR NEITHER");
			if(!(autoPressDown||autoPressUp))
			{
				CtrlV = 0;
			}
			else if(autoPressDown)
			{
				CtrlV = -1;
			}
			else
			{
				CtrlV = 1;
			}
		}
		else if(FighterState.DownKey)
		{
			//print("LEFT");
			CtrlV = -1;
		}
		else
		{
			//print("RIGHT");
			CtrlV = 1;
		}

		if(CtrlV<0)
		{
			facingDirectionV = -1; //true means up (the direction), false means down.
		}
		else if(CtrlV>0)
		{
			facingDirectionV = 1; //true means up (the direction), false means down.
		}
		else
		{
			facingDirectionV = 0;	
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
			
		//if(FighterState.JumpKey&&(m_Grounded||m_Ceilinged||m_LeftWalled||m_RightWalled))
		if(FighterState.JumpKey)
		{
			FighterState.JumpKey = false;
			if(m_Kneeling)
			{
				ZonJump(FighterState.PlayerMouseVector.normalized);
			}
			else if(m_JumpBufferG>0 || m_JumpBufferC>0 || m_JumpBufferL>0 || m_JumpBufferR>0)
			{
				Jump(CtrlH);
			}
			else
			{
				StrandJumpTypeA(CtrlH, CtrlV);
			}
		}
			
		if(FighterState.LeftClickRelease&&!(FighterState.DevMode||d_ClickToKnockFighter)&&!m_Kneeling)
		{
			if(!(FighterState.LeftKey&&(FighterState.PlayerMouseVector.normalized.x>0))&&!(FighterState.RightKey&&(FighterState.PlayerMouseVector.normalized.x<0))) // If trying to run opposite your punch direction, do not punch.
			{
				ThrowPunch(FighterState.PlayerMouseVector.normalized);
			}
			//print("Leftclick detected");
			g_VelocityPunchExpended = false;
			FighterState.LeftClickRelease = false;
		}	

		if(FighterState.LeftClickHold)
		{
			i_LeftClickHoldDuration += Time.fixedDeltaTime;
			g_Stance = 1;

			if(i_LeftClickHoldDuration>=g_VelocityPunchChargeTime)
			{
				g_VelocityPunching = true;
				o_VelocityPunch.inUse = true;
			}

//			if(g_VelocityPunching)
//			{
//				if(FighterState.Vel.magnitude <= 70||g_VelocityPunchExpended)
//				{
//					g_VelocityPunching = false;
//					o_VelocityPunch.inUse = false;
//					g_VelocityPunchExpended = true;
//				}
//			}
//			else
//			{
//				if((FighterState.Vel.magnitude > 70)&&(!g_VelocityPunchExpended)&&(i_LeftClickHoldDuration>=0.5f)) //If going fast enough and holding click for long enough.
//				{
//					g_VelocityPunching = true;
//					o_VelocityPunch.inUse = true;
//				}
//			}
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
		if(Input.GetButtonDown("Tilde"))
		{
			i_DevkeyTilde = true;				
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
		if(Input.GetButtonDown("F5"))
		{
			i_DevKey5  = true;				
		}
		if(Input.GetButtonDown("F6"))
		{
			i_DevKey6  = true;				
		}
		if(Input.GetButtonDown("F7"))
		{
			i_DevKey7  = true;				
		}
		if(Input.GetButtonDown("F8"))
		{
			i_DevKey8  = true;				
		}
		if(Input.GetButtonDown("F9"))
		{
			i_DevKey9  = true;				
		}
		if(Input.GetButtonDown("F10"))
		{
			i_DevKey10  = true;				
		}
		if(Input.GetButtonDown("F11"))
		{
			i_DevKey11  = true;				
		}
		if(Input.GetButtonDown("F12"))
		{
			i_DevKey12 = true;				
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

	}

	protected override void FixedUpdateAnimation()
	{
		v_FighterGlow = g_ZonLevel;
		if (v_FighterGlow > 7){v_FighterGlow = 7;}

		if(v_FighterGlow>0)
		{
			//o_TempLight.color = new Color(1,1,0,1);
			o_SpriteRenderer.color = new Color(1,1,(1f-(v_FighterGlow/7f)),1);
			//o_TempLight.intensity = (v_FighterGlow)+(UnityEngine.Random.Range(0,1f));
		}
		else
		{
			o_SpriteRenderer.color = Color.white;
			//o_TempLight.color = new Color(1,1,1,1);
			//o_TempLight.intensity = 2;
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
			o_Anim.SetBool("IsFacingRight", false);
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
			o_Anim.SetBool("IsFacingRight", true);
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
		o_Anim.SetFloat("hSpeed", FighterState.Vel.x);
		o_Anim.SetFloat("vSpeed", FighterState.Vel.y);
		o_Anim.SetInteger("Stance", g_Stance);


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

	protected void CameraControlTypeA() //CCTA
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
		o_MainCameraTransform.position = new Vector3(this.transform.position.x, this.transform.position.y, -10f);
		//o_MainCamera.orthographicSize = 100f; // REMOVE THIS WHEN NOT DEBUGGING.

		#endregion
	}

	protected void CameraControlTypeB() //CCTB
	{
		if(!o_MainCamera){return;}
		v_CameraZoom = Mathf.Lerp(v_CameraZoom, FighterState.Vel.magnitude, 0.1f);
		//v_CameraZoom = 50f;
		float zoomChange = 0;
		if((0.15f*v_CameraZoom)>=5f)
		{
			zoomChange = (0.15f*v_CameraZoom)-5f;
		}
		if(8f+zoomChange >= 50f)
		{
			o_MainCamera.orthographicSize = 50f;
		}
		else
		{
			o_MainCamera.orthographicSize = 8f+zoomChange;
		}

		float camAverageX = (this.transform.position.x+this.transform.position.x+FighterState.MouseWorldPos.x)/3;
		float camAverageY = (this.transform.position.y+this.transform.position.y+FighterState.MouseWorldPos.y)/3;


		Vector3 topRightEdge= new Vector3((1+v_CameraXLeashM)/2, (1+v_CameraYLeashM)/2, 0f);
		Vector3 theMiddle 	= new Vector3(0.5f, 0.5f, 0f);
		topRightEdge = o_MainCamera.ViewportToWorldPoint(topRightEdge);
		theMiddle = o_MainCamera.ViewportToWorldPoint(theMiddle);
		float xDistanceToEdge = topRightEdge.x-theMiddle.x;
		float yDistanceToEdge = topRightEdge.y-theMiddle.y;

		Vector3 topRightMax = new Vector3((1+v_CameraXLeashLim)/2, (1+v_CameraXLeashLim)/2, 0f);
		topRightMax = o_MainCamera.ViewportToWorldPoint(topRightMax);
		float xDistanceToMax = topRightMax.x-theMiddle.x;
		float yDistanceToMax = topRightMax.y-theMiddle.y;

		//print("botLeftEdge: "+botLeftEdge);
		//print("topRightEdge: "+topRightEdge);
		//print("Player: "+this.transform.position);
		//print("theMiddle: "+theMiddle);
		//print("player: "+this.transform.position.x+"\n lefted: "+botLeftEdge.x);

		if(camAverageX-xDistanceToEdge>this.transform.position.x) //If the edge of the proposed camera position is beyond the player, snap it back
		{
			//print("Too far left! player: "+this.transform.position.x+", edge: "+botLeftEdge.x);
			camAverageX = this.transform.position.x+(xDistanceToEdge); //If it's outside of the leashzone, lock it to the edge.
		}
		if(camAverageX+xDistanceToEdge<this.transform.position.x)
		{
			//print("Too far Right! player: "+this.transform.position.x+", edge: "+topRightEdge.x);
			camAverageX = this.transform.position.x-(xDistanceToEdge);
		}

		if(camAverageY-yDistanceToEdge>this.transform.position.y) //If the edge of the proposed camera position is beyond the player, snap it back
		{
			//print("Too far down!");
			camAverageY = this.transform.position.y+(yDistanceToEdge); //If it's outside of the leashzone, lock it to the edge.
		}
		if(camAverageY+yDistanceToEdge<this.transform.position.y)
		{
			//print("Too far up! player: "+this.transform.position.y+", edge: "+topRightEdge.y);
			camAverageY = this.transform.position.y-(yDistanceToEdge);
		}

		Vector3 camGoalLocation = new Vector3(camAverageX, camAverageY, -10f);

		o_MainCameraTransform.position = Vector3.Lerp(o_MainCameraTransform.position, camGoalLocation, 0.1f); // CAMERA LERP TO POSITION. USUAL MOVEMENT METHOD.

		//
		// The following block of code is for when the player hits the maximum bounds. The camera will instantly snap to the edge and won't go any further. Does not use lerp.
		//

		if(o_MainCameraTransform.position.x-xDistanceToMax>this.transform.position.x) //If the edge of the proposed camera position is beyond the player, snap it back
		{
			//	print("Too far left! player: "+this.transform.position.x+", edge: "+botLeftEdge.x);
			o_MainCameraTransform.position = new Vector3(this.transform.position.x+(xDistanceToMax),o_MainCameraTransform.position.y, -10f); // CAMERA LOCK X VALUE TO KEEP PLAYER IN FRAME
		}
		if(o_MainCameraTransform.position.x+xDistanceToMax<this.transform.position.x)
		{
			//print("Too far Right! player: "+this.transform.position.x+", edge: "+topRightEdge.x);
			o_MainCameraTransform.position = new Vector3(this.transform.position.x-(xDistanceToMax),o_MainCameraTransform.position.y, -10f); // CAMERA LOCK X VALUE TO KEEP PLAYER IN FRAME
		}

		if(o_MainCameraTransform.position.y-yDistanceToMax>this.transform.position.y) //If the edge of the proposed camera position is beyond the player, snap it back
		{
			//print("Too far down!");
			o_MainCameraTransform.position = new Vector3(o_MainCameraTransform.position.x,this.transform.position.y+(yDistanceToMax), -10f); // CAMERA LOCK Y VALUE TO KEEP PLAYER IN FRAME
		}
		if(o_MainCameraTransform.position.y+yDistanceToMax<this.transform.position.y)
		{
			//print("Too far up! player: "+this.transform.position.y+", edge: "+topRightEdge.y);
			o_MainCameraTransform.position = new Vector3(o_MainCameraTransform.position.x,this.transform.position.y-(yDistanceToMax), -10f); // CAMERA LOCK Y VALUE TO KEEP PLAYER IN FRAME
		}

		//o_MainCamera.orthographicSize = 20f; // REMOVE THIS WHEN NOT DEBUGGING.

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