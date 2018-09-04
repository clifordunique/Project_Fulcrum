using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Shoe : Item {

	[SerializeField] public int shoeID;			// Shoe index number.
	[SerializeField] public int soundType;		// 0 equals normal, 1 equals metal.

	/// <summary>
	/// Movement Attributes
	/// </summary>

	[SerializeField] public MovementVars m;
//	[Space(10)]
//	[SerializeField] public float minSpeed;							// The instant starting speed while moving.
//	[SerializeField] public float maxRunSpeed;							// The fastest the fighter can travel along land.
//	[Range(0,50)][SerializeField] public float startupAccelRate;  			// How quickly the fighter accelerates.
//	[Space(10)]
//	[SerializeField] public float vJumpForce;                  		// Amount of vertical force added when the fighter jumps.
//	[SerializeField] public float hJumpForce;  						// Amount of horizontal force added when the fighter jumps.
//	[SerializeField] public float wallVJumpForce;                  	// Amount of vertical force added when the fighter walljumps.
//	[SerializeField] public float wallHJumpForce;  					// Amount of horizontal force added when the fighter walljumps.
//	[SerializeField] public float etherJumpForcePerCharge; 				// How much force does each Ether Charge add to the jump power?
//	[SerializeField] public float etherJumpForceBase; 					// How much force does a no-power Ether jump have?
//	[Space(10)]
//	[SerializeField] public float tractionChangeT;						// Threshold where movement changes from exponential to linear acceleration.  
//	[SerializeField] public float wallTractionT;						// Speed threshold at which wallsliding traction changes.
//	[Range(0,5)][SerializeField] public float linearStopRate; 			// How fast the fighter decelerates when changing direction.
//	[Range(0,5)][SerializeField] public float linearSlideRate;			// How fast the fighter decelerates with no input.
//	[Range(0,5)][SerializeField] public float linearOverSpeedRate;		// How fast the fighter decelerates when running too fast.
//	[Range(0,5)][SerializeField] public float linearAccelRate;			// How fast the fighter accelerates with input.
//	[Range(1,89)][SerializeField] public float impactDecelMinAngle;	// Any impacts at sharper angles than this will start to slow the fighter down. Reaches full halt at impactDecelMaxAngle.
//	[Range(1,89)][SerializeField] public float impactDecelMaxAngle;	// Any impacts at sharper angles than this will result in a full halt. DO NOT SET THIS LOWER THAN impactDecelMinAngle!!
//	[Range(1,89)][SerializeField] public float tractionLossMinAngle; 	// Changes the angle at which steeper angles start to linearly lose traction, and eventually starts slipping back down. Default of 45 degrees.
//	[Range(45,90)][SerializeField] public float tractionLossMaxAngle;	// Changes the angle at which fighter loses ALL traction, and starts slipping back down. Default of 90 degrees.
//	[Range(0,2)][SerializeField] public float slippingAcceleration;  	// Changes how fast the fighter slides down overly steep slopes.
//	[Range(0.5f,3)][SerializeField] public float surfaceClingTime; 	// How long the fighter can cling to walls before gravity takes over.
//	[Range(20,70)][SerializeField] public float clingReqGForce;		// This is the amount of impact GForce required for a full-duration ceiling cling.
//	[Space(10)]
//	[SerializeField] public float slamT;								// Impact threshold for slam
//	[SerializeField] public float craterT; 							// Impact threshold for crater
//	[SerializeField] public float guardSlamT; 							// Guarded Impact threshold for slam
//	[SerializeField] public float guardCraterT;						// Guarded Impact threshold for crater
//	[Space(10)]
//	[SerializeField][Range(0,1)] public float strandJumpSpeedLossM; 	// Percent of speed lost with each strand jump
//	[SerializeField][Range(0f,180f)]public float widestStrandJumpAngle;// Most shallow angle that allows a strand jump
//
	public virtual void Awake()
	{
		itemType = 0;
		itemSprite = this.GetComponent<SpriteRenderer>();
		inactiveTimeCur = inactiveTimeMax;
	}


	public override void OnTriggerEnter2D(Collider2D theObject)
	{
		FighterChar thePlayer = theObject.gameObject.GetComponent<FighterChar>();

		if(thePlayer!=null)
		{
			if(thePlayer.IsPlayer() && inactiveTimeCur <= 0)
			{
				thePlayer.EquipItem(this);
			}
		}
		else //if ( theObject.gameObject.layer == 15 ) // If collided object is a world object.
		{
			//print("collided with: "+theObject.name);
			falling = false;
		}
	}
}
