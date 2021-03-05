using UnityEngine;

namespace GoThrough.Samples.UnsolvableMaze
{
    internal class Cell : MonoBehaviour
    {
        internal Portal[] portals;

        private void Awake()
        {
            this.portals = this.GetComponentsInChildren<Portal>();

            foreach (Portal p in this.portals)
                p.Destination = p;
        }
    }
}