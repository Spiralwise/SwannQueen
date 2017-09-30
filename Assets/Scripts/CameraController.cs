using UnityEngine;

public class CameraController : MonoBehaviour {

	static CameraController instance;

	public float moveSpeedMinZoom, moveSpeedMaxZoom;
	public float stickMinZoom, stickMaxZoom;
	public float swivelMinZoom, swivelMaxZoom;
	public float rotationSpeed;
	public HexGrid grid;

	Transform swivel, stick;

	float zoom = 1f;
	float rotationAngle;

	public static bool Locked {
		set {
			instance.enabled = !value;
		}
	}

	void Awake () {
		instance = this;
		swivel = transform.GetChild (0);
		stick = swivel.GetChild (0);
	}
		
	void Update () {
		float zoomDelta = Input.GetAxis ("Mouse ScrollWheel");
		if (zoomDelta != 0f)
			AdjustZoom (zoomDelta);

		float rotationDelta = Input.GetAxis ("Rotation");
		if (rotationDelta != 0f)
			AdjustRotation (rotationDelta);

		float xDelta = Input.GetAxis ("Horizontal");
		float yDelta = Input.GetAxis ("Vertical");
		if (xDelta != 0f || yDelta != 0f)
			AdjustPosition (xDelta, yDelta);
	}

	void AdjustZoom (float delta) {
		zoom = Mathf.Clamp01 (zoom + delta);
		float distance = Mathf.Lerp (stickMinZoom, stickMaxZoom, zoom);
		stick.localPosition = new Vector3 (0f, 0f, distance);
		float angle = Mathf.Lerp (swivelMinZoom, swivelMaxZoom, zoom);
		swivel.localRotation = Quaternion.Euler (angle, 0f, 0f);
	}

	void AdjustRotation (float delta) {
		rotationAngle += delta * rotationSpeed * Time.deltaTime;
		if (rotationAngle < 0f)
			rotationAngle += 360f;
		else if (rotationAngle >= 360f)
			rotationAngle -= 360f;
		transform.localRotation = Quaternion.Euler (0f, rotationAngle, 0f);
	}

	public static void ValidatePosition () {
		instance.AdjustPosition (0f, 0f);
	}

	void AdjustPosition (float xDelta, float yDelta) {
		Vector3 direction = transform.localRotation * new Vector3 (xDelta, 0f, yDelta).normalized;
		float moveSpeed = Mathf.Lerp (moveSpeedMinZoom, moveSpeedMaxZoom, zoom);
		float damping = Mathf.Max (Mathf.Abs (xDelta), Mathf.Abs (yDelta));
		float distance = moveSpeed * damping * Time.deltaTime;

		Vector3 position = transform.localPosition;
		position += direction * distance;
		transform.localPosition = ClampPosition (position);
	}

	Vector3 ClampPosition (Vector3 position) {
		float xMax = (grid.cellCountX - 0.5f) * (2f * HexMetrics.innerRadius);
		position.x = Mathf.Clamp (position.x, 0f, xMax);
		float yMax = (grid.cellCountY -1f) * (2f * HexMetrics.innerRadius);
		position.y = Mathf.Clamp (position.y, 0f, yMax);

		return position;
	}
}
