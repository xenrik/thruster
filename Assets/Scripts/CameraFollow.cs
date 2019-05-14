using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public GameObject Target;
	public Vector3 Offset;
	public float RotationSpeed;

	private float CurrentRotation;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Target == null) {
			return;
		}
		
		// Orbit the target
		Vector3 targetPosition = Target.transform.position;
		Quaternion rotation = Quaternion.Euler(0, CurrentRotation, 0);
		targetPosition += rotation * Offset;

		Vector3 position = Vector3.Lerp(transform.position, targetPosition, 0.9f);
		transform.position = position;

		// Alway look at it
		gameObject.transform.LookAt(Target.transform.position, Vector3.up);

		// Rotate if needed
		if (Input.GetKeyDown(KeyCode.Q)) {
			StopAllCoroutines();
			StartCoroutine(Rotate(1));
		} else if (Input.GetKeyDown(KeyCode.E)) {
			StopAllCoroutines();
			StartCoroutine(Rotate(-1));
		}
	}

	private IEnumerator Rotate(int direction) {
		float start, p;
		if (direction > 0) {
			start = Mathf.Floor(CurrentRotation / 90) * 90;
			p = (CurrentRotation - start) / 90;
		} else {
			start = Mathf.Ceil(CurrentRotation / 90) * 90;
			p = (start - CurrentRotation) / 90;
		}

		while (p < 1) {
			p += (1 / RotationSpeed) * Time.deltaTime;
			if (p > 1) {
				p = 1;
			}

			CurrentRotation = start + (Mathf.SmoothStep(0, 1, p) * 90 * direction);
			yield return null;
		}
	}
}
