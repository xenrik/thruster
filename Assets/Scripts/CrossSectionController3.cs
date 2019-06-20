using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CrossSectionController3 : MonoBehaviour {

	public GameObject Thruster;
	public GameObject SurfaceLevel;

	public GameObject Tunnel;
	public float FadeDuration;

	public GameObject Plane;

	public float PositionTolerance;
	public float RotationTolerance;

	private List<Material> tunnelMaterials;

	private bool showingCrossSection = false;
	private GameObject crossSection;

	private Vector3 oldPosition;
	private Quaternion oldRotation;

	private SlicerDetails[] slicers;
	private bool justdoit = true;


	// Use this for initialization
	void Start () {
		crossSection = Instantiate(Tunnel);
		crossSection.name += " (CrossSection)";
		
		MeshFilter[] filter = crossSection.GetComponentsInChildren<MeshFilter>();
		slicers = new SlicerDetails[filter.Length];
		for (int i = 0; i < slicers.Length; ++i) {
			slicers[i] = new SlicerDetails(filter[i]);
		}

		// Collect the materials for the tunnel
		tunnelMaterials = new List<Material>();
		Renderer[] renderers = Tunnel.GetComponentsInChildren<Renderer>();
		foreach (Renderer r in renderers) {
			tunnelMaterials.AddRange(r.materials);
		}
	}

	float xoffset = 0;
	
	// Update is called once per frame
	void Update () {
		if (Thruster == null) {
			return;
		}

		if (Thruster.transform.position.y > SurfaceLevel.transform.position.y) {
			if (showingCrossSection) {
				StopAllCoroutines();
				StartCoroutine(HideCrossSection());

				showingCrossSection = false;
			}
		} else {
			if (!showingCrossSection) {
				StopAllCoroutines();
				StartCoroutine(ShowCrossSection());
			}

			Vector3 offset = transform.position - oldPosition;
			float rotationOffset = Quaternion.Angle(transform.rotation, oldRotation);
			if (offset.magnitude > PositionTolerance || rotationOffset > RotationTolerance || !showingCrossSection || justdoit) {
				GameObject camera = gameObject;

				float yRot = camera.transform.rotation.eulerAngles.y;
				yRot = (Mathf.RoundToInt(yRot / 90) % 4) * 90;
				bool showingFront = yRot == 0 || yRot == 180;

				Vector3 planePos = (Thruster.transform.position - Tunnel.transform.position);
				planePos.x /= Tunnel.transform.localScale.x;
				planePos.y /= Tunnel.transform.localScale.y;
				planePos.z /= Tunnel.transform.localScale.z;

				planePos = Vector3.zero;
				
				/* 
				planePos.y = 0;
				if (showingFront) {
					planePos.x = 0;
				} else {
					planePos.z = 0;
				}
				*/

				Plane plane = new Plane();
				Quaternion rotation = Quaternion.Euler(xoffset++, yRot + 180, 0);// * Quaternion.Euler(30, 0, 0);
				Vector3 normal = rotation * Vector3.up;
				//Vector3 normal = Quaternion.Euler(0, 90, 0) * Vector3.Cross(rotation * Vector3.up, rotation * Vector3.left);
				plane.normal = normal;
				plane.Translate(planePos);

				Debug.Log(planePos + "," + normal);
				//plane.Translate(Tunnel.transform.position);

				if (Plane != null) {
					Plane.transform.rotation = rotation;
					Plane.transform.position = planePos;
				}

				Slice(plane);
			}

			/*
			Vector4 planePosition = new Vector4(stencilPos.x, stencilPos.y, stencilPos.z, 1);
			Vector3 planeNormal3 = Quaternion.Euler(0, 90, 0) * Vector3.Cross(Stencil.transform.rotation * Vector3.up, Stencil.transform.rotation * Vector3.left);

			Vector4 planeNormal = new Vector4(planeNormal3.x, planeNormal3.y, planeNormal3.z, 1);
			foreach (Material m in crossSectionMaterials) {
				m.SetVector("_PlanePosition", planePosition);
				m.SetVector("_PlaneNormal", planeNormal);
			}
			*/

			showingCrossSection = true;
			oldPosition = transform.position;
			oldRotation = transform.rotation;
		}
	}

	private void Slice(Plane plane) {
		crossSection.transform.position = Tunnel.transform.position;
		crossSection.transform.rotation = Tunnel.transform.rotation;

		foreach (SlicerDetails details in slicers) {
			Debug.Log("Slice: " + details.filter.gameObject);

			details.slicer.slice(plane);
			details.filter.mesh = details.slicer.posMesh;
		}
	}

	private IEnumerator HideCrossSection() {
		Color[] colors = new Color[tunnelMaterials.Count]; 
		for (int i = 0; i < colors.Length; ++i) {
			colors[i] = tunnelMaterials[i].GetColor("_Color");
		}

		if (!Tunnel.activeSelf) {
			Tunnel.SetActive(true);
			for (int i = 0; i < colors.Length; ++i) {
				colors[i].a = 0;
			}
		}

		float da = 1 / FadeDuration;
		bool changedColor = true;
		while (changedColor) {
			changedColor = false;

			for (int i = 0; i < colors.Length; ++i) {
				tunnelMaterials[i].SetColor("_Color", colors[i]);
				if (colors[i].a < 1) {
					colors[i].a = Mathf.Min(1, colors[i].a + (da * Time.deltaTime));
					changedColor = true;
				}
			}

			yield return null;
		}

		crossSection.SetActive(false);
	}

	private IEnumerator ShowCrossSection() {
		Color[] colors = new Color[tunnelMaterials.Count]; 
		for (int i = 0; i < colors.Length; ++i) {
			colors[i] = tunnelMaterials[i].GetColor("_Color");
		}

		if (!crossSection.activeSelf) {
			crossSection.SetActive(true);
			for (int i = 0; i < colors.Length; ++i) {
				colors[i].a = 1;
			}
		}

		float da = 1 / FadeDuration;
		bool changedColor = true;
		while (changedColor) {
			changedColor = false;

			for (int i = 0; i < colors.Length; ++i) {
				tunnelMaterials[i].SetColor("_Color", colors[i]);
				if (colors[i].a > 0) {
					colors[i].a = Mathf.Max(0, colors[i].a - (da * Time.deltaTime));
					changedColor = true;
				}
			}

			yield return null;
		}

		Tunnel.SetActive(false);
	}

	private class SlicerDetails {
		public MeshFilter filter;
		public Slicer slicer;

		public SlicerDetails(MeshFilter filter) {
			this.filter = filter;
			this.slicer = new Slicer(filter.mesh, true, true);
		}
	}
}
