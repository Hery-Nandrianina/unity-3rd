using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : Action
{
    bool push;

    public override void Act() {
        base.Act();

        push = true;
        transform.parent = player.transform;
    }

    public override void Step() {
        if(push)
        {
            if(Input.GetKeyDown(KeyCode.A)) {
                enable = true;
                push = false;
                transform.parent = null;
            }
        }
    }
}
