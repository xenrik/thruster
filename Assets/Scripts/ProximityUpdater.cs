using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityUpdater : MonoBehaviour {

	public GameObject Target;

	private Material[] materials;

	// Use this for initialization
	void Start () {
		materials = GetComponent<Renderer>().materials;
	}
	
	// Update is called once per frame
	void Update () {
		if (Target == null) {
			return;
		}

		Vector3 pos3 = Target.transform.position;
		Vector4 pos = new Vector4(pos3.x, pos3.y, pos3.z, 1);
		foreach (Material m in materials) {
			m.SetVector("_Position", pos);
		}
	}
}
