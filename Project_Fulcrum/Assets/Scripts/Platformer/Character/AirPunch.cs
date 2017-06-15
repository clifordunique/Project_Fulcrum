using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirPunch : MonoBehaviour {

	public Vector2 aimDirection = Vector2.zero;
	public FighterChar punchThrower;
	public bool alreadyHitSomething = false;

	void Start()
	{
	}

	void OnTriggerEnter2D(Collider2D theObject)
	{
		FighterChar enemyFighter = null;
		if(theObject.gameObject != null)
		{
			enemyFighter = theObject.gameObject.GetComponent<FighterChar>();
		}
		if(enemyFighter != null)
		{
			if(!alreadyHitSomething)
			{
				alreadyHitSomething = true;
				punchThrower.PunchConnect(theObject.gameObject, aimDirection);
			}
		}
	}

	public void Complete()
	{
		Destroy(this.transform.parent.gameObject);
	}
}
