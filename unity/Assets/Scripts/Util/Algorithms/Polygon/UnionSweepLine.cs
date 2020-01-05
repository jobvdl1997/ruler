using System.Collections.Generic;
using System.Linq;
using Util.Geometry.Contour;
using Util.Geometry.Polygon;

namespace Util.Algorithms.Polygon
{
    /// <summary>
    /// Implements the <see cref="Union"/> method by using a Martinez sweep line approach.
    /// </summary>
    public class UnionSweepLine : IUnion
    {
        /// <inheritdoc />
        public IPolygon2D Union(ICollection<Polygon2D> polygons)
        {
            if (polygons.Count == 0)
            {
                return new MultiPolygon2D();
            }

            var result = polygons.First().ToContourPolygon();

            foreach (Polygon2D polygon in polygons.Skip(1))
            {
                var martinez = new Martinez(result, polygon.ToContourPolygon(), Martinez.OperationType.Union);
                
                result = martinez.Run();
            }
            
            return result;
        }
    }
}