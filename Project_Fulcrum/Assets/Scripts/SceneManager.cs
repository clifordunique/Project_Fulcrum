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

	public ChoicePrefabScript activeChoicePrefab;
	public Scene[] sceneList;
	public GameObject ChoicePrefab;
	public GameObject DialoguePrefab;
    public GameObject menuChoicePrefab;
	public GameObject ScenePrefab;
	public GameObject DNodePrefab;
    public RectTransform ScrollMenu;
    public InputField LeftDialogue;
    public InputField RightDialogue;
	public Animator SocialMenu;
	public Animator ActorAnimator;
    public Choice selectedChoice;
    public int modifier = -1;
    private int currentDialogue = 0;
	private DNode activeNode;
	public Scene activeScene;
	private bool nodeChanging = false;
	public bool manualSave;

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
					
					//ChangeNode(selectedChoice.outcome[modifier]);
					if (selectedChoice.outcomeID[modifier] < 0)
					{
						ChangeNode(activeScene.nodes[0]);
						nodeChanging = false;
						modifier = -1;
					}
					else
					{
						ChangeNode(activeScene.nodes[selectedChoice.outcomeID[modifier]]);
						nodeChanging = false;
						modifier = -1;
					}
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

	public void StartScene(int sceneID)
	{
		Scene scene = JsonToScene(sceneID);
		if (scene == null)
		{
			print("SCENE_" + sceneID + " NOT FOUND");
		}
		else
		{
			activeScene = scene;
	        ChangeNode(scene.root);
		}
	}

	public void ExitScene()
	{
		if (manualSave)
		{
			SceneToJson(activeScene);
		}
		activeScene = null;
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
			RightDialogue.GetComponent<TextFieldModder>().activeDialogue = null;
        }
        else
        {
            RightDialogue.text = dialogue.text;
            RightDialogue.GetComponent<TextFieldModder>().activeDialogue = dialogue;
            LeftDialogue.text = "";
			LeftDialogue.GetComponent<TextFieldModder>().activeDialogue = null;
        }
		if (dialogue.exit) 
		{
            ExitScene();
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
        	//print("No neutral response available.");
        }
    }

	public void ChangeNode(DNode newNode){
		//print("Switching to node: "+ newNode);
		if (activeNode != null) {
			for (int i = 0; i < activeNode.responses.Length; i++) {
				Destroy(GameObject.Find("Choice_" + i));
			}
		}
		activeNode = newNode;
        //print("Now switched to node: "+ activeNode.nodeName);
		for(int i = 0; i < activeNode.responses.Length; i++){
            //print("Choice number " + i + " created");
			GameObject newChoice = (GameObject)Instantiate(menuChoicePrefab);
			newChoice.transform.SetParent(ScrollMenu, false);
            newChoice.GetComponent<Text>().text = activeNode.responses[i].choiceName;
			newChoice.name = "Choice_"+i;
            //print(newChoice.GetComponent<ChoicePrefabScript>());
            newChoice.GetComponent<ChoicePrefabScript>().SetChoice(activeNode.responses[i]);
            newChoice.GetComponent<ChoicePrefabScript>().mySceneManager = this;
//			activeChoices[i] = newChoice;
		}
		//SocialMenu.CrossFade ("Dialogue", 0.5f);
        SocialMenu.SetInteger("state", (int)STATE.Dialogue);
		ActorAnimator.SetInteger("state", (int)STATE.Dialogue);
        currentDialogue = 0;
        SetDialogue(activeNode.speech[0]);
	}



