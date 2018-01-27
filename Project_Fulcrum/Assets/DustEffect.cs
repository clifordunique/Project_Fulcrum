using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustEffect : MonoBehaviour {

	[SerializeField][ReadOnlyAttribute] private float curDuration;
	[SerializeField][Range(0,10)] private float fullSizeDuration; //Time for the dust clouds to reach full size;
	[SerializeField][Range(0,10)] private float maxDuration;
	[SerializeField][ReadOnlyAttribute] private SpriteRenderer myRenderer;
	[SerializeField][ReadOnlyAttribute] private float randomRotationSpeed;
	[SerializeField][ReadOnlyAttribute] private float randomSize;
	[SerializeField][ReadOnlyAttribute] private float randomPosY;
	[SerializeField][ReadOnlyAttribute] private float curSize;


	// Use this for initialization
	void Start () 
	{
		this.transform.localScale = Vector3.zero;
		curSize = 0;
		myRenderer = this.GetComponent<SpriteRenderer>();
		randomRotationSpeed = Random.Range(-1f, 1f);
		randomSize = Random.Range(0.8f, 1.2f);
		randomPosY = Random.Range(-0.1f, 0.1f);
		this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y+randomPosY);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(curDuration<fullSizeDuration)
		{
			curSize = (curSize+randomSize*(curDuration/fullSizeDuration))/2;
		}
		else
		{
			curSize = randomSize;
		}
		this.transform.localScale = curSize*Vector3.one;
		this.transform.Rotate(new Vector3(0,0, randomRotationSpeed));
		myRenderer.color = new Color(1, 1, 1, 1-(curDuration/maxDuration));
		curDuration += Time.deltaTime;
		if(curDuration>maxDuration){Complete();}
	}

	public void Complete()
	{
		Destroy(this.transform.parent.gameObject);
	}
}
