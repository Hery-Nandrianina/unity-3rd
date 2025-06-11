using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Aim : MonoBehaviour
{
    public Transform target;
    // public Vector3 offset;
    public bool hold;
    public bool regain;
    public float idleTime;
    public Vector2 sensitivity;

    [Space]
    public bool staticSpeed;
    public float frame, speed;

    [Space]
    public GameObject pointer;
    public bool isAiming;

    private Quaternion rotation;
    private float lastX, toIdle;
    [HideInInspector]public bool isSmoothing;

    CamControl cam;
    Graphic graphic;
    Animator anim;
    IKBehaviour ik;
    Move move;
    Body body;
    Switch weapon;

    private void Start() {
        cam = GetComponent<CamControl>();
        move = GameObject.FindGameObjectWithTag("Player").GetComponent<Move>();
        graphic = move.GetComponentInChildren<Graphic>();
        anim = graphic.GetComponent<Animator>();
        weapon = move.GetComponent<Switch>();
        body = move.GetComponent<Body>();
        ik = graphic.GetComponent<IKBehaviour>();
        isSmoothing = false;
    }

    public void startShoot() {
        if(!Input.GetMouseButton(1) && hold)
            toIdle = Time.time + idleTime;

        if(!isAiming && !isSmoothing)
            StartCoroutine("SmoothIn");
    }

    private void Update() {
        if(!hold) {
        if( ((Input.GetMouseButtonDown(1) && !isSmoothing) || (weapon.type != 3 && weapon.type!=2) || (move.mode && !move.isRolling) 
        || !body.isGrounded(1f)) && isAiming ) {
            StopAllCoroutines();
            // StartCoroutine("SmoothOut", ((move.mode && !move.isRolling) || !body.isGrounded(1f)) ? 0 : frame);
            StartCoroutine("SmoothOut", frame);
            return;
        }

        if (regain && ((Input.GetMouseButtonDown(1) && !isSmoothing) 
            || (weapon.type != 3 && weapon.type!=2) || (move.mode && !move.isRolling)))
            regain = false;

        if (!isAiming)
        if(body.isGrounded(1f) && !isSmoothing && !move.isRolling && !move.mode && (weapon.type == 3 || weapon.type==2)
            && (Input.GetMouseButtonDown(1) || (regain && body.velocity.y <= 0))) { //Temporary fix
            StartCoroutine("SmoothIn");
            regain = false;
        } } else {
        //Hold mode
        if(((!Input.GetMouseButton(1) && (Time.time > toIdle || Input.GetMouseButtonUp(1)) && !isSmoothing) 
        || (weapon.type != 3 && weapon.type!=2) || (move.mode && !move.isRolling) 
           || !body.isGrounded(1f)) && isAiming) {
            StopAllCoroutines();
            // StartCoroutine("SmoothOut", ((move.mode && !move.isRolling) || !body.isGrounded(1f)) ? 0 : frame);
            StartCoroutine("SmoothOut", frame);
            return;
        }
        if(Input.GetMouseButton(1) && (!move.mode && !isAiming && !isSmoothing && !move.isRolling && (weapon.type == 3 || weapon.type==2) && body.isGrounded()))
            StartCoroutine("SmoothIn");
        }

        //Temporary fix (all IK are temporary)
        if(isAiming) {
        if(move.isRolling)
            foreach (Rig rig in ik.aimRigs)
                rig.weight = Mathf.Lerp(rig.weight, 0, 0.3f);
        else
            foreach (Rig rig in ik.aimRigs)
                rig.weight = Mathf.Lerp(rig.weight, 1, 0.3f);
        if (weapon.type == 3) ik.pistolRig.weight = Mathf.Lerp(ik.pistolRig.weight, 1, 0.3f);
        else ik.pistolRig.weight = Mathf.Lerp(ik.pistolRig.weight, 0, 0.3f);
        } else {
            foreach (Rig rig in ik.aimRigs)
                rig.weight = Mathf.Lerp(rig.weight, 0, 0.3f);
            ik.pistolRig.weight = Mathf.Lerp(ik.pistolRig.weight, 0, 0.3f);
        }
    }

    public IEnumerator SmoothOut(float nb) {
        isSmoothing = true;
        isAiming = false;
        anim.ResetTrigger("Shoot");
        // bool lastMode = move.mode;
        //Why do they move though? (Maybe cause of lerp)
        float lastY = Vector3.SignedAngle(target.position + graphic.transform.right - transform.position, 
                        graphic.transform.forward, -graphic.transform.right);
        lastX = Vector3.SignedAngle(Vector3.forward, graphic.transform.forward, Vector3.up);
        
        pointer.SetActive(false);
        anim.SetBool("Shooting", false);
        // move.mode = true;
        // if(!move.mode)
        //     graphic.mode = false;
        
        Vector3 position = target.position + Quaternion.Euler (lastY, lastX, 0) * Vector3.forward * -cam.dist;
        float delta = Vector3.Distance(transform.position, position);
        Quaternion rotation = Quaternion.LookRotation(Quaternion.Euler (lastY, lastX, 0) * Vector3.forward * cam.dist);
        float omega = Quaternion.Angle(transform.rotation, rotation);

        Vector3 startPos = move.transform.position;
        Vector3 pos = transform.position;

        for (int i = 0; i < nb; i++)
        {
            // foreach (Rig rig in ik.aimRigs)
            // rig.weight = Mathf.MoveTowards(rig.weight, 0, 1/nb);
            // if(weapon.type == 3) ik.pistolRig.weight = Mathf.MoveTowards(ik.pistolRig.weight, 0, 1/nb);

            pos = Vector3.MoveTowards(pos, position, delta / nb);
            transform.position = (move.transform.position - startPos) + pos;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, omega / nb);
            yield return null;
        }
        // transform.position = (move.transform.position - startPos) + position;
        // transform.rotation = rotation;
        cam.x = lastX;
        cam.y = lastY;

        cam.control = true;
        cam.tempSens = new Vector2(cam.sensX, cam.sensY);
        isSmoothing = false;
        // yield return null;
        //Temporary fix
        // move.mode = lastMode;
    }

    IEnumerator SmoothIn(){
        anim.SetBool("Shooting", true);
        isSmoothing = true;
        lastX = cam.x;
        cam.tempSens = sensitivity;
        float lastY = cam.y;
        rotation = Quaternion.Euler(lastY, lastX, 0);
        cam.control = false;
        // graphic.mode = true;
        
        //Temporary fix
        // move.mode = true;
        // anim.SetFloat("Speed", 0);
        Vector3 position = (target.position + Quaternion.Euler(0, lastX, 0) * Vector3.right + rotation * Vector3.forward * -2);
        Vector3 pos = transform.position;
        Vector3 startPos = move.transform.position;

        float angle = Quaternion.Angle(Quaternion.LookRotation(rotation * Vector3.forward * 2), transform.rotation);
        float omega = Quaternion.Angle(graphic.transform.rotation, Quaternion.Euler(0, lastX, 0));
        float delta = Vector3.Distance(transform.position, position);
        //For static speed
        float nb = (staticSpeed) ? Mathf.Max(omega / 12, frame) : frame;
        for (int i = 0; i < nb; i++) {
            // foreach (Rig rig in ik.aimRigs)
            // rig.weight = Mathf.MoveTowards(rig.weight, 1, 1/nb);
            // if(weapon.type == 3) ik.pistolRig.weight = Mathf.MoveTowards(ik.pistolRig.weight, 1, 1/nb);

            pos  = Vector3.MoveTowards(pos, position, delta / nb);
            transform.position = (move.transform.position - startPos) + pos;

            graphic.transform.rotation = Quaternion.RotateTowards(graphic.transform.rotation, 
                                        Quaternion.Euler(0, lastX, 0), omega / nb );
            transform.rotation = Quaternion.RotateTowards(transform.rotation, 
                Quaternion.LookRotation(rotation * Vector3.forward * 2), angle / nb );
            yield return null;
        }
        // graphic.transform.rotation = Quaternion.Euler(0, lastX, 0);
        // transform.rotation = Quaternion.LookRotation(rotation * Vector3.forward * 2);
        // transform.position = (move.transform.position - startPos) + position;

        cam.x = lastX;
        cam.y = lastY;
        if(toIdle > Time.time || Input.GetMouseButton(1) || !hold)
            pointer.SetActive(true);
        isAiming = true;
        isSmoothing = false;

        //Temporary fix
        // move.mode = false;
    }

    void LateUpdate()
    {
        if(!isAiming) return;
        graphic.transform.rotation = Quaternion.Lerp(graphic.transform.rotation, Quaternion.Euler(0, cam.x, 0), 0.1f);

        // Vector3 dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        // float input = Mathf.Clamp01(dir.magnitude);
        // anim.SetFloat("Speed",input * (Input.GetKey(KeyCode.LeftShift) ? 2 : 1) , 0.1f, Time.deltaTime);

        rotation = Quaternion.Lerp(rotation, Quaternion.Euler(cam.y, cam.x, 0), 0.1f);
        transform.position = target.position + graphic.transform.right + rotation * Vector3.forward * -2;
        // transform.LookAt(target.position + graphic.transform.right);
        transform.rotation = Quaternion.LookRotation(target.position + graphic.transform.right - transform.position);
    }
}
