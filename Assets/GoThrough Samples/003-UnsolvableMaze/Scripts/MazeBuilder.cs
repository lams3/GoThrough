using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GoThrough.Samples.UnsolvableMaze
{
    internal class MazeBuilder : MonoBehaviour
    {
        internal Traveller player;
        
        private Dictionary<Portal, Cell> portalToCell; 
        private Cell[] cells;

        private void Awake()
        {
            this.SetupCells();
            this.SetupPlayer();
            this.RebuildMaze();
        }

        private void SetupCells()
        {
            this.cells = FindObjectsOfType<Cell>();
            this.portalToCell = new Dictionary<Portal, Cell>();
            foreach (Cell cell in this.cells)
                foreach (Portal p in cell.portals)
                    this.portalToCell[p] = cell;
        }

        private void SetupPlayer()
        {
            this.player.transform.position = this.cells[0].transform.position;
            this.player.OnTeleport += this.OnPlayerTeleport;
        }

        private void OnPlayerTeleport(Traveller player, Portal source, Portal destination)
        {
            Cell sourceCell = this.portalToCell[source];
            Cell destinationCell = this.portalToCell[destination];
            
            destination.Destination = source;

            this.RebuildMaze(new Cell[] { sourceCell, destinationCell });
        }

        private void RebuildMaze(IEnumerable<Cell> ignoredCells = null)
        {
            Predicate<Cell> isElegible = (Cell c) => ignoredCells == null || !ignoredCells.Contains(c);

            foreach (Cell cell in this.cells)
                if (isElegible(cell))
                    foreach (Portal portal in cell.portals)
                        portal.Destination = null;

            foreach (Cell cell in this.cells)
                if (isElegible(cell))
                    this.RebuildCell(cell);
        }

        private void RebuildCell(Cell cell)
        {
            HashSet<Cell> cellsToAvoid = new HashSet<Cell>();

            cellsToAvoid.Add(cell);
            var cellsLeadingToSelf = this.cells.SelectMany(c => c.portals).Where(p => cell.portals.Contains(p.Destination)).Select(p => this.portalToCell[p]);
            foreach (Cell c in cellsLeadingToSelf)
                cellsToAvoid.Add(c);


            foreach (Portal p in cell.portals)
            {
                List<Portal> elegiblePortals = this.cells.Except(cellsToAvoid).SelectMany(c => c.portals).ToList();

                if (elegiblePortals.Count == 0)
                    elegiblePortals.Add(this.cells.Where(c => c != cell).First().portals[0]);

                p.Destination = elegiblePortals[Random.Range(0, elegiblePortals.Count)];

                cellsToAvoid.Add(this.portalToCell[p.Destination]);
            }
        }
    }
}