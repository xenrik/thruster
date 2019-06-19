using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BzKovSoft.ObjectSlicer;

public class CrossSectionController4 : MonoBehaviour {

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


	// Use this for initialization
	void Start () {
		// Collect the materials for the tunnel
		tunnelMaterials = new List<Material>();
		Renderer[] renderers = Tunnel.GetComponentsInChildren<Renderer>();
		foreach (Renderer r in renderers) {
			tunnelMaterials.AddRange(r.materials);
		}
	}
	
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
			if (offset.magnitude > PositionTolerance || rotationOffset > RotationTolerance || !showingCrossSection) {
				GameObject camera = gameObject;

				float yRot = camera.transform.rotation.eulerAngles.y;
				yRot = (Mathf.RoundToInt(yRot / 90) % 4) * 90;
				bool showingFront = yRot == 0 || yRot == 180;

				Vector3 planePos = Thruster.transform.position;
				planePos.y = 0;
				if (showingFront) {
					planePos.x = 0;
				} else {
					planePos.z = 0;
				}

				Quaternion rotation = Quaternion.Euler(0, yRot + 180, 0) ; //* Quaternion.Euler(90, 0, 0);

				Plane plane = new Plane();
				Vector3 normal = Quaternion.Euler(0, 90, 0) * Vector3.Cross(rotation * Vector3.up, rotation * Vector3.left);
				plane.SetNormalAndPosition(normal, planePos);

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
		if (crossSection != null) {
			Destroy(crossSection);
		}
		
		crossSection = Instantiate(Tunnel);
		crossSection.name += " (CrossSection)";

		foreach (IBzSliceableNoRepeat sliceable in crossSection.GetComponentsInChildren<IBzSliceableNoRepeat>()) {
			sliceable.Slice(plane, 0, OnSliceFinished);
		}
	}

	private void OnSliceFinished(BzSliceTryResult result) {
		Debug.Log("Result: " + result.sliced + " - " + result.outObjectNeg + "," + result.outObjectPos);
		if (result.sliced) {
			Destroy(result.outObjectPos);
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

		if (crossSection != null) {
			crossSection.SetActive(false);
		}
	}

	private IEnumerator ShowCrossSection() {
		Color[] colors = new Color[tunnelMaterials.Count]; 
		for (int i = 0; i < colors.Length; ++i) {
			colors[i] = tunnelMaterials[i].GetColor("_Color");
		}

		if (crossSection != null && !crossSection.activeSelf) {
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
}
