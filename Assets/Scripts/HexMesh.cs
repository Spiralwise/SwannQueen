using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {

	public bool useCollider;
	public bool useColors;
	public bool useUVCoordinates;

	Mesh mesh;
	MeshCollider localCollider;
	[NonSerialized] List<Vector3> vertices;
	[NonSerialized] List<int> triangles;
	[NonSerialized] List<Color> colors;
	[NonSerialized] List<Vector2> uvs;

	public void Awake () {
		GetComponent<MeshFilter> ().mesh = mesh = new Mesh ();
		if (useCollider)
			localCollider = gameObject.AddComponent<MeshCollider> ();
		mesh.name = "Hex Mesh";
	}

	public void Clear () {
		mesh.Clear ();
		vertices = ListPool<Vector3>.Get ();
		triangles = ListPool<int>.Get ();
		if (useColors)
			colors = ListPool<Color>.Get ();;
		if (useUVCoordinates)
			uvs = ListPool<Vector2>.Get ();
	}

	public void Apply () {
		mesh.SetVertices (vertices);
		ListPool<Vector3>.Add (vertices);
		mesh.SetTriangles (triangles, 0);
		ListPool<int>.Add (triangles);
		if (useColors) {
			mesh.SetColors (colors);
			ListPool<Color>.Add (colors);
		}
		if (useUVCoordinates) {
			mesh.SetUVs (0, uvs);
			ListPool<Vector2>.Add (uvs);
		}
		mesh.RecalculateNormals ();
		if (useCollider)
			localCollider.sharedMesh = mesh;
	}
		
	public void AddTriangle (Vector3 a, Vector3 b, Vector3 c) {
		int index = vertices.Count;
		vertices.Add (HexMetrics.Perturb (a));
		vertices.Add (HexMetrics.Perturb (b));
		vertices.Add (HexMetrics.Perturb (c));
		triangles.Add (index);
		triangles.Add (index + 1);
		triangles.Add (index + 2);
	}

	public void AddTriangleUnperturbed (Vector3 a, Vector3 b, Vector3 c) {
		int index = vertices.Count;
		vertices.Add (a);
		vertices.Add (b);
		vertices.Add (c);
		triangles.Add (index);
		triangles.Add (index + 1);
		triangles.Add (index + 2);
	}

	public void AddTriangleColor (Color c1, Color c2, Color c3) {
		colors.Add (c1);
		colors.Add (c2);
		colors.Add (c3);
	}

	public void AddTriangleColor (Color c) {
		for (int i = 0; i < 3; i++)
			colors.Add (c);
	}

	public void AddTriangleUV (Vector2 uv1, Vector2 uv2, Vector3 uv3) {
		uvs.Add (uv1);
		uvs.Add (uv2);
		uvs.Add (uv3);
	}

	public void AddQuad (Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
		int index = vertices.Count;
		vertices.Add (HexMetrics.Perturb (v1));
		vertices.Add (HexMetrics.Perturb (v2));
		vertices.Add (HexMetrics.Perturb (v3));
		vertices.Add (HexMetrics.Perturb (v4));
		triangles.Add (index);
		triangles.Add (index + 2);
		triangles.Add (index + 1);
		triangles.Add (index + 1);
		triangles.Add (index + 2);
		triangles.Add (index + 3);
	}

	public void AddQuadColor (Color c1, Color c2, Color c3, Color c4) {
		colors.Add (c1);
		colors.Add (c2);
		colors.Add (c3);
		colors.Add (c4);
	}

	public void AddQuadColor (Color c1, Color c2) {
		colors.Add (c1);
		colors.Add (c1);
		colors.Add (c2);
		colors.Add (c2);
	}

	public void AddQuadColor (Color c) {
		for (int i = 0; i < 4; i++)
			colors.Add (c);
	}

	public void AddQuadUV (Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4) {
		uvs.Add (uv1);
		uvs.Add (uv2);
		uvs.Add (uv3);
		uvs.Add (uv4);
	}

	public void AddQuadUV (float uMin, float uMax, float vMin, float vMax) {
		uvs.Add (new Vector2 (uMin, vMin));
		uvs.Add (new Vector2 (uMax, vMin));
		uvs.Add (new Vector2 (uMin, vMax));
		uvs.Add (new Vector2 (uMax, vMax));
	}
}
