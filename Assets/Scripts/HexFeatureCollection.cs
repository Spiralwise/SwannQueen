using UnityEngine;

[System.Serializable]
public class HexFeatureCollection {

	public Transform[] prefabs;

	public Transform Pick (float choice) {
		return prefabs [(int)(choice * prefabs.Length)];
	}
}
