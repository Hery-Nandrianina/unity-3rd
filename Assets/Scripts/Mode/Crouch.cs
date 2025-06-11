using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crouch : MonoBehaviour
{
    [HideInInspector]
    public bool crouched;
    public bool fall;
    public Vector2 limit;
    public float down;
    [Space]
    public bool jump;
    public float cSpeed, cHeight, cRadius;

    [HideInInspector] public bool standBy;

    Move move;
    Animator anim;
    Body body;

    private void Start() {
        move = GetComponent<Move>();
        anim = GetComponentInChildren<Animator>();
        body = GetComponent<Body>();
    }

    public void Stand() {
        if(!body.checkCap(transform.position + Vector3.up * cHeight/2)) {
            move.tempSpeed = move.speed;
            crouched = false;
            anim.SetBool("Crouch", false);
            StopCoroutine("Shape");
            body.resetCollider();
            standBy = false;
        }
    }

    public void Into() {
        crouched = true;
        move.tempSpeed = cSpeed;
        anim.SetBool("Crouch", true);
        StartCoroutine("Shape");
    }

    IEnumerator Shape() {
        //Temporary fix
        while(!anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch Cycle"))
            yield return null;
        body.shapeCollider(cHeight, cRadius, Vector3.zero);
    }

    void LateUpdate()
    {
        if(crouched && (!body.isGrounded(1f) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Space) 
        ||Input.GetKeyDown(KeyCode.LeftShift))) {
            Stand();
            return;
        }
    
        if(standBy) {
            if(body.isGrounded() && !move.mode)
                Stand(); 
            else
                standBy = false;
        }
        
        if(Input.GetKeyDown(KeyCode.C) && body.isGrounded() && !crouched && (!move.mode || move.isRolling))
            Into();
    }
}
