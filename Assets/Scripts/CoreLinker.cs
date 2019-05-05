using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreLinker : MonoBehaviour {

	private GameObject thruster;

	private GameObject currentTarget;
	private ConfigurableJoint joint;

	private LineRenderer line;

	// Use this for initialization
	void Start () {
		thruster = gameObject.transform.parent.gameObject;

		line = GetComponent<LineRenderer>();
		line.positionCount = 2;
		line.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space)) {
			if (joint != null) {
				Destroy(joint);
				line.enabled = false;
			} else if (currentTarget != null) {
				joint = thruster.AddComponent<ConfigurableJoint>();
				joint.connectedBody = currentTarget.GetComponentInParent<Rigidbody>();

				joint.anchor = new Vector3(0, -0.4f, 0);

				joint.xMotion = ConfigurableJointMotion.Limited;
				joint.yMotion = ConfigurableJointMotion.Limited;
				joint.zMotion = ConfigurableJointMotion.Limited;
				
				SoftJointLimit limit = new SoftJointLimit();
				limit.limit = (currentTarget.transform.position - gameObject.transform.position).magnitude / 2.0f;
				joint.linearLimit = limit;

				/*
				SoftJointLimitSpring spring = new SoftJointLimitSpring();
				spring.spring = 10f;
				joint.linearLimitSpring = spring;
				*/
				
				line.enabled = true;
			}
		}

		if (line.enabled) {
			line.SetPosition(0, gameObject.transform.position);
			line.SetPosition(1, joint.connectedBody.transform.position);
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Core")) {
			currentTarget = other.gameObject;
			Debug.Log("Current Target: " + currentTarget);
		}
	}

	private void OnTriggerExit(Collider other) {
		if (other == currentTarget) {
			Debug.Log("Current Target: <lost>");
			currentTarget = null;
		}
	}
}
