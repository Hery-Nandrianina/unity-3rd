using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body : MonoBehaviour
{
    public enum Drag
    {
        Exponential,
        Multiply,
        Linear,
    }

    public bool charCtrl;
    public Drag method;
    public float friction, close, holdTime;

    [Space]
    public float stepOffset;
    public float slopeLimit, floorOffset;

    //Temporary fix
    [Space]
    public bool slope;
    public float gnd;
    public float slopeCounter;
    public float slopeSnap;
    [Range(0,360)] public float slopeAngle;
    [Range(0,360)] public float slideAngle;
    public float slideForce;
    
    private float slider = 1, counter = 1;

    private Vector3 velo;
    private bool gravity = true;
    private float hold;

    [Space]
    public bool onGround;
    public LayerMask layerMask;
    [Range(0,1)] public float toGround, radius;
    [Range(0,1)] public float radiusOff;

    private Vector3 to;
    private float capHeight, capRadius;
    private Vector3 capCenter;
    [HideInInspector] public Transform reference;
    private Vector3 colOffset;

    CharacterController character;
    CapsuleCollider cap;
    Rigidbody rb;

    private void Start() {
        cap = GetComponent<CapsuleCollider>();
        capRadius = cap.radius;
        capHeight = cap.height;
        capCenter = cap.center;
        colOffset = Vector3.zero;
        reference = transform;

        if(charCtrl) {
            character = gameObject.AddComponent<CharacterController>();
            character.center = capCenter;
            character.radius = capRadius;
            character.height = capHeight;
            character.slopeLimit = slopeLimit;
            cap.enabled = false;
        } else {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.freezeRotation = true;
            rb.drag = 1;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    // private void OnControllerColliderHit(ControllerColliderHit hit) {
    //     if(velocity.y > 0)
    //         graphic.Cancel();
    // }
    
    public static float Damp(float a, float b, float lambda, float dt)
    {
        return Mathf.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
    }

    private void FixedUpdate() {
        onGround = isGrounded();
        if(colOffset != Vector3.zero) {
            if(charCtrl) {
                character.center = capCenter + reference.rotation * colOffset;
            } else {
                cap.center = capCenter + reference.rotation * colOffset;
            }
        }
        
        if(charCtrl) {
            velo += Vector3.down * 9.8f * Time.fixedDeltaTime;
            // Temporary fix
            if(hold < Time.time)
            switch (method) {
                case Drag.Multiply: {
                    velo.x *= 1 - friction * Time.fixedDeltaTime;
                    velo.z *= 1 - friction * Time.fixedDeltaTime;
                    if(!gravity) velo.y *= 1 - friction * Time.fixedDeltaTime;
                } break;
                case Drag.Exponential: { 
                    velo.x = Damp(velo.x, 0, friction, Time.fixedDeltaTime);
                    velo.z = Damp(velo.z, 0, friction, Time.fixedDeltaTime);
                    if(!gravity) velo.y = Damp(velo.y, 0, friction, Time.fixedDeltaTime);
                } break;
                case Drag.Linear: {
                    velo.x = Mathf.MoveTowards(velo.x, 0, friction);
                    velo.z = Mathf.MoveTowards(velo.z, 0, friction);
                    if(!gravity) velo.y = Mathf.MoveTowards(velo.y, 0, friction);
                } break;
            }

            if(new Vector2(velo.x, velo.z).magnitude < close)
            { velo.x = 0; velo.z = 0; }
            if(velo.y < close && !gravity) velo.y = 0;

            if((isGrounded()) && velo.y < 0)
                velo.y = 0;
            //Is it safe?
            if(!isGrounded(1f))
                character.stepOffset = 0;
            else
                character.stepOffset = stepOffset;
            if(character.enabled)
                character.Move(to + velo * Time.deltaTime);
            to = Vector3.zero;
        }
    }

    //Temporary fix (for rb too)
    // public float getVelocity() {
    //     return (character.velocity.magnitude / 4);
    // }

    public (Vector3, float) Slope(Vector3 dir) {
        if(!slope)
            return (Vector3.zero, 1);
            
        //Temporary fix
        Vector3 down = Vector3.zero;
        float factor = 1;
        if(velocity.y <= 0) {
            Vector3 normal = gndNormal();
            float effector = Vector3.Angle(Vector3.up, normal)/360;

            if(dir.magnitude == 0) {
                if(Vector3.Angle(normal, Vector3.up) > slideAngle && charCtrl) {
                    down = (normal + Vector3.down*effector*16).normalized * effector * slideForce * slider; 
                    slider = Mathf.Clamp(slider + Time.fixedDeltaTime, 1, 20);
                }
                counter = 1;
            } else {
                if(Vector3.Angle(normal, dir) <= slopeAngle) {
                    down = Vector3.down * effector * slopeSnap; counter = 1;
                } else if(charCtrl) { //Add another variable
                    factor = 1 - Mathf.Clamp01(effector * (counter + slopeCounter));
                    counter = Mathf.Clamp(counter + Time.fixedDeltaTime, 1, 0.4f);
                }
                slider = 1;
            }
        }
        return (down, factor);
    }

    public Vector3 gndNormal() {
        RaycastHit hit;
        //How many for distance?
        if(Physics.Raycast(transform.position, Vector3.down, out hit, 10)) {
            return hit.normal;
        } else
            return Vector3.zero;
    }

    //Rearrange this to (checkTop)
    public bool checkCap(Vector3 position) {
        //Temporary fix
        // return  Physics.CheckCapsule(transform.position + capCenter + Vector3.up * (capHeight/2-capRadius), 
        // capCenter + transform.position, capRadius, layerMask);
        if(charCtrl)
            return  Physics.CheckCapsule(position + character.center + Vector3.up * (character.height/2-character.radius), 
        character.center + position + Vector3.down * (character.height/2-character.radius), character.radius, layerMask);
        else
            return  Physics.CheckCapsule(position + cap.center + Vector3.up * (cap.height/2-cap.radius), 
        cap.center + position + Vector3.down * (cap.height/2-cap.radius), cap.radius, layerMask);
    }

    /* private void OnDrawGizmos() {
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down*(1+toGround));
        Gizmos.DrawWireSphere(capCenter + Vector3.down * (capHeight/2-capRadius) + transform.position, capRadius);
        Gizmos.DrawWireSphere(transform.position + capCenter + Vector3.up * (capHeight/2-capRadius), capRadius);
    } */

    /* public bool isFloor(){
        // return Physics.Raycast(transform.position, Vector3.up, 1.3f + jump/4);
        return Physics.CheckCapsule(transform.position, transform.position + Vector3.up*(1+jumpOff), radius, layerMask);
    } */
    
    public void shapeCollider(float height, float radius, Vector3 offset, Transform refer = null) {
        colOffset = offset;
        if(refer != null)
            reference = refer;

        if(charCtrl) {
            to = Vector3.zero;
            character.height = (height != 0) ? height:capHeight;
            character.radius = (radius != 0) ? radius:capRadius;
            if(height != 0)
            character.center = capCenter + Vector3.down * ((capHeight - height) / 2) ;
        } else {
            cap.height = (height != 0) ? height:capHeight;
            cap.radius = (radius != 0) ? radius:capRadius;
            if(height != 0)
            cap.center = capCenter + Vector3.down * ((capHeight - height) / 2) ;
        }
    }

    public void resetCollider() {
        reference = transform;
        colOffset = Vector3.zero;
        if(charCtrl) {
            character.height = capHeight;
            character.radius = capRadius;
            character.center = capCenter;
        } else {
            cap.height = capHeight;
            cap.radius = capRadius;
            cap.center = capCenter;
        }
    }

    public bool isGrounded(float offset = 0) {
        // return Physics.Raycast(transform.position, Vector3.down, 1.4f);
        //Why is it even too high on rb?
        return Physics.CheckCapsule(transform.position, transform.position
            + Vector3.down*(1+toGround+offset-((charCtrl) ? floorOffset : 0)), radius - ((charCtrl) ? 0 : radiusOff), layerMask);
    }

    public bool isGrounded(float offset, Vector3 center, float rayon) {
        // return Physics.Raycast(transform.position, Vector3.down, 1.4f);
        //Why is it even too high on rb?
        return Physics.CheckCapsule(transform.position + center, transform.position + center
            + Vector3.down*(1+toGround+offset-((charCtrl) ? floorOffset : 0)), rayon, layerMask);
    }

    public void MoveTo(Vector3 position) {
        //Temporary fix (Is it safe?)
        if(charCtrl)
            // character.Move(position - transform.position);
            to = position - transform.position;
        else
            rb.MovePosition(position);
    }

    // public void VelocityChange(Vector3 value) {
    //     rb.AddForce(velocity, ForceMode.VelocityChange);
    // }

    public Vector3 velocity{
        get { if(charCtrl) return velo; else return rb.velocity; }
        set { if(charCtrl) {velo = value; hold = holdTime + Time.time;} else rb.velocity = value; }
    }

    //How about character controller
    public bool sleep {
        set {
            if(charCtrl) {
                if(value)
                    character.enabled = false;
                else
                    character.enabled = true;
            } else {
                if(value) {
                    rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    rb.isKinematic = true;
                } else {
                    rb.isKinematic = false;
                    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                }
            }
        }
        get {
            if(charCtrl) return character.enabled;
            else return rb.isKinematic;
            
        }
    }

    public bool useGravity {
        get {if(charCtrl) return gravity; else return rb.useGravity;}
        set {if(charCtrl) gravity = value; else rb.useGravity = value;}
    }

    //Push rigidbody
    //Temporary fix
    void OnControllerColliderHit(ControllerColliderHit hit) {
         Rigidbody body = hit.collider.attachedRigidbody;
         if (body == null || body.isKinematic) return;

        // body.velocity += hit.controller.velocity;
        Vector3 force;
        // float weight = 2f;
        float pushPower = 20f;
    
        if (hit.moveDirection.y < -0.3f) {
            // force = new Vector3(0, -0.5f, 0) * 9.8f * weight;
            force = Vector3.zero;
        } else {
            force = hit.controller.velocity * pushPower;
        }
    
        body.AddForceAtPosition(force, hit.point);
     }
}
