using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CrossSectionController : MonoBehaviour {

	public GameObject SurfaceLevel;

	public GameObject Thruster;
	public GameObject Tunnel;
	public GameObject Stencil;
	
	private CrossSectionMaterial[] materials;
	private Renderer[] renderers;

	private bool showingCrossSectionMaterial = false;

	// Use this for initialization
	void Start () {
		// On the tunnel, find all renderers and from them all materials that have the cross section shader
		materials = Tunnel.GetComponentsInChildren<CrossSectionMaterial>();
		renderers = new Renderer[materials.Length];
		for (int i = 0; i < materials.Length; ++i) {
			renderers[i] = materials[i].gameObject.GetComponent<Renderer>();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Thruster == null) {
			return;
		}

		if (Thruster.transform.position.y > SurfaceLevel.transform.position.y) {
			if (showingCrossSectionMaterial) {
				for (int i = 0; i < renderers.Length; ++i) {
					renderers[i].material = materials[i].Standard;
				}

				Stencil.SetActive(false);
				showingCrossSectionMaterial = false;
			}
		} else {
			if (!showingCrossSectionMaterial) {
				for (int i = 0; i < renderers.Length; ++i) {
					renderers[i].material = materials[i].CrossSection;
				}
				
				Stencil.SetActive(true);
				showingCrossSectionMaterial = true;
			}

			Vector3 stencilPos = Thruster.transform.position;
			stencilPos.y = 0;
			stencilPos.z = 0;

			Stencil.transform.position = stencilPos;

			Vector4 planePosition = new Vector4(stencilPos.x, 0, 0, 1);
			foreach (CrossSectionMaterial m in materials) {
				m.CrossSection.SetVector("_PlanePosition", planePosition);
			}
		}
	}
}
