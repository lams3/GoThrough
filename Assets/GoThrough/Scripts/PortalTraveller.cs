using GoThrough.Utility;
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

        Transform source, destiny;

        public void BeginTransition(Transform source, Transform destiny)
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
            this.destiny = destiny;

            this.graphicsClone?.SetActive(true);

            foreach (Material mat in this.originalMaterials)
                mat.SetInt("_UseClipPlane", 1);

            foreach (Material mat in this.cloneMaterials)
                mat.SetInt("_UseClipPlane", 1);
        }

        public void EndTransition()
        {
            this.source = this.destiny = null;

            this.graphicsClone.SetActive(false);

            foreach (Material mat in this.originalMaterials)
                mat.SetInt("_UseClipPlane", 0);

            foreach (Material mat in this.cloneMaterials)
                mat.SetInt("_UseClipPlane", 0);
        }

        private void LateUpdate()
        {
            if (this.source && this.destiny)
            {
                Matrix4x4 cloneTransform = this.destiny.localToWorldMatrix * this.source.worldToLocalMatrix * this.graphics.transform.localToWorldMatrix;

                foreach (Material mat in this.originalMaterials)
                {
                    mat.SetVector("_ClipPlaneCenter", this.source.position);
                    mat.SetVector("_ClipPlaneNormal", -this.source.forward);
                }

                foreach (Material mat in this.cloneMaterials)
                {
                    mat.SetVector("_ClipPlaneCenter", this.destiny.position);
                    mat.SetVector("_ClipPlaneNormal", this.destiny.forward);
                }

                this.graphicsClone.transform.SetPositionAndRotation(cloneTransform.GetColumn(3), cloneTransform.rotation);
            }
        }
    }
}