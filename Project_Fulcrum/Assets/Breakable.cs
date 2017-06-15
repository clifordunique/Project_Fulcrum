using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour {

	[SerializeField]private Transform spriteObject;
	Vector3 trueSpritePosition;
	private float shakiness = 5;

	// Use this for initialization
	void Awake() 
	{
		spriteObject = transform.Find("Sprite");
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(shakiness > 0)
		{
			shakiness -= Time.deltaTime/2;
			spriteObject.localPosition = new Vector3(UnityEngine.Random.Range(-0.1f,0.1f), UnityEngine.Random.Range(-0.1f,0.1f), 0);
		}
	}

	public void GotHit(float impactGForce)
	{
		
	}
}
