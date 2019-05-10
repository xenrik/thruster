using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CrossSectionController : MonoBehaviour {

	public GameObject Thruster;
	public GameObject Tunnel;
	public GameObject Stencil;

	public Shader CrossSectionShader;

	private List<Material> crossSectionMaterials;
	
	// Use this for initialization
	void Start () {
		// On the tunnel, find all renderers and from them all materials that have the cross section shader
		Renderer[] renderers = Tunnel.GetComponentsInChildren<Renderer>();

		// Collect the materials
		var materials = new List<Material>();
		foreach (Renderer r in renderers) {
			materials.AddRange(r.materials);
		}

		// Filter to only those we want
		crossSectionMaterials = materials.Where(m => m.shader != null && m.shader.name == CrossSectionShader.name).ToList();
	}
	
	// Update is called once per frame
	void Update () {
		if (Thruster == null) {
			return;
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
