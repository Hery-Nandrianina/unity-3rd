using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public LayerMask mask;
    public GameObject fx;

    private bool cancel;
    Melee melee;

    private void Start() {
        // for (int i = 0; i < 32; i++) {
        //     if (mask != (mask | (1 << i)))
        //         Physics.IgnoreLayerCollision(gameObject.layer, i);
        // }

        melee = GetComponentInParent<Melee>();
    }

    public void Hit() {
        RaycastHit hit;
        Vector3 dir = -transform.up;
        if(Physics.Raycast(transform.position - dir*.5f, dir, out hit, 2, mask)) {
            melee.Cancel();
            Instantiate(fx, hit.point, Quaternion.identity);
        }
    }

    private void OnDrawGizmos() {
        Gizmos.DrawLine(transform.position, transform.position-transform.up*2);
    }
}
