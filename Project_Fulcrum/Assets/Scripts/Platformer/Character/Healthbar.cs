using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour 
{

	[SerializeField] private Image o_WhiteHealth;
	[SerializeField] private Image o_RedHealth;
	[SerializeField] private float whiteHealthAmount;
	[SerializeField] private float redHealthAmount;
	[SerializeField] private float maxHealth;


	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(whiteHealthAmount != redHealthAmount)
		{
			if(whiteHealthAmount < redHealthAmount)
			{
				whiteHealthAmount = redHealthAmount;
			}
			else
			{
				whiteHealthAmount = Mathf.Lerp(whiteHealthAmount, redHealthAmount, Time.deltaTime*5);
				if (Mathf.Abs(whiteHealthAmount-redHealthAmount)<=0.1f)
				{
					whiteHealthAmount = redHealthAmount;
				}
			}
		}
		o_RedHealth.fillAmount = redHealthAmount/maxHealth;
		o_WhiteHealth.fillAmount = whiteHealthAmount/maxHealth;
	}

	public void SetCurHealth(float currentHealth)
	{
		redHealthAmount = currentHealth;
	}

	public void SetMaxHealth(float  maxAmount)
	{
		maxHealth = maxAmount;
	}
}
