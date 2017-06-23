using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : FighterChar {

	protected void Start () 
	{
		this.FighterState.FinalPos = this.transform.position;
	}

	protected override void Respawn()
	{
		FighterState.Dead = false;
		FighterState.CurHealth = g_MaxHealth;
		o_Anim.SetBool("Dead", false);
		o_SpriteRenderer.color = new Color(1,0.6f,0,1);
	}
}
