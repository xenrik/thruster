using System.Collections.Generic;
using UnityEngine;

public class SliceBehaviour : MonoBehaviour {
     private Slicer slicer;

	public float Tolerance = 0.01f;
	public float RotationTolerance = 1;

	private Vector3 oldPosition;
	private Quaternion oldRotation;

	private Plane plane;

	private HashSet<GameObject> slices = new HashSet<GameObject>();

     private void Start() {
          MeshFilter f = GetComponent<MeshFilter>();
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
			foreach (GameObject slice in slices) {
				Destroy(slice);
			}
			slices.Clear();

			plane.SetNormalAndPosition(transform.up, transform.position);
			Bounds b = new Bounds();
			foreach (Renderer r in GetComponentsInChildren<Renderer>()) {
				b.Encapsulate(r.bounds);
			}

			if (PlaneIntersects(plane, b)) {
				Debug.Log("Slice!");

                    slicer.slice(plane);
                    
                    GameObject posGO = new GameObject();
                    MeshFilter posFilter = posGO.AddComponent<MeshFilter>();
                    posFilter.mesh = slicer.posMesh;
                    posGO.AddComponent<MeshRenderer>();
                    
                    GameObject negGO = new GameObject();
                    MeshFilter negFilter = negGO.AddComponent<MeshFilter>();
                    negFilter.mesh = slicer.negMesh;
                    negGO.AddComponent<MeshRenderer>();

                    slices.Add(posGO);
                    slices.Add(negGO);
				GetComponent<MeshRenderer>().enabled = false;
			} else {
				Debug.Log("OOB");
				GetComponent<MeshRenderer>().enabled = true;
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
}