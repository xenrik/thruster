using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CrossSectionController2 : MonoBehaviour {

	public GameObject Thruster;
	public GameObject SurfaceLevel;

	public GameObject TunnelCrossSection;
	public GameObject TunnelAboveSurface;

	public GameObject Stencil;

	public Shader[] CrossSectionShaders;

	public float FadeDuration;

	private List<Material> crossSectionMaterials;
	private List<Material> aboveSurfaceMaterials;

	private bool showingCrossSection = false;
	private Quaternion stencilRotationOffset;

	// Use this for initialization
	void Start () {
		// On the tunnel, find all renderers and from them all materials that have the cross section shader
		Renderer[] renderers = TunnelCrossSection.GetComponentsInChildren<Renderer>();
		Debug.Log("Found " + renderers.Length + " renderers");

		// Collect the materials
		var materials = new List<Material>();
		foreach (Renderer r in renderers) {
			materials.AddRange(r.materials);
		}
		Debug.Log("Found " + materials.Count + " materials");

		// Filter to only those we want
		crossSectionMaterials = materials.Where(m => m.shader != null && 
			CrossSectionShaders.Any(s => s.name.Equals(m.shader.name))).ToList();

		Debug.Log("Found " + crossSectionMaterials.Count + " cross-sections materials");

		// Collect the materials for the above surface renderers
		aboveSurfaceMaterials = new List<Material>();
		renderers = TunnelAboveSurface.GetComponentsInChildren<Renderer>();
		foreach (Renderer r in renderers) {
			aboveSurfaceMaterials.AddRange(r.materials);
		}

		stencilRotationOffset = Stencil.transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		if (Thruster == null) {
			return;
		}

/*
		if (Thruster.transform.position.y > SurfaceLevel.transform.position.y) {
			if (showingCrossSection) {
				StopAllCoroutines();
				StartCoroutine(HideCrossSection());

				showingCrossSection = false;
			}
		} else {*/
			if (!showingCrossSection) {
				StopAllCoroutines();
				StartCoroutine(ShowCrossSection());

				showingCrossSection = true;
			}
		
			GameObject camera = gameObject;
			float yRot = camera.transform.rotation.eulerAngles.y;
			yRot = (Mathf.RoundToInt(yRot / 90) % 4) * 90;
			bool showingFront = yRot == 0 || yRot == 180;

			Vector3 stencilPos = Thruster.transform.position;
			stencilPos.y = 0;
			if (showingFront) {
				stencilPos.x = 0;
			} else {
				stencilPos.z = 0;
			}

			Stencil.transform.position = stencilPos;
			Stencil.transform.rotation = Quaternion.Euler(0, yRot + 180, 0) * stencilRotationOffset;

			Vector4 planePosition = new Vector4(stencilPos.x, stencilPos.y, stencilPos.z, 1);
			Vector3 planeNormal3 = Quaternion.Euler(0, 90, 0) * Vector3.Cross(Stencil.transform.rotation * Vector3.up, Stencil.transform.rotation * Vector3.left);
//			Vector3 planeNormal3 = Stencil.transform.rotation * Vector3.forward;

			Vector4 planeNormal = new Vector4(planeNormal3.x, planeNormal3.y, planeNormal3.z, 1);
			foreach (Material m in crossSectionMaterials) {
				m.SetVector("_PlanePosition", planePosition);
				m.SetVector("_PlaneNormal", planeNormal);
			}
//		}
	}

	private IEnumerator HideCrossSection() {
		Color[] colors = new Color[aboveSurfaceMaterials.Count]; 
		for (int i = 0; i < colors.Length; ++i) {
			colors[i] = aboveSurfaceMaterials[i].GetColor("_Color");
		}

		if (!TunnelAboveSurface.activeSelf) {
			TunnelAboveSurface.SetActive(true);
			for (int i = 0; i < colors.Length; ++i) {
				colors[i].a = 0;
			}
		}

		float da = 1 / FadeDuration;
		bool changedColor = true;
		while (changedColor) {
			changedColor = false;

			for (int i = 0; i < colors.Length; ++i) {
				aboveSurfaceMaterials[i].SetColor("_Color", colors[i]);
				if (colors[i].a < 1) {
					colors[i].a = Mathf.Min(1, colors[i].a + (da * Time.deltaTime));
					changedColor = true;
				}
			}

			yield return null;
		}

		TunnelCrossSection.SetActive(false);
	}

	private IEnumerator ShowCrossSection() {
		Color[] colors = new Color[aboveSurfaceMaterials.Count]; 
		for (int i = 0; i < colors.Length; ++i) {
			colors[i] = aboveSurfaceMaterials[i].GetColor("_Color");
		}

		if (!TunnelCrossSection.activeSelf) {
			TunnelCrossSection.SetActive(true);
			for (int i = 0; i < colors.Length; ++i) {
				colors[i].a = 1;
			}
		}

		float da = 1 / FadeDuration;
		bool changedColor = true;
		while (changedColor) {
			changedColor = false;

			for (int i = 0; i < colors.Length; ++i) {
				aboveSurfaceMaterials[i].SetColor("_Color", colors[i]);
				if (colors[i].a > 0) {
					colors[i].a = Mathf.Max(0, colors[i].a - (da * Time.deltaTime));
					changedColor = true;
				}
			}

			yield return null;
		}

		TunnelAboveSurface.SetActive(false);
	}
}
