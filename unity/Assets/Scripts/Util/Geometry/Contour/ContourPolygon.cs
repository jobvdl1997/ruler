using System.Collections;
using System.Text;
using Util.Geometry.Polygon;

namespace Util.Geometry.Contour
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Math;

    /// <summary>
    /// A contour polygon as specified in the paper by Martinez et al. (2013):
    /// Each contour is a simple polygon and the edges of the contours are interior
    /// disjoint. Next, we make the following definitions:
    /// - External contour: it is a contour not included in any of the other polygon contours.
    /// - Internal contour: it is a contour included in at least one of the other polygon contours.
    /// - Parent contour: Given an internal contour C, let P be the contour equal to the intersection
    ///                   of all the polygon contours that contain C -- i.e. the smallest contour that contains C.
    ///                   Then, we say that P is the parent contour of C and that C is a child contour of P.
    ///
    /// A polygon consisting of several contours can be represented as follows:
    ///  - The vertices of the contours included in an even number of contours are listed in ccw order.
    ///  - The vertices of the contours included in an odd number of contours are listed in cw order.
    ///  - For every parent contour its children contours are listed.
    ///  - This results in a tree structure with levels ccw - cw - ccw - cw etc.
    ///
    /// All ContourPolygons created should adhere to this specification. If not, operations might return wrong results,
    /// e.g. returning a negative area.
    /// </summary>
    public class ContourPolygon : IPolygon2D
    {
        /// <summary>
        /// List of contours that define the polygon. There can be multiple external contours and multiple
        /// internal contours. Internal contours (holes) are stored by using their ID within the Contour.
        /// </summary>
        public List<Contour> Contours { get; private set; }

        public int NumberOfContours
        {
            get { return Contours.Count; }
        }

        public int VertexCount
        {
            get { return Contours.Aggregate(0, (acc, contour) => acc + contour.VertexCount); }
        }
        
        public ICollection<Vector2> Vertices
        {
            get { return Contours.SelectMany(v => v.Vertices).Select(v => v.Vector2).ToList(); }
        }
        
        public ICollection<LineSegment> Segments
        {
            get { return Contours.SelectMany(v => v.Segments).ToList(); }
        }

        public ContourPolygon()
        {
            Contours = new List<Contour>();
        }

        public ContourPolygon(IEnumerable<Contour> contours)
        {
            Contours = new List<Contour>(contours);
        }

        public void Add(Contour c)
        {
            Contours.Add(c);
        }
        
        public Contour this[int index]
        {
            get
            {
                return Contours[index];
            }
        }

        public void Join(ContourPolygon pol)
        {
            var size = NumberOfContours;
            Contours.AddRange(pol.Contours);
            foreach (var contour in pol.Contours)
            {
                contour.ClearHoles();
                foreach (var holeId in contour.Holes)
                {
                    Contours.Last().AddHole(holeId + size);
                }
            }
        }

        public Rect BoundingBox(float margin = 0f)
        {
            return BoundingBoxComputer.FromPoints(Vertices, margin);
        }

        public float Area
        {
            get { return Contours.Aggregate(0f, (acc, contour) => acc + contour.Area); }
        }

        public string Visualize()
        {
            var result = new StringBuilder();
            foreach (var contour in Contours)
            {
                result.Append("\\draw ");
                foreach (var point in contour.Vertices)
                {
                    result.AppendFormat("({0:f2}, {1:f2}) -- ", point.x, point.y);
                }

                result.Append("cycle;");
            }

            return result.ToString();
        }

        public string PolygonFormat()
        {
            var result = new StringBuilder();
            result.AppendFormat("{0}\n", NumberOfContours);
            foreach (var contour in Contours) // Write the contours
            {
                result.AppendFormat("{0}\n", VertexCount);
                foreach (var point in contour.Vertices)
                {
                    result.AppendFormat("\t{0:N18} {1:N18}\n", point.x, point.y);
                }
            }

            for (int i = 0; i < Contours.Count; i++) // Write the holes of every contour
            {
                var contour = Contours[i];
                
                if (contour.Holes.Count == 0)
                {
                    continue;
                }

                result.AppendFormat("{0}: {1}\n", i, string.Join(" ", contour.Holes.Select(v => v.ToString()).ToArray()));
            }

            return result.ToString();
        }

        public string GeoJson()
        {
            var builder = new StringBuilder();
            builder.Append("{ \"type\": \"Feature\", \"properties\": {}, \"geometry\": { \"type\": \"Polygon\", \"coordinates\": [");
            var firstContour = true;
            foreach (var contour in Contours)
            {
                if (!firstContour)
                {
                    builder.Append(",");
                }
                else
                {
                    firstContour = false;
                }

                builder.Append("[");
                
                foreach (var vertex in contour.Vertices)
                {
                    builder.AppendFormat("[{0},{1}], ", vertex.x, vertex.y);
                }
                // The first and last coordinates are equivalent in GeoJSON
                builder.AppendFormat("[{0},{1}]", contour.Vertices[0].x, contour.Vertices[0].y);
                
                builder.Append("]");
            }
            builder.AppendLine("]}}");
            return builder.ToString();
        }

        public bool Equals(IPolygon2D other)
        {
            throw new System.NotImplementedException();
        }

        public Vector2? Next(Vector2 pos)
        {
            throw new System.NotImplementedException();
        }

        public Vector2? Prev(Vector2 pos)
        {
            throw new System.NotImplementedException();
        }

        public void AddVertex(Vector2 pos)
        {
            throw new System.NotImplementedException();
        }

        public void AddVertexFirst(Vector2 pos)
        {
            throw new System.NotImplementedException();
        }

        public void AddVertexAfter(Vector2 pos, Vector2 after)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveVertex(Vector2 pos)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveFirst()
        {
            throw new System.NotImplementedException();
        }

        public void RemoveLast()
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public bool ContainsInside(Vector2 pos)
        {
            throw new System.NotImplementedException();
        }

        public bool ContainsVertex(Vector2 pos)
        {
            throw new System.NotImplementedException();
        }

        public bool IsConvex()
        {
            throw new System.NotImplementedException();
        }

        public bool IsSimple()
        {
            throw new System.NotImplementedException();
        }

        public bool IsClockwise()
        {
            throw new System.NotImplementedException();
        }

        public void Reverse()
        {
            throw new System.NotImplementedException();
        }

        Rect IPolygon2D.BoundingBox(float margin)
        {
            throw new System.NotImplementedException();
        }
    }
}