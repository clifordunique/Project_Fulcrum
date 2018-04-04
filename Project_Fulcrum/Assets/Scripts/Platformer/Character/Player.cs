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
	[SerializeField][ReadOnlyAttribute] private ShoeTooltip o_ShoeTooltip;      			// Reference to the speed indicator (dev tool).
	[SerializeField][ReadOnlyAttribute] private Text o_Speedometer;      			// Reference to the speed indicator (dev tool).
	[SerializeField][ReadOnlyAttribute] private Reporter o_Reporter;      			// Reference to the console (dev tool).
	[SerializeField][ReadOnlyAttribute] private Text o_EnergyCounter;      			// Reference to the level of zon power (dev tool).
	[SerializeField][ReadOnlyAttribute] private Camera o_MainCamera;				// Reference to the main camera.
	[SerializeField][ReadOnlyAttribute] private Transform o_MainCameraTransform;	// Reference to the main camera's parent's transform, used to move it.
	//[SerializeField][ReadOnlyAttribute] private CameraShaker o_CamShaker;			// Reference to the main camera's shaking controller.
	[SerializeField][ReadOnlyAttribute] public Spooler o_Spooler;					// Reference to the character's spooler component, which handles power charging gameplay.
	[SerializeField][ReadOnlyAttribute] public Healthbar o_Healthbar;				// Reference to the Healthbar UI element.
	[SerializeField][ReadOnlyAttribute] private ProximityLiner o_ProximityLiner;	// Reference to the proximity line handler object. This handles the little lines indicating the direction of offscreen enemies.
	[SerializeField] private GameObject p_TestObject;								// Reference to any prefab you wish, allowing you to spawn that prefab by pressing F9.
	#endregion
	//############################################################################################################################################################################################################
	// PHYSICS&RAYCASTING
	//###########################################################################################################################################################################
	#region PHYSICS&RAYCASTING
	#endregion
	//##########################################################################################################################################################################
	// PLAYER INPUT VARIABLES
	//###########################################################################################################################################################################
	#region PLAYERINPUT
	[Header("Player Input:")]
	[SerializeField] public float i_DoubleTapDelayTime; // How short the time between presses must be to count as a doubletap.
	public int inputBufferSize = 2;
	[SerializeField] public Queue<FighterState> inputBuffer;
	#endregion
	//############################################################################################################################################################################################################
	// DEBUGGING VARIABLES
	//##########################################################################################################################################################################
	#region DEBUGGING
	[SerializeField]private float d_LastFrameSpeed;
	[SerializeField]private float d_DeltaV;
	#endregion
	//############################################################################################################################################################################################################
	// VISUAL&SOUND VARIABLES
	//###########################################################################################################################################################################
	#region VISUALS&SOUND
	[SerializeField] private Vector3 v_CamWhiplashAmount;
	[SerializeField] private Vector3 v_CamWhiplashRecovery;
	[SerializeField] private float v_CamWhiplashM = 1;
	[SerializeField] public float v_CameraZoomLevel = 12;
	[SerializeField][ReadOnlyAttribute] public float v_CameraFinalSize;
	[SerializeField] private float v_CameraScrollZoom = 12;
	[SerializeField] private float v_CameraZoomLerp = 12;	
	[SerializeField] private float v_SuperJumpStanceTime = 0;				// How long the player has been crouched with energy, ready to super jump.
	[SerializeField] private float v_MaxSuperJumpStanceTime = 0.33f;			 // Max crouch time before the camera begins to pan out, letting the player see their surroundings further away so they can line jumps up easier.


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
		print("Executing post-scenelaunch player code!");
		o_MainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
		v_DefaultCameraMode = 3;
		o_MainCameraTransform = o_MainCamera.transform.parent.transform;
		o_MainCameraTransform.SetParent(this.transform);
		o_MainCameraTransform.localPosition = new Vector3(0, 0, -10f);
		o_Speedometer = GameObject.Find("Speedometer").GetComponent<Text>();
		o_EnergyCounter = GameObject.Find("Energy Counter").GetComponent<Text>();
		o_Healthbar = GameObject.Find("Healthbar").GetComponent<Healthbar>();
		o_Spooler = this.gameObject.GetComponent<Spooler>();
		o_ShoeTooltip = GameObject.Find("ShoeTooltip").GetComponent<ShoeTooltip>();
		o_ShoeTooltip.SetFighter(this);
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

		if(!isLocalPlayer)
		{
			if(inputBuffer.Count > 0)
			{
				FighterState = inputBuffer.Dequeue();
			}
		}
		d_TickCounter++;
		d_TickCounter = (d_TickCounter > 60) ? 0 : d_TickCounter; // Rolls back to zero when hitting 60
		FixedUpdateProcessInput();
		UpdateCurrentNavSurf();
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
		FixedUpdateWwiseAudio();

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
	}

	protected override void Update()
	{
		//
		// Any player code.
		//
		UpdateAnimation();
		//
		// Local player code.
		//
		if(!sceneIsReady){return;}
		if(!isLocalPlayer){return;}
		UpdatePlayerInput();
		UpdatePlayerAnimation();
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


	public override void EquipShoe(Shoe shoe)
	{
		if(shoe==null)
		{
			shoe = Instantiate(o_ItemHandler.shoes[0], this.transform.position, Quaternion.identity).GetComponent<Shoe>();
		}

		UnequipShoe(); // Drop old shoes.

		///
		/// Movestat code
		///

		this.m_MinSpeed = shoe.m_MinSpeed;					
		this.m_MaxRunSpeed = shoe.m_MaxRunSpeed;				
		this.m_StartupAccelRate = shoe.m_StartupAccelRate;  			

		this.m_VJumpForce = shoe.m_VJumpForce;               
		this.m_HJumpForce = shoe.m_HJumpForce;  				
		this.m_WallVJumpForce = shoe.m_WallVJumpForce;           
		this.m_WallHJumpForce = shoe.m_WallHJumpForce;  			
		this.m_ZonJumpForcePerCharge = shoe.m_ZonJumpForcePerCharge; 	
		this.m_ZonJumpForceBase = shoe.m_ZonJumpForceBase; 		

		this.m_TractionChangeT = shoe.m_TractionChangeT;			
		this.m_WallTractionT = shoe.m_WallTractionT;			
		this.m_LinearStopRate = shoe.m_LinearStopRate; 			
		this.m_LinearSlideRate = shoe.m_LinearSlideRate;			
		this.m_LinearOverSpeedRate = shoe.m_LinearOverSpeedRate;		
		this.m_LinearAccelRate = shoe.m_LinearAccelRate;			
		this.m_ImpactDecelMinAngle = shoe.m_ImpactDecelMinAngle;
		this.m_ImpactDecelMaxAngle = shoe.m_ImpactDecelMaxAngle;
		this.m_TractionLossMinAngle = shoe.m_TractionLossMinAngle; 
		this.m_TractionLossMaxAngle = shoe.m_TractionLossMaxAngle;
		this.m_SlippingAcceleration = shoe.m_SlippingAcceleration;  	
		this.m_SurfaceClingTime = shoe.m_SurfaceClingTime;
		this.m_ClingReqGForce = shoe.m_ClingReqGForce;

		this.m_SlamT = shoe.m_SlamT;					
		this.m_CraterT = shoe.m_CraterT; 					
		this.m_GuardSlamT = shoe.m_GuardSlamT; 				
		this.m_GuardCraterT = shoe.m_GuardCraterT;				

		this.m_StrandJumpSpeedLossM = shoe.m_StrandJumpSpeedLossM;
		this.m_WidestStrandJumpAngle = shoe.m_WidestStrandJumpAngle;

		///
		/// Non movestat code
		///
		o_EquippedShoe = shoe;
		shoe.PickedUpBy(this);

		AkSoundEngine.SetRTPCValue("ShoeType_ID", shoe.soundType);

		if(shoe.shoeID!=0)
		{
			o_FighterAudio.EquipSound();
		}
		if(isLocalPlayer)
		{
			o_ShoeTooltip.SetShoe(shoe);
		}
	}

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

	protected override void FixedUpdateProcessInput() // FUPI
	{
		m_WorldImpact = false; //Placeholder??
		FighterState.Stance = 0;
		m_Kneeling = false;

		if(FighterState.RightClickPress&&(FighterState.DevMode))
		{
			GameObject newAirBurst = (GameObject)Instantiate(p_AirBurstPrefab, FighterState.MouseWorldPos, Quaternion.identity);
			newAirBurst.GetComponentInChildren<AirBurst>().Create(true, 30+70, 0.2f, 250); 

			FighterState.RightClickPress = false;
			float Magnitude = 2f;
			float Roughness = 10f;
			float FadeOutTime = 5f;
			float FadeInTime = 0f;
			Vector3 RotInfluence = new Vector3(1,1,1);
			Vector3 PosInfluence = new Vector3(0.15f,0.15f,0.15f);
			CameraShaker.Instance.ShakeOnce(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);
		}	

		if(FighterState.LeftClickHold&&(FighterState.DevMode||d_ClickToKnockFighter))
		{
			FighterState.Vel += FighterState.PlayerMouseVector*20*Time.fixedDeltaTime;
			//print("Knocking the fighter.");
		}	

		// Automatic input options.
		if(autoJump)
		{
			FighterState.JumpKeyPress = true;
		}
		if(autoLeftClick)
		{
			FighterState.LeftClickPress = true;
			FighterState.LeftClickHold = true;
		}
		if(FighterState.DevkeyTilde)
		{
			o_Reporter.doShow();
			FighterState.DevkeyTilde = false;
		}
		if(FighterState.DevKey1)
		{
			if(FighterState.DevMode)
			{
				FighterState.DevMode = false;
				o_NavMaster.setAllVisible = false;
				o_NavMaster.UpdateAllSurfaces();
			}
			else
			{
				o_ProximityLiner.DetectAllFighters();
				o_NavMaster.setAllVisible = true;
				o_NavMaster.UpdateAllSurfaces();
				FighterState.DevMode = true;
			}
			FighterState.DevKey1 = false;
		}


		if(FighterState.DevKey2)
		{
			this.Respawn();
			g_CurStun = 0;
			g_Stunned = false;
			g_Staggered = false;
			FighterState.DevKey2 = false;
		}

		if(FighterState.DevKey3)
		{
			if(autoPressLeft==false)
			{
				autoPressLeft = true;
			}
			else
			{
				autoPressLeft = false;
			}
			FighterState.DevKey3 = false;
		}

		if(FighterState.DevKey4)
		{
			FighterState.ZonLevel = 8;
			FighterState.DevKey4 = false;
		}
		if(FighterState.DevKey5)
		{
			v_DefaultCameraMode++;
			string[] cameraNames = {"A - LockedClose","B - AimGuided","C - AimGuided SuperJump","D - AimGuidedWhiplash","E - Stationary","F - SlowMo"};
			if(v_DefaultCameraMode==2)
			{
				v_DefaultCameraMode++;
			}
			if(v_DefaultCameraMode>5)
			{
				v_DefaultCameraMode = 0;
			}
			print("Camera mode "+cameraNames[v_DefaultCameraMode]);
			FighterState.DevKey5 = false;
		}
		if(FighterState.DevKey6)
		{
			TakeDamage(10);
			g_CurStun = 2f;
			g_Stunned = true;
			FighterState.DevKey6 = false;
		}
		if(FighterState.DevKey7)
		{
			float timeSpeed = o_TimeManager.GetTimeDilationM();

			if(timeSpeed<=0.1f)
			{
				o_TimeManager.TimeDilation(0.25f);
			}
			else if(timeSpeed<=0.25f)
			{
				o_TimeManager.TimeDilation(1);
			}
			else
			{
				o_TimeManager.TimeDilation(0.1f);
			}
			FighterState.DevKey7 = false;
		}
		if(FighterState.DevKey8)
		{
			v_Gender = (v_Gender) ? false : true;

			if(v_Gender)
			{
				AkSoundEngine.PostEvent("Set_Gender_Male", gameObject);
			}
			else
			{
				AkSoundEngine.PostEvent("Set_Gender_Female", gameObject);
			}

			FighterState.DevKey8 = false;
		}
		if(FighterState.DevKey9)
		{
			if(p_TestObject!=null)
			{
				GameObject newBoom = (GameObject)Instantiate(p_TestObject, this.transform.position, Quaternion.identity);
			}
			FighterState.DevKey9 = false;
		}
		if(FighterState.DevKey10)
		{
			if(n_AutoGenerateNavCon)
			{
				n_AutoGenerateNavCon = false;
				print("AutoGenNavCon Disabled");

			}
			else
			{
				n_AutoGenerateNavCon = true;
				print("AutoGenNavCon Enabled");
			}		
			FighterState.DevKey10 = false;
		}
		if(FighterState.DevKey11)
		{
			v_TriggerAtkHit = true;
			FighterState.DevKey11 = false;
		}
		if(FighterState.DevKey12)
		{
			FighterState.DevKey12 = false;
			#if UNITY_EDITOR
			EditorApplication.isPaused = true;
			#endif
		}

		if(IsDisabled())
		{
			FighterState.RightClickPress = false;
			FighterState.RightClickRelease = false;
			FighterState.RightClickHold = false;
			FighterState.LeftClickPress = false;
			FighterState.LeftClickRelease = false;
			FighterState.LeftClickHold = false;
			FighterState.ShiftKeyPress = false;
			FighterState.UpKeyHold = false;
			FighterState.LeftKeyHold = false;
			FighterState.DownKeyHold = false;
			FighterState.RightKeyHold = false;
			FighterState.JumpKeyPress = false;
			FighterState.ZonKeyPress = false;
			FighterState.DisperseKeyPress = false;
		}

		//#################################################################################
		//### ALL INPUT AFTER THIS POINT IS DISABLED WHEN THE FIGHTER IS INCAPACITATED. ###
		//#################################################################################

		if(FighterState.ZonKeyPress)
		{
			o_Spooler.LockRing();
		}

		if(FighterState.ShiftKeyPress)
		{
			o_Spooler.OpenSpooler();
		}

		if(FighterState.ShiftKeyRelease)
		{
			if(o_Spooler.r_Active)
			{
				o_Spooler.HideSpooler();
			}
			else
			{
				print("Not active, cannot close spooler.");
			}
		}
		//Horizontal button pressing
		FighterState.PlayerMouseVector = FighterState.MouseWorldPos-Vec2(this.transform.position);
		if((FighterState.LeftKeyHold && FighterState.RightKeyHold) || !(FighterState.LeftKeyHold||FighterState.RightKeyHold))
		{
			//print("BOTH OR NEITHER");
			if(!(autoPressLeft||autoPressRight)|| IsDisabled())
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
		else if(FighterState.LeftKeyHold)
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

		if(m_Airborne)
		{
			if(FighterState.Vel.x>0)
			{
				facingDirection = true;
			}
			else if(FighterState.Vel.x<0)
			{
				facingDirection = false;
			}
		}

		//Vertical button pressing
		if((FighterState.DownKeyHold && FighterState.UpKeyHold) || !(FighterState.UpKeyHold||FighterState.DownKeyHold))
		{
			//print("BOTH OR NEITHER");
			if(!(autoPressDown||autoPressUp)||IsDisabled())
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
		else if(FighterState.DownKeyHold)
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
			
		//if(FighterState.JumpKeyPress&&(m_Grounded||m_Ceilinged||m_LeftWalled||m_RightWalled))
		if(FighterState.JumpKeyPress)
		{
			FighterState.JumpKeyPress = false;
			if(CtrlV<0 && !m_Airborne) // If holding down and on a surface, allow zonjump.
			{
				if(FighterState.ZonLevel>0)
				{
					ZonJump(FighterState.PlayerMouseVector.normalized);
				}
			}
			else if(m_JumpBufferG>0 || m_JumpBufferC>0 || m_JumpBufferL>0 || m_JumpBufferR>0)
			{
				Jump(CtrlH);
			}
		}

		//
		// Strand Jump key double-taps
		//

		if(FighterState.LeftKeyDoubleTapReady){FighterState.LeftKeyDoubleTapDelay += Time.unscaledDeltaTime;} 	// If player pressed key, time how long since it was pressed.
		if(FighterState.RightKeyDoubleTapReady){FighterState.RightKeyDoubleTapDelay += Time.unscaledDeltaTime;} // If player pressed key, time how long since it was pressed.
		if(FighterState.UpKeyDoubleTapReady){FighterState.UpKeyDoubleTapDelay += Time.unscaledDeltaTime;} 		// If player pressed key, time how long since it was pressed.
		if(FighterState.DownKeyDoubleTapReady){FighterState.DownKeyDoubleTapDelay += Time.unscaledDeltaTime;}	// If player pressed key, time how long since it was pressed.
			
		int strandJumpHorz = 0;
		int strandJumpVert = 0;


		if(FighterState.LeftKeyDoubleTapDelay>i_DoubleTapDelayTime) // If over the time limit, next keypress won't count as a doubletap.
		{
			FighterState.LeftKeyDoubleTapReady = false;
		}
		if(FighterState.RightKeyDoubleTapDelay>i_DoubleTapDelayTime) // If over the time limit, next keypress won't count as a doubletap.
		{
			FighterState.RightKeyDoubleTapReady = false;
		}
		if(FighterState.UpKeyDoubleTapDelay>i_DoubleTapDelayTime) // If over the time limit, next keypress won't count as a doubletap.
		{
			FighterState.UpKeyDoubleTapReady = false;
		}
		if(FighterState.DownKeyDoubleTapDelay>i_DoubleTapDelayTime) // If over the time limit, next keypress won't count as a doubletap.
		{
			FighterState.DownKeyDoubleTapReady = false;
		}

		if(FighterState.LeftKeyPress) 
		{
			FighterState.RightKeyDoubleTapReady = false; // Other keys interrupt double taps.
			FighterState.UpKeyDoubleTapReady = false; // Other keys interrupt double taps.
			FighterState.DownKeyDoubleTapReady = false; // Other keys interrupt double taps.
			if(FighterState.LeftKeyDoubleTapReady) // If double tap, strand jump. If not, prime double tap.
			{
				FighterState.LeftKeyDoubleTapReady = false;
				strandJumpHorz = -1;
				//print("LeftKeyDoubleTapDelay "+FighterState.LeftKeyDoubleTapDelay);
			}
			else
			{
				FighterState.LeftKeyDoubleTapReady = true;
				FighterState.RightKeyDoubleTapReady = false;
			}
		}

		if(FighterState.RightKeyPress)
		{
			FighterState.LeftKeyDoubleTapReady = false; // Other keys interrupt double taps.
			FighterState.UpKeyDoubleTapReady = false; // Other keys interrupt double taps.
			FighterState.DownKeyDoubleTapReady = false; // Other keys interrupt double taps.
			if(FighterState.RightKeyDoubleTapReady) // If double tap, strand jump. If not, prime double tap.
			{
				FighterState.RightKeyDoubleTapReady = false;
				strandJumpHorz = 1;
				//print("RightKeyDoubleTapDelay "+FighterState.RightKeyDoubleTapDelay);
			}
			else
			{
				FighterState.RightKeyDoubleTapReady = true;
				FighterState.LeftKeyDoubleTapReady = false;
			}
		}

		if(FighterState.UpKeyPress)
		{
			FighterState.LeftKeyDoubleTapReady = false; // Other keys interrupt double taps.
			FighterState.RightKeyDoubleTapReady = false; // Other keys interrupt double taps.
			FighterState.DownKeyDoubleTapReady = false; // Other keys interrupt double taps.
			if(FighterState.UpKeyDoubleTapReady) // If double tap, strand jump. If not, prime double tap.
			{
				FighterState.UpKeyDoubleTapReady = false;
				strandJumpVert = 1;
				//print("UpKeyDoubleTapDelay "+FighterState.UpKeyDoubleTapDelay);
			}
			else
			{
				FighterState.UpKeyDoubleTapReady = true;
				FighterState.DownKeyDoubleTapReady = false;
			}
		}
		if(FighterState.DownKeyPress)
		{
			FighterState.LeftKeyDoubleTapReady = false; // Other keys interrupt double taps.
			FighterState.RightKeyDoubleTapReady = false; // Other keys interrupt double taps.
			FighterState.UpKeyDoubleTapReady = false; // Other keys interrupt double taps.
			if(FighterState.DownKeyDoubleTapReady) // If double tap, strand jump. If not, prime double tap.
			{
				FighterState.DownKeyDoubleTapReady = false;
				strandJumpVert = -1;
				//print("DownKeyDoubleTapDelay "+FighterState.DownKeyDoubleTapDelay);
			}
			else
			{
				FighterState.DownKeyDoubleTapReady = true;
				FighterState.UpKeyDoubleTapReady = false;
			}
		}
			

//		if(FighterState.LeftKeyPress) 
//		{
//			if(!FighterState.RightKeyPress){FighterState.RightKeyDoubleTapReady = false;} // Other keys interrupt double taps. 
//			if(!FighterState.UpKeyPress){FighterState.UpKeyDoubleTapReady = false;} // Other keys interrupt double taps.
//			if(!FighterState.DownKeyPress){FighterState.DownKeyDoubleTapReady = false;} // Other keys interrupt double taps.
//		}
//		if(FighterState.RightKeyPress) //Must come after double tap detection code so that simultaneous doubletaps don't block each other, but chains of conflicting keypresses will stop doubletaps.
//		{
//			FighterState.LeftKeyDoubleTapReady = false; // Other keys interrupt double taps.
//			FighterState.UpKeyDoubleTapReady = false; // Other keys interrupt double taps.
//			FighterState.DownKeyDoubleTapReady = false; // Other keys interrupt double taps.
//		}
//		if(FighterState.UpKeyPress) //Must come after double tap detection code so that simultaneous doubletaps don't block each other, but chains of conflicting keypresses will stop doubletaps.
//		{
//			FighterState.LeftKeyDoubleTapReady = false; // Other keys interrupt double taps.
//			FighterState.RightKeyDoubleTapReady = false; // Other keys interrupt double taps.
//			FighterState.DownKeyDoubleTapReady = false; // Other keys interrupt double taps.
//		}
//		if(FighterState.DownKeyPress) //Must come after double tap detection code so that simultaneous doubletaps don't block each other, but chains of conflicting keypresses will stop doubletaps.
//		{
//			FighterState.LeftKeyDoubleTapReady = false; // Other keys interrupt double taps.
//			FighterState.RightKeyDoubleTapReady = false; // Other keys interrupt double taps.
//			FighterState.UpKeyDoubleTapReady = false; // Other keys interrupt double taps.
//		}

		if(FighterState.LeftKeyDoubleTapDelay>i_DoubleTapDelayTime) // If over the time limit, next keypress won't count as a doubletap.
		{
			FighterState.LeftKeyDoubleTapReady = false;
		}
		if(FighterState.RightKeyDoubleTapDelay>i_DoubleTapDelayTime) // If over the time limit, next keypress won't count as a doubletap.
		{
			FighterState.RightKeyDoubleTapReady = false;
		}
		if(FighterState.UpKeyDoubleTapDelay>i_DoubleTapDelayTime) // If over the time limit, next keypress won't count as a doubletap.
		{
			FighterState.UpKeyDoubleTapReady = false;
		}
		if(FighterState.DownKeyDoubleTapDelay>i_DoubleTapDelayTime) // If over the time limit, next keypress won't count as a doubletap.
		{
			FighterState.DownKeyDoubleTapReady = false;
		}

		if((strandJumpHorz != 0)||(strandJumpVert != 0))
		{
			StrandJumpTypeA(strandJumpHorz, strandJumpVert);
		}

		if(FighterState.LeftClickRelease&&!(FighterState.DevMode||d_ClickToKnockFighter)&&!m_Kneeling)
		{
			if(IsVelocityPunching())
			{
				v_TriggerAtkHit = true;
			}
			else
			{
				if(!(FighterState.LeftKeyHold && (FighterState.PlayerMouseVector.normalized.x>0)) && !(FighterState.RightKeyHold && (FighterState.PlayerMouseVector.normalized.x<0))) // If trying to run opposite your punch direction, do not punch.
				{
					ThrowPunch(FighterState.PlayerMouseVector.normalized);
				}
				else
				{
					ThrowPunch(FighterState.Vel.normalized);
				}
			}
			//print("Leftclick detected");
			FighterState.LeftClickRelease = false;
		}

		if(FighterState.RightClickHold)
		{
			FighterState.Stance = 2;
		}

		if(FighterState.DisperseKeyPress)
		{
			ZonPulse();
			FighterState.DisperseKeyPress = false;
		}
			
		if(FighterState.LeftClickHold)
		{
			FighterState.LeftClickHoldDuration += Time.fixedDeltaTime;
			FighterState.Stance = 1;

			if((FighterState.LeftClickHoldDuration>=g_VelocityPunchChargeTime) && (!this.isSliding()))
			{
				g_VelocityPunching = true;
				o_VelocityPunch.inUse = true;
			}
			else
			{
				g_VelocityPunching = false;
				o_VelocityPunch.inUse = false;
			}
		}
		else
		{
			FighterState.LeftClickHoldDuration = 0;
			g_VelocityPunching = false;
			o_VelocityPunch.inUse = false;
		}
			
		if(FighterState.DisperseKeyPress)
		{
			ZonPulse();
			FighterState.DisperseKeyPress = false;
		}

		// Once the input has been processed, set the press inputs to false so they don't run several times before being changed by update() again. 
		// FixedUpdate can run multiple times before Update refreshes, so a keydown input can be registered as true multiple times before update changes it back to false, instead of just the intended one time.
		FighterState.LeftClickPress = false; 	
		FighterState.RightClickPress = false;
		FighterState.ZonKeyPress = false;				
		FighterState.DisperseKeyPress = false;				
		FighterState.JumpKeyPress = false;				
		FighterState.LeftKeyPress = false;
		FighterState.ShiftKeyPress = false;
		FighterState.RightKeyPress = false;
		FighterState.UpKeyPress = false;
		FighterState.DownKeyPress = false;

		FighterState.LeftClickRelease = false; 	
		FighterState.RightClickRelease = false;			
		FighterState.LeftKeyRelease = false;
		FighterState.ShiftKeyRelease = false;
		FighterState.RightKeyRelease = false;
		FighterState.UpKeyRelease = false;
		FighterState.DownKeyRelease = false;

		FighterState.DevkeyTilde = false;				
		FighterState.DevKey1 = false;				
		FighterState.DevKey2  = false;				
		FighterState.DevKey3  = false;				
		FighterState.DevKey4  = false;				
		FighterState.DevKey5  = false;				
		FighterState.DevKey6  = false;				
		FighterState.DevKey7  = false;				
		FighterState.DevKey8  = false;				
		FighterState.DevKey9  = false;
		FighterState.DevKey10  = false;				
		FighterState.DevKey11  = false;				
		FighterState.DevKey12 = false;				
	}

	protected override void UpdatePlayerInput() //UPI
	{
		if(!isLocalPlayer){return;}
		//
		// Individual keydown presses
		//

		FighterState.ScrollWheel = Input.GetAxis("Mouse ScrollWheel");

		if(Input.GetMouseButtonDown(0))
		{
			FighterState.LeftClickPress = true;
		}
		if(Input.GetMouseButtonDown(1))
		{
			FighterState.RightClickPress = true;
		}
		if(Input.GetButtonDown("Q"))
		{
			FighterState.ZonKeyPress = true;				
		}
		if(Input.GetButtonDown("Shift"))
		{
			FighterState.ShiftKeyPress = true;				
		}
		if(Input.GetButtonDown("Disperse"))
		{
			FighterState.DisperseKeyPress = true;				
		}
		if(Input.GetButtonDown("Jump"))
		{
			FighterState.JumpKeyPress = true;				
		}
		if(Input.GetButtonDown("Left"))
		{
			FighterState.LeftKeyPress = true;
			FighterState.LeftKeyDoubleTapDelay = 0;
		}
		if(Input.GetButtonDown("Right"))
		{
			FighterState.RightKeyPress = true;
			FighterState.RightKeyDoubleTapDelay = 0;
		}
		if(Input.GetButtonDown("Up"))
		{
			FighterState.UpKeyPress = true;
			FighterState.UpKeyDoubleTapDelay = 0;
		}
		if(Input.GetButtonDown("Down"))
		{
			FighterState.DownKeyPress = true;
			FighterState.DownKeyDoubleTapDelay = 0;
		}
	
		//
		// Dev Keys
		//
		if(Input.GetButtonDown("Tilde"))
		{
			FighterState.DevkeyTilde = true;				
		}
		if(Input.GetButtonDown("F1"))
		{
			FighterState.DevKey1 = true;				
		}
		if(Input.GetButtonDown("F2"))
		{
			FighterState.DevKey2  = true;				
		}
		if(Input.GetButtonDown("F3"))
		{
			FighterState.DevKey3  = true;				
		}
		if(Input.GetButtonDown("F4"))
		{
			FighterState.DevKey4  = true;				
		}
		if(Input.GetButtonDown("F5"))
		{
			FighterState.DevKey5  = true;				
		}
		if(Input.GetButtonDown("F6"))
		{
			FighterState.DevKey6  = true;				
		}
		if(Input.GetButtonDown("F7"))
		{
			FighterState.DevKey7  = true;				
		}
		if(Input.GetButtonDown("F8"))
		{
			FighterState.DevKey8  = true;				
		}
		if(Input.GetButtonDown("F9"))
		{
			FighterState.DevKey9  = true;				
		}
		if(Input.GetButtonDown("F10"))
		{
			FighterState.DevKey10  = true;				
		}
		if(Input.GetButtonDown("F11"))
		{
			FighterState.DevKey11  = true;				
		}
		if(Input.GetButtonDown("F12"))
		{
			FighterState.DevKey12 = true;				
		}

		//
		// Key-Up releases
		//
		if(Input.GetMouseButtonUp(0))
			FighterState.LeftClickRelease = true;
		
		if(Input.GetMouseButtonUp(1))
			FighterState.RightClickRelease = true;
		
		if(Input.GetButtonUp("Left"))
			FighterState.LeftKeyRelease = true;
		
		if(Input.GetButtonUp("Right"))
			FighterState.RightKeyRelease = true;
		
		if(Input.GetButtonUp("Up"))
			FighterState.UpKeyRelease = true;
		
		if(Input.GetButtonUp("Down"))
			FighterState.DownKeyRelease = true;

		if(Input.GetButtonUp("Shift"))
			FighterState.ShiftKeyRelease = true;				

		//
		// Key Hold-Downs
		//
		FighterState.LeftKeyHold = Input.GetButton("Left");
		FighterState.RightKeyHold = Input.GetButton("Right");
		FighterState.UpKeyHold = Input.GetButton("Up");
		FighterState.DownKeyHold = Input.GetButton("Down");
		FighterState.LeftClickHold = Input.GetMouseButton(0);
		FighterState.RightClickHold = Input.GetMouseButton(1);

		//
		// Mouse position in world space
		//
		Vector3 mousePoint = o_MainCamera.ScreenToWorldPoint(Input.mousePosition);
		FighterState.MouseWorldPos = Vec2(mousePoint);

	}

	protected override void ZonPulse()
	{
		if(FighterState.ZonLevel <= 0)
		{
			return;
		}

		FighterState.ZonLevel--;
		o_ProximityLiner.ClearAllFighters();
		GameObject newZonPulse = (GameObject)Instantiate(p_ZonPulse, this.transform.position, Quaternion.identity);
		newZonPulse.GetComponentInChildren<ZonPulse>().originFighter = this;
		newZonPulse.GetComponentInChildren<ZonPulse>().pulseRange = 150+(FighterState.ZonLevel*50);
		//o_ProximityLiner.outerRange = 100+(FighterState.ZonLevel*25);
		o_FighterAudio.ZonPulseSound();
	}

	protected void UpdatePlayerAnimation() // UPA
	{
		v_CameraScrollZoom -= FighterState.ScrollWheel*1.5f;
		v_CameraScrollZoom = (v_CameraScrollZoom>15) ? 15 : v_CameraScrollZoom; // Clamp max
		v_CameraScrollZoom = (v_CameraScrollZoom<1) ? 1 : v_CameraScrollZoom; // Clamp min

		v_CameraMode = v_DefaultCameraMode;
		if(m_Kneeling&&GetZonLevel()>0) // If kneeling, use superjump cam.
		{
			v_CameraMode = 2;
		}
		if((v_CameraMode==3 || v_CameraMode==2)&&o_TimeManager.GetTimeDilationM() < 1) // If set to normal camera mode, and game is in slowmo, use slowmo cam.
		{
			v_CameraMode = 5;
		}

	

		switch(v_CameraMode)
		{
		case 0: 
			{
				CameraControlTypeA(); //Player-locked velocity size-reactive camera
				break;
			}
		case 1: 
			{
				CameraControlTypeB(); //Mouse Directed Camera
				break;
			}
		case 2:
			{
				CameraControlTypeC(); // SuperJump Camera
				break;
			}
		case 3:
			{
				CameraControlTypeD(); // SuperJump Camera
				break;
			}
		case 4:
			{
				CameraControlTypeE(); // Locked Map Location Camera
				break;
			}
		case 5:
			{
				CameraControlTypeF(); // SlowMo focus cam
				break;	
			}
		default:
			{
				throw new Exception("ERROR: CAMERAMODE UNDEFINED.");
			}
		}
		v_CameraFinalSize = o_MainCamera.orthographicSize;

		o_Healthbar.SetCurHealth(FighterState.CurHealth);

		d_DeltaV = Math.Abs(d_LastFrameSpeed-FighterState.Vel.magnitude);
		d_LastFrameSpeed = FighterState.Vel.magnitude;


		if(o_Speedometer != null)
		{
			o_Speedometer.text = ""+Math.Round(FighterState.Vel.magnitude,0);
			//o_Speedometer.text = ""+Math.Round(d_DeltaV,0);
		}
		if(o_EnergyCounter!=null)
		{
			o_EnergyCounter.text = ""+FighterState.ZonLevel;
		}
	}

	protected void CameraControlTypeA() //CCTA - locked cam
	{
		if(!o_MainCamera){return;}
		v_SuperJumpStanceTime = 0;
		if(o_MainCameraTransform.parent==null)
		{
			o_MainCameraTransform.parent = this.transform;
			o_MainCameraTransform.localPosition = new Vector3(0, 0, -10);
		}

		#region zoom
		float zoomChange = 0;
		v_CameraZoomLevel = Mathf.Lerp(v_CameraZoomLevel, v_CameraScrollZoom, Time.deltaTime*10);
		o_MainCamera.orthographicSize = v_CameraZoomLevel;

		//o_MainCameraTransform.position = new Vector3(this.transform.position.x, this.transform.position.y, -10f);
		o_MainCameraTransform.localPosition = new Vector3(0, 0, -10f);
		//o_MainCamera.orthographicSize = 100f; // REMOVE THIS WHEN NOT DEBUGGING.

		#endregion
	}
		
	protected void CameraControlTypeB() //CCTB - simple aim cam
	{
		if(!o_MainCamera){return;}
		v_SuperJumpStanceTime = 0;
		if(o_MainCameraTransform.parent==null)
		{
			o_MainCameraTransform.parent = this.transform;
			o_MainCameraTransform.localPosition = new Vector3(0, 0, -10);
		}

		float goalZoom;
		float mySpeed = FighterState.Vel.magnitude;
		float camSpeedModifier = 0;
		if((0.15f*mySpeed)>=5f)
		{
			camSpeedModifier = (0.15f*mySpeed)-5f;
		}

		if(v_CameraScrollZoom+camSpeedModifier >= 40f)
		{
			goalZoom = 40f;
		}
		else
		{
			goalZoom = v_CameraScrollZoom+camSpeedModifier;
		}
		v_CameraZoomLevel = Mathf.Lerp(v_CameraZoomLevel, goalZoom, Time.deltaTime*10);
		o_MainCamera.orthographicSize = v_CameraZoomLevel;


		float camAverageX = (FighterState.MouseWorldPos.x-this.transform.position.x)/3;
		float camAverageY = (FighterState.MouseWorldPos.y-this.transform.position.y)/3;


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

		Vector3 camGoalLocation = new Vector3(camAverageX, camAverageY, -10f);

		o_MainCameraTransform.localPosition = Vector3.Lerp(o_MainCameraTransform.localPosition, camGoalLocation, 10*Time.unscaledDeltaTime); // CAMERA LERP TO POSITION. USUAL MOVEMENT METHOD.
	
	}

	protected void CameraControlTypeC() //CCTC - super jump cam
	{
		if(!o_MainCamera){return;}

		if(o_MainCameraTransform.parent==null)
		{
			o_MainCameraTransform.parent = this.transform;
			o_MainCameraTransform.localPosition = Vector3.zero;
		}
	
		float mySpeed = FighterState.Vel.magnitude;
		if(v_SuperJumpStanceTime<v_MaxSuperJumpStanceTime)
		{
			v_SuperJumpStanceTime += Time.unscaledDeltaTime;
		}
		else
		{
			mySpeed = GetZonLevel()*25;
		}

		float goalZoom;
		float camSpeedModifier = 0;
		if((0.15f*mySpeed)>=5f)
		{
			camSpeedModifier = (0.15f*mySpeed)-5f;
		}

		if(v_CameraScrollZoom+camSpeedModifier >= 40f)
		{
			goalZoom = 40f;
		}
		else
		{
			goalZoom = v_CameraScrollZoom+camSpeedModifier;
		}

		v_CameraZoomLevel = Mathf.Lerp(v_CameraZoomLevel, goalZoom, Time.deltaTime*3);
		o_MainCamera.orthographicSize = v_CameraZoomLevel;

		float camAverageX = (FighterState.MouseWorldPos.x-this.transform.position.x)/3;
		float camAverageY = (FighterState.MouseWorldPos.y-this.transform.position.y)/3;

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

		v_CamWhiplashAmount = Vector2.Lerp(v_CamWhiplashAmount, FighterState.Vel*v_CamWhiplashM, Time.unscaledDeltaTime);
		v_CamWhiplashRecovery = Vector2.Lerp(v_CamWhiplashRecovery, v_CamWhiplashAmount, Time.unscaledDeltaTime);

		float finalXPos = camAverageX-(v_CamWhiplashAmount.x-v_CamWhiplashRecovery.x);
		float finalYPos = camAverageY-(v_CamWhiplashAmount.y-v_CamWhiplashRecovery.y);

		//
		// The following block of code is for when the player hits the maximum bounds. The camera will instantly snap to the edge and won't go any further. Does not use lerp.
		//
		if(Mathf.Abs(finalXPos)>=xDistanceToMax)
		{
			if(finalXPos>0)
			{
				finalXPos = xDistanceToMax;
			}
			else
			{
				finalXPos = -xDistanceToMax;
			}
			//print("Too far horizontal!");
		}
		else
		{
			//print("Mathf.Abs(camAverageX) = "+Mathf.Abs(camAverageX)+" < "+xDistanceToMax+" = xDistanceToMax");
		}

		if(Mathf.Abs(finalYPos)>=yDistanceToMax)
		{
			//print("Too far vertical!");
			if(finalYPos>0)
			{
				finalYPos = yDistanceToMax;
			}
			else
			{
				finalYPos = -yDistanceToMax;
			}
		}
		else
		{
			//	print("Mathf.Abs(camAverageX) = "+Mathf.Abs(camAverageY)+" < "+yDistanceToMax+" = xDistanceToMax");
		}





		Vector3 camGoalLocation = new Vector3(finalXPos, finalYPos, -10f);

		o_MainCameraTransform.localPosition = Vector3.Lerp(o_MainCameraTransform.localPosition, camGoalLocation, 10*Time.unscaledDeltaTime); // CAMERA LERP TO POSITION. USUAL MOVEMENT METHOD.
	}
		
	protected void CameraControlTypeD() //CCTD - whiplash aimcam - Main camera style
	{
		if(!o_MainCamera){return;}
		v_SuperJumpStanceTime = 0;
		if(o_MainCameraTransform.parent==null)
		{
			o_MainCameraTransform.parent = this.transform;
			o_MainCameraTransform.localPosition = Vector3.zero;
		}
		#region zoom
		float goalZoom;
		float mySpeed = FighterState.Vel.magnitude;
		float camSpeedModifier = 0;
		if((0.15f*mySpeed)>=5f)
		{
			camSpeedModifier = (0.15f*mySpeed)-5f;
		}

		if(v_CameraScrollZoom+camSpeedModifier >= 40f)
		{
			goalZoom = 40f;
		}
		else
		{
			goalZoom = v_CameraScrollZoom+camSpeedModifier;
		}
		v_CameraZoomLevel = Mathf.Lerp(v_CameraZoomLevel, goalZoom, Time.deltaTime*10);
		o_MainCamera.orthographicSize = v_CameraZoomLevel;

		#endregion
		#region position
		float camAverageX = (FighterState.MouseWorldPos.x-this.transform.position.x)/3;
		float camAverageY = (FighterState.MouseWorldPos.y-this.transform.position.y)/3;


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
		//print("player: "+ +"\n lefted: "+botLeftEdge.x);
	
//		if(o_MainCameraTransform.position.x+camAverageX-xDistanceToEdge>this.transform.position.x) //If the edge of the proposed camera position is beyond the player, snap it back
//		{
//			print("Too far left!");
//			//camAverageX = this.transform.position.x+(xDistanceToEdge); //If it's outside of the leashzone, lock it to the edge.
//		}
//		if(o_MainCameraTransform.position.x+camAverageX+xDistanceToEdge<this.transform.position.x)
//		{
//			print("Too far Right!");
//			//camAverageX = this.transform.position.x-(xDistanceToEdge);
//		}
//
//		if(o_MainCameraTransform.position.y+camAverageY-yDistanceToEdge>this.transform.position.y) //If the edge of the proposed camera position is beyond the player, snap it back
//		{
//			print("Too far down!");
//			//camAverageY = this.transform.position.y+(yDistanceToEdge); //If it's outside of the leashzone, lock it to the edge.
//		}
//		if(o_MainCameraTransform.position.y+camAverageY+yDistanceToEdge<this.transform.position.y)
//		{
//			print("Too far up! player: "+this.transform.position.y+", edge: "+topRightEdge.y);
//			//camAverageY = this.transform.position.y-(yDistanceToEdge);
//		}
//

		v_CamWhiplashAmount = Vector2.Lerp(v_CamWhiplashAmount, FighterState.Vel*v_CamWhiplashM, Time.unscaledDeltaTime*2);

//		if(v_CamWhiplashAmount.magnitude>FighterState.Vel.magnitude*v_CamWhiplashM)
//		{
//			v_CamWhiplashAmount = FighterState.Vel*v_CamWhiplashM;
//		}
//
		v_CamWhiplashRecovery = Vector2.Lerp(v_CamWhiplashRecovery, v_CamWhiplashAmount, Time.unscaledDeltaTime*2);


		float finalXPos = camAverageX-(v_CamWhiplashAmount.x-v_CamWhiplashRecovery.x);
		float finalYPos = camAverageY-(v_CamWhiplashAmount.y-v_CamWhiplashRecovery.y);

		if(Mathf.Abs(finalXPos)>=xDistanceToMax)
		{
			if(finalXPos>0)
			{
				finalXPos = xDistanceToMax;
			}
			else
			{
				finalXPos = -xDistanceToMax;
			}
			//print("Too far horizontal!");
		}
		else
		{
			//print("Mathf.Abs(camAverageX) = "+Mathf.Abs(camAverageX)+" < "+xDistanceToMax+" = xDistanceToMax");
		}

		if(Mathf.Abs(finalYPos)>=yDistanceToMax)
		{
			//print("Too far vertical!");
			if(finalYPos>0)
			{
				finalYPos = yDistanceToMax;
			}
			else
			{
				finalYPos = -yDistanceToMax;
			}
		}
		else
		{
		//	print("Mathf.Abs(camAverageX) = "+Mathf.Abs(camAverageY)+" < "+yDistanceToMax+" = xDistanceToMax");
		}


		Vector3 camGoalLocation = new Vector3(finalXPos, finalYPos, -10f);

		o_MainCameraTransform.localPosition = Vector3.Lerp(o_MainCameraTransform.localPosition, camGoalLocation, 10*Time.unscaledDeltaTime); // CAMERA LERP TO POSITION. USUAL MOVEMENT METHOD.
		#endregion

		//
		// The following block of code is for when the player hits the maximum bounds. The camera will instantly snap to the edge and won't go any further. Does not use lerp.
		//


		//o_MainCamera.orthographicSize = 20f; // REMOVE THIS WHEN NOT DEBUGGING.

	}

	protected void CameraControlTypeE() // Scenic stationary camera
	{
		if(!o_MainCamera){return;}
		v_SuperJumpStanceTime = 0;
		v_CameraZoomLevel = Mathf.Lerp(v_CameraZoomLevel, 20f, Time.deltaTime*10);
		o_MainCamera.orthographicSize = v_CameraZoomLevel;
		o_MainCameraTransform.parent = null;
		o_MainCameraTransform.position = new Vector3(-4, 263, -10f);
	}

	protected void CameraControlTypeF() //CCTF - SlowMo
	{
		if(!o_MainCamera){return;}
		v_SuperJumpStanceTime = 0;
		if(o_MainCameraTransform.parent==null)
		{
			o_MainCameraTransform.parent = this.transform;
		}

		float mySpeed = FighterState.Vel.magnitude;
		float camSpeedModifier = 10f;
		if((0.15f*mySpeed)>=10f)
		{
			camSpeedModifier = (0.15f*mySpeed);
		}

		float timespeed = o_TimeManager.GetTimeDilationM();
		float zoomModifier = 1;
		if(timespeed<1)
		{
			zoomModifier += (1-timespeed);
		}
			
		o_MainCameraTransform.localPosition = new Vector3(0, 0, -10);
		v_CameraZoomLevel = Mathf.Lerp(v_CameraZoomLevel, (camSpeedModifier)/zoomModifier, Time.unscaledDeltaTime*5);
		o_MainCamera.orthographicSize = v_CameraZoomLevel;
	}

	#endregion
	//###################################################################################################################################
	// PUBLIC FUNCTIONS
	//###################################################################################################################################
	#region PUBLIC FUNCTIONS

	public void PulseHit(FighterChar theFighter)
	{
		o_ProximityLiner.AddFighter(theFighter);
	}

	#endregion
}