using GoThrough.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GoThrough
{
    public class PortalManager : MonoBehaviourSingleton<PortalManager>
    {
        public int recursionMaxDepth = 5;
        public int maxRenderTextureAllocations = 100;

        private HashSet<Portal> portals = new HashSet<Portal>();
        private Camera portalCamera;

        private void Awake()
        {
            var cameraObject = new GameObject("PortalCamera");
            cameraObject.transform.SetParent(this.transform);
            this.portalCamera = cameraObject.AddComponent<Camera>();
            this.portalCamera.enabled = false;

            RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
            RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
        }

        private void OnDestroy()
        {
            RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_beginCameraRendering;
            RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
        }

        public void Subscribe(Portal portal)
        {
            this.portals.Add(portal);
        }

        public void Unsubscribe(Portal portal)
        {
            this.portals.Remove(portal);
        }

        private void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext ctx, Camera cam)
        {
            if (cam.cameraType != CameraType.Game)
                return;

            foreach(var portal in this.portals)
                portal.SetupScreen(cam);

            var graph = new VisibilityGraph(cam, this.portalCamera, this.portals);
            graph.Render(ctx);
        }

        private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext ctx, Camera cam)
        {
            RenderTexturePool.Instance.ReleaseAllRenderTextures();
        }
    }
}
