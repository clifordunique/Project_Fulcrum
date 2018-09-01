using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirBurst : WindEffector {
	[ReadOnlyAttribute][SerializeField]public float g_MaxRange;		// Final radius of the airburst.
	[ReadOnlyAttribute][SerializeField]public float g_MinRange;		// Starting radius of the airburst.
	[ReadOnlyAttribute][SerializeField]public float g_CurRange;		// Current radius of the airburst.
	[ReadOnlyAttribute][SerializeField]public float g_ExpandTime; 	// Time it takes for airburst to reach maximum radius.
	[ReadOnlyAttribute][SerializeField]public float g_Duration; 	// Duration of the airburst. Recommended to make this >= g_ExpandTime
	[ReadOnlyAttribute][SerializeField]public float g_TimeAlive; 	// Time since the airburst appeared. When it reaches duration, the airburst is destroyed.
	//[ReadOnlyAttribute][SerializeField]public bool g_IsShockwave;	// When true, sets windType to 0(radial shockwave). When false, sets windType to 1(radial blow).
	[ReadOnlyAttribute][SerializeField]public WindZone g_WindZone; 	// WindEthere - Unity component used to effect particle systems.

	// Use this for initialization
	void Awake() 
	{
		g_WindZone = this.GetComponent<WindZone>();
		g_MaxRange = 20;		// Max radius of the airburst.
		g_ExpandTime = 0.3f; 	// Time it takes for airburst to reach maximum radius.
		g_Duration = 0.3f; 		// Duration of the airburst. Recommended to make this >= g_ExpandTime
		blowDirection = Vector2.zero;
		g_IntensityDefault = 300;
		g_Intensity = g_IntensityDefault;
	}

	public void Create(bool isShockwave, float minRange, float maxRange, float expandTime, float duration, float intensity)
	{
		bool isAShockwave = isShockwave;
		g_MinRange = minRange;
		g_MaxRange = maxRange;		// Max radius of the airburst.

		if(expandTime > duration)
		{
			expandTime = duration;
		}
		g_ExpandTime = expandTime; 	// Time it takes for airburst to reach maximum radius.
		g_Duration = duration; 		// Duration of the airburst. Recommended to make this >= g_ExpandTime
		g_IntensityDefault = intensity;
		g_Intensity = g_IntensityDefault;
		if(isAShockwave)
		{
			g_WindType = 0;
		}
		else
		{
			g_WindType = 1;
		}
		g_WindZone.windMain = g_Intensity;
		g_WindZone.radius = g_CurRange;
	}

	public void Create(bool isShockwave, float maxRange, float duration, float intensity)
	{
		bool isAShockwave = isShockwave;
		g_MinRange = 0;
		g_MaxRange = maxRange;		// Max radius of the airburst.
		g_ExpandTime = duration; 	// Time it takes for airburst to reach maximum radius.
		g_Duration = duration; 		// Duration of the airburst. Recommended to make this >= g_ExpandTime
		g_IntensityDefault = intensity;
		g_Intensity = g_IntensityDefault;
		g_WindZone.windMain = g_Intensity;
		if(isAShockwave)
		{
			g_WindType = 0;
		}
		else
		{
			g_WindType = 1;
		}
		g_WindZone.windMain = g_Intensity;
		g_WindZone.radius = g_CurRange;
	}

	void  FixedUpdate () 
	{
		g_TimeAlive += Time.fixedDeltaTime;
		if(g_TimeAlive >= g_Duration)
		{
			Complete();
			return;
		}
		this.transform.localScale = new Vector3(g_CurRange,g_CurRange,1);
		if(g_TimeAlive<g_ExpandTime)
		{
			g_CurRange = g_TimeAlive*(g_MaxRange/g_ExpandTime);
		}else
		{
			g_CurRange = g_MaxRange;
		}
		if(g_WindType == 0)
		{
			g_Intensity = g_IntensityDefault*(1-(g_CurRange/g_MaxRange));
			if(g_Intensity <= 0.1f)
			{
				g_Intensity = 0;
			}
		}
		g_WindZone.windMain = -g_Intensity/2;
		g_WindZone.radius = g_CurRange;
	}
}
