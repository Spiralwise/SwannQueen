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
	public HexUnit unitPrefab;

	public Text hexLabelPrefab;

	public Material terrainMaterial;

	int chunkCountX, chunkCountY;
	HexCell[] cells;
	HexGridChunk[] chunks;
	List<HexUnit> units = new List<HexUnit> ();

	HexCell currentPathFrom, currentPathTo;
	bool currentPathExists;
	HexCellPriorityQueue searchFrontier;
	int searchFrontierPhase;

	HexCellShaderData cellShaderData;

	public void Awake () {
		terrainMaterial.DisableKeyword ("GRID_ON");
		HexMetrics.noiseSource = noiseSource;
		HexMetrics.InitializeHashGrid (seed);
		HexUnit.unitPrefab = unitPrefab;
		cellShaderData = gameObject.AddComponent<HexCellShaderData> ();
		cellShaderData.Grid = this;
		CreateMap (cellCountX, cellCountY);
	}

	void OnEnable () {
		if (!HexMetrics.noiseSource) {
			HexMetrics.noiseSource = noiseSource;
			HexMetrics.InitializeHashGrid (seed);
			HexUnit.unitPrefab = unitPrefab;
			ResetVisibility ();
		}
	}

	public bool HasPath {
		get {
			return currentPathExists;
		}
	}

	public bool CreateMap (int x, int y) {
		ClearPath ();
		ClearUnits ();
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
		cellShaderData.Initialize (x, y);
		CreateChunks ();
		CreateCells ();
		return true;
	}

	public void Save (BinaryWriter writer) {
		writer.Write (cellCountX);
		writer.Write (cellCountY);
		for (int c = 0; c < cells.Length; c++)
			cells [c].Save (writer);
		writer.Write (units.Count);
		for (int u = 0; u < units.Count; u++)
			units [u].Save (writer);
	}

	public void Load (BinaryReader reader, int header) {
		ClearPath ();
		ClearUnits ();
		int x = 20, y = 15;
		if (header >= 1) {
			x = reader.ReadInt32 ();
			y = reader.ReadInt32 ();
		}
		if ((x != cellCountX || y != cellCountY) && !CreateMap (x, y))
			return;

		bool originalImmediateMode = cellShaderData.ImmediateMode;
		cellShaderData.ImmediateMode = true;

		for (int c = 0; c < cells.Length; c++)
			cells [c].Load (reader, header);
		for (int c = 0; c < chunks.Length; c++)
			chunks [c].Refresh ();

		if (header >= 2) {
			int unitCount = reader.ReadInt32 ();
			for (int i = 0; i < unitCount; i++)
				HexUnit.Load (reader, this);
		}

		cellShaderData.ImmediateMode = originalImmediateMode;
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
		localCell.Index = i;
		localCell.coordinates = HexCoordinates.FromOffsetCoordinates (x, y);
		localCell.ShaderData = cellShaderData;
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

	public HexCell GetCell (Ray ray) {
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit))
			return GetCell (hit.point);
		return null;
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

	public void AddUnit (HexUnit unit, HexCell location, float orientation) {
		units.Add (unit);
		unit.Grid = this;
		unit.transform.SetParent (transform, false);
		unit.Location = location;
		unit.Orientation = orientation;
	}

	public void RemoveUnit (HexUnit unit) {
		units.Remove (unit);
		unit.Die ();
	}

	void ClearUnits () {
		for (int i = 0; i < units.Count; i++)
			units [i].Die ();
		units.Clear ();
	}

	public void FindPath (HexCell fromCell, HexCell toCell, HexUnit unit) {
		ClearPath ();
		currentPathFrom = fromCell;
		currentPathTo = toCell;
		currentPathExists = Search (fromCell, toCell, unit);
		ShowPath (unit.Speed);
	}

	public List<HexCell> GetPath () {
		if (!currentPathExists)
			return null;
		List<HexCell> path = ListPool<HexCell>.Get ();
		for (HexCell c = currentPathTo; c != currentPathFrom; c = c.PathFrom)
			path.Add (c);
		path.Add (currentPathFrom);
		path.Reverse ();
		return path;
	}

	void ShowPath (int speed) {
		if (currentPathExists) {
			HexCell current = currentPathTo;
			while (current != currentPathFrom) {
				int turn = (current.Distance - 1) / speed;
				current.SetLabel (turn.ToString ());
				current.EnableOutline (Color.white);
				current = current.PathFrom;
			}
			currentPathFrom.EnableOutline (Color.blue);
			currentPathTo.EnableOutline (Color.red);
		}
	}

	public void ClearPath () {
		if (currentPathExists) {
			HexCell current = currentPathTo;
			while (current != currentPathFrom) {
				current.SetLabel (null);
				current.DisableOutline ();
				current = current.PathFrom;
			}
			current.DisableOutline ();
			currentPathExists = false;
		}
		currentPathFrom = currentPathTo = null;
	}

	bool Search (HexCell fromCell, HexCell toCell, HexUnit unit) {
		int speed = unit.Speed;
		searchFrontierPhase += 2;
		if (searchFrontier == null)
			searchFrontier = new HexCellPriorityQueue ();
		else
			searchFrontier.Clear ();
		fromCell.Distance = 0;
		fromCell.SearchPhase = searchFrontierPhase;
		searchFrontier.Enqueue (fromCell);
		while (searchFrontier.Count > 0) {
			HexCell current = searchFrontier.Dequeue ();
			current.SearchPhase += 1;
			if (current == toCell)
				return true;
			int currentTurn = (current.Distance - 1) / speed;
			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
				HexCell neighbor = current.GetNeighbor (d);
				if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase)
					continue;
				if (!unit.IsValidDestination (neighbor))
					continue;
				int moveCost = unit.GetMoveCost (current, neighbor, d);
				if (moveCost < 0)
					continue;
				int distance = current.Distance + moveCost;
				int turn = (distance - 1) / speed;
				if (turn > currentTurn)
					distance = turn * speed + moveCost;
				if (neighbor.SearchPhase < searchFrontierPhase) {
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					neighbor.SearchHeuristic = neighbor.coordinates.DistanceTo (toCell.coordinates);
					neighbor.SearchPhase = searchFrontierPhase;
					searchFrontier.Enqueue (neighbor);
				} else if (distance < neighbor.Distance) {
					int oldPriority = neighbor.SearchPriority;
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					searchFrontier.Change (neighbor, oldPriority);
				}
			} // TODO (2017-10-01) How about rivers?
		}
		return false;
	}
		
	List<HexCell> GetVisibleCells (HexCell fromCell, int range) {
		List<HexCell> visibleCells = ListPool<HexCell>.Get ();

		searchFrontierPhase += 2;
		if (searchFrontier == null)
			searchFrontier = new HexCellPriorityQueue ();
		else
			searchFrontier.Clear ();
		fromCell.Distance = 0;
		range += fromCell.ViewElevation;
		fromCell.SearchPhase = searchFrontierPhase;
		searchFrontier.Enqueue (fromCell);
		HexCoordinates fromCoordinates = fromCell.coordinates;
		while (searchFrontier.Count > 0) {
			HexCell current = searchFrontier.Dequeue ();
			current.SearchPhase += 1;
			visibleCells.Add (current);
			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
				HexCell neighbor = current.GetNeighbor (d);
				if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase)
					continue;

				int distance = current.Distance + 1;
				if (distance + neighbor.ViewElevation > range
					|| distance > fromCoordinates.DistanceTo(neighbor.coordinates))
					continue;
				if (neighbor.SearchPhase < searchFrontierPhase) {
					neighbor.Distance = distance;
					neighbor.SearchHeuristic = 0;
					neighbor.SearchPhase = searchFrontierPhase;
					searchFrontier.Enqueue (neighbor);
				} else if (distance < neighbor.Distance) {
					int oldPriority = neighbor.SearchPriority;
					neighbor.Distance = distance;
					searchFrontier.Change (neighbor, oldPriority);
				}
			}
		}
		return visibleCells;
	}

	public void IncreaseVisibility (HexCell fromCell, int range) {
		List<HexCell> cells = GetVisibleCells (fromCell, range);
		for (int i = 0; i < cells.Count; i++)
			cells [i].IncreaseVisibility ();
		ListPool<HexCell>.Add (cells);
	}

	public void DecreaseVisibility (HexCell fromCell, int range) {
		List<HexCell> cells = GetVisibleCells (fromCell, range);
		for (int i = 0; i < cells.Count; i++)
			cells [i].DecreaseVisibility ();
		ListPool<HexCell>.Add (cells);
	}

	public void ResetVisibility () {
		for (int i = 0; i < cells.Length; i++)
			cells [i].ResetVisibility ();
		for (int i = 0; i < units.Count; i++) {
			HexUnit unit = units [i];
			IncreaseVisibility (unit.Location, unit.VisionRange);
		}
	}

	public void ShowUI (bool visible) {
		for (int i = 0; i < chunks.Length; i++)
			chunks [i].ShowUI (visible);
	}
}
