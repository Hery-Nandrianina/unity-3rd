using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swim : MonoBehaviour
{
    public bool swimming;
    public float speed, offset;
    public float counter;

    [Space]
    public float smooth;
    public float sensitivity = 6f, limit = 60;
    public float climbOffset;

    [Space]
    public bool corner;
    public float cornerOffset;
    public float range, down;
    public Vector2 off;

    private GameObject water;
    private float top;
    private float y;
    [HideInInspector] public bool atTop;

    CamControl cam;
    Animator anim;
    Move move;
    Graphic graphic;
    Body body;
    LadderMove ladder;

    private void Start() {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CamControl>();
        anim = GetComponentInChildren<Animator>();
        move = GetComponent<Move>();
        graphic = GetComponentInChildren<Graphic>();
        body = GetComponent<Body>();
        ladder = GetComponent<LadderMove>();
    }

    private void Update() {
        if (!swimming) { y = 0; return; }
        if(Input.GetKey(KeyCode.Space)) {
            // y -= sensitivity;
            y = Mathf.Lerp(y, -limit, 0.1f);
            // if(y < -limit) y = -limit;
        } else if(Input.GetKey(KeyCode.LeftShift)) {
            // y += sensitivity;
            y = Mathf.Lerp(y, limit, 0.1f);
            // if(y > limit) y = limit;
        } else {
            //THIS IS NOT WORKING AT ALL!
            // if(body.velocity.y < 0) {
            //     y = Vector2.Angle(Vector2.right, new Vector2(1, body.velocity.y));
            //     return;
            // } else
                y = Mathf.Lerp(y, 0, 0.1f);
        }
        // y = Mathf.Clamp(y, -limit, limit);
    }

    public bool canClimb() {
        if(swimming)
            return (Mathf.Abs(transform.position.y - water.transform.localPosition.y - top - offset) < climbOffset);
        return false;
    }

    private void FixedUpdate() {
        if (!swimming) return;
        Vector3 dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        float input = Mathf.Clamp01(dir.magnitude);
        anim.SetFloat("Speed", input , smooth, Time.deltaTime);

        Vector3 to = dir;
        to.y = (Quaternion.Euler(y, 0, 0) * Vector3.forward).y * input/2;
        Vector3 newPos = transform.position + Quaternion.Euler(0, cam.x, 0) * to.normalized * speed * Time.deltaTime;
        if(newPos.y > water.transform.localPosition.y + top + offset) {
            newPos.y = water.transform.localPosition.y + top + offset;
            y = 0;

            if(body.isGrounded()) {
                Stand();
                return;
            }
        }

        if(body.velocity.y >= 0 && Mathf.Abs(transform.position.y - water.transform.localPosition.y - top - offset) < cornerOffset) {
            //Temporary fix
            //Raycast illegal outside body
            RaycastHit hit;
            if(corner = Physics.Raycast(transform.position + Vector3.down * down, (newPos - transform.position).normalized, out hit, range, body.layerMask)) {
                Vector3 pos = hit.point - hit.normal* off.x + Vector3.up * (off.y + down);
                if(!body.checkCap(pos)) {
                    body.MoveTo(pos);
                    Stand();
                    return;
                }
            }
            
        }

        // Debug.Log(newPos.y);
        body.MoveTo(newPos);
        
        if(input > 0 || body.velocity.y < 0) {
            float angle = cam.x + Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);
            anim.transform.rotation = Quaternion.Lerp(anim.transform.rotation, Quaternion.Euler(y * input, angle, 0), 0.04f);
        } else {
            //Can be used as aesthetic
            // Vector3 plan = anim.transform.forward;
            // plan.y = 0;
            // anim.transform.rotation = Quaternion.Lerp(anim.transform.rotation, Quaternion.LookRotation(plan), 0.01f);
        }
    }

    public void Stand() {
        swimming = false;
        anim.SetBool("Swimming", false);
        graphic.mode = false;
        move.mode = false;
        water = null;
        body.useGravity = true;
        // anim.transform.rotation = Quaternion.Euler(0, cam.x, 0);
        //Temporary fix
        graphic.lastDir = Vector3.zero;
        corner = false;
    }

    private void OnTriggerStay(Collider other) {
        BoxCollider col = other.GetComponent<BoxCollider>();
        if(col) {
            top = col.center.y + col.size.y * other.transform.localScale.y/2;
        }

        //localPosition or position
        //Input is a temporary fix
        if((!move.mode || (ladder.climb && ladder.Axis() < 0)) && other.CompareTag("Water") && !swimming 
            && transform.position.y < other.transform.localPosition.y + top + offset) {
            if(ladder.climb) {
                LadderMove.evClimb?.Invoke(null, 1);
                ladder.EndClimbing();
                anim.ResetTrigger("offMidAir");
            }

            swimming = true;
            move.mode = true;
            graphic.mode = true;
            water = other.gameObject;
            anim.SetBool("Swimming", true);
            GetComponent<Crouch>().Stand();

            body.useGravity = false;
            // y = Vector3.Angle(anim.transform.forward, rb.velocity);
            //Temporary fix
            // move.velocity *= 1 - counter;
            // Debug.Log("Enter " + y);
        
            //Check this out
            // Vector3 newPos = transform.position;
            // newPos.y = water.transform.position.y + top + offset;
            // transform.position = newPos;
        }
    }
}
