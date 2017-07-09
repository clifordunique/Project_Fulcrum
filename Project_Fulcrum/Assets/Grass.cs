using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour {
	private Renderer r_Grass;
	[SerializeField]float shakeWidthDefault = 0.2f;	// How far left and right the blades will drift naturally.
	[SerializeField]float shakeSpeedDefault = 1;	// How frequently the blades flap back and forth naturally.
	[SerializeField]float shakeWidth = 0;			// How far left and right the blades will drift when blown.
	[SerializeField]float shakeSpeed = 0;			// How frequently the blades flap back and forth when blown.
	[SerializeField]float windForce = 1;			// Current wind direction. When unset, drifts back to default.
	[SerializeField]float windForceDefault = 1; 	// Default wind direction. Negative is left, Positive is right.
	[SerializeField]float defaultRandomOffset = 0.2f;// Random variance in base position. Changes windforceDefault at the start.
	[SerializeField]float temporalOffset = 0;		// Offsets the waving motion so they don't all wave in sync which looks unnatural.
	[SerializeField]float temporalOffsetM = 1;		// Changes the width of the offset, resulting in bigger or smaller "waves" of grass.
	[SerializeField]float springBackSpeed = 2;		// How fast the grass returns to its default position after being moved.
	[SerializeField][ReadOnlyAttribute]FighterChar fighterChar;
	[SerializeField][ReadOnlyAttribute]WindEffector windEffector;


	// Use this for initialization
	void Start () 
	{
		r_Grass = this.GetComponent<Renderer>();
		//temporalOffset = (this.transform.position.x%6.28f)*temporalOffsetM;
		temporalOffset = this.transform.position.x*temporalOffsetM;
		windForceDefault += UnityEngine.Random.Range(-defaultRandomOffset,defaultRandomOffset);

		//this.transform.localPosition = new Vector3(this.transform.localPosition.x+Random.Range(-0.1f,0.1f),0,0);
	}

	void OnTriggerEnter2D(Collider2D theObject)
	{
		FighterChar thePlayer = theObject.gameObject.GetComponent<FighterChar>();
		WindEffector theWind =  theObject.gameObject.GetComponent<WindEffector>();
		if(thePlayer != null)
		{
			fighterChar = thePlayer;
			thePlayer.g_IsInGrass++;
		}
		else if(theWind != null)
		{
			windEffector = theWind;
		}
	}

	void OnTriggerExit2D(Collider2D theObject)
	{
		FighterChar thePlayer = theObject.gameObject.GetComponent<FighterChar>();
		WindEffector theWind =  theObject.gameObject.GetComponent<WindEffector>();

		if(thePlayer != null)
		{
			if(thePlayer == fighterChar)
			{
				thePlayer.g_IsInGrass--;
				fighterChar = null;
			}
		}
		else if(theWind != null)
		{
			windEffector = null;
		}
	}

	// Update is called once per frame
	void Update () 
	{
		if(windEffector!=null)
		{
			if(windEffector.blowDirection == Vector2.zero)
			{
				Vector2 trueBlowDirection = windEffector.transform.position-this.transform.position;
				if(trueBlowDirection.x>0)
				{
					windForce = 2*(windEffector.g_Intensity/200);
				}
				else
				{
					windForce = -2*(windEffector.g_Intensity/200);
				}
				shakeSpeed = 50*((windEffector.g_Intensity-100)/300);
				if(shakeSpeed < 0){shakeSpeed=0;}
				if(shakeSpeed > 50){shakeSpeed=50;}
				if(windEffector.g_WindType == 0)
				{
					windEffector = null;
				}
			}
		}
		else if(fighterChar!=null)
		{
			shakeSpeed = 0;
			shakeWidth = 0;
			if(Mathf.Abs(fighterChar.GetVelocity().x) >= 0.6f)
			{
				windForce = -fighterChar.GetVelocity().x;
				if(windForce > 2){windForce = 2;}
				if(windForce < -2){windForce = -2;}
			}
		}
		else
		{
			shakeSpeed = 0;
			shakeWidth = 0;
			if(Mathf.Abs(windForce) <= 0.01f)
			{
				windForce = 0;
			}
			else
			{
				//windForce -= Time.fixedDeltaTime*springBackSpeed*Mathf.Sign(windForce);
				windForce -= windForce*Time.fixedDeltaTime*springBackSpeed;
				//windForce += (windForceDefault-windForce)*Time.fixedDeltaTime*springBackSpeed;
			}
//			if(Mathf.Abs(windForce-windForceDefault) <= 0.01f)
//			{
//				windForce = windForceDefault;
//			}
//			else
//			{
//				windForce += (windForceDefault-windForce)*Time.fixedDeltaTime*springBackSpeed;
//			}
		}
		float timeSin = Mathf.Sin((Time.time+temporalOffset)*(shakeSpeedDefault+shakeSpeed));
		//print("timeSin"+timeSin);
		float shakeAmount = (shakeWidthDefault+shakeWidth)*timeSin;
		//print("shakeAmount"+shakeAmount);
		float grassBend = windForce + windForceDefault;
		if(grassBend > 2){grassBend = 2;}
		if(grassBend < -2){grassBend = -2;}
		grassBend += shakeAmount;
		//print(grassBend);
		r_Grass.material.SetFloat("_WindForce", grassBend);
	}
}
