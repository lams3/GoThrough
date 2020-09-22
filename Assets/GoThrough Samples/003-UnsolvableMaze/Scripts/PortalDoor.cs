using UnityEngine;

namespace GoThrough.Samples.UnsolvableMaze
{
    [RequireComponent(typeof(Portal))]
    internal class PortalDoor : MonoBehaviour
    {
        internal GameObject door;

        private PlayerController player;
        private Portal portal;
        private bool shouldOpen;

        private GameObject DestinationDoor => this.portal.Destination.GetComponent<PortalDoor>().door;

        private void Awake()
        {
            this.player = FindObjectOfType<PlayerController>();
            this.portal = this.GetComponent<Portal>();

            this.door = new GameObject();
            this.door.transform.SetParent(this.transform, false);

            var doorGfx = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorGfx.transform.SetParent(this.door.transform, false);
            doorGfx.transform.localScale = new Vector3(3.0f, 3.0f, 0.5f);
            doorGfx.transform.localPosition = new Vector3(0.0f, 1.5f, 0.0f);
        }

        private void Update()
        {
            Vector3 desiredPosition = this.shouldOpen ? Vector3.up * 2.75f : Vector3.zero;

            this.door.transform.localPosition = Vector3.MoveTowards(this.door.transform.localPosition, desiredPosition, 2.75f * Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (this.shouldOpen)
                this.DestinationDoor.transform.localPosition = this.door.transform.localPosition;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody == player.GetComponent<Rigidbody>())
                this.shouldOpen = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.attachedRigidbody == player.GetComponent<Rigidbody>())
                this.shouldOpen = false;
        }
    }
}