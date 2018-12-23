using UnityEngine;
using UnityEngine.UI;


//[ExecuteInEditMode]
public class MenuWipeCamera : MonoBehaviour
{

	//[SerializeField] private Camera[] cameraList;
	[SerializeField] private Material UIHologramMaterial;
	[SerializeField] private UITabManager myUITabManager;


	[SerializeField] [ReadOnlyAttribute] private RenderTexture outputRT;
	[SerializeField] [ReadOnlyAttribute] private Camera myInputCam; 
	[SerializeField] [ReadOnlyAttribute] private GameObject myOutputPanel;
	[SerializeField] [ReadOnlyAttribute] private MaterialPropertyBlock propBlock;
	[SerializeField] [ReadOnlyAttribute] public Material myMaterial;
	[SerializeField] [ReadOnlyAttribute] public bool isRightNeighbourOnTop;
	[SerializeField] [ReadOnlyAttribute] public bool isLeftNeighbourOnTop;
	[SerializeField] [ReadOnlyAttribute] public bool isBottomLayer = true;
	[SerializeField] [ReadOnlyAttribute] public bool wasBottomLayer = false; // Used to trigger a smooth brightness transition when the layer is changed from bottom layer to non bottom layer.
	[SerializeField] [ReadOnlyAttribute] public bool isTopLayer = false;
	//[SerializeField] [ReadOnlyAttribute] public float wipeAmountLerp = 1; // 0-1 range.

	public int menuID;
	[SerializeField] public MenuWipeCamera neighbourLeft;
	[SerializeField] public MenuWipeCamera neighbourRight;
	[SerializeField] public bool triggerWipe;


	//Managered variables
	[SerializeField] [ReadOnlyAttribute] public float curWipeAmount = 1; // 0-1 range.
	[SerializeField] [ReadOnlyAttribute] public float curBrightness = 1; // 0-1 range.
	[SerializeField] [ReadOnlyAttribute] public float curBrightnessLerp = 1; // 0-1 range.
	[SerializeField] [ReadOnlyAttribute] public float debugGlobalSliderValue = 0; // 0-4 range.
	[SerializeField] [ReadOnlyAttribute] public float debugGlobalSliderLerpValue = 0; // 0-4 range.

	//V3 variables
	public int moveState; // 1, 0, or -1  for right, stationary, and left respectively. 
	public int currentLayerDepth;
	public float goalValue;
	public bool hasAGoal;
	public float offset;

	// Lerp variables
	public float followWipeExp = 1;
	public float followWipeLinear = 0.2f;
	public float leadWipeExp = 0.5f;
	public float leadWipeLinear = 0.1f;
	public float brightnessExp = 0.5f;
	public float brightnessLinear = 0.1f;



	// Use this for initialization
	void Start()
	{
		int scrW = Screen.width;
		int scrH = Screen.height;
		outputRT = new RenderTexture((int)(scrW), (int)(scrH), 24);
		//		outputRT.anisoLevel = 0;
		outputRT.filterMode = FilterMode.Point;
		//		outputRT.antiAliasing = 1;
		outputRT.Create();
		isBottomLayer = false;
		wasBottomLayer = false;
		myInputCam = this.gameObject.GetComponent<Camera>();
		myInputCam.targetTexture = outputRT;
		myOutputPanel.GetComponent<RawImage>().texture = outputRT;
		myMaterial = Instantiate(myOutputPanel.GetComponent<RawImage>().material);
		myOutputPanel.GetComponent<RawImage>().material = myMaterial;
	}

	void UpdateWipeRight()
	{
		if (neighbourRight != null)
			{neighbourRight.isLeftNeighbourOnTop = true;}

		if (isLeftNeighbourOnTop) // FOLLOW WIPE
		{
			float localGoal = neighbourLeft.curWipeAmount - 0.1f;
			curWipeAmount = Mathf.Lerp(curWipeAmount, localGoal, Time.smoothDeltaTime * 2.5f *followWipeExp);
			if (curWipeAmount < localGoal) { curWipeAmount += Time.smoothDeltaTime * 2.5f  * followWipeLinear; }
			if (curWipeAmount > localGoal) { curWipeAmount -= Time.smoothDeltaTime * 2.5f  * followWipeLinear; }
			if (Mathf.Abs(curWipeAmount - localGoal) <= 0.001f)
			{ curWipeAmount = localGoal; }
		}
		else // LEADING WIPE
		{
			bool a = (goalValue == 0 && curWipeAmount > 0);
			bool b = (goalValue == 1 && curWipeAmount < 1);
			bool c = (goalValue < 1 && goalValue > 0);
			if (a || b || c)
			{
				curWipeAmount = Mathf.Lerp(curWipeAmount, goalValue, Time.smoothDeltaTime * 2.5f  * leadWipeExp);
				if (curWipeAmount < goalValue) { curWipeAmount += Time.smoothDeltaTime * 2.5f  * leadWipeLinear; }
				else if (curWipeAmount > goalValue) { curWipeAmount -= Time.smoothDeltaTime * 2.5f  * leadWipeLinear; }
				if (Mathf.Abs(curWipeAmount - goalValue) <= 0.001f)
				{ curWipeAmount = goalValue; }
			}
			else
			{
				curWipeAmount = goalValue;
			}
		}
	}

	void UpdateWipeLeft()
	{
		if (neighbourLeft != null)
			{neighbourLeft.isRightNeighbourOnTop = true;}

		if (isRightNeighbourOnTop) // FOLLOW WIPE
		{
			float localGoal = neighbourRight.curWipeAmount - 0.1f;
			curWipeAmount = Mathf.Lerp(curWipeAmount, localGoal, Time.smoothDeltaTime * 2.5f  * followWipeExp);
			if (curWipeAmount < localGoal) { curWipeAmount += Time.smoothDeltaTime * 2.5f  * followWipeLinear; }
			else if (curWipeAmount > localGoal) { curWipeAmount -= Time.smoothDeltaTime * 2.5f  * followWipeLinear; }
			if (Mathf.Abs(curWipeAmount - localGoal) <= 0.001f)
			{ curWipeAmount = localGoal; }
		}
		else // LEADING WIPE
		{
			bool a = (goalValue == 0 && curWipeAmount > 0);
			bool b = (goalValue == 1 && curWipeAmount < 1);
			bool c = (goalValue < 1 && goalValue > 0);
			if (a || b || c)
			{
				curWipeAmount = Mathf.Lerp(curWipeAmount, goalValue, Time.smoothDeltaTime * 2.5f  * leadWipeExp);
				if (curWipeAmount < goalValue) { curWipeAmount += Time.smoothDeltaTime * 2.5f  * leadWipeLinear; }
				else if (curWipeAmount > goalValue) { curWipeAmount -= Time.smoothDeltaTime * 2.5f  * leadWipeLinear; }
				if (Mathf.Abs(curWipeAmount - goalValue) <= 0.001f)
				{ curWipeAmount = goalValue; }
			}
			else
			{
				curWipeAmount = goalValue;
			}
		}
	}

