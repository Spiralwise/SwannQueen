using UnityEngine;
using UnityEngine.UI;

public class HexGridChunk : MonoBehaviour {

	HexCell[] cells;

	HexMesh mesh;
	Canvas canvas;

	void Awake () {
		canvas = GetComponentInChildren<Canvas> ();
		mesh = GetComponentInChildren<HexMesh> ();

		cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeY];
		ShowUI (false);
	}

	public void AddCell (int index, HexCell cell) {
		cells [index] = cell;
		cell.chunk = this;
		cell.transform.SetParent (transform, false);
		cell.uiRect.SetParent (canvas.transform, false);
	}

	public void Refresh () {
		enabled = true;
	}

	public void ShowUI (bool visible) {
		canvas.gameObject.SetActive (visible);
	}

	void LateUpdate () {
		mesh.Triangulate (cells);
		enabled = false;
	}
}
