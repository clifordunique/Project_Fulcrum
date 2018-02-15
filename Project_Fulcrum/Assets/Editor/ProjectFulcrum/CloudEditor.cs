using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(CloudHandler))]
public class CloudEditor : Editor {

	private CloudHandler myCloudHandler;

	public override void OnInspectorGUI() 
	{
		DrawDefaultInspector();
		if(GUILayout.Button("Generate Cloudlayer"))
		{
			myCloudHandler.GenerateClouds();
		}
		EditorGUILayout.Space();
//		myColor = EditorGUILayout.ColorField("Cutout Color", myColor);
//		sortingOrder = EditorGUILayout.IntField("Sorting Order", sortingOrder);
//		sortingLayer = EditorGUILayout.TextField("Sorting Layer", sortingLayer);
//
		if(GUILayout.Button("Apply cloud changes"))
		{
			myCloudHandler.RefreshCurrent();
			myCloudHandler.UpdateAllClouds();
		}

	}

	void Awake()
	{
		myCloudHandler = (CloudHandler)target;
		//string filePath = "Assets/Materials/CutoutShader2D.mat";
		//cutoutShader2D = (Material)AssetDatabase.LoadAssetAtPath(filePath, typeof(Material));
	}
	 
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
