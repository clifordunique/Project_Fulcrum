using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Interactable))]
public class Shoe : Item {

	[SerializeField] public int shoeID;						// Shoe index number.
	[SerializeField] public int soundType;					// 0 equals normal, 1 equals metal.
	[Range(0, 10)] public int maxPickupDistance = 10;		// Max distance the player can be and still pick up the item.
	[SerializeField] public Interactable myInteractable;	// The component that handles click events.


	[SerializeField] public MovementVars m;

	public virtual void Start()
	{
		//if (this.gameObject.GetComponent<Interactable>() == null)
		//{
		//	this.gameObject.AddComponent<Interactable>();
		//}
		//myInteractable = this.gameObject.GetComponent<Interactable>();
		itemType = 0;
		itemSprite = this.GetComponent<SpriteRenderer>();
		inactiveTimeCur = inactiveTimeMax;
	}

	//public float timetilltest = 5;

	//public void FixedUpdate()
	//{
	//	timetilltest -= Time.fixedDeltaTime;
	//	if (timetilltest <= 0)
	//	{
	//		myInteractable.mouseDownEvent.AddListener(Interact);
	//		timetilltest = 10000;
	//	}
	//}

	public void Interact(GameObject theObject)
	{
		print("Interact triggered");

		if (theObject == null)
		{
			print("Error: Argument null.");
			return;
		}

		print(theObject);

		FighterChar thePlayer = theObject.gameObject.GetComponent<FighterChar>();

		if (thePlayer != null)
		{
			Vector3 meToYou = theObject.transform.position - this.transform.position;
			float distance = meToYou.magnitude;
			print("Distance to object: " + distance);

			if (thePlayer.IsPlayer() && inactiveTimeCur <= 0 && distance <= maxPickupDistance)
			{
				thePlayer.EquipItem(this);
			}
		}
		else
		{
			print("Error: Clicked on but not by player??");
		}
	}
}
