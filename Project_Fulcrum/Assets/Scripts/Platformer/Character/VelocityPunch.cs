using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityPunch : MonoBehaviour {
	FighterChar myFighter; 
	Transform myParent;
	SpriteRenderer mySprite;
	TrailRenderer myTrailRenderer;
	public bool inUse = false;
	private float punchThreshold;
	private float punchMinThreshold;

	// Use this for initialization
	void Start ()
	{
		myParent = this.transform.root;
		myFighter = myParent.GetComponent<FighterChar>();
		mySprite = this.GetComponent<SpriteRenderer>();
		myTrailRenderer = this.transform.GetComponent<TrailRenderer>();
		punchThreshold = myFighter.m.velPunchT;
		punchMinThreshold = punchThreshold/4;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(!inUse)
		{
			this.transform.localScale = Vector3.zero;
			mySprite.enabled = false;
			myTrailRenderer.Clear();
			return;
		}
		float speed = myFighter.GetSpeed();
		//this.transform.localPosition = -myFighter.GetVelocity().normalized;
		if(speed >= (punchThreshold))
		{
			//print("it's warkin");
			//myTrailRenderer.enabled = true;
			mySprite.enabled = true;

			this.transform.localScale = Vector3.one/2;
		}
		else if(speed >= (punchThreshold-punchMinThreshold))
		{
			//print("it's werkin");
			mySprite.enabled = true;
			float myScale = (speed-(punchThreshold-punchMinThreshold))/punchMinThreshold; //Size is 0 at punchMinThreshold, and 1 at punchThreshold.
			myScale *= 0.5f; // Half size is max, since original sprite is too large.
			myTrailRenderer.enabled = true;
			this.transform.localScale = new Vector3(myScale, myScale, 1); 
		}
		else
		{
			myTrailRenderer.Clear();
			//this.enabled = false;
			mySprite.enabled = false;
			//this.transform.localScale = Vector3.zero;
		}
	}
}
