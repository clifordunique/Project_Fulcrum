using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCard : MonoBehaviour {

	[SerializeField] private Image frameImg;
	[SerializeField] private Image portraitBGImg;
	[SerializeField] private Image portraitImg;
	[SerializeField] private Image statPanelImg;
	[SerializeField] private Text nameTag;
	[SerializeField] private Image healthFillImg;
	[SerializeField] private Image stressFillImg;
	[SerializeField] private Image moraleFillImg;

	[SerializeField] private string nameTagText;

	[SerializeField] private Color frameColor;
	[SerializeField] private Color portraitBGColor;
	[SerializeField] private Color textColor;
	[SerializeField] private Color statPanelColor;

	[SerializeField] [Range(0, 16)] private int healthFill;
	[SerializeField] [Range(0, 16)] private int stressFill;
	[SerializeField] [Range(0, 16)] private int moraleFill;

	public void SetHealthFill(int i)
	{
		healthFill = i;
		UpdateCharacterCard();
	}

	public void SetStressFill(int i)
	{
		stressFill = i;
		UpdateCharacterCard();
	}

	public void SetMoraleFill(int i)
	{
		moraleFill = i;
		UpdateCharacterCard();
	}

	public void UpdateCharacterCard()
	{
		frameImg.color = frameColor;
		portraitBGImg.color = portraitBGColor;
		statPanelImg.color = statPanelColor;
		nameTag.text = nameTagText;
		nameTag.color = textColor;

		healthFillImg.fillAmount = (((float)healthFill)/16f);
		stressFillImg.fillAmount = (((float)stressFill)/16f);
		moraleFillImg.fillAmount = (((float)moraleFill)/16f);
	}

	private void OnMouseDown()
	{
		print("Character Card Clicked on!");
	}

}
