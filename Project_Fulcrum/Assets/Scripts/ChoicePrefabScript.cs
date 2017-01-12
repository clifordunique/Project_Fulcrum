using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChoicePrefabScript : MonoBehaviour {

	public GameObject neutralButton;
	public GameObject passiveButton;
	public GameObject activeButton;
	public GameObject wittyButton;
	public GameObject subversiveButton;

    public Choice choice;
    public SceneManager mySceneManager;

    public void SetChoice(Choice theChoice) 
	{
		print("SetChoice activating on: "+theChoice.choiceID);
        choice = theChoice;
		if (choice.options[1])
		{
			print("Passive choice = true, making interactable");
			passiveButton.GetComponent<Button>().interactable = true;
		}
		else
		{
			passiveButton.GetComponent<Button>().interactable = false;
		}
		if(choice.options[2])
		{
			activeButton.GetComponent<Button>().interactable = true;
		}
		else
		{
			activeButton.GetComponent<Button>().interactable = false;
		}
		if(choice.options[3])
		{
			wittyButton.GetComponent<Button>().interactable = true;
		}
		else
		{
			wittyButton.GetComponent<Button>().interactable = false;
		}
		if(choice.options[4])
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
		if (mySceneManager.activeChoicePrefab != this)
		{
			if (mySceneManager.activeChoicePrefab == null)
			{
				mySceneManager.activeChoicePrefab = this;
			}
			else
			{
				//print("IT'S NOT THIS");
				mySceneManager.activeChoicePrefab.neutralButton.SetActive(true);
				mySceneManager.activeChoicePrefab = this;
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
		//mySceneManager.SelectPassive(choice);
		//PassiveButton.SetActive(false);
	}

	public void ActiveClick()
	{
//		mySceneManager.SelectActive(choice);
//		neutralButton.SetActive(false);
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
