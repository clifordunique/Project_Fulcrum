using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : FighterChar {

	protected void Start () 
	{
		this.FighterState.FinalPos = this.transform.position;
	}
}
