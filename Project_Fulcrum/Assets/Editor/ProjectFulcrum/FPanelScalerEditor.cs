using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
[CanEditMultipleObjects]
[CustomEditor(typeof(FulcrumPanelScaler), true)]
public class FPanelScalerEditor : Editor
{

	private FulcrumPanelScaler PS;

	void Awake()
	{
		PS = (FulcrumPanelScaler)target;
	}

	public override void OnInspectorGUI()
	{
		if (GUILayout.Button("Set offsets (ONLY DO THIS IF EDITOR IS IN 1920x1080 MODE)"))
		{
			RectTransform myRectTransform = PS.GetComponent<RectTransform>();

			PS.unscaledOffsetTop = (int)Mathf.Abs(myRectTransform.offsetMax.y);
			PS.unscaledOffsetBot = (int)Mathf.Abs(myRectTransform.offsetMin.y);
			PS.unscaledOffsetLeft = (int)Mathf.Abs(myRectTransform.offsetMin.x);
			PS.unscaledOffsetRight = (int)Mathf.Abs(myRectTransform.offsetMax.x);
		}

		if (GUILayout.Button("Convert pixel offsets to percent offsets"))
		{
			RectTransform myRectTransform = PS.GetComponent<RectTransform>();

			PS.UpdateResolution();

			myRectTransform.anchorMax = new Vector2(1.0f-PS.percentOffsetRight, 1.0f-PS.percentOffsetTop);
			myRectTransform.anchorMin = new Vector2(PS.percentOffsetLeft, PS.percentOffsetBot);

			myRectTransform.offsetMax = new Vector2(0, 0);
			myRectTransform.offsetMin = new Vector2(0, 0);

		}

		if (GUILayout.Button("Revert to original offsets."))
		{
			RectTransform myRectTransform = PS.GetComponent<RectTransform>();

			myRectTransform.anchorMax = new Vector2(1, 1);
			myRectTransform.anchorMin = new Vector2(0, 0);

			myRectTransform.offsetMax = new Vector2(PS.unscaledOffsetRight, PS.unscaledOffsetTop);
			myRectTransform.offsetMin = new Vector2(PS.unscaledOffsetLeft, PS.unscaledOffsetBot);
		}

		if (GUILayout.Button("Make the magic happen (Part A)"))
		{
			RectTransform myRectTransform = PS.GetComponent<RectTransform>();

			Vector3[] v = new Vector3[4];
			myRectTransform.GetLocalCorners(v);

			float width = v[2].x - v[0].x;
			float height = v[2].y - v[0].y;

			PS.myPixelWidth = width;
			PS.myPixelHeight = height;

			Debug.Log("width: " + width + ", height: " + height);

			PS.myParentTransform = PS.myRectTransform.parent.GetComponent<RectTransform>();
			PS.myParent = PS.myParentTransform.GetComponent<FulcrumPanelScaler>();

			if (PS.myParent != null)
			{
				PS.parentPixelHeight = PS.myParent.myPixelHeight;
				PS.parentPixelWidth = PS.myParent.myPixelWidth;
			}


			PS.localAnchorOffsetBot = PS.unscaledOffsetBot / PS.parentPixelHeight;
			PS.localAnchorOffsetTop= PS.unscaledOffsetTop / PS.parentPixelHeight;

			PS.localAnchorOffsetLeft = PS.unscaledOffsetLeft / PS.parentPixelWidth;
			PS.localAnchorOffsetRight= PS.unscaledOffsetRight / PS.parentPixelWidth;



			//myRectTransform.anchorMax = new Vector2(1.0f - PS.percentOffsetRight, 1.0f - PS.percentOffsetTop);
			//myRectTransform.anchorMin = new Vector2(PS.percentOffsetLeft, PS.percentOffsetBot);

			//myRectTransform.offsetMax = new Vector2(0, 0);
			//myRectTransform.offsetMin = new Vector2(0, 0);

		}


		if (GUILayout.Button("Make the magic happen (Part B )"))
		{
			RectTransform myRectTransform = PS.GetComponent<RectTransform>();

			myRectTransform.anchorMax = new Vector2(1.0f - PS.localAnchorOffsetRight, 1.0f - PS.localAnchorOffsetTop);
			myRectTransform.anchorMin = new Vector2(PS.localAnchorOffsetLeft, PS.localAnchorOffsetBot);

			myRectTransform.offsetMax = new Vector2(0, 0);
			myRectTransform.offsetMin = new Vector2(0, 0);
			PS.percentScaling = true;
		}

		DrawDefaultInspector();
	}

	//	public void DefaultMovementVars()
	//	{
	//		myFighterChar.m = new MovementVars();
	//	}
	//

}
