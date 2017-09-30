using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

//	public int chunkCountX = 4;
//	public int chunkCountY = 3;
	public int cellCountX = 20;
	public int cellCountY = 15;
	public HexCell hexPrefab;
	public HexGridChunk chunkPrefab;
	public Color[] colors;
	public Texture2D noiseSource;
	public int seed;

	public Text hexLabelPrefab;

	int chunkCountX, chunkCountY;
	HexCell[] cells;
	HexGridChunk[] chunks;

	public void Awake () {
		HexMetrics.noiseSource = noiseSource;
		HexMetrics.InitializeHashGrid (seed);
		HexMetrics.colors = colors;

		CreateMap (cellCountX, cellCountY);
	}

	void OnEnable () {
		if (!HexMetrics.noiseSource) {
			HexMetrics.noiseSource = noiseSource;
			HexMetrics.InitializeHashGrid (seed);
			HexMetrics.colors = colors;
		}
	}

	public bool CreateMap (int x, int y) {
		Debug.Log ("Who you gonna call?");
		if (x <= 0 || x % HexMetrics.chunkSizeX != 0
		    || y <= 0 || y % HexMetrics.chunkSizeY != 0) {
			Debug.LogError ("Can't create a new map: Unsupported map size.");
			return false;
		}
		if (chunks != null)
			for (int i = 0; i < chunks.Length; i++)
				Destroy (chunks [i].gameObject);
		cellCountX = x;
		cellCountY = y;
		chunkCountX = cellCountX / HexMetrics.chunkSizeX;
		chunkCountY = cellCountY / HexMetrics.chunkSizeY;
		CreateChunks ();
		CreateCells ();
		return true;
	}

	public void Save (BinaryWriter writer) {
		writer.Write (cellCountX);
		writer.Write (cellCountY);
		for (int c = 0; c < cells.Length; c++)
			cells [c].Save (writer);
	}

	public void Load (BinaryReader reader, int header) {
		int x = 20, y = 15;
		if (header >= 1) {
			x = reader.ReadInt32 ();
			y = reader.ReadInt32 ();
		}
		if ((x != cellCountX || y != cellCountY) && !CreateMap (x, y))
			return;
		
		for (int c = 0; c < cells.Length; c++)
			cells [c].Load (reader);
		for (int c = 0; c < chunks.Length; c++)
			chunks [c].Refresh ();
	}

	void CreateChunks () {
		chunks = new HexGridChunk[chunkCountX * chunkCountY];
		for (int y = 0, i = 0; y < chunkCountY; y++)
			for (int x = 0; x < chunkCountX; x++) {
				HexGridChunk chunk = chunks [i++] = Instantiate (chunkPrefab);
				chunk.transform.SetParent (transform);
			}
	}

	void AddCellToChunk (int x, int y, HexCell cell) {
		int chunkX = x / HexMetrics.chunkSizeX;
		int chunkY = y / HexMetrics.chunkSizeY;
		HexGridChunk chunk = chunks [chunkX + chunkY * chunkCountX];

		int localX = x - chunkX * HexMetrics.chunkSizeX;
		int localY = y - chunkY * HexMetrics.chunkSizeY;
		chunk.AddCell (localX + localY * HexMetrics.chunkSizeX, cell);
	}

	void CreateCells () {
		int i = 0;
		cells = new HexCell[cellCountY * cellCountX];
		for (int y = 0; y < cellCountY; y++)
			for (int x = 0; x < cellCountX; x++)
				CreateCell (x, y, i++);
	}

	void CreateCell (int x, int y, int i) {
		Vector3 position;
		position.x = (x + (y % 2) * 0.5f) * 2f * HexMetrics.innerRadius;
		position.y = 0f;
		position.z = y * 1.5f * HexMetrics.outterRadius;
		HexCell localCell = cells[i] = Instantiate<HexCell> (hexPrefab);
		localCell.coordinates = HexCoordinates.FromOffsetCoordinates (x, y);
		localCell.transform.position = position;

		Text localLabel = Instantiate<Text> (hexLabelPrefab);
		localLabel.rectTransform.anchoredPosition = new Vector2 (position.x, position.z);
		localLabel.text = localCell.coordinates.ToStringOnSeparateLines ();

		if (x > 0)
			localCell.SetNeighbor (HexDirection.W, cells [x - 1 + y * cellCountX]);
		if (y > 0) {
			if ((y % 2) == 1) {
				localCell.SetNeighbor (HexDirection.SW, cells [x + (y - 1) * cellCountX]);
				if (x < cellCountX - 1)
					localCell.SetNeighbor (HexDirection.SE, cells [x + 1 + (y - 1) * cellCountX]);
			} else {
				localCell.SetNeighbor (HexDirection.SE, cells [x + (y - 1) * cellCountX]);
				if (x > 0)
					localCell.SetNeighbor (HexDirection.SW, cells [x - 1 + (y - 1) * cellCountX]);
			}
		}

		localCell.uiRect = localLabel.rectTransform;
		localCell.Elevation = 0;

		AddCellToChunk (x, y, localCell);
	}

	public HexCell GetCell (Vector3 position) {
		position = transform.InverseTransformPoint (position);
		HexCoordinates targetCoordinates = HexCoordinates.FromPosition (position);
		int index = targetCoordinates.X + targetCoordinates.Y / 2 + targetCoordinates.Y * cellCountX;
		return cells[index];
		//Debug.Log ("touched at " + targetCoordinates.toString ());
	}

	public HexCell GetCell (HexCoordinates coordinates) {
		int y = coordinates.Y;
		if (y < 0 || y >= cellCountY)
			return null;
		int x = coordinates.X + y / 2;
		if (x < 0 || x >= cellCountX)
			return null;
		return cells [x + y * cellCountX];
	}

	public void ShowUI (bool visible) {
		for (int i = 0; i < chunks.Length; i++)
			chunks [i].ShowUI (visible);
	}
}
