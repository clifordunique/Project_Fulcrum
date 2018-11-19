using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimHeightAdjuster : MonoBehaviour {

	[SerializeField][ReadOnlyAttribute]private FighterChar myFighter;
	[SerializeField][ReadOnlyAttribute]private Vector2 downDirection;
	[SerializeField][ReadOnlyAttribute]private float angle;
	[SerializeField][ReadOnlyAttribute]private float radians;
	[SerializeField][ReadOnlyAttribute]private float embedAmount;

	void Start () {
		myFighter = this.transform.parent.gameObject.GetComponent<FighterChar>(); // Get this fighter's FighterChar component.
	}
	
	void Update () 
	{
		angle = this.transform.localRotation.eulerAngles.z;
		radians = (angle/360)*(2*Mathf.PI);
		downDirection = new Vector2((float)Mathf.Sin(radians), -(float)Mathf.Cos(radians));
		RaycastHit2D downCast = Physics2D.Raycast(this.transform.parent.position, downDirection, myFighter.m_GroundFootLength, myFighter.m_TerrainMask); 	// Ground	

		if(downCast)
		{
			embedAmount = myFighter.m_GroundFootLength-downCast.distance;
			this.transform.localPosition = downDirection.normalized*-embedAmount;
		}
		else
		{
			this.transform.localPosition = Vector3.zero;
		}
			//print("radians: "+radians+"\n"+"downDirection: "+downDirection+"\n"+"embedAmount: "+(myFighter.m_GroundFootLength-embedAmount));
	}
}
