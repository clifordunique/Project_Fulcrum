using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

[CustomEditor(typeof(Interactable), true)]
public class InteractableHandler : Editor {

	private Interactable myInteractable;
	void OnEnable() 
	{
		myInteractable = (Interactable)target;
		myInteractable.mySprite = myInteractable.GetComponent<SpriteRenderer>();
		myInteractable.highlightMat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Shaders/Tex&Mats/Interactable.mat", typeof(Material));
		myInteractable.originalMat = myInteractable.mySprite.sharedMaterial;

		Debug.Log("My sprite size: " + (int)myInteractable.mySprite.sprite.rect.x);
		myInteractable.mySpriteSize = (int)myInteractable.mySprite.sprite.rect.x;

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

	//void OnEnable()
	//{

	//}
}
