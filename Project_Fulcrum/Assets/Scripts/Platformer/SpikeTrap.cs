using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : MonoBehaviour {


	public float Magnitude = 3;
	public float Roughness = 500;
	public float FadeOutTime = 1f;
	public float FadeInTime = 0f;
	public Vector3 RotInfluence = new Vector3(0,0,0);
	public Vector3 PosInfluence = new Vector3(1,1,0);

	void OnTriggerEnter2D(Collider2D theObject)
	{
		FighterChar theFighter = theObject.gameObject.GetComponent<FighterChar>();

		if((theFighter != null))
		{
			if(theFighter.isAlive())
			{
				float speed = theFighter.GetSpeed();
				if(speed>25)
				{
					theFighter.TakeDamage((int)(speed/4)-5);
					theFighter.v.triggerFlinched = true;
					theFighter.o.fighterAudio.PainSound();
	
					if(theFighter.IsPlayer())
					{
						theFighter.ShakeFighter(Magnitude, Roughness, FadeInTime, FadeOutTime, PosInfluence, RotInfluence);
					}
				}
			}
		}
	}

	void OnTriggerStay2D(Collider2D theObject)
	{
		FighterChar theFighter = theObject.gameObject.GetComponent<FighterChar>();

		if((theFighter != null))
		{
			if(theFighter.isAlive())
			{
				float speed = theFighter.GetSpeed();
				if(speed>25)
				{
					theFighter.TakeDamage((int)(1));
					theFighter.SetSpeed((speed*0.9f)-(1*Time.fixedDeltaTime));
				}
			}
		}
	}

}
