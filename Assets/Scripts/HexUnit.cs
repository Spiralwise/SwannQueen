using UnityEngine;

public class HexUnit : MonoBehaviour {

	HexCell location;
	float orientation;

	public HexCell Location {
		get {
			return location;
		}
		set {
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

	public void ValidateLocation () {
		transform.localPosition = location.Position;
	}

	public void Die () {
		location.Unit = null;
		Destroy (gameObject);
	}
}
