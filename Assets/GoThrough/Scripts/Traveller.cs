using GoThrough.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace GoThrough
{
    /// <summary>
    /// Placed on a object to make it able to go through Portals.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Traveller : MonoBehaviour
    {
        #region SerializedFields

        [SerializeField]
        private GameObject graphics;

        #endregion

        #region PrivateFields

        private GameObject graphicsClone;
        private List<Material> originalMaterials;
        private List<Material> cloneMaterials;
        private new Rigidbody rigidbody;

        private Transform source, destination;

        private bool teleportedThisFrame = false;

        #endregion

        #region InternalProperties
        
        internal bool TeleportedThisFrame => this.teleportedThisFrame;

        #endregion

        #region Events

        public event OnEnterPortalZoneDelegate OnEnterPortalZone = (t, p) => { };
        public event OnLeavePortalZoneDelegate OnLeavePortalZone = (t, p) => { };
        public event OnTeleportDelegate OnTeleport = (t, s, d) => { };

        #endregion

        #region Lifecycle

        private void Awake()
        {
            this.rigidbody = this.GetComponent<Rigidbody>();
        }

        private void LateUpdate()
        {
            if (this.source && this.destination)
            {
                Matrix4x4 cloneTransform = this.destination.localToWorldMatrix * this.source.worldToLocalMatrix * this.graphics.transform.localToWorldMatrix;

                foreach (Material mat in this.originalMaterials)
                {
                    mat.SetVector("_ClipPlaneCenter", this.source.position);
                    mat.SetVector("_ClipPlaneNormal", -this.source.forward);
                }

                foreach (Material mat in this.cloneMaterials)
                {
                    mat.SetVector("_ClipPlaneCenter", this.destination.position);
                    mat.SetVector("_ClipPlaneNormal", this.destination.forward);
                }

                this.graphicsClone.transform.SetPositionAndRotation(cloneTransform.GetColumn(3), cloneTransform.rotation);
            }

            this.teleportedThisFrame = false;
        }

        #endregion

        #region InternalMethods
        /// <summary>
        /// Teleports an Traveller from <paramref name="source"/> to <paramref name="destination"/>.
        /// </summary>
        /// <param name="source">The Portal the traveller is currently on.</param>
        /// <param name="destination">The destination.</param>
        internal void Teleport(Portal source, Portal destination)
        {
            Matrix4x4 localTransform = destination.OutTransform.localToWorldMatrix * source.transform.worldToLocalMatrix * this.transform.localToWorldMatrix;
            this.transform.SetPositionAndRotation(localTransform.GetColumn(3), localTransform.rotation);
            this.rigidbody.position = localTransform.GetColumn(3);
            this.rigidbody.rotation = localTransform.rotation;

            Matrix4x4 globalTransform = destination.OutTransform.localToWorldMatrix * source.transform.worldToLocalMatrix;
            this.rigidbody.velocity = globalTransform.MultiplyVector(rigidbody.velocity);
            this.rigidbody.angularVelocity = globalTransform.MultiplyVector(rigidbody.angularVelocity);

            this.teleportedThisFrame = true;
        }

        /// <summary>
        /// Puts the Traveller in a transition state between two transforms.
        /// </summary>
        /// <param name="source">The Transform the traveller is currently on.</param>
        /// <param name="destination">The destination.</param>
        internal void BeginTransition(Transform source, Transform destination)
        {
            if (this.graphicsClone == null)
            {
                this.graphicsClone = Instantiate(this.graphics);
                this.graphicsClone.transform.parent = this.graphics.transform.parent;
                this.graphicsClone.transform.localScale = this.graphics.transform.localScale;
                this.originalMaterials = this.graphics.GetMaterials();
                this.cloneMaterials = this.graphicsClone.GetMaterials();
            }

            this.source = source;
            this.destination = destination;

            this.graphicsClone?.SetActive(true);

            foreach (Material mat in this.originalMaterials)
                mat.SetInt("_UseClipPlane", 1);

            foreach (Material mat in this.cloneMaterials)
                mat.SetInt("_UseClipPlane", 1);
        }

        /// <summary>
        /// Ends any transition ocurring on this Traveller.
        /// </summary>
        internal void EndTransition()
        {
            this.source = this.destination = null;

            this.graphicsClone.SetActive(false);

            foreach (Material mat in this.originalMaterials)
                mat.SetInt("_UseClipPlane", 0);

            foreach (Material mat in this.cloneMaterials)
                mat.SetInt("_UseClipPlane", 0);
        }

        /// <summary>
        /// Invokes the OnEnterPortalZone event on this Traveller.
        /// </summary>
        /// <param name="portal">The Portal calling the event.</param>
        internal void InvokeOnEnterPortalZone(Portal portal)
        {
            this.OnEnterPortalZone.Invoke(this, portal);
        }

        /// <summary>
        /// Invokes the OnLeavePortalZone event on this Traveller.
        /// </summary>
        /// <param name="portal">The Portal calling the event.</param>
        internal void InvokeOnLeavePortalZone(Portal portal)
        {
            this.OnLeavePortalZone.Invoke(this, portal);
        }

        /// <summary>
        /// Invokes the OnTeleport event on this Traveller.
        /// </summary>
        /// <param name="source">The source Portal.</param>
        /// <param name="destination">The destination Portal.</param>
        internal void InvokeOnTeleport(Portal source, Portal destination)
        {
            this.OnTeleport.Invoke(this, source, destination);
        }

        #endregion

        #region InnerTypes

        public delegate void OnEnterPortalZoneDelegate(Traveller traveller, Portal portal);
        public delegate void OnLeavePortalZoneDelegate(Traveller traveller, Portal portal);
        public delegate void OnTeleportDelegate(Traveller traveller, Portal source, Portal destination);

        #endregion
    }
}