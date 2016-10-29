using UnityEngine;
using System.Collections;

public class MenuChoicePrefab : MonoBehaviour {

    public Choice choice;

    public void SetChoice(Choice theChoice) {
        choice = theChoice;
    }

    public void NeutralClick(){
        print("neutral click");
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
