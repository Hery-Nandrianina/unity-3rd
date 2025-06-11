using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Action : MonoBehaviour
{
    public bool enable = true, dontAsk;
    [Space]
    public bool useCollider;
    public float radius;
    public bool drawGizmos;

    [Header("Debug")]
    public bool inside;
    [Space]
    public UnityEvent m_event;
    public static GameObject target;
    protected GameObject player;

    void Awake() {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update() {
        Step();
        inside = false;
        if(useCollider) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= radius && enable && Check()) {
            Handler();
        } else {
            DeHandler();
        }
    }

    void OnTriggerStay(Collider other) {
        if (!useCollider || !enable) return;
        if (other.tag == "Player" && Check())
            Handler();
    }

    void OnTriggerExit(Collider other) {
        if (other.tag == "Player")
            DeHandler();
    }

    private void Handler() {
        if (target == null || target == gameObject) {
            target = gameObject;
            inside = true;
			if (dontAsk || Input.GetKeyDown(KeyCode.F)) {
				Act ();
            }
        }
    }

    private void DeHandler() {
        if(target == gameObject) target = null;
    }

    public virtual bool Check() {
		GameObject obj;
		Ray cast = new Ray (transform.position, player.transform.position - transform.position);
		float dist = Vector3.Distance (transform.position, player.transform.position);
		RaycastHit hit;
		if (!Physics.Raycast (cast, out hit, dist))
			obj = player.gameObject;
		else
		    obj = hit.collider.gameObject;

		if (obj != player)
			return false;
		return true;
    }

    public virtual void Act() {
        Debug.Log("Interacted with " + gameObject.ToString());
        enable = false;
        target = null;
        if(m_event != null) m_event.Invoke();
    }

    public virtual void Step() {
        
    }

    void OnDrawGizmos(){
		if (drawGizmos && !useCollider) {
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere (transform.position, radius);
		}
	}
}
