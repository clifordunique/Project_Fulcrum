using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GrassColourer : MonoBehaviour 
{
	[SerializeField] public Color grassColor;
	[SerializeField] public SpriteRenderer[] grassSprites;

	public void ApplyChanges()
	{
		grassSprites = this.GetComponentsInChildren<SpriteRenderer>();
		foreach(SpriteRenderer sr in grassSprites)
		{
			sr.color = grassColor;
		}
	}

}
