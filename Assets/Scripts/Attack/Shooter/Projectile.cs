using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float force = 0.1f;
    public bool raycast;
    public LayerMask mask;

    [HideInInspector] public Vector3 forward;
	[HideInInspector] public float range;
    private Vector3 start;
    public GameObject fx;

	Rigidbody rb;

    private void Start() {
        start = transform.position;

        if(!raycast) {
            for (int i = 0; i < 32; i++) {
                if (mask != (mask | (1 << i)))
                    Physics.IgnoreLayerCollision(gameObject.layer, i);
            } 
            rb = GetComponent<Rigidbody> ();
        } else {
            RaycastHit hit;
            if(Physics.Raycast(start, forward, out hit, range, mask))
                Hit(hit.point, forward, hit.collider.gameObject);
            Destroy(gameObject);
        }
    }

	void Update(){
		if(Vector3.Distance(transform.position, start) > range)
			Destroy (gameObject);
		
        rb.AddForce(forward * force, ForceMode.VelocityChange);
	}

	void OnCollisionEnter(Collision other) {
        Hit(other.GetContact(0).point, forward, other.gameObject);
		Destroy (gameObject);
	}

    void Hit(Vector3 point, Vector3 direction, GameObject other) {
        if(fx)
            Instantiate(fx, point, Quaternion.identity);
        if(other.layer == LayerMask.NameToLayer("Target")) {
            Vector3[] data = {point, direction};
            other.SendMessage("Hit", data, SendMessageOptions.DontRequireReceiver);
        }
    }
}
