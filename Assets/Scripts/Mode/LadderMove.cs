using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderMove : MonoBehaviour
{
    public float speed;
    [Range(0,2)] public float offset;
    public float radius, back;

    [Space]
    public float maxAngle;
    [Range(-1,1)] public float toGround;

    [Space]
    public float topExit;
    public float snapY;
    [Range(0,2)] public float snapXZ;
    public float topLimit;
    public float bottomExit;

    [HideInInspector]
    public bool climb;

    private bool pseudoClimb;
    [HideInInspector] public bool Lock;
    [HideInInspector] public GameObject instance;
    private Vector3 newPos;

    CamControl cam;
    Move move;
    Crouch bend;
    Body body;
    Swim swim;
    Graphic graphic;
    Animator anim;

    public delegate void climbHandler(GameObject e, int i=0);
    public static climbHandler evClimb;

    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CamControl>();
        move = GetComponent<Move>();
        bend = GetComponent<Crouch>();
        body = GetComponent<Body>();
        swim = GetComponent<Swim>();
        graphic = GetComponentInChildren<Graphic>();
        anim = graphic.GetComponent<Animator>();
        climb = false;
    }

    void LateUpdate() {
        if(Input.GetKeyDown(KeyCode.Space) && (Axis() <= 0f || true) && climb) {
            climb = false;
            instance = null;
            EndClimbing();
            evClimb?.Invoke(null, 1);
        }
        
    }

    public float Axis() {
        float angle = CamControl.Clamp0360(cam.x - Vector3.SignedAngle(-Vector3.right, graphic.transform.forward, Vector3.up));
        float axis = ((angle > 180) ? 1:-1) * Input.GetAxis("Vertical") + ((angle > 90 && angle < 270) ? 1:-1) * Input.GetAxis("Horizontal");
        return Mathf.Clamp(axis, -1, 1);
    }

    private void FixedUpdate() {
        if(!pseudoClimb) return;
        
        Vector3 dir = new Vector3(0, Axis(), 0);
        if(Lock) {
            // Debug.Log(string.Format("({0}-{2},{1}-{3})", transform.position.x, transform.position.z, newPos.x, newPos.z));
            // Vector3 toPos = newPos;
            Vector3 toPos = Vector3.Lerp(transform.position, newPos, 0.1f);
            toPos.y = transform.position.y;
            body.MoveTo(toPos + dir * Time.deltaTime * speed * (climb ? 1 : 0));
        }

        if(climb && instance!=null) {
        if(dir.y < 0 && body.isGrounded(-toGround + ((body.charCtrl) ? body.floorOffset : 0))) {
            climb = false;
            instance = null;
            evClimb?.Invoke(null, 2);
            return;
        }
        
        BoxCollider col = instance.GetComponent<BoxCollider>();
        float max = instance.transform.position.y + col.size.y/2 + col.center.y;
        float min = instance.transform.position.y - col.size.y/2 + col.center.y;
        
        if(transform.position.y >= max + topExit && Axis() > 0) {
            Vector3 to = transform.position;
            to.y = max + topExit;
            transform.position = to;

            climb = false;
            instance = null;
            Lock = false;
            evClimb?.Invoke(null);
            return;
        }

        if(transform.position.y <= min + bottomExit && Axis() < 0) {
            Vector3 to = transform.position;
            to.y = min + bottomExit;
            transform.position = to;

            climb = false;
            instance = null;
            EndClimbing();
            evClimb?.Invoke(null, 1);
        }
        }
    }

    public void EndClimbing(){
        // Debug.Log(graphic.total);
        body.resetCollider();
        graphic.motion = false;
        graphic.mode = false;
        body.sleep = false;
        climb = false;
        instance = null;

        body.useGravity = true;
        move.mode = false;
        pseudoClimb = false;
        Lock = false;
    }

    private void OnTriggerStay(Collider other) {
        bool top = false;
        if(other.transform.parent!=null && body.isGrounded())
            if(other.transform.parent.CompareTag("Ladder"))
                top = true;

        if(top || other.CompareTag("Ladder")) {
        Transform coord = (top) ? other.transform.parent : other.transform;
        BoxCollider col = coord.GetComponent<BoxCollider>();
        //Consider scale
        float max = coord.position.y + col.size.y/2 + col.center.y;
    
        if(!pseudoClimb && !move.isRolling && (!move.mode || (swim.canClimb() && !top))) {
            Vector3 dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            //Temporary fix
            if(dir.magnitude > 0 && (!top || !anim.GetCurrentAnimatorStateInfo(0).IsName("Unarmed-Fall"))
            && Vector3.Angle(coord.forward, Quaternion.Euler(0, cam.x, 0)  * dir * (top ? 1 : -1)) < maxAngle 
            && (top || transform.position.y < max - topLimit)) {
                if(bend.crouched) {
                    if(body.checkCap(transform.position + Vector3.up * bend.cHeight/2))
                        return;
                    else
                        bend.Stand();
                }
                    
                if(swim.swimming)
                    swim.Stand();

                newPos = coord.position + coord.forward * offset;
                newPos.y = transform.position.y;

                body.useGravity = false;
                body.velocity = Vector3.zero;
                body.sleep = true;
                instance = coord.gameObject;
                move.mode = true;
                Lock = false;
                pseudoClimb = true;

                Vector3 toPos = coord.position + coord.forward * (-snapXZ + offset );
                toPos.y = max + snapY;
                StartCoroutine(Smooth(top, (top) ? toPos : newPos));
            }
        }
        }
    }

    public string GetCurrentClipName(Animator animator){
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        return clipInfo[0].clip.name;
    }

    IEnumerator Smooth(bool top, Vector3 nPos) {
        //Temporary fix
        bool air = (anim.GetCurrentAnimatorStateInfo(0).IsName("Unarmed-Fall") || anim.GetCurrentAnimatorStateInfo(0).IsName("Climb Cycle")
        || anim.GetCurrentAnimatorStateInfo(0).IsName("Unarmed-Jump") || anim.GetAnimatorTransitionInfo(0).IsUserName("toJump"));
        air &= (!anim.GetAnimatorTransitionInfo(0).IsUserName("toLand"));
        if(air)
            anim.SetBool("Climb", true);
        float omega = 0;
        // if(top)
            graphic.mode = true;
        omega = Quaternion.Angle(Quaternion.LookRotation(-instance.transform.forward), graphic.transform.rotation);
        float delta = Vector3.Distance(transform.position, nPos);
        //Temporary fix
        float frame =  Mathf.Max(omega / 8, delta * 20);
        // float frame = 0;
        // if(frame < 4)   frame = 0;
        
        for (int i = 0; i < frame; i++) {
            // if(top)
                anim.SetFloat("Speed", 0, 1/frame, Time.deltaTime);
            graphic.transform.rotation = Quaternion.RotateTowards(graphic.transform.rotation,
                Quaternion.LookRotation(-instance.transform.forward), omega / frame);
            
            transform.position = Vector3.MoveTowards(transform.position, nPos, delta / frame);
            yield return null;
        }
        transform.position = nPos;
        graphic.transform.rotation = Quaternion.LookRotation(-instance.transform.forward);
        
        //Temporary fix
        if(air) {
            graphic.GetComponent<MGraphic>().startClimbing();
            evClimb?.Invoke(instance, (top) ? 1 : 3);
        } else
            evClimb?.Invoke(instance, (top) ? 1 : 0);
    }
}
