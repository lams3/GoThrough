using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 2.0f;
    public float rotateSpeed = 2.0f;

    private new Rigidbody rigidbody;

    private void Awake()
    {
        this.rigidbody = this.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // Rotate around y - axis
        this.transform.Rotate(0, Input.GetAxis("Horizontal") * this.rotateSpeed, 0);

        // Move forward / backward
        Vector3 forward = this.transform.TransformDirection(Vector3.forward);
        float curSpeed = this.speed * Input.GetAxis("Vertical");
        this.rigidbody.AddForce(forward * curSpeed * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }
}
