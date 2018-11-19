using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicLogoEffect : MonoBehaviour {
	[SerializeField] private SpriteRenderer mySpriteRenderer;

	// Use this for initialization
	void Start () 
	{
		mySpriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		mySpriteRenderer.material.SetFloat("_Magnitude", ((1+Mathf.Sin(Time.time*2))/2)*10);
	}
}
