using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour {

	public HexGrid grid;
	public CanvasGroup editorPanel;

	bool editMode;

	bool isDrag;
	HexDirection dragDirection;
	HexCell searchFromCell, searchToCell;
	HexCell previousCell;
	HexCell antePreviousCell;

	int activeTerrainTypeIndex = -1;
	bool applyColor;
	int activeElevation;
	int activeWaterLevel;
	int activeUrbanLevel, activeFarmLevel, activePlantLevel, activeSpecialIndex;
	bool applyElevation = true;
	bool applyWaterLevel = true;
	bool applyUrbanLevel = true, applyFarmLevel = true, applyPlantLevel = true, applySpecialIndex = true;
	int brushSize;

	enum OptionalToggle {
		Ignore, Yes, No
	}

	OptionalToggle riverMode;
	OptionalToggle roadMode;
	OptionalToggle walledMode;

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
			if (editMode)
				EditCells (currentCell);
			else if (Input.GetKey(KeyCode.LeftShift) && searchToCell != currentCell) {
				if (searchFromCell)
					searchFromCell.DisableOutline ();
				searchFromCell = currentCell;
				searchFromCell.EnableOutline (Color.blue);
				if (searchToCell)
					grid.FindPath (searchFromCell, searchToCell);
			}
			else if (searchFromCell && searchFromCell != currentCell) {
				searchToCell = currentCell;
				grid.FindPath (searchFromCell, searchToCell);
			}
			antePreviousCell = previousCell;
			previousCell = currentCell;
		} else {
			previousCell = null;
			antePreviousCell = null;
		}
	}

	public void SetEditMode (bool toggle) {
		editMode = toggle;
		grid.ShowUI (!toggle);
		editorPanel.blocksRaycasts = toggle;
		editorPanel.alpha = toggle ? 1.0f : 0.0f;
	}

	public void SetTerrainTypeIndex (int index) {
		activeTerrainTypeIndex = index;
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

	public void SetApplyUrbanLevel (bool toggle) {
		applyUrbanLevel = toggle;
	}

	public void SetUrbanLevel (float level) {
		activeUrbanLevel = (int)level;
	}

	public void SetApplyFarmLevel (bool toggle) {
		applyFarmLevel = toggle;
	}

	public void SetFarmLevel (float level) {
		activeFarmLevel = (int)level;
	}

	public void SetApplyPlantLevel (bool toggle) {
		applyPlantLevel = toggle;
	}

	public void SetPlantLevel (float level) {
		activePlantLevel = (int)level;
	}

	public void SetApplySpecialIndex (bool toggle) {
		applySpecialIndex = toggle;
	}

	public void SetSpecialIndex (float index) {
		activeSpecialIndex = (int)index;
	}

	public void SetWalledMode (int mode) {
		walledMode = (OptionalToggle)mode;
	}

	public void SetBrushSize (float size) {
		brushSize = (int)size;
	}

	public void ToggleLabels (bool visible) {
		grid.ShowUI (visible);
	}

	void EditCell (HexCell cell) {
		if (cell) {
			if (activeTerrainTypeIndex >= 0)
				cell.TerrainTypeIndex = activeTerrainTypeIndex;
			
			if (applyElevation)
				cell.Elevation = activeElevation;

			if (applyWaterLevel)
				cell.WaterLevel = activeWaterLevel;
		
			if (riverMode == OptionalToggle.No)
				cell.RemoveRiver ();

			if (roadMode == OptionalToggle.No)
				cell.RemoveRoads ();

			if (applyUrbanLevel)
				cell.UrbanLevel = activeUrbanLevel;

			if (applyFarmLevel)
				cell.FarmLevel = activeFarmLevel;

			if (applyPlantLevel)
				cell.PlantLevel = activePlantLevel;

			if (applySpecialIndex)
				cell.SpecialIndex = activeSpecialIndex;

			if (walledMode != OptionalToggle.Ignore)
				cell.Walled = walledMode == OptionalToggle.Yes;
			
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
