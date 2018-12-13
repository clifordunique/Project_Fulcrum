using UnityEngine;
using UnityEngine.UI;
//using FulcrumHelpers.mp;

public class UITabManager : MonoBehaviour {
	public MenuWipeCamera[] tabWipers;
	public FulcrumSlideSelector mySlideSelector;
	public Slider mainMenuSlider;
	public RectTransform[] tabRenderLayers;
	public string[] debugTN;
	private float layerDelay = 0.175f;
	//private float layerDelay = 0.5f;

	//Managered variables
	public float currentSlideValue;

	//V3 Variables
	public int movingTabsCount; // Cannot dispatch any opposite direction tabs until this reaches 0;
	public float linearMenuGoal; // set by fulcrumslideselector.
	public int movingState; //1 is right, 0 stopped, and -1 is left. cannot change until movingtabscount = 0
	public int currentMenu;


	// Use this for initialization
	void Start ()
	{
		currentMenu = 4;
	}

	void Update()
	{

		currentSlideValue = mySlideSelector.currentPosGoalPercent * 5;


		if (currentSlideValue - Mathf.RoundToInt(currentSlideValue) <= 0.05f)
			{currentSlideValue = Mathf.RoundToInt(currentSlideValue);}

		if (Mathf.Abs(currentSlideValue - currentMenu) > 0.001f && movingState == 0)
			{StartTransition();}
		if ((currentMenu - currentSlideValue  > movingTabsCount && movingState == -1)|| (currentSlideValue - currentMenu > movingTabsCount && movingState == 1))
			{LateDispatch();}
		if (movingTabsCount == 0 && movingState != 0)
			{CompleteTransition();}

	}

	void CompleteTransition()
	{
		//print("TRANSITION FINISHED. Ended on menu: "+debugTN[currentMenu]);
		movingState = 0;
	}

	void LateDispatch()
	{
		if (movingTabsCount == 0) { return; }

		int tabsInvolved = Mathf.Abs((int)(currentSlideValue - currentMenu)) + 1;
		//print("Transitioning with " + tabsInvolved + " tabs.");
		if (movingState == -1) // Moving left
		{
			string multiprint = "Late dispatch left...\n";

			multiprint += "(int)currentSlideValue=" + (int)currentSlideValue;
			multiprint += "\tcurrentMenu=" + currentMenu;
			//print(multiprint);
			for (int i = currentMenu-movingTabsCount; i > (int)(currentSlideValue); i--)
			{
				print("LATE LEFT DISPATCH ON debugTN[i]");
				//print("Starting transition for " + debugTN[i] + " in direction " + movingState);
				tabWipers[i].currentLayerDepth = currentMenu - i;
				tabWipers[i].StartTransition(movingState);
			}
		}
		else if (movingState == 1) // Moving right
		{
			string multiprint = " Late dispatch right...\n";

			multiprint += "(int)currentSlideValue=" + (int)currentSlideValue;
			multiprint += "\tcurrentMenu=" + currentMenu;
			//print(multiprint);
			for (int i = currentMenu+movingTabsCount; i < (int)(currentSlideValue); i++)
			{
				print("LATE RIGHT DISPATCH ON debugTN[i]");
				//print("Starting transition for "+ debugTN[i]+" in direction "+movingState);
				tabWipers[i].currentLayerDepth = i - currentMenu;
				tabWipers[i].StartTransition(movingState);
			}
		}
		else
		{
			//print("transitioning to nowhere lol.");
		}
	}

	void StartTransition()
	{
		if (movingTabsCount != 0) { return; }


		int tabsInvolved = Mathf.Abs((int)(currentSlideValue-currentMenu))+1;
		//print("Transitioning with " + tabsInvolved + " tabs.");
		if (currentSlideValue - currentMenu < 0) // Moving left
		{
			string multiprint = "Moving left...\n";
			movingState = -1;
			RearrangeLayers(movingState);

			multiprint += "(int)currentSlideValue=" + (int)currentSlideValue;
			multiprint += "\tcurrentMenu=" + currentMenu;
			//print(multiprint);
			for (int i = currentMenu; i > (int)(currentSlideValue); i--)
			{
				//print("Starting transition for " + debugTN[i] + " in direction " + movingState);
				print("Setting tab " + debugTN[i] + " to layer depth " + (currentMenu - i));
				tabWipers[i].currentLayerDepth = currentMenu-i;
				tabWipers[i].StartTransition(movingState);
			}
		}
		else if (currentSlideValue - currentMenu > 0) // Moving right
		{
			string multiprint = "Moving right...\n";
			movingState = 1;
			RearrangeLayers(movingState);

			multiprint += "(int)currentSlideValue=" + (int)currentSlideValue;
			multiprint += "\tcurrentMenu=" + currentMenu;
			//print(multiprint);
			for (int i = currentMenu; i < (int)(currentSlideValue); i++)
			{
				print("Setting tab "+ debugTN[i]+" to layer depth "+ (i - currentMenu));
				tabWipers[i].currentLayerDepth = i-currentMenu;
				tabWipers[i].StartTransition(movingState);
			}
		}
		else
		{
			//print("transitioning to nowhere lol.");
		}
	}

	void RearrangeLayers(int direction)
	{
		int menuIterator = currentMenu;
		if (direction == -1)
		{
			string multiprint = "Ordering layers:\n";
			for (int i = 5; i > 0; i--)
			{
				if (menuIterator < 0) { menuIterator = 4; }
				tabRenderLayers[menuIterator].SetSiblingIndex(i);
				multiprint += debugTN[menuIterator] + "[" + i + "]======> ";
				menuIterator--;
			}
			//print(multiprint);
		}
		else if (direction == 1)
		{
			string multiprint = "Ordering layers:\n";

			for (int i = 5; i > 0; i--)
			{ 
				if (menuIterator > 4) { menuIterator = 0; }
				tabRenderLayers[menuIterator].SetSiblingIndex(i);
				multiprint += debugTN[menuIterator] + "[" + i + "]======> ";
				menuIterator++;
			}
			//print(multiprint);
		}
	}
}
