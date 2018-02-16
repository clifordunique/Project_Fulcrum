using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


[ExecuteInEditMode]
public class MinimapHolo : MonoBehaviour {

	//[SerializeField] private Camera[] cameraList;
	[SerializeField] private Material testMat;
	[SerializeField] private RenderTexture destinationTex;



	// Use this for initialization
	void Start () 
	{
		int scrW = Screen.currentResolution.width;
		int scrH = Screen.currentResolution.height;
		destinationTex = new RenderTexture((int)(scrW*0.2), (int)(scrH*0.2), 0);
	}

	void OnRenderImage(RenderTexture src,RenderTexture dst)
	{
		//Graphics.
		//dst.DiscardContents();
		Graphics.Blit(src, dst, testMat);
	}

	// Update is called once per frame
	void Update () 
	{

	}
}
