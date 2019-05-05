using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepPosition : MonoBehaviour {

	public PIDControllerFloat PositionController = new PIDControllerFloat();
	public float TargetPosition;

	private Rigidbody body;

	void Start () {
		body = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float error = body.transform.position.x - TargetPosition;
        float correction = PositionController.Update(error, Time.fixedDeltaTime);

		Vector3 force = Vector3.left * correction;
		Debug.DrawRay(transform.position, force * 10, Color.red);
		
		body.AddForce(force, ForceMode.Force);
	}
}
