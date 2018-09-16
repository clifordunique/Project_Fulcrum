using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

[CustomEditor(typeof(Shoe), true)]
public class ShoeHandler : Editor {

	private Shoe myShoe;

	void Awake() 
	{
		myShoe = (Shoe)target;
	}

	public override void OnInspectorGUI() 
	{
		DrawDefaultInspector();
		if(GUILayout.Button("Set default movement variables."))
		{
			Undo.RecordObject(myShoe, "Set default movement variables.");
			myShoe.m.SetDefaults();
		}
	}
}
