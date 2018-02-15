using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Shoe : MonoBehaviour {

	[SerializeField] public string shoeName;	// Shoe's name text.
	[SerializeField] public int shoeID;		// Shoe index number.
	[SerializeField] public string shoeDesc;	// Shoe description text.
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

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
