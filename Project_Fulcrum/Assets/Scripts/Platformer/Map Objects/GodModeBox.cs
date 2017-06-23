using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodModeBox : MonoBehaviour 
{
	void OnTriggerEnter2D(Collider2D theObject)
	{
		Debug.Log("TEST SUCCESS!");
		print(theObject.gameObject); //.GetComponent<PlatformerCharacter2D>().d_DevMode = true;
		FighterChar thePlayer = theObject.gameObject.GetComponent<FighterChar>();

		if(thePlayer != null)
		{
			//thePlayer.FighterState.DevMode = true;
		}
	}
}
