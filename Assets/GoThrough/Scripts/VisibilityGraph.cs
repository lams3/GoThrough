using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Rendering;

namespace GoThrough
{
    public partial class VisibilityGraph
    {
        private PortalRenderer renderer;
        private List<Node> dependencies = new List<Node>();

        public VisibilityGraph(PortalRenderer renderer)
        {
            this.renderer = renderer;

            foreach (Portal p in PortalManager.Instance.Portals)
                if (p.IsVisibleFrom(this.renderer.BaseCamera))
                    this.dependencies.Add(new Node(this.renderer, p, renderer.BaseCamera.transform.localToWorldMatrix));
        }

        public void Render(ScriptableRenderContext ctx)
        {
            foreach (var node in this.dependencies)
                node.Render(ctx, out _, out _);
        }

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
    }
}
