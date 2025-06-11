using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ledge : MonoBehaviour
{
    public bool ledge;
    public float cSpeed;
    public Vector2 center;
    public float end;

    private float max;
    private GameObject instance;
    private Vector3 dir;
    private bool isSmoothing;

    Move move;
    Graphic graphic;
    Animator anim;
    Body body;

    private void Start() {
        move = GetComponent<Move>();
        graphic = GetComponentInChildren<Graphic>();
        anim = GetComponentInChildren<Animator>();
        body = GetComponent<Body>();
    }

    private void FixedUpdate() {
        if(ledge && !isSmoothing) {
            float input = Input.GetAxis("Horizontal");
            Vector3 cross = Vector3.Cross(Vector3.up, dir);
            if(input!=0) {
            if(Vector3.Distance(transform.position + Vector3.down * center.y + dir*center.x + cross*end*Mathf.Sign(input), 
            instance.transform.position) < max) {
                anim.SetFloat("ledgeSpeed", input, 0.1f, Time.fixedDeltaTime);
                body.MoveTo(transform.position + cross*input*cSpeed*Time.deltaTime);
                return;
            } else {
                body.MoveTo(instance.transform.position + Vector3.up * center.y - dir*center.x + cross*(max - end)*Mathf.Sign(input) );
            }
            }
            anim.SetFloat("ledgeSpeed", 0, 0.2f, Time.deltaTime);
        }
                        
    }

    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Ledge") && !move.mode && !ledge) {
            BoxCollider col = other.GetComponent<BoxCollider>();
            max = other.transform.localScale.x * col.size.x/2;//Consider center
            instance = other.gameObject;
            if(Vector3.Distance(transform.position + Vector3.down * center.y + dir*center.x, 
            instance.transform.position) > max)
                return;

            dir = -other.transform.forward;
            Vector3 cross = Vector3.Cross(Vector3.up, dir);
            anim.SetFloat("ledgeSpeed", 0);
            // instance = other.gameObject;
            bool found;
            Vector3 middle = GetIntersection(other.transform.position, other.transform.position + cross,
                transform.position, transform.position + dir, out found);
            middle.y = other.transform.position.y;
            Vector3 off = middle - other.transform.position;
            Vector3 to = other.transform.position + off + Vector3.up * center.y -dir*center.x;
            StartCoroutine(Smooth(to, transform.position -dir*center.x, 12));
        }
    }

    public Vector3 GetIntersection(Vector3 A1, Vector3 A2, Vector3 B1, Vector3 B2, out bool found)
    {
        float tmp = (B2.x - B1.x) * (A2.z - A1.z) - (B2.z - B1.z) * (A2.z - A1.z);
        if (tmp == 0) { found = false; return Vector2.zero; }
        float mu = ((A1.x - B1.x) * (A2.z - A1.z) - (A1.z - B1.z) * (A2.x - A1.x)) / tmp;
        found = true;
        return new Vector3(B1.x + (B2.x - B1.x) * mu, 0, B1.z + (B2.z - B1.z) * mu);
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.LeftShift) && ledge && !isSmoothing) {
            endLedge();
        }
    }

    IEnumerator Smooth(Vector3 position, Vector3 mid, float w) {
        isSmoothing = true;
        move.mode = true;
        graphic.mode = true;
        anim.SetBool("Ledge", true);
        body.sleep = true;
        float omega = Quaternion.Angle(graphic.transform.rotation, Quaternion.LookRotation(dir));
        float delta2 = Vector3.Distance(transform.position, mid);
        float delta = Vector3.Distance(mid, position);
        //For static speed
        float frame = omega / w;
        for (int i = 0; i < frame; i++) {
            if(i < frame*delta2/(delta + delta2))
                transform.position  = Vector3.MoveTowards(transform.position, mid, (delta+delta2)/frame);
            else
                transform.position  = Vector3.MoveTowards(transform.position, position, (delta+delta2)/frame );
            graphic.transform.rotation = Quaternion.RotateTowards(graphic.transform.rotation, Quaternion.LookRotation(dir), omega / frame );
            yield return null;
        }
        transform.position = position;
        graphic.transform.rotation = Quaternion.LookRotation(dir);
        ledge = true;
        body.sleep = false;
        isSmoothing = false;
        body.useGravity = false;
        body.velocity = Vector3.zero;
    }

    public void endLedge() {
        ledge = false;
        move.mode = false;
        graphic.mode = false;
        anim.SetBool("Ledge", false);
        body.useGravity = true;
    }
}
