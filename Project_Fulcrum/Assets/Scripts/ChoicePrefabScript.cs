using UnityEngine;
using System.Collections;

public class ChoicePrefabScript : MonoBehaviour {

    public Choice choice;
    public SceneManager mySceneManager;

    public void SetChoice(Choice theChoice) {
        choice = theChoice;
    }

    public void NeutralClick(){
        mySceneManager.SelectNeutral(choice);
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
