using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour {

	public HexGridChunk chunk;
	public HexCoordinates coordinates;
	public RectTransform uiRect;

	int elevation = int.MinValue;
	Color color;
	bool hasIncomingRiver, hasOutgoingRiver;
	HexDirection incomingRiver, outgoingRiver;

	[SerializeField]
	HexCell[] neighbors;

	public int Elevation {
		get {
			return elevation;
		}
		set {
			if (elevation == value)
				return;
			elevation = value;
			Vector3 position = transform.localPosition;
			position.y = value * HexMetrics.elevationStep;
			position.y += ((HexMetrics.SampleNoise (position)).y * 2f - 1f) * HexMetrics.elevationPerturbStrenght;
			transform.localPosition = position;
			Vector3 uiPosition = uiRect.localPosition;
			uiPosition.z = -position.y;
			uiRect.localPosition = uiPosition;

			if (hasOutgoingRiver && elevation < GetNeighbor (outgoingRiver).elevation)
				RemoveOutgoingRiver ();
			if (hasIncomingRiver && elevation > GetNeighbor (incomingRiver).elevation)
				RemoveIncomingRiver ();

			Refresh ();
		}
	}

	public Color Color {
		get {
			return color;
		}
		set {
			if (color == value)
				return;
			color = value;
			Refresh ();
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

	public void SetOutgoingRiver (HexDirection direction) {
		if (HasOutgoingRiver && OutgoingRiver == direction)
			return;
		
		HexCell neighbor = GetNeighbor (direction);
		if (!neighbor || elevation < neighbor.elevation)
			return;

		RemoveOutgoingRiver ();
		if (hasIncomingRiver && incomingRiver == direction)
			RemoveIncomingRiver ();

		hasOutgoingRiver = true;
		outgoingRiver = direction;
		RefreshSelf ();

		neighbor.RemoveIncomingRiver ();
		neighbor.hasIncomingRiver = true;
		neighbor.incomingRiver = direction.Opposite ();
		neighbor.RefreshSelf ();
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
}
