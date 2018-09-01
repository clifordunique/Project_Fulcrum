using UnityEngine.UI;
using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using AK.Wwise;

public class Spooler : NetworkBehaviour
{
	#region OBJECT REFERENCES
	[SerializeField]private GameObject p_SpoolRingPrefab;
	[SerializeField]private AudioMixer o_SpoolMixer;
	[SerializeField]private AudioClip[] s_SpoolUp;
	[SerializeField][ReadOnlyAttribute] private Player o_Player;
	[SerializeField][ReadOnlyAttribute] private Transform o_SpoolerTransform;
	[SerializeField]private Text o_FeedbackText;
	[SerializeField]private Ring[] o_Rings;
	[SerializeField]private Ring o_Core;
	[SerializeField]private Ring o_Limit;
	#endregion

	#region RINGPARAMS
	[SerializeField][Range(0,0.2f)] private float r_RingWidth;		// Thickness of rings.
	[SerializeField][Range(0,0.05f)] private float r_RingGap;		// Static gap between rings.
	[SerializeField][Range(0,0.5f)] private float r_BufferZone;		// Distance from core.
	[SerializeField][Range(0,0.2f)] private float r_CoreSize;		// Core size. Keep this lower than r_BufferZone.
	[SerializeField][Range(0,300)] private float r_LimitRadius; 	// Max radius before overload occurs.
	[SerializeField][Range(0,20)] private int r_LimitInRings = 9; 	// Max radius measured in rings.
	[SerializeField][Range(0,0.04f)] private float r_LimitThickness; // Thickness of outer bound ring.
	[SerializeField][Range(0,10f)] private float r_MaxTime;			// Time per full rotation of ring.
	[SerializeField][ReadOnlyAttribute] private float r_CurTime;	// Amount of time since rotation started.
	[SerializeField][ReadOnlyAttribute] private int r_TotalPower;	// Amount of total power amassed. Max of 1 per ring.
	[SerializeField][ReadOnlyAttribute] private float r_OuterRadius;// Radius of the current outer ring.
	[SerializeField][ReadOnlyAttribute] private float r_MinRadius;	// Minimum radius of the current outer ring.
	[SerializeField][ReadOnlyAttribute] private float r_Rotation;   // Orientation of current ring.
	[SerializeField][ReadOnlyAttribute] private float r_Accuracy;   	// For your accuracy score.
	[SerializeField][ReadOnlyAttribute] private float r_Balance; 	// Negative when the current ring is ended early, pos when ended late.
	[SerializeField][ReadOnlyAttribute] private int r_OuterRingID;	// Number of the current outer ring. Starts at 0. -1 when no circles present.
	[SerializeField][ReadOnlyAttribute] private bool r_TooEarly;	// True when the turn is ended prematurely.
	[SerializeField][ReadOnlyAttribute] private bool r_Paused;		// True when spooling is paused.
	[SerializeField][ReadOnlyAttribute] private bool r_MaxReached;		// True when spooling is at its max size.
	[SerializeField][ReadOnlyAttribute] public bool r_Active;	 	// True when a ring is mid spool.
	[SerializeField][Range(0,0.5f)] private float r_CritRange=0.025f;// Percent distance away from 100% that the player will have critically succeeded in locking a ring.

	#endregion

	public bool i_GoodStance;
	private float currentZoom;

	// Use this for initialization
	void Start () 
	{
		o_Player = this.gameObject.GetComponent<Player>();
		o_SpoolerTransform = transform.Find("SpoolerTransform");
		if(!o_Player.isLocalPlayer)
		{
			this.enabled = false;
		}
		GameObject newCore = (GameObject)Instantiate(p_SpoolRingPrefab);
		newCore.name = "Core";
		newCore.transform.SetParent(o_SpoolerTransform, false);
		o_Core = newCore.GetComponent<Ring>();

		GameObject newLimit = (GameObject)Instantiate(p_SpoolRingPrefab);
		newLimit.name = "Limit";
		newLimit.transform.SetParent(o_SpoolerTransform, false);
		o_Limit = newLimit.GetComponent<Ring>();

		o_Rings = new Ring[30];

		for(int i = 0; i<o_Rings.Length; i++)
		{
			GameObject newRing = (GameObject)Instantiate(p_SpoolRingPrefab);
			newRing.name = "Ring_"+i;
			newRing.transform.SetParent(o_SpoolerTransform, false);
			o_Rings[i] = newRing.GetComponent<Ring>();
			o_Rings[i].ringHidden = true;
		}

		i_GoodStance = false;
		r_Paused = true;
		r_CurTime = 0;
		r_OuterRingID = -1;
		r_OuterRadius = 0;
		r_Rotation = 0;
		r_TotalPower = 0;
		r_Accuracy = 0;
		//o_FeedbackText = GameObject.Find("Dev_SpoolScore").GetComponent<Text>();
	}

