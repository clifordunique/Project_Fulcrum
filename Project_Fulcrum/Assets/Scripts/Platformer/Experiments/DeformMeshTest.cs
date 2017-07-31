using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformMeshTest : MonoBehaviour {
	Vector2[] pointList;
	// Use this for initialization
	void Start () 
	{
		pointList = this.GetComponent<PolygonCollider2D>().points;
//		for(int i = 0; i<pointList.Length; i++)
//		{
//			print(pointList[i]);
//		}
	}
	
	// Update is called once per frame
	void Update () 
	{

	}

	void FixedUpdate()
	{
		for(int i = 0; i<pointList.Length; i++)
		{
			float moveAmount = 0.01f*Time.fixedDeltaTime;
			pointList[i] = new Vector2(pointList[i].x-moveAmount,pointList[i].y-moveAmount);
			//print("zwoopie!");
		}
		this.GetComponent<PolygonCollider2D>().points = pointList;
	}
}
