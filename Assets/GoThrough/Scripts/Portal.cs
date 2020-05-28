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
        private Transform destiny;

        [SerializeField]
        private new MeshRenderer renderer;

        private RenderTexture renderTexture;
        private float nearClipOffset = 0.05f;
        private float nearClipLimit = 0.2f;

        private Vector3 originalScreenPosition;

        private Dictionary<PortalTraveller, Vector3> trackedTravellers = new Dictionary<PortalTraveller, Vector3>();

        private void Awake()
        {
            this.originalScreenPosition = this.renderer.transform.localPosition;
        }

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
                float viewDot = Vector3.Dot((camera.transform.position - this.originalScreenPosition).normalized, -this.renderer.transform.forward);
                bool cameraIsInFront = viewDot >= 0;
                bool isVisibleFromCamera = this.renderer.IsVisibleFrom(camera);
                bool shouldRender = cameraIsInFront && isVisibleFromCamera;

                if (!shouldRender)
                {
                    this.renderer.enabled = false;
                    return;
                }

                this.renderer.enabled = true;

                this.CreateViewTexture();
                this.ProtectFromNearPlaneClipping(camera);

                Matrix4x4 matrix = this.destiny.transform.localToWorldMatrix * this.transform.worldToLocalMatrix * camera.transform.localToWorldMatrix;
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
                    Matrix4x4 matrix = this.destiny.transform.localToWorldMatrix * this.transform.worldToLocalMatrix * traveller.transform.localToWorldMatrix;
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
                this.renderer.material.SetTexture("_MainTex", renderTexture);
            }
        }

        private void ProtectFromNearPlaneClipping(Camera camera)
        {
            float halfHeight = camera.nearClipPlane * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * camera.aspect;
            float dstToNearPlaneCorner = new Vector3(halfWidth, halfHeight, camera.nearClipPlane).magnitude;

            Transform screenT = this.renderer.transform;
            float camFacing = 0.5f * Mathf.Sign(Vector3.Dot(this.transform.forward, this.transform.position - camera.transform.position));
            screenT.localScale = new Vector3(screenT.localScale.x, screenT.localScale.y, dstToNearPlaneCorner);
            screenT.localPosition = this.originalScreenPosition + (Vector3.forward * dstToNearPlaneCorner * camFacing);
        }

        // Use custom projection matrix to align portal camera's near clip plane with the surface of the portal
        // Note that this affects precision of the depth buffer, which can cause issues with effects like screenspace AO
        private void SetProjectionMatrix(Camera template)
        {
            // Learning resource:
            // http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
            Transform clipPlane = this.destiny.transform;
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