﻿/*
 THANKS TO DAVID DION-PAQUET AND HIS BLOG POST!
 http://www.gamasutra.com/blogs/DavidDionPaquet/20140601/218766/Creating_a_parallax_system_in_Unity3D_is_harder_than_it_seems.php
*/

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ParallaxLayer : MonoBehaviour {
	public float speedX;
	public float speedY;
	public float parallaxMovement; // full movement at 1, no movement at 0
	[Range(0, 100)]public float distanceKM = -1;
	public bool moveInOppositeDirection;

	private Transform cameraTransform;
	private Vector3 previousCameraPosition;
	private bool previousMoveParallax;
	private ParallaxOption options;

	GameObject gameCamera;
	[SerializeField] private bool isNewStyle;
	[SerializeField] private Vector3 worldSpaceCoords;
	[SerializeField] private Vector3 cameraSpaceCoords;

	void OnEnable() 
	{
		gameCamera = GameObject.Find("Main Camera");
		if(gameCamera == null)
		{
			print("CAMERA NOT HERE!");
		}
		options = gameCamera.GetComponent<ParallaxOption>();
		if(options == null)
		{
			print("ParallaxOption NOT HERE!");
		}
		cameraTransform = gameCamera.transform;
		previousCameraPosition = cameraTransform.position;
	}

	void Update () 
	{
		if(!isNewStyle)
		{
			OldStyleUpdate();
		}
		else
		{
			WorldSpaceUpdate();
		}
	}

	void WorldSpaceUpdate()
	{
		float distanceFactor = Mathf.Pow((distanceKM/100f)-1, 2);
		//this.transform.localScale = new Vector3((1.0f/(1.0f+(distanceKM/2f))),(1.0f/(1.0f+(distanceKM/2f))),1);
		this.transform.localScale = new Vector3(distanceFactor/2,distanceFactor/2,distanceFactor/2);
		//parallaxMovement = 1-Mathf.Pow((distanceKM/100f)-1, 2);// Mathf.Log10(distanceKM);
		speedX = 1-distanceFactor;
		speedY = 1-distanceFactor;
		if(!Application.isPlaying && !options.moveParallax){return;}
		cameraSpaceCoords = worldSpaceCoords+(gameCamera.transform.position*speedX);
		transform.position = cameraSpaceCoords;
	}

	void OldStyleUpdate()
	{
		if(distanceKM > 0.005f)
		{
			this.transform.localScale = new Vector3((1.0f/(1.0f+distanceKM)),(1.0f/(1.0f+distanceKM)),1);
			parallaxMovement = 1/distanceKM;
			speedX = 1-parallaxMovement;
			speedY = 1-parallaxMovement;
		}
		if(options.moveParallax && !previousMoveParallax)
		{
			previousCameraPosition = cameraTransform.position;
		}

		previousMoveParallax = options.moveParallax;

		if(!Application.isPlaying && !options.moveParallax){return;}

		Vector3 distance = cameraTransform.position - previousCameraPosition;
		float direction = (moveInOppositeDirection) ? -1f : 1f;
		transform.position += Vector3.Scale(distance, new Vector3(speedX, speedY)) * direction;

		previousCameraPosition = cameraTransform.position;
	}
}
