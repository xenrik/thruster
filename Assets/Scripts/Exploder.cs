using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Exploder : MonoBehaviour {

	public GameObject explosionPrefab;
	public string[] ForbiddenTags;
	public string[] AllowedTags;

	public float ExplosionRadius;
	public float ExplosionForce;

	private new Renderer renderer;
	private new Collider collider;

	private void Start() {
		renderer = GetComponent<Renderer>();
		collider = GetComponent<Collider>();
	}

	private void OnCollisionEnter(Collision other) {
		bool collide = ForbiddenTags.Length > 0 && ForbiddenTags.Any(tag => other.gameObject.CompareTag(tag));
		collide = collide || AllowedTags.Length > 0 && !AllowedTags.Any(tag => other.gameObject.CompareTag(tag));

		if (collide) {
			Debug.Log("Collision with: " + other.gameObject + " (" + other.gameObject.tag + ")");

			renderer.enabled = false;
			collider.enabled = false;

			GameObject explosion = Instantiate(explosionPrefab);
			explosion.transform.position = transform.position;

			if (ExplosionForce != 0 && ExplosionRadius != 0) {
				Collider[] allAffected = Physics.OverlapSphere(transform.position, ExplosionRadius);
				foreach (Collider affected in allAffected) {
					Rigidbody body = affected.GetComponent<Rigidbody>();
					if (body != null) {
						Debug.Log("Explosion Hit: " + body.gameObject);
						body.AddExplosionForce(ExplosionForce, transform.position, ExplosionRadius);
					}
				}
			}

			Destroy(gameObject);
		}
	}
}
