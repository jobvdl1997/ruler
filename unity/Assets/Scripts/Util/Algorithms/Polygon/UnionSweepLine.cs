namespace Util.Algorithms.Polygon
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.DataStructures.BST;
    using Util.Geometry.Polygon;

    /// <summary>
    /// Implements the <see cref="Union"/> method by using a planeSweep approach
    /// </summary>
    public class UnionSweepLine : SweepLine<UnionSweepLine.StatusItem>, IUnion
    {
        /// <inheritdoc />
        public IPolygon2D Union(ICollection<Polygon2D> polygons)
        {
            // TODO: Set up event queue and status tree
            VerticalSweep(this.HandleEvent);

            // TODO: Return result
            throw new NotImplementedException("UnionSweepLine is not yet implemented");
        }

        private void HandleEvent(IBST<ISweepEvent<StatusItem>> events, IBST<StatusItem> status,
            ISweepEvent<StatusItem> ev)
        {
        }

        public class SweepEvent : ISweepEvent<StatusItem>
        {
            public int CompareTo(ISweepEvent<StatusItem> other)
            {
                throw new NotImplementedException();
            }

            public bool Equals(ISweepEvent<StatusItem> other)
            {
                throw new NotImplementedException();
            }

            public Vector2 Pos { get; private set; }
            public StatusItem StatusItem { get; private set; }
            public bool IsStart { get; private set; }
            public bool IsEnd { get; private set; }
        }

        public class StatusItem : IComparable<StatusItem>, IEquatable<StatusItem>
        {
            public int CompareTo(StatusItem other)
            {
                throw new NotImplementedException();
            }

            public bool Equals(StatusItem other)
            {
                throw new NotImplementedException();
            }
        }
    }
}