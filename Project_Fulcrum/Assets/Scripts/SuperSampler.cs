using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


[ExecuteInEditMode]
public class SuperSampler : MonoBehaviour {

	[SerializeField] private Material testMat;
	[SerializeField] private Camera[] cameraList;
	[SerializeField] private RenderTexture ssTex;
	[SerializeField] private bool enableSuperSampling;


	// Use this for initialization
	void Start () 
	{
		int scrW = Screen.width;
		int scrH = Screen.height;

		if(!enableSuperSampling)
		{
			foreach(Camera c in cameraList)
			{
				c.targetTexture = null;
			}
			return;
		}
		ssTex = new RenderTexture(scrW*2, scrH*2, 0);
		ssTex.anisoLevel = 0;
		ssTex.filterMode = FilterMode.Bilinear;
		ssTex.antiAliasing = 2;
		ssTex.Create();

		foreach(Camera c in cameraList)
		{
			c.targetTexture = ssTex;
		}
	}

	void OnRenderImage(RenderTexture src,RenderTexture dst)
	{
		if(!enableSuperSampling)
		{
			return;
		}
//		for(int i = 0; i<cameraList.Length; i++)
//		{
//			Camera cam = cameraList[i];
//			cam.targetTexture = ssTex;
//			cam.Render();
//			//cam.targetTexture = null;
//		}
		Graphics.Blit(ssTex, dst);
	}

	// Update is called once per frame
	void Update () 
	{

	}
}
