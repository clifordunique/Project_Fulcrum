using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirPunch : MonoBehaviour {

	public Vector2 aimDirection = Vector2.zero;
	public FighterChar punchThrower;

	void OnTriggerEnter2D(Collider2D theObject)
	{
		if(theObject.gameObject != null)
		{
			punchThrower.PunchConnect(theObject.gameObject, aimDirection);
		}
		//print(theObject.gameObject);
		//FighterChar theChar = theObject.gameObject.GetComponent<FighterChar>();

//		if(theChar != null)
//		{
//			Debug.Log("HIT!");
//			//theChar.CmdGotHitBy(this.transform.parent.gameObject);
//			//theChar.CmdGotHit(aimDirection);
//			punchThrower.PunchConnect(theChar, aimDirection);
//		}
	}

	public void Complete()
	{
		//print("Punch ended, destroying airpunch object.");
		//print("Before I go, here is my parent object:"+this.transform.parent.transform.parent.gameObject);
		Destroy(this.transform.parent.gameObject);
	}
}
