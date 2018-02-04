using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GrassColourer))]
public class GrassColourerEditor : Editor {

	private GrassColourer myGrassColourer;

	void Awake() 
	{
		myGrassColourer = (GrassColourer)target;
	}


	public override void OnInspectorGUI() 
	{
		DrawDefaultInspector();

		if(GUILayout.Button("Set Colour"))
		{
			myGrassColourer.ApplyChanges();
			EditorUtility.SetDirty(myGrassColourer);
		}

	}
}
