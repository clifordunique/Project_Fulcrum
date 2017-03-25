using UnityEngine;
using System;
using System.Collections;
using System.Text;
using System.IO;  

[Serializable]
public class JSONDialogue : MonoBehaviour {

	public string[] text;
	public int emote;
	public string pid;
	public int actor;
	public bool exit;
	[SerializeField]public bool leftSide;
	GameObject newDialog;


	// Use this for initialization
	void Start () {
		//string glurk = JsonUtility.ToJson(this);
		//print (glurk);
		string contents = File.ReadAllText("Assets/Text/test3.json");
		JsonGen(contents);
	}

	// Update is called once per frame
	void Update () {

	}



	public void JsonGen(string jstring){
		JsonUtility.FromJsonOverwrite(jstring, this);
	}

	/*
	public void GenFromString(string str)
	{
		string[] components = str.Split("-");
		foreach (string s in components)
		{
			print (s);
		}
	}
	*/

	public GameObject Create(int id, string[] t, int e, int a){
		newDialog = new GameObject("Dialogue_"+id);
		text = t;
		emote = e;
		actor = a;
		return newDialog;
	}

	public void Kill() {
		Destroy(newDialog);
		Destroy(this);
	}

}
