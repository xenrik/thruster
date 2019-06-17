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
	private Slicer.Debug slicerDebug = null;

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
				//UnityEngine.Debug.Log("Slice!");
				plane.Translate(Slicee.transform.position);
				
				slice(plane);

				Slicee.SetActive(false);
			} else {
				//UnityEngine.Debug.Log("OOB");
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
		ScriptProfiler.StartGroup("Slice");
		MeshRenderer renderer;

		GameObject posGO = new GameObject();
		posGO.transform.position = Slicee.transform.position;
		posGO.transform.rotation = Slicee.transform.rotation;
		slices.Add(posGO);

		renderer = posGO.AddComponent<MeshRenderer>();
		renderer.material = Material;

		MeshFilter posFilter = posGO.AddComponent<MeshFilter>();
		Mesh posMesh = new Mesh();
		posFilter.mesh = posMesh;

		GameObject negGO = new GameObject();
		negGO.transform.position = Slicee.transform.position;
		negGO.transform.rotation = Slicee.transform.rotation;
		slices.Add(negGO);

		renderer = negGO.AddComponent<MeshRenderer>();
		renderer.material = Material;

		MeshFilter negFilter = negGO.AddComponent<MeshFilter>();

		Mesh negMesh = new Mesh();
		negFilter.mesh = negMesh;

		if (Debug) {
			StopAllCoroutines();
			StartCoroutine(sliceDebug(plane, posGO, posFilter, negGO, negFilter));
		} else {
			Profiler.BeginSample("Slice", this);
			slicer.slice(plane);
			Profiler.EndSample();

			posFilter.mesh = slicer.posMesh;
			posGO.AddComponent<MeshCollider>();
			posGO.SetActive(false);

			negFilter.mesh = slicer.negMesh;
			negGO.AddComponent<MeshCollider>();

			//UnityEngine.Debug.Log("Finished!");
		}

		ScriptProfiler.EndGroup();
		ScriptProfiler.Report(ReportMode.Method);
	}

	private IEnumerator sliceDebug(Plane p, GameObject posGO, MeshFilter posFilter, GameObject negGO, MeshFilter negFilter) {
		foreach (Slicer.Debug debug in slicer.sliceDebug(plane)) {
			slicerDebug = debug;

			UnityEngine.Debug.Break();
			yield return true;
		}

		slicerDebug = null;
		posFilter.mesh = slicer.posMesh;
		posGO.AddComponent<MeshCollider>();

		negFilter.mesh = slicer.negMesh;
		negGO.AddComponent<MeshCollider>();

		//UnityEngine.Debug.Log("Finished!");
	}

	private void DrawTriangle(Vector3 a, Vector3 b, Vector3 c, int width, Ray mouseRay, Vector3 circumcircleOrigin, float circumcircleRadius) {
		MoreGizmos.DrawLine(a, b, width);
		MoreGizmos.DrawLine(b, c, width);
		MoreGizmos.DrawLine(c, a, width);

		if (Triangle.Intersects(a, b, c, mouseRay)) {
			MoreGizmos.DrawCircle(circumcircleOrigin, slicerDebug.Plane.normal, circumcircleRadius, 1);
		}
	}

	/*
	private void OnDrawGizmos() {
		if (slicerDebug == null) {
			return;
		}

		Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

		Vector3 offset = Slicee.transform.position;
		Gizmos.color = Color.white;
		foreach (Triangle3D tri in slicerDebug.AllTriangles) {
			DrawTriangle(tri.a + offset, tri.b + offset, tri.c + offset, 3, ray, tri.circumcircleOrigin + offset, tri.circumcircleRadius);
		}

		Gizmos.color = Color.red;
		foreach (Triangle3D tri in slicerDebug.BadTriangles) {
			DrawTriangle(tri.a + offset, tri.b + offset, tri.c + offset, 5, ray, tri.circumcircleOrigin + offset, tri.circumcircleRadius);
		}

		Gizmos.color = Color.green;
		foreach (Triangle3D tri in slicerDebug.NewTriangles) {
			DrawTriangle(tri.a + offset, tri.b + offset, tri.c + offset, 3, ray, tri.circumcircleOrigin + offset, tri.circumcircleRadius);
		}

		Gizmos.color = Color.yellow;
		foreach (Vector3 point in slicerDebug.Perimiter) {
			Gizmos.DrawSphere(point + offset, 0.05f);
		}

		Gizmos.color = Color.blue;
		if (!slicerDebug.CurrentPoint.Equals(Vector3.zero)) {
			Gizmos.DrawSphere(slicerDebug.CurrentPoint + offset, 0.05f);
		}
		if (slicerDebug.CurrentTriangle.circumcircleRadius > 0) {
			Triangle3D tri = slicerDebug.CurrentTriangle;
			DrawTriangle(tri.a + offset, tri.b + offset, tri.c + offset, 1, ray, tri.circumcircleOrigin + offset, tri.circumcircleRadius);
		}
		
		Gizmos.color = Color.cyan;
		if (!slicerDebug.CurrentEdge.a.Equals(Vector3.zero) || !slicerDebug.CurrentEdge.b.Equals(Vector3.zero)) {
			Vector3 a = slicerDebug.CurrentEdge.a + offset;
			Vector3 b = slicerDebug.CurrentEdge.b + offset;

			MoreGizmos.DrawLine(a, b, 2);
		}
	}
	*/
}