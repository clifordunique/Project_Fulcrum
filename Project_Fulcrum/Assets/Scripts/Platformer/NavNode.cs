using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavNode : MonoBehaviour {

	public NavNode[] adj; // Adjacent node list. 
	public int[] connectionType; // 0 is walkable, 1 is jumpable
	public float[] avgTravelTime; // How long it takes to move to that adjacent node from this one.

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
}
