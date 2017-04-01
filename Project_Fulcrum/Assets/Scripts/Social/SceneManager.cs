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
    Choice
};
    
public enum MODIFIER
{
  Neutral,
  Active,
  Witty,
  Passive,
  Subversive
};


public class SceneManager : MonoBehaviour 
{
	//############################################################################################################################################################################################################
	// OBJECTS&PREFABS
	//###########################################################################################################################################################################
	#region OBJECTS&PREFABS
	[SerializeField]public ChoicePrefabScript p_ActiveChoicePrefab;
	[SerializeField]public GameObject p_ChoicePrefab;				// Prefab for choice data objects. By data I mean it only holds the information, and does not have a physical presence in the game.
	[SerializeField]public GameObject p_DialoguePrefab;				// Prefab for dialogue data objects.
	[SerializeField]public GameObject p_ScenePrefab;				// Prefab for scene data objects.
	[SerializeField]public GameObject p_DNodePrefab;				// Prefab for node data objects. These are the location nodes on the branching diaogue tree.
	[SerializeField]public GameObject p_DialogueChainPrefab;		// Prefab for dialogue-sequence data object. Used for when multiple dialogues are spoken in succession.
	[SerializeField]private GameObject p_MenuChoicePrefab;			// Prefab for choice UI objects.
	[SerializeField]private Scene[] o_SceneList;					// List of scenes used in the level.		
    [SerializeField]private RectTransform o_ScrollMenu;				// UI Object.
	[SerializeField]private InputField o_LeftDialogue;			  	// UI Object.
	[SerializeField]private InputField o_RightDialogue;				// UI Object.
	[SerializeField]private Animator o_SocialMenu;					// Animator for conversation menu.
	[SerializeField]private ActorAnimator o_ActorAnimator;			// Animator for the convo actors.
	#endregion
	//############################################################################################################################################################################################################
	// OTHER VARIABLES
	//###########################################################################################################################################################################
	#region OTHER VARIABLES
    public Choice selectedChoice;
    public int modifier = -1;
    private int currentDialogue = 0;
	private DNode activeNode;
	private DialogueChain activeDialogueChain;
	public Scene activeScene;
	private bool nodeChanging = false;
	public bool manualSave;
	#endregion
	//############################################################################################################################################################################################################
	// SCENE HANDLING
	//###########################################################################################################################################################################
	#region SCENE HANDLING
    public void ShowChoices()
	{
        o_RightDialogue.text = "";
        o_LeftDialogue.text = "";
        o_SocialMenu.SetInteger("state", (int)STATE.Choice);
		//ActorAnimator.SetInteger("state", (int)STATE.Choice);
    }

