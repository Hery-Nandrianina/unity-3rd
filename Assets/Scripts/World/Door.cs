using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Action
{
    Animator anim;
    bool open;

    private void Start() {
        anim = GetComponent<Animator>();
    }

    public override void Act() {
        base.Act();
        
        anim.SetTrigger(open ? "Close" : "Open");
        open = !open;
    }

    public override void Step() {
        if(!enable && !anim.IsInTransition(0))
        if((open && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99f)
        || (!open && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle")))
            enable = true;
    }

}
