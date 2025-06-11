using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamControl : MonoBehaviour
{
	public bool control;
    public float sensX, sensY, minY, maxY, dist;
	[HideInInspector] public Vector2 tempSens;
	public float sensDir;
	public float smoothSpeed;
	[Range(0,1f)] public float offset;
	public Transform target;
	public LayerMask layerMask;
	
	[Space]
	[Range(0,1f)] public float hideDist;
	public Transform point;
	public LayerMask playerCulling;

	[Space]
	[Range(0, 1)] public float colRadius;
	public LayerMask camMask;

	[Header("Debug")]
	public bool inside;
	public bool usingRay, dangerZone;
	
	[HideInInspector]
	public float x, y;
	Camera cam;
	Aim shoot;
	Body body;
	Move move;

	private Vector3 tempPos;

	private void Start() {
		tempSens = new Vector2(sensX, sensY);
		tempPos =  Vector3.zero;

		cam = GetComponent<Camera>();
		shoot = GetComponent<Aim>();
		move = GameObject.FindGameObjectWithTag("Player").GetComponent<Move>();
		body = move.GetComponent<Body>();
	}

	// private float yAccumulatedInput, xAccumulatedInput, timeSinceLastSample;
	// const float inputSampleTime = 1.0f / 30f;

	private void Update() {
		// Deprecated since control is used for real
		// if(Input.GetKeyDown(KeyCode.A))
		// 	control = !control;

		/* yAccumulatedInput += Input.GetAxis("Mouse Y");
        xAccumulatedInput += Input.GetAxis("Mouse X");

		timeSinceLastSample += Time.deltaTime;

		if(timeSinceLastSample >= inputSampleTime) {
			x += xAccumulatedInput * tempSens.x;
			y += yAccumulatedInput * tempSens.y;

			timeSinceLastSample = 0;
			xAccumulatedInput = 0;
			yAccumulatedInput = 0;
		} */

		x += Input.GetAxis("Mouse X") * tempSens.x + ((!shoot.isAiming && body.isGrounded()) ? Input.GetAxis("Horizontal") * sensDir : 0);
		y += Input.GetAxis("Mouse Y") * tempSens.y;
		x = Clamp0360(x);
		
		y = Mathf.Clamp (y, minY, maxY);

		if(!control) return;

		dist += Input.GetAxis("Mouse ScrollWheel") * -2;
	}

	public static float Clamp0360(float eulerAngles)
     {
         float result = eulerAngles - Mathf.CeilToInt(eulerAngles / 360f) * 360f;
         if (result < 0)
         {
             result += 360f;
         }
         return result;
     }

	void LateUpdate(){
		if(!control) return;
		
		RaycastHit hit;
		Vector3 newPosition, position;

		position = Quaternion.Euler (y, x, 0) * Vector3.forward * -dist + target.position;
		RaycastHit[] hitArray;

		dangerZone = false;
		usingRay = false;
		inside = false;

		if((hitArray = Physics.SphereCastAll(target.position, colRadius, (position - target.position).normalized, dist, layerMask)).Length > 0) {
			while(true) {
			hit = hitArray[0];
			if( usingRay = (hit.distance == 0 && hit.point == Vector3.zero) )
				if(!Physics.Raycast(target.position, (position - target.position).normalized, out hit, dist, layerMask)) {
					newPosition = position;
					dangerZone = true;
					break;
				}

			newPosition = hit.point + hit.normal*offset;// - target.position;
			
			if(hit.collider.gameObject.layer == LayerMask.NameToLayer("camPass"))
				if( !(inside = Physics.CheckSphere(position + target.position, colRadius, camMask)) )
					newPosition = position;
			break;
			}
		} else {
			newPosition = position;
			// Debug.Log("Normal");
		}

		// tempPos = Vector3.Lerp(tempPos, newPosition, smoothSpeed);
		// transform.position = target.position + tempPos;
		transform.position = Vector3.Lerp(transform.position, newPosition, smoothSpeed);
		// transform.position = Vector3.MoveTowards(transform.position, newPosition, smoothSpeed * 0.5f);
		// transform.position = newPosition;
		transform.LookAt (target);
		// transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position), 0.8f);

		if(Mathf.Min(Vector3.Distance(transform.position, target.position),
						Vector3.Distance(transform.position, point.position)) <= hideDist) {
			cam.cullingMask = playerCulling;
			// transform.rotation = Quaternion.LookRotation(transform.position - target.position);
			// sensX = -Mathf.Abs(sensX);
		} else {
			cam.cullingMask = -1;
		}
	}

	/* private void OnDrawGizmos() {
		Gizmos.DrawLine(target.position, target.position + Quaternion.Euler (y, x, 0) * Vector3.forward * -dist);
	} */
}
