using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour {

	[SerializeField]private Transform spriteObject;
	[SerializeField]private AudioSource breakNoiseEmitter;

	Vector3 trueSpritePosition;
	private float duration = 1f;
	private float maxDuration = 1f; //Max shake, scales linearly from minforce to maxforce;
	[SerializeField]private int minForce = 60; // min force before shaking.
	[SerializeField]private int maxForce = 250; // max force before breaking.
	private bool broken = false;

	// Use this for initialization
	void Awake() 
	{
		spriteObject = transform.Find("Sprite");
		breakNoiseEmitter = this.transform.GetComponent<AudioSource>();
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
		breakNoiseEmitter.Play();
		this.gameObject.layer = 2;
		broken = true;
	}
}
