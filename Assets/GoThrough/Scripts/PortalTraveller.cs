using GoThrough.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GoThrough
{
    public class PortalTraveller : MonoBehaviour
    {
        [SerializeField]
        private GameObject graphics;

        private GameObject graphicsClone;
        private List<Material> originalMaterials;
        private List<Material> cloneMaterials;
        private new Rigidbody rigidbody;

        private Transform source, destination;

        public event Action<Portal, Portal> OnTeleport = (source, destiny) => { };

        private bool teleportedThisFrame = false;

        public bool TeleportedThisFrame => this.teleportedThisFrame;

        public void Teleport(Portal source, Portal destination)
        {
            Matrix4x4 localTransform = destination.OutTransform.localToWorldMatrix * source.transform.worldToLocalMatrix * this.transform.localToWorldMatrix;
            this.transform.SetPositionAndRotation(localTransform.GetColumn(3), localTransform.rotation);
            this.rigidbody.position = localTransform.GetColumn(3);
            this.rigidbody.rotation = localTransform.rotation;

            Matrix4x4 globalTransform = destination.OutTransform.localToWorldMatrix * source.transform.worldToLocalMatrix;
            this.rigidbody.velocity = globalTransform.MultiplyVector(rigidbody.velocity);
            this.rigidbody.angularVelocity = globalTransform.MultiplyVector(rigidbody.angularVelocity);

            this.OnTeleport.Invoke(source, destination);

            this.teleportedThisFrame = true;
        }

        public void BeginTransition(Transform source, Transform destination)
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

        public void EndTransition()
        {
            this.source = this.destination = null;

            this.graphicsClone.SetActive(false);

            foreach (Material mat in this.originalMaterials)
                mat.SetInt("_UseClipPlane", 0);

            foreach (Material mat in this.cloneMaterials)
                mat.SetInt("_UseClipPlane", 0);
        }

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
    }
}