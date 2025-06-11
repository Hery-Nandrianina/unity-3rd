using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class Move : MonoBehaviour
{
    public float speed;
    public float runFactor;
    public float roll;
    public bool standBy;
    public float noRoll = 0.15f;
    private float nextRoll;
    
    [Space]
    public bool dirJump;
    public float jump;
    [Range(0,1)] public float dirFactor;
    [Range(0,1)] public float airFactor;
    public float airFactor2;

    [Header("Debug")] // [HideInInspector]
    public bool mode;
     // [HideInInspector]
    public bool isRolling;
    [HideInInspector] public float tempSpeed;
    private Vector3 dir;

    CamControl cam;
    Crouch bend;
    Graphic graphic;
    Body body;
    
    void Start()
    {
        tempSpeed = speed;

        bend = GetComponent<Crouch>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CamControl>();
        graphic = GetComponentInChildren<Graphic>();
        body = GetComponent<Body>();
    }

    void FixedUpdate()
    {
        //Give space to other mode
        if(!mode) {
        dir = Quaternion.Euler(0, cam.x, 0) * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        //Is it safe?
        if(body.isGrounded(1f) || !dirJump) {
            var data = body.Slope(dir);

            //Run implemented here
            if(!bend.crouched)
                body.MoveTo(transform.position + data.Item1 + dir * tempSpeed * ((Input.GetKey(KeyCode.LeftShift) 
                && body.isGrounded(1f)) ? runFactor : 1) * Time.deltaTime * data.Item2 * ((body.isGrounded(1f) || dirJump) ? 1 : airFactor2));
            else {
                Vector3 dir1 = dir.normalized * ((dir.magnitude < 1) ? 1 : dir.magnitude);
                if(body.isGrounded(body.gnd + bend.down, data.Item1 + dir1, bend.limit.x) || bend.fall)
                    body.MoveTo(transform.position + data.Item1 + dir * tempSpeed * Time.deltaTime * data.Item2);
                else {
                    //Raycast illegal outside body
                    // Debug.Log("INside");
                    RaycastHit info;
                    Vector3 from = transform.position + data.Item1 + dir1 + Vector3.down*
                    (1+bend.limit.x+bend.down+body.toGround+body.gnd-((body.charCtrl) ? body.floorOffset : 0));
                    if(!Physics.Raycast(from, -dir, out info, dir1.magnitude+bend.limit.x, body.layerMask))
                        return;
                    Vector3 cross = Vector3.Cross(Vector3.up, info.normal);
                    dir = cross * Vector3.Dot(cross, dir);
                    dir1 = dir.normalized * ((dir.magnitude < 1) ? 1 : dir.magnitude);
                    // dir1 = dir.normalized;
                    if(body.isGrounded(body.gnd + bend.down, data.Item1 + dir1, bend.limit.x))
                        body.MoveTo(transform.position + data.Item1 + dir * tempSpeed * Time.deltaTime * data.Item2);
                    // else
                    //     Debug.Log("IN2side");
                }
            }
        } 
        else if(body.velocity.y < 0 && dirJump) //Move in Air
            body.MoveTo(transform.position + dir * speed * Time.deltaTime * airFactor);
        }
    }

    void Update() {
        //Give space to other mode
        if(mode) return;
        if((!bend.crouched || bend.jump) && body.isGrounded(body.gnd) && Input.GetKeyDown(KeyCode.Space)) {//} && !isFloor()) {
            Vector3 velo = Vector3.zero;
            //Temporary fix (There should not be Physics check outside body)
            if((!Physics.Raycast(transform.position + Vector3.down * 1.3f, dir, 1f) && dirJump) || body.charCtrl)
                velo = dir.normalized* dirFactor * jump;
            body.velocity = Vector3.up * jump + velo;
            graphic.Jump();
        }

        //Roll implemented here
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if(Input.GetKeyDown(KeyCode.Tab))
        if (input.magnitude > 0 && body.isGrounded(body.gnd) && nextRoll < Time.time) {
            Roll(input, cam.x);
        }
    }

    public void Roll(Vector3 dir, float x = 0) {
        mode = true;
        isRolling = true;
        body.velocity = Quaternion.Euler(0, x, 0) * (dir.normalized * roll);
        body.shapeCollider(bend.cHeight, bend.cRadius, Vector3.zero);
        graphic.Roll(dir, x);
        nextRoll = Mathf.Infinity;
    }

    public void endRoll() {
        nextRoll = Time.time + noRoll;
        isRolling = false;
        mode = false;
        body.velocity = Vector3.zero;
        if(body.checkCap(transform.position + Vector3.up * bend.cHeight/2) && body.isGrounded() ) {
            bend.standBy = !bend.crouched && standBy;
            bend.Into();
        } else {
            body.resetCollider();
        }
    }
}
