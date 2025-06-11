using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public static GameObject target;

    GameObject player;

    private void Start() {
        player = GameObject.FindGameObjectWithTag("Player");    
    }

    void OnTriggerEnter(Collider other) {
        // Debug.Log("Trigger enter!");
        if(other.CompareTag("Player"))
            if(target == null) {
                target = gameObject;
                player.transform.parent = transform;
            }
    }

    private void OnTriggerExit(Collider other) {
        if(other.CompareTag("Player")) {
            if (target == gameObject) target = null;
            player.transform.parent = null;
        }
    }

}
