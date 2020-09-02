using GoThrough.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace GoThrough
{
    public class Portal : MonoBehaviour
    {
        #region NewCode

        public Transform OutTransform => this.outTransform;

        public bool IsVisibleFrom(Camera camera)
        {
            return this.screen.IsVisibleFrom(camera);
        }

        public bool IsVisibleWithin(Camera camera, Portal portal)
        {
            return camera.BoundsOverlap(portal.screenMeshFilter, this.screenMeshFilter);
        }

        public void SetTexture(Texture texture)
        {
            this.screen.material.SetTexture("_MainTex", texture);
        }

        public Texture GetTexture()
        {
            return this.screen.material.GetTexture("_MainTex");
        }

        public Vector4 GetClippingPlane()
        {
            Plane clipPlane = new Plane(-this.OutTransform.forward, this.OutTransform.position);
            Vector4 clipPlaneVector = new Vector4(clipPlane.normal.x, clipPlane.normal.y, clipPlane.normal.z, clipPlane.distance);
            return clipPlaneVector;
        }

        public void SetupScreen(Camera camera)
        {
            float halfHeight = camera.nearClipPlane * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * camera.aspect;
            float dstToNearPlaneCorner = new Vector3(halfWidth, halfHeight, camera.nearClipPlane).magnitude;
            float screenThickness = 2.0f * dstToNearPlaneCorner;

            Vector3 offset = -Vector3.forward * screenThickness * 0.5f;

            Transform screenT = this.screen.transform;
            screenT.localPosition = this.originalScreenPosition + offset;
            screenT.localScale = new Vector3(screenT.localScale.x, screenT.localScale.y, screenThickness);
        }

        public void EnableScreen()
        {
            this.screen.shadowCastingMode = ShadowCastingMode.TwoSided;
            this.screen.enabled = true;
        }

        public void DisableScreen()
        {
            this.screen.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            this.screen.enabled = false;
        }

        #endregion

        #region Parameters

        public Portal destination;

        #endregion

        #region PrivateMembers
        private MeshRenderer screen;
        private MeshFilter screenMeshFilter;
        private Vector3 originalScreenPosition;

        private Transform outTransform;
        private Dictionary<PortalTraveller, Vector3> trackedTravellers = new Dictionary<PortalTraveller, Vector3>();

        #endregion

        #region Lifecycle

        private void Awake()
        {
            this.outTransform = this.transform.Find("OutTransform");
            this.screen = this.transform.Find("Screen").GetComponent<MeshRenderer>();

            this.screenMeshFilter = this.screen.GetComponent<MeshFilter>();
            this.originalScreenPosition = this.screen.transform.localPosition;
        }

        private void OnEnable()
        {
            PortalManager.Instance?.Subscribe(this);
        }

        private void OnDisable()
        {
            PortalManager.Instance?.Unsubscribe(this);
        }

        private void FixedUpdate()
        {
            if (this.destination)
                this.StartCoroutine(this.HandleAllTravellers());
        }

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            PortalTraveller traveller = rb ? rb.GetComponent<PortalTraveller>() : other.GetComponent<PortalTraveller>();
            this.BeginTracking(traveller);
        }

        private void OnTriggerExit(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            PortalTraveller traveller = rb ? rb.GetComponent<PortalTraveller>() : other.GetComponent<PortalTraveller>();
            this.StopTracking(traveller);
        }

        #endregion

        #region Teleporting
        
        private void BeginTracking(PortalTraveller traveller)
        {
            if (traveller && !this.trackedTravellers.ContainsKey(traveller))
            {
                this.trackedTravellers.Add(traveller, traveller.transform.position);
                traveller.BeginTransition(this.transform, this.destination.OutTransform);
            }
        }

        private IEnumerator HandleAllTravellers()
        {
            yield return new WaitForFixedUpdate();

            PortalTraveller[] travellers = this.trackedTravellers.Keys.ToArray();
            foreach (PortalTraveller traveller in travellers)
                this.HandleTraveller(traveller);
        }

        private void HandleTraveller(PortalTraveller traveller)
        {
            if (!this.trackedTravellers.ContainsKey(traveller))
                return;

            Vector3 oldPos = this.transform.InverseTransformPoint(this.trackedTravellers[traveller]);
            Vector3 newPos = this.transform.InverseTransformPoint(traveller.transform.position);

            bool passFront = oldPos.z >= 0 && newPos.z < 0;
            if (passFront && !traveller.TeleportedThisFrame)
            {
                this.StopTracking(traveller);
                traveller.Teleport(this, this.destination);
                this.destination.BeginTracking(traveller);
                return;
            }

            this.trackedTravellers[traveller] = traveller.transform.position;
        }

        private void StopTracking(PortalTraveller traveller)
        {
            if (traveller && this.trackedTravellers.ContainsKey(traveller))
            {
                this.trackedTravellers.Remove(traveller);
                traveller.EndTransition();
            }
        }

        #endregion
    }
}