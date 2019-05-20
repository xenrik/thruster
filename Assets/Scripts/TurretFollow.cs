using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretFollow : MonoBehaviour {

	public GameObject Target;

	// Update is called once per frame
	void Update () {
		transform.LookAt(Target.transform);
	}
}
