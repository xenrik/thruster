using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliceBehaviour : MonoBehaviour {
	public GameObject Slicee;
	public Material Material;

	public float Tolerance = 0.01f;
	public float RotationTolerance = 1;

	public float DebugSpeed = 0.1f;
	public bool DebugBreak = true;

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
				
				//StartCoroutine(sliceDebug(plane));
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

		foreach (Slicer.SlicerDebug result in slicer.sliceDebug(plane)) {
			debug = result;

        	posMesh.vertices = result.vertices.ToArray();
        	posMesh.triangles = result.posTriangles.ToArray();
        	posMesh.colors = result.colours.ToArray();
        	posMesh.RecalculateBounds();
        	posMesh.RecalculateNormals();

	        negMesh.vertices = result.vertices.ToArray();
        	negMesh.triangles = result.negTriangles.ToArray();
        	negMesh.colors = result.colours.ToArray();
        	negMesh.RecalculateBounds();
        	negMesh.RecalculateNormals();

			float ttl = 0;
			while (ttl < DebugSpeed) {
				yield return null;
				ttl += Time.deltaTime;
			}

			if (DebugBreak) {
				Debug.Break();
				yield return null;
			}
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

		Slicer.SlicerDebug debug = (Slicer.SlicerDebug)this.debug;
		Gizmos.color = Color.yellow;
		foreach (Vector3 point in debug.perimiter) {
			Gizmos.DrawSphere(point, 0.05f);
		}
		if (debug.badTriangles != null) {
			Gizmos.color = Color.red;
			for (int i = 0; i < debug.badTriangles.Count; i+=3) {
				DrawLine(debug.badTriangles[i], debug.badTriangles[i+1], 5);
				DrawLine(debug.badTriangles[i+1], debug.badTriangles[i+2], 5);
				DrawLine(debug.badTriangles[i+2], debug.badTriangles[i], 5);
			}
		}
		if (debug.testPoint != Vector3.positiveInfinity) {
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(debug.testPoint, 0.05f);
		}
	}

	public static void DrawLine(Vector3 p1, Vector3 p2, float width)
 {
     int count = 1 + Mathf.CeilToInt(width); // how many lines are needed.
     if (count == 1)
     {
         Gizmos.DrawLine(p1, p2);
     }
     else
     {
         Camera c = Camera.current;
         if (c == null)
         {
             Debug.LogError("Camera.current is null");
             return;
         }
         var scp1 = c.WorldToScreenPoint(p1);
         var scp2 = c.WorldToScreenPoint(p2);
 
         Vector3 v1 = (scp2 - scp1).normalized; // line direction
         Vector3 n = Vector3.Cross(v1, Vector3.forward); // normal vector
 
         for (int i = 0; i < count; i++)
         {
             Vector3 o = 0.99f * n * width * ((float)i / (count - 1) - 0.5f);
             Vector3 origin = c.ScreenToWorldPoint(scp1 + o);
             Vector3 destiny = c.ScreenToWorldPoint(scp2 + o);
             Gizmos.DrawLine(origin, destiny);
         }
     }
 }
}