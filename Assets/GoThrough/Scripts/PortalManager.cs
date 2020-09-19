using GoThrough.Utility;
using System.Collections.Generic;

namespace GoThrough
{
    public class PortalManager : MonoBehaviourSingleton<PortalManager>
    {
        public IReadOnlyCollection<Portal> Portals => this.portals;

        private HashSet<Portal> portals = new HashSet<Portal>();

        public void Subscribe(Portal portal)
        {
            if (!portal.Destination)
                throw new System.NullReferenceException($"{portal.name} has no destination. Please, set a destination for it.");
            this.portals.Add(portal);
        }

        public void Unsubscribe(Portal portal)
        {
            this.portals.Remove(portal);
        }
    }
}
