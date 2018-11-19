using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

[CustomEditor(typeof(Interactable), true)]
public class InteractableHandler : Editor {

	private Interactable myInteractable;
	void Awake() 
	{
		myInteractable = (Interactable)target;
		myInteractable.interactableMat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Shaders/Tex&Mats/Interactable.mat", typeof(Material));
		myInteractable.mySprite = myInteractable.GetComponent<SpriteRenderer>();
		if (myInteractable.mouseEnterEvent == null)
		{
			myInteractable.mouseEnterEvent = new GameObjEvent();
		}
		if (myInteractable.mouseExitEvent == null)
		{
			myInteractable.mouseExitEvent = new GameObjEvent();
		}
		if (myInteractable.mouseDownEvent == null)
		{
			myInteractable.mouseDownEvent = new GameObjEvent();
		}
	}

	void OnEnable()
	{

	}
}
