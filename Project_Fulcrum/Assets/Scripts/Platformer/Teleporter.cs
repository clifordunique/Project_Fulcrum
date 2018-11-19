using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour {

	[Header("Teleport Parameters")]
	public Vector2 destination;
	public Transform destAnchor; // If set to a transform in the level, it will use that as coordinates for the teleport. Simply for convenience over manually typing coordinates.

	void Awake()
	{
		if(destAnchor!=null)
		{
			destination = destAnchor.position;
		}
	}

	void OnTriggerEnter2D(Collider2D theObject)
	{
		FighterChar theFighter = theObject.gameObject.GetComponent<FighterChar>();

		if((theFighter != null))
		{
			theFighter.Teleport(destination, false);
		}
	}
}
