using UnityEngine;
using System.Collections;

public class Choice : MonoBehaviour {

	[SerializeField] public string choiceName;
	[SerializeField] public Dialogue[] playerDialogue;
	[SerializeField] public int token;
	[SerializeField] public DNode[] outcome;


	GameObject newChoice;


	// Use this for initialization
	void Start () {
//		gameObject.GetComponent<Text>.text = playerDialogue[0].text;
	}

	// Update is called once per frame
	void Update () {

	}

	public GameObject Create(int id, int t, DNode[] o){
		newChoice = new GameObject("Choice_"+id);
		token = t;
		outcome = o;
		return newChoice;
	}

	public void Kill() {
		Destroy(newChoice);
		Destroy(this);
	}

}
