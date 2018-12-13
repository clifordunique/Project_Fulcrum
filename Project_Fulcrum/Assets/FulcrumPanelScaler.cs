using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class FulcrumPanelScaler : MonoBehaviour {

	public RectTransform myRectTransform;
	public int screenWidth;
	public int screenHeight;
	public int marginScale;


	// Use this for initialization
	void Start ()
	{
		myRectTransform = this.GetComponent<RectTransform>();
	}

	public void UpdateResolution()
	{
		screenHeight = Screen.height;
		screenWidth = Screen.width;
		marginScale = (int)((float)screenHeight * 0.04f);
		int trueHeight = screenHeight - marginScale;
		int trueWidth = screenWidth - marginScale;
		int posY = 0;
		int posX = marginScale;
		trueHeight -= posY;
		trueWidth -= posX;
		myRectTransform.sizeDelta = new Vector2(trueWidth, trueHeight);
	}

	private void OnEnable()
	{
		UpdateResolution();
	}

	// Update is called once per frame
	void Update ()
	{
		
	}
}
