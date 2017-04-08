using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TextFieldModder : MonoBehaviour {
    
    public Dialogue activeDialogue;
    public string currentText = "";
    public GameObject parent;
  
    public void OverwriteField(){
        if (activeDialogue != null)
        {
            print("Dialogue option has been altered!");
            currentText = parent.GetComponent<InputField>().text;
            activeDialogue.text = currentText;
        }
    }

	// Use this for initialization
	void Start () {
        parent = this.gameObject;
        currentText = parent.GetComponent<InputField>().text;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
