using UnityEngine;

public class CameraController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<Camera>().orthographicSize = Screen.height/2;

    }
	
	// Update is called once per frame
	void Update () {

    }
}
