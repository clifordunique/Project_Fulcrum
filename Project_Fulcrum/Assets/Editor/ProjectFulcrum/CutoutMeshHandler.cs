using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

[CustomEditor(typeof(MeshFilter))]
public class CutoutMeshHandler : Editor {

	private Material cutoutShader2D;
	private Cutout2D myCutout2D;
	private SortingGroup mySortingGroup;
	private GameObject myGameObject;
	private MeshFilter myMeshFilter;
	private MeshRenderer myMeshRenderer;
	private Color myColor = Color.white;
	private int sortingOrder = 0;
	private string sortingLayer = "Foreground";

	void Awake() 
	{
		myMeshFilter = (MeshFilter)target;
		string filePath = "Assets/Materials/CutoutShader2D.mat";
		cutoutShader2D = (Material)AssetDatabase.LoadAssetAtPath(filePath, typeof(Material));
	}


	public override void OnInspectorGUI() 
	{
		DrawDefaultInspector();
		if(GUILayout.Button("Generate collider for 2D mesh"))
		{
			GeneratePolygonCollider2D();
		}
		EditorGUILayout.Space();
		myColor = EditorGUILayout.ColorField("Cutout Color", myColor);
		sortingOrder = EditorGUILayout.IntField("Sorting Order", sortingOrder);
		sortingLayer = EditorGUILayout.TextField("Sorting Layer", sortingLayer);

		if(GUILayout.Button("Apply OBJ -> Cutout2D conversion"))
		{
			Apply2DCutoutShader();
		}

	}

	public void Apply2DCutoutShader()
	{
		myGameObject = myMeshFilter.gameObject;
		myMeshRenderer = myGameObject.GetComponent<MeshRenderer>();
		myMeshRenderer.material = cutoutShader2D;

		MaterialPropertyBlock _propBlock;

		_propBlock = new MaterialPropertyBlock();
		_propBlock.SetColor("_Color", myColor);
		myMeshRenderer.SetPropertyBlock(_propBlock);

		//Configuring cutout2d component
		if(myGameObject.GetComponent<Cutout2D>())
		{
			myCutout2D = myGameObject.GetComponent<Cutout2D>();
			myCutout2D.color = myColor;
		}
		else
		{
			myCutout2D = myGameObject.AddComponent<Cutout2D>();
			myCutout2D.color = myColor;
		}

		//Configuring sortinglayer
		if(myGameObject.GetComponent<SortingGroup>())
		{
			mySortingGroup = myGameObject.GetComponent<SortingGroup>();
			mySortingGroup.sortingOrder = sortingOrder;
		}
		else
		{
			mySortingGroup = myGameObject.AddComponent<SortingGroup>();
			mySortingGroup.sortingOrder = sortingOrder;
		}

		if(sortingLayer!="")
		{
			mySortingGroup.sortingLayerName = sortingLayer;
		}

	}

	public void GeneratePolygonCollider2D()
	{
		// Stop if polygoncollider already exists.
		if (myMeshFilter.gameObject.GetComponent<PolygonCollider2D>() != null) 
		{
			return;
		}

		// Get triangles and vertices from mesh
		int[] triangles = myMeshFilter.sharedMesh.triangles;
		Vector3[] vertices = myMeshFilter.sharedMesh.vertices;

		// Get just the outer edges from the mesh's triangles (ignore or remove any shared edges)
		Dictionary<string, KeyValuePair<int, int>> edges = new Dictionary<string, KeyValuePair<int, int>>();
		for (int i = 0; i < triangles.Length; i += 3) {
			for (int e = 0; e < 3; e++) {
				int vert1 = triangles[i + e];
				int vert2 = triangles[i + e + 1 > i + 2 ? i : i + e + 1];
				string edge = Mathf.Min(vert1, vert2) + ":" + Mathf.Max(vert1, vert2);
				if (edges.ContainsKey(edge)) {
					edges.Remove(edge);
				} else {
					edges.Add(edge, new KeyValuePair<int, int>(vert1, vert2));
				}
			}
		}

		// Create edge lookup (Key is first vertex, Value is second vertex, of each edge)
		Dictionary<int, int> lookup = new Dictionary<int, int>();
		foreach (KeyValuePair<int, int> edge in edges.Values) {
			if (lookup.ContainsKey(edge.Key) == false) {
				lookup.Add(edge.Key, edge.Value);
			}
		}

		// Create empty polygon collider
		PolygonCollider2D polygonCollider = myMeshFilter.gameObject.AddComponent<PolygonCollider2D>();
		polygonCollider.pathCount = 0;

		// Loop through edge vertices in order
		int startVert = 0;
		int nextVert = startVert;
		int highestVert = startVert;
		List<Vector2> colliderPath = new List<Vector2>();
		while (true) {

			// Add vertex to collider path
			colliderPath.Add(vertices[nextVert]);

			// Get next vertex
			nextVert = lookup[nextVert];

			// Store highest vertex (to know what shape to move to next)
			if (nextVert > highestVert) {
				highestVert = nextVert;
			}

			// Shape complete
			if (nextVert == startVert) {

				// Add path to polygon collider
				polygonCollider.pathCount++;
				polygonCollider.SetPath(polygonCollider.pathCount - 1, colliderPath.ToArray());
				colliderPath.Clear();

				// Go to next shape if one exists
				if (lookup.ContainsKey(highestVert + 1)) {

					// Set starting and next vertices
					startVert = highestVert + 1;
					nextVert = startVert;

					// Continue to next loop
					continue;
				}

				// No more verts
				break;
			}
		}
	}

}

//EditorUtility.SetDirty(myGrassColourer);