using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviour 
{
	
	#region visualparams
	[SerializeField] public Vector4 color = new Vector4(1,1,1,1);		// Colour.
	[SerializeField][Range(0,360)] public float rotation = 0; 			// Starting rotation.
	[SerializeField][Range(0,0.1f)] public float radius = 0; 			// Inner radius.
	[SerializeField][Range(0,0.1f)] public float thickness = 0; 		// Width of ring.
	[SerializeField][Range(0,6.28f)]  public float fillAmount = 0; 		// How much the ring is filled, in radians. 0 to 6.28.
	[SerializeField] public int depth = 0; 								// Render depth.
	[SerializeField] private Renderer renderer;							// Sprite renderer
	#endregion

	#region gameparams
	#endregion

	// Use this for initialization
	void Start () 
	{
		renderer = this.GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		UpdateVisuals();
	}

	public void UpdateVisuals()
	{
		renderer.material.SetFloat("_Angle", fillAmount);
		renderer.material.SetFloat("_Radius", radius);
		renderer.material.SetColor("_Color", color);
		renderer.material.SetFloat("_RingWidth", thickness);
		renderer.sortingOrder = depth;
		Vector3 rot = new Vector3(0,0,rotation + 90);
		this.transform.localEulerAngles = rot;
	}
		
}
