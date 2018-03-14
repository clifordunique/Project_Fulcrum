using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviour 
{
	
	#region visualparams
	[SerializeField] public Vector4 color = new Vector4(1,1,1,1);		// Colour.
	[SerializeField] public Vector4 badColor = new Vector4(0,0,1,1);	// Colour.
	[SerializeField] public Vector4 blue = new Vector4(0,0,1,1);		// Colour.
	[SerializeField] public Vector4 red = new Vector4(1,0,0,1);			// Colour.


	[SerializeField][Range(0,360)] public float rotation = 0; 			// Starting rotation.
	[SerializeField][Range(0,0.1f)] public float radius = 0; 			// Inner radius.
	[SerializeField][Range(0,0.1f)] public float visualradius = 0; 		// Inner radius visual.
	[SerializeField][Range(0,0.1f)] public float thickness = 0; 			// Width of ring.
	[SerializeField][Range(0,6.28f)] private float fillAmount = 0; 		// How much the ring is filled, in radians. 0 to 6.28.
	[SerializeField][Range(0,6.28f)] private float badFillAmount = 0; 	// How much the badring is filled, in radians. 0 to 6.28.
	[SerializeField] public bool badOnTop = false; 						// Sets which colour ring sits on top.
	[SerializeField] public bool isCore = false; 						// Sets which colour ring sits on top.
	[SerializeField] public bool ringHidden = false; 					// Sets whether or not to shrink the ring into invisibility or not.
	[SerializeField] public bool lerpAllChanges = true; 				// Sets whether to move instantly or lerp towards real values.
	[SerializeField] public float lerpRadius = 0; 						// Transitional radius. Moves toward real value.
	[SerializeField] public float lerpthickness = 0; 					// Transitional thickness. Moves toward real value.
	[SerializeField] private Renderer r_Ring;							// Ring Sprite renderer
	[SerializeField] private Renderer r_BadRing;						// badRing Sprite renderer
	#endregion

	#region gameparams
	[SerializeField] public bool earlyStop = false; 		
	[SerializeField][Range(0,2f)] private float percentFilled = 0; //percentFilled * 100 = the actual percent value.
	[SerializeField][Range(0,1f)] private float percentBlue = 0; //percentBlue * 100 = the actual percent value.
	#endregion

	// Use this for initialization
	void Start () 
	{	
	}

	public void ResetRing()
	{
		percentBlue = 0;
		percentFilled = 0;
		ringHidden = true;
		earlyStop = false;
	}

	public void SetPercentFull(float percent)
	{
		if(percent >= 0 && percent <= 2)
		{
			if(earlyStop)
			{
				percentBlue = percent;
			}
			else
			{
				percentFilled = percent;
			}
		}
	}

	public float GetPercentFull()
	{
		if(earlyStop)
		{
			return percentBlue;
		}
		else
		{
			return percentFilled;
		}
	}

	public float GetPercentWhite()
	{
		return percentFilled;
	}

		
	// Update is called once per frame
	void Update ()
	{
		if(lerpAllChanges)
		{
			RingLerp();
		}

		UpdateVisuals();
	}

	private void RingLerp()
	{
		if(!ringHidden)
		{
			if(Mathf.Abs(radius-lerpRadius)>0.001f)
			{
				lerpRadius += (radius-lerpRadius)/8;
			}
			else
			{
				lerpRadius = radius;
			}

			if(Mathf.Abs(thickness-lerpthickness)>0.001f)
			{
				lerpthickness += (thickness-lerpthickness)/8;
			}
			else
			{
				lerpthickness = thickness;
			}
		}
		else
		{
			if(Mathf.Abs(lerpRadius)>0.001f)
			{
				lerpRadius -= (lerpRadius)/8;
			}
			else // Once radius is fully retracted, contract the width also.
			{
				lerpRadius = 0;
				if(Mathf.Abs(lerpthickness)>0.001f)
				{
					lerpthickness -= (lerpthickness)/8;
				}
				else
				{
					lerpthickness = 0;
				}
			}

		}

	}

	public void UpdateVisuals()
	{

		if(lerpAllChanges && lerpthickness<=0)
		{
			r_Ring.enabled = false;
			r_BadRing.enabled = false;
		}
		else
		{
			r_Ring.enabled = true;
			r_BadRing.enabled = true;
		}

		if(earlyStop&&percentFilled>1)
		{
			earlyStop = false; // If filled past 100%, could not possibly be underfilled.
		}

		if(earlyStop) //If stopped early by player, render extra fill as blue.
		{
			badOnTop = false;
			badColor = blue;
			if(percentBlue <= 1) 
			{
				badFillAmount = (percentBlue)*6.28f;
				fillAmount = (percentFilled)*6.28f;
			}
			else
			{
				fillAmount = (percentFilled)*6.28f;
				badFillAmount = 6.28f;
			}
		}
		else
		{
			if(percentFilled <= 1)
			{
				badOnTop = false;
				fillAmount = (percentFilled)*6.28f;
				badFillAmount = 0;
				badColor = blue;
			}
			else
			{
				badOnTop = true;
				fillAmount = 6.28f;
				badFillAmount = (percentFilled-1)*6.28f;
				badColor = red;
			}
		}

		if(badOnTop)
		{
			r_BadRing.sortingOrder = 1;
		}
		else
		{
			r_BadRing.sortingOrder = -1;
		}
		float visualRadius = radius; //Used for lerpin'
		float visualThickness = thickness; //Used for lerpin'
		if(lerpAllChanges)
		{
			visualRadius = lerpRadius;
			visualThickness = lerpthickness;
		}

		r_Ring.material.SetFloat("_Angle", fillAmount);
		r_Ring.material.SetFloat("_Radius", visualRadius);
		r_Ring.material.SetColor("_Color", color);
		r_Ring.material.SetFloat("_RingWidth", visualThickness);

		r_BadRing.material.SetFloat("_Angle", badFillAmount);
		r_BadRing.material.SetFloat("_Radius", visualRadius);
		r_BadRing.material.SetColor("_Color", badColor);
		r_BadRing.material.SetFloat("_RingWidth", visualThickness);

		Vector3 rot = new Vector3(0,0,rotation + 90);
		this.transform.localEulerAngles = rot;
	}
}
