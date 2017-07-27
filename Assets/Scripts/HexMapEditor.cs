using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour {

	public Color[] colors;
	public HexGrid grid;

	Color activeColor;
	bool applyColor;
	int activeElevation;
	bool applyElevation = true;
	int brushSize;

	void Awake () {
		SelectColor (0);
	}

	void Update () {
		if (Input.GetMouseButton (1) && !EventSystem.current.IsPointerOverGameObject ())
			HandleInput ();
	}

	void HandleInput () {
		Ray inputRay = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast (inputRay, out hit)) {
			EditCells (grid.GetCell (hit.point));
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
}
