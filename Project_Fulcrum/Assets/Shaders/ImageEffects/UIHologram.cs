using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


[ExecuteInEditMode]
public class UIHologram : MonoBehaviour {

	//[SerializeField] private Camera[] cameraList;
	[SerializeField] private Material UIHologramMaterial;

	[SerializeField][ReadOnlyAttribute] private RenderTexture outputRT;
	[SerializeField][ReadOnlyAttribute] private Camera myInputCam; // Camera that gives UIHologram its rendertexture input.



	// Use this for initialization
	void Start () 
	{
		int scrW = Screen.width;
		int scrH = Screen.height;
		outputRT = new RenderTexture((int)(scrW), (int)(scrH), 0);
//		outputRT.anisoLevel = 0;
//		outputRT.filterMode = FilterMode.Point;
//		outputRT.antiAliasing = 1;
		outputRT.Create();

		myInputCam = this.gameObject.GetComponent<Camera>();
		myInputCam.targetTexture = outputRT;

	}

	void OnPreRender()
	{
		myInputCam.targetTexture = outputRT;
	}

	void OnPostRender()
	{
		myInputCam.targetTexture = null;
		Graphics.Blit(outputRT, null, UIHologramMaterial);
	}
}
