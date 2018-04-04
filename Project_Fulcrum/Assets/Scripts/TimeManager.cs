using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AK.Wwise;

public class TimeManager : MonoBehaviour {
	private bool permaSlow;
	private float slowmoTimer = 0;
	public float timeSpeed;

	public void TimeDilation(float timeDilationM)
	{
		permaSlow = true;
		Time.timeScale = timeDilationM;
	}

	public void TimeDilation(float timeDilationM, float duration)
	{
		permaSlow = false;
		slowmoTimer = duration;
		Time.timeScale = timeDilationM;
	}

	public void ExitTimeDilation()
	{
		permaSlow = false;
		slowmoTimer = 0;
		Time.timeScale = 1;
	}

	public void ExitTimeDilationGradual()
	{
		slowmoTimer = 0;

		Time.timeScale = Mathf.Lerp(Time.timeScale, 1, Time.fixedUnscaledDeltaTime*5);
		if(Mathf.Abs(Time.timeScale-1)<0.05f) // if timescale is within 0.05 of normal time speed, set it to normal
		{
			ExitTimeDilation();
		} 
	}

	public float GetTimeDilationM()
	{
		return timeSpeed;
	}

	// Update is called once per frame
	void Update () 
	{
		timeSpeed = Time.timeScale;
		AkSoundEngine.SetRTPCValue("TimeDilation", Time.timeScale);
	}

	// FixedUpdate is called once per physics frame
	void FixedUpdate () 
	{		
		if(permaSlow){return;}
		if(slowmoTimer>0)
		{
			//print("slowmoTimer = "+slowmoTimer);
			slowmoTimer -= Time.fixedUnscaledDeltaTime;
		}
		else
		{
			ExitTimeDilationGradual();
		}
	}
}
