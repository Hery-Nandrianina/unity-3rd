using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cover : MonoBehaviour
{
    public bool cover;
    public LayerMask mask;
    public float maxAngle, distance;
    public Vector2 corner;
    public float cSpeed, offset;
    private Vector3 dir;
    private bool isSmoothing;
    private float sign = 1;
    [SerializeField] private float blend = 0.4f;

    Move move;
    Graphic graphic;
    Animator anim;
    Body body;
    CamControl cam;

    private void Start() {
        move = GetComponent<Move>();
        graphic = GetComponentInChildren<Graphic>();
        anim = GetComponentInChildren<Animator>();
        body = GetComponent<Body>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CamControl>();
    }

    /* private void OnDrawGizmos() {
        if(cover && !isSmoothing) {
            Vector3 cross = -Vector3.Cross(Vector3.up, dir);
            Vector3 start = transform.position + (-Vector3.Cross(Vector3.up, dir) * corner.x * sign);
            Gizmos.DrawLine(start, start + -dir * distance);
            start = transform.position;
            Gizmos.DrawLine(start, start + -dir * distance);
            start = transform.position + Vector3.up * corner.y;
            Gizmos.DrawLine(start, start + cross * sign * corner.x);
        }
    } */

    private void FixedUpdate() {
        if(cover && !isSmoothing) {
            float input = Input.GetAxis("Horizontal");
            Vector3 cross = -Vector3.Cross(Vector3.up, dir);
            // RaycastHit point;
            // if(Physics.Raycast(transform.position, -dir, out point, distance, mask))//Not on center //Is it safe without angular limit
            //     dir = point.normal;
            // graphic.transform.rotation = Quaternion.RotateTowards(graphic.transform.rotation, Quaternion.LookRotation(dir), 8f);
            if(input != 0) {
                sign = Mathf.Sign(input);
                RaycastHit hit;
                if(!Physics.Raycast(transform.position + Vector3.up * corner.y, cross * sign, out hit, corner.x*1.5f, mask)) {
                    if(Physics.Raycast(transform.position + (cross * corner.x * sign) + Vector3.up * corner.y, -dir, distance)) {
                        anim.SetFloat("coverSpeed", Mathf.Max(Mathf.Abs(input), blend)*sign, 0.2f, Time.deltaTime);
                        body.MoveTo(transform.position + cross*input*cSpeed*Time.deltaTime);
                        return;
                    } else {
                        //Outter corner
                        if(Physics.Raycast(transform.position + (cross * corner.x * sign) - (dir * distance)
                            + Vector3.up * corner.y , cross * -sign , out hit, corner.x, mask)) {
                            Vector3 yLess = hit.normal;
                            yLess.y = 0;
                            if(Input.GetKeyDown(KeyCode.A) && Vector3.Angle(hit.normal, yLess) < maxAngle) {
                                float value = Vector3.Distance(hit.point + dir * offset, transform.position - (dir * distance) );
                                dir = hit.normal;
                                // dir = cross * sign;
                                anim.SetFloat("coverSpeed", Mathf.Max(Mathf.Abs(input), blend)*sign, 0.2f, Time.deltaTime);
                                StartCoroutine(Smooth(hit.point + dir*offset, transform.position + (cross * value * sign), 6));
                                return;
                            }
                        }
                    }
                } else {
                    //Inner corner //Is it safe without angular limit
                    anim.SetFloat("coverSpeed", Mathf.Max(Mathf.Abs(input), blend)*sign, 0.2f, Time.deltaTime);
                    dir = hit.normal;
                    // dir = cross * -sign;
                    // graphic.transform.rotation = Quaternion.LookRotation(dir);
                    // transform.position = hit.point + dir*offset;
                    StartCoroutine(Smooth(hit.point + dir*offset, transform.position, 8));
                } 
            }
            anim.SetFloat("coverSpeed", blend*sign, 0.2f, Time.deltaTime);
        }
    }

    private void Update() {
        RaycastHit hit;
        if(!move.mode && Input.GetKeyDown(KeyCode.W) && !cover && !isSmoothing) {
            Vector3 axis = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            Vector3 rayDir = (axis!=Vector3.zero) ? Quaternion.Euler(0, cam.x, 0) * axis:Quaternion.Euler(0, cam.x, 0)*Vector3.forward;
            if(Physics.Raycast(transform.position, rayDir, out hit, 2, mask)) {
                Vector3 yLess = hit.normal;
                yLess.y = 0;
                if(Vector3.Angle(hit.normal, yLess) < maxAngle) {
                    dir = hit.normal;
                    sign = 1;
                    anim.SetFloat("coverSpeed", blend*sign);
                    StartCoroutine(Smooth(hit.point + dir*offset, transform.position, 12));
                    return;
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.W) && cover && !isSmoothing) {
            endCover();
        }
    }

    IEnumerator Smooth(Vector3 position, Vector3 mid, float w) {
        isSmoothing = true;
        move.mode = true;
        graphic.mode = true;
        anim.SetBool("Cover", true);
        body.sleep = true;
        float omega = Quaternion.Angle(graphic.transform.rotation, Quaternion.LookRotation(dir));
        float delta2 = Vector3.Distance(transform.position, mid);
        float delta = Vector3.Distance(mid, position);
        //For static speed
        float frame = omega / w;
        for (int i = 0; i < frame; i++) {
            if(i < frame*delta2/(delta + delta2))
                transform.position  = Vector3.MoveTowards(transform.position, mid, (delta+delta2)/frame);
            else
                transform.position  = Vector3.MoveTowards(transform.position, position, (delta+delta2)/frame );
            graphic.transform.rotation = Quaternion.RotateTowards(graphic.transform.rotation, Quaternion.LookRotation(dir), omega / frame );
            yield return null;
        }
        cover = true;
        body.sleep = false;
        isSmoothing = false;
    }

    public void endCover() {
        cover = false;
        move.mode = false;
        graphic.mode = false;
        anim.SetBool("Cover", false);
    }
}
