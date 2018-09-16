using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Interactable : MonoBehaviour {

	[SerializeField] UnityEvent mouseEnterEvent;
	[SerializeField] UnityEvent mouseExitEvent;
	[SerializeField] UnityEvent mouseDownEvent;
	[SerializeField] Material interactableMat;

	[SerializeField][ReadOnlyAttribute] private SpriteRenderer mySprite; 

	void OnMouseEnter()
	{
		mouseEnterEvent.Invoke();
		mySprite.color = Color.red;
		//print("MOUSE ENTERED!");
	}

	void OnMouseExit()
	{
		mouseExitEvent.Invoke();
		mySprite.color = Color.white;
		//print("MOUSE EXITED!");

	}

	void OnMouseDown()
	{
		mouseDownEvent.Invoke();
		//print("MOUSE DOWN!");
	}

	// Use this for initialization
	void Start () 
	{
		mouseEnterEvent = new UnityEvent();
		mouseExitEvent = new UnityEvent();
		mouseDownEvent = new UnityEvent();
		mySprite = this.GetComponent<SpriteRenderer>();
	}

	public void SetVisuals(int spriteSize, Color flashColour, Color outlineColour)
	{
		this.mySprite.material.SetInt("_SpriteWidth", spriteSize); 
		this.mySprite.material.SetColor("_FlashColor", flashColour);
		this.mySprite.material.SetColor("_OutlineColor", outlineColour);
	}

	public void SetFlashMagnitude(float mult)
	{
		this.mySprite.material.SetFloat("_FlashMagnitude", mult); 
	}

	// Update is called once per frame
	void Update () 
	{
		
	}
}
