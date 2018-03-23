using UnityEngine;
using System;
using System.Collections;
using System.Text;
using System.IO;  

public class Dialogue : MonoBehaviour {

	public string text;
    public int dialogueID;
    public string rawtext;
	//public int emote;

	public ActorAction[] actorAction;
	//public bool exit;
	[SerializeField]public bool leftSide;
	//GameObject newDialog;


	// Use this for initialization
	void Start () {
	
	}
		
	// Update is called once per frame
	void Update () {
	
	}

    public void GenFromJson(string filePath)
    {
        string jsonString = File.ReadAllText(filePath);
        JsonUtility.FromJsonOverwrite(jsonString, this);
    }
	/*
	public GameObject Create(int id, string t, int e, int a){
		newDialog = new GameObject("Dialogue_"+id);
        dialogueID = id;
		text = t;
		emote = e;
		actor = a;
		return newDialog;
	}
	*/
	public void Kill() {
		Destroy(this);
	}

}
	
[System.Serializable]
public struct ActorAction
{
	/// <summary>
	/// The designated actor for this action. A value of -1 designates the special case of setting the scene event, such as panning in close, showing action lines or ending the scene.
	/// [-1] - Scene
	/// [0] - Placeholder Actor
	/// [1] - Athena
	/// [2] - Vella
	/// </summary>
	public int actorID;
	/// <summary>
	/// The int value is what action the designated actor should take. 
	/// 0 and up are all actor specific emotes.
	/// If actor selected is the scene itself, it has special options which are as follows:
	/// [-1] - Exit
	/// </summary>
	public int actionID;
	/// <summary>
	/// The int value determines what position the actor takes in the scene. 
	/// [0] - Exit
	/// [1] - Left
	/// [2] - Right
	/// </summary>
	public int positionID;
}
