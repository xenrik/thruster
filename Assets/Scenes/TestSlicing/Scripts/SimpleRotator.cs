using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotator : MonoBehaviour {
    public Vector3 Rotation;
    public float Speed;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {       
        transform.rotation = transform.rotation * (Quaternion.Euler(Rotation * 1 / Speed * Time.deltaTime));
    }
}
