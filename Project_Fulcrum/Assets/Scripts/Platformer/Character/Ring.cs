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
	[SerializeField][Range(0,0.1f)] public float thickness = 0; 		// Width of ring.
	[SerializeField][Range(0,6.28f)] private float fillAmount = 0; 		// How much the ring is filled, in radians. 0 to 6.28.
	[SerializeField][Range(0,6.28f)] private float badFillAmount = 0; 	// How much the badring is filled, in radians. 0 to 6.28.
	[SerializeField] public bool badOnTop = false; 						// Sets which colour ring sits on top.
	[SerializeField] public bool lerpAllChanges = false; 				// Sets whether to move instantly or lerp towards real values.
	[SerializeField] public float lerpRadius = 0; 						// Transitional radius. Moves toward real value.
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

	public void setPercentFull(float percent)
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

	public float getPercentFull()
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

	public float getPercentWhite()
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
		if(radius-lerpRadius > 0.001f)
		{
			lerpRadius += (radius-lerpRadius)/8;
		}
		else
		{
			lerpRadius = radius;
		}
	}

	public void UpdateVisuals()
	{
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

		if(lerpAllChanges)
		{
			visualRadius = lerpRadius;
		}

		r_Ring.material.SetFloat("_Angle", fillAmount);
		r_Ring.material.SetFloat("_Radius", visualRadius);
		r_Ring.material.SetColor("_Color", color);
		r_Ring.material.SetFloat("_RingWidth", thickness);

		r_BadRing.material.SetFloat("_Angle", badFillAmount);
		r_BadRing.material.SetFloat("_Radius", visualRadius);
		r_BadRing.material.SetColor("_Color", badColor);
		r_BadRing.material.SetFloat("_RingWidth", thickness);

		Vector3 rot = new Vector3(0,0,rotation + 90);
		this.transform.localEulerAngles = rot;
	}
}
