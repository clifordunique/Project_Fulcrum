using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FulcrumSlideSelector : MonoBehaviour {
	[HideInInspector] public int buttonCount = 5;
	[HideInInspector] public int spacesCount;
	[HideInInspector] public int buttonWidth;
	[HideInInspector] public int scrW;
	[HideInInspector] public int scrH;
	[HideInInspector] public int spacing;
	[HideInInspector] public int currentTab;
	[HideInInspector] public float dragStartPos;
	[HideInInspector] public float dragStartTab;
	public UITabManager myUITabManager;
	public float currentPosGoal;
	public float currentPosLerp;
	public float currentPosLerpPercent;

	public Transform[] dbgMarker;

	// Use this for initialization
	void Start ()
	{
		scrW = Screen.width;
		scrH = Screen.height;
		currentTab = myUITabManager.currentMenu;
		//spacing = scrW / buttonCount;
		buttonWidth = scrW / buttonCount;
		spacesCount = buttonCount - 1;
		spacing = scrW / buttonCount;
		this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.4f;

		for (int i = 0; i <= 4; i++)
		{
			dbgMarker[i].position = new Vector3((i * spacing)+(buttonWidth/2), this.transform.position.y, this.transform.position.z);
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		currentPosLerp = Mathf.Lerp(currentPosLerp, currentPosGoal, Time.deltaTime*10);
		if (Mathf.Abs(currentPosLerp - currentPosGoal) < 0.0001f)
		{
			currentPosLerp = currentPosGoal;
		}
		currentPosLerpPercent = currentPosLerp / scrW;

		

		this.transform.position = new Vector3(currentPosLerp, this.transform.position.y, this.transform.position.z);
	}

	public void SetCurrentSliderTab(int tab)
	{
		if (myUITabManager.movingState != 0) { return; }
		currentTab = tab;
		if (currentTab > 4) { currentTab = 4; }
		DragRelease();
	}

	public void DragRelease()
	{
		if (currentTab > 4) { currentTab = 4; }
		currentPosGoal = currentTab * buttonWidth;
		myUITabManager.TransitionTo(currentTab);
	}

	public void DragStart()
	{
		//if (myUITabManager.movingState != 0) { return; }
		dragStartPos = Input.mousePosition.x;
		dragStartTab = currentTab;
	}

	public void Dragging()
	{
		if (currentTab != dragStartTab)
		{
			DragStart();
			myUITabManager.TransitionTo(currentTab);
		}
		//float mouseMovement = Input.mousePosition.x - dragStartPos - ((currentTab-dragStartTab)*buttonWidth);
		float mouseMovement = Input.mousePosition.x - dragStartPos;
		float tabCenter = (currentTab * buttonWidth) + (buttonWidth / 2);
		currentTab = (int)((tabCenter+mouseMovement)/spacing);
		if (currentTab > 4) { currentTab = 4; }

		//float translatedPos = (currentTab * spacing) + mouseMovement;
		//currentTab = (int)(translatedPos / spacing);
		float weighted = ((currentTab * buttonWidth * 2) + mouseMovement) / 2;

		if (weighted < 0) { weighted = 0; }
		if (weighted > (buttonCount-1) * buttonWidth) { weighted = (buttonCount-1) * buttonWidth; }

		dbgMarker[5].position = new Vector3(tabCenter + mouseMovement, this.transform.position.y + 10, this.transform.position.z);

		print("(currentTab * buttonWidth) " + (currentTab * buttonWidth) + "\nmouseMovement " + mouseMovement + "\nweighted" + weighted);
		//currentTab = (int)(weighted / spacing);
		currentPosGoal = weighted;

		//print("Input.mousePosition.x "+ Input.mousePosition.x + "\nspacing "+ spacing + "\ncurrentTab" + currentTab);

		//this.transform.position = new Vector3(currentTab * buttonWidth+mouseMovement, this.transform.position.y, this.transform.position.z);
	}


	//public void DragRelease()
	//{
	//	float mouseMovement = Input.mousePosition.x - dragStartPos;
	//	float translatedPos = (currentTab * spacing) + mouseMovement;
	//	currentTab = (int)(translatedPos / spacing);
	//	this.transform.position = new Vector3(currentTab*spacing, this.transform.position.y, this.transform.position.z);
	//}

	//public void DragStart()
	//{
	//	dragStartPos = Input.mousePosition.x;
	//}

	//public void Dragging()
	//{
	//	//currentTab = (int)(Input.mousePosition.x / spacing);
	//	float mouseMovement = Input.mousePosition.x - dragStartPos;
	//	float translatedPos = (currentTab * spacing) + mouseMovement;
	//	currentTab = (int)(translatedPos / spacing);
	//	float weighted = ((currentTab * 2 * spacing) + translatedPos) / 3;

	//	print("mouseMovement " + mouseMovement + "\ntranslatedPos " + translatedPos + "\nweighted" + weighted);
	//	//currentTab = (int)(weighted / spacing);

	//	//print("Input.mousePosition.x "+ Input.mousePosition.x + "\nspacing "+ spacing + "\ncurrentTab" + currentTab);
	//	this.transform.position = new Vector3(weighted, this.transform.position.y, this.transform.position.z);
	//}
}
