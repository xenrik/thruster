using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEditor;

public class SliceBehaviour : MonoBehaviour {
	public GameObject Slicee;
	public Material Material;
	public bool Debug;
	public bool CheckForHoles;
	public bool Optimise;

	public float Tolerance = 0.01f;
	public float RotationTolerance = 1;

    private Slicer slicer;

	private Vector3 oldPosition;
	private Quaternion oldRotation;

	private Plane plane;

	private HashSet<GameObject> slices = new HashSet<GameObject>();

    private void Start() {
    	MeshFilter f = Slicee.GetComponent<MeshFilter>();
        slicer = new Slicer(f.mesh, Optimise, CheckForHoles);

		oldPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		oldRotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		UnityEngine.Debug.DrawRay(transform.position, transform.up * 5, Color.red);
		Vector3 offset = transform.position - oldPosition;
		float rotationOffset = Quaternion.Angle(transform.rotation, oldRotation);
		if (offset.magnitude > Tolerance || rotationOffset > RotationTolerance || true) {
			StopAllCoroutines();

			foreach (GameObject slice in slices) {
				Destroy(slice);
			}
			slices.Clear();

			Quaternion rot = Quaternion.FromToRotation(Slicee.transform.up, transform.up);
			Vector3 normal = rot * Vector3.up;

			plane.SetNormalAndPosition(normal, transform.position);
			
			Bounds b = new Bounds();
			foreach (Renderer r in Slicee.GetComponentsInChildren<Renderer>()) {
				b.Encapsulate(r.bounds);
			}

			if (PlaneIntersects(plane, b)) {
				plane.Translate(Slicee.transform.position);
				slice(plane);

				Slicee.SetActive(false);
			} else {
				Slicee.SetActive(true);
			}

			oldPosition = transform.position;
			oldRotation = transform.rotation;
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

	private void slice(Plane p) {
		MeshRenderer renderer;

		GameObject negGO = new GameObject();
		negGO.transform.position = Slicee.transform.position;
		negGO.transform.rotation = Slicee.transform.rotation;
		slices.Add(negGO);

		renderer = negGO.AddComponent<MeshRenderer>();
		renderer.material = Material;

		MeshFilter negFilter = negGO.AddComponent<MeshFilter>();

		Mesh negMesh = new Mesh();
		negFilter.mesh = negMesh;

		slicer.slice(plane);

		negFilter.mesh = slicer.negMesh;
		negGO.AddComponent<MeshCollider>();
	}
}