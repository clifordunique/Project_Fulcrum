using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShoeTooltip : MonoBehaviour {

	[SerializeField]private FighterChar myFighter;
	[SerializeField]private Shoe myShoe;
	[SerializeField]private Transform myTag;
	[SerializeField]private Transform myOrb;
	[SerializeField][ReadOnlyAttribute]private Image itemIcon;
	[SerializeField][ReadOnlyAttribute]private Text itemName;
	private bool mouseOver = false;
	private Vector3 retractedPos = new Vector3(-350, -10, 0);
	private Vector3 extendedPos = new Vector3(20, -10, 0);

	void Awake()
	{
		myTag = transform.Find("Mask/Tag");
		myOrb = transform.Find("Orb");
		itemIcon = myOrb.GetChild(0).GetComponent<Image>();
		itemName = myTag.GetChild(0).GetComponent<Text>();
	}

	public void OnMouseDown()
	{
		if(myFighter!=null)
		{
			DropShoe();
		}
	}

	public void OnMouseEnter()
	{
		mouseOver = true;
	}
	public void OnMouseExit()
	{
		mouseOver = false;
	}

	public void DropShoe()
	{
		myFighter.EquipItem(null);
	}
	 
	public void SetShoe(Shoe newShoe)
	{
		if(newShoe!=null)
		{
			myShoe = newShoe;
			itemName.text = newShoe.itemName;
			itemIcon.sprite = newShoe.GetComponent<SpriteRenderer>().sprite;
		}
	}

	public void SetFighter(FighterChar newFighter)
	{
		myFighter = newFighter;
	}

	// Update is called once per frame
	void Update () 
	{
		if(mouseOver)
		{
			myTag.localPosition = Vector3.Lerp(myTag.localPosition, extendedPos, Time.deltaTime*10);
		}
		else
		{
			myTag.localPosition = Vector3.Lerp(myTag.localPosition, retractedPos, Time.deltaTime*10);
		}
		
	}
}
