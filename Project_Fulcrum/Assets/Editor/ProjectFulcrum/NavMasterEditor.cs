using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NavMaster))]
public class NavMasterEditor : Editor {

	private NavMaster myNavMaster;
	private int testNum1;
	private int testNum2;

	void Awake() 
	{
		myNavMaster = (NavMaster)target;
	}


	public override void OnInspectorGUI() 
	{
		DrawDefaultInspector();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		testNum1 = EditorGUILayout.IntField("Testing Start Node", testNum1);
		testNum2 = EditorGUILayout.IntField("Testing End Node", testNum2);
		if(GUILayout.Button("Test weighted all-paths pathfinder."))
		{
			myNavMaster.testNavPathList = myNavMaster.GetPathList(testNum1, testNum2);
		}
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		if(GUILayout.Button("Update all surfaces"))
		{
			Debug.Log("Updating surfaces.");
			myNavMaster.UpdateAllSurfaces();
		}
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		if(GUILayout.Button("Generate All Adjacency Connections"))
		{
			Debug.Log("Generating Adjacency Connections");
			myNavMaster.GenerateAllAdjacencyConnections();
		}

	}
}
