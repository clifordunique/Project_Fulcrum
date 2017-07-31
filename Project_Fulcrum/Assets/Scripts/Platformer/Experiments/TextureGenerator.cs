using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureGenerator : MonoBehaviour 
{
	Vector2[] pointList;
	// Use this for initialization
	void Start () 
	{
		pointList = this.GetComponent<PolygonCollider2D>().points;
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
}
