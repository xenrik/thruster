using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrustIt : MonoBehaviour {

	public float VerticalForce;

	public float LateralForce;
	public float TurnForce;

	public bool TurnSupported;

	private Rigidbody rigid;


	// Use this for initialization
	void Start () {
		rigid = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float thrust = VerticalForce * Input.GetAxis("Vertical");
		float lateral = LateralForce * Input.GetAxis("Horizontal");
		float turn = TurnForce * Input.GetAxis("Horizontal");
		
		if (TurnSupported) {
			rigid.AddTorque(Vector3.left * turn);
			rigid.AddForce(transform.up * thrust);
		} else {
			rigid.AddForce((Vector3.up * thrust) + (Vector3.left * lateral));
		}
			
	}
}
