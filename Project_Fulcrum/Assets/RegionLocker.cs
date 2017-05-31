using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RegionLocker : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
	{
		NetworkManager networkManager = FindObjectOfType<NetworkManager>();
		networkManager.SetMatchHost("us1-mm.unet.unity3d.com", networkManager.matchPort, true);
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
}
