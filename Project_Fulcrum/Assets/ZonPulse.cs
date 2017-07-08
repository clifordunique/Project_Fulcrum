using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZonPulse : MonoBehaviour {
	
	public Player originPlayer;
	[ReadOnlyAttribute][SerializeField] float pulseRadius;
	[ReadOnlyAttribute][SerializeField] public float pulseRange = 40;

	// Use this for initialization
	void Start () 
	{
		pulseRadius = 0.01f;
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		this.transform.localScale = new Vector3(pulseRadius,pulseRadius,1);
		pulseRadius += Time.fixedDeltaTime*60;
		if(pulseRadius >= pulseRange)
		{
			Complete();
		}
	}

	public void Complete()
	{
		Destroy(this.transform.parent.gameObject);
	}

	void OnTriggerEnter2D(Collider2D theObject)
	{
		FighterChar theFighter = null;
		if(theObject.gameObject != null)
		{
			theFighter = theObject.gameObject.GetComponent<FighterChar>();
		}
		if((theFighter != null)&&(theFighter!=originPlayer))
		{
			originPlayer.PulseHit(theFighter);
		}
	}
}
