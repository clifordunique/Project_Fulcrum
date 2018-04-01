using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour {
	public float maxLifeTime = 5;
	[ReadOnlyAttribute] private float aliveDuration; 
	private SpriteRenderer myRenderer;

	// Use this for initialization
	void Start () 
	{
		myRenderer = this.gameObject.GetComponentInChildren<SpriteRenderer>();
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
