using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityPunch : MonoBehaviour {
	FighterChar myFighter; 
	Transform myParent;
	TrailRenderer myTrailRenderer;
	public bool inUse = false;

	// Use this for initialization
	void Start () 
	{
		myParent = this.transform.parent;
		myFighter = myParent.GetComponent<FighterChar>();
		myTrailRenderer = this.transform.GetComponent<TrailRenderer>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(!inUse)
		{
			this.transform.localScale = Vector3.zero;
			myTrailRenderer.Clear();
			return;
		}
		float speed = myFighter.GetSpeed();
		this.transform.localPosition = -myFighter.GetVelocity().normalized;
		if(speed >= 80)
		{
			//myTrailRenderer.enabled = true;
			this.transform.localScale = Vector3.one;
		}
		else if(speed > 70)
		{
			//myTrailRenderer.enabled = true;
			this.transform.localScale = new Vector3((speed-70)/10, (speed-70)/10, 1);
		}
		else
		{
			myTrailRenderer.Clear();
			//this.enabled = false;
			this.transform.localScale = Vector3.zero;
		}
	}
}
