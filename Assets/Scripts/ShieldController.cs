using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShieldController : MonoBehaviour {

	public float ShieldMax = 100;

	public float RechargeSpeed;
	public float RechargeDelay;

	public float CollisionTriggerForce;
	public float CollisionTriggerMaxForce;

	public float CollisionEffect;

	public ForceMode BounceType;
	public float BounceMultiplier;

	public RectTransform ShieldBar;

	private MeshRenderer shieldRenderer;
	private Outline shieldOutline;
    private PhysicMaterial shieldPhyMaterial;

	public GameObject Target;
	private Rigidbody targetBody;

	private Color shieldColor;
	private float shieldValue;

	private float startRecharging;

	private Coroutine shieldAnimator;

	// Use this for initialization
	void Start () {
		shieldValue = ShieldMax;

		shieldRenderer = GetComponentInChildren<MeshRenderer>();
		shieldRenderer.enabled = false;

		shieldOutline = GetComponentInChildren<Outline>();
		shieldColor = shieldOutline.OutlineColor;
		shieldColor.a = 1;
		shieldOutline.OutlineColor = shieldColor;

		shieldPhyMaterial = GetComponentInChildren<Collider>().material;
		
		targetBody = Target.GetComponent<Rigidbody>();

		StartCoroutine(AnimateShieldBar(shieldValue));
	}

	void Update() {
		if (Target == null) {
			return;
		}

		transform.position = Target.transform.position;
		transform.rotation = Target.transform.rotation;
		
		if (shieldValue < ShieldMax && startRecharging < Time.time) {
			shieldValue = Mathf.Min(ShieldMax, shieldValue + (RechargeSpeed * Time.deltaTime));

			shieldColor.a = shieldValue / ShieldMax;
			shieldOutline.OutlineColor = shieldColor;
		}
 	}

	void OnCollisionEnter(Collision collision) {
		if (targetBody == null || collision.gameObject == Target) {
			return;
		}
		
		float force =  targetBody.velocity.magnitude;
		Debug.Log("Collision Speed: " + force + " - with: " + collision.collider);
		if (force > CollisionTriggerForce && shieldValue > 0) {
			startRecharging = Time.time + RechargeDelay;

			float oldValue = shieldValue;
			float p = (Mathf.Min(force, CollisionTriggerMaxForce) - CollisionTriggerForce) / (CollisionTriggerMaxForce - CollisionTriggerForce);
			float reduction = (ShieldMax - CollisionEffect) * p;
			shieldValue = Mathf.Max(0, shieldValue - reduction);

			Vector3 bounce = collision.contacts[0].normal * force * (1 + shieldPhyMaterial.bounciness) * BounceMultiplier;
			targetBody.AddForce(bounce, BounceType);

			if (shieldAnimator != null) {
				StopCoroutine(shieldAnimator);
			}
			shieldAnimator = StartCoroutine(AnimateShield(oldValue, shieldValue));
		}
	}

	IEnumerator AnimateShield(float oldValue, float newValue) {
		shieldColor.a = oldValue;

		shieldOutline.OutlineColor = shieldColor;
		shieldOutline.enabled = true;

		shieldRenderer.material.SetColor("_Color", shieldColor);
		shieldRenderer.transform.localScale = Vector3.one;
		shieldRenderer.enabled = true;

		float ttl = 0.1f;
		bool explode = false;
		if (newValue == 0) {
			oldValue = ShieldMax;
			newValue = 0;
			explode = true;
		}

		float t = 0;
		while (t < ttl) {
			float p = t / ttl;
			float a = Mathf.Lerp(oldValue, newValue, p) / ShieldMax;

			shieldColor.a = a;
			shieldOutline.OutlineColor = shieldColor;
			shieldRenderer.material.SetColor("_Color", shieldColor);
			shieldRenderer.material.SetTextureOffset("_MainTex", new Vector2(Time.time, Time.time / 2));

			if (explode) {
				shieldRenderer.transform.localScale = Vector3.one * (1 + p);
			}

			yield return null;
			t += Time.deltaTime;
		}

		shieldOutline.enabled = false;
		shieldRenderer.enabled = false;
	}

	IEnumerator AnimateShieldBar(float newValue) {
		float startValue = -1;
		float targetValue = shieldValue;
		float currentValue = shieldValue;

		float ttl = 1;
		float t = 0;
		Vector3 scale = Vector3.one;
		while (true) {
			if (targetValue != shieldValue) {
				targetValue = shieldValue;
				if (startValue == -1) {
					startValue = currentValue;
					t = 0;
				}
			}

			if (currentValue != targetValue) {
				float p = t / ttl;
				currentValue = Mathf.Lerp(startValue, targetValue, p);

				scale.x = currentValue / ShieldMax;
				ShieldBar.transform.localScale = scale;
			} else {
				startValue = -1;
			}

			yield return null;
			t += Time.deltaTime;
		}
	}
}
