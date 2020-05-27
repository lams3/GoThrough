using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GoThrough
{
    public class Portal : MonoBehaviour
    {
        [SerializeField]
        private Camera portalCamera;

        [SerializeField]
        private Transform pairPortal;

        private RenderTexture renderTexture;
        private float nearClipOffset = 0.05f;
        private float nearClipLimit = 0.2f;

        private Dictionary<PortalTraveller, Vector3> trackedTravellers = new Dictionary<PortalTraveller, Vector3>();

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += this.Render;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= this.Render;
        }

        private void Render(ScriptableRenderContext context, Camera camera)
        {
            if ((camera.cameraType == CameraType.Game || camera.cameraType == CameraType.SceneView))
            {
                this.CreateViewTexture();

                Matrix4x4 matrix = this.pairPortal.transform.localToWorldMatrix * this.transform.worldToLocalMatrix * camera.transform.localToWorldMatrix;
                this.portalCamera.transform.SetPositionAndRotation(matrix.GetColumn(3), matrix.rotation);

                this.SetProjectionMatrix(camera);

                UniversalRenderPipeline.RenderSingleCamera(context, this.portalCamera);
            }
        }

        private void LateUpdate()
        {
            PortalTraveller[] travellers = this.trackedTravellers.Keys.ToArray();
            foreach (PortalTraveller traveller in travellers)
            {
                if (!this.trackedTravellers.ContainsKey(traveller))
                    continue;

                Vector3 oldPos = this.trackedTravellers[traveller];
                Vector3 newPos = traveller.transform.position;
                Vector3 thisPos = this.transform.position;
                float oldDot = Vector3.Dot(this.transform.forward, (oldPos - thisPos).normalized);
                float newDot = Vector3.Dot(this.transform.forward, (newPos - thisPos).normalized);

                if (oldDot < 0 && newDot >= 0)
                {
                    Matrix4x4 matrix = this.pairPortal.transform.localToWorldMatrix * this.transform.worldToLocalMatrix * traveller.transform.localToWorldMatrix;
                    traveller.transform.SetPositionAndRotation(matrix.GetColumn(3), matrix.rotation);
                    this.trackedTravellers.Remove(traveller);
                    continue;
                }

                this.trackedTravellers[traveller] = traveller.transform.position;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            PortalTraveller traveller = rb ? rb.GetComponent<PortalTraveller>() : other.GetComponent<PortalTraveller>();
            if (traveller)
                this.trackedTravellers.Add(traveller, traveller.transform.position);
        }

        private void OnTriggerExit(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            PortalTraveller traveller = rb ? rb.GetComponent<PortalTraveller>() : other.GetComponent<PortalTraveller>();
            if (traveller)
                this.trackedTravellers.Remove(traveller);
        }

        private void CreateViewTexture()
        {
            if (renderTexture == null || renderTexture.width != Screen.width || renderTexture.height != Screen.height)
            {
                if (renderTexture != null)
                    renderTexture.Release();

                renderTexture = new RenderTexture(Screen.width, Screen.height, 24);

                // Render the view from the portal camera to the view texture
                portalCamera.targetTexture = renderTexture;

                // Display the view texture on the screen of the linked portal
                this.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", renderTexture);
            }
        }

        // Use custom projection matrix to align portal camera's near clip plane with the surface of the portal
        // Note that this affects precision of the depth buffer, which can cause issues with effects like screenspace AO
        private void SetProjectionMatrix(Camera template)
        {
            // Learning resource:
            // http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
            Transform clipPlane = this.pairPortal.transform;
            int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, clipPlane.position - this.portalCamera.transform.position));

            Vector3 camSpacePos = this.portalCamera.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
            Vector3 camSpaceNormal = this.portalCamera.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
            float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + this.nearClipOffset;

            // Don't use oblique clip plane if very close to portal as it seems this can cause some visual artifacts
            if (Mathf.Abs(camSpaceDst) > this.nearClipLimit)
            {
                Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

                // Update projection based on new clip plane
                // Calculate matrix with player cam so that player camera settings (fov, etc) are used
                this.portalCamera.projectionMatrix = template.CalculateObliqueMatrix(clipPlaneCameraSpace);
            }
            else
            {
                this.portalCamera.projectionMatrix = template.projectionMatrix;
            }
        }
    }
}