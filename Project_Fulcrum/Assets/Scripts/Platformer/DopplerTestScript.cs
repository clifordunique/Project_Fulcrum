using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DopplerTestScript : MonoBehaviour {
	protected Rigidbody2D o_Rigidbody2D;	
	// Use this for initialization
	void Start () 
	{
		o_Rigidbody2D = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (o_Rigidbody2D.position.y <= -1500) 
		{
			o_Rigidbody2D.position = new Vector2(o_Rigidbody2D.position.x, 2000);
		}
	}
}
