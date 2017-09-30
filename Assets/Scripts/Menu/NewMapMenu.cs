using UnityEngine;

public class NewMapMenu : MonoBehaviour {

	public HexGrid grid;

	// ---- UI slots
	public void Open () {
		gameObject.SetActive (true);
		CameraController.Locked = true;
	}

	public void Close () {
		gameObject.SetActive (false);
		CameraController.Locked = false;
	}

	public void CreateSmall() {
		CreateMap (20, 15);
	}

	public void CreateNormal() {
		CreateMap (40, 30);
	}

	public void CreateLarge() {
		CreateMap (80, 60);
	}

	// ---- Methods
	void CreateMap (int x, int y) {
		grid.CreateMap (x, y);
		CameraController.ValidatePosition ();
		Close ();
	}
}
