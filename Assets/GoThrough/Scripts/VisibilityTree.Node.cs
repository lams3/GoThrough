using GoThrough.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GoThrough
{
    internal partial class VisibilityTree
    {
        /// <summary>
        /// Represents a Node of the VisibilityTree.
        /// </summary>
        private class Node
        {
            #region PrivateFields

            private PortalRenderer renderer;
            private Portal portal;
            private Matrix4x4 viewPose;
            private int depth;
            private List<Node> dependencies = new List<Node>();

            #endregion

            #region Constructors

            internal Node(PortalRenderer renderer, Portal portal, Matrix4x4 viewPose, int currentDepth = 1)
            {
                this.renderer = renderer;
                this.portal = portal;
                this.depth = currentDepth;

                Matrix4x4 newViewPose = portal.Destination.OutTransform.localToWorldMatrix * portal.transform.worldToLocalMatrix * viewPose;
                this.viewPose = newViewPose;

                if (currentDepth > this.renderer.MaxRecursionDepth)
                    return;

                foreach (Portal p in PortalManager.Instance.Portals)
                {
                    if (p == portal.Destination)
                        continue;

                    this.renderer.PortalCamera.transform.SetPositionAndRotation(newViewPose.GetColumn(3), newViewPose.rotation);

                    if (p.IsVisibleWithin(this.renderer.PortalCamera, this.portal.Destination))
                        this.dependencies.Add(new Node(this.renderer, p, newViewPose, currentDepth + 1));
                }
            }

            #endregion

            #region InternalMethods

            /// <summary>
            /// Recursivelly renders the Node and it's children in a depth first order.
            /// </summary>
            /// <param name="ctx">The rendering context.</param>
            /// <param name="temporaryPoolItem">An render texture to be released after render.</param>
            /// <param name="originalTexture">The previous texture attached to this Node's Portal.</param>
            internal void Render(ScriptableRenderContext ctx, out RenderTexturePool.Item temporaryPoolItem, out Texture originalTexture)
            {
                var visiblePortalResourcesList = new List<VisiblePortalResources>();

                foreach (var node in this.dependencies)
                {
                    node.Render(ctx, out var visiblePortalTempPoolItem, out var visiblePortalOriginalTexture);
                    visiblePortalResourcesList.Add(new VisiblePortalResources
                    {
                        visiblePortal = node.portal,
                        poolItem = visiblePortalTempPoolItem,
                        originalTexture = visiblePortalOriginalTexture
                    });
                }

                temporaryPoolItem = renderer.RenderTexturePool.GetRenderTexture();
                this.renderer.PortalCamera.targetTexture = temporaryPoolItem.renderTexture;

                this.renderer.PortalCamera.transform.SetPositionAndRotation(this.viewPose.GetColumn(3), this.viewPose.rotation);

                Matrix4x4 cameraTransform = Matrix4x4.Transpose(Matrix4x4.Inverse(this.renderer.PortalCamera.worldToCameraMatrix));
                Vector4 clippingPlane = this.portal.Destination.GetClippingPlane();
                this.renderer.PortalCamera.projectionMatrix = this.renderer.BaseCamera.CalculateObliqueMatrix(cameraTransform * clippingPlane);

                if (this.depth > this.renderer.MaxRecursionDepth)
                {
                    RenderTexture.active = temporaryPoolItem.renderTexture;
                    GL.ClearWithSkybox(true, this.renderer.PortalCamera);
                }
                else
                {
                    this.portal.Destination.DisableScreen();
                    UniversalRenderPipeline.RenderSingleCamera(ctx, this.renderer.PortalCamera);
                    this.portal.Destination.EnableScreen();
                }


                foreach (var resources in visiblePortalResourcesList)
                {
                    resources.visiblePortal.SetTexture(resources.originalTexture);

                    if (resources.poolItem != null)
                        this.renderer.RenderTexturePool.ReleaseRenderTexture(resources.poolItem);
                }

                originalTexture = this.portal.GetTexture();
                this.portal.SetTexture(temporaryPoolItem.renderTexture);
            }

            /// <summary>
            /// Recursivelly counts the amount of Nodes in this subtree.
            /// </summary>
            internal int GetChildrenCount()
            {
                if (this.depth == this.renderer.MaxRecursionDepth)
                    return 0;

                return this.dependencies.Sum(el => 1 + el.GetChildrenCount());
            }

            #endregion

            #region PublicMethods

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

            #endregion

            #region InnerTypes

            private struct VisiblePortalResources
            {
                internal Portal visiblePortal;
                internal RenderTexturePool.Item poolItem;
                internal Texture originalTexture;
            }

            #endregion
        }
    }
}
