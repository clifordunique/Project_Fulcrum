using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AK.Wwise;

public class ExplosionEffect : MonoBehaviour {
	public float maxLifeTime = 5;
	[ReadOnlyAttribute] private float aliveDuration; 
	private SpriteRenderer myRenderer;
	[SerializeField]private AK.Wwise.Event e_Crater = null;
	public float craterForce; // Force involved in the impact. Set by the creator of the explosion upon creation, or left at 0, which results in a silent effect.

	// Use this for initialization
	void Start () 
	{
		myRenderer = this.gameObject.GetComponentInChildren<SpriteRenderer>();
		CraterSound();
	}

	public void CraterSound()
	{
		e_Crater.Post(this.gameObject);
		AkSoundEngine.SetRTPCValue("GForce_Instant", craterForce, this.gameObject);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(aliveDuration<maxLifeTime)
		{
			aliveDuration += Time.deltaTime;
			if(aliveDuration>1)
			{
				myRenderer.color = new Color(myRenderer.color.r, myRenderer.color.g, myRenderer.color.b, 1-(aliveDuration/maxLifeTime));
			}
		}
		else
		{
			Destroy(this.gameObject);
		}
	}
}
