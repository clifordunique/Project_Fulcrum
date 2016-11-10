using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Text;
using System.IO;  

public enum STATE 
{
    Closed,
    Dialogue,
    Choice,
};
    
public enum MODIFIER
{
  Neutral,
  Active,
  Witty,
  Passive,
  Subversive
};


public class SceneManager : MonoBehaviour {

    public GameObject menuChoicePrefab;
    public RectTransform ScrollMenu;
    public InputField LeftDialogue;
    public InputField RightDialogue;
	public Animator SocialMenu;
	public Animator ActorAnimator;
    public Choice selectedChoice;
    public int modifier = -1;
    private int currentDialogue = 0;
	private DNode activeNode;
	private bool nodeChanging = false;

	// Use this for initialization
	void Start () {
	}

    public void ShowChoices(){
        RightDialogue.text = "";
        LeftDialogue.text = "";
        SocialMenu.SetInteger("state", (int)STATE.Choice);
		ActorAnimator.SetInteger("state", (int)STATE.Choice);
    }

    public void OkayButton(){
        if (SocialMenu.GetInteger("state") == (int)STATE.Dialogue) //Advances dialogue if dialogue screen is already open
        {
            //print("currentDialogue = " + currentDialogue + ", dialoguelength = " + activeNode.speech.Length);
            if (currentDialogue + 2 <= activeNode.speech.Length)
            {
                currentDialogue++;
                SetDialogue(activeNode.speech[currentDialogue]);
            }
            else
            {
				if (nodeChanging) 
				{
					ChangeNode(selectedChoice.outcome[modifier]);
					nodeChanging = false;
                    modifier = -1;
				} 
				else 
				{
					ShowChoices();
				}
                
            }
        }
        else //Begins the dialogue chain of the specific choice
        {
            if (selectedChoice != null && modifier >= 0)
            {	
				SetDialogue(selectedChoice.playerDialogue[0]);
				ActorAnimator.SetInteger("state", (int)STATE.Dialogue);
				SocialMenu.SetInteger("state", (int)STATE.Dialogue);
				nodeChanging = true;
            }
        }
    }

	public void StartConvo(DNode startNode)
	{
		ChangeNode(startNode);
        ConvoToJson(startNode);
	}

	public void ExitConvo()
	{
		activeNode = null;
		selectedChoice = null;
		modifier = -1;
		currentDialogue = 0;
		nodeChanging = false;
		ActorAnimator.SetInteger("state", (int)STATE.Closed);
		SocialMenu.SetInteger("state", (int)STATE.Closed);
	}

	public void SetDialogue(Dialogue dialogue){
        if (dialogue.leftSide)
        {
            LeftDialogue.text = dialogue.text;
            LeftDialogue.GetComponent<TextFieldModder>().activeDialogue = dialogue;
            RightDialogue.text = "";
        }
        else
        {
            RightDialogue.text = dialogue.text;
            RightDialogue.GetComponent<TextFieldModder>().activeDialogue = dialogue;
            LeftDialogue.text = "";
        }
		if (dialogue.exit) 
		{
            ExitConvo();
		}
	}

    public void SelectNeutral(Choice clickedChoice)
    {
        if (clickedChoice.playerDialogue[0] != null)
        {
            selectedChoice = clickedChoice;
            SetDialogue(clickedChoice.playerDialogue[0]);
            modifier = (int)MODIFIER.Neutral;
        }
        else
        {
            print("No neutral response available.");
        }
    }

	public void ChangeNode(DNode newNode){
		if (activeNode != null) {
			for (int i = 0; i < activeNode.Responses.Length; i++) {
				Destroy(GameObject.Find("Choice_" + i));
			}
		}
		activeNode = newNode;
        print("Now switched to node: "+ activeNode.nodeName);
		for(int i = 0; i < activeNode.Responses.Length; i++){
            print("Choice number " + i + " created");
			GameObject newChoice = (GameObject)Instantiate(menuChoicePrefab);
			newChoice.transform.SetParent(ScrollMenu, false);
            newChoice.GetComponent<Text>().text = activeNode.Responses[i].choiceName;
			newChoice.name = "Choice_"+i;
            print(newChoice.GetComponent<ChoicePrefabScript>());
            newChoice.GetComponent<ChoicePrefabScript>().SetChoice(activeNode.Responses[i]);
            newChoice.GetComponent<ChoicePrefabScript>().mySceneManager = this;
//			activeChoices[i] = newChoice;
		}
		//SocialMenu.CrossFade ("Dialogue", 0.5f);
        SocialMenu.SetInteger("state", (int)STATE.Dialogue);
		ActorAnimator.SetInteger("state", (int)STATE.Dialogue);
        currentDialogue = 0;
        SetDialogue(activeNode.speech[0]);
      
	}
	
    public void JsonToConvo()
    {
    


    }

    public void ConvoToJson(DNode root)
    {
        string jsonString = JsonUtility.ToJson(root);
        string outPath = Application.persistentDataPath + "Node_" + root.nodeID + ".json";
        print(outPath);
        //FileStream output = File.Create(Application.persistentDataPath + "Node_" + root.nodeID + ".json");
        System.IO.File.WriteAllText(outPath, jsonString);
    }
}
