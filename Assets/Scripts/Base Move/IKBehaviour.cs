using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class IKBehaviour : MonoBehaviour
{
    public bool footIK;
    [Range(0,1f)] public float DistanceToGround;
    public LayerMask layerMask;

    [Space]
    public bool AimIK;
    public Rig[] aimRigs;
    public Rig handRig;
    public Rig pistolRig;

    Aim look;
    Animator anim;

    private void Start() {
        anim = this.GetComponent<Animator>();
        look = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Aim>();
    }

    private void OnAnimatorIK(int layerIndex) {
        if(footIK){
        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0f);
        
        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0f);

        RaycastHit hit;
        //LeftFoot
        if (Physics.Raycast(anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up,
            Vector3.down, out hit, DistanceToGround + 1f, layerMask)) {
                    Vector3 footPosition = hit.point;
                    footPosition.y += DistanceToGround;
                    anim.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);
                    anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation);
        }

        //RightFoot
        if (Physics.Raycast(anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up,
            Vector3.down, out hit, DistanceToGround + 1f, layerMask)) {
                    Vector3 footPosition = hit.point;
                    footPosition.y += DistanceToGround;
                    anim.SetIKPosition(AvatarIKGoal.RightFoot, footPosition);
                    anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation);
        }
        }
    }
}
