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
	[SerializeField] [ReadOnlyAttribute] public bool isBottomLayer = true;
	//[SerializeField] [ReadOnlyAttribute] public float wipeAmountLerp = 1; // 0-1 range.

	[SerializeField] public MenuWipeCamera neighbourLeft;
	[SerializeField] public MenuWipeCamera neighbourRight;
	[SerializeField] public bool triggerWipe;


	//Managered variables
	[SerializeField] [ReadOnlyAttribute] public float curWipeAmount = 1; // 0-1 range.
	[SerializeField] [ReadOnlyAttribute] public float debugGlobalSliderValue = 0; // 0-4 range.
	[SerializeField] [ReadOnlyAttribute] public float debugGlobalSliderLerpValue = 0; // 0-4 range.




	//	void LateUpdate()
	//	{
	//		myInputCam.Render();
	//	}

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
		myInputCam = this.gameObject.GetComponent<Camera>();
		myInputCam.targetTexture = outputRT;
		myOutputPanel.GetComponent<RawImage>().texture = outputRT;
		myMaterial = Instantiate(myOutputPanel.GetComponent<RawImage>().material);
		myOutputPanel.GetComponent<RawImage>().material = myMaterial;
	}

	public void WipeSetup(bool wipesRight, float myBrightness)
	{
		//print("Set layer to orderDepth of:" + orderDepth);
		if (wipesRight)
		{
			isRightNeighbourOnTop = true;
			myMaterial.SetFloat("_SliceDirection", 1.0f);
			myMaterial.SetFloat("_Brightness", myBrightness);

		}
		else
		{
			isRightNeighbourOnTop = false;
			myMaterial.SetFloat("_SliceDirection", -1.0f);
			myMaterial.SetFloat("_Brightness", myBrightness);
		}
	}

	//private void Update()
	//{
	//	if (triggerWipe)
	//	{
	//		if (wipeAmountLerp < 1)
	//		{
	//			wipeAmountLerp = Mathf.Lerp(wipeAmountLerp, 1, Time.deltaTime);
	//			wipeAmountLerp += Time.deltaTime/5;
	//			if (myMaterial != null)
	//			{
	//				myMaterial.SetFloat("_SliceAmount", wipeAmountLerp);
	//			}
	//		}
	//		else
	//		{
	//			triggerWipe = false;
	//		}
	//	}
	//	if (isBottomLayer)
	//	{
	//		if (isRightNeighbourOnTop)
	//		{
	//			if (neighbourRight != null)
	//			{
	//				float brightnessM = neighbourRight.wipeAmountLerp;
	//				if (brightnessM > 1) { brightnessM = 1; }
	//				if (brightnessM < 0) { brightnessM = 0; }
	//				myMaterial.SetFloat("_Brightness", brightnessM);
	//			}
	//			else
	//			{
	//				myMaterial.SetFloat("_Brightness", 1);
	//			}
	//		}
	//		else
	//		{
	//			if (neighbourLeft != null)
	//			{
	//				float brightnessM = neighbourLeft.wipeAmountLerp;
	//				if (brightnessM > 1) { brightnessM = 1; }
	//				if (brightnessM < 0) { brightnessM = 0; }
	//				myMaterial.SetFloat("_Brightness", brightnessM);
	//			}
	//			else
	//			{
	//				myMaterial.SetFloat("_Brightness", 1);
	//			}
	//		}
	//	}
	//}

	private void Update()
	{
		debugGlobalSliderValue = myUITabManager.currentSlideValue;
		debugGlobalSliderLerpValue = myUITabManager.currentSlideLerp;
		//if (triggerWipe)
		//{
		//	if (wipeAmountLerp < 1)
		//	{
		//		wipeAmountLerp = Mathf.Lerp(wipeAmountLerp, 1, Time.deltaTime);
		//		wipeAmountLerp += Time.deltaTime / 5;
		//		if (myMaterial != null)
		//		{
		//			myMaterial.SetFloat("_SliceAmount", wipeAmountLerp);
		//		}
		//	}
		//	else
		//	{
		//		triggerWipe = false;
		//	}
		//}
		if (isBottomLayer)
		{
			if (isRightNeighbourOnTop)
			{
				if (neighbourRight != null)
				{
					float brightnessM = neighbourRight.curWipeAmount;
					if (brightnessM > 1) { brightnessM = 1; }
					if (brightnessM < 0) { brightnessM = 0; }
					myMaterial.SetFloat("_Brightness", brightnessM);
				}
				else
				{
					myMaterial.SetFloat("_Brightness", 1);
				}
			}
			else
			{
				if (neighbourLeft != null)
				{
					float brightnessM = neighbourLeft.curWipeAmount;
					if (brightnessM > 1) { brightnessM = 1; }
					if (brightnessM < 0) { brightnessM = 0; }
					myMaterial.SetFloat("_Brightness", brightnessM);
				}
				else
				{
					myMaterial.SetFloat("_Brightness", 1);
				}
			}
		}
	}
}