//
//
// SCENE LOADING FROM JSON FUNCTIONS
//
//


	public Scene JsonToScene(int sceneID)
	{
		Transform requestedScene = transform.Find("Scene_"+sceneID);
		if(requestedScene!=null)
		{
			activeScene = requestedScene.gameObject.GetComponent<Scene>();
			return activeScene;
			//print("Scene already loaded. Using.");
		}
		else
		{
			//print("Loading Scene from JSON.");
			string sceneDirectory = Application.persistentDataPath + "/Scenes/Scene_" + sceneID + "/";
			if (Directory.Exists(sceneDirectory))
			{
				GameObject sceneObject = (GameObject)Instantiate(ScenePrefab);
				sceneObject.name = "Scene_"+sceneID;
				sceneObject.transform.SetParent(this.gameObject.transform, false);
				Scene myScene = sceneObject.GetComponent<Scene>();
				myScene.sceneID = sceneID;
				myScene.GenFromJson(sceneDirectory + "Scene_" + sceneID + ".json");
				for (int i = 0; i < myScene.nodes.Length; i++)
				{
					myScene.nodes[i] = JsonToDNode(sceneObject, sceneDirectory, i);
				}
				myScene.root = myScene.nodes[myScene.rootID];

				return myScene;
			}
			else
			{
				return null;
			}
		}
	}

	public DNode JsonToDNode(GameObject parent, string dir, int dnodeID)
	{
		string dnodeDirectory = dir + "DNode_" + dnodeID + "/";

		if (Directory.Exists(dnodeDirectory))
		{
			GameObject dnodeObject = (GameObject)Instantiate(DNodePrefab);
			dnodeObject.name = "DNode_"+dnodeID;
			dnodeObject.transform.SetParent(parent.transform, false);
			DNode myDNode = dnodeObject.GetComponent<DNode>();
			myDNode.dnodeID = dnodeID;
			myDNode.GenFromJson(dnodeDirectory + "DNode_" + dnodeID + ".json");

			for (int i = 0; i < myDNode.responses.Length; i++)
			{
				myDNode.responses[i] = JsonToChoice(dnodeObject, dnodeDirectory, i);
			}

			for (int i = 0; i < myDNode.speech.Length; i++)
			{
				myDNode.speech[i] = JsonToDialogue(dnodeObject, dnodeDirectory, i);
			}

			return myDNode;
		}
		else
		{
			return null;
		}
	}
		
	public Choice JsonToChoice(GameObject parent, string dir, int choiceID)
	{
		string choiceDirectory = dir + "Choice_" + choiceID + "/";

		if (Directory.Exists(choiceDirectory))
		{
			GameObject choiceObject = (GameObject)Instantiate(ChoicePrefab);
			choiceObject.name = "Choice_"+choiceID;
			choiceObject.transform.SetParent(parent.transform, false);
			Choice myChoice = choiceObject.GetComponent<Choice>();
			myChoice.choiceID = choiceID;
			myChoice.GenFromJson(choiceDirectory + "Choice_" + choiceID + ".json");

			for (int i = 0; i < myChoice.playerDialogue.Length; i++)
			{
				myChoice.playerDialogue[i] = JsonToDialogue(choiceObject, choiceDirectory, i);
			}

			/*
			for (int i = 0; i < myChoice.options.Length; i++)
			{
				if (myChoice.outcomeID[i] >= 0)
				{
					myChoice.options[i] = true;
					print("i: " + i + ", NODEID: " + myChoice.outcomeID[i]);
					if (activeScene.nodes[0] != null)
					{
						myChoice.outcome[i] = activeScene.nodes[myChoice.outcomeID[i]];
					}
					else
					{
						print("shit is fucked");
					}
				}
				else
				{
					myChoice.options[i] = false;
					myChoice.outcome[i] = null;
				}
			}
			*/

			return myChoice;
		}
		else
		{
			return null;
		}
	}
		
	public Dialogue JsonToDialogue(GameObject parent, string dir, int dialogueID)
	{
		string dialogueDirectory = dir + "Dialogue_" + dialogueID + ".json";

		if (Directory.Exists(dir)&&System.IO.File.Exists(dialogueDirectory))
		{
			GameObject dialogueObject = (GameObject)Instantiate(DialoguePrefab);
			dialogueObject.name = "Dialogue_"+dialogueID;
			dialogueObject.transform.SetParent(parent.transform, false);
			Dialogue myDialogue = dialogueObject.GetComponent<Dialogue>();
			myDialogue.dialogueID = dialogueID;
			myDialogue.GenFromJson(dialogueDirectory);

			return myDialogue;
		}
		else
		{
			return null;
		}
	}


