using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class FulcrumPanelScaler : MonoBehaviour {

	public FulcrumPanelScaler myParent;

	public bool percentScaling = true;

	public Canvas myCanvas;
	public RectTransform myRectTransform;
	public RectTransform myParentTransform;
	//public int screenWidth;
	//public int screenHeight;
	//public int marginScale;
	public bool evenMargins = true;




	private float screenWidth;
	private float screenHeight;
	private float marginScale;
	private float bottomMarginScale;

	public int unscaledOffsetTop;
	public int unscaledOffsetBot;
	public int unscaledOffsetLeft;
	public int unscaledOffsetRight;

	public int defaultWidth = 1920;
	public int defaultHeight = 1080;

	public float parentPixelWidth;
	public float parentPixelHeight;
	public float myPixelWidth;
	public float myPixelHeight;

	[ReadOnlyAttribute] public float localAnchorOffsetTop;
	[ReadOnlyAttribute] public float localAnchorOffsetBot;
	[ReadOnlyAttribute] public float localAnchorOffsetLeft;
	[ReadOnlyAttribute] public float localAnchorOffsetRight;

	[ReadOnlyAttribute] public float percentOffsetTop;
	[ReadOnlyAttribute] public float percentOffsetBot;
	[ReadOnlyAttribute] public float percentOffsetLeft;
	[ReadOnlyAttribute] public float percentOffsetRight;

	[ReadOnlyAttribute] public int scaledOffsetTop;
	[ReadOnlyAttribute] public int scaledOffsetBot;
	[ReadOnlyAttribute] public int scaledOffsetLeft;
	[ReadOnlyAttribute] public int scaledOffsetRight;

	// Use this for initialization
	void Start ()
	{

	}

	public void UpdateResolution()
	{
		if (percentScaling) { return; }
		screenHeight = Screen.height;
		screenWidth = Screen.width;

		percentOffsetTop = (float)unscaledOffsetTop/ (float)defaultHeight;
		percentOffsetBot = (float)unscaledOffsetBot / (float)defaultHeight;

		if (evenMargins)
		{
			percentOffsetLeft = (float)unscaledOffsetLeft / (float)defaultHeight;
			percentOffsetRight = (float)unscaledOffsetRight / (float)defaultHeight;
		}
		else
		{
			percentOffsetLeft = (float)unscaledOffsetLeft / (float)defaultWidth;
			percentOffsetRight = (float)unscaledOffsetRight / (float)defaultWidth;
		}
		scaledOffsetTop = (int)(percentOffsetTop*screenHeight);
		scaledOffsetBot = (int)(percentOffsetBot*screenHeight);



		if (evenMargins)
		{
			scaledOffsetLeft = (int)(percentOffsetLeft * screenHeight);
			scaledOffsetRight = (int)(percentOffsetRight * screenHeight);
		}
		else
		{
			scaledOffsetLeft = (int)(percentOffsetLeft * screenWidth);
			scaledOffsetRight = (int)(percentOffsetRight * screenWidth);
		}

		myRectTransform.offsetMax = new Vector2(-scaledOffsetRight, -scaledOffsetTop);
		myRectTransform.offsetMin = new Vector2(scaledOffsetLeft, scaledOffsetBot);
	}

	private void OnEnable()
	{
		myRectTransform = this.GetComponent<RectTransform>();
		myParentTransform = this.myRectTransform.parent.GetComponent<RectTransform>();
		myParent = myParentTransform.GetComponent<FulcrumPanelScaler>();
		UpdateResolution();
	}

	// Update is called once per frame
	void Update ()
	{
		
	}
}
