using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VigorBar : MonoBehaviour 
{
	[SerializeField] private Image o_AltOuter;
	[SerializeField] private Image o_AltInner;
	[SerializeField] private Image o_RedOuter;
	[SerializeField] private Image o_RedInner;

	[SerializeField] private float whiteVigorAmount;
	[SerializeField] private float redVigorAmount;

	[SerializeField] private float lerpWhiteOuterAmount;
	[SerializeField] private float lerpWhiteInnerAmount;

	[SerializeField] private float lerpRedOuterAmount;
	[SerializeField] private float lerpRedInnerAmount;



	[SerializeField] private float maxVigor;


	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		UpdateVigorType2();
	}

	void UpdateVigorType2()
	{
		if(lerpRedOuterAmount != redVigorAmount)
		{
			if(lerpRedOuterAmount < redVigorAmount)
			{
				lerpRedOuterAmount = redVigorAmount;
				lerpRedInnerAmount = redVigorAmount;
				lerpWhiteOuterAmount = redVigorAmount;
				lerpWhiteInnerAmount = redVigorAmount;
			}
			else
			{
				
				lerpRedOuterAmount = Mathf.Lerp(lerpRedOuterAmount, redVigorAmount, Time.deltaTime*10);
				lerpRedInnerAmount = Mathf.Lerp(lerpRedInnerAmount, lerpRedOuterAmount, Time.deltaTime*10);

				lerpWhiteOuterAmount = Mathf.Lerp(lerpWhiteOuterAmount, lerpRedOuterAmount, Time.deltaTime*10);
				lerpWhiteInnerAmount =  Mathf.Lerp(lerpWhiteInnerAmount, lerpWhiteOuterAmount, Time.deltaTime*10);

				lerpWhiteOuterAmount = Mathf.Lerp(lerpWhiteOuterAmount, lerpWhiteInnerAmount, Time.deltaTime*5);
				if (Mathf.Abs(whiteVigorAmount-redVigorAmount)<=0.1f)
				{
					whiteVigorAmount = redVigorAmount;
				}
			}
		}

		o_RedInner.fillAmount = lerpRedInnerAmount/maxVigor;
		o_RedOuter.fillAmount = lerpRedOuterAmount/maxVigor;
		o_AltInner.fillAmount = lerpWhiteInnerAmount/maxVigor;
		o_AltOuter.fillAmount = lerpWhiteOuterAmount/maxVigor;
//		o_AltInner.fillAmount = 1f;
//		o_AltOuter.fillAmount = 1f;
	}

	void UpdateVigorType1()
	{
		if(whiteVigorAmount != redVigorAmount)
		{
			if(whiteVigorAmount < redVigorAmount)
			{
				whiteVigorAmount = redVigorAmount;
			}
			else
			{
				whiteVigorAmount = Mathf.Lerp(whiteVigorAmount, redVigorAmount, Time.deltaTime*5);
				if (Mathf.Abs(whiteVigorAmount-redVigorAmount)<=0.1f)
				{
					whiteVigorAmount = redVigorAmount;
				}
			}
		}

		o_RedInner.fillAmount = redVigorAmount/maxVigor;
		o_RedOuter.fillAmount = redVigorAmount/maxVigor;
		o_AltInner.fillAmount = whiteVigorAmount/maxVigor;
		o_AltOuter.fillAmount = whiteVigorAmount/maxVigor;
	}


	public void SetCurVigor(float currentVigor)
	{
		redVigorAmount = currentVigor;
	}

	public void SetMaxVigor(float  maxAmount)
	{
		maxVigor = maxAmount;
	}
}
