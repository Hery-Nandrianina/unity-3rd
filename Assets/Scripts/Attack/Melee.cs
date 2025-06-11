using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : MonoBehaviour
{
    public float inBetween;
    private float to;

    // [HideInInspector]
    public bool onAttack;
    // private bool cancel;

    Animator anim;
    Move move;
    Switch weapon;
    Body body;
    Weapon sword;
    Graphic graphic;
    Crouch bend;

    private void Start() {
        anim = GetComponentInChildren<Animator>();
        move = GetComponent<Move>();
        body = GetComponent<Body>();
        weapon = GetComponent<Switch>();
        bend = GetComponent<Crouch>();
        sword = GetComponentInChildren<Weapon>();
        graphic = GetComponentInChildren<Graphic>();
    }

    private bool launch;

    private void Update() {
        if(weapon.type != 1) return;
        if(anim.GetCurrentAnimatorStateInfo(0).IsTag("Last") && !anim.IsInTransition(0) && inBetween!=0) {
            anim.ResetTrigger("Sword");
            to = Time.time + inBetween;
            return;
        }

        if(Input.GetMouseButtonDown(0) && body.isGrounded() && (!move.mode || onAttack) && to < Time.time && !bend.crouched) {
            anim.SetTrigger("Sword");
            graphic.mode = true;
            // anim.SetFloat("Forward", 1);
            onAttack = true;
            move.mode = true;
        }

        if(anim.GetAnimatorTransitionInfo(0).IsUserName("End") && !launch) {
            endAttack();
            launch = true;
        }
        if(!anim.IsInTransition(0) && launch)
            launch = false;

        // if(cancel) {
        //     anim.SetFloat("Forward", Mathf.Lerp(anim.GetFloat("Forward"), -0.1f, 0.01f));

        //     if(anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0) {
        //         anim.SetTrigger("End");
        //         // anim.SetFloat("Forward", 1);
        //         cancel = false;
        //         move.mode = false;
        //     }
        // }

        // if(onAttack) 
        //     sword.Hit();
    }

    public void Cancel() {
        // anim.SetFloat("Forward", -1f);
        // cancel = true;
        // onAttack = false;
        // Debug.Log("Cancel Attack");
    }

    public void Hit() {
        
    }

    public void endAttack() {
        anim.ResetTrigger("Sword");
        move.mode = false;
        onAttack = false;
        graphic.mode = false;
    }
}
