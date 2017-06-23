using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KineticObject : MonoBehaviour {

	public int matterType;
	public Vector2 Vel;

	// Use this for initialization
	void Start () 
	{
		
	}

	// Update is called once per frame
	void Update () 
	{
		
	}

	public bool recieveHit(KineticObject hitter)
	{
		return true;
	}

}
