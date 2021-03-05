using GoThrough.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GoThrough
{
    /// <summary>
    /// Used to manage currently active Portals.
    /// </summary>
    public class PortalManager : MonoBehaviourSingleton<PortalManager>
    {
        #region PrivateFields

        private HashSet<Portal> portals = new HashSet<Portal>();

        private List<PortalRenderer> sceneViewRenderers = new List<PortalRenderer>();

        #endregion
        
        #region PublicProperties

        public IReadOnlyCollection<Portal> Portals => this.portals;

        #endregion

        #region InternalMethods

        /// <summary>
        /// Called by a portal when it is enabled.
        /// </summary>
        /// <param name="portal"></param>
        internal void Subscribe(Portal portal)
        {
            if (!portal.Destination)
                throw new System.NullReferenceException($"{portal.name} has no destination. Please, set a destination for it.");
            this.portals.Add(portal);
        }

        /// <summary>
        /// Called by a portal when it is disabled.
        /// </summary>
        /// <param name="portal"></param>
        internal void Unsubscribe(Portal portal)
        {
            this.portals.Remove(portal);
        }

        #endregion

        #region Lifecycle

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += this.RenderPipelineManager_beginCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= this.RenderPipelineManager_beginCameraRendering;

            foreach (PortalRenderer renderer in this.sceneViewRenderers)
                DestroyImmediate(renderer);

            this.sceneViewRenderers.Clear();
        }

        #endregion

        #region PrivateMethods

        private void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext _, Camera cam)
        {
            if (cam.cameraType == CameraType.SceneView)
                this.sceneViewRenderers.Add(cam.gameObject.GetOrAddComponent<PortalRenderer>());
        }

        #endregion
    }
}
