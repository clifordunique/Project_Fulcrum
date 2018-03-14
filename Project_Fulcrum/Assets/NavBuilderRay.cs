using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class NavBuilderRay : MonoBehaviour {

	[SerializeField] private GameObject p_NavSurfPrefab; // Nav surface.
	[SerializeField] private RaycastHit2D testRay;
	[SerializeField] public LayerMask terrainMask;	// Mask used for terrain collisions.
	[SerializeField][Range(0.01f, 1)] private float raycastDensity;	// How much space between raycasts when testing along the surface.
	[SerializeField][Range(0.01f, 1)] private float raycastHeight = 0.1f;	// How much space between raycasts when testing along the surface.
	[SerializeField] private Vector2 surfaceNormal;
	[SerializeField] private Vector2 nextLineDirection;
	[SerializeField] private Vector2 nextLineOrigin;
	[SerializeField] private int surfaceIterations;
	[SerializeField] private List<Vector2> exploredPointList;

	[SerializeField] private LineRenderer v_VisLine; //debug line.
	[SerializeField] private bool v_IsSeedIteration;
	[SerializeField][ReadOnlyAttribute] private bool v_SparkJumping;
	[SerializeField] private bool v_isVisible;	// true when linerenderer is visible



	// Use this for initialization
	void Start () 
	{
		v_VisLine = this.gameObject.GetComponent<LineRenderer>();
	}

	public void GenerateNavSurface(Vector2 rayDirection, Vector2 rayOrigin)
	{
		surfaceIterations++;
		if(surfaceIterations>=200)
		{
			return;
		}
		nextLineDirection = Vector2.zero;
		v_isVisible = true;
		v_VisLine = this.gameObject.GetComponent<LineRenderer>();
		testRay = Physics2D.Raycast(rayOrigin, rayDirection, rayDirection.magnitude, terrainMask); 	
		if(testRay)
		{
			v_SparkJumping = false;
			if(testRay.distance<0.0001f)
			{
				print("Navgenerator originated inside of terrain, disregarding.");
				return;
			}
			print("Hit surface!");
			surfaceNormal = testRay.normal;
			Vector2 surfacePara = Perp(surfaceNormal);

			if(surfacePara.x<0)
			{
				surfacePara *= -1; // Ensures that surfacePara always faces left.
			}

			if(surfaceNormal.y<0) // Ensures that upside down surfaces' surfacePara always face right. This means that the surfaces first node will be on the right, causing the steepness calculation to evaluate it as an upside down surface.
			{
				surfacePara *= -1;
			}

			Vector3[] positions = { testRay.point+surfaceNormal*0.1f, testRay.point+surfaceNormal*0.1f };

			RaycastHit2D obstacleFinderRight = Physics2D.Raycast(testRay.point+surfaceNormal*0.1f, surfacePara, 100, terrainMask); 	// horizontal ray to detect walls that would prematurely end the navsurface.

			int r;
			for(r = 1; r<3000; r++)
			{
				if(obstacleFinderRight && obstacleFinderRight.distance-0.05f<r*raycastDensity)
				{
					print(r+": Obstacle on right side, ending...");
					positions[1] = obstacleFinderRight.point-surfacePara*0.1f;
					break;
				}
				Vector3 newRayStartPos = (testRay.point+surfaceNormal*0.1f)+surfacePara*r*raycastDensity;
				RaycastHit2D newIncrementalRay = Physics2D.Raycast(newRayStartPos, -surfaceNormal, 0.2f, terrainMask); 	// 
				if(newIncrementalRay)
				{
					if(newIncrementalRay.normal==surfaceNormal && newIncrementalRay.distance<=0.15f)
					{
						positions[1] = newIncrementalRay.point+surfaceNormal*0.1f;
					}
					else
					{
						break;
					}
				}
				else
				{
					break;
				}
			}

			RaycastHit2D obstacleFinderLeft = Physics2D.Raycast(testRay.point+surfaceNormal*0.1f, -surfacePara, 100, terrainMask); 	// horizontal ray to detect walls that would prematurely end the navsurface.
			int l;
			for(l = -1; l>-3000; l--)// Testing left of origin
			{
				if(obstacleFinderLeft && obstacleFinderLeft.distance-0.05f<Mathf.Abs(l*raycastDensity))
				{
					print(l+": Obstacle on left side, ending...");
					positions[0] = obstacleFinderLeft.point+surfacePara*0.1f;
					break;
				}
				Vector3 newRayStartPos = (testRay.point+surfaceNormal*0.1f)+surfacePara*l*raycastDensity;
				RaycastHit2D newIncrementalRay = Physics2D.Raycast(newRayStartPos, -surfaceNormal, 0.2f, terrainMask); 	// Ground
				if(newIncrementalRay)
				{
					if(newIncrementalRay.normal==surfaceNormal && newIncrementalRay.distance<=0.15f)
					{
						positions[0] = newIncrementalRay.point+surfaceNormal*0.1f;
					}
					else
					{
						break;
					}
				}
				else
				{
					break;
				}
			}
			v_VisLine.SetPositions(positions);

			Vector2 midpoint = positions[0]+(positions[1]-positions[0])/2;
			foreach(Vector2 vec in exploredPointList)
			{
				if((midpoint-vec).magnitude<0.5f)
				{
					
					print(midpoint+" is too close to "+vec+"; Arrived at preexisting surface, ending.");
					return;
				}
			}

			if(r+Mathf.Abs(l)>=3) // If the tested surface is sufficiently wide, generate a new nav surface.
			{
				nextLineDirection = surfacePara*0.5f;
				nextLineOrigin = (Vector2)positions[1]-surfacePara*0.05f;
				exploredPointList.Add(midpoint);
				NavSurface newNavSurface = Instantiate(p_NavSurfPrefab, (positions[0]+positions[1])/2, Quaternion.identity).GetComponent<NavSurface>();
				newNavSurface.Initialize(positions[0], positions[1]);
			}
			else
			{
				print("Surface too small!");
			}
		}
		else
		{
			
			nextLineDirection = Rotate(rayDirection, -15);
			nextLineDirection = nextLineDirection.normalized*0.1f;
			if(v_SparkJumping)
			{
				nextLineOrigin = rayOrigin+rayDirection;
			}
			else
			{
				nextLineOrigin = rayOrigin;
			}
			v_SparkJumping = true;
			print("Missed next surface, curving around corner.");
		}
		Vector3[] linePositions = {nextLineOrigin, nextLineOrigin+nextLineDirection};
		v_VisLine.SetPositions(linePositions);
	}

	public void ManualGenByStep()
	{
		if(v_IsSeedIteration)
		{
			GenerateNavSurface();
			v_IsSeedIteration = false;
		}
		else
		{
			if(nextLineDirection!=Vector2.zero)
			{
				GenerateNavSurface(nextLineDirection, nextLineOrigin);
			}
			else
			{
				print("Reached the end.");
				v_IsSeedIteration = true;
			}
		}
	}

	public void GenerateNavSurface()
	{		
		v_SparkJumping = false;
		exploredPointList = new List<Vector2>();

		surfaceIterations = 0;
		if(surfaceIterations>=20)
		{
			return;
		}
		v_isVisible = true;
		nextLineDirection = Vector2.zero;
		v_VisLine = this.gameObject.GetComponent<LineRenderer>();
		testRay = Physics2D.Raycast(this.transform.position, Vector2.down, 100, terrainMask); 	
		if(testRay)
		{
			if(testRay.distance < 0.1f)
			{
				print("Navgenerator originated inside of terrain, disregarding.");
				return;
			}

			print("Hit surface!");
			surfaceNormal = testRay.normal;
			Vector2 surfacePara = Perp(surfaceNormal);

			if(surfacePara.x<0)
			{
				surfacePara *= -1; // Ensures that surfacePara always faces left.
			}

			if(surfaceNormal.y<0) // Ensures that upside down surfaces' surfacePara always face right. This means that the surfaces first node will be on the right, causing the steepness calculation to evaluate it as an upside down surface.
			{
				surfacePara *= -1;
			}

			Vector3[] positions = {testRay.point+surfaceNormal*0.1f, testRay.point+surfaceNormal*0.1f};

			RaycastHit2D obstacleFinderRight = Physics2D.Raycast(testRay.point+surfaceNormal*0.1f, surfacePara, 100, terrainMask); 	// horizontal ray to detect walls that would prematurely end the navsurface.

			int r;
			for(r = 1; r<1000; r++)
			{
				if(obstacleFinderRight && obstacleFinderRight.distance-0.05f<r*raycastDensity)
				{
					print(r+": Obstacle on right side, ending...");
					positions[1] = obstacleFinderRight.point-surfacePara*0.1f;
					break;
				}
				Vector3 newRayStartPos = (testRay.point+surfaceNormal*0.1f)+surfacePara*r*raycastDensity;
				RaycastHit2D newIncrementalRay = Physics2D.Raycast(newRayStartPos, -surfaceNormal, 0.2f, terrainMask); 	// 
				if(newIncrementalRay)
				{
					if(newIncrementalRay.normal==surfaceNormal&&newIncrementalRay.distance <= 0.15f)
					{
						positions[1] = newIncrementalRay.point+surfaceNormal*0.1f;
					}
					else
					{
						break;
					}
				}
				else
				{
					break;
				}
			}

			RaycastHit2D obstacleFinderLeft = Physics2D.Raycast(testRay.point+surfaceNormal*0.1f, -surfacePara, 100, terrainMask); 	// horizontal ray to detect walls that would prematurely end the navsurface.
			int l;
			for(l = -1; l>-1000; l--)// Testing left of origin
			{
				if(obstacleFinderLeft && obstacleFinderLeft.distance-0.05f<Mathf.Abs(l*raycastDensity))
				{
					print(l+": Obstacle on left side, ending...");
					positions[0] = obstacleFinderLeft.point+surfacePara*0.1f;
					break;
				}
				Vector3 newRayStartPos = (testRay.point+surfaceNormal*0.1f)+surfacePara*l*raycastDensity;
				RaycastHit2D newIncrementalRay = Physics2D.Raycast(newRayStartPos, -surfaceNormal, 0.2f, terrainMask); 	// Ground
				if(newIncrementalRay)
				{
					if(newIncrementalRay.normal==surfaceNormal&&newIncrementalRay.distance <= 0.15f)
					{
						positions[0] = newIncrementalRay.point+surfaceNormal*0.1f;
					}
					else
					{
						break;
					}
				}
				else
				{
					break;
				}
			}
			v_VisLine.SetPositions(positions);
			Vector2 midpoint = positions[0]+(positions[1]-positions[0])/2;
			if(r+Mathf.Abs(l)>=3) // If the tested surface is sufficiently wide, generate a new nav surface.
			{
				nextLineDirection = surfacePara*0.5f;
				nextLineOrigin = (Vector2)positions[1]-surfacePara*0.05f;
				exploredPointList.Add(midpoint);
				NavSurface newNavSurface = Instantiate(p_NavSurfPrefab, (positions[0]+positions[1])/2, Quaternion.identity).GetComponent<NavSurface>();
				newNavSurface.Initialize(positions[0], positions[1]);
			}
			else
				print("Surface too small!");
		}
		Vector3[] linePositions = {nextLineOrigin, nextLineOrigin+nextLineDirection};
		v_VisLine.SetPositions(linePositions);
	}
		


	// Update is called once per frame
	void Update () 
	{
		if(v_isVisible)
			v_VisLine.enabled = true;
		else
			v_VisLine.enabled = false;
	}

	#region Utility Functions
	public float Get2DAngle(Vector2 vector2, float degOffset) // Get angle, from -180 to +180 degrees. Degree offset shifts the origin from up, clockwise, by the amount of degrees specified. For example, 90 degrees shifts the origin to horizontal right.
	{
		float angle = Mathf.Atan2(vector2.x, vector2.y)*Mathf.Rad2Deg;
		angle = degOffset-angle;
		if(angle>180)
			angle = -360+angle;
		if(angle<-180)
			angle = 360+angle;
		return angle;
	}

	protected Vector2 Perp(Vector2 input) //Perpendicularizes the vector.
	{
		Vector2 output;
		output.x = input.y;
		output.y = -input.x;
		return output;
	}

	protected Vector2 Rotate(Vector2 v, float degrees)
	{
		float radians = degrees * Mathf.Deg2Rad;
		float sin = Mathf.Sin(radians);
		float cos = Mathf.Cos(radians);

		float tx = v.x;
		float ty = v.y;

		return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
	}

	protected Vector2 Proj(Vector2 A, Vector2 B) //Projects vector A onto vector B.
	{
		float component = Vector2.Dot(A,B)/B.magnitude;
		return component*B.normalized;
	}		
	#endregion
}
