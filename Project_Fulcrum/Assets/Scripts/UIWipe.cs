using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWipe : MonoBehaviour {
	private Image UIWipeImage;
	private float timer = 5;
	[SerializeField] private float wipespeed = 5;

	void Start ()
	{
		UIWipeImage = this.GetComponent<Image>();
		UIWipeImage.material.SetFloat("_SliceAmount", 0);

	}

	void Update()
	{

		if (timer > 0)
		{

			if (timer < 1)
			{
				timer -= Time.deltaTime * wipespeed;
				UIWipeImage.material.SetFloat("_SliceAmount", 1 - timer);
			}
			else
			{
				timer -= Time.deltaTime * 2;
			}
		}
		else if (timer > -5)
		{
			timer -= Time.deltaTime * 2;
		}
		else
		{
			timer = 5;
		}

	}
}
