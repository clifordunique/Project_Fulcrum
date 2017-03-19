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
	[SerializeField]private Ring[] o_BadRings;

	#endregion
	#region RINGPARAMS
	[SerializeField][Range(0,0.1f)] private float r_RingWidth;		// Thickness of rings.
	[SerializeField][Range(0,0.1f)] private float r_RingGap;		// Static gap between rings.
	[SerializeField][ReadOnlyAttribute] private float r_CurTime;	// Amount of time since rotation started.
	[SerializeField][Range(0,10f)] private float r_MaxTime;			// Time per full rotation of ring.
	[SerializeField][ReadOnlyAttribute] private float r_TotalPower;	// Amount of total power amassed. Max of 1 per ring.
	[SerializeField][ReadOnlyAttribute] private float r_OuterRadius;// Radius of the current outer ring.
	[SerializeField][ReadOnlyAttribute] private float r_MinRadius;	// Minimum radius of the current outer ring.
	[SerializeField][ReadOnlyAttribute] private float r_RingGrowth; // How much the outer ring has expanded from its base state.
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
		r_RingGrowth = 0;
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

		if(i_Spool)
		{
			if(r_RingNum < 20 && r_OuterRadius <= 0.06f)
			{
				if(r_RingNum >= 0)
				{
					if((r_RingGrowth-1)>0)
					{
						EndRing();
						AddRing();
					}
					else
					{
						//print("TOO EARLY");
						r_TooEarly = true;
					}
				}
				else
				{
					r_Paused = false;
					AddRing();
				}

			}
			else
			{
				if(!r_Paused)
				{
					DevEndScore();
				}
				else
				{
					Reset();
				}
			}
		}

		if(!r_Paused && r_RingNum >= 0 && r_RingNum < 20 && r_OuterRadius <= 0.06f) // This code executes when there is an active ring filling.
		{
			if(r_TooEarly) // if the player pressed button before a full turn.
			{
				r_CurTime += Time.deltaTime/2;
				if(r_CurTime <= r_MaxTime)
				{
					//print("SPOOLING!");	
					float radians = (r_CurTime/r_MaxTime)*6.28f;
					o_BadRings[r_RingNum].fillAmount = radians;
					//print(radians);
				}
				else
				{
					o_BadRings[r_RingNum].fillAmount = 6.28f;
					//o_Rings[r_RingNum].fillAmount = 6.28f;
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
					float radians = (r_CurTime/r_MaxTime)*6.28f;
					o_BadRings[r_RingNum].fillAmount = radians;
					o_Rings[r_RingNum].fillAmount = radians;
					r_RingGrowth = (radians/6.28f);
					//print(radians);
				}
				else if(r_CurTime <= r_MaxTime*2)
				{
					//print("BADSPOOLING!");	
					o_Rings[r_RingNum].fillAmount = 6.28f;
					float radians = ((r_CurTime/r_MaxTime)*6.28f)-6.28f;
					o_BadRings[r_RingNum].fillAmount = radians;
					r_RingGrowth = 1+(radians/6.28f);
					o_BadRings[r_RingNum].radius = r_OuterRadius*r_RingGrowth;
					o_Rings[r_RingNum].radius = r_OuterRadius*r_RingGrowth;
					o_BadRings[r_RingNum].color = new Vector4(1,0,0,1);
					o_BadRings[r_RingNum].depth = 1;

				}
				else
				{
					r_RingGrowth = 2;
					o_BadRings[r_RingNum].fillAmount = 6.28f;
					o_Rings[r_RingNum].fillAmount = 6.28f;
					EndRing();
					AddRing();
				}
			}
		}
		else
		{
			if(r_RingNum >= 0)
			{	
				DevEndScore(); 
			}
		}

	}

	private void AddRing()
	{
		r_OuterRadius *= r_RingGrowth;
		r_OuterRadius += r_RingWidth+r_RingGap;
		r_MinRadius = r_OuterRadius;

		r_RingNum++;
		GameObject AddRing = (GameObject)Instantiate(o_SpoolRingPrefab);
		AddRing.name = "Ring_"+r_RingNum;
		AddRing.transform.SetParent(this.transform, false);
		o_Rings[r_RingNum] = AddRing.GetComponent<Ring>();

		//o_Rings[r_RingNum].transform.localPosition = new Vector2(1*r_RingNum, 0);

		o_Rings[r_RingNum].fillAmount = 0;
		o_Rings[r_RingNum].radius = r_OuterRadius;
		o_Rings[r_RingNum].thickness = r_RingWidth;
		o_Rings[r_RingNum].rotation = r_Rotation;

		GameObject newBadRing = (GameObject)Instantiate(o_SpoolRingPrefab);
		newBadRing.name = "BadRing_"+r_RingNum;
		newBadRing.transform.SetParent(this.transform, false);
		o_BadRings[r_RingNum] = newBadRing.GetComponent<Ring>();

		//o_BadRings[r_RingNum].transform.localScale = new Vector3(-1,1,1);

		o_BadRings[r_RingNum].fillAmount = 0;
		o_BadRings[r_RingNum].radius = r_OuterRadius;
		o_BadRings[r_RingNum].thickness = r_RingWidth;
		o_BadRings[r_RingNum].rotation = r_Rotation;
		o_BadRings[r_RingNum].color = new Vector4(0,0,1,1);
		o_BadRings[r_RingNum].depth = -1;

		r_CurTime = 0;
		r_RingGrowth = 0;

		o_BadRings[r_RingNum].UpdateVisuals();
		o_Rings[r_RingNum].UpdateVisuals();
	}

	private void EndRing()
	{
		r_TotalPower++;

		float radians = ((r_CurTime/r_MaxTime)*6.28f)-6.28f;
		float degrees = ((radians/3.14f)*180f);
		r_Rotation += degrees;
		r_TooEarly = false;

		if(r_OuterRadius*r_RingGrowth < r_MinRadius)
		{
			//print("TOO SMALL!");
			o_BadRings[r_RingNum].color = new Vector4(0,0,1,1);
			o_Rings[r_RingNum].color = new Vector4(1,1,1,1);
			o_BadRings[r_RingNum].radius = r_OuterRadius;
			o_Rings[r_RingNum].radius = r_OuterRadius;
			r_Accuracy += r_RingGrowth;
			print("This frame: " + (int)(r_RingGrowth*100));
			print("r_Accuracy: " + (int)(r_Accuracy*100));
			print("True accuracy: " + (int)(r_Accuracy*100/(r_TotalPower)));
			r_RingGrowth = 1;
		}
		else
		{
			o_BadRings[r_RingNum].radius = r_OuterRadius*r_RingGrowth;
			o_Rings[r_RingNum].radius = r_OuterRadius*r_RingGrowth;
			r_Accuracy += 2-(r_RingGrowth);
			print("This frame " +(int)((2-r_RingGrowth)*100));
			print("r_Accuracy: " + (int)(r_Accuracy*100/(r_TotalPower)));
		}
		print("END RING ####################");	
	}

	private void Contract()
	{
		
	}

	private void Reset()
	{
		r_Paused = true;
		r_CurTime = 0;
		r_RingNum = -1;
		r_OuterRadius = 0;
		r_RingGrowth = 0;
		r_Rotation = 0;
		r_TotalPower = 0;
		r_Accuracy = 0;

		foreach(Ring theRing in o_Rings)
		{
			if(theRing != null)
			{
				Destroy(theRing.gameObject);
			}
		}
		foreach(Ring theRing in o_BadRings)
		{
			if(theRing != null)
			{
				Destroy(theRing.gameObject);
			}
		}
	}

	private void DevEndScore()
	{
		r_Paused = true;
		float accuracyScore = 100*(r_Accuracy/r_TotalPower);
		o_FeedbackText.text = "Power Level: " + r_TotalPower + "\nAccuracy:" +(int)accuracyScore+"%";
		o_FeedbackText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 280-(r_OuterRadius*1600));
	}

}
