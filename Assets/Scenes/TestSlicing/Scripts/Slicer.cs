using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BzKovSoft.ObjectSlicer;

public class Slicer : MonoBehaviour {

	public GameObject Slicee;
	public float Tolerance;

	private Vector3 oldPosition;
	private Plane plane;

	private HashSet<GameObject> slices = new HashSet<GameObject>();

	// Use this for initialization
	void Start () {
		oldPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Slicee.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 offset = transform.position - oldPosition;
		if (offset.magnitude > Tolerance) {
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
		}
	}

	private void OnSliceFinished(BzSliceTryResult result) {
		Debug.Log("Result: " + result.sliced + " - " + result.outObjectNeg + "," + result.outObjectPos);
		slices.Add(result.outObjectNeg);
		result.outObjectNeg.SetActive(true);
		Destroy(result.outObjectPos);
	}

	private bool PlaneIntersects(Plane p, Bounds b) {
		return !p.SameSide(b.min, b.max);
	}
}
