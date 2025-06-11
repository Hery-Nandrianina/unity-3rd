using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graphic : MonoBehaviour
{
    [Range(0,1)]
    public float cycleSmooth, turnSpeed;
    public float rollOn;
    public bool rollIdle;

    [HideInInspector]
    public Vector3 lastDir;
    private float lastCamX;

    // [HideInInspector]
    [Header("Debug")]
    public bool motion;
    public bool mode;
    
    Animator anim;
    CamControl cam;
    Move move;
    Body body;
    Aim aim;

    private void Start() {
        anim = this.GetComponent<Animator>();
        move = this.GetComponentsInParent<Move>()[0];
        body = this.GetComponentsInParent<Body>()[0];
        cam  = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CamControl>();
        aim  = cam.GetComponent<Aim>();
    }

    // public void Cancel(){
    //     if(anim.GetNextAnimatorStateInfo(0).IsName("Unarmed-Jump")) {
    //         if(move.isGrounded(0.2f))
    //             anim.SetTrigger("Land");
    //         else
    //             anim.SetTrigger("Cancel");
    //     }
    // }

    public void Jump(){
        //Temporary fix
        Aim aim = cam.GetComponent<Aim>();
        if(aim.isAiming) {
            aim.StartCoroutine("SmoothOut",  cam.GetComponent<Aim>().frame);
            aim.regain = true;
        }

        if(mode) return;
        anim.ResetTrigger("Cancel");
        anim.SetTrigger("Jump");

        if(move.dirJump){
             lastDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
             lastCamX = cam.x;
        }
    }

    public void Roll(Vector3 dir, float x) {
        anim.SetTrigger("Roll");
        lastDir = dir;
        lastCamX = x;
    }

    // [HideInInspector]
    // public Vector3 total;
    //Root motion needed for mode
    void OnAnimatorMove() {
        if(motion) {
            //Is it safe?
            Vector3 round = anim.deltaPosition;
            // if(Mathf.Abs(round.x) < 0.03f) round.x = 0;
            // if(Mathf.Abs(round.z) < 0.03f) round.z = 0;

            transform.parent.position += round;
            // total += round;
            // Debug.Log(round);
        }
    }

    private bool trans;

    void Update() {
        //Should be called on transition finished
        if(anim.GetAnimatorTransitionInfo(0).IsUserName("RollEnd") && move.isRolling && !trans) {
            move.endRoll();
            lastDir = Vector3.zero;
            trans = true;
        }
        if(!anim.IsInTransition(0)) trans = false;

        if(mode) return;

        Vector3 dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        bool rLock = ( (lastDir!=Vector3.zero) && body.velocity.y > 0 ) || move.isRolling;
        if((dir.magnitude > 0 || rLock) && !(aim.isAiming || aim.isSmoothing)) {
            float angle = (rLock ? lastCamX : cam.x) 
                + Vector3.SignedAngle(Vector3.forward, rLock ? lastDir : dir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, angle, 0), turnSpeed * 0.1f);
            // Debug.Log("Proper Update");
        }

        //Run implemented here
        float input = Mathf.Clamp01(dir.magnitude);
        // float input = body.getVelocity();
        anim.SetFloat("Speed",input * (Input.GetKey(KeyCode.LeftShift) ? 2 : 1) , cycleSmooth, Time.deltaTime);

        if(body.isGrounded()) {
            if(anim.GetCurrentAnimatorStateInfo(0).IsName("Unarmed-Fall") && !anim.IsInTransition(0) && !move.isRolling) {
                lastDir = Vector3.zero;
                //Temporary fix
                anim.ResetTrigger("Fall");
                anim.SetTrigger("Land");

                //Old
                if(anim.name == "RPG-Character" && body.velocity.y < -rollOn && (dir.magnitude != 0 || rollIdle)) {
                    // anim.SetTrigger("LandRoll");
                    anim.ResetTrigger("Land");
                    move.Roll(transform.forward);
                }
            }
        }
        
        if(!body.isGrounded(1f))
            if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Ground")  && !anim.IsInTransition(0)) {
                //  if(move.isRolling)
                //     endRoll();
                anim.SetBool("Crouch", false);
                anim.SetTrigger("Fall");
            }
    }
}
