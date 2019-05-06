﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public GameObject Target;

	public Vector3 Offset;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Target == null) {
			return;
		}
		
		Vector3 position = Vector3.Lerp(transform.position, Target.transform.position + Offset, 0.9f);
		transform.position = position;

		gameObject.transform.LookAt(Target.transform.position, Vector3.up);
	}
}
