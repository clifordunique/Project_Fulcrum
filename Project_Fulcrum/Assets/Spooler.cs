using UnityEngine.UI;
using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Spooler : MonoBehaviour 
{
	#region OBJECT REFERENCES
	[SerializeField]private GameObject o_SpoolRingPrefab;
	[SerializeField]private GameObject o_Player;
	[SerializeField]private GameObject[] o_Rings;
	[SerializeField]private GameObject[] o_BadRings;
	#endregion
	#region RINGPARAMS
	[SerializeField][Range(0,0.1f)] private float r_RingWidth;
	[SerializeField][Range(0,0.1f)] private float r_RingGap;
	[SerializeField][ReadOnlyAttribute] private float r_CurTime;
	[SerializeField][Range(0,10f)] private float r_MaxTime;
	[SerializeField][ReadOnlyAttribute] private int r_RingNum;
	[SerializeField][ReadOnlyAttribute] private bool r_Paused;
	[SerializeField][ReadOnlyAttribute] private bool r_Active;	 // True when a ring is mid spool.
	#endregion
	#region PLAYERINPUT
	private bool i_Jump;
	private bool i_KeyLeft;
	private bool i_KeyRight;
	private bool i_KeyUp;
	private bool i_KeyDown;
	private bool i_Spool;

	private int CtrlH; 		// Tracks horizontal keys pressed. Values are -1 (left), 0 (none), or 1 (right). 
	private int CtrlV; 		// Tracks vertical keys pressed. Values are -1 (down), 0 (none), or 1 (up).
	private bool facingDirection; 		// true means right (the direction), false means left.
	#endregion

	// Use this for initialization
	void Start () 
	{
		r_Paused = true;
		r_CurTime = 0;
		r_RingNum = -1;
	}
	
	// Update is called once per frame
	void Update () 
	{
		i_KeyLeft = CrossPlatformInputManager.GetButton("Left");
		i_KeyRight = CrossPlatformInputManager.GetButton("Right");
		i_KeyUp = CrossPlatformInputManager.GetButton("Up");
		i_KeyDown = CrossPlatformInputManager.GetButton("Down");
		i_Spool = CrossPlatformInputManager.GetButtonDown("Spooling");

		if(i_Spool)
		{
			if(r_RingNum < 4)
			{
				print("SPOOLING BEGINS!");	
				r_RingNum++;
				o_Rings[r_RingNum] = (GameObject)Instantiate(o_SpoolRingPrefab);
				o_Rings[r_RingNum].name = "Ring_"+r_RingNum;
				o_Rings[r_RingNum].transform.SetParent(this.transform, false);
				//o_Rings[r_RingNum].transform.localPosition = new Vector2(1*r_RingNum, 0);
				o_Rings[r_RingNum].GetComponent<Renderer>().material.SetFloat("_Angle", 0);
				o_Rings[r_RingNum].GetComponent<Renderer>().material.SetFloat("_Radius", (r_RingWidth+r_RingGap)*(r_RingNum+1));
				o_Rings[r_RingNum].GetComponent<Renderer>().material.SetFloat("_RingWidth", r_RingWidth);
				//o_Rings[r_RingNum].GetComponent<Renderer>().material.SetColor("_Color", new Vector4(1,1,1,0.4f));

				o_BadRings[r_RingNum] = (GameObject)Instantiate(o_SpoolRingPrefab);
				o_BadRings[r_RingNum].name = "BadRing_"+r_RingNum;
				o_BadRings[r_RingNum].transform.SetParent(this.transform, false);
				//o_BadRings[r_RingNum].transform.localScale = new Vector3(-1,1,1);
				o_BadRings[r_RingNum].GetComponent<Renderer>().material.SetFloat("_Angle", 0);
				o_BadRings[r_RingNum].GetComponent<Renderer>().material.SetFloat("_Radius", (r_RingWidth+r_RingGap)*(r_RingNum+1));
				o_BadRings[r_RingNum].GetComponent<Renderer>().material.SetFloat("_RingWidth", r_RingWidth);
				o_BadRings[r_RingNum].GetComponent<Renderer>().material.SetColor("_Color", new Vector4(0,0,0,1));
				o_BadRings[r_RingNum].GetComponent<Renderer>().sortingOrder = 1;

				r_Paused = false;
				r_CurTime = 0;
			}
			else
			{
				print("SPOOLING ENDS!");
				r_Paused = true;
			}
		}

		if(!r_Paused && r_RingNum >= 0 && r_RingNum  < 5)
		{
			
			r_CurTime += Time.deltaTime;
			if(r_CurTime <= r_MaxTime)
			{
				print("SPOOLING!");	
				float radians = (r_CurTime/r_MaxTime)*6.28f;
				o_Rings[r_RingNum].GetComponent<Renderer>().material.SetFloat("_Angle", radians);
				//print(radians);
			}
			else if(r_CurTime <= r_MaxTime*2)
			{
				print("BADSPOOLING!");	
				o_Rings[r_RingNum].GetComponent<Renderer>().material.SetFloat("_Angle", 6.28f);
				float radians = ((r_CurTime/r_MaxTime)*6.28f)-6.28f;
				o_BadRings[r_RingNum].GetComponent<Renderer>().material.SetFloat("_Angle", radians);
				//print(radians);
			}
			else
			{
				o_BadRings[r_RingNum].GetComponent<Renderer>().material.SetFloat("_Angle", 6.28f);
				o_Rings[r_RingNum].GetComponent<Renderer>().material.SetFloat("_Angle", 6.28f);
				print("SPOOLING ENDS!");	
				r_Paused = true;
			}
		}
	}
}
