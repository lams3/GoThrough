using System.Linq;
using UnityEngine;

namespace GoThrough.Samples
{
    public class Cell : MonoBehaviour
    {
        public enum CellType { 
            Room,
            LCorridor
        }

        public Portal[] portals;
        public CellType cellType = CellType.Room;

        private void Awake()
        {
            FindObjectOfType<PortalTraveller>().OnTeleport += (source, destination) =>
            {
                if (this.portals.Contains(source))
                {
                    var sourceName = $"{this.name}/{source.name}";
                    var destinationName = $"{destination.GetComponentInParent<Cell>()}/{destination.name}";
                    //Debug.Log($"from {sourceName} to {destinationName}");
                }
            };
        }
    }
}