using UnityEngine;
using System.Collections;

public class Dialogue : MonoBehaviour {

	public string text;
	public int emote;
	public int actor;
	public bool exit;
	[SerializeField]public bool leftSide;
	GameObject newDialog;


	// Use this for initialization
	void Start () {
	
	}
		
	// Update is called once per frame
	void Update () {
	
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

	public GameObject Create(int id, string t, int e, int a){
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