//
//
// SCENE SAVING TO JSON FUNCTIONS
//
//

    public void SceneToJson(Scene scene)
    {
		//print("Saving scene number: " + scene.sceneID);
        string sceneJson = JsonUtility.ToJson(scene);
        string sceneDirectory = Application.persistentDataPath + "/Scenes/Scene_" + scene.sceneID + "/";
        string scenePath = sceneDirectory + "Scene_" + scene.sceneID + ".json";
        //print(scenePath);

        if (!Directory.Exists(sceneDirectory))
        {
            Directory.CreateDirectory(sceneDirectory);
        }

        for (int i = 0; i < scene.nodes.Length; i++)
        {
			//print("Saving DNode number: " + i);
            DNodeToJson(sceneDirectory, scene.nodes[i]);
        }

        //FileStream output = File.Create(Application.persistentDataPath + "Node_" + root.nodeID + ".json");
        System.IO.File.WriteAllText(scenePath, sceneJson);
    }

    public void DNodeToJson(string dir, DNode dnode)
    {
        string dnodeJson = JsonUtility.ToJson(dnode);
        string dnodeDirectory = dir + "DNode_" + dnode.dnodeID + "/";
        string dnodePath = dnodeDirectory + "DNode_" + dnode.dnodeID + ".json";

        if (!Directory.Exists(dnodeDirectory))
        {
            Directory.CreateDirectory(dnodeDirectory);
        }

        for (int i = 0; i < dnode.speech.Length; i++)
        {
			//print("Saving dialogue number: " + i);
            DialogueToJson(dnodeDirectory, dnode.speech[i]);
        }
       
        for (int i = 0; i < dnode.responses.Length; i++)
        {
			//print("Saving choice number: " + i);
            ChoiceToJson(dnodeDirectory, dnode.responses[i]);
        }

        System.IO.File.WriteAllText(dnodePath, dnodeJson);
    }

    public void ChoiceToJson(string dir, Choice choice)
    {
		for (int i = 0; i < choice.outcomeID.Length; i++)
		{
			if (choice.outcomeID[i] >= 0)
			{
				//print("Saving choice outcome number: " + i);
				choice.options[i] = true;
				//choice.outcomeID[i] = choice.outcome[i].dnodeID;
			}
			else
			{
				//print("Saving choice outcome number: " + i);
				choice.options[i] = false;
				choice.outcomeID[i] = -1;
			}
			/*
			if (choice.outcome[i] != null)
			{
				print("Saving choice outcome number: " + i);
				choice.options[i] = true;
				choice.outcomeID[i] = choice.outcome[i].dnodeID;
			}
			else
			{
				//print("Saving choice outcome number: " + i);
				choice.options[i] = false;
				choice.outcomeID[i] = -1;
			} */
		}

        string choiceJson = JsonUtility.ToJson(choice);
        string choiceDirectory = dir + "Choice_" + choice.choiceID + "/";
        string choicePath = choiceDirectory + "Choice_" + choice.choiceID + ".json";

        if (!Directory.Exists(choiceDirectory))
        {
            Directory.CreateDirectory(choiceDirectory);
        }

        for (int i = 0; i < choice.playerDialogue.Length; i++)
        {
            if (choice.playerDialogue[i] != null)
            {
				//print("Saving dialogue number: " + i);
                DialogueToJson(choiceDirectory, choice.playerDialogue[i]);
            }
        }

        System.IO.File.WriteAllText(choicePath, choiceJson);
    }

    public void DialogueToJson(string dir, Dialogue dialogue)
    {
        string dialogueJson = JsonUtility.ToJson(dialogue);
        string dialogueDirectory = dir;
        string dialoguePath = dialogueDirectory + "Dialogue_" + dialogue.dialogueID + ".json";

        if (!Directory.Exists(dialogueDirectory))
        {
            Directory.CreateDirectory(dialogueDirectory);
        }

        System.IO.File.WriteAllText(dialoguePath, dialogueJson);
    }
}
