using System.Collections.Generic;
using UnityEngine;
using Geometry;

namespace Utilities
{
    public class Vector2Comparer : IComparer<Vector2>
    {
        public int Compare(Vector2 first, Vector2 second)
        {
            if (first.x.CompareTo(second.x) != 0)
            {
                return first.x.CompareTo(second.x);
            } else
            {
                return first.y.CompareTo(second.y);
            }
        }
    }

    public struct RowCol
    {
        public RowCol(int row, int col)
        {
            Row = row;
            Col = col;
        }
        public int Row { get; }
        public int Col { get; }
    }
}

