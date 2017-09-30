using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SaveMapMenu : MonoBehaviour {

	public HexGrid grid;
	public Text menuLabel, actionButtonLabel;
	public InputField nameInput;
	public RectTransform listContent;
	public SaveItem itemPrefab;

	bool saveMode;

	// ---- UI slots
	public void Open (bool saveMode) {
		this.saveMode = saveMode;
		if (saveMode) {
			menuLabel.text = "Save map";
			actionButtonLabel.text = "Save";
		}
		else {
			menuLabel.text = "Load map";
			actionButtonLabel.text = "Load";
		}
		FillList ();
		gameObject.SetActive (true);
		CameraController.Locked = true;
	}

	public void Close () {
		gameObject.SetActive (false);
		CameraController.Locked = false;
	}

	public void Action () {
		string path = GetSelectedPath ();
		if (path == null)
			return;
		if (saveMode)
			Save (path);
		else
			Load (path);
		Close ();
	}

	public void Delete () {
		string path = GetSelectedPath ();
		if (path == null)
			return;
		if (File.Exists (path))
			File.Delete (path);
		nameInput.text = "";
		FillList ();
	}

	public void SelectItem (string name) {
		nameInput.text = name;
	}

	// ---- Methods
	string GetSelectedPath () {
		string mapName = nameInput.text;
		if (mapName.Length == 0)
			return null;
		return Path.Combine (Application.persistentDataPath, mapName + ".map");
	}

	void FillList () {
		for (int i = 0; i < listContent.childCount; i++)
			Destroy (listContent.GetChild (i).gameObject);
		string[] paths = Directory.GetFiles (Application.persistentDataPath, "*.map");
		Array.Sort (paths);
		for (int i = 0; i < paths.Length; i++) {
			SaveItem item = Instantiate (itemPrefab);
			item.menu = this;
			item.MapName = Path.GetFileNameWithoutExtension (paths [i]);
			item.transform.SetParent (listContent, false);
		}
	}

	void Save (string path) {
		using (BinaryWriter writer = new BinaryWriter (File.Open (path, FileMode.Create))) {
			writer.Write (1);
			grid.Save (writer);
			Debug.Log ("Map saved to " + path + ".");
		}
	}

	void Load (string path) {
		if (!File.Exists(path)) {
			Debug.LogError ("File does not exist (" + path + "). Loading aborted.");
			return;
		}
		using (BinaryReader reader = new BinaryReader(File.OpenRead(path))) {
			int header = reader.ReadInt32 ();
			if (header <= 1) {
				grid.Load (reader, header);
				CameraController.ValidatePosition ();
				Debug.Log ("Map loaded from " + path + " (Version: " + header + ").");
			}
			else {
				Debug.LogWarning ("Unknown map format " + header + " from " + path + ". Map not loaded.");
			}
		}
	}
}
