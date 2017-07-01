using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour {
	private Renderer r_Grass;
	[SerializeField]float shakeWidth = 0.2f;		// How far left and right the blades will drift naturally.
	[SerializeField]float windForce = 1;			// Current wind direction. When unset, drifts back to default.
	[SerializeField]float windForceDefault = 1; 	// Default wind direction. Negative is left, Positive is right.
	[SerializeField]float defaultRandomOffset = 0.2f;// Random variance in base position. Changes windforceDefault at the start.
	[SerializeField]float shakeSpeed = 1;			// How frequently the blades flap back and forth.
	[SerializeField]float temporalOffset = 0;		// Offsets the waving motion so they don't all wave in sync which looks unnatural.
	[SerializeField]float temporalOffsetM = 1;		// Changes the width of the offset, resulting in bigger or smaller "waves" of grass.
	[SerializeField]float springBackSpeed = 2;		// How fast the grass returns to its default position after being moved.
	[SerializeField][ReadOnlyAttribute]FighterChar fighterChar;

	// Use this for initialization
	void Start () 
	{
		r_Grass = this.GetComponent<Renderer>();
		//temporalOffset = (this.transform.position.x%6.28f)*temporalOffsetM;
		temporalOffset = this.transform.position.x*temporalOffsetM;
		windForceDefault += UnityEngine.Random.Range(-defaultRandomOffset,defaultRandomOffset);
	}

	void OnTriggerEnter2D(Collider2D theObject)
	{
		FighterChar thePlayer = theObject.gameObject.GetComponent<FighterChar>();

		if(thePlayer != null)
		{
			fighterChar = thePlayer;
			thePlayer.g_IsInGrass++;
			//thePlayer.FighterState.DevMode = true;
		}
	}

	void OnTriggerExit2D(Collider2D theObject)
	{
		FighterChar thePlayer = theObject.gameObject.GetComponent<FighterChar>();

		if(thePlayer != null)
		{
			if(thePlayer == fighterChar)
			{
				thePlayer.g_IsInGrass--;
				fighterChar = null;
			}
		}
	}

	// Update is called once per frame
	void Update () 
	{
		if(fighterChar!=null)
		{
			if(Mathf.Abs(fighterChar.GetVelocity().x) >= 0.6f)
			{
				windForce = -fighterChar.GetVelocity().x;
				if(windForce > 2){windForce = 2;}
				if(windForce < -2){windForce = -2;}
			}
		}
		else
		{
			if(Mathf.Abs(windForce-windForceDefault) <= 0.01f)
			{
				windForce = windForceDefault;
			}
			else
			{
				windForce += (windForceDefault-windForce)*Time.fixedDeltaTime*springBackSpeed;
			}
		}
		//float swish = UnityEngine.Random.Range(-1f,1f);
		float timeSin = Mathf.Sin((Time.time+temporalOffset)*shakeSpeed);
		//print("timeSin"+timeSin);
		float shakeAmount = shakeWidth*timeSin;
		//print("shakeAmount"+shakeAmount);
		float grassBend = windForce + shakeAmount;
		//print(grassBend);
		r_Grass.material.SetFloat("_WindForce", grassBend);
	}
}
