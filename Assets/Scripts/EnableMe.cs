using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableMe : MonoBehaviour {

	public GameObject Target;
	
	private int triggerDepth;

	private void OnTriggerEnter(Collider other) {
		Target.SetActive(true);
		++triggerDepth;
	}

	private void OnTriggerExit(Collider other) {
		--triggerDepth;
		if (triggerDepth == 0) {
			Target.SetActive(false);	
		}
	}
}
