using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergySpiralUI : MonoBehaviour {


	private Image[] o_SegmentImages;
	private float[] segmentColLerp;
	private float[] segmentColTarget;


	[SerializeField] private Color defCol; 
	[SerializeField] private Color litCol;
	[SerializeField] private Color surgeCol;

	[SerializeField][ReadOnlyAttribute] private int currentEnergy;

	int iterator = 0;
	int delay = 0;
	int currentSetting = 0;

	// Use this for initialization
	void Start () 
	{
		o_SegmentImages = new Image[24];
		segmentColLerp = new float[24];
		segmentColTarget = new float[24];

		for(int i = 0; i<24; i++)
		{
			o_SegmentImages[i] = this.transform.GetChild(i).GetComponent<Image>();
			o_SegmentImages[i].color = defCol;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		SpiralUpdate();
	}

	public void SetCurEnergy(int curEnergy)
	{
		if(curEnergy>24){curEnergy = 24;}
		for(int i = 0; i<curEnergy; i++)
		{
			segmentColTarget[i] = 1;
		}
		for(int i = curEnergy; i<24; i++)
		{
			segmentColTarget[i] = 0;
		}
	}

	void SpiralTestUpdate()
	{
		for(int i = 0; i<24; i++)
		{
			segmentColLerp[i] = Mathf.Lerp(segmentColLerp[i], segmentColTarget[i], Time.fixedDeltaTime*5);
			o_SegmentImages[i].color = Color.Lerp(defCol, litCol, segmentColLerp[i]);
		}

		delay++;
		if(delay<5){return;}
		delay = 0;

		if(iterator==24)
		{	
			iterator=0;
			for(int i = 0; i<24; i++)
			{
				o_SegmentImages[i].color = defCol;
				segmentColTarget[i] = 0;
				segmentColLerp[i] = 0;
			}
			return;
		}

		segmentColTarget[iterator] = 1;

		if(iterator==23)
		{
			delay = -50;
		}

		iterator++;
	}

	void SpiralUpdate()
	{
		for(int i = 0; i<24; i++)
		{
			segmentColLerp[i] = Mathf.Lerp(segmentColLerp[i], segmentColTarget[i], Time.fixedDeltaTime*5);
			o_SegmentImages[i].color = Color.Lerp(defCol, litCol, segmentColLerp[i]);
		}

		delay++;
		if(delay<5){return;}
		delay = 0;



//		if(iterator==24)
//		{	
//			iterator=0;
//			for(int i = 0; i<24; i++)
//			{
//				o_SegmentImages[i].color = defCol;
//				segmentColTarget[i] = 0;
//				segmentColLerp[i] = 0;
//			}
//			return;
//		}
//
//		segmentColTarget[iterator] = 1;
//
//		if(iterator==23)
//		{
//			delay = -50;
//		}
//
//		iterator++;
	}
}
