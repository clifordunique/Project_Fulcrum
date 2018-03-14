using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NavBuilderRay))]
public class NavBuilderRayEditor : Editor {

	private NavBuilderRay myNavBuilderRay;

	void Awake() 
	{
		myNavBuilderRay = (NavBuilderRay)target;
	}


	public override void OnInspectorGUI() 
	{
		DrawDefaultInspector();
		if(GUILayout.Button("Build NavSurface Step by Step"))
		{
			myNavBuilderRay.ManualGenByStep();
		}
		EditorGUILayout.Space();
	}
}
