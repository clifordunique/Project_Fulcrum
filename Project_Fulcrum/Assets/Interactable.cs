using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class GameObjEvent : UnityEvent<GameObject>
{
}

public class Interactable : MonoBehaviour {

	[SerializeField] public GameObject localPlayer;
	[SerializeField] public GameObjEvent mouseEnterEvent;
	[SerializeField] public GameObjEvent mouseExitEvent;
	[SerializeField] public GameObjEvent mouseDownEvent;
	[SerializeField] public Material interactableMat;
	[SerializeField][ReadOnlyAttribute] public SpriteRenderer mySprite; 

	void OnMouseEnter()
	{
		FindPlayer();
		mouseEnterEvent.Invoke(localPlayer);
		mySprite.color = Color.red;
		//print("MOUSE ENTERED!");
	}

	void OnMouseExit()
	{
		FindPlayer();
		mouseExitEvent.Invoke(localPlayer);
		mySprite.color = Color.white;
		//print("MOUSE EXITED!");

	}

	void OnMouseDown()
	{
		FindPlayer();
		mouseDownEvent.Invoke(localPlayer);
		//print("MOUSE DOWN!");
	}

	// Use this for initialization
	void Start () {}

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

	public void FindPlayer() // Yes, I know this is terrible.
	{
		Player[] playerlist = FindObjectsOfType<Player>();
		foreach (Player p in playerlist)
		{
			if (p.isLocalPlayer)
			{
				localPlayer = p.gameObject;
			}
		}
		print("Local player found to be: " + localPlayer);
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
}
