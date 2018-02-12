using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using AK.Wwise;

public class Breakable : NetworkBehaviour {

	[SerializeField][ReadOnlyAttribute]private Transform spriteObject;
	[SerializeField][ReadOnlyAttribute]private TimeManager timeManager;

	Vector3 trueSpritePosition;
	private float duration = 0;
	private float maxDuration = 1f; //Max shake, scales linearly from minforce to maxforce;
	[SerializeField]private int minForce = 60; // min force before shaking.
	[SerializeField]private int maxForce = 250; // max force before breaking.
	[SerializeField]private float slowmoDuration; // slow mo duration upon breaking. If set to 0, no slowmo occurs.
	[SerializeField]private float slowmoSpeedM = 0.25f; //0 is timestop, 1 is normal motion.
	private bool broken = false;


	// Use this for initialization
	void Awake() 
	{
		spriteObject = transform.Find("Sprite");
		timeManager = GameObject.Find("PFGameManager").GetComponent<TimeManager>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(broken){return;}
		if(duration > 2)
		{
			//print(duration);
			duration -= Time.deltaTime;
			float shakiness = 0.2f;
			spriteObject.localPosition = new Vector3(UnityEngine.Random.Range(-shakiness,shakiness), UnityEngine.Random.Range(-shakiness,shakiness), 0);
		}
		else if(duration > 0)
		{
			//print(duration);
			duration -= Time.deltaTime/2;
			float shakiness = duration/5;
			spriteObject.localPosition = new Vector3(UnityEngine.Random.Range(-shakiness,shakiness), UnityEngine.Random.Range(-shakiness,shakiness), 0);
		}
		else{duration = 0;}
	}

	public bool RecieveHit(FighterChar hitter)
	{
		
		Vector2 velocitee = hitter.GetVelocity();
		float speed = velocitee.magnitude;
		if(hitter.IsVelocityPunching())
		{
			//print("breakable would recieve a blow of force: "+speed);
			//print("POWER BOOST!!");
			velocitee *= 1.5f;
		}
		speed = velocitee.magnitude;
		//this.gameObject.SetActive(false);
		//print("breakable recieved a blow of force: "+speed);
		if(speed >= maxForce)
		{
			//this.gameObject.SetActive(false);
			this.Break(velocitee);
			hitter.SetSpeed(speed-maxForce);
			//print("set speed to: "+(speed-maxForce));
			return true; //Returns true if the player broke through and should not be deflected.
		}
		else
		{
			duration = maxDuration*(speed-minForce)/(maxForce-minForce);
			return false;
		}
	}

	public void Break(Vector2 hitForce)
	{
		
		this.transform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
		this.transform.GetComponent<Rigidbody2D>().AddForce(hitForce, ForceMode2D.Impulse);
		//print("Hit with speed:"+hitForce);
		AkSoundEngine.PostEvent("Breakable_Break", gameObject);
		this.gameObject.layer = 2;
		broken = true;
		if(slowmoDuration>0)
		{
			timeManager.TimeDilation(slowmoSpeedM, slowmoDuration);
		}
	}
}
