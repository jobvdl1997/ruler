using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
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
            
            var builder = new StringBuilder();
            builder.AppendLine("If the union crashes, use the following to create a unit test:");

            int i = 0;
            foreach (var polygon2D in polygons)
            {
                builder.AppendFormat("var polygon{0} = new Polygon2D(new List<Vector2> {1}", i++, "{");
                foreach (var vertex in polygon2D.Vertices)
                {
                    builder.AppendFormat("new Vector2(BitConverter.ToSingle(Convert.FromBase64String(\"{0}\"), 0), BitConverter.ToSingle(Convert.FromBase64String(\"{1}\"), 0)), ", Convert.ToBase64String(BitConverter.GetBytes(vertex.x)), Convert.ToBase64String(BitConverter.GetBytes(vertex.y)));
                }

                builder.Append("});\n");
            }

            Debug.Log(builder.ToString());

            var result = polygons.First().ToContourPolygon();
            
#if UNITY_DEBUG
            Debug.Log(result.Visualize());
#endif

            foreach (var polygon in polygons.Skip(1))
            {
#if UNITY_DEBUG
                Debug.Log(result.PolygonFormat());
                Debug.Log(polygon.ToContourPolygon().PolygonFormat());
                Debug.Log(result.Visualize());
                Debug.Log(polygon.ToContourPolygon().Visualize());
                builder = new StringBuilder();
                builder.AppendFormat("var subject = new Polygon2D(new List<Vector2> {0}", "{");
                foreach (var vertex in result.Vertices)
                {
                    builder.AppendFormat("new Vector2(BitConverter.ToSingle(Convert.FromBase64String(\"{0}\"), 0), BitConverter.ToSingle(Convert.FromBase64String(\"{1}\"), 0)), ", Convert.ToBase64String(BitConverter.GetBytes(vertex.x)), Convert.ToBase64String(BitConverter.GetBytes(vertex.y)));
                }
                builder.Append("});\n");
                builder.AppendFormat("var clipping = new Polygon2D(new List<Vector2> {0}", "{");
                foreach (var vertex in polygon.Vertices)
                {
                    builder.AppendFormat("new Vector2(BitConverter.ToSingle(Convert.FromBase64String(\"{0}\"), 0), BitConverter.ToSingle(Convert.FromBase64String(\"{1}\"), 0)), ", Convert.ToBase64String(BitConverter.GetBytes(vertex.x)), Convert.ToBase64String(BitConverter.GetBytes(vertex.y)));
                }
                builder.Append("});\n");
                Debug.Log(builder);
                Debug.Log(result.GeoJson());
                Debug.Log(polygon.ToContourPolygon().GeoJson());
#endif

                var martinez = new Martinez(result, polygon.ToContourPolygon(), Martinez.OperationType.Union);

                result = martinez.Run();

#if UNITY_DEBUG
                Debug.Log(result.Visualize());
#endif
            }

            return result;
        }
    }
}