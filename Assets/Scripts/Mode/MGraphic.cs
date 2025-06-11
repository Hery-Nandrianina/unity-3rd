using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGraphic : MonoBehaviour
{
    private bool climb;
    private Quaternion fixRotation;

    Animator anim;
    Move move;
    LadderMove ladder;
    Graphic graphic;
    Body body;
    Shooter shooter;

    private void Start() {
        LadderMove.evClimb += Climb;

        anim = GetComponent<Animator>();
        move = GetComponentInParent<Move>();
        body = GetComponentInParent<Body>();
        ladder = GetComponentInParent<LadderMove>();
        graphic = GetComponent<Graphic>();
        shooter = GetComponentInParent<Shooter>();
    }

    //Melee implemented here
    public void Hit() {
        GetComponentInParent<Melee>().Hit();
    }

    //Shoot implemented here
    public void EndReload() {
        shooter.Active();
        shooter.isReload = false;
    }

    public void Reload() {
        
    }

    //Climb implemented here
    private void Climb(GameObject other, int type) {
        climb = graphic.mode = (other != null);
        anim.SetBool("Climb", climb);
        // graphic.total = Vector3.zero;
        if(climb) {
            //Check this out
            // fixRotation = Quaternion.Inverse(other.transform.rotation);
            fixRotation = Quaternion.LookRotation(-other.transform.forward);
            switch(type) {
                case 0: {
                    //Temporary fix
                    if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Ground") || anim.GetCurrentAnimatorStateInfo(0).IsName("Swim Cycle")
                       || anim.GetAnimatorTransitionInfo(0).IsUserName("toLand")) {
                        graphic.motion = true;
                    }
                } break;
                case 1: {
                    graphic.motion = true;
                    anim.SetTrigger("onTop");
                    anim.SetFloat("climbSpeed", 0);
                } break;
            }
        } else {
            anim.ResetTrigger("onTop");
            switch(type) {
                case 0: {anim.SetTrigger("offTop"); graphic.motion = graphic.mode = true; body.sleep = true;} break;
                case 1: anim.SetTrigger("offMidAir"); break;
                case 2: {graphic.mode = graphic.motion = true; body.sleep = true;} break;
            }
        }
    }

    public void startClimbing() {
        ladder.climb = true;
        ladder.Lock = true;
        graphic.motion = false;
        body.sleep = false;
        anim.SetFloat("Speed", 0);
        body.shapeCollider(0, ladder.radius, -Vector3.forward * ladder.back, transform);
    }

    private bool launch;

    private void Update() {
        //Temporary fix //Should be at the end of the transition
        if(anim.GetAnimatorTransitionInfo(0).IsUserName("toClimb") && !launch) {
            startClimbing();
            launch = true;
        } if(anim.GetAnimatorTransitionInfo(0).IsUserName("climbTo") && !launch) {
            ladder.EndClimbing();
            launch = true;
        }

        if(!anim.IsInTransition(0) && launch)
            launch = false;

        if(climb) {
            //Temporary fix
            // if(anim.GetCurrentAnimatorStateInfo(0).IsName("Climb Cycle") && !ladder.climb)
                // ladder.climb = true;
            anim.SetFloat("climbSpeed", ladder.Axis() + 1, 0.1f, Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, fixRotation, 0.2f);
            // transform.rotation = Quaternion.RotateTowards(transform.rotation, fixRotation, 6f);
        }
    }
}
