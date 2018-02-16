using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenFilter : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		if(gameObject.GetComponent<SpriteRenderer>())
		{
			gameObject.GetComponent<SpriteRenderer>().enabled = true;
		}
	}
}
