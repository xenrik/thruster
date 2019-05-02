using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : MonoBehaviour {

	public float RechargeSpeed;
	public float DischargeSpeed;
	public float RechargeDelay;

	public float CollisionForce;

	private MeshRenderer shieldRenderer;
	private Outline shieldOutline;

	private Color shieldColor;
	private float shieldValue = 1.0f;
	
	private bool colliding;	
	private float startRecharging;

	// Use this for initialization
	void Start () {
		shieldRenderer = GetComponent<MeshRenderer>();
		shieldRenderer.enabled = false;

		shieldOutline = GetComponent<Outline>();

		shieldColor = shieldOutline.OutlineColor;
		shieldColor.a = 1;
		shieldOutline.OutlineColor = shieldColor;
	}

	void Update() {
		if (colliding) {
			shieldValue = Mathf.Max(0f, shieldValue - (1 / DischargeSpeed) * Time.deltaTime);
			startRecharging = Time.time + RechargeDelay;
		} else if (shieldValue < 1 && startRecharging < Time.time) {
			shieldValue = Mathf.Max(0f, shieldValue + (1 / RechargeSpeed) * Time.deltaTime);
		}

		if (shieldValue == 0) {
			shieldRenderer.enabled = false;
			shieldOutline.enabled = false;
		} else if (shieldValue < 0.1f) {
			shieldRenderer.enabled = Random.Range(0, 5) == 0;
			shieldRenderer.material.SetTextureOffset("_MainTex", new Vector2(Time.time, Time.time / 2));

			shieldOutline.enabled = shieldRenderer.enabled;
			shieldColor.a = 1;
			shieldOutline.OutlineColor = shieldColor;
		} else if (shieldValue < 1.0f || shieldColor.a < 0.99f) {
			shieldRenderer.enabled = true;
			shieldRenderer.material.SetTextureOffset("_MainTex", new Vector2(Time.time, Time.time / 2));

			shieldOutline.enabled = true;
			shieldOutline.OutlineColor = shieldColor;
			shieldColor.a = Mathf.Lerp(shieldColor.a, shieldValue, Time.deltaTime);
		} else {
			shieldRenderer.enabled = false;
			shieldOutline.enabled = false;
		}
 	}

	void OnCollisionEnter(Collision collision) {
		Debug.Log("Collision Speed: " + collision.relativeVelocity.magnitude + " - with: " + collision.collider);
		if (collision.relativeVelocity.magnitude > CollisionForce) {
			shieldValue *= 0.5f;
			colliding = true;
		}
	}

	void OnCollisionExit(Collision collision) {
		Debug.Log("Exit: " + collision.collider);
		colliding = false;
	}
}
