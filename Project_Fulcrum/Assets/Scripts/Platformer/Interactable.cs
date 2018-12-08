using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class GameObjEvent : UnityEvent<GameObject>
{
}

public class Interactable : MonoBehaviour {

	/* 
	Place this component on any GameObject in the level that has a collider.
	Then, you can connect it to any other GameObject in the scene and execute public functions of them.
	This is useful for making any buttons, damaging zones, levers, doors, etc that need to interacted with.
	The trigger unityevents will only work if the collider is set to "asdfadsf" mode.
	*/


	[SerializeField] public GameObject localPlayer;
	[SerializeField] public GameObjEvent mouseEnterEvent;
	[SerializeField] public GameObjEvent mouseExitEvent;
	[SerializeField] public GameObjEvent mouseDownEvent;
	[SerializeField] public GameObjEvent enterTriggerEvent;
	[SerializeField] public GameObjEvent exitTriggerEvent;
	[SerializeField][ReadOnlyAttribute] public SpriteRenderer mySprite;

	//Visual/shader variables
	[SerializeField] public Material highlightMat;
	[SerializeField] public Material originalMat; //The material assigned to the object in the editor. 
	[SerializeField] public int mySpriteSize;
	[SerializeField] public Color myOutlineColour = Color.white;
	[SerializeField] public Color myFlashColour = Color.white;



	[Tooltip("Set to true if you want the sprite to display a highlighted outline when it is hovered over.")]
	[SerializeField] private bool useHighlightShader = true;


	void OnMouseEnter()
	{
		FindPlayer();
		mouseEnterEvent.Invoke(localPlayer);
		//mySprite.color = Color.red;
		//print("MOUSE ENTERED!");
		if (useHighlightShader)
		{
			mySprite.material = highlightMat;
			SetVisuals(mySpriteSize, myFlashColour, myOutlineColour);
		}

	}

	void OnMouseExit()
	{
		FindPlayer();
		mouseExitEvent.Invoke(localPlayer);
		//mySprite.color = Color.white;
		//print("MOUSE EXITED!");
		if (useHighlightShader)
		{
			mySprite.material = originalMat;
		}

	}

	void OnMouseDown()
	{
		FindPlayer();
		mouseDownEvent.Invoke(localPlayer);
		//print("MOUSE DOWN!");
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		if (true)
		{ // Replace this with code for what you want to trigger it. For example, "if(col.gameobject.tag == 'Player')"
			enterTriggerEvent.Invoke(col.gameObject);
		}
	}

	void OnTriggerExit2D(Collider2D col)
	{
		if (true)
		{ // Replace this with code for what you want to trigger it. For example, "if(col.gameobject.tag == 'Player')"
			exitTriggerEvent.Invoke(col.gameObject);
		}
	}


	// Use this for initialization
	void Start ()
	{
		mySprite = this.GetComponent<SpriteRenderer>();

		if (mySprite != null)
		{
			originalMat = mySprite.material;
			print("My sprite size: "+ (int)mySprite.sprite.rect.x);
			mySpriteSize = (int)mySprite.sprite.rect.x;
			if (useHighlightShader)
			{
				mySprite.material = highlightMat;
				SetVisuals(mySpriteSize, myFlashColour, myOutlineColour);
				mySprite.material = originalMat;
			}
		}
		else
		{
			Debug.LogError("MySprite is null!");
		}
	}

	public void SetVisuals(int spriteSize, Color flashColour, Color outlineColour)
	{
		this.mySprite.material.SetInt("_SpriteWidth", spriteSize); 
		this.mySprite.material.SetColor("_FlashColor", flashColour);
		this.SetFlashMagnitude(0.25f);
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
