using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnContact : MonoBehaviour {
	private bool destroyMe = false;

	private void LateUpdate() {
		if (destroyMe) {
			Destroy(gameObject);
		}
	}

	private void OnCollisionEnter(Collision other) {
		destroyMe = true;
	}
}
