using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrustIt : MonoBehaviour {

	public float VerticalForce;
	public float LateralForce;

	private Rigidbody rigid;

	// Use this for initialization
	void Start () {
		rigid = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		Vector3 thrust = (Vector3.up * VerticalForce * Input.GetAxis("Vertical")) +
			(-Vector3.forward * LateralForce * Input.GetAxis("Horizontal"));
		rigid.AddForce(thrust);
	}
}
