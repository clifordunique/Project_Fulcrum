using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudHandler : MonoBehaviour {
	[SerializeField]private GameObject p_CloudLayer;		// Cloudlayer Prefab.
	[SerializeField][ReadOnlyAttribute]private GameObject[] o_CloudLayer; 	// Array of all cloud layers.
	[Header("Cloud Customization")]
	public float cloudLayerElevation;	// Height above ground level in game units.
	public float zDistanceKM;			// How deep into the distance in km the clouds will stretch.
	public int layerDensity;			// How many cloud cutouts will span between the foreground and the max zDistanceKM. More layers = more realistic.
	public Color startColor;			// Foreground cloud colour.
	public Color endColor;				// Most distant cloud colour.
	public float startScaleX = 2;		// Size of foreground cloud.
	public float startScaleY = 2;		// Size of foreground cloud.
	public Transform parallaxHolder; 	// Transform that acts as parent to all parallaxLayers
	public float cloudXRandomness = 200; 		// Horizontal "shuffling" of layers.

	[Header("Important Variables")]
	[Range(1,4)][SerializeField] private float distanceSizeRatio = 0.5f;
	[Range(0,1)][SerializeField] private float distanceParallaxRatio = 0.5f;
	[SerializeField] private float distributionCurveM = 0;

	private float current_cloudLayerElevation;
	private float current_zDistanceKM;				
	private Color current_startColor;			
	private Color current_endColor;			
	private float current_startScaleX = 2;		
	private float current_startScaleY = 2;		
	private float current_distanceSizeRatio = 0.5f;
	private float current_distanceParallaxRatio = 0.5f;

	// Use this for initialization
	void Start() 
	{
		parallaxHolder = GameObject.Find("Parallax").transform;
		Vector2 cloudOrigin = new Vector2(0, cloudLayerElevation);
		GameObject realCloudLayer = (GameObject)Instantiate(p_CloudLayer, cloudOrigin, Quaternion.identity);
		realCloudLayer.transform.localScale = new Vector3(startScaleX*0.5f,startScaleY*0.5f,1);
		realCloudLayer.layer = 15;
		ParallaxLayer prlx = realCloudLayer.GetComponent<ParallaxLayer>();
		prlx.speedX = 0;
		prlx.speedY = 0;
		SpriteRenderer[] cloudRenderer = realCloudLayer.GetComponentsInChildren<SpriteRenderer>();
		foreach(SpriteRenderer sp in cloudRenderer)
		{
			sp.color = startColor;
			sp.sortingLayerName = "Foreground";
			sp.sortingOrder = 1000;
		}
		realCloudLayer.name = "RealCloudLayer";

		//########################################
		distributionCurveM = ((zDistanceKM)/((float)(layerDensity*layerDensity)));
		o_CloudLayer = new GameObject[layerDensity];
		for(int i = 1; i <= layerDensity; i++)
		{
			o_CloudLayer[i-1] = SetupCloudLayer(i);
		}
		//##########################################


	}

	void RefreshCurrent()
	{
		current_cloudLayerElevation = cloudLayerElevation;
		current_zDistanceKM = zDistanceKM;				
		current_startColor = startColor;			
		current_endColor = 	endColor;			
		current_startScaleX = startScaleX;		
		current_startScaleY = startScaleY;		
		current_distanceSizeRatio = distanceSizeRatio;
		current_distanceParallaxRatio = distanceParallaxRatio;
	}

	void UpdateAllClouds()
	{
		//print("UPDATING CLOUD VARS");
		for(int i = 1; i <= o_CloudLayer.Length; i++)
		{
			AdjustCloudLayer(i);
		}
	}

//	void AdjustCloudLayer(int layerDepth)
//	{
//		if(!o_CloudLayer[layerDepth-1])
//		{
//			print("ERROR: Cloudlayer is null.");
//			return;
//		}
//		//Vector2 cloudOrigin = new Vector2(0, cloudLayerElevation);
//
//		o_CloudLayer[layerDepth-1].layer = 11;
//		ParallaxLayer prlx = o_CloudLayer[layerDepth-1].GetComponent<ParallaxLayer>();
//		//prlx.distanceKM = ((float)layerDepth/(float)layerDensity)*zDistanceKM;
//		prlx.distanceKM = (float)layerDepth*(float)layerDepth*distributionCurveM;
//		SpriteRenderer[] cloudRenderer = o_CloudLayer[layerDepth-1].GetComponentsInChildren<SpriteRenderer>();
//		Color dynamiColor = Color.Lerp(startColor,endColor,(prlx.distanceKM/zDistanceKM));
//		foreach(SpriteRenderer sp in cloudRenderer)
//		{
//			sp.color = dynamiColor;
//		}
//	}

//
//	GameObject SetupCloudLayer(int layerDepth)
//	{
//		float randomXOffset = Random.Range(-cloudXRandomness,cloudXRandomness);
//		Vector2 cloudOrigin = new Vector2(0, cloudLayerElevation);
//		GameObject newCloudLayer = (GameObject)Instantiate(p_CloudLayer, cloudOrigin, Quaternion.identity);
//
//		newCloudLayer.layer = 11;
//		ParallaxLayer prlx = newCloudLayer.GetComponent<ParallaxLayer>();
//		prlx.distanceKM = (float)layerDepth*(float)layerDepth*distributionCurveM;
//		SpriteRenderer[] cloudRenderer = newCloudLayer.GetComponentsInChildren<SpriteRenderer>();
//		int sortOrder= -100-layerDepth*5;
//
//		Color dynamiColor = Color.Lerp(startColor,endColor,(layerDepth/(float)layerDensity));
//		foreach(SpriteRenderer sp in cloudRenderer)
//		{
//			sp.color = dynamiColor;
//			sp.sortingLayerName = "Background";
//			sp.sortingOrder = sortOrder;
//		}
//
//		foreach(Transform child in newCloudLayer.transform)
//		{
//			child.localPosition = new Vector3(child.localPosition.x+randomXOffset, child.localPosition.y, child.localPosition.z);
//		}
//
//		newCloudLayer.name = "CloudLayer_"+layerDepth;
//
//		return newCloudLayer;
//	}
//

	GameObject SetupCloudLayer(int layerDepth)
	{
		Vector2 cloudOrigin = new Vector2(0, cloudLayerElevation);
		GameObject newCloudLayer = (GameObject)Instantiate(p_CloudLayer, cloudOrigin, Quaternion.identity);
		Color dynamiColor = Color.Lerp(startColor,endColor,(layerDepth/(float)layerDensity));
		float distanceKilometers = (float)layerDepth*(float)layerDepth*distributionCurveM;
		newCloudLayer.GetComponent<CloudLayer>().Initialize(layerDepth, distanceKilometers, dynamiColor);

		newCloudLayer.name = "CloudLayer_"+layerDepth;
		return newCloudLayer;
	}

	void AdjustCloudLayer(int layerDepth)
	{
		if(!o_CloudLayer[layerDepth-1])
		{
			print("ERROR: Cloudlayer is null.");
			return;
		}
		Color dynamiColor = Color.Lerp(startColor,endColor,(o_CloudLayer[layerDepth-1].GetComponent<CloudLayer>().distanceKM/zDistanceKM));

		float distanceKilometers = (float)layerDepth*(float)layerDepth*distributionCurveM;
		o_CloudLayer[layerDepth-1].GetComponent<CloudLayer>().Adjust(layerDepth, distanceKilometers, dynamiColor);
	}

	// Update is called once per frame
	void Update () 
	{
		int changes = 0;
		if(current_cloudLayerElevation != cloudLayerElevation){changes++;}
		if(current_zDistanceKM != zDistanceKM){changes++;}				
		if(current_startColor != startColor){changes++;}		
		if(current_endColor != endColor){changes++;}		
		if(current_startScaleX != startScaleX){changes++;}		
		if(current_startScaleY != startScaleY){changes++;}		
		if(current_distanceSizeRatio != distanceSizeRatio){changes++;}
		if(current_distanceParallaxRatio != distanceParallaxRatio){changes++;}

		if(changes > 0)
		{
			RefreshCurrent();
			UpdateAllClouds();
		}
	}
}
