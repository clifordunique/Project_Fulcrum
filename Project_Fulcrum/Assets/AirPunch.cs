using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirPunch : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public void Complete()
	{
		//print("Punch ended, destroying airpunch object.");
		//print("Before I go, here is my parent object:"+this.transform.parent.transform.parent.gameObject);
		Destroy(this.transform.parent.gameObject);
	}
}
