using UnityEngine;
using System.Collections;
using Lockstep;
using Lockstep.Abilities;
using Lockstep.Utility;
public class BuildGridNode {
    public BuildGridNode(BuildGridManager parentGrid, Coordinate position) {
        this.Position = position;
        this.ParentGrid = parentGrid;
    }
    public BuildGridManager ParentGrid {get; private set;}
    public Coordinate Position {get; private set;}
    public bool Occupied {get; internal set;}
    public bool IsNeighbor {
        get {
            int buildSpacing = ParentGrid.BuildSpacing;
            for (int x = Position.x - buildSpacing; x <= Position.x + buildSpacing; x++) {
                for (int y = Position.y - buildSpacing; y <= Position.y + buildSpacing; y++) {
                    if (ParentGrid.IsOnGrid (x,y) && ParentGrid.Grid[x,y].Occupied) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
