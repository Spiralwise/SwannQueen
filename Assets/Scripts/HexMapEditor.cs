﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour {

	public HexGrid grid;
	public CanvasGroup editorPanel;

	bool isDrag;
	HexDirection dragDirection;
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

	void Awake () {
		SetEditMode (false);
	}

	void Update () {
		if (!EventSystem.current.IsPointerOverGameObject ()) {
			if (Input.GetMouseButton (0)) {
				HandleInput ();
			}
			else if (Input.GetKeyDown (KeyCode.U)) {
				if (Input.GetKey (KeyCode.LeftShift))
					DestroyUnit ();
				else
					CreateUnit ();
			}
			else {
				previousCell = null;
				antePreviousCell = null;
			}
		}

	}

	void HandleInput () {
		HexCell currentCell = GetCellUnderCursor ();
		if (currentCell) {
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

	public void SetEditMode (bool toggle) {
		enabled = toggle;
		editorPanel.alpha = toggle ? 1.0f : 0.0f;
		editorPanel.interactable = toggle;
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

	HexCell GetCellUnderCursor () {
		return grid.GetCell (Camera.main.ScreenPointToRay (Input.mousePosition));
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

	void CreateUnit () {
		HexCell cell = GetCellUnderCursor ();
		if (cell && !cell.Unit) {
			grid.AddUnit (Instantiate (HexUnit.unitPrefab), cell, Random.Range (0f, 360f));
		}
	}

	void DestroyUnit () {
		HexCell cell = GetCellUnderCursor ();
		if (cell && cell.Unit)
			grid.RemoveUnit (cell.Unit);
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
