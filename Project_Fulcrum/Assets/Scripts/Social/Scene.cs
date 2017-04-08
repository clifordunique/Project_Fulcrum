using UnityEngine;
using System;
using System.Collections;
using System.Text;
using System.IO;  

public class Scene : MonoBehaviour {

    public string sceneDesc; //Description for developer convenience only.
    public int sceneID;
    public DNode root;
	public int rootID;
    public DNode[] nodes;
	GameObject newScene;

	public void Create(int id)
	{
		sceneID = id;
	}


	public void GenFromJson(string dir)
	{
		string contents = File.ReadAllText(dir);
		JsonUtility.FromJsonOverwrite(contents, this);
	}

	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
