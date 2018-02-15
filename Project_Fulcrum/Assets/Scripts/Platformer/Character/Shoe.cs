using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Shoe : MonoBehaviour {

	[SerializeField] public string shoeName;	// Shoe's name text.
	[SerializeField] public int shoeID;			// Shoe index number.
	[SerializeField] public string shoeDesc;	// Shoe description text.
	[SerializeField] public int soundType;		// 0 equals normal, 1 equals metal.
	private SpriteRenderer shoeSprite;			// Shoe description text.



	private bool falling = true;
	private float inactiveTimeMax = 2; // Max inactive time upon being dropped.
	private float inactiveTimeCur; // Current remaining time spent inactive (unable to be picked up).

	
	/// <summary>
	/// Movement Attributes
	/// </summary>
	[Space(10)]
	[SerializeField] public float m_MinSpeed;							// The instant starting speed while moving.
	[SerializeField] public float m_MaxRunSpeed;							// The fastest the fighter can travel along land.
	[Range(0,2)][SerializeField] public float m_Acceleration;  			// How quickly the fighter accelerates.
	[Space(10)]
	[SerializeField] public float m_VJumpForce;                  		// Amount of vertical force added when the fighter jumps.
	[SerializeField] public float m_HJumpForce;  						// Amount of horizontal force added when the fighter jumps.
	[SerializeField] public float m_WallVJumpForce;                  	// Amount of vertical force added when the fighter walljumps.
	[SerializeField] public float m_WallHJumpForce;  					// Amount of horizontal force added when the fighter walljumps.
	[SerializeField] public float m_ZonJumpForcePerCharge; 				// How much force does each Zon Charge add to the jump power?
	[SerializeField] public float m_ZonJumpForceBase; 					// How much force does a no-power Zon jump have?
	[Space(10)]
	[SerializeField] public float m_TractionChangeT;						// Threshold where movement changes from exponential to linear acceleration.  
	[SerializeField] public float m_WallTractionT;						// Speed threshold at which wallsliding traction changes.
	[Range(0,5)][SerializeField] public float m_LinearStopRate; 			// How fast the fighter decelerates when changing direction.
	[Range(0,5)][SerializeField] public float m_LinearSlideRate;			// How fast the fighter decelerates with no input.
	[Range(0,5)][SerializeField] public float m_LinearOverSpeedRate;		// How fast the fighter decelerates when running too fast.
	[Range(0,5)][SerializeField] public float m_LinearAccelRate;			// How fast the fighter accelerates with input.
	[Range(1,89)][SerializeField] public float m_ImpactDecelMinAngle;	// Any impacts at sharper angles than this will start to slow the fighter down. Reaches full halt at m_ImpactDecelMaxAngle.
	[Range(1,89)][SerializeField] public float m_ImpactDecelMaxAngle;	// Any impacts at sharper angles than this will result in a full halt. DO NOT SET THIS LOWER THAN m_ImpactDecelMinAngle!!
	[Range(1,89)][SerializeField] public float m_TractionLossMinAngle; 	// Changes the angle at which steeper angles start to linearly lose traction, and eventually starts slipping back down. Default of 45 degrees.
	[Range(45,90)][SerializeField] public float m_TractionLossMaxAngle;	// Changes the angle at which fighter loses ALL traction, and starts slipping back down. Default of 90 degrees.
	[Range(0,2)][SerializeField] public float m_SlippingAcceleration;  	// Changes how fast the fighter slides down overly steep slopes.
	[Range(0.5f,3)][SerializeField] public float m_SurfaceClingTime; 	// How long the fighter can cling to walls before gravity takes over.
	[Range(20,70)][SerializeField] public float m_ClingReqGForce;		// This is the amount of impact GForce required for a full-duration ceiling cling.
	[Space(10)]
	[SerializeField] public float m_SlamT;								// Impact threshold for slam
	[SerializeField] public float m_CraterT; 							// Impact threshold for crater
	[SerializeField] public float m_GuardSlamT; 							// Guarded Impact threshold for slam
	[SerializeField] public float m_GuardCraterT;						// Guarded Impact threshold for crater
	[Space(10)]
	[SerializeField][Range(0,1)] public float m_StrandJumpSpeedLossM; 	// Percent of speed lost with each strand jump
	[SerializeField][Range(0f,180f)]public float m_WidestStrandJumpAngle;// Most shallow angle that allows a strand jump

	public void Awake()
	{
		shoeSprite = this.GetComponent<SpriteRenderer>();
		inactiveTimeCur = inactiveTimeMax;
	}

	public void FixedUpdate()
	{
		if(falling)
		{
			this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y-Time.fixedDeltaTime, this.transform.position.z);
		}
		if(inactiveTimeCur>0)
		{
			inactiveTimeCur -= Time.fixedDeltaTime;
		}
		else
		{
			inactiveTimeCur = 0;
		}
	}

	public void DestroyThis()
	{
		Destroy(this.gameObject);
	}

	public void PickedUpBy(FighterChar newFighter)
	{
		this.GetComponent<CircleCollider2D>().enabled = false;
		transform.parent = newFighter.transform;
		shoeSprite.enabled = false;
		falling = false;
	}

	public void Drop()
	{
		if(shoeID==0)
		{
			print("Feet got dropped!");
		}
		this.GetComponent<CircleCollider2D>().enabled = true;
		transform.localPosition = Vector3.zero;
		transform.parent = null;
		shoeSprite.enabled = true;
		inactiveTimeCur = inactiveTimeMax;
		falling = true;
	}

	void OnTriggerEnter2D(Collider2D theObject)
	{
		FighterChar thePlayer = theObject.gameObject.GetComponent<FighterChar>();

		if(thePlayer!=null)
		{
			if(thePlayer.IsPlayer() && inactiveTimeCur <= 0)
			{
				thePlayer.EquipShoe(this);
			}
		}
		else //if ( theObject.gameObject.layer == 15 ) // If collided object is a world object.
		{
			print("collided with: "+theObject.name);
			falling = false;
		}
	}
}
