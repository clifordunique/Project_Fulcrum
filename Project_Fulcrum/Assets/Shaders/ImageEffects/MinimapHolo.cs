using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


[ExecuteInEditMode]
public class MinimapHolo : MonoBehaviour {

	//[SerializeField] private Camera[] cameraList;
	[SerializeField] private Material holoMat;
	//[SerializeField] private Material holoHighlightMat;
//	[SerializeField] private Texture2D test;

	[SerializeField] private RenderTexture outputRT;
	[SerializeField] private RenderTexture baseTex;
//	[SerializeField] private RenderTexture test1;
//	[SerializeField] private RenderTexture test2;
	[SerializeField][ReadOnlyAttribute] private GameObject minimapUI; // UI element which displays the final result.
	[SerializeField][ReadOnlyAttribute] private Camera myCam; 
	[SerializeField][ReadOnlyAttribute] private Camera myHighlightCam; 

	[SerializeField] private int pass; // 0 for red, 1 for green overlay.



	// Use this for initialization
	void Start () 
	{
		int scrW = Screen.width;
		int scrH = Screen.height;
		outputRT = new RenderTexture((int)(scrW*0.2), (int)(scrH*0.2), 0);
		outputRT.anisoLevel = 0;
		outputRT.filterMode = FilterMode.Point;
		outputRT.antiAliasing = 1;
		outputRT.Create();

		baseTex = new RenderTexture((int)(scrW*0.2), (int)(scrH*0.2), 1);
		baseTex.anisoLevel = 0;
		baseTex.filterMode = FilterMode.Point;
		baseTex.antiAliasing = 1;
		baseTex.Create();

		holoMat.SetTexture("_BaseLayer", baseTex);

		myCam = this.gameObject.GetComponent<Camera>();
		myCam.targetTexture = outputRT;

		pass = 0;
		myCam.cullingMask = 1 << 15;
		myCam.targetTexture = baseTex;

		minimapUI = GameObject.Find("Canvas/MinimapUI");
		minimapUI.GetComponent<RawImage>().texture = outputRT;
	}

	void OnRenderImage(RenderTexture src,RenderTexture dst)
	{
		Graphics.Blit(src, dst, holoMat, pass);
	}
		
	void Update () 
	{
		pass = 0;
		myCam.cullingMask = 1 << 15;
		myCam.targetTexture = baseTex;
		myCam.Render();

		pass = 1;
		myCam.cullingMask = 1 << 16;
		myCam.targetTexture = outputRT;
		myCam.Render();
	}
}
