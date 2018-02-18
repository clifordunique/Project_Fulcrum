using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


[ExecuteInEditMode]
public class MinimapHolo : MonoBehaviour {

	//[SerializeField] private Camera[] cameraList;
	[SerializeField] private Material testMat;
	[SerializeField] private RenderTexture destinationTex;
	[SerializeField][ReadOnlyAttribute] private GameObject minimapUI; // UI element which displays the final result.
	[SerializeField][ReadOnlyAttribute] private Camera myCam; 



	// Use this for initialization
	void Start () 
	{
		int scrW = Screen.width;
		int scrH = Screen.height;
		destinationTex = new RenderTexture((int)(scrW*0.2), (int)(scrH*0.2), 0);
		destinationTex.anisoLevel = 0;
		destinationTex.filterMode = FilterMode.Point;
		destinationTex.antiAliasing = 1;
		destinationTex.Create();

		myCam = this.gameObject.GetComponent<Camera>();
		minimapUI = GameObject.Find("Canvas/MinimapUI");

		myCam.targetTexture = destinationTex;
		minimapUI.GetComponent<RawImage>().texture = destinationTex;
	}

	void OnRenderImage(RenderTexture src,RenderTexture dst)
	{
		Graphics.Blit(src, dst, testMat);
	}

	// Update is called once per frame
	void Update () 
	{

	}
}
