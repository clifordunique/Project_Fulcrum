using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using System;
using UnityEditor.Events;

[CustomEditor(typeof(Shoe), true)]
public class ShoeHandler : Editor {

	private Shoe myShoe;

	void Awake() 
	{
		myShoe = (Shoe)target;
	}

	void OnEnable()
	{
		SetupEvents();
	}

	void SetupEvents()
	{
		if (myShoe.gameObject.GetComponent<Interactable>() == null)
		{
			myShoe.gameObject.AddComponent<Interactable>();
		}
		myShoe.myInteractable = myShoe.gameObject.GetComponent<Interactable>();

		UnityAction<GameObject> action = new UnityAction<GameObject>(myShoe.Interact);
		myShoe.myInteractable.mouseDownEvent.RemoveAllListeners();
		myShoe.myInteractable.mouseDownEvent.AddListener(action);
		//UnityEventTools.AddObjectPersistentListener<GameObject>(myShoe.myInteractable.mouseDownEvent, action, myShoe.myInteractable.localPlayer);
	}

	public override void OnInspectorGUI() 
	{
		DrawDefaultInspector();
		if(GUILayout.Button("Set default movement variables."))
		{
			Undo.RecordObject(myShoe, "Set default movement variables.");
			myShoe.m.SetDefaults();
		}
		if (GUILayout.Button("Reset event interactions."))
		{
			SetupEvents();
		}
	}
}
