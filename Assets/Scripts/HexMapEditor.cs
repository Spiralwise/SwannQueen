using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour {

	public Color[] colors;
	public HexGrid grid;

	Color activeColor;
	int activeElevation;

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
		activeColor = colors [index];
	}

	public void SetElevation (float elevation) {
		activeElevation = (int)elevation;
	}

	void EditCell (HexCell cell) {
		cell.color = activeColor;
		cell.Elevation = activeElevation;
		grid.Refresh ();
	}
}
