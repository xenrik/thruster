using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BzKovSoft.ObjectSlicer;

public class BzSlicer : MonoBehaviour {

	public GameObject Slicee;
	public float Tolerance = 0.01f;
	public float RotationTolerance = 1;

	private Vector3 oldPosition;
	private Quaternion oldRotation;

	private Plane plane;

	private HashSet<GameObject> slices = new HashSet<GameObject>();

	// Use this for initialization
	void Start () {
		oldPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		oldRotation = transform.rotation;

		Slicee.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		Debug.DrawRay(transform.position, transform.up * 5, Color.red);
		Vector3 offset = transform.position - oldPosition;
		float rotationOffset = Quaternion.Angle(transform.rotation, oldRotation);
		if (offset.magnitude > Tolerance || rotationOffset > RotationTolerance) {
			foreach (GameObject slice in slices) {
				Destroy(slice);
			}
			slices.Clear();

			plane.SetNormalAndPosition(transform.up, transform.position);
			Bounds b = new Bounds();
			foreach (Renderer r in Slicee.GetComponentsInChildren<Renderer>()) {
				b.Encapsulate(r.bounds);
			}

			if (PlaneIntersects(plane, b)) {
				Debug.Log("Slice!");
				GameObject copy = Instantiate(Slicee);

				// Slice
				foreach (IBzSliceableNoRepeat sliceable in copy.GetComponentsInChildren<IBzSliceableNoRepeat>()) {
					sliceable.Slice(plane, 0, OnSliceFinished);
				}

				Slicee.SetActive(false);
			} else {
				Debug.Log("OOB");
				Slicee.SetActive(true);
			}

			oldPosition = transform.position;
			oldRotation = transform.rotation;
		}
	}

	private void OnSliceFinished(BzSliceTryResult result) {
		Debug.Log("Result: " + result.sliced + " - " + result.outObjectNeg + "," + result.outObjectPos);
		if (result.sliced) {
			slices.Add(result.outObjectNeg);
			result.outObjectNeg.SetActive(true);
			Destroy(result.outObjectPos);
		}
	}

	private bool PlaneIntersects(Plane p, Bounds b) {
		bool side = p.GetSide(b.min);

		if (side != p.GetSide(b.max)) {
			return true;
		}
		if (side != p.GetSide(new Vector3(b.min.x, b.min.y, b.max.z))) {
			return true;
		}
		if (side != p.GetSide(new Vector3(b.min.x, b.max.y, b.min.z))) {
			return true;
		}
		if (side != p.GetSide(new Vector3(b.min.x, b.max.y, b.max.z))) {
			return true;
		}
		if (side != p.GetSide(new Vector3(b.max.x, b.min.y, b.min.z))) {
			return true;
		}
		if (side != p.GetSide(new Vector3(b.max.x, b.min.y, b.max.z))) {
			return true;
		}
		if (side != p.GetSide(new Vector3(b.max.x, b.max.y, b.min.z))) {
			return true;
		}

		return false;
	}
}
