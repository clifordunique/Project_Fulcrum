using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
//RequireComponent(typeof(TextMesh))]
public class NavSurface : MonoBehaviour {

	//
	//Visualization variables
	//
	public bool isVisible;
	public GameObject p_ConnectionLinePrefab;
	public LineRenderer[] connectionLines;
	private LineRenderer surfVis; // Surface Visualizer

	//
	// Nav data variables
	//
	[SerializeField] private int surfaceType; // 0 for ground, 1 for ceiling, 2 for leftwall, 3 for rightwall. 
	[ReadOnlyAttribute] public float totalLength;
	[ReadOnlyAttribute] public int id;
	public NavNode[] nodes;
	public NavConnection[] navCon; // Connection data object that carries information of a connection between surfaces.

	void Update()
	{
		if(transform.hasChanged&&Application.isEditor)
		{
			UpdateSurface();
		}
	}

	// Use this for initialization
	void Start () 
	{
		UpdateSurface();
	}


	public void UpdateSurface()
	{
		//print("Updated Surface.");
		surfVis = this.GetComponent<LineRenderer>();

		LineRenderer[] oldLines = transform.GetComponentsInChildren<LineRenderer>();
		foreach(LineRenderer lr in oldLines)
		{
			if(lr!=null)
			{
				if(lr.gameObject.name=="NavConnectionVisualizer(Clone)")
					DestroyImmediate(lr.gameObject);	
			}
		}

		connectionLines = new LineRenderer[navCon.Length];

		foreach(NavConnection nc in navCon)
		{
			nc.orig = this;
			nc.traversaltimeout = 5; // placeholder

			if(nc.exitVelRange>0)
			{
				nc.minExitVel = nc.exitVel.x-nc.exitVelRange;
				nc.maxExitVel = nc.exitVel.x+nc.exitVelRange;
			}
			else
			{
				nc.minExitVel = nc.exitVel.x;
				nc.maxExitVel = nc.exitVel.x;
			}
		}

		for(int i = 0; i<connectionLines.Length; i++)
		{
			GameObject newConLine = (GameObject)Instantiate(p_ConnectionLinePrefab, transform);
			connectionLines[i] = newConLine.GetComponent<LineRenderer>();
			connectionLines[i].positionCount = 2;
			connectionLines[i].SetPosition(0, LinToWorldPos(navCon[i].exitPosition));
			connectionLines[i].SetPosition(1, navCon[i].dest.LinToWorldPos(navCon[i].destPosition));

			connectionLines[i].startColor = new Color(0,1,0,0.1f);
			connectionLines[i].endColor = new Color(0, 1, 0, 0.1f);
		}
		totalLength = 0;
		for(int i = 0; i<nodes.Length-1; i++)
		{
			totalLength += (nodes[i].transform.position-nodes[i+1].transform.position).magnitude;
		}

		if(surfVis!=null)
		{
			if(surfaceType<=1)
			{
				surfVis.startColor = new Color(0, 1, 1, 0.3f);
				surfVis.endColor = new Color(0, 1, 1, 0.3f);
			}
			else
			{
				surfVis.startColor = Color.red;
				surfVis.endColor = Color.red;
			}

			surfVis.positionCount = nodes.Length;
			for(int i = 0; i<nodes.Length; i++)
			{
				surfVis.SetPosition(i, nodes[i].transform.localPosition);
			}
		}

		SetVisible(isVisible);
	}

	public Vector2 LinToWorldPos(float linPos) // Converts distance along surface line to world position.
	{
		Vector2 lineVec = nodes[1].transform.position-nodes[0].transform.position;
		return ((Vector2)nodes[0].transform.position)+(lineVec*(linPos/totalLength));
	}

	public float WorldToLinPos(Vector2 worldPos) // Converts world position to distance along surface line. 
	{
		Vector2 lineVec = nodes[1].transform.position-nodes[0].transform.position;
		Vector2 fighterVec = worldPos-(Vector2)nodes[0].transform.position;
		float dist = Proj(fighterVec, lineVec).magnitude;
		return dist;
	}

