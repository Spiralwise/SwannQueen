using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

	public int width = 6;
	public int height = 6;

	public HexCell hexPrefab;
	public Color defaultColor = Color.white;
	public Color touchedColor = Color.magenta;

	public Text hexLabelPrefab;

	HexCell[] cells;
	Canvas canvas;
	HexMesh mesh;

	public void Awake () {
		canvas = GetComponentInChildren<Canvas> ();
		mesh = GetComponentInChildren<HexMesh> ();
		int i = 0;
		cells = new HexCell[height * width];
		for (int y = 0; y < height; y++)
			for (int x = 0; x < width; x++)
				CreateCell (x, y, i++);
	}

	public void Start () {
		mesh.Triangulate (cells);
	}

	void CreateCell (int x, int y, int i) {
		Vector3 position;
		position.x = (x + (y % 2) * 0.5f) * 2f * HexMetrics.innerRadius;
		position.y = 0f;
		position.z = y * 1.5f * HexMetrics.outterRadius;
		HexCell localCell = cells[i] = Instantiate<HexCell> (hexPrefab);
		localCell.coordinates = HexCoordinates.FromOffsetCoordinates (x, y);
		localCell.transform.SetParent (transform, false);
		localCell.transform.position = position;
		localCell.color = defaultColor;

		Text localLabel = Instantiate<Text> (hexLabelPrefab);
		localLabel.rectTransform.SetParent (canvas.transform, false);
		localLabel.rectTransform.anchoredPosition = new Vector2 (position.x, position.z);
		localLabel.text = localCell.coordinates.ToStringOnSeparateLines ();

		if (x > 0)
			localCell.SetNeighbor (HexDirection.W, cells [x - 1 + y * width]);
		if (y > 0) {
			if ((y % 2) == 1) {
				localCell.SetNeighbor (HexDirection.SW, cells [x + (y - 1) * width]);
				if (x < width - 1)
					localCell.SetNeighbor (HexDirection.SE, cells [x + 1 + (y - 1) * width]);
			} else {
				localCell.SetNeighbor (HexDirection.SE, cells [x + (y - 1) * width]);
				if (x > 0)
					localCell.SetNeighbor (HexDirection.SW, cells [x - 1 + (y - 1) * width]);
			}
		}

		localCell.uiRect = localLabel.rectTransform;
	}

	public HexCell GetCell (Vector3 position) {
		position = transform.InverseTransformPoint (position);
		HexCoordinates targetCoordinates = HexCoordinates.FromPosition (position);
		int index = targetCoordinates.X + targetCoordinates.Y / 2 + targetCoordinates.Y * width;
		return cells[index];
		//Debug.Log ("touched at " + targetCoordinates.toString ());
	}

	public void Refresh () {
		mesh.Triangulate (cells);
	}
}