	private void SetBrightness( bool applyInstantly)
	{
		if (!isLeftNeighbourOnTop && !isRightNeighbourOnTop && goalValue == 0)
		{
			currentLayerDepth = 0;
		}
		if (isBottomLayer)
		{
			applyInstantly = true;
			if (isRightNeighbourOnTop && neighbourRight != null)
			{
				if ( neighbourRight.moveState !=0 )
				{
					float brightnessM = neighbourRight.curWipeAmount*1.3f;
					if (brightnessM > 1) { brightnessM = 1; }
					if (brightnessM < 0) { brightnessM = 0; }
					curBrightness = brightnessM;
				}
				else
				{
					curBrightness = 1;
				}
			}
			else if(isLeftNeighbourOnTop && neighbourLeft != null)
			{
				if (neighbourLeft.moveState != 0)
				{
					float brightnessM = neighbourLeft.curWipeAmount * 1.3f;
					if (brightnessM > 1) { brightnessM = 1; }
					if (brightnessM < 0) { brightnessM = 0; }
					curBrightness = brightnessM;
				}
				else
				{
					curBrightness = 1;
				}
			}
		}
		else
		{
			curBrightness = 0.5f + (0.5f / ((float)currentLayerDepth + 1));
		}

		if (applyInstantly)
		{
			curBrightnessLerp = curBrightness;
		}
		else
		{
			if (Mathf.Abs(curBrightnessLerp - curBrightness) > 0.01f)
			{
				curBrightnessLerp = Mathf.Lerp(curBrightnessLerp, curBrightness, Time.smoothDeltaTime * 2.5f );
				if (curBrightnessLerp < curBrightness) { curBrightnessLerp += Time.smoothDeltaTime * 2.5f  / 10; }
				if (curBrightnessLerp > curBrightness) { curBrightnessLerp -= Time.smoothDeltaTime * 2.5f  / 10; }
				if (Mathf.Abs(curBrightnessLerp - curBrightness) <= 0.01f)
				{ curBrightnessLerp = curBrightness; }
			}
		}


		myMaterial.SetFloat("_Brightness", curBrightnessLerp);


	}

	public void CompleteTransition()
	{
		//print("Transition complete for " + myUITabManager.debugTN[menuID]);
		myUITabManager.movingTabsCount--;

		if (goalValue == 1)
		{
			myUITabManager.tabRenderLayers[menuID].SetSiblingIndex(1);
			myUITabManager.currentMenu = menuID + moveState; // gives you the next menu in the requested direction.
		}
		else if (goalValue == 0)
		{
			//myUITabManager.tabRenderLayers[menuID].SetSiblingIndex(5);
			//myUITabManager.currentMenu = menuID;
		}
		moveState = 0;
		curWipeAmount = 0;
		myMaterial.SetFloat("_SliceAmount", curWipeAmount);
		curBrightness = 1;
		currentLayerDepth = 0;
		wasBottomLayer = false;
		myMaterial.SetFloat("_Brightness", 1);

		if (neighbourLeft != null)
		{
			neighbourLeft.isRightNeighbourOnTop = false;
		}
		if (neighbourRight != null)
		{
			neighbourRight.isLeftNeighbourOnTop = false;
		}
		//currentLayerDepth = -1; //Set to the unused value;
	}

	public void StartTransition(int direction)
	{
		if (direction == 0|| moveState != 0) { return; }
		myUITabManager.movingTabsCount++;
		moveState = direction;
		myMaterial.SetFloat("_SliceDirection", direction);
		wasBottomLayer = false;
		//SetBrightness(true);
	}

	private void Update()
	{
		debugGlobalSliderValue = myUITabManager.currentSlideValue;

		if (moveState == 0)
			{ isBottomLayer = true; }
		else if(isBottomLayer)
		{
			isBottomLayer = false;
			wasBottomLayer = true;
		}

		SetBrightness(!wasBottomLayer);

		if (moveState == 0)
			{return;}


		offset = myUITabManager.currentSlideValue - menuID;

		if (moveState == 1)
		{
			if (offset >= 1 )
			{
				goalValue = 1;
				hasAGoal = true;
			}
			else if (offset <= 0 )
			{
				goalValue = 0;
				hasAGoal = true;
			}
			else
			{
				//print("nogoal triggered on +1 movestate");
				goalValue = Mathf.Abs(offset);
				hasAGoal = false;
			}
			UpdateWipeRight();
		}

		if (moveState == -1)
		{
			if (offset <= -1)
			{
				goalValue = 1;
				hasAGoal = true;
			}
			else if (offset >= 0)
			{
				goalValue = 0;
				hasAGoal = true;
			}
			else
			{
				//print("nogoal triggered on -1 movestate");
				goalValue = Mathf.Abs(offset);
				hasAGoal = false;
			}
			UpdateWipeLeft();
		}

		myMaterial.SetFloat("_SliceAmount", curWipeAmount);

		if (curWipeAmount == goalValue && hasAGoal)
		{
			CompleteTransition();
		}

	}
}
