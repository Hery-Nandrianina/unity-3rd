using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere : Action
{
    [Header("Sphere")]
    public float hitForce;
    public Vector3 force, origin;
    bool picked, potential;

    Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    public void Hit(Vector3[] data) {
        rb.AddForceAtPosition(data[1] * hitForce, data[0]);
    }

    public override void Act() {
        base.Act();

        picked = true;
        transform.parent = player.transform.GetChild(0);
        rb.isKinematic = true;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = origin;
    }

    public override void Step() {
        if(picked && Input.GetKeyDown(KeyCode.A)) {
            picked = false;
            rb.isKinematic = false;
            rb.AddRelativeForce(force);
            transform.parent = null;
        }

        if(potential && rb.velocity.magnitude < 0.6f) {
            potential = false;
            enable = true;
        }
    }

    private void OnCollisionEnter(Collision other) {
        // picked = false;
        // rb.isKinematic = false;
        // transform.parent = null;
        potential = true; //Wait real Ground
    }
}
