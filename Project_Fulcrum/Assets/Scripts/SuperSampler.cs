using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


[ExecuteInEditMode]
public class SuperSampler : MonoBehaviour {

	[SerializeField] private Camera[] cameraList;
	//[SerializeField] private Material testMat;


	// Use this for initialization
	void Start () 
	{

	}

	void OnRenderImage(RenderTexture src,RenderTexture dst)
	{

	}

	// Update is called once per frame
	void Update () 
	{

	}
}
