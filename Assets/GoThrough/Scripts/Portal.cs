using GoThrough.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace GoThrough
{
    /// <summary>
    /// The component representing a portal.
    /// </summary>
    public class Portal : MonoBehaviour
    {
        #region SerializedFields

        [SerializeField]
        private Portal destination;

        #endregion

        #region PublicProperties

        public Portal Destination
        {
            get => this.destination;
            set
            {
                this.destination = value;
                if (this.isActiveAndEnabled)
                    PortalManager.Instance?.Subscribe(this);
            }
        }

        #endregion

        #region InternalProperties

        internal Transform OutTransform => this.outTransform;

        #endregion

        #region Events

        /// <summary>
        /// Called when a traveller enters tracking zone.
        /// </summary>
        public event OnTravellerEnterZoneDelegate OnTravellerEnterZone = (p, t) => { };

        /// <summary>
        /// Called when a traveller leaves tracking zone.
        /// </summary>
        public event OnTravellerLeaveZoneDelegate OnTravellerLeaveZone = (p, t) => { };

        /// <summary>
        /// Called when a traveller is teleported.
        /// </summary>
        public event OnTeleportTravellerDelegate OnTeleportTraveller = (s, d, t) => { };

        #endregion

        #region PrivateMembers
        private MeshRenderer screen;
        private MeshFilter screenMeshFilter;
        private Vector3 originalScreenPosition;

        private Transform outTransform;
        private Dictionary<Traveller, Vector3> trackedTravellers = new Dictionary<Traveller, Vector3>();

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
            if (this.Destination)
                this.StartCoroutine(this.HandleAllTravellers());
        }

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            Traveller traveller = rb ? rb.GetComponent<Traveller>() : other.GetComponent<Traveller>();
            this.BeginTracking(traveller);
        }

        private void OnTriggerExit(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            Traveller traveller = rb ? rb.GetComponent<Traveller>() : other.GetComponent<Traveller>();
            this.StopTracking(traveller);
        }

        #endregion

        #region InternalMethods

        /// <summary>
        /// Checks if the portal is visible by <paramref name="camera"/>.
        /// </summary>
        /// <param name="camera">The camera.</param>
        /// <returns>True if <paramref name="camera"/> can see the portal.</returns>
        internal bool IsVisibleFrom(Camera camera)
        {
            return this.screen.IsVisibleFrom(camera);
        }

        /// <summary>
        /// Checks if the portal can be seen by <paramref name="camera"/> through the frame of <paramref name="frame"/>.
        /// </summary>
        /// <param name="camera">The camera.</param>
        /// <param name="frame">The other portal.</param>
        /// <returns>True if the portal can be seen through the frame of <paramref name="frame"/> by <paramref name="camera"/>.</returns>
        internal bool IsVisibleWithin(Camera camera, Portal frame)
        {
            return camera.BoundsOverlap(frame.screenMeshFilter, this.screenMeshFilter);
        }

        /// <summary>
        /// Sets the portal's screen texture.
        /// </summary>
        /// <param name="texture">The texture to be used.</param>
        internal void SetTexture(Texture texture)
        {
            this.screen.material.SetTexture("_MainTex", texture);
        }

        /// <summary>
        /// Retrieves the texture currently assigned to the portal's screen.
        /// </summary>
        /// <returns>The texture currently assigned.</returns>
        internal Texture GetTexture()
        {
            return this.screen.material.GetTexture("_MainTex");
        }

        /// <summary>
        /// Calculates the clipping plane of the portal in world space.
        /// </summary>
        /// <returns>The clipping plane of the portal in world space.</returns>
        internal Vector4 GetClippingPlane()
        {
            Plane clipPlane = new Plane(-this.OutTransform.forward, this.OutTransform.position);
            Vector4 clipPlaneVector = new Vector4(clipPlane.normal.x, clipPlane.normal.y, clipPlane.normal.z, clipPlane.distance);
            return clipPlaneVector;
        }

        /// <summary>
        /// Setup the screen to avoid nearplane clipping.
        /// </summary>
        /// <param name="camera">The camera to avoid clipping with.</param>
        internal void SetupScreen(Camera camera)
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

        /// <summary>
        /// Enable the screen.
        /// </summary>
        internal void EnableScreen()
        {
            this.screen.shadowCastingMode = ShadowCastingMode.TwoSided;
            this.screen.enabled = true;
        }

        /// <summary>
        /// Disable the screen.
        /// </summary>
        internal void DisableScreen()
        {
            this.screen.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            this.screen.enabled = false;
        }

        #endregion

        #region PrivateMethods
        
        private void BeginTracking(Traveller traveller)
        {
            if (traveller && !this.trackedTravellers.ContainsKey(traveller))
            {
                this.trackedTravellers.Add(traveller, traveller.transform.position);
                traveller.BeginTransition(this.transform, this.Destination?.OutTransform);

                this.OnTravellerEnterZone.Invoke(this, traveller);
                traveller.InvokeOnEnterPortalZone(this);
            }
        }

        private IEnumerator HandleAllTravellers()
        {
            yield return new WaitForFixedUpdate();

            Traveller[] travellers = this.trackedTravellers.Keys.ToArray();
            foreach (Traveller traveller in travellers)
                this.HandleTraveller(traveller);
        }

        private void HandleTraveller(Traveller traveller)
        {
            if (!this.trackedTravellers.ContainsKey(traveller))
                return;

            Vector3 oldPos = this.transform.InverseTransformPoint(this.trackedTravellers[traveller]);
            Vector3 newPos = this.transform.InverseTransformPoint(traveller.transform.position);

            bool passFront = oldPos.z >= 0 && newPos.z < 0;
            if (passFront && !traveller.TeleportedThisFrame)
            {
                this.StopTracking(traveller);

                traveller.Teleport(this, this.Destination);
                
                this.OnTeleportTraveller.Invoke(this, this.Destination, traveller);
                traveller.InvokeOnTeleport(this, this.Destination);
                
                this.Destination.BeginTracking(traveller);
                
                return;
            }

            this.trackedTravellers[traveller] = traveller.transform.position;
        }

        private void StopTracking(Traveller traveller)
        {
            if (traveller && this.trackedTravellers.ContainsKey(traveller))
            {
                this.trackedTravellers.Remove(traveller);
                traveller.EndTransition();

                this.OnTravellerLeaveZone.Invoke(this, traveller);
                traveller.InvokeOnLeavePortalZone(this);
            }
        }

        #endregion

        #region InnerTypes

        public delegate void OnTravellerEnterZoneDelegate(Portal portal, Traveller traveller);
        public delegate void OnTravellerLeaveZoneDelegate(Portal portal, Traveller traveller);
        public delegate void OnTeleportTravellerDelegate(Portal source, Portal destination, Traveller traveller);

        #endregion
    }
}