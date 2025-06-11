using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public WheelCollider frontDW, frontPW, rearDW, rearPW;
    public Transform frontD, frontP, rearD, rearP;
    public float maxSteerAngle = 30, motorForce = 50;

    private float steerAngle;

    private void Update() {
        steerAngle = maxSteerAngle * Input.GetAxis("Horizontal");
        frontDW.steerAngle = frontPW.steerAngle = steerAngle;

        rearDW.motorTorque = rearPW.motorTorque = motorForce * Input.GetAxis("Vertical");
        UpdateWheelPose(rearPW, rearP);
        UpdateWheelPose(frontPW, frontP);
        UpdateWheelPose(rearDW, rearD);
        UpdateWheelPose(frontDW, frontD);
    }

    private void UpdateWheelPose(WheelCollider wheel, Transform _transform) {
        Vector3 pos = _transform.position;
        Quaternion quat = _transform.rotation;

        wheel.GetWorldPose(out pos, out quat);

        _transform.position = pos;
        _transform.rotation = quat;
    }
}
