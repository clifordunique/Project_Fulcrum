using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class ParallaxHandler : MonoBehaviour {
	[SerializeField] float distributionCurveM = 0.25f;
	int updateEvery100Frames = 0;



	// Use this for initialization
	void ParallaxUpdateDistribute() 
	{
		int children = transform.childCount;
		for(int i = 1; i<children; ++i)
		{
			print("i:"+i);
			SortingGroup mySortingGroup;
			GameObject myGameObject = transform.GetChild(i).gameObject;
			ParallaxLayer para = myGameObject.GetComponent<ParallaxLayer>();
			float multiplier = ((float)i)/children;
			para.distanceKM = (100f*multiplier*multiplier*multiplier);
			print("dist:"+para.distanceKM);
			print("children:"+children);

			if(myGameObject.GetComponent<SortingGroup>())
			{
				mySortingGroup = myGameObject.GetComponent<SortingGroup>();
				mySortingGroup.sortingOrder = -100-i;
			}
			else
			{
				mySortingGroup = myGameObject.AddComponent<SortingGroup>();
				mySortingGroup.sortingOrder = -100-i;
			}

			mySortingGroup.sortingLayerName = "Background";

		}
	}

	// Use this for initialization
	void ParallaxUpdateFill() 
	{
		int i = 1;
		foreach(Transform t in transform)
		{
			SortingGroup mySortingGroup;
			GameObject myGameObject = t.gameObject;
			ParallaxLayer para = myGameObject.GetComponent<ParallaxLayer>();
			//para.distanceKM = 10*Mathf.Log10(0.25f*(Mathf.Pow(2, i)));
			para.distanceKM =  i*(i-1)*distributionCurveM;

			if(myGameObject.GetComponent<SortingGroup>())
			{
				mySortingGroup = myGameObject.GetComponent<SortingGroup>();
				mySortingGroup.sortingOrder = -100+i;
			}
			else
			{
				mySortingGroup = myGameObject.AddComponent<SortingGroup>();
				mySortingGroup.sortingOrder = -100+i;
			}
				
			mySortingGroup.sortingLayerName = "Background";

			i++;
		}	
	}

	
	// Update is called once per frame
	void Update() 
	{
		if(updateEvery100Frames>=100)
		{
			ParallaxUpdateDistribute();
			updateEvery100Frames = 0;
		}
		else
		{
			updateEvery100Frames++;
		}
	}
}
