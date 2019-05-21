using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretShoot : MonoBehaviour {

	public GameObject BulletPrefab;
	public float Offset;

	public float ShootFrequency = 1;
	public float BulletForce = 1;
	public float BulletTtl = 1;

	private float nextShot;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		nextShot -= Time.deltaTime;
		if (nextShot < 0) {
			StartCoroutine(ShootBullet());
			nextShot = ShootFrequency;
		}
	}

	private IEnumerator ShootBullet() {
		GameObject bullet = Instantiate(BulletPrefab);
		Rigidbody bulletBody = bullet.GetComponent<Rigidbody>();
		bullet.transform.position = transform.position + (transform.forward * Offset);
		bullet.transform.rotation = transform.rotation;
		bulletBody.AddForce(bullet.transform.forward * BulletForce, ForceMode.VelocityChange);

		float ttl = BulletTtl;
		while (ttl > 0) {
			if (bullet == null) {
				yield break;
			}
			
			bulletBody.AddForce(bullet.transform.forward * BulletForce);
			yield return null;

			ttl -= Time.deltaTime;
		}

		Destroy(bullet);
	}
}
