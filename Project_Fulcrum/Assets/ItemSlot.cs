using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour {

	[SerializeField][ReadOnlyAttribute]private FighterChar o_Fighter;
	[SerializeField][ReadOnlyAttribute]private ItemHandler o_ItemHandler;
	[SerializeField][ReadOnlyAttribute]private Item myItem;
	[SerializeField][ReadOnlyAttribute]private Transform myDrawer;
	[SerializeField][ReadOnlyAttribute]private Image itemIcon;
	[SerializeField][ReadOnlyAttribute]private Text itemName;
	[SerializeField][ReadOnlyAttribute]private Text itemDesc;
	private bool mouseOver = false;
	private Vector3 retractedPos;
	private Vector3 extendedPos;

	void Awake()
	{
		myDrawer = transform.Find("Drawer");
		o_ItemHandler = GameObject.Find("PFGameManager").GetComponent<ItemHandler>();
		retractedPos = myDrawer.localPosition;
		extendedPos = retractedPos + new Vector3(196, 0, 0);
		itemName = myDrawer.GetChild(0).GetComponent<Text>();
		itemIcon = myDrawer.GetChild(1).GetComponent<Image>();
	}

	public void OnMouseDown()
	{
		if(o_Fighter!=null)
		{
			DropItem();
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

	public void DropItem()
	{
		if(myItem.itemType==0)
		{
			Shoe barefoot = Instantiate(o_ItemHandler.shoes[0], this.transform.position, Quaternion.identity).GetComponent<Shoe>();
			o_Fighter.EquipItem(barefoot);
			print("Equipping bare feet.");
		}
		else if(myItem.itemType==1)
		{
//			Gadget emptyhanded = Instantiate(o_ItemHandler.Gadgets[0], this.transform.position, Quaternion.identity).GetComponent<Gadget>();
//			o_Fighter.EquipItem(emptyhanded);
		}
		else if(myItem.itemType==2)
		{
//			Weapon fists = Instantiate(o_ItemHandler.weapons[0], this.transform.position, Quaternion.identity).GetComponent<Weapon>();
//			o_Fighter.EquipItem(fists);
		}
	}

	public void SetItem(Item newItem)
	{
		if(newItem!=null)
		{
			myItem = newItem;
			itemName.text = newItem.itemName;
			itemIcon.sprite = newItem.GetComponent<SpriteRenderer>().sprite;
		}
	}

	public void SetFighter(FighterChar newFighter)
	{
		o_Fighter = newFighter;
	}

	// Update is called once per frame
	void Update () 
	{
		if(mouseOver)
		{
			myDrawer.localPosition = Vector3.Lerp(myDrawer.localPosition, extendedPos, Time.deltaTime*10);
		}
		else
		{
			myDrawer.localPosition = Vector3.Lerp(myDrawer.localPosition, retractedPos, Time.deltaTime*10);
		}

	}
}
