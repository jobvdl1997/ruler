namespace Util.Algorithms.Polygon
{
    using System.Collections.Generic;
    using System.Linq;
    using Util.Geometry.Polygon;

    /// <summary>
    /// Implements the <see cref="Union"/> method by using a naive approach
    /// </summary>
    public class UnionNaive : IUnion
    {
        /// <inheritdoc />
        public IPolygon2D Union(ICollection<Polygon2D> polygons)
        {
            if (polygons.Count <= 0) return new MultiPolygon2D();

            // create multi polygon of polygons
            var visiblePolygon = new MultiPolygon2D(polygons.First());

            // add all polygons, cutting out the overlap
            foreach (Polygon2D polygon in polygons.Skip(1))
            {
                visiblePolygon = Clipper.CutOut(visiblePolygon, polygon);
                visiblePolygon.AddPolygon(polygon);
            }

            // return complete polygon
            return visiblePolygon;
        }
    }
}