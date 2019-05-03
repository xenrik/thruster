using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : MonoBehaviour {

	public float RechargeSpeed;
	public float DischargeSpeed;
	public float RechargeDelay;

	public float CollisionForce;

	public ForceMode BounceType;
	public float BounceMultiplier;

	private MeshRenderer shieldRenderer;
	private Outline shieldOutline;
	private Vector3 shieldOffset;
    private Rigidbody shieldBody;
    private PhysicMaterial shieldPhyMaterial;

	public GameObject Target;
	private Rigidbody targetBody;

	//private FixedJoint joint;
	//private Rigidbody jointConnectedBody;

	private Color shieldColor;
	private float shieldValue = 1.0f;

	private bool colliding;	
	private float startRecharging;

	// Use this for initialization
	void Start () {
		shieldBody = GetComponent<Rigidbody>();
		
		shieldRenderer = GetComponent<MeshRenderer>();
		shieldRenderer.enabled = false;

		shieldOutline = GetComponent<Outline>();
		shieldColor = shieldOutline.OutlineColor;
		shieldColor.a = 1;
		shieldOutline.OutlineColor = shieldColor;

		//joint = gameObject.AddComponent<FixedJoint>();
		//joint.connectedBody = Target.GetComponent<Rigidbody>();

		shieldOffset = Target.transform.position - transform.position;
		shieldPhyMaterial = GetComponent<Collider>().material;
		
		targetBody = Target.GetComponent<Rigidbody>();
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

		if (colliding && shieldValue > 0) {
			shieldValue = Mathf.Max(0f, shieldValue - (1 / DischargeSpeed) * Time.deltaTime);
			startRecharging = Time.time + RechargeDelay;
		} else if (!colliding && shieldValue < 1 && startRecharging < Time.time) {
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
		float force =  targetBody.velocity.magnitude;
		Debug.Log("Collision Speed: " + force + " - with: " + collision.collider);
		if (force > CollisionForce) {
			shieldValue *= 0.5f;
			colliding = true;

			Vector3 bounce = collision.contacts[0].normal * force * (1 + shieldPhyMaterial.bounciness) * BounceMultiplier;
			targetBody.AddForce(bounce, BounceType);
		}
	}

	void OnCollisionExit(Collision collision) {
		colliding = false;
	}
}
