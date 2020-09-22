using GoThrough.Utility;
using System.Collections.Generic;

namespace GoThrough
{
    /// <summary>
    /// Used to manage currently active Portals.
    /// </summary>
    public class PortalManager : MonoBehaviourSingleton<PortalManager>
    {
        #region PrivateFields

        private HashSet<Portal> portals = new HashSet<Portal>();

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
    }
}
