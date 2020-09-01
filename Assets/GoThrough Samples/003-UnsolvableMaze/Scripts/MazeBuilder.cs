using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GoThrough.Samples
{

    public class MazeBuilder : MonoBehaviour
    {
        public PortalTraveller player;
        
        private Dictionary<Portal, Cell> portalToCell; 
        private Cell[] cells;

        private void Awake()
        {
            this.player.OnTeleport += this.OnPlayerTeleport;

            this.cells = FindObjectsOfType<Cell>();
            this.portalToCell = new Dictionary<Portal, Cell>();
            foreach (Cell cell in this.cells)
                foreach (Portal p in cell.portals)
                    this.portalToCell[p] = cell;

            this.Rebuild();

            this.player.transform.position = this.cells[0].transform.position;
        }

        private void OnPlayerTeleport(Portal source, Portal destination)
        {
            Cell sourceCell = this.portalToCell[source];
            Cell destinationCell = this.portalToCell[destination];
            
            destination.destination = source;

            this.Rebuild(new Cell[] { sourceCell, destinationCell });
        }

        private void Rebuild(IEnumerable<Cell> ignoredCells = null)
        {
            Predicate<Cell> isElegible = (Cell c) => ignoredCells == null || !ignoredCells.Contains(c);

            foreach (Cell cell in this.cells)
                if (isElegible(cell))
                    foreach (Portal portal in cell.portals)
                        portal.destination = null;

            foreach (Cell cell in this.cells)
                if (isElegible(cell))
                    this.RebuildCell(cell);
        }

        private void RebuildCell(Cell cell)
        {
            HashSet<Cell> cellsToAvoid = new HashSet<Cell>();
            HashSet<Cell.CellType> cellTypesToAvoid = new HashSet<Cell.CellType>();

            cellsToAvoid.Add(cell);
            var cellsLeadingToSelf = this.cells.SelectMany(c => c.portals).Where(p => cell.portals.Contains(p.destination)).Select(p => this.portalToCell[p]);
            foreach (Cell c in cellsLeadingToSelf)
                cellsToAvoid.Add(c);

            //if (cell.cellType == Cell.CellType.Room)
            //    cellTypesToAvoid.Add(Cell.CellType.Room);


            foreach (Portal p in cell.portals)
            {
                Portal[] elegiblePortals = this.cells.Where(c => !cellsToAvoid.Contains(c) && !cellTypesToAvoid.Contains(c.cellType)).SelectMany(c => c.portals).ToArray();

                if (elegiblePortals.Length == 0)
                {
                    p.destination = this.cells.Where(c => c != cell).First().portals[0];
                    continue;
                }

                p.destination = elegiblePortals[Random.Range(0, elegiblePortals.Length)];
                var destinationCell = this.portalToCell[p.destination];

                cellsToAvoid.Add(destinationCell);
                //if (destinationCell.cellType == Cell.CellType.Room)
                //    cellTypesToAvoid.Add(destinationCell.cellType);
            }
        }
    }
}