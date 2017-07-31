using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour {

	public Color[] colors;
	public HexGrid grid;

	bool isDrag;
	HexDirection dragDirection;
	HexCell previousCell;
	HexCell antePreviousCell;

	Color activeColor;
	bool applyColor;
	int activeElevation;
	int activeWaterLevel;
	bool applyElevation = true;
	bool applyWaterLevel = true;
	int brushSize;

	enum OptionalToggle {
		Ignore, Yes, No
	}

	OptionalToggle riverMode;
	OptionalToggle roadMode;

	void Awake () {
		SelectColor (-1);
	}

	void Update () {
		if (Input.GetMouseButton (1) && !EventSystem.current.IsPointerOverGameObject ())
			HandleInput ();
		else {
			previousCell = null;
			antePreviousCell = null;
		}
	}

	void HandleInput () {
		Ray inputRay = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast (inputRay, out hit)) {
			HexCell currentCell = grid.GetCell (hit.point);
			if (previousCell && previousCell != currentCell 
				&& (!antePreviousCell || antePreviousCell != currentCell))
				ValidateDrag (currentCell);
			else
				isDrag = false;
			EditCells (currentCell);
			antePreviousCell = previousCell;
			previousCell = currentCell;
		} else {
			previousCell = null;
			antePreviousCell = null;
		}
	}

	public void SelectColor (int index) {
		applyColor = index >= 0;
		if (applyColor)
			activeColor = colors [index];
	}

	public void SetElevation (float elevation) {
		activeElevation = (int)elevation;
	}

	public void SetApplyElevation (bool toggle) {
		applyElevation = toggle;
	}

	public void SetRiverMode (int mode) {
		riverMode = (OptionalToggle)mode;
	}

	public void SetRoadMode (int mode) {
		roadMode = (OptionalToggle)mode;
	}

	public void SetApplyWaterLevel (bool toggle) {
		applyWaterLevel = toggle;
	}

	public void SetWaterLevel (float level) {
		activeWaterLevel = (int)level;
	}

	public void SetBrushSize (float size) {
		brushSize = (int)size;
	}

	public void ToggleLabels (bool visible) {
		grid.ShowUI (visible);
	}

	void EditCell (HexCell cell) {
		if (cell) {
			if (applyColor)
				cell.Color = activeColor;
		
			if (applyElevation)
				cell.Elevation = activeElevation;

			if (applyWaterLevel)
				cell.WaterLevel = activeWaterLevel;
		
			if (riverMode == OptionalToggle.No)
				cell.RemoveRiver ();

			if (roadMode == OptionalToggle.No)
				cell.RemoveRoads ();
			
			if (isDrag) {
				HexCell that = cell.GetNeighbor (dragDirection.Opposite ());
				if (that) {
					if (riverMode == OptionalToggle.Yes)
						that.SetOutgoingRiver (dragDirection);
					if (roadMode == OptionalToggle.Yes)
						that.AddRoad (dragDirection);
				}
			}
		}
	}

	void EditCells (HexCell center) {
		int centerX = center.coordinates.X;
		int centerY = center.coordinates.Y;

		for (int r = 0, y = centerY - brushSize; y <= centerY; y++, r++) {
			for (int x = centerX - r; x <= centerX + brushSize; x++)
				EditCell (grid.GetCell (new HexCoordinates (x, y)));
		}

		for (int r = 0, y = centerY + brushSize; y > centerY; y--, r++) {
			for (int x = centerX - brushSize; x <= centerX + r; x++)
				EditCell (grid.GetCell (new HexCoordinates (x, y)));
		}
	}

	void ValidateDrag (HexCell currentCell) {
		for (dragDirection = HexDirection.NE; dragDirection <= HexDirection.NW; dragDirection++)
			if (previousCell.GetNeighbor(dragDirection) == currentCell) {
				isDrag = true;
				return;
			}
		isDrag = false;
	}
}
