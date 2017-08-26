using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParallaxLayer))]

public class CloudLayer : MonoBehaviour {
	public GameObject[] cloudSpritesL;
	public GameObject[] cloudSpritesM;
	public GameObject[] cloudSpritesS;
	public GameObject[] allClouds;


	public float cloudXRandomness = 200; 
	public float distanceKM;
	public int layerDepth;
	public Color cloudColor;

	public float spriteWidth;
	public int cloudCount;

	public ParallaxLayer parallaxLayer;
	public bool isInitialized;

	// Use this for initialization
	void Start() 
	{

	}

	void Awake()
	{
		parallaxLayer = this.GetComponent<ParallaxLayer>();	
		allClouds = new GameObject[cloudCount];
		spriteWidth = cloudSpritesL[0].GetComponent<SpriteRenderer>().bounds.size.x;
//		if(!isInitialized)
//		{
//			Initialize(1, 10, Color.blue);
//		}
		//print("AWAKE!");
	}

	public void Initialize(int lyrDepth, float distKM, Color color)
	{
		//print("Initializing! \n LayerDepth: "+lyrDepth+"\n DistanceKM: "+distKM+"\n Colour: "+color);
		distanceKM = distKM;
		layerDepth = lyrDepth;
		cloudColor = color;
		parallaxLayer.distanceKM = distKM;
		int sortOrder= -100-layerDepth*5;
		float randomXOffset = Random.Range(-cloudXRandomness,cloudXRandomness);
		if(distKM > 0)
		{
			this.gameObject.layer = 11;
		}
		else
		{
			this.gameObject.layer = 15;
		}
		for(int i = 0; i < cloudCount; i++)
		{
			Vector2 cloudOrigin = new Vector2(this.transform.position.x+(i*spriteWidth)-(spriteWidth*cloudCount/2),this.transform.position.y);
			GameObject newCloud = (GameObject)Instantiate(cloudSpritesL[0], cloudOrigin, Quaternion.identity, this.transform);
			newCloud.transform.localPosition = new Vector3(newCloud.transform.localPosition.x+randomXOffset, newCloud.transform.localPosition.y, newCloud.transform.localPosition.z);

			SpriteRenderer cloudSprite = newCloud.GetComponent<SpriteRenderer>();
			cloudSprite.color = color;
			cloudSprite.sortingLayerName = "Background";
			cloudSprite.sortingOrder = sortOrder;

			newCloud.name = "Cloud_"+layerDepth+"_"+i;
			newCloud.layer = this.gameObject.layer;
			allClouds[i] = newCloud;
			isInitialized = true;
			//print("Initialization of Cloud_"+layerDepth+"_"+i+" is complete");
		}
	}

	public void Adjust(int lyrDepth, float distKM, Color color)
	{
		//print("Adjust activated.");
		distanceKM = distKM;
		layerDepth = lyrDepth;
		cloudColor = color;
		parallaxLayer.distanceKM = distanceKM;

		if(distKM > 0)
		{
			this.gameObject.layer = 11;
		}
		else
		{
			this.gameObject.layer = 15;
		}

		for(int i = 0; i < allClouds.Length; i++)
		{
			SpriteRenderer cloudSprite = allClouds[i].GetComponent<SpriteRenderer>();
			//allClouds[i].transform.localPosition = new Vector3(allClouds[i].transform.localPosition.x+randomXOffset, allClouds[i].transform.localPosition.y, allClouds[i].transform.localPosition.z);
			int sortOrder= -100-layerDepth*5;
			cloudSprite.color = cloudColor;
			cloudSprite.sortingLayerName = "Background";
			cloudSprite.sortingOrder = sortOrder;
		}
	}

//
	// Update is called once per frame
	void Update () 
	{
		
	}
}
	