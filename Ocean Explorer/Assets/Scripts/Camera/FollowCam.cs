using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Transform target;
    public Vector3 followOffset = new Vector3(0f, 10f, -21f);
    public float lookAheadDst = 10;
    public float smoothTime = .1f;
    public float rotSmoothSpeed = 12.5f;

    Vector3 smoothV;


    void LateUpdate()
    {
        Vector3 targetPos = target.position + target.forward * followOffset.z + target.up * followOffset.y + target.right * followOffset.x;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref smoothV, smoothTime);

        Quaternion rot = transform.rotation;
        transform.LookAt(target.position + target.forward * lookAheadDst);
        Quaternion targetRot = transform.rotation;

        transform.rotation = Quaternion.Slerp(rot, targetRot, Time.deltaTime * rotSmoothSpeed);
    }
}