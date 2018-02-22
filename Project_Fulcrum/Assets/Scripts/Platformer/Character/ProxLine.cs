using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ProxLine : MonoBehaviour {
	private float minDist = 50; 	// Min distance that proxline starts to fade out as fighers moves away from the player.
	private float maxDist = 300;	// Max distance where proxline is visible.
	[SerializeField][ReadOnlyAttribute] private float dist;
	[SerializeField][ReadOnlyAttribute] private float approachSpd;
	[SerializeField][ReadOnlyAttribute] private Vector2 curScale;
	[SerializeField][ReadOnlyAttribute] private Vector2 golScale;
	[SerializeField][ReadOnlyAttribute] private float curRot;
	[SerializeField][ReadOnlyAttribute] private float golRot;
	private float fadeInOpacity = 1;
	public FighterChar assignedFighter;
	public FighterChar originPlayer;
	private Transform spriteHolder;
	private SpriteRenderer spriteRenderer;
	[ReadOnlyAttribute]public Camera myCamera;
	private RectTransform myRect;


	// Use this for initialization
	void Start () 
	{
		fadeInOpacity = 1;
		myCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
		spriteHolder = this.transform.GetChild(0);
		spriteRenderer = spriteHolder.GetComponent<SpriteRenderer>();
		myRect = this.GetComponent<RectTransform>();
	}

	Vector2 LineToFighter()
	{
		Vector2 distance = assignedFighter.GetPosition()-originPlayer.GetPosition();
		//print("D1: "+distance);
		float theDistance = Vector3.Distance(assignedFighter.GetPosition(), originPlayer.GetPosition());
		//print("D2: "+theDistance);
		return distance;
	}

	void LerpScaleAndRot()
	{
		curScale.x = Mathf.Lerp(curScale.x, golScale.x, Time.deltaTime*5);
		curScale.y = Mathf.Lerp(curScale.y, golScale.y, Time.deltaTime*5);
		if(Math.Abs(curRot-golRot)>90)
		{
			curRot = golRot;
		}
		else
		{
			curRot = Mathf.Lerp(curRot, golRot, Time.deltaTime*5);
		}
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

		// SETTING VIEWPORT COORDINATES
		Vector3 screenPos;
		Vector2 onScreenPos;
		Vector2 WorldPos;
		float opacity = 0.5f;
		screenPos =myCamera.WorldToViewportPoint(assignedFighter.GetPosition());

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


		// DOING DIRECTION AND SPEED MATH
		Vector2 l2f = LineToFighter();
		dist = l2f.magnitude;
		float projectionVal;
		float angleSimilarity;
		if(l2f.sqrMagnitude == 0)
		{
			projectionVal = 0;
			angleSimilarity = 0;
		}
		else
		{
			projectionVal = Vector2.Dot(assignedFighter.GetVelocity(), l2f)/l2f.sqrMagnitude;
			angleSimilarity = Vector2.Dot(assignedFighter.GetVelocity().normalized, l2f.normalized)/l2f.normalized.sqrMagnitude;
		}

		Vector2 velProjected = projectionVal*l2f;

		approachSpd = velProjected.magnitude; //Speed that the fighter is approaching the player, found by projecting velocity onto the line between them.

		float distanceM = ((dist-minDist)/(maxDist-minDist));
		if(distanceM < 0){distanceM=0;}
		if(distanceM > 1){distanceM=1;}

//		print("mindist"+minDist);
//		print("MaxDist"+maxDist);
//		print("dist"+dist);
//		print("dist-minDist"+dist-minDist);
//		print("mindist"+minDist);

		Vector3 directionNormal = onScreenPos-new Vector2(0.5f,0.5f);

		if(approachSpd > 0 && projectionVal<0&&angleSimilarity<-0.9f) 
		{
			directionNormal = -assignedFighter.GetVelocity();
			directionNormal.Normalize();
			golScale = new Vector3(25+(approachSpd),5,1);
		}
		else
		{
			golScale = new Vector3(25,5,1);
		}


		directionNormal.Normalize();
		if(directionNormal != Vector3.zero)
		{
			if(Math.Abs(directionNormal.x) < 0.01f){directionNormal.x = 0;} //Duct tape fix
			if(Math.Abs(directionNormal.y) < 0.01f){directionNormal.y = 0;} //Duct tape fix
			Quaternion rotationAngle = Quaternion.LookRotation(directionNormal);
			rotationAngle.x = 0;
			rotationAngle.y = 0;

			if(directionNormal.x == 0) //Duct tape fix
			{
				if(directionNormal.y < 0)
				{
					rotationAngle.eulerAngles = new Vector3(0, 0, -90);
				}
				else if(directionNormal.y > 0)
				{
					rotationAngle.eulerAngles = new Vector3(0, 0, 90);
				}
				else
				{
					print("ERROR: IMPACT DIRECTION OF (0,0)");
				}
			}

			if(directionNormal.x < 0)
			{
				rotationAngle.eulerAngles = new Vector3(0, 0, rotationAngle.eulerAngles.z-180);
			}
			golRot = rotationAngle.eulerAngles.z;
		}
		myRect.localScale = curScale;
		Quaternion newRotation = Quaternion.identity;
		newRotation.eulerAngles = new Vector3(0, 0, curRot);
		myRect.rotation = newRotation;
		LerpScaleAndRot();

		#region Opacity
		opacity = 1;//-distanceM;

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
		#endregion
		//COLOR CODING
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
