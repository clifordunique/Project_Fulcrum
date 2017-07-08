using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProxLine : MonoBehaviour {
	private float minDist = 50; 	// Min distance that proxline starts to fade out as fighers moves away from the player.
	private float maxDist = 300;	// Max distance where proxline is visible.
	[SerializeField][ReadOnlyAttribute] private float dist;
	private float fadeInOpacity = 1;
	public FighterChar assignedFighter;
	public FighterChar originPlayer;
	private Transform spriteHolder;
	private SpriteRenderer spriteRenderer;
	[ReadOnlyAttribute]public Camera camera;
	private RectTransform myRect;

	// Use this for initialization
	void Start () 
	{
		fadeInOpacity = 1;
		camera = Camera.main;
		spriteHolder = this.transform.GetChild(0);
		spriteRenderer = spriteHolder.GetComponent<SpriteRenderer>();
		myRect = this.GetComponent<RectTransform>();
	}

	float DistanceToFighter()
	{
		Vector2 distance = assignedFighter.GetPosition()-originPlayer.GetPosition();
		//print("D1: "+distance);
		float theDistance = Vector3.Distance(assignedFighter.GetPosition(), originPlayer.GetPosition());
		//print("D2: "+theDistance);
		return distance.magnitude;
	}

	// Update is called once per frame
	void Update () 
	{
		if(assignedFighter == null)
		{
			print("ASSIGNEDFIGHTER IS NULL FOR PROXLINE!");
			return;
		}

		if(!assignedFighter.isAlive())
		{
			spriteHolder.gameObject.SetActive(false);
			return;
		}
		Vector3 screenPos;
		Vector2 onScreenPos;
		Vector2 WorldPos;
		float opacity = 0.5f;
		screenPos = camera.WorldToViewportPoint(assignedFighter.GetPosition());

		if(screenPos.x >= 0 && screenPos.x <= 1 && screenPos.y >= 0 && screenPos.y <= 1)
		{
			spriteHolder.gameObject.SetActive(false);
			return;
		}
		else
		{
			spriteHolder.gameObject.SetActive(true);
		}
		onScreenPos = new Vector2(screenPos.x-0.5f, screenPos.y-0.5f)*2; //2D version, new mapping
		float max = Mathf.Max(Mathf.Abs(onScreenPos.x), Mathf.Abs(onScreenPos.y)); //get largest offset
		onScreenPos = (onScreenPos/(max*2))+new Vector2(0.5f, 0.5f); //undo mapping

		myRect.anchorMin = onScreenPos;
		myRect.anchorMax = onScreenPos;

		Vector3 directionNormal = onScreenPos-new Vector2(0.5f,0.5f);

		directionNormal.Normalize();

		if(directionNormal != Vector3.zero)
		{
			Quaternion rotationAngle = Quaternion.LookRotation(directionNormal);
			rotationAngle.x = 0;
			rotationAngle.y = 0;
			if(directionNormal.x < 0)
			{
				rotationAngle.eulerAngles = new Vector3(0, 0, rotationAngle.eulerAngles.z-180);
			}
			myRect.rotation = rotationAngle;
		}

//		if(assignedFighter.GetSpeed()>=50)
//		{
//			opacity = 1;
//		}
//		else
//		{
//			opacity = 0.35f;
//		}

		dist = DistanceToFighter();

		float distanceM = ((dist-minDist)/(maxDist-minDist));
		if(distanceM < 0){distanceM=0;}
		if(distanceM > 1){distanceM=1;}
//		print("mindist"+minDist);
//		print("MaxDist"+maxDist);
//		print("dist"+dist);
//		print("dist-minDist"+dist-minDist);
//		print("mindist"+minDist);


		opacity = 1-distanceM;

		if(opacity < 0)
		{
			opacity = 0;
		}

		if(fadeInOpacity > 0)
		{
			fadeInOpacity -= Time.fixedDeltaTime;
		}
		else
		{
			fadeInOpacity = 0;
		}

		if(fadeInOpacity>opacity)
		{
			opacity = fadeInOpacity;
		}

		if(assignedFighter.IsPlayer())
		{
			//print("POS: "+assignedFighter.GetPosition());
			spriteRenderer.color = new Color(0,1,0,opacity);
		}
		else
		{
			spriteRenderer.color = new Color(1,1,1,opacity);
		}
	}
}
