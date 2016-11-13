using UnityEngine;
using System;
using System.Collections;
using System.Text;
using System.IO;  

public class Choice : MonoBehaviour {

	public string choiceName;
	public int choiceID;
	[SerializeField] public Dialogue[] playerDialogue;
	[SerializeField] public int token;
	public bool[] options;
	[SerializeField] public DNode[] outcome;
    public int[] outcomeID;


	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {

	}
		
	public void GenFromJson(string dir)
	{
		string contents = File.ReadAllText(dir);
		JsonUtility.FromJsonOverwrite(contents, this);
	}

	public void Kill() {
		Destroy(this);
	}

}
