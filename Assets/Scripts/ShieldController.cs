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
	private Vector3 shieldOffset;
    private Rigidbody shieldBody;
    private PhysicMaterial shieldPhyMaterial;

	public GameObject Target;
	private Rigidbody targetBody;

	//private FixedJoint joint;
	//private Rigidbody jointConnectedBody;

	private Material shieldMaterial;
	private Color shieldColor;
	private float shieldValue;

	private bool colliding;	
	private float startRecharging;

	private Coroutine shieldAnimator;
	private Coroutine shieldBarAnimator;
	private float shieldBarValue;

	// Use this for initialization
	void Start () {
		shieldValue = ShieldMax;
		shieldBarValue = ShieldMax;

		shieldBody = GetComponent<Rigidbody>();
		
		shieldRenderer = GetComponentInChildren<MeshRenderer>();
		shieldRenderer.enabled = false;
		shieldMaterial = shieldRenderer.material;

		shieldOutline = GetComponentInChildren<Outline>();
		shieldColor = shieldOutline.OutlineColor;
		shieldColor.a = 1;
		shieldOutline.OutlineColor = shieldColor;

		//joint = gameObject.AddComponent<FixedJoint>();
		//joint.connectedBody = Target.GetComponent<Rigidbody>();

		shieldOffset = Target.transform.position - transform.position;
		shieldPhyMaterial = GetComponentInChildren<Collider>().material;
		
		targetBody = Target.GetComponent<Rigidbody>();

		shieldBarAnimator = StartCoroutine(AnimateShieldBar(shieldValue));
	}

	string lastMessage =  "";
	void Log(string message) {
		if (!lastMessage.Equals(message)) {
			Debug.Log(message);
			lastMessage = message;
		}
	}

	void Update() {
		transform.position = Target.transform.position - shieldOffset;
		transform.rotation = Target.transform.rotation;
		
		/*/
		if (colliding && shieldValue > 0) {
			shieldValue = Mathf.Max(0f, shieldValue - (1 / DischargeSpeed) * Time.deltaTime);
			startRecharging = Time.time + RechargeDelay;
		} else 
		*/

		if (shieldValue < ShieldMax && startRecharging < Time.time) {
			shieldValue = Mathf.Min(ShieldMax, shieldValue + (RechargeSpeed * Time.deltaTime));

			shieldColor.a = shieldValue / ShieldMax;
			shieldOutline.OutlineColor = shieldColor;
		}

		/*
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
		 */
 	}

	void OnCollisionEnter(Collision collision) {
		float force =  targetBody.velocity.magnitude;
		Debug.Log("Collision Speed: " + force + " - with: " + collision.collider);
		if (force > CollisionTriggerForce && shieldValue > 0) {
			colliding = true;
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

	void OnCollisionExit(Collision collision) {
		colliding = false;
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
