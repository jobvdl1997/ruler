using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.ArtGallery.Util;
using UnityEngine;
using Util.Algorithms.Polygon;
using Util.Geometry;
using Util.Geometry.Polygon;

namespace ArtGallery
{
    public class
        NaiveLighthouseToLighthouseVisibility :
            ILighthouseToLightHouseVisibility
    {
        /// <inheritdoc />
        public bool VisibleToOtherVertex(
            Vector2 vertex1,
            Vector2 vertex2,
            Polygon2D polygon)
        {
            LineSegment seg1 = new LineSegment(vertex1, vertex2);

            foreach (LineSegment lineSegment in polygon.Segments)
            {
                // Check if any of the line segments intersect with the
                // line segments between the two vertices. If they so then 
                // the two vertices cannot see each other. With the exception
                // that an intersection with an endpoint does not count as 
                // blocking visibility
                Vector2? intersection = seg1.IntersectProper(lineSegment);

                if (intersection != null)
                {
                    // Check if the two lines are parallel. We allow two vertices
                    // to see each other if they they are along a straight line
                    if (!seg1.IsParallel(lineSegment))
                    {
                        return false;
                    }
                }
            }

            // Non of the line segments of the polygon intersect with the 
            // visibility line between the two points. Next we need to check if 
            // the visibility line is inside of the polygon
            // We take the midpoint of the line. If this is inside the polygon 
            // then the two points can see each other
            return polygon.ContainsInside(seg1.Midpoint);
        }

        /// <inheritdoc />
        public bool VisibleToOtherVertex(
            Vector2 vertex,
            List<Vector2> otherVerteces,
            Polygon2D polygon)
        {
            // Calculate all the visible vertices
            int numberOfVisiblevertices =
                VisibleToOtherVertices(vertex, otherVerteces, polygon)
                    .Count;

            // Check if there is at least 1 vertex visible
            return numberOfVisiblevertices > 0;
        }

        /// <inheritdoc />
        public bool VisibleToOtherVertex(
            List<Vector2> vertices,
            Polygon2D polygon)
        {
            // For each of the vertices check if they are visible to at least
            // one other vertex
            foreach (Vector2 vertex in vertices)
            {
                var othervertices = vertices.Where(i => i != vertex).ToList();

                // If the vertex cannot be seen by one other vertex return 
                if (!VisibleToOtherVertex(vertex, othervertices, polygon))
                {
                    return false;
                }
            }

            // if every vertex can be seen by one other vertex return true
            return true;
        }

        /// <inheritdoc />
        public ICollection<Vector2> VisibleToOtherVertices(
            Vector2 vertex,
            List<Vector2> otherVertices,
            Polygon2D polygon)
        {
            List<Vector2> result = new List<Vector2>();

            // check if the vertex can be seen by any of the other vertices
            foreach (Vector2 vertex2 in otherVertices)
            {
                // If the vertex can be seen by one other vertex add it to the 
                // list
                if (VisibleToOtherVertex(vertex, vertex2, polygon))
                {
                    result.Add(vertex2);
                }
            }

            // Return the list with all vertices that can see the vertex
            return result;
        }

        /// <inheritdoc />
        public IDictionary<Vector2, ICollection<Vector2>>
            VisibleToOtherVertices(
                List<Vector2> vertices,
                Polygon2D polygon)
        {
            // Create dictionary to store the result
            IDictionary<Vector2, ICollection<Vector2>> result =
                new Dictionary<Vector2, ICollection<Vector2>>();

            // Iterate over all the vertices and calculate for each vertex the
            // other vertices it can see.
            foreach (Vector2 vertex in vertices)
            {
                // Select all vertices except the current vertex 
                var othervertices = vertices.Where(i => i != vertex).ToList();

                // Calculate the visible vertices 
                var visibleVertices = VisibleToOtherVertices(
                    vertex,
                    othervertices,
                    polygon);

                // Add a dictionary item. The key is the current vertex and
                // the value is the vertices it can see.
                result.Add(vertex, visibleVertices);
            }

            // Return the dictionary containing the vertex to vertex visibility
            return result;
        }
    }
}