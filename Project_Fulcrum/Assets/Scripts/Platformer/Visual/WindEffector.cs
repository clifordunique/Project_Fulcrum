using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindEffector : MonoBehaviour {
	[ReadOnlyAttribute][SerializeField]public Vector2 blowDirection;
	[SerializeField]public float g_Intensity; // Measured in kph airspeed.
	[SerializeField]public float g_IntensityDefault; // Measured in kph airspeed.
	[SerializeField]public int g_WindType; // See below for wind types
	// 0 - Radial Shockwave
	// 1 - Radial Blow
	// 2 - Radial Implode Shockwave
	// 3 - Directional Blow


	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public void Complete()
	{
		Destroy(this.transform.parent.gameObject);
	}
}
