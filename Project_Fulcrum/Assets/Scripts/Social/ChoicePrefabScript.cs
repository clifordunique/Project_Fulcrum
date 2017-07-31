using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Social;

namespace Social
{
	public class ChoicePrefabScript : MonoBehaviour {

		public GameObject neutralButton;
		public GameObject passiveButton;
		public GameObject activeButton;
		public GameObject wittyButton;
		public GameObject subversiveButton;
		public int activeModifier = 0;

	    public Choice choice;
	    public SocialManager mySceneManager;

	    public void SetChoice(Choice theChoice) 
		{
			print("SetChoice activating on: "+theChoice.choiceID);
	        choice = theChoice;
			if (choice.options[(int)MODIFIER.Passive])
			{
				print("Passive choice available, making button interactable");
				passiveButton.GetComponent<Button>().interactable = true;
			}
			else
			{
				passiveButton.GetComponent<Button>().interactable = false;
			}
			if(choice.options[(int)MODIFIER.Active])
			{
				activeButton.GetComponent<Button>().interactable = true;
			}
			else
			{
				activeButton.GetComponent<Button>().interactable = false;
			}
			if(choice.options[(int)MODIFIER.Witty])
			{
				wittyButton.GetComponent<Button>().interactable = true;
			}
			else
			{
				wittyButton.GetComponent<Button>().interactable = false;
			}
			if(choice.options[(int)MODIFIER.Subversive])
			{
				subversiveButton.GetComponent<Button>().interactable = true;
			}
			else
			{
				subversiveButton.GetComponent<Button>().interactable = false;
			}
	    }

	    public void NeutralClick()
		{
			if (mySceneManager.p_ActiveChoicePrefab != this)
			{
				if (mySceneManager.p_ActiveChoicePrefab == null)
				{
					mySceneManager.p_ActiveChoicePrefab = this;
				}
				else
				{
					//print("IT'S NOT THIS");
					mySceneManager.p_ActiveChoicePrefab.neutralButton.SetActive(true);
					mySceneManager.p_ActiveChoicePrefab = this;
				}
			}
			else
			{
				//print("IT'S THIS!!!!!!!!!!!!!");
			}
	        mySceneManager.SelectNeutral(choice);
			neutralButton.SetActive(false);
	    }

		public void PassiveClick()
		{
			mySceneManager.SelectPassive(choice);
		}

		public void ActiveClick()
		{
			mySceneManager.SelectActive(choice);
		}

		public void WittyClick()
		{
			mySceneManager.SelectWitty(choice);
		}

		public void SubversiveClick()
		{
			mySceneManager.SelectSubversive(choice);
		}

		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}
	}
}