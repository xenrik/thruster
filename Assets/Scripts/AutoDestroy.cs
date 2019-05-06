using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AutoDestroy : MonoBehaviour {
    private ParticleSystem[] particleSystems;

    // Use this for initialization
    void Start () {
		particleSystems = GetComponentsInChildren<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update () {
		if (particleSystems.Any(ps => ps.IsAlive())) {
			return;
		}

		Debug.Log("Destroyed!");
		Destroy(gameObject);
	}
}
