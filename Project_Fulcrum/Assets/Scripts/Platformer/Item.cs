using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Item : MonoBehaviour {


	[SerializeField][ReadOnlyAttribute] public int itemType;	// Item's slot type. Shoe(0), Gadget(1), or Weapon(2).
	[SerializeField] public string itemName;	// Item's name text.
	[SerializeField] public string itemDesc;	// Item description text.
	protected SpriteRenderer itemSprite;			// Item description text.



	protected bool falling = true;
	protected float inactiveTimeMax = 2; // Max inactive time upon being dropped.
	protected float inactiveTimeCur; // Current remaining time spent inactive (unable to be picked up).


	public virtual void Awake()
	{
		itemSprite = this.GetComponent<SpriteRenderer>();
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
		itemSprite.enabled = false;
		falling = false;
	}

	public void Drop()
	{
		this.GetComponent<CircleCollider2D>().enabled = true;
		transform.localPosition = Vector3.zero;
		transform.parent = null;
		itemSprite.enabled = true;
		inactiveTimeCur = inactiveTimeMax;
		falling = true;
	}

	public virtual void OnTriggerEnter2D(Collider2D theObject)
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
