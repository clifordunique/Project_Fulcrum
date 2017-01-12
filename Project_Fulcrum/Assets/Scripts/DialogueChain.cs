using UnityEngine;
using System;
using System.Collections;
using System.Text;
using System.IO;  

public class DialogueChain : MonoBehaviour {

	public string chainName;
	public int chainID;
	[SerializeField] public Dialogue[] chainDialogue;

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
