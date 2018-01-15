using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrandJumpEffect : MonoBehaviour {

	[SerializeField][ReadOnlyAttribute] public float secondsPassed;
	[SerializeField] private float duration = 1f;
	[SerializeField][ReadOnlyAttribute] private FighterChar fighterChar;
	[SerializeField][ReadOnlyAttribute] private LineRenderer strandLine;
	[SerializeField][ReadOnlyAttribute] private bool animPlaying;
	[SerializeField] private Transform startPoint;
	[SerializeField] private Transform endPoint;



	public void SetFighterChar(FighterChar originChar)
	{
		fighterChar = originChar;
	}

	void Start()
	{
		strandLine = this.GetComponent<LineRenderer>();
		Vector3[] points = new Vector3[3];
		points[0] = startPoint.position;
		points[1] = fighterChar.GetPosition();
		points[2] = endPoint.position;
		strandLine.SetPositions(points);
		animPlaying = true;
	}

	void FixedUpdate()
	{
		if(fighterChar.IsKinematic()&&animPlaying)
		{
			strandLine.SetPosition(1, fighterChar.GetPosition());
		}
		else
		{
			animPlaying = false;
			strandLine.SetPosition(1, this.transform.position);
			secondsPassed += Time.fixedDeltaTime;
			if(secondsPassed>=duration)
			{
				Complete();
			}
		}
	}

	public void Complete()
	{
		Destroy(this.gameObject);
	}
}