	public float DistFromLine(Vector2 worldPos) // Converts world position to distance from the surface line
	{
		Vector2 lineVec = nodes[1].transform.position-nodes[0].transform.position;
		Vector2 fighterVec = worldPos-(Vector2)nodes[0].transform.position;
		Vector2 proj = Proj(fighterVec, lineVec);
		float dist = (fighterVec-proj).magnitude;

		if( Vector2.Dot(lineVec, fighterVec)<0 ) // If beyond the first endpoint, measure distance to first endpoint instead.
			dist = ((Vector2)nodes[0].transform.position-worldPos).magnitude;
		
		if( proj.magnitude>lineVec.magnitude ) // If beyond the second endpoint, measure distance to second endpoint instead.
			dist = ((Vector2)nodes[1].transform.position-worldPos).magnitude;

		return dist;
	}

	protected Vector2 Proj(Vector2 A, Vector2 B) //Projects vector A onto vector B.
	{
		float component = Vector2.Dot(A,B)/B.magnitude;
		return component*B.normalized;
	}	

	public void SetVisible(bool yes)
	{
		if(yes)
		{
			surfVis.enabled = true;
			foreach(LineRenderer lr in connectionLines)
			{
				lr.enabled = true;
			}
		}
		else
		{
			surfVis.enabled = false;
			for(int i = 0; i<connectionLines.Length; i++)
			{
				connectionLines[i].enabled = false;
			}
		}
	}
}

[System.Serializable] public class NavPath : IComparable<NavPath>
{
	[SerializeField][ReadOnlyAttribute] public NavConnection[] edges; // Ordered list of the edges to traverse to get to the destination.
	[SerializeField][ReadOnlyAttribute] public float totalWeight; // How expensive is this path to traverse? Measured in time cost.
	[SerializeField][ReadOnlyAttribute] public int totalDepth; // How many edges in the path. In a literal sense, how many surface changes will it take.

	public int CompareTo(NavPath np)
	{
		if(np==null)
		{
			return 1;
		}
		else
		{
			return this.totalWeight.CompareTo(np.totalWeight);
		}
	}
}

[System.Serializable] public class NavConnection
{
	[SerializeField] public float edgeWeight = 1; 		// Edge weight for use in pathfinding algorithms.
	[SerializeField] public NavSurface orig; 		// Origin surface.
	[SerializeField] public NavSurface dest; 		// Destination surface.
	[SerializeField] public float exitPosition; 	// Position on current surface to leave from.
	[SerializeField] public float exitPositionRange = 0.5f; // Allowed variance in linear position before traversing.

	[SerializeField] public float destPosition;  	// Position on destination surface to aim for.

	[SerializeField] public Vector2 exitVel;  	// Exit velocity to reach the next platform. Range widened by exitVelRange.
	[SerializeField] public float exitVelRange; // Determines how close the fighter must be to exitVel to traverse surfaces.

	[SerializeField] public float minExitVel;  // Min speed to leave current surface. Overrides exitVel and exitVelRange if set.
	[SerializeField] public float maxExitVel;  // Max speed to leave current surface. Overrides exitVel and exitVelRange if set.

	[SerializeField] public int traverseType;  // Type of movement used to traverse between surfaces.
	[SerializeField][ReadOnlyAttribute] public int failedTraversals;  // Amount of times NPCs failed to move to the next surface using this connection.
	[SerializeField][ReadOnlyAttribute] public int successfulTraversals;  // Amount of times NPCs succeeded using this connection.
	[SerializeField] public float traversaltimeout = 5;  // Max time in seconds the NPCs can try to traverse before deeming the attempt a failure.
	[SerializeField][ReadOnlyAttribute] public float averageTraversalTime;  // Average time it takes NPCs to traverse this connection. 

}
