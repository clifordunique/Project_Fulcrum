using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityPunch : MonoBehaviour {
	FighterChar myFighter; 
	Transform myParent;
	TrailRenderer myTrailRenderer;
	public bool inUse = false;
	private float punchThreshold;

	// Use this for initialization
	void Start () 
	{
		myParent = this.transform.parent;
		myFighter = myParent.GetComponent<FighterChar>();
		myTrailRenderer = this.transform.GetComponent<TrailRenderer>();
		punchThreshold = myFighter.m_VelPunchT;
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
		if(speed >= (punchThreshold+10))
		{
			//myTrailRenderer.enabled = true;
			this.transform.localScale = Vector3.one;
		}
		else if(speed > punchThreshold)
		{
			//myTrailRenderer.enabled = true;
			this.transform.localScale = new Vector3((speed-punchThreshold)/10, (speed-punchThreshold)/10, 1);
		}
		else
		{
			myTrailRenderer.Clear();
			//this.enabled = false;
			this.transform.localScale = Vector3.zero;
		}
	}
}
