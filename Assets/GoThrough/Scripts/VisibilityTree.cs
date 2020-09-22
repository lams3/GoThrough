using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Rendering;

namespace GoThrough
{
    /// <summary>
    /// The visibility tree used to render Portals in the correct order.
    /// </summary>
    public partial class VisibilityTree
    {
        #region PrivateFields

        private PortalRenderer renderer;
        private List<Node> dependencies = new List<Node>();

        #endregion

        #region Constructors

        public VisibilityTree(PortalRenderer renderer)
        {
            this.renderer = renderer;

            foreach (Portal p in PortalManager.Instance.Portals)
                if (p.IsVisibleFrom(this.renderer.BaseCamera))
                    this.dependencies.Add(new Node(this.renderer, p, renderer.BaseCamera.transform.localToWorldMatrix));
        }

        #endregion

        #region PublicMethods

        /// <summary>
        /// Used to efectivelly Render the tree.
        /// </summary>
        /// <param name="ctx">The rendering context to render.</param>
        public void Render(ScriptableRenderContext ctx)
        {
            foreach (var node in this.dependencies)
                node.Render(ctx, out _, out _);
        }

        /// <summary>
        /// Count the amount of nodes in the tree.
        /// </summary>
        public int GetNodeCount()
        {
            return 1 + this.dependencies.Sum(el => 1 + el.GetChildrenCount());
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(this.renderer.BaseCamera.name);

            stringBuilder.Append("\n");

            foreach (var node in this.dependencies)
                stringBuilder.Append(node.ToString());

            return stringBuilder.ToString();
        }

        #endregion
    }
}
