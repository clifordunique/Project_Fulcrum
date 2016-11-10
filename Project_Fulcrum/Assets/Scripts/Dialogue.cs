using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;  

public class Dialogue : MonoBehaviour {

	public string text;
    public int dialogue_id;
    public string rawtext;
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

    public void GenFromJson(string filePath)
    {
        string jsonString = File.ReadAllText(filePath);
        JsonUtility.FromJsonOverwrite(jsonString, this);
    }

	public GameObject Create(int id, string t, int e, int a){
		newDialog = new GameObject("Dialogue_"+id);
        dialogue_id = id;
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
