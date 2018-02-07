using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndoorFader : MonoBehaviour {
	SpriteRenderer[] sprites;
	private bool isPlayerIndoors = false;
	[SerializeField]private bool invertOpacity = false; // When true, this object becomes visible when the player enters it, rather than becoming invisible
	private float transitionTime = 0;
	private float MaxDuration = 2;
	private float opacity = 1;

	// Use this for initialization
	void Start () 
	{
		sprites = this.GetComponentsInChildren<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(transitionTime >0)
		{
			transitionTime -= Time.deltaTime;
		}

		if(transitionTime == 0){return;}

		if(transitionTime<0){transitionTime = 0;}

		if(isPlayerIndoors)
		{
			opacity = transitionTime*2;
			foreach (SpriteRenderer sp in sprites)
			{
				Color newColor = new Color(sp.color.r,sp.color.g,sp.color.b,opacity);
				sp.color = newColor;
			}
		}
		else
		{
			opacity = 1-(transitionTime*2);
			foreach (SpriteRenderer sp in sprites)
			{
				Color newColor = new Color(sp.color.r,sp.color.g,sp.color.b,opacity);
				sp.color = newColor;
			}
		}
	}

	void OnTriggerEnter2D(Collider2D theObject)
	{
		FighterChar thePlayer = theObject.gameObject.GetComponent<FighterChar>();

		if((thePlayer != null)&&(thePlayer.isLocalPlayer))
		{
			Debug.Log("Player went indoors.");
			isPlayerIndoors = true;
			transitionTime = 0.5f;
			if(invertOpacity)
			{
				isPlayerIndoors = !isPlayerIndoors;
			}
		}
	}

	void OnTriggerExit2D(Collider2D theObject)
	{
		FighterChar thePlayer = theObject.gameObject.GetComponent<FighterChar>();

		if((thePlayer != null)&&(thePlayer.isLocalPlayer))
		{
			Debug.Log("Player went outdoors.");
			isPlayerIndoors = false;
			transitionTime = 0.5f;
			if(invertOpacity)
			{
				isPlayerIndoors = !isPlayerIndoors;
			}
		}
	}
}
