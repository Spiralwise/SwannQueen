using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HexUnit : MonoBehaviour {

	const float travelSpeed = 3f;

	public static HexUnit unitPrefab;

	HexCell location;
	float orientation;
	List<HexCell> pathToTravel;

	public HexCell Location {
		get {
			return location;
		}
		set {
			if (location)
				location.Unit = null;
			location = value;
			value.Unit = this;
			transform.localPosition = value.Position;
		}
	}

	public float Orientation {
		get {
			return orientation;
		}
		set {
			orientation = value;
			transform.localRotation = Quaternion.Euler (0f, value, 0f);
		}
	}

	public bool IsValidDestination (HexCell cell) {
		return !cell.IsUnderWater && !cell.Unit;
	}

	public void ValidateLocation () {
		transform.localPosition = location.Position;
	}

	public void Die () {
		location.Unit = null;
		Destroy (gameObject);
	}

	public void Travel (List<HexCell> path) {
		Location = path [path.Count - 1];
		pathToTravel = path;
		StopAllCoroutines ();
		StartCoroutine (TravelPath ());
	}

	IEnumerator TravelPath () {
		Vector3 a, b, c = pathToTravel [0].Position;
		float t = Time.deltaTime * travelSpeed;
		for (int i = 1; i < pathToTravel.Count; i++) {
			a = c;
			b = pathToTravel [i - 1].Position;
			c = (b + pathToTravel [i].Position) * 0.5f;
			for (; t < 1f; t += Time.deltaTime * travelSpeed) {
				transform.localPosition = Beziers.GetPoint(a, b, c, t);
				yield return null;
			}
			t -= 1f;
		}
		a = c;
		b = pathToTravel [pathToTravel.Count - 1].Position;
		c = b;
		for (; t < 1f; t += Time.deltaTime * travelSpeed) {
			transform.localPosition = Beziers.GetPoint(a, b, c, t);
			yield return null;
		}
		transform.localPosition = location.Position;
	}

	public void Save (BinaryWriter writer) {
		location.coordinates.Save (writer);
		writer.Write (orientation);
	}

	public static void Load (BinaryReader reader, HexGrid grid) {
		HexCoordinates coordinates = HexCoordinates.Load (reader);
		float orientation = reader.ReadSingle ();
		grid.AddUnit (Instantiate (unitPrefab), grid.GetCell (coordinates), orientation);
	}

	void OnEnable() {
		if (location)
			transform.localPosition = location.Position;
	}

	void OnDrawGizmos () {
		if (pathToTravel == null || pathToTravel.Count == 0)
			return;
		Vector3 a, b, c = pathToTravel [0].Position;
		for (int i = 1; i < pathToTravel.Count; i++) {
			a = c;
			b = pathToTravel [i - 1].Position;
			c = (b + pathToTravel [i].Position) * 0.5f;
			for (float t = 0f; t < 1f; t += 0.1f)
				Gizmos.DrawSphere (Beziers.GetPoint(a, b, c, t), 2f);
		}
		a = c;
		b = pathToTravel [pathToTravel.Count - 1].Position;
		c = b;
		for (float t = 0f; t < 1f; t += 0.1f)
			Gizmos.DrawSphere (Beziers.GetPoint(a, b, c, t), 2f);
	}
}
