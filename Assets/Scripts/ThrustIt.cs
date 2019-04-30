using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrustIt : MonoBehaviour {

	public float Force;

	private Rigidbody rigid;

	// Use this for initialization
	void Start () {
		rigid = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		Vector3 thrust = (Vector3.up * Force * Input.GetAxis("Vertical")) +
			(-Vector3.forward * Force * Input.GetAxis("Horizontal"));
		rigid.AddForce(thrust);
	}
}