	void Awake() 
	{
		o_Player = this.gameObject.GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update() 
	{
		i_GoodStance = true;
		o_Limit.radius = r_LimitRadius;

		if(r_OuterRadius/r_LimitRadius>0.75f)
		{
			//print("DANGER! POWER AT "+(r_OuterRadius/r_LimitRadius*100)+" percent of maximum!!");
		}

		if(currentZoom!=o_Player.v_CameraFinalSize)
		{
			currentZoom = o_Player.v_CameraFinalSize;
			float scaleFactor = currentZoom/2;
			Vector3 scaleVector = new Vector3(scaleFactor, scaleFactor, 1);
			//o_Core.transform.localScale = scaleVector;
			o_Limit.transform.localScale = scaleVector;
			foreach(Ring r in o_Rings)
			{
				if(r!=null)
				{
					r.transform.localScale = scaleVector;
				}
			}
		}

		if(!r_Active || (r_Paused) || (r_OuterRingID<0))
		{
			return;
		}

		//if: not exceeding max rings,		and not outside max radius		
		if((r_OuterRingID < o_Rings.Length) && ((r_MinRadius*(r_CurTime/r_MaxTime))+r_RingWidth <= r_LimitRadius) ) // This code executes when there is an active ring filling, and it's not too large.
		{
			//ContractTest();
			//print("Growing");
			if(r_TooEarly) // if the player pressed button before a full turn.
			{
				r_CurTime += Time.deltaTime/2;
				r_OuterRadius = r_MinRadius;
				if(r_CurTime <= r_MaxTime)
				{
					//print("EARLY!");	
					float thePercent = (r_CurTime/r_MaxTime);
					o_Rings[r_OuterRingID].SetPercentFull(thePercent);
					o_Rings[r_OuterRingID].radius = r_OuterRadius;
					//print(radians);
				}
				else
				{
					//print("EARLY ENDING!");	
					r_CurTime = r_MaxTime;
					o_Rings[r_OuterRingID].SetPercentFull(1);
					o_Rings[r_OuterRingID].radius = r_OuterRadius;
					EndRing();
					AddRing();
				}
			}
			else
			{
				r_CurTime += Time.deltaTime;
				if(r_CurTime <= r_MaxTime)
				{
					//print("SPOOLING!");	
					float thePercent = (r_CurTime/r_MaxTime);
					o_Rings[r_OuterRingID].SetPercentFull(thePercent);
				}
				else if(r_CurTime <= r_MaxTime*2)
				{
					//print("OVERSPOOLING!");	
					float thePercent = (r_CurTime/r_MaxTime);
					r_OuterRadius = r_MinRadius*thePercent;
					o_Rings[r_OuterRingID].SetPercentFull(thePercent);
					o_Rings[r_OuterRingID].radius = r_OuterRadius;
				}
				else
				{
					r_CurTime = r_MaxTime*2;
					float thePercent = 2;
					r_OuterRadius = r_MinRadius*thePercent;
					o_Rings[r_OuterRingID].SetPercentFull(thePercent);
					o_Rings[r_OuterRingID].radius = r_OuterRadius;
					EndRing();
					AddRing();
				}
			}
		}
		else
		{
			if(r_OuterRingID >= 0)
			{	
				r_OuterRadius = r_LimitRadius;
				o_Player.ChargeBackfire(r_OuterRingID);
				Reset();
				r_Paused = true;
			}
		}
	}

	public void Reset()
	{
		AbsorbAndClose();
		o_Player.SetEtherLevel(0);
		o_Core.lerpRadius = 0;
		o_Core.lerpthickness = 0;
		o_Core.lerpVortexAmount = 0;

		o_Limit.ringHidden = true;
		o_Limit.lerpRadius = 0;
		o_Limit.lerpthickness = 0;
		o_Limit.lerpVortexAmount = 0;

		foreach(Ring r in o_Rings)
		{
			if(r!=null)
			{
				r.ringHidden = true;
				r.lerpRadius = 0;
				r.lerpthickness = 0;
				r.lerpVortexAmount = 0;
			}
		}
	}

	public void LockRing()
	{
		if(!r_Active)
		{
			return;
		}

		//print("Q Pressed");


		if(r_OuterRingID < o_Rings.Length && r_OuterRadius < r_LimitRadius)
		{
			if(r_Paused)
			{
				//print("Adding first ring.");
				r_Paused = false;
				AddRing();
			}
			else if(r_OuterRingID>=0)
			{
				if(o_Rings[r_OuterRingID].GetPercentWhite()>=1)
				{
					EndRing();
					//print("Ended last ring and started new.");
					AddRing();
				}
				else
				{
					//print("Too early.");
					r_TooEarly = true;
					o_Rings[r_OuterRingID].earlyStop = true;
				}
			}
		}
		else
		{
			//print("Final circle, no more rings allowed.");
			r_Paused = true;
		}
	}

	private void AddRing()
	{
		//print("AddRing");

		r_OuterRingID++;

		o_Rings[r_OuterRingID].ringHidden = false;
		o_Rings[r_OuterRingID].SetPercentFull(0);
		o_Rings[r_OuterRingID].radius = r_OuterRadius;
		o_Rings[r_OuterRingID].lerpRadius = r_OuterRadius;
		o_Rings[r_OuterRingID].thickness = r_RingWidth;
		o_Rings[r_OuterRingID].lerpthickness = r_RingWidth;

		o_Rings[r_OuterRingID].rotation = r_Rotation;
		o_Rings[r_OuterRingID].lerpAllChanges = true;
		o_Rings[r_OuterRingID].lerpRadius = r_OuterRadius;


		//o_Rings[r_RingNum].color = new Vector4(1,1,1,0.5f);
		o_Rings[r_OuterRingID].UpdateVisuals();

		AkSoundEngine.PostEvent("EnergyCharge", gameObject);
		AkSoundEngine.SetRTPCValue("EnergyLevel", r_TotalPower, gameObject);

		//print("RINGNUM="+r_RingNum);
		//print("thePitch"+thePitch);	
		r_CurTime = 0;
	}

	private void EndRing() //ER
	{
		//print("EndRing");

		r_TotalPower++;
		o_Player.SetEtherLevel(r_TotalPower);

		float thePercent = o_Rings[r_OuterRingID].GetPercentWhite();

		if(Math.Abs(thePercent-1)<=r_CritRange)
		{
			thePercent = 1;
			AkSoundEngine.PostEvent("ChargeCrit", gameObject);
			o_Rings[r_OuterRingID].criticalSuccess = true;
			r_LimitRadius += (r_RingGap+r_RingWidth)/2;
		}

		if(thePercent>1)
		{
			r_Accuracy += 2-thePercent;
			//print("This frame " +(int)((2-thePercent)*100));
			//print("r_Accuracy: " + (int)(r_Accuracy*100/(r_TotalPower)));
		}
		else
		{
			r_Accuracy += thePercent;
			//print("This frame: " + (int)(thePercent*100));
			//print("r_Accuracy: " + (int)(r_Accuracy*100));
			//print("True accuracy: " + (int)(r_Accuracy*100/(r_TotalPower)));
			thePercent = 1;
		}
			
		r_OuterRadius = r_MinRadius*thePercent;
		//print("r_OuterRadius="+r_OuterRadius);

		float degrees = (thePercent*360);
		r_Rotation += degrees;

		if(r_Rotation >= 360)
		{
			r_Rotation -= 360;
		}
		if(r_Rotation <= -360)
		{
			r_Rotation += 360;
		}

		r_TooEarly = false;

		if(thePercent < 1)
		{
			//print("TOO SMALL!");
			o_Rings[r_OuterRingID].radius = r_OuterRadius;
		}
		else
		{
			//print("Proper size!");
			o_Rings[r_OuterRingID].radius = r_OuterRadius;
			//r_Accuracy += 2-thePercent;

		}
			
		r_OuterRadius += r_RingWidth+r_RingGap;
		r_MinRadius = r_OuterRadius;
		//print("END RING ####################");	
	}

	private void ContractTest()
	{
		r_MinRadius -= 0.00002f;
		foreach(Ring theRing in o_Rings)
		{
			if(theRing != null)
			{
				theRing.radius -= 0.00002f;
			}
		}
	}

	public void HideSpooler()
	{
		AbsorbAndClose();
	}
		
	public void OpenSpooler()
	{
		o_Core.ringHidden = false;
		o_Limit.ringHidden = false;

		foreach(Ring r in o_Rings)
		{
			if(r!=null)
			{
				r.ringHidden = false;
			}
		}
		if(!r_Active)
		{
			StartSpool();
		}
	}
		
	public void AbsorbAndClose()
	{
		//print("AbsorbAndClose");
		if(!r_Active)
		{
			return;
		}



		//r_RingNum = -1;
		//r_OuterRadius = 0;
		//r_Rotation = 0;
		//r_TotalPower = 0;
		//r_Accuracy = 0;
		if(r_OuterRingID>=0) // If at least one ring exists
		{
			if(r_OuterRingID<o_Rings.Length)
			{
				if(!r_Paused && o_Rings[r_OuterRingID].GetPercentFull()>1) // If final ring is over full, end it before closing.
				{
					EndRing();
				}
				else if((o_Rings[r_OuterRingID].GetPercentFull()<1)) // If final ring is incomplete, discard it before closing.
				{
					//print("Discarding incomplete outer ring");
					o_Rings[r_OuterRingID].SetPercentFull(0);
					r_OuterRingID--;
				}
			}
		}


		o_Core.ringHidden = true;
		o_Limit.ringHidden = true;

		//o_FeedbackText.text = "";

		o_Player.SetEtherLevel(r_TotalPower);

		foreach(Ring r in o_Rings)
		{
			if(r!=null)
			{
				r.ringHidden = true;
			}
		}
		r_Active = false;
		r_Paused = true;
		r_CurTime = 0;
		r_TooEarly = false;
	}

	public void StartSpool()
	{
		//print("StartSpool!");
		r_LimitRadius = r_CoreSize+r_BufferZone+(r_LimitInRings*(r_RingGap+r_RingWidth));
		r_TotalPower = o_Player.GetEtherLevel();

		for(int i = 0; i<r_TotalPower; i++)
		{
			o_Rings[i].ringHidden = false;
			o_Rings[i].thickness = r_RingWidth;
		}

		for(int i = r_TotalPower; i<o_Rings.Length; i++)
		{
			o_Rings[i].ResetRing();
			o_Rings[i].thickness = r_RingWidth;
		}

		r_TooEarly = false;
		r_CurTime = 0;
		r_OuterRingID = r_TotalPower-1;
		r_Accuracy = 0;

		if(r_OuterRingID<0)
		{
			r_OuterRadius = r_CoreSize+r_BufferZone;
			r_MinRadius = r_OuterRadius;
		}
		else
		{
			r_OuterRadius = o_Rings[r_OuterRingID].radius+r_RingWidth+r_RingGap;
			r_MinRadius = r_OuterRadius;
		
		}
		o_Core.ringHidden = false;
		o_Core.SetPercentFull(1);
		o_Core.radius = 0;
		o_Core.thickness = r_CoreSize;
		o_Core.rotation = r_Rotation;
		o_Core.lerpAllChanges = true;
		o_Core.UpdateVisuals();

		o_Limit.ringHidden = false;
		o_Limit.SetPercentFull(1);
		o_Limit.radius = r_LimitRadius;
		o_Limit.thickness = r_LimitThickness;
		o_Limit.rotation = r_Rotation;
		o_Limit.isTransparent = true;
		o_Limit.lerpAllChanges = true;
		o_Limit.UpdateVisuals();

		r_Active = true;
	}

	//###################################################################################################################################
	// PUBLIC FUNCTIONS
	//###################################################################################################################################
	#region PUBLIC FUNCTIONS

	public int GetTotalPower()
	{
		return r_TotalPower;
	}

	#endregion

}
