using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NavMaster : MonoBehaviour {

	[SerializeField]private NavSurface[] surfaceList;
	[SerializeField]private int connectionCount;
	[SerializeField]public bool setAllVisible;
	[SerializeField]public NavPath[] testNavPathList;
	[SerializeField]public float proximityJoinT = 0.01f; // Distance at which navsurfaces will automatically link together with walking connections since they're adjacent.

	//[SerializeField]private NavConnection[] adjacencyList;

	// Use this for initialization
	void Start () 
	{
		UpdateAllSurfaces();
	}

	public NavPath[] GetVelocityAwarePathList(int startID, int endID) // Modified Prim's algorithm. Returns a list of paths, sorted from quickest to slowest.
	{
		float totalWeight = 0;
		List<NavPath> pathList = new List<NavPath>();
		float[] nodeWeight = new float[surfaceList.Length];;
		int[] nodeDepth = new int[surfaceList.Length];;

		bool pathFound = false;

		bool[] nodeVisited = new bool[surfaceList.Length];
		int[] nodeVisitCount = new int[surfaceList.Length];
		NavConnection[] previousEdge = new NavConnection[surfaceList.Length];
		nodeVisited[startID] = true;
		int untouchedNodes = surfaceList.Length;
		int activeNode = startID;
		List<NavConnection> edgeList = new List<NavConnection>();
		List<NavConnection> treeEdgeList = new List<NavConnection>();

		if(startID==endID)
		{
			print("Destination is the same as origin!");
			return null;
		}

		if(surfaceList[startID]==null)
		{
			print("Invalid surface ID!");
			return null;
		}

		if(surfaceList[startID].navCon.Length==0)
		{
			print("Surface has no connections!");
			return null;
		}

		foreach(NavConnection nc in surfaceList[startID].navCon)
		{
			edgeList.Add(nc);
		}
		while(edgeList.Count > 0)
		{

			string testList = "Current edgelist: \n";

			foreach(NavConnection nc in edgeList)
			{
				testList += "["+nc.orig.id+"->"+nc.dest.id+"] ";
			}

			//print(testList);

			NavConnection currentEdge = edgeList[0];

			if(nodeVisited[currentEdge.orig.id] && currentEdge.dest.id==endID)
			{
				pathFound = true;
				//print("Navconnection: ["+ currentEdge.orig+"->"+currentEdge.dest+"] successful");
				previousEdge[currentEdge.dest.id] = currentEdge;
				treeEdgeList.Add(currentEdge); 

				nodeWeight[currentEdge.dest.id] = nodeWeight[currentEdge.orig.id]+currentEdge.edgeWeight;
				nodeDepth[currentEdge.dest.id] = nodeDepth[currentEdge.orig.id]+1;


				pathList.Add(SaveNavPath(previousEdge, startID, endID, nodeWeight[endID], nodeDepth[endID]));
				edgeList.RemoveAt(0);
				continue;
			}

			if(nodeVisited[currentEdge.orig.id] && nodeVisitCount[currentEdge.dest.id] < currentEdge.dest.navCon.Length) // If origin has been visited and destination has been visited fewer times than it has outgoing edges.
			{
				//print("Navconnection: ["+ currentEdge.orig+"->"+currentEdge.dest+"] successful");
				nodeVisited[currentEdge.dest.id] = true;
				previousEdge[currentEdge.dest.id] = currentEdge;
				treeEdgeList.Add(currentEdge); 

				nodeWeight[currentEdge.dest.id] = nodeWeight[currentEdge.orig.id]+currentEdge.edgeWeight;
				nodeDepth[currentEdge.dest.id] = nodeDepth[currentEdge.orig.id]+1;

				foreach(NavConnection nc in surfaceList[currentEdge.dest.id].navCon)
				{
					//print("Added edge ["+nc.orig.id+"->"+nc.dest.id+"] to list.");
					edgeList.Add(nc);
				}
			}
			else
			{
				if(!nodeVisited[currentEdge.orig.id])
				{
					//print("Navconnection: ["+ currentEdge.orig+"->"+currentEdge.dest+"] origin not yet visited.");
				}
				if(nodeVisited[currentEdge.dest.id])
				{
					//print("Navconnection: ["+ currentEdge.orig+"->"+currentEdge.dest+"] destination already visited.");
				}
			}
			edgeList.RemoveAt(0);
		}

		if(!pathFound)
		{ 
			print("No suitable path found.");
			return null;
		}
		pathList.Sort();
		return pathList.ToArray();
	}
		
	public NavPath[] GetPathList(int startID, int endID) // Modified Prim's algorithm. Returns a list of paths, sorted from quickest to slowest.
	{
		float totalWeight = 0;
		List<NavPath> pathList = new List<NavPath>();
		float[] nodeWeight = new float[surfaceList.Length];;
		int[] nodeDepth = new int[surfaceList.Length];;

		bool pathFound = false;

		bool[] nodeVisited = new bool[surfaceList.Length];
		NavConnection[] previousEdge = new NavConnection[surfaceList.Length];
		nodeVisited[startID] = true;
		int untouchedNodes = surfaceList.Length;
		int activeNode = startID;
		List<NavConnection> edgeList = new List<NavConnection>();
		List<NavConnection> treeEdgeList = new List<NavConnection>();

		if(startID==endID)
		{
			print("Destination is the same as origin!");
			return null;
		}

		if(surfaceList[startID]==null)
		{
			print("Invalid surface ID!");
			return null;
		}

		if(surfaceList[startID].navCon.Length==0)
		{
			print("Surface has no connections!");
			return null;
		}

		foreach(NavConnection nc in surfaceList[startID].navCon)
		{
			edgeList.Add(nc);
		}
		while(edgeList.Count > 0)
		{

			string testList = "Current edgelist: \n";

			foreach(NavConnection nc in edgeList)
			{
				testList += "["+nc.orig.id+"->"+nc.dest.id+"] ";
			}

			//print(testList);

			NavConnection currentEdge = edgeList[0];

			if(nodeVisited[currentEdge.orig.id] && currentEdge.dest.id==endID)
			{
				pathFound = true;
				//print("Navconnection: ["+ currentEdge.orig+"->"+currentEdge.dest+"] successful");
				previousEdge[currentEdge.dest.id] = currentEdge;
				treeEdgeList.Add(currentEdge); 

				nodeWeight[currentEdge.dest.id] = nodeWeight[currentEdge.orig.id]+currentEdge.edgeWeight;
				nodeDepth[currentEdge.dest.id] = nodeDepth[currentEdge.orig.id]+1;


				pathList.Add(SaveNavPath(previousEdge, startID, endID, nodeWeight[endID], nodeDepth[endID]));
				edgeList.RemoveAt(0);
				continue;
			}

			if(nodeVisited[currentEdge.orig.id] && !nodeVisited[currentEdge.dest.id])
			{
				//print("Navconnection: ["+ currentEdge.orig+"->"+currentEdge.dest+"] successful");
				nodeVisited[currentEdge.dest.id] = true;
				previousEdge[currentEdge.dest.id] = currentEdge;
				treeEdgeList.Add(currentEdge); 

				nodeWeight[currentEdge.dest.id] = nodeWeight[currentEdge.orig.id]+currentEdge.edgeWeight;
				nodeDepth[currentEdge.dest.id] = nodeDepth[currentEdge.orig.id]+1;

				foreach(NavConnection nc in surfaceList[currentEdge.dest.id].navCon)
				{
					//print("Added edge ["+nc.orig.id+"->"+nc.dest.id+"] to list.");
					edgeList.Add(nc);
				}
			}
			else
			{
				if(!nodeVisited[currentEdge.orig.id])
				{
					//print("Navconnection: ["+ currentEdge.orig+"->"+currentEdge.dest+"] origin not yet visited.");
				}
				if(nodeVisited[currentEdge.dest.id])
				{
					//print("Navconnection: ["+ currentEdge.orig+"->"+currentEdge.dest+"] destination already visited.");
				}
			}
			edgeList.RemoveAt(0);
		}

		if(!pathFound)
		{ 
			print("No suitable path found.");
			return null;
		}
		pathList.Sort();
		return pathList.ToArray();
	}

	private NavPath SaveNavPath(NavConnection[] previousEdge, int startID, int endID, float pathWeight, int pathDepth)
	{
		string output = "";
		int currentPoint = endID;

		NavPath outputPath = new NavPath();
		outputPath.edges = new NavConnection[pathDepth];
		outputPath.totalWeight = pathWeight;
		outputPath.totalDepth = pathDepth;

		if(pathDepth>100)
		{
			print("Path greater than 100 links! Discarding.");
			return null;
		}

		if(pathDepth==0)
		{
			print("No path at all!");
		}

		for(int i = pathDepth-1; i >= 0; i--)
		{
			if(previousEdge[currentPoint]!=null)
			{
				outputPath.edges[i] = previousEdge[currentPoint];
				output = previousEdge[currentPoint].orig.id+"-->"+previousEdge[currentPoint].dest.id+", "+output; 
				currentPoint = previousEdge[currentPoint].orig.id;
			}
			if(currentPoint==startID)
			{ 
				break;
			}
		}
		output = "Path confirmed: "+output+" | Total path weight: "+pathWeight;
		print(output);
		return outputPath;
	}
		
	public NavSurface GetSurface(int id)
	{
		return surfaceList[id];
	}

	public NavSurface[] GetSurfaces()
	{
		return surfaceList;
	}

	public void UpdateAllSurfaces()
	{
		surfaceList = FindObjectsOfType<NavSurface>();
		int i = 0;
		foreach(NavSurface ns in surfaceList)
		{
			ns.gameObject.name = "NavSurf["+i+"]";
			ns.transform.parent = this.transform;
			ns.id = i;
			ns.isVisible = setAllVisible;
			ns.UpdateSurface();
			i++;
		}
		connectionCount = 0;
		foreach(NavSurface ns in surfaceList)
		{
			connectionCount += ns.navCon.Length;
		}
	}

	public void GenerateAllAdjacencyConnections()
	{
		UpdateAllSurfaces();
		foreach(NavSurface ns in surfaceList)
		{
			ns.GenerateAdjacencyConnections();
		}
	}


	// Update is called once per frame
	void Update () {
		
	}
}


