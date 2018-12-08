using UnityEngine;
using UnityEngine.UI;

public class UITabManager : MonoBehaviour {
	public MenuWipeCamera[] tabWipers;
	public FulcrumSlideSelector mySlideSelector;
	public Slider mainMenuSlider;
	public int currentMenu;
	public int goalMenu;
	public GameObject[] tabRenderTextures;
	public string[] debugTN;
	private float layerDelay = 0.175f;
	//private float layerDelay = 0.5f;

	//Managered variables
	public float[] tabWipeAmount;
	public float currentSlideValue;
	public float currentSlideLerp;
	public int movingState; //1 is right, 0 stopped, and -1 is left.
	public int transLayerCount;

	// Use this for initialization
	void Start ()
	{
		tabWipeAmount = new float[tabWipers.Length];
		TransitionTo(0);
		goalMenu = currentMenu;
		CompleteTransition();
	}

	// Update is called once per frame
	void Update()
	{
		if (movingState != 0)
		{
			currentSlideValue = mySlideSelector.currentPosLerpPercent * 5;
			float currentSlideTab = mySlideSelector.currentTab;

			currentSlideLerp = Mathf.Lerp(currentSlideLerp, currentSlideValue, Time.deltaTime);

			if (Mathf.Abs(currentSlideLerp - currentSlideValue) < 0.0001f)
			{
				currentSlideLerp = currentSlideValue;
				CompleteTransition();
				return;
			}

			if (currentSlideLerp < currentSlideValue)
			{
				currentSlideLerp += Time.deltaTime / 5;
			}
			else
			{
				currentSlideLerp -= Time.deltaTime / 5;
			}

			

			for (int i = 0; i < tabWipers.Length; i++)
			{
				//bool betweenRight = (i > currentMenu && i < goalMenu);
				//bool betweenLeft = (i < currentMenu && i > goalMenu);
				//if (betweenRight || betweenLeft)
				//{
				if (movingState == 1)
				{
					tabWipeAmount[i] = currentSlideLerp - i;
				}
				else
				{
					tabWipeAmount[i] = i - currentSlideLerp;
				}
					if (tabWipeAmount[i] >= 1) { tabWipeAmount[i] = 1.1f; }
					tabWipers[i].curWipeAmount = tabWipeAmount[i];
					tabWipers[i].myMaterial.SetFloat("_SliceAmount", tabWipeAmount[i]);
				//}
			}
		}
		else
		{
			for (int i = 0; i < tabWipers.Length; i++)
			{
				tabWipeAmount[i] = 0;
				tabWipers[i].curWipeAmount = tabWipeAmount[i];
				tabWipers[i].myMaterial.SetFloat("_SliceAmount", tabWipeAmount[i]);
			}
		}
	}

	public void TransitionTo(int menuID)
	{
		if (movingState == 0)
		{
			StartTransition(menuID);
		}
		else
		{
			RedirectTransition(menuID);
		}
	}

	public void StartTransition(int menuID)
	{
		if (currentMenu == menuID) { return; }
		goalMenu = menuID;
		print("Starting transition to " + menuID + " from " + currentMenu);
		foreach (MenuWipeCamera m in tabWipers)
		{
			m.myMaterial.SetFloat("_SliceAmount", 0);
		}

		int menuIterator = currentMenu;
		float interval = Mathf.Abs(1 / (float)(currentMenu - goalMenu));
		print("interval " + interval);
		tabWipers[goalMenu].triggerWipe = false;


		if (goalMenu > currentMenu) //BRANCH A
		{
			movingState = 1;
			print("Moving Right.");
			string multiprint = "Ordering layers:\n";
			for (int i = 5; i > 0; i--)
			{
				if (menuIterator > 4){menuIterator = 0;}
				RectTransform nextUp = tabRenderTextures[menuIterator].GetComponent<RectTransform>();
				nextUp.SetSiblingIndex(i);
				multiprint += debugTN[menuIterator] + "[" + i + "]======> ";
				menuIterator++;
			}

			print(multiprint);
			int middleLayers = 0;
			for (int i = currentMenu; i < goalMenu; i++)
			{
				int countUp = i-currentMenu;
				int countDown = goalMenu-i;
				tabWipers[i].WipeSetup(false, 0.4f+(0.5f*countDown * interval));
				tabWipers[i].isBottomLayer = false;
				middleLayers++;
			}
			transLayerCount = middleLayers;
			tabWipers[currentMenu].WipeSetup(false, 1);
			tabWipers[goalMenu].WipeSetup(false, 0);
			tabWipers[goalMenu].isBottomLayer = true;
		}
		else //BRANCH B
		{
			movingState = -1;
			print("Moving Left");
			string multiprint = "Ordering layers:\n";

			for (int i = 5; i > 0; i--)
			{
				if (menuIterator < 0) { menuIterator = 4; }
				RectTransform nextUp = tabRenderTextures[menuIterator].GetComponent<RectTransform>();
				nextUp.SetSiblingIndex(i);
				multiprint += debugTN[menuIterator] + "[" + i +"]======> ";
				menuIterator--;
				//layerOrder--;
			}
			print(multiprint);
			int middleLayers = 0;
			for (int i = currentMenu; i > goalMenu; i--)
			{
				int countUp = currentMenu - i;
				int countDown = i - goalMenu;
				tabWipers[i].triggerWipe = true;
				tabWipers[i].WipeSetup(true, 0.4f + (0.5f * countDown * interval));
				tabWipers[i].isBottomLayer = false;
				middleLayers++;
			}
			transLayerCount = middleLayers;
			tabWipers[currentMenu].WipeSetup(true, 1);
			tabWipers[goalMenu].WipeSetup(true, 0);
			tabWipers[goalMenu].isBottomLayer = true;
		}
	}

