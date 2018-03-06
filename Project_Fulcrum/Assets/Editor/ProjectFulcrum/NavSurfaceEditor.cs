using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NavSurface))]
public class NavSurfaceEditor : Editor {

	private NavSurface myNavSurface;

	void Awake() 
	{
		myNavSurface = (NavSurface)target;
	}


	public override void OnInspectorGUI() 
	{
		DrawDefaultInspector();
		if(GUILayout.Button("Update Surface"))
		{
			myNavSurface.UpdateSurface();
		}
		EditorGUILayout.Space();
//		sortingOrder = EditorGUILayout.IntField("Sorting Order", sortingOrder);
//		sortingLayer = EditorGUILayout.TextField("Sorting Layer", sortingLayer);
//
//		if(GUILayout.Button("Apply OBJ -> Cutout2D conversion"))
//		{
//			Apply2DCutoutShader();
//		}

	}
}
