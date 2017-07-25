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
			EditCell (grid.GetCell (hit.point));
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

	void EditCell (HexCell cell) {
		if (applyColor)
			cell.Color = activeColor;
		if (applyElevation)
			cell.Elevation = activeElevation;
	}
}
