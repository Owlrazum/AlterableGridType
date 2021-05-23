using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
public class Move
{
    public enum Type
    {
        TilePlacement,
        GroupPlacement,
        Deletion
    }

    public Type type;

    public Move(Type mvTp)
    {
        type = mvTp;
    }

    public bool IsInitialized { get; private set; } // Maybe will not needed

    public (int, int) Horizs { get; set; }
    public (int, int) Verts { get; set; }
    public (int, int) TopLefts { get; set; }
    public (int, int) BotLefts { get; set; }
    
    public List<Tile> AffectedTiles = new List<Tile>();
}
