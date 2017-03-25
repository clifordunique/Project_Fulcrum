using UnityEngine.UI;
using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Spooler : MonoBehaviour 
{
	#region OBJECT REFERENCES
	[SerializeField]private GameObject o_SpoolRingPrefab;
	[SerializeField]private GameObject o_Player;
	[SerializeField]private Text o_FeedbackText;
	[SerializeField]private Ring[] o_Rings;
	[SerializeField]private Ring o_Core;
	[SerializeField]private Ring o_Limit;

	#endregion

	#region RINGPARAMS
	[SerializeField][Range(0,0.02f)] private float r_RingWidth;		// Thickness of rings.
	[SerializeField][Range(0,0.02f)] private float r_RingGap;		// Static gap between rings.
	[SerializeField][Range(0,0.02f)] private float r_BufferZone;		// Distance from core.
	[SerializeField][Range(0,0.02f)] private float r_CoreSize;		// Core size. Keep this lower than r_BufferZone.
	[SerializeField][Range(0,0.5f)] private float r_LimitRadius; // Max radius before overload occurs.
	[SerializeField][Range(0,0.02f)] private float r_LimitThickness; // Thickness of outer bound ring.
	[SerializeField][Range(0,10f)] private float r_MaxTime;			// Time per full rotation of ring.
	[SerializeField][ReadOnlyAttribute] private float r_CurTime;	// Amount of time since rotation started.
	[SerializeField][ReadOnlyAttribute] private float r_TotalPower;	// Amount of total power amassed. Max of 1 per ring.
	[SerializeField][ReadOnlyAttribute] private float r_OuterRadius;// Radius of the current outer ring.
	[SerializeField][ReadOnlyAttribute] private float r_MinRadius;	// Minimum radius of the current outer ring.
	[SerializeField][ReadOnlyAttribute] private float r_Rotation;   // Orientation of current ring.
	[SerializeField][ReadOnlyAttribute] private float r_Accuracy;   	// For your accuracy score.
	[SerializeField][ReadOnlyAttribute] private float r_Balance; 	// Negative when the current ring is ended early, pos when ended late.
	[SerializeField][ReadOnlyAttribute] private int r_RingNum;		// Number of the current outer ring.
	[SerializeField][ReadOnlyAttribute] private bool r_TooEarly;	// True when the turn is ended prematurely.
	[SerializeField][ReadOnlyAttribute] private bool r_Paused;		// True when spooling is paused.
	[SerializeField][ReadOnlyAttribute] private bool r_Active;	 	// True when a ring is mid spool.
	#endregion

	#region PLAYERINPUT
	private bool i_Jump;
	private bool i_KeyLeft;	
	private bool i_KeyRight;
	private bool i_KeyUp;
	private bool i_KeyDown;
	private bool i_Spool;
	private bool i_Reset;

	private int CtrlH; 				// Tracks horizontal keys pressed. Values are -1 (left), 0 (none), or 1 (right). 
	private int CtrlV; 				// Tracks vertical keys pressed. Values are -1 (down), 0 (none), or 1 (up).
	private bool facingDirection; 	// True means Right (the direction), false means Left.
	#endregion

	// Use this for initialization
	void Start () 
	{
		r_Paused = true;
		r_CurTime = 0;
		r_RingNum = -1;
		r_OuterRadius = 0;
		r_Rotation = 0;
		r_TotalPower = 0;
		r_Accuracy = 0;
	}
	
	// Update is called once per frame
	void Update () 
	{
		i_KeyLeft = CrossPlatformInputManager.GetButton("Left");
		i_KeyRight = CrossPlatformInputManager.GetButton("Right");
		i_KeyUp = CrossPlatformInputManager.GetButton("Up");
		i_KeyDown = CrossPlatformInputManager.GetButton("Down");
		i_Spool = CrossPlatformInputManager.GetButtonDown("Spooling");
		i_Reset = CrossPlatformInputManager.GetButtonDown("Interact");

		if(i_Reset)
		{
			Reset();
		}

		if(i_Spool)
		{
			if(r_RingNum < o_Rings.Length && r_OuterRadius < r_LimitRadius)
			{
				if(r_RingNum >= 0)
				{
					if(o_Rings[r_RingNum].getPercentWhite()>=1)
					{
						EndRing();
						AddRing();
					}
					else
					{
						r_TooEarly = true;
						o_Rings[r_RingNum].earlyStop = true;
					}
				}
				else
				{
					StartSpool();
				}

			}
			else
			{
				if(!r_Paused)
				{
					DevEndScore();
				}
			}
		}

		if(!r_Paused && r_RingNum >= 0 && r_RingNum < o_Rings.Length && (r_MinRadius*(r_CurTime/r_MaxTime))+r_RingWidth <= r_LimitRadius) // This code executes when there is an active ring filling.
		{
			//ContractTest();
				
			if(r_TooEarly) // if the player pressed button before a full turn.
			{
				r_CurTime += Time.deltaTime/2;
				r_OuterRadius = r_MinRadius;
				if(r_CurTime <= r_MaxTime)
				{
					//print("EARLY!");	
					float thePercent = (r_CurTime/r_MaxTime);
					o_Rings[r_RingNum].setPercentFull(thePercent);
					o_Rings[r_RingNum].radius = r_OuterRadius;
					//print(radians);
				}
				else
				{
					//print("EARLY ENDING!");	
					r_CurTime = r_MaxTime;
					o_Rings[r_RingNum].setPercentFull(1);
					o_Rings[r_RingNum].radius = r_OuterRadius;
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
					o_Rings[r_RingNum].setPercentFull(thePercent);
				}
				else if(r_CurTime <= r_MaxTime*2)
				{
					//print("OVERSPOOLING!");	
					float thePercent = (r_CurTime/r_MaxTime);
					r_OuterRadius = r_MinRadius*thePercent;
					o_Rings[r_RingNum].setPercentFull(thePercent);
					o_Rings[r_RingNum].radius = r_OuterRadius;
				}
				else
				{
					r_CurTime = r_MaxTime*2;
					float thePercent = 2;
					r_OuterRadius = r_MinRadius*thePercent;
					o_Rings[r_RingNum].setPercentFull(thePercent);
					o_Rings[r_RingNum].radius = r_OuterRadius;
					EndRing();
					AddRing();
				}
			}
		}
		else
		{
			if(r_RingNum >= 0)
			{	
				r_OuterRadius = r_LimitRadius;
				DevEndScore(); 
			}
		}

	}

	private void AddRing()
	{
		r_OuterRadius += r_RingWidth+r_RingGap;
		r_MinRadius = r_OuterRadius;

		r_RingNum++;
		GameObject AddRing = (GameObject)Instantiate(o_SpoolRingPrefab);
		AddRing.name = "Ring_"+r_RingNum;
		AddRing.transform.SetParent(this.transform, false);
		o_Rings[r_RingNum] = AddRing.GetComponent<Ring>();

		o_Rings[r_RingNum].setPercentFull(0);
		o_Rings[r_RingNum].radius = r_OuterRadius;
		o_Rings[r_RingNum].thickness = r_RingWidth;
		o_Rings[r_RingNum].rotation = r_Rotation;
		o_Rings[r_RingNum].UpdateVisuals();

		r_CurTime = 0;
	}

	private void EndRing()
	{
		r_TotalPower++;

		float thePercent = o_Rings[r_RingNum].getPercentWhite();

		if(thePercent>1)
		{
			r_Accuracy += 2-thePercent;
			print("This frame " +(int)((2-thePercent)*100));
			print("r_Accuracy: " + (int)(r_Accuracy*100/(r_TotalPower)));
		}
		else
		{
			r_Accuracy += thePercent;
			print("This frame: " + (int)(thePercent*100));
			print("r_Accuracy: " + (int)(r_Accuracy*100));
			print("True accuracy: " + (int)(r_Accuracy*100/(r_TotalPower)));
			thePercent = 1;
		}
			
		r_OuterRadius = r_MinRadius*thePercent;
		print("r_OuterRadius="+r_OuterRadius);

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
			print("TOO SMALL!");
			o_Rings[r_RingNum].radius = r_OuterRadius;
		}
		else
		{
			print("Proper size!");
			o_Rings[r_RingNum].radius = r_OuterRadius;
			//r_Accuracy += 2-thePercent;

		}
		print("END RING ####################");	
	}

	private void Contract()
	{
		
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
		
	private void Reset()
	{
		if(!r_Active)
		{
			return;
		}
		r_Paused = true;
		r_CurTime = 0;
		r_RingNum = -1;
		r_OuterRadius = 0;
		r_Rotation = 0;
		r_TotalPower = 0;
		r_Accuracy = 0;
		r_TooEarly = false;

		o_FeedbackText.text = "";

		Destroy(o_Core.gameObject);

		Destroy(o_Limit.gameObject);

		foreach(Ring theRing in o_Rings)
		{
			if(theRing != null)
			{
				Destroy(theRing.gameObject);
			}
		}
		r_Active = false;
	}

	private void StartSpool()
	{
		r_CurTime = 0;
		r_RingNum = -1;
		r_Rotation = 0;
		r_TotalPower = 0;
		r_Accuracy = 0;

		r_Paused = false;

		GameObject newCore = (GameObject)Instantiate(o_SpoolRingPrefab);
		newCore.name = "Core";
		newCore.transform.SetParent(this.transform, false);
		o_Core = newCore.GetComponent<Ring>();
		o_Core.setPercentFull(1);
		o_Core.radius = 0;
		o_Core.thickness = r_CoreSize;
		o_Core.rotation = r_Rotation;
		o_Core.lerpAllChanges = true;
		o_Core.UpdateVisuals();

		GameObject newLimit = (GameObject)Instantiate(o_SpoolRingPrefab);
		newLimit.name = "Limit";
		newLimit.transform.SetParent(this.transform, false);
		o_Limit = newLimit.GetComponent<Ring>();
		o_Limit.setPercentFull(1);
		o_Limit.radius = r_LimitRadius;
		o_Limit.thickness = r_LimitThickness;
		o_Limit.rotation = r_Rotation;
		o_Limit.color = new Vector4(1,1,1,0.3f);
		o_Limit.lerpAllChanges = true;
		o_Limit.UpdateVisuals();


		r_OuterRadius = r_CoreSize+r_BufferZone;
		AddRing();
		r_Active = true;
	}

	private void DevEndScore()
	{
		r_Paused = true;
		float accuracyScore = 100*(r_Accuracy/r_TotalPower);
		o_FeedbackText.text = "Power Level: " + r_TotalPower + "\nAccuracy:" +(int)accuracyScore+"%";
		o_FeedbackText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 500-(r_OuterRadius*300*this.transform.localScale.magnitude));
	}

}
