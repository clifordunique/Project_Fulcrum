using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityLiner : MonoBehaviour {

	[ReadOnlyAttribute]public FighterChar o_LocalPlayer;
	[ReadOnlyAttribute]public List<FighterChar> o_FighterList;
	[ReadOnlyAttribute]public List<GameObject> o_ProxLine;
	[ReadOnlyAttribute]public Camera camera;
	[ReadOnlyAttribute]public GameObject Canvas;
	[ReadOnlyAttribute]public float outerRange = 100;
	public GameObject p_ProxLinePrefab;

	void Start()
	{
		o_LocalPlayer = this.transform.GetComponent<FighterChar>();
		if(!o_LocalPlayer.isLocalPlayer)
		{
			this.enabled = false;
			return;
		}
		camera = Camera.main;
		Canvas = GameObject.Find("Canvas");
		o_ProxLine = new List<GameObject>();
	}

	public void ClearAllFighters()
	{
		foreach(GameObject proxline in o_ProxLine)
		{
			Destroy(proxline);
		}
		o_ProxLine.Clear();
		o_FighterList.Clear();
	}

	public void AddFighter(FighterChar fighterChar)
	{
		if( (fighterChar == o_LocalPlayer) || (o_FighterList.Contains(fighterChar)) ){return;} //If the added fighter is the player or is already in the list, skip it.
		o_FighterList.Add(fighterChar);
		o_ProxLine.Add((GameObject)Instantiate(p_ProxLinePrefab, Canvas.transform));
		ProxLine p = o_ProxLine[o_ProxLine.Count-1].GetComponent<ProxLine>();
		p.assignedFighter = fighterChar;
		p.originPlayer = o_LocalPlayer;
	}

	public void DetectAllFighters()
	{
		ClearAllFighters();
		FighterChar[] o_FighterArray = FindObjectsOfType(typeof(FighterChar)) as FighterChar[];

		for(int i = 0; i < o_FighterArray.Length; i++)
		{
			AddFighter(o_FighterArray[i]);
		}

		//o_FighterList.AddRange(o_FighterArray);
//		if(o_ProxLine.Count < o_FighterList.Count)
//		{
//			for(int i = o_ProxLine.Count; i < o_FighterList.Count; i++)
//			{
//				//if(o_FighterList[i] == o_LocalPlayer){continue;}
//				o_ProxLine.Add((GameObject)Instantiate(p_ProxLinePrefab, Canvas.transform));
//				o_ProxLine[i].GetComponent<ProxLine>().assignedFighter = o_FighterList[i];
//				ProxLine p = o_ProxLine[i].GetComponent<ProxLine>();
//				p.assignedFighter = o_FighterList[i];
//				p.originPlayer = o_LocalPlayer;
//			}
//		}
	}

	void Update () 
	{
		
	}
}
