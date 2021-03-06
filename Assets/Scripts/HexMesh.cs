﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {

	public bool useCollider;
	public bool useCellData;
	public bool useUVCoordinates, useUV2Coordinates;

	Mesh mesh;
	MeshCollider localCollider;
	[NonSerialized] List<Vector3> vertices;
	[NonSerialized] List<int> triangles;
	[NonSerialized] List<Color> cellWeights;
	[NonSerialized] List<Vector2> uvs, uv2s;
	[NonSerialized] List<Vector3> cellIndices;

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
		if (useCellData) {
			cellWeights = ListPool<Color>.Get ();
			cellIndices = ListPool<Vector3>.Get ();
		}
		if (useUVCoordinates)
			uvs = ListPool<Vector2>.Get ();
		if (useUV2Coordinates)
			uv2s = ListPool<Vector2>.Get ();
	}

	public void Apply () {
		mesh.SetVertices (vertices);
		ListPool<Vector3>.Add (vertices);
		mesh.SetTriangles (triangles, 0);
		ListPool<int>.Add (triangles);
		if (useCellData) {
			mesh.SetColors (cellWeights);
			ListPool<Color>.Add (cellWeights);
			mesh.SetUVs (2, cellIndices);
			ListPool<Vector3>.Add (cellIndices);
		}
		if (useUVCoordinates) {
			mesh.SetUVs (0, uvs);
			ListPool<Vector2>.Add (uvs);
		}
		if (useUV2Coordinates) {
			mesh.SetUVs (1, uv2s);
			ListPool<Vector2>.Add (uv2s);
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

	public void AddTriangleCellData (Vector3 indices, Color weights1, Color weights2, Color weights3) {
		cellIndices.Add (indices);
		cellIndices.Add (indices);
		cellIndices.Add (indices);
		cellWeights.Add (weights1);
		cellWeights.Add (weights2);
		cellWeights.Add (weights3);
	}

	public void AddTriangleCellData (Vector3 indices, Color weights) {
		AddTriangleCellData (indices, weights, weights, weights);
	}

	public void AddQuadCellData (Vector3 indices, Color weights1, Color weights2, Color weights3, Color weights4) {
		cellIndices.Add (indices);
		cellIndices.Add (indices);
		cellIndices.Add (indices);
		cellIndices.Add (indices);
		cellWeights.Add (weights1);
		cellWeights.Add (weights2);
		cellWeights.Add (weights3);
		cellWeights.Add (weights4);
	}

	public void AddQuadCellData (Vector3 indices, Color weights1, Color weights2) {
		AddQuadCellData (indices, weights1, weights1, weights2, weights2);
	}

	public void AddQuadCellData (Vector3 indices, Color weights) {
		AddQuadCellData (indices, weights, weights, weights, weights);
	}

	public void AddTriangleUV (Vector2 uv1, Vector2 uv2, Vector3 uv3) {
		uvs.Add (uv1);
		uvs.Add (uv2);
		uvs.Add (uv3);
	}

	public void AddTriangleUV2 (Vector2 uv1, Vector2 uv2, Vector3 uv3) {
		uv2s.Add (uv1);
		uv2s.Add (uv2);
		uv2s.Add (uv3);
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

	public void AddQuadUnperturbed (Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
		int vertexIndex = vertices.Count;
		vertices.Add (v1);
		vertices.Add (v2);
		vertices.Add (v3);
		vertices.Add (v4);
		triangles.Add (vertexIndex);
		triangles.Add (vertexIndex + 2);
		triangles.Add (vertexIndex + 1);
		triangles.Add (vertexIndex + 1);
		triangles.Add (vertexIndex + 2);
		triangles.Add (vertexIndex + 3);
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

	public void AddQuadUV2 (Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4) {
		uv2s.Add (uv1);
		uv2s.Add (uv2);
		uv2s.Add (uv3);
		uv2s.Add (uv4);
	}

	public void AddQuadUV2 (float uMin, float uMax, float vMin, float vMax) {
		uv2s.Add (new Vector2 (uMin, vMin));
		uv2s.Add (new Vector2 (uMax, vMin));
		uv2s.Add (new Vector2 (uMin, vMax));
		uv2s.Add (new Vector2 (uMax, vMax));
	}
}
