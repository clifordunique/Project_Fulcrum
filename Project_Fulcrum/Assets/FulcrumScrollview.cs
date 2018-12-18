using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FulcrumScrollview : MonoBehaviour {

	[ReadOnlyAttribute] public float scrollInput;
	public RectTransform contentPanel;
	public float contentPanelHeight;

	public RectTransform scrollViewPanel;
	public float scrollViewHeight;

	public float heightDifference = -1;
	public float currentOffset;
	public float cPanelStartingPosition;
	public Vector3[] fourCorners;

	// Use this for initialization
	void Start ()
	{
		if (contentPanel == null)
		{
			contentPanel = this.transform.GetChild(0).GetComponent<RectTransform>();
		}
		scrollViewPanel = this.GetComponent<RectTransform>();

		heightDifference = -1;

		cPanelStartingPosition = contentPanel.localPosition.y;
	}

	private void OnMouseOver()
	{
		scrollInput = Input.GetAxis("Mouse ScrollWheel");
		if (scrollInput > 0)
		{
			//print("Going up!");
			currentOffset -= 50;
			if (currentOffset < 0)
			{
				//print("Reached upward movement max");
				currentOffset = 0;
			}
			contentPanel.localPosition = new Vector2(contentPanel.localPosition.x, cPanelStartingPosition + currentOffset);


		}
		else if (scrollInput < 0)
		{
			//print("Going down!");
			currentOffset += 50;
			if (currentOffset > heightDifference)
			{
				currentOffset = heightDifference;
				//print("Reached downward movement max");
			}
			contentPanel.localPosition = new Vector2(contentPanel.localPosition.x, cPanelStartingPosition + currentOffset);

		}
	}

	// Update is called once per frame
	void Update ()
	{
		if (contentPanelHeight == 0)
		{
			contentPanelHeight = contentPanel.rect.height;
		}
		if (scrollViewHeight == 0)
		{
			scrollViewHeight = scrollViewPanel.rect.height;
		}
		if (scrollViewHeight != 0 && contentPanelHeight != 0 && heightDifference < 0)
		{
			heightDifference = contentPanelHeight - scrollViewHeight;
			//print("Setting heightdifference!!");
			if (heightDifference < 0)
			{
				//print(contentPanelHeight + " is less than " + scrollViewHeight + "!!");
				heightDifference = 0;
			}
		}
	}
}
