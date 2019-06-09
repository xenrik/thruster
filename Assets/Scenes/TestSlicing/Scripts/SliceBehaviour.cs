using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SliceBehaviour : MonoBehaviour {
	public GameObject Slicee;
	public Material Material;

	public bool ShowDebug;
	public float DebugSpeed = 0.1f;

	public float Tolerance = 0.01f;
	public float RotationTolerance = 1;


    private Slicer slicer;

	private Vector3 oldPosition;
	private Quaternion oldRotation;

	private Plane plane;

	private HashSet<GameObject> slices = new HashSet<GameObject>();
	private Slicer.SlicerDebug? debug;

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
				
				if (ShowDebug) {
					StartCoroutine(sliceDebug(plane));
				} else {
					slice(plane);
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
	}

	private IEnumerator sliceDebug(Plane p) {
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

		List<Vector3> verts = new List<Vector3>();
		List<Color> colors = new List<Color>();
		List<int> posTris = new List<int>();
		List<int> negTris = new List<int>();

		foreach (Slicer.SlicerDebug result in slicer.sliceDebug(plane)) {
			debug = result;

			verts.Clear();
			colors.Clear();
			posTris.Clear();
			negTris.Clear();

			int idx = 0;
			foreach (Triangle3D tri in result.allTriangles) {				
				verts.Add(tri.a); verts.Add(tri.b); verts.Add(tri.c);
				colors.Add(tri.color); colors.Add(tri.color); colors.Add(tri.color);

				posTris.Add(idx); posTris.Add(idx + 1); posTris.Add(idx + 2);
				negTris.Add(idx+2); negTris.Add(idx + 1); negTris.Add(idx);
				idx += 3;
			}

        	posMesh.vertices = verts.ToArray();
        	posMesh.colors = colors.ToArray();
        	posMesh.triangles = posTris.ToArray();
        	posMesh.RecalculateBounds();
        	posMesh.RecalculateNormals();

	        negMesh.vertices = posMesh.vertices;
        	negMesh.colors = negMesh.colors;
        	negMesh.triangles = negTris.ToArray();
        	negMesh.RecalculateBounds();
        	negMesh.RecalculateNormals();

			float ttl = 0;
			while (ttl < DebugSpeed) {
				yield return null;
				ttl += Time.deltaTime;
			}

			Debug.Break();
			yield return null;
		}

		posFilter.mesh = slicer.posMesh;
		negFilter.mesh = slicer.negMesh;
		debug = null;
		Debug.Log("Finished!");
	}

	private void OnDrawGizmos() {
		if (this.debug == null) {
			return;
		}

		Vector3 offset = Slicee.transform.position;
		Slicer.SlicerDebug debug = (Slicer.SlicerDebug)this.debug;
		Gizmos.color = Color.white;
		foreach (Vector3 point in debug.perimiter) {
			Gizmos.DrawSphere(point + offset, 0.025f);
		}

		foreach (Triangle3D tri in debug.allTriangles) {
			Gizmos.color = tri.color;
			MoreGizmos.DrawCircle(tri.circumcircleOrigin + offset, plane.normal, tri.circumcircleRadius, 5);
		}

		float speed = 5;
		foreach (Triangle3D tri in debug.badTriangles) {
			Color c = Color.red;
			c.a = (Mathf.Sin((float)EditorApplication.timeSinceStartup * speed) + 1) / 2;
			Gizmos.color = c;
			
			MoreGizmos.DrawCircle(tri.circumcircleOrigin + offset, plane.normal, tri.circumcircleRadius, 5);

			MoreGizmos.DrawLine(tri.a + offset, tri.b + offset, 5);
			MoreGizmos.DrawLine(tri.b + offset, tri.c + offset, 5);
			MoreGizmos.DrawLine(tri.c + offset, tri.a + offset, 5);
		}

		if (debug.testPoint != Vector3.positiveInfinity) {
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(debug.testPoint + offset, 0.025f);
		}
	}
}