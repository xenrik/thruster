using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelfRighting : MonoBehaviour {

	public PIDControllerVector3 HeadingController = new PIDControllerVector3();
    public PIDControllerVector3 RotationController = new PIDControllerVector3();

	public bool CorrectHeading = true;
	public bool CorrectRotation = true;

	private Rigidbody body;

	// Use this for initialization
	void Start () {
		body = GetComponent<Rigidbody>();
	}
	/*
	string FormatVector(Vector3 v) {
		return "[" + v.x.ToString("0.000") + "," + v.y.ToString("0.000") + "," + v.z.ToString("0.000") + "]";
	}
	*/

	// Update is called once per frame
	void FixedUpdate () {
		Vector3 targetRotation = -Vector3.left;
		Vector3 rotationError = Vector3.Cross(body.transform.forward, targetRotation);
        Vector3 rotationCorrection = RotationController.Update(rotationError, Time.fixedDeltaTime);

        Vector3 targetHeading = Vector3.up;
        Vector3 headingError = Vector3.Cross(body.transform.up, targetHeading);
        Vector3 headingCorrection = HeadingController.Update(headingError, Time.fixedDeltaTime);

        Vector3 torque = (CorrectRotation ? rotationCorrection : Vector3.zero) + 
			(CorrectHeading ? headingCorrection : Vector3.zero);

		body.AddTorque(torque, ForceMode.Force);
	}
}
