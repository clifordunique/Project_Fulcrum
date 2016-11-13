using UnityEngine;
using System;
using System.Collections;
using System.Text;
using System.IO;  

public class DNode : MonoBehaviour {

	[SerializeField] public string nodeName;
    [SerializeField] public int dnodeID;
    //[SerializeField] public int[] dialogIDs;
    //[SerializeField] public int[] choiceIDs;
    [SerializeField] public Dialogue[] speech;
    [SerializeField] public Choice[] responses;

	public void GenFromJson(string dir)
	{
		string contents = File.ReadAllText(dir);
		JsonUtility.FromJsonOverwrite(contents, this);
	}

	// Use this for initialization
	void Start () {
//		string glurk = JsonUtility.ToJson(this);
//		print (glurk);
	}

	// Update is called once per frame
	void Update () {

	}

	public void Kill(){
		Destroy(this);
	}

}
