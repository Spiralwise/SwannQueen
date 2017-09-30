using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HexCell : MonoBehaviour {

	public HexGridChunk chunk;
	public HexCoordinates coordinates;
	public RectTransform uiRect;

	int elevation = int.MinValue;
	int urbanLevel, farmLevel, plantLevel;
	int specialIndex;
	bool walled;
	int terrainTypeIndex;
	bool hasIncomingRiver, hasOutgoingRiver;
	HexDirection incomingRiver, outgoingRiver;
	int waterLevel;

	[SerializeField]
	HexCell[] neighbors;

	[SerializeField]
	bool[] roads;

	public void Save (BinaryWriter writer) {
		writer.Write (terrainTypeIndex);
		writer.Write (elevation);
		writer.Write (waterLevel);
		writer.Write (urbanLevel);
		writer.Write (farmLevel);
		writer.Write (plantLevel);
		writer.Write (specialIndex);
		writer.Write (walled);
		writer.Write (hasIncomingRiver);
		writer.Write ((int)incomingRiver);
		writer.Write (hasOutgoingRiver);
		writer.Write ((int)outgoingRiver);
		for (int i = 0; i < roads.Length; i++)
			writer.Write (roads [i]);
	}

	public void Load (BinaryReader reader) {
		terrainTypeIndex = reader.ReadInt32 ();
		elevation = reader.ReadInt32 ();
		RefreshPosition ();
		waterLevel = reader.ReadInt32 ();
		urbanLevel = reader.ReadInt32 ();
		farmLevel = reader.ReadInt32 ();
		plantLevel = reader.ReadInt32 ();
		specialIndex = reader.ReadInt32 ();
		walled = reader.ReadBoolean ();
		hasIncomingRiver = reader.ReadBoolean ();
		incomingRiver = (HexDirection)reader.ReadInt32 ();
		hasOutgoingRiver = reader.ReadBoolean ();
		outgoingRiver = (HexDirection)reader.ReadInt32 ();
		for (int i = 0; i < roads.Length; i++)
			roads [i] = reader.ReadBoolean ();
	}

	public int Elevation {
		get {
			return elevation;
		}
		set {
			if (elevation == value)
				return;
			elevation = value;
			RefreshPosition ();
			Refresh ();
		}
	}

	public int UrbanLevel {
		get {
			return urbanLevel;
		}
		set {
			if (urbanLevel != value) {
				urbanLevel = value;
				RefreshSelf ();
			}
		}
	}

	public int FarmLevel {
		get {
			return farmLevel;
		}
		set {
			if (farmLevel != value) {
				farmLevel = value;
				RefreshSelf ();
			}
		}
	}

	public int PlantLevel {
		get {
			return plantLevel;
		}
		set {
			if (plantLevel != value) {
				plantLevel = value;
				RefreshSelf ();
			}
		}
	}

	public int SpecialIndex {
		get {
			return specialIndex;
		}
		set {
			if (specialIndex != value && !HasRiver) {
				specialIndex = value;
				RemoveRoads ();
				RefreshSelf ();
			}
		}
	}

	public bool IsSpecial {
		get {
			return specialIndex > 0;
		}
	}

	public bool Walled {
		get {
			return walled;
		}
		set {
			if (walled != value) {
				walled = value;
				Refresh ();
			}
		}
	}

	public int TerrainTypeIndex {
		get {
			return terrainTypeIndex;
		}
		set {
			if (terrainTypeIndex != value) {
				terrainTypeIndex = value;
				Refresh ();
			}
		}
	}

	public Color Color {
		get {
			return HexMetrics.colors[terrainTypeIndex];
		}
	}

	public Vector3 Position {
		get {
			return transform.localPosition;
		}
	}

	public bool HasRiver {
		get {
			return hasIncomingRiver || hasOutgoingRiver;
		}
	}

	public bool HasIncomingRiver {
		get {
			return hasIncomingRiver;
		}
	}

	public bool HasOutgoingRiver {
		get {
			return hasOutgoingRiver;
		}
	}

	public bool HasRiverBeginOrEnd {
		get {
			return hasIncomingRiver != hasOutgoingRiver;
		}
	}

	public HexDirection IncomingRiver {
		get {
			return incomingRiver;
		}
	}

	public HexDirection OutgoingRiver {
		get {
			return outgoingRiver;
		}
	}

	public float StreamBedY {
		get {
			return (elevation + HexMetrics.streamBedElevationOffset) * HexMetrics.elevationStep;
		}
	}

	public float RiverSurfaceY {
		get {
			return (elevation + HexMetrics.waterElevationOffset) * HexMetrics.elevationStep;
		}
	}

	public bool HasRoads {
		get {
			for (int i = 0; i < roads.Length; i++)
				if (roads [i])
					return true;
			return false;
		}
	}

	public int WaterLevel {
		get {
			return waterLevel;
		}
		set {
			if (waterLevel == value)
				return;
			waterLevel = value;
			ValidateRivers ();
			Refresh ();
		}
	}

	public bool IsUnderWater {
		get {
			return waterLevel > elevation;
		}
	}

	public float WaterSurfaceY {
		get {
			return (waterLevel + HexMetrics.waterElevationOffset) * HexMetrics.elevationStep;
		}
	}

	public int GetElevationDifference (HexDirection direction) {
		int difference = elevation - GetNeighbor (direction).elevation;
		return difference >= 0 ? difference : -difference;
	}

	public HexCell GetNeighbor (HexDirection direction) {
		return neighbors [(int)direction];
	}

	public void SetNeighbor (HexDirection direction, HexCell cell) {
		neighbors [(int)direction] = cell;
		cell.neighbors [(int)direction.Opposite ()] = this;
	}

	public HexEdgeType GetEdgeType (HexDirection direction) {
		return HexMetrics.GetEdgeType (elevation, neighbors [(int)direction].elevation);
	}

	public HexEdgeType GetEdgeType (HexCell thatCell) {
		return HexMetrics.GetEdgeType (elevation, thatCell.elevation);
	}

	public bool HasRiverThroughEdge (HexDirection direction) {
		return hasIncomingRiver && incomingRiver == direction || hasOutgoingRiver && OutgoingRiver == direction;
	}

	public HexDirection RiverBeginOrEndDirection {
		get {
			return hasIncomingRiver ? incomingRiver : outgoingRiver;
		}
	}

	bool IsValidRiverDestination (HexCell neighbor) {
		return neighbor && (elevation >= neighbor.elevation || waterLevel == neighbor.elevation);
	}

	void ValidateRivers () {
		if (hasOutgoingRiver && !IsValidRiverDestination (GetNeighbor (outgoingRiver)))
			RemoveOutgoingRiver ();
		if (hasIncomingRiver && !GetNeighbor (incomingRiver).IsValidRiverDestination (this))
			RemoveIncomingRiver ();
	}

	public bool HasRoadThroughEdge (HexDirection direction) {
		return roads [(int)direction];
	}

	public void SetOutgoingRiver (HexDirection direction) {
		if (HasOutgoingRiver && OutgoingRiver == direction)
			return;
		
		HexCell neighbor = GetNeighbor (direction);
		if (!IsValidRiverDestination(neighbor))
			return;

		RemoveOutgoingRiver ();
		if (hasIncomingRiver && incomingRiver == direction)
			RemoveIncomingRiver ();

		hasOutgoingRiver = true;
		outgoingRiver = direction;
		specialIndex = 0;

		neighbor.RemoveIncomingRiver ();
		neighbor.hasIncomingRiver = true;
		neighbor.incomingRiver = direction.Opposite ();
		neighbor.specialIndex = 0;

		SetRoad ((int)direction, false);
	}

	void SetRoad (int index, bool state) {
		roads [index] = state;
		neighbors [index].roads [(int)((HexDirection)index).Opposite ()] = state;
		neighbors [index].RefreshSelf ();
		RefreshSelf ();
	}

	public void AddRoad (HexDirection direction) {
		if (!roads [(int)direction]
		    && !HasRiverThroughEdge (direction)
		    && GetElevationDifference (direction) <= 1
		    && !IsSpecial
		    && !GetNeighbor (direction).IsSpecial)
			SetRoad ((int)direction, true);
	}

	public void RemoveOutgoingRiver () {
		if (!hasOutgoingRiver)
			return;
		hasOutgoingRiver = false;
		RefreshSelf ();

		HexCell neighbor = GetNeighbor (outgoingRiver);
		neighbor.hasIncomingRiver = false;
		neighbor.RefreshSelf ();
	}

	public void RemoveIncomingRiver () {
		if (!hasIncomingRiver)
			return;
		hasIncomingRiver = false;
		RefreshSelf ();

		HexCell neighbor = GetNeighbor (incomingRiver);
		neighbor.hasOutgoingRiver = false;
		neighbor.RefreshSelf ();
	}

	public void RemoveRiver () {
		RemoveOutgoingRiver ();
		RemoveIncomingRiver ();
	}

	public void RemoveRoads () {
		for (int i = 0; i < neighbors.Length; i++) {
			if (roads [i]) {
				SetRoad (i, false);
			}
		}
	}

	void Refresh () {
		if (chunk) {
			chunk.Refresh ();
			for (int i = 0; i < neighbors.Length; i++) {
				HexCell neighbor = neighbors [i];
				if (neighbor != null && neighbor.chunk != chunk)
					neighbor.chunk.Refresh ();
			}
		}
	}

	void RefreshSelf () {
		chunk.Refresh ();
	}

	void RefreshPosition () {
		Vector3 position = transform.localPosition;
		position.y = elevation * HexMetrics.elevationStep;
		position.y += ((HexMetrics.SampleNoise (position)).y * 2f - 1f) * HexMetrics.elevationPerturbStrenght;
		transform.localPosition = position;
		Vector3 uiPosition = uiRect.localPosition;
		uiPosition.z = -position.y;
		uiRect.localPosition = uiPosition;

		ValidateRivers ();

		for (int i = 0; i < roads.Length; i++)
			if (roads [i] && GetElevationDifference ((HexDirection)i) > 1)
				SetRoad (i, false);
	}
}
