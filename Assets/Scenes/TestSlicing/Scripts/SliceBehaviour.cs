using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SliceBehaviour : MonoBehaviour {
	public GameObject Slicee;
	public Material Material;

	public float Tolerance = 0.01f;
	public float RotationTolerance = 1;

    private Slicer slicer;

	private Vector3 oldPosition;
	private Quaternion oldRotation;

	private Plane plane;

	private HashSet<GameObject> slices = new HashSet<GameObject>();

    private void Start() {
    	MeshFilter f = Slicee.GetComponent<MeshFilter>();
        slicer = new Slicer(f.mesh);

		oldPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		oldRotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		Debug.DrawRay(transform.position, transform.up * 5, Color.red);
		Vector3 offset = transform.position - oldPosition;
		float rotationOffset = Quaternion.Angle(transform.rotation, oldRotation);
		if (offset.magnitude > Tolerance || rotationOffset > RotationTolerance) {
			StopAllCoroutines();

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
				plane.Translate(Slicee.transform.position);
				
				slice(plane);

				Slicee.SetActive(false);
			} else {
				Debug.Log("OOB");
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

		GameObject posGO = new GameObject();
		posGO.transform.position = Slicee.transform.position;
		slices.Add(posGO);

		renderer = posGO.AddComponent<MeshRenderer>();
		renderer.material = Material;

		MeshFilter posFilter = posGO.AddComponent<MeshFilter>();
		Mesh posMesh = new Mesh();
		posFilter.mesh = posMesh;

		GameObject negGO = new GameObject();
		negGO.transform.position = Slicee.transform.position;
		slices.Add(negGO);

		renderer = negGO.AddComponent<MeshRenderer>();
		renderer.material = Material;

		MeshFilter negFilter = negGO.AddComponent<MeshFilter>();

		Mesh negMesh = new Mesh();
		negFilter.mesh = negMesh;

		slicer.slice(plane);

		posFilter.mesh = slicer.posMesh;
		negFilter.mesh = slicer.negMesh;

		Debug.Log("Finished!");
	}
}