using UnityEngine;

namespace GoThrough.Samples
{
    public class PlayerController : MonoBehaviour
    {
        public float lookSensitivity = 100.0f;
        public float moveSpeed = 2.0f;
        public new Camera camera;

        private new Rigidbody rigidbody;
        private float xRotation;

        private void Awake()
        {
            this.rigidbody = this.GetComponent<Rigidbody>();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            this.Look();
            this.Move();
        }

        private void Look()
        {
            Vector2 lookDelta = new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y")) * this.lookSensitivity * Time.deltaTime;

            this.transform.Rotate(Vector3.up, lookDelta.x, Space.Self);

            this.xRotation += lookDelta.y;
            this.xRotation = Mathf.Clamp(this.xRotation, -90.0f, 90.0f);
            this.camera.transform.localRotation = Quaternion.Euler(this.xRotation, 0.0f, 0.0f);
        }

        private void Move()
        {
            Vector3 moveDelta = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical")) * this.moveSpeed;
            Vector3 targetVelocity = this.transform.TransformVector(moveDelta);

            var velocity = this.rigidbody.velocity;
            var velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -5, 5);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -5, 5);
            velocityChange.y = 0;
            rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }
}