	public void RedirectTransition(int menuID)
	{

	}

	public void CompleteTransition()
	{
		print("Completed transition to " + goalMenu+" from "+ currentMenu);
		transLayerCount = 0;
		currentMenu = goalMenu;
		movingState = 0;

		tabWipers[currentMenu].isBottomLayer = false;
		tabWipers[currentMenu].WipeSetup(false, 1);

		int menuIterator = currentMenu;
		string multiprint = "Ordering layers:\n";
		for (int i = 5; i > 0; i--)
		{
			if (menuIterator > 4) { menuIterator = 0; }
			RectTransform nextUp = tabRenderTextures[menuIterator].GetComponent<RectTransform>();
			nextUp.SetSiblingIndex(i);
			multiprint += debugTN[menuIterator] + "[" + i + "]======> ";
			menuIterator++;
		}
		print(multiprint);

		//currentSlideLerp = currentSlideValue;
		//for (int i = 0; i < tabWipers.Length; i++)
		//{
		//	tabWipeAmount[i] = 0;
		//	tabWipers[i].myMaterial.SetFloat("_SliceAmount", tabWipeAmount[i]);
		//}

	}

	//public void OpenMenu(int menuID)
	//{
	//	goalMenu = menuID;
	//	if (currentMenu == goalMenu) { return; }
	//	foreach (MenuWipeCamera m in tabWipers)
	//	{
	//		if (m.triggerWipe) { return; }
	//	}
	//	foreach (MenuWipeCamera m in tabWipers)
	//	{
	//		m.myMaterial.SetFloat("_SliceAmount", 0);
	//	}

	//	int menuIterator = currentMenu;
	//	float interval = Mathf.Abs(1 / (float)(currentMenu - goalMenu));
	//	print("interval " + interval);
	//	tabWipers[goalMenu].triggerWipe = false;


	//	if (goalMenu > currentMenu) //BRANCH A
	//	{
	//		print("Branch A selected.");
	//		for (int i = 5; i > 0; i--)
	//		{
	//			if (menuIterator > 4){menuIterator = 0;}
	//			RectTransform nextUp = tabRenderTextures[menuIterator].GetComponent<RectTransform>();
	//			nextUp.SetSiblingIndex(i);
	//			print("Setting: "+debugTN[menuIterator]+" to position "+i);
	//			menuIterator++;
	//			//layerOrder--;
	//		}

	//		print("currentMenu: " + currentMenu);
	//		print("goalMenu: " + goalMenu);

	//		for (int i = currentMenu; i < goalMenu; i++)
	//		{
	//			int countUp = i-currentMenu;
	//			int countDown = goalMenu-i;
	//			tabWipers[i].triggerWipe = true;
	//			tabWipers[i].WipeSetup(false, 0.4f+(0.5f*countDown * interval));
	//			print("Giving menu " + debugTN[i] + " brightness of:" + countDown * interval);
	//			float delay = (countUp * layerDelay);
	//			//print("Giving menu: " + debugTN[i] + " delay of:" + delay);
	//			tabWipers[i].timer = 0-delay;
	//			tabWipers[i].isBottomLayer = false;
	//			tabWipers[i].wipeAmountLerp = 0 - delay;
	//		}
	//		tabWipers[currentMenu].WipeSetup(false, 1);
	//		tabWipers[goalMenu].WipeSetup(false, 0);
	//		tabWipers[goalMenu].isBottomLayer = true;
	//	}
	//	else //BRANCH B
	//	{
	//		print("Branch B selected.");

	//		for (int i = 5; i > 0; i--)
	//		{
	//			if (menuIterator < 0) { menuIterator = 4; }
	//			RectTransform nextUp = tabRenderTextures[menuIterator].GetComponent<RectTransform>();
	//			nextUp.SetSiblingIndex(i);
	//			//print("Setting: " + debugTN[menuIterator] + " to position " + i);
	//			menuIterator--;
	//			//layerOrder--;
	//		}

	//		print("currentMenu: " + currentMenu);
	//		print("goalMenu: " + goalMenu);
	//		for (int i = currentMenu; i > goalMenu; i--)
	//		{
	//			int countUp = currentMenu - i;
	//			int countDown = i - goalMenu;
	//			tabWipers[i].triggerWipe = true;
	//			tabWipers[i].WipeSetup(true, 0.4f + (0.5f * countDown * interval));
	//			print("Giving menu " + debugTN[i] + " brightness of:" + countUp * interval);
	//			float delay = (countUp * layerDelay);
	//			//print("Giving menu: " + debugTN[i] + " delay of:" + delay);
	//			tabWipers[i].timer = 0-delay;
	//			tabWipers[i].isBottomLayer = false;
	//			tabWipers[i].wipeAmountLerp = 0 - delay;
	//		}
	//		tabWipers[currentMenu].WipeSetup(true, 1);
	//		tabWipers[goalMenu].WipeSetup(true, 0);
	//		tabWipers[goalMenu].isBottomLayer = true;
	//	}
	//	currentMenu = goalMenu;
	//	//Set goal layer under current layer, 
	//	print("OPENMENU: " + menuID);
	//}
}