    public void OkayButton()
	{
        if (o_SocialMenu.GetInteger("state") == (int)STATE.Dialogue) //Advances dialogue if dialogue screen is already open
        {
            //print("currentDialogue = " + currentDialogue + ", dialoguelength = " + activeNode.speech.Length);
			if (currentDialogue + 2 <= activeDialogueChain.chainDialogue.Length)
            {
                currentDialogue++;
				SetDialogue(activeDialogueChain.chainDialogue[currentDialogue]);
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
				SetDialogueChain(selectedChoice.playerDialogueChain[modifier]);
				//ActorAnimator.SetInteger("state", (int)STATE.Dialogue);
				o_SocialMenu.SetInteger("state", (int)STATE.Dialogue);
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
		//ActorAnimator.SetInteger("state", (int)STATE.Closed);
		o_SocialMenu.SetInteger("state", (int)STATE.Closed); 
	}

	public void SetDialogue(Dialogue dialogue)
	{
		o_ActorAnimator.AnimateDialogue(dialogue);
		if (dialogue.exit) 
		{
			ExitScene();
			return;
		}
        if (dialogue.leftSide)
        {
            o_LeftDialogue.text = dialogue.text;
            o_LeftDialogue.GetComponent<TextFieldModder>().activeDialogue = dialogue;
            o_RightDialogue.text = "";
			o_RightDialogue.GetComponent<TextFieldModder>().activeDialogue = null;
        }
        else
        {
            o_RightDialogue.text = dialogue.text;
            o_RightDialogue.GetComponent<TextFieldModder>().activeDialogue = dialogue;
            o_LeftDialogue.text = "";
			o_LeftDialogue.GetComponent<TextFieldModder>().activeDialogue = null;
        }
	}

	public void SetDialogueChain(DialogueChain chain)
	{
		activeDialogueChain = chain;
		SetDialogue(chain.chainDialogue[0]);
		currentDialogue = 0;
	}

    public void SelectNeutral(Choice clickedChoice)
    {
		if (clickedChoice.playerDialogueChain[(int)MODIFIER.Neutral] != null)
        {
            selectedChoice = clickedChoice;
			SetDialogueChain(clickedChoice.playerDialogueChain[(int)MODIFIER.Neutral]);
            modifier = (int)MODIFIER.Neutral;
        }
        else
        {
        	print("No neutral response available.");
        }
    }

	public void SelectPassive(Choice clickedChoice)
	{
		if (clickedChoice.playerDialogueChain[(int)MODIFIER.Passive] != null)
		{
			selectedChoice = clickedChoice;
			SetDialogueChain(clickedChoice.playerDialogueChain[(int)MODIFIER.Passive]);
			modifier = (int)MODIFIER.Passive;
		}
		else
		{
			print("No passive response available.");
		}
	}

	public void SelectActive(Choice clickedChoice)
	{
		if (clickedChoice.playerDialogueChain[(int)MODIFIER.Active] != null)
		{
			selectedChoice = clickedChoice;
			SetDialogueChain(clickedChoice.playerDialogueChain[(int)MODIFIER.Active]);
			modifier = (int)MODIFIER.Active;
		}
		else
		{
			print("No active response available.");
		}
	}

	public void SelectWitty(Choice clickedChoice)
	{
		if (clickedChoice.playerDialogueChain[(int)MODIFIER.Witty] != null)
		{
			selectedChoice = clickedChoice;
			SetDialogueChain(clickedChoice.playerDialogueChain[(int)MODIFIER.Witty]);
			modifier = (int)MODIFIER.Witty;
		}
		else
		{
			print("No witty response available.");
		}
	}

	public void SelectSubversive(Choice clickedChoice)
	{
		if (clickedChoice.playerDialogueChain[(int)MODIFIER.Subversive] != null)
		{
			selectedChoice = clickedChoice;
			SetDialogueChain(clickedChoice.playerDialogueChain[(int)MODIFIER.Subversive]);
			modifier = (int)MODIFIER.Subversive;
		}
		else
		{
			print("No subversive response available.");
		}
	}

	public void ChangeNode(DNode newNode)
	{
		//print("Switching to node: "+ newNode);
		if (activeNode != null) 
		{
			for (int i = 0; i < activeNode.responses.Length; i++) 
			{
				Destroy(GameObject.Find("Choice_" + i));
			}
		}
		activeNode = newNode;
        //print("Now switched to node: "+ activeNode.nodeName);
		for(int i = 0; i < activeNode.responses.Length; i++)
		{
            //print("Choice number " + i + " created");
			GameObject newChoice = (GameObject)Instantiate(p_MenuChoicePrefab);
			newChoice.transform.SetParent(o_ScrollMenu, false);
            newChoice.GetComponent<Text>().text = activeNode.responses[i].choiceName;
			newChoice.name = "Choice_"+i;
            //print(newChoice.GetComponent<ChoicePrefabScript>());
            newChoice.GetComponent<ChoicePrefabScript>().SetChoice(activeNode.responses[i]);
            newChoice.GetComponent<ChoicePrefabScript>().mySceneManager = this;
//			activeChoices[i] = newChoice;
		}
		//SocialMenu.CrossFade ("Dialogue", 0.5f);
        o_SocialMenu.SetInteger("state", (int)STATE.Dialogue);
		//ActorAnimator.SetInteger("state", (int)STATE.Dialogue);
        SetDialogueChain(activeNode.speechChain[0]);
	}
	#endregion
	//############################################################################################################################################################################################################
	// LOADING SCENE FROM JSON
	//###########################################################################################################################################################################
	#region JSON->SCENE
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
			print("Loading Scene from JSON at:"+Application.persistentDataPath + "/Scenes/Scene_" + sceneID + "/");
			string sceneDirectory = Application.persistentDataPath + "/Scenes/Scene_" + sceneID + "/";
			if (Directory.Exists(sceneDirectory))
			{
				GameObject sceneObject = (GameObject)Instantiate(p_ScenePrefab);
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
			GameObject dnodeObject = (GameObject)Instantiate(p_DNodePrefab);
			dnodeObject.name = "DNode_"+dnodeID;
			dnodeObject.transform.SetParent(parent.transform, false);
			DNode myDNode = dnodeObject.GetComponent<DNode>();
			myDNode.dnodeID = dnodeID;
			myDNode.GenFromJson(dnodeDirectory + "DNode_" + dnodeID + ".json");

			for (int i = 0; i < myDNode.responses.Length; i++)
			{
				myDNode.responses[i] = JsonToChoice(dnodeObject, dnodeDirectory, i);
			}

			for (int i = 0; i < myDNode.speechChain.Length; i++)
			{
				myDNode.speechChain[i] = JsonToDialogueChain(dnodeObject, dnodeDirectory, i);
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
			GameObject choiceObject = (GameObject)Instantiate(p_ChoicePrefab);
			choiceObject.name = "Choice_"+choiceID;
			choiceObject.transform.SetParent(parent.transform, false);
			Choice myChoice = choiceObject.GetComponent<Choice>();
			myChoice.choiceID = choiceID;
			myChoice.GenFromJson(choiceDirectory + "Choice_" + choiceID + ".json");

			for (int i = 0; i < myChoice.playerDialogueChain.Length; i++)
			{
				myChoice.playerDialogueChain[i] = JsonToDialogueChain(choiceObject, choiceDirectory, i);
			}

			return myChoice;
		}
		else
		{
			return null;
		}
	}

	public DialogueChain JsonToDialogueChain(GameObject parent, string dir, int chainID)
	{
		string chainDirectory = dir + "Chain_" + chainID + "/";

		if (Directory.Exists(chainDirectory))
		{
			GameObject chainObject = (GameObject)Instantiate(p_DialogueChainPrefab);
			chainObject.name = "Chain_"+chainID;
			chainObject.transform.SetParent(parent.transform, false);
			DialogueChain myChain = chainObject.GetComponent<DialogueChain>();
			myChain.chainID = chainID;
			myChain.GenFromJson(chainDirectory + "Chain_" + chainID + ".json");

			for (int i = 0; i < myChain.chainDialogue.Length; i++)
			{
				myChain.chainDialogue[i] = JsonToDialogue(chainObject, chainDirectory, i);
			}

			return myChain;
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
			GameObject dialogueObject = (GameObject)Instantiate(p_DialoguePrefab);
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
	#endregion 
	//############################################################################################################################################################################################################
	// SAVING SCENE TO JSON
	//###########################################################################################################################################################################
	#region SCENE->JSON
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

        for (int i = 0; i < dnode.speechChain.Length; i++)
        {
			//print("Saving dialogue number: " + i);
            DialogueChainToJson(dnodeDirectory, dnode.speechChain[i]);
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
		}

        string choiceJson = JsonUtility.ToJson(choice);
        string choiceDirectory = dir + "Choice_" + choice.choiceID + "/";
        string choicePath = choiceDirectory + "Choice_" + choice.choiceID + ".json";

        if (!Directory.Exists(choiceDirectory))
        {
            Directory.CreateDirectory(choiceDirectory);
        }

        for (int i = 0; i < choice.playerDialogueChain.Length; i++)
        {
            if (choice.playerDialogueChain[i] != null)
            {
				//print("Saving dialogue number: " + i);
                DialogueChainToJson(choiceDirectory, choice.playerDialogueChain[i]);
            }
        }

        System.IO.File.WriteAllText(choicePath, choiceJson);
    }

	public void DialogueChainToJson(string dir, DialogueChain chain)
	{
		string chainJson = JsonUtility.ToJson(chain);
		string chainDirectory = dir+ "Chain_" + chain.chainID + "/";
		string chainPath = chainDirectory + "Chain_" + chain.chainID + ".json";

		if (!Directory.Exists(chainDirectory))
		{
			Directory.CreateDirectory(chainDirectory);
		}

		for (int i = 0; i < chain.chainDialogue.Length; i++)
		{
			if (chain.chainDialogue[i] != null)
			{
				//print("Saving dialogue number: " + i);
				DialogueToJson(chainDirectory, chain.chainDialogue[i]);
			}
		}
		System.IO.File.WriteAllText(chainPath, chainJson);
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
	#endregion
}
