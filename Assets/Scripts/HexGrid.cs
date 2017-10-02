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
	public Texture2D noiseSource;
	public int seed;

	public Text hexLabelPrefab;

	public Material terrainMaterial;

	int chunkCountX, chunkCountY;
	HexCell[] cells;
	HexGridChunk[] chunks;

	public void Awake () {
		terrainMaterial.DisableKeyword ("GRID_ON");
		HexMetrics.noiseSource = noiseSource;
		HexMetrics.InitializeHashGrid (seed);

		CreateMap (cellCountX, cellCountY);
	}

	void OnEnable () {
		if (!HexMetrics.noiseSource) {
			HexMetrics.noiseSource = noiseSource;
			HexMetrics.InitializeHashGrid (seed);
		}
	}

	public bool CreateMap (int x, int y) {
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
		StopAllCoroutines ();
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

	public void ShowGrid (bool visible) {
		if (visible)
			terrainMaterial.EnableKeyword ("GRID_ON");
		else
			terrainMaterial.DisableKeyword ("GRID_ON");
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

	public void FindPath (HexCell fromCell, HexCell toCell) {
		StopAllCoroutines ();
		StartCoroutine (Search (fromCell, toCell));
	}

	IEnumerator Search (HexCell fromCell, HexCell toCell) {
		for (int i = 0; i < cells.Length; i++) {
			cells [i].Distance = int.MaxValue;
			cells [i].DisableOutline ();
		}
		fromCell.EnableOutline (Color.blue);
		toCell.EnableOutline (Color.red);
		WaitForSeconds delay = new WaitForSeconds (1 / 60f);
		List<HexCell> openSet = new List<HexCell> ();
		fromCell.Distance = 0;
		openSet.Add (fromCell);
		while (openSet.Count > 0) {
			yield return delay;
			HexCell current = openSet [0];
			openSet.RemoveAt (0);
			if (current == toCell) {
				current = current.PathFrom;
				while (current != fromCell) {
					current.EnableOutline (Color.white);
					current = current.PathFrom;
				}
				break;
			}
			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
				HexCell neighbor = current.GetNeighbor (d);
				if (neighbor == null)
					continue;
				if (neighbor.IsUnderWater)
					continue;
				HexEdgeType edgeType = current.GetEdgeType (neighbor);
				if (edgeType == HexEdgeType.Cliff)
					continue;

				int distance = current.Distance;
				if (current.HasRoadThroughEdge (d))
					distance += 1;
				else if (current.Walled != neighbor.Walled)
					continue;
				else {
					distance += edgeType == HexEdgeType.Flat ? 5 : 10;
					distance += neighbor.UrbanLevel + neighbor.FarmLevel + neighbor.PlantLevel;
				}
				
				if (neighbor.Distance == int.MaxValue) {
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					openSet.Add (neighbor);
				} else if (distance < neighbor.Distance) {
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
				}
				openSet.Sort ((x, y) => x.Distance.CompareTo (y.Distance));
			} // TODO (2017-10-01) How about rivers?
		}
	}

	public void ShowUI (bool visible) {
		for (int i = 0; i < chunks.Length; i++)
			chunks [i].ShowUI (visible);
	}
}
