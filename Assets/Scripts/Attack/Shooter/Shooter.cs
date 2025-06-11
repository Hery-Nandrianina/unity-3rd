using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    // public bool raycast;
    public GameObject bullet;
    public Transform tip;
    public float range, fireRate;
    
    // public Vector3 offset;
    public int regain;
    // private float nextFire;
    private bool canShoot = true;
    [HideInInspector] public bool isReload;

    Aim aim;
    Move move;
    Animator anim;
    Body body;
    Switch weapon;

    CamControl cam;
    IKBehaviour ik;

    private void Start() {
        cam = Camera.main.GetComponent<CamControl>();
        aim = cam.GetComponent<Aim>();

        move = GetComponent<Move>();
        body = GetComponent<Body>();
        anim = GetComponentInChildren<Animator>();
        weapon = GetComponent<Switch>();
        ik = GetComponentInChildren<IKBehaviour>();
    }

    private void Update() {
        if(move.mode && !move.isRolling && weapon.gun) 
        { regain = weapon.type; weapon.Index(0); }
        if(regain!=0 && !move.mode) 
        { weapon.Index(regain); regain = 0; }
        if(!body.isGrounded(1f) || move.isRolling)
        { anim.SetTrigger("cancelReload"); isReload=false; return; }

        if(!weapon.hand2) ik.handRig.weight = Mathf.Lerp(ik.handRig.weight, 0, 0.2f);
        if(!weapon.gun)
        { anim.SetTrigger("cancelReload"); isReload=false; return; }
        
        if(weapon.hand2)
            ik.handRig.weight = Mathf.Lerp(ik.handRig.weight, 1, 0.2f);

        if(Input.GetKeyDown(KeyCode.R) && !isReload) {
            anim.SetTrigger("Reload");
            anim.ResetTrigger("cancelReload");
            CancelInvoke();
            canShoot = false;
            isReload = true;
        }

		if (((Input.GetMouseButtonDown(0) && fireRate <= 0) || (Input.GetMouseButton(0)) 
            && fireRate > 0) && canShoot) {//&& Time.time > nextFire
				startShoot ();
				// nextFire = Time.time + Mathf.Abs(fireRate);
                canShoot = false;
                Invoke("Active", fireRate);
		}
    }

    public void Active() {
        canShoot = true;
    }

    // public void endShoot() {
    //     main.mode = false;
    // }

    public void Shoot() {
        //Temporary fix
        // Vector3 dir = Quaternion.Euler(offset) * cam.transform.forward;
        Vector3 dir = (cam.transform.position + cam.transform.forward * range - tip.position).normalized;
        Vector3 pos = tip.position;

        //For Raycast only; don't work on close target (between camera and player)
        // if(bullet.GetComponent<Projectile>().raycast) {
        //     pos = cam.transform.position;
        //     dir = cam.transform.forward;
        // }

        GameObject projectile = Instantiate (bullet, pos, Quaternion.identity) as GameObject;
		projectile.GetComponent<Projectile> ().range = range;
		projectile.GetComponent<Projectile> ().forward =  dir;
    }

    private void startShoot() {
        aim.startShoot();
        anim.SetTrigger("Shoot");
        Shoot();
    }
}
