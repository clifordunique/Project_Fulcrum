using UnityEngine;
using System.Collections;

public class DNode : MonoBehaviour {

	[SerializeField] public string nodeName;
    [SerializeField] public int nodeID;
	[SerializeField] public Dialogue[] speech;
	[SerializeField] public Choice[] Responses;
    GameObject newDNode;


	// Use this for initialization
	void Start () {
//		string glurk = JsonUtility.ToJson(this);
//		print (glurk);
	}

	// Update is called once per frame
	void Update () {

	}

	public GameObject Create(int id,string name, Dialogue[] npcspeech){
		speech = npcspeech;
        nodeID = id;
		newDNode = new GameObject("Node_"+id);
		nodeName = name;
		return newDNode;
	}

	public void Kill() {
		Destroy(newDNode);
		Destroy(this);
	}

}
