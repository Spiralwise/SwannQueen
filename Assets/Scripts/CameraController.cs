using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {

	public int sensivity = 1000;
	public int scrollSensivity = 5000;
	public GameObject cameraTarget;

	public Vector3 target;

	void Start () {
		if (cameraTarget != null)
			target = cameraTarget.transform.position;
		else
			target = Vector3.zero;
		transform.LookAt (target);
		if (cameraTarget != null)
			cameraTarget.transform.SetParent (transform);
	}

	void Update () {
		if (!EventSystem.current.IsPointerOverGameObject ()) {
			if (cameraTarget != null)
				target = cameraTarget.transform.position;
			float mouseX = Input.GetAxis ("Mouse X");
			float mouseY = Input.GetAxis ("Mouse Y");
			if (Input.GetMouseButton (0)) {
				if (Input.GetKey (KeyCode.LeftAlt))
					transform.RotateAround (target, Vector3.up, mouseX * sensivity * Time.deltaTime);
				else {
					Vector3 forward = Vector3.ProjectOnPlane (transform.forward, Vector3.up);
					transform.Translate (new Vector3 (
						-mouseX * sensivity * Time.deltaTime,
						0f,
						0f));
					transform.position -= mouseY * forward.normalized * sensivity * Time.deltaTime;
				}
			}
			transform.Translate (Vector3.forward * Input.GetAxis ("Mouse ScrollWheel") * scrollSensivity * Time.deltaTime);
		}
	}
}
