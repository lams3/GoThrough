using GoThrough.Utility;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GoThrough
{
    public class VisibilityGraph
    {
        private class Node
        {
            private struct VisiblePortalResources
            {
                public Portal visiblePortal;
                public RenderTexturePool.Item poolItem;
                public Texture originalTexture;
            }

            public Portal portal;
            private Matrix4x4 viewPose;
            private Camera portalCamera;
            private List<Node> dependencies = new List<Node>();
            private int depth;

            public Node(Portal portal, Camera portalCamera, HashSet<Portal> activePortals, Matrix4x4 viewPose, int currentDepth = 1)
            {
                this.portal = portal;
                this.portalCamera = portalCamera;
                this.depth = currentDepth;
                
                Matrix4x4 newViewPose = portal.destiny.OutTransform.localToWorldMatrix * portal.transform.worldToLocalMatrix * viewPose;
                this.viewPose = newViewPose;

                if (currentDepth > PortalConfig.Instance.recursionMaxDepth)
                    return;

                foreach (Portal p in activePortals)
                {
                    if (p == portal.destiny)
                        continue;

                    portalCamera.transform.SetPositionAndRotation(newViewPose.GetColumn(3), newViewPose.rotation);

                    if (p.IsVisibleWithin(portalCamera, portal.destiny))
                        this.dependencies.Add(new Node(p, portalCamera, activePortals, newViewPose, currentDepth + 1));
                }
            }

            public void Render(ScriptableRenderContext ctx, Camera baseCamera, out RenderTexturePool.Item temporaryPoolItem, out Texture originalTexture)
            {
                var visiblePortalResourcesList = new List<VisiblePortalResources>();

                foreach (var node in this.dependencies)
                {
                    node.Render(ctx, baseCamera, out var visiblePortalTempPoolItem, out var visiblePortalOriginalTexture);
                    visiblePortalResourcesList.Add(new VisiblePortalResources
                    {
                        visiblePortal = node.portal,
                        poolItem = visiblePortalTempPoolItem,
                        originalTexture = visiblePortalOriginalTexture
                    });
                }

                temporaryPoolItem = RenderTexturePool.Instance.GetRenderTexture();
                this.portalCamera.targetTexture = temporaryPoolItem.renderTexture;

                this.portalCamera.transform.SetPositionAndRotation(this.viewPose.GetColumn(3), this.viewPose.rotation);
                Matrix4x4 cameraTransform = Matrix4x4.Transpose(Matrix4x4.Inverse(portalCamera.worldToCameraMatrix));
                Vector4 clippingPlane = this.portal.destiny.GetClippingPlane();
                this.portalCamera.projectionMatrix = baseCamera.CalculateObliqueMatrix(cameraTransform * clippingPlane);

                if (this.depth > PortalConfig.Instance.recursionMaxDepth)
                {
                    int actualCullingMask = this.portalCamera.cullingMask;
                    this.portalCamera.cullingMask = 0;
                    UniversalRenderPipeline.RenderSingleCamera(ctx, this.portalCamera);
                    this.portalCamera.cullingMask = actualCullingMask;
                }
                else
                {
                    this.portal.destiny.DisableScreen();
                    UniversalRenderPipeline.RenderSingleCamera(ctx, this.portalCamera);
                    this.portal.destiny.EnableScreen();
                }


                foreach (var resources in visiblePortalResourcesList)
                {
                    resources.visiblePortal.SetTexture(resources.originalTexture);

                    if (resources.poolItem != null)
                        RenderTexturePool.Instance.ReleaseRenderTexture(resources.poolItem);
                }

                originalTexture = this.portal.GetTexture();
                this.portal.SetTexture(temporaryPoolItem.renderTexture);
            }


            public override string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append('\t', this.depth);

                var p = this.viewPose.GetColumn(3);
                var r = this.viewPose.rotation.eulerAngles;
                stringBuilder.Append($"{this.portal.name}: p{p}; r{r}.");

                stringBuilder.Append('\n');

                foreach (var node in this.dependencies)
                    stringBuilder.Append(node.ToString());

                return stringBuilder.ToString();
            }
        }

        private Camera baseCamera;
        private Camera portalCamera;

        private List<Node> dependencies = new List<Node>();

        public VisibilityGraph(Camera baseCamera, Camera portalCamera, HashSet<Portal> activePortals)
        {
            this.baseCamera = baseCamera;
            this.portalCamera = portalCamera;

            portalCamera.cullingMask = baseCamera.cullingMask;
            portalCamera.projectionMatrix = baseCamera.projectionMatrix;

            foreach (Portal p in activePortals)
                if (p.IsVisibleFrom(baseCamera))
                    this.dependencies.Add(new Node(p, portalCamera, activePortals, baseCamera.transform.localToWorldMatrix));
        }

        public void Render(ScriptableRenderContext ctx)
        {
            foreach (var node in this.dependencies)
                node.Render(ctx, baseCamera, out _, out _);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(this.baseCamera.name);

            stringBuilder.Append("\n");

            foreach (var node in this.dependencies)
                stringBuilder.Append(node.ToString());

            return stringBuilder.ToString();
        }
    }
}
