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

	public Shader CrossSectionShader;
	public float FadeDuration;

	private List<Material> crossSectionMaterials;
	private List<Material> aboveSurfaceMaterials;

	private bool showingCrossSection = false;

	// Use this for initialization
	void Start () {
		// On the tunnel, find all renderers and from them all materials that have the cross section shader
		Renderer[] renderers = TunnelCrossSection.GetComponentsInChildren<Renderer>();

		// Collect the materials
		var materials = new List<Material>();
		foreach (Renderer r in renderers) {
			materials.AddRange(r.materials);
		}

		// Filter to only those we want
		crossSectionMaterials = materials.Where(m => m.shader != null && m.shader.name == CrossSectionShader.name).ToList();

		// Collect the materials for the above surface renderers
		aboveSurfaceMaterials = new List<Material>();
		renderers = TunnelAboveSurface.GetComponentsInChildren<Renderer>();
		foreach (Renderer r in renderers) {
			aboveSurfaceMaterials.AddRange(r.materials);
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

				showingCrossSection = true;
			}
		
			Vector3 stencilPos = Thruster.transform.position;
			stencilPos.y = 0;
			stencilPos.z = 0;

			Stencil.transform.position = stencilPos;

			Vector4 planePosition = new Vector4(stencilPos.x, 0, 0, 1);
			foreach (Material m in crossSectionMaterials) {
				m.SetVector("_PlanePosition", planePosition);
			}
		}
	}

	private IEnumerator HideCrossSection() {
		Debug.Log("Hiding Cross Section");

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

		Debug.Log("Finished");
		TunnelCrossSection.SetActive(false);
	}

	private IEnumerator ShowCrossSection() {
		Debug.Log("Showing Cross Section");

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

		Debug.Log("Finished");
		TunnelAboveSurface.SetActive(false);
	}
}
