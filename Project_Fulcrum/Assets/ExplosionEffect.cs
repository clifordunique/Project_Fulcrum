using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour {
	public float maxLifeTime = 5;
	[ReadOnlyAttribute] private float aliveDuration; 
	private SpriteRenderer renderer;

	// Use this for initialization
	void Start () 
	{
		renderer = this.gameObject.GetComponentInChildren<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(aliveDuration<maxLifeTime)
		{
			aliveDuration += Time.deltaTime;
			if(aliveDuration>1)
			{
				renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 1-(aliveDuration/maxLifeTime));
			}
		}
		else
		{
			Destroy(this.gameObject);
		}
	}
}
