using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
	public int layerCount;

	[Header("Important Variables")]
	[Range(1,4)][SerializeField] private float distanceSizeRatio = 1.25f;
	[Range(0,1)][SerializeField] private float distanceParallaxRatio = 1f;
	[SerializeField] private float distributionCurveM = 0;

	private float current_cloudLayerElevation;
	private float current_zDistanceKM;				
	private Color current_startColor;			
	private Color current_endColor;			
	private float current_startScaleX = 2;		
	private float current_startScaleY = 2;		
	private float current_distanceSizeRatio = 0.5f;
	private float current_distanceParallaxRatio = 0.5f;

	public void GenerateClouds()
	{
		parallaxHolder = GameObject.Find("CloudParallax").transform;
		Vector2 cloudOrigin = new Vector2(0, cloudLayerElevation);
		GameObject realCloudLayer = (GameObject)Instantiate(p_CloudLayer, cloudOrigin, Quaternion.identity);
		realCloudLayer.transform.localScale = new Vector3(startScaleX*0.5f,startScaleY*0.5f,1);
		realCloudLayer.layer = 15;
		ParallaxLayer prlx = realCloudLayer.GetComponent<ParallaxLayer>();
		prlx.enabled = false;
		//prlx.speedX = 0;
		//prlx.speedY = 0;
		SpriteRenderer[] cloudRenderer = realCloudLayer.GetComponentsInChildren<SpriteRenderer>();
		foreach(SpriteRenderer sp in cloudRenderer)
		{
			sp.color = startColor;
			sp.sortingLayerName = "Foreground";
			sp.sortingOrder = 1000;
		}
		realCloudLayer.name = "RealCloudLayer";
		realCloudLayer.transform.parent = parallaxHolder;

		//########################################
		distributionCurveM = ((zDistanceKM)/((float)(layerDensity*layerDensity)));
		o_CloudLayer = new GameObject[layerDensity];
		for(int i = 1; i <= layerDensity; i++)
		{
			o_CloudLayer[i-1] = SetupCloudLayer(i, layerDensity);
			o_CloudLayer[i-1].gameObject.transform.parent = parallaxHolder;
		}
		//##########################################
	}

	public void RefreshCurrent()
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

	public void UpdateAllClouds()
	{
		//print("UPDATING CLOUD VARS");
		for(int i = 1; i <= o_CloudLayer.Length; i++)
		{
			AdjustCloudLayer(i);
		}
	}

	GameObject SetupCloudLayer(int layerDepth, int totalLayers)
	{
		layerCount = totalLayers;
		float multiplier = ((float)layerDepth)/totalLayers;
		float distanceKilometers = (100f*multiplier*multiplier*multiplier);
		Vector2 cloudOrigin = new Vector2(0, cloudLayerElevation);
		GameObject newCloudLayer = (GameObject)Instantiate(p_CloudLayer, cloudOrigin, Quaternion.identity);
		Color dynamiColor = Color.Lerp(startColor,endColor,(distanceKilometers/100));
		//float distanceKilometers = (float)layerDepth*(float)layerDepth*distributionCurveM;

		newCloudLayer.GetComponent<CloudLayer>().Initialize(layerDepth, distanceKilometers, dynamiColor, cloudOrigin);

		SortingGroup mySortingGroup;
		mySortingGroup = newCloudLayer.AddComponent<SortingGroup>();
		mySortingGroup.sortingOrder = -100-((layerDepth)*2);

		mySortingGroup.sortingLayerName = "Background";
		newCloudLayer.name = "CloudLayer_"+layerDepth;
		return newCloudLayer;

		//ParallaxLayer para = newCloudLayer.GetComponent<ParallaxLayer>();
		//float multiplier = ((float)i)/children;
		//para.distanceKM = (100f*multiplier*multiplier*multiplier);
		//print("dist:"+para.distanceKM);
		//print("children:"+children);


	}

	public void AdjustCloudLayer(int layerDepth)
	{
		if(!o_CloudLayer[layerDepth-1])
		{
			print("ERROR: Cloudlayer is null.");
			return;
		}
		float multiplier = ((float)layerDepth)/layerCount;
		float distanceKilometers = (100f*multiplier*multiplier*multiplier);
		Color dynamiColor = Color.Lerp(startColor,endColor,(distanceKilometers/100));
		Vector2 cloudOrigin = new Vector2(0, cloudLayerElevation);

		o_CloudLayer[layerDepth-1].GetComponent<CloudLayer>().Adjust(layerDepth, distanceKilometers, dynamiColor, cloudOrigin);
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
