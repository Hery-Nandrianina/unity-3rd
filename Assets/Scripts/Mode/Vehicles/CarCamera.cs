using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCamera : MonoBehaviour
{
    public Vector3 offset;
    public Transform target;
    // public float speed;

    private void LateUpdate() {
        transform.position = target.position + ( Quaternion.Euler(0, target.rotation.eulerAngles.y - 90, 0) * offset);
        transform.LookAt(target);
    }
}
