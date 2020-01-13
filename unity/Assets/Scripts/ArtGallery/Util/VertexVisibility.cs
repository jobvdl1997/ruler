using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArtGallery;
using UnityEngine;
using Util.Algorithms.Polygon;
using Util.Geometry;
using Util.Geometry.Polygon;

namespace Assets.Scripts.ArtGallery.Util
{
    public static class VertexVisibility
    {

        public static ICollection<Vector2> VisibleVertices(
            Polygon2D polygon,
            Vector2 vertex)
        {
            List<Vector2> result = new List<Vector2>();

            // calculate new visibility polygon
            var vision = Visibility.Vision(polygon, vertex);

            if (vision == null)
            {
                throw new Exception("Vision polygon cannot be null");
            }

            // Check if the polygon is in clockwise order, if not, make 
            // the polygon in clockwise order
            if (!polygon.IsClockwise())
            {
                polygon.Reverse();
            }

            // Check if the vision polygon is in clockwise order, if not, make 
            // the vision polygon in clockwise order
            if (!vision.IsClockwise())
            {
                vision.Reverse();
            }

            List<Vector2> polyvertices = polygon.Vertices.ToList();

            List<Vector2> visibilityvertices = vision.Vertices.ToList();

            polyvertices = polyvertices.StartAt(vertex).ToList();
            visibilityvertices = visibilityvertices.StartAt(vertex).ToList();

            // move all vertices such that the vertex vertex is at the origin
            // and transform them to PolarPoint2D

            var polyPolarPoints = polyvertices
                                  .Select(v => v - vertex)
                                  .Select(x => new PolarPoint2D(x))
                                  .ToList();

            var visPolarPoints = visibilityvertices
                                 .Select(v => v - vertex)
                                 .Select(x => new PolarPoint2D(x))
                                 .ToList();

            var initAngle = polyPolarPoints[1].Theta;

            // rotate all points of the shifted polygon clockwise such that v0 lies
            // on the x axis
            foreach (var curr in polyPolarPoints)
            {
                if (!curr.IsOrigin())
                {
                    curr.RotateClockWise(initAngle);
                }
            }

            foreach (var curr in visPolarPoints)
            {
                if (!curr.IsOrigin())
                {
                    curr.RotateClockWise(initAngle);
                }
            }

            bool done = false;
            int polyIndex = 0;
            int visIndex = 0;
            int polyCount = polyvertices.Count;
            int visCount = visibilityvertices.Count;

            while (!done)
            {
                var polyCurrent = polyvertices[polyIndex];
                var visCurrent = visibilityvertices[visIndex];
                var polyNext = polyvertices[(polyIndex + 1) % polyCount];

                var polyLast =
                    polyvertices[(polyIndex - 1 + polyCount) % polyCount];

                var visNext = visibilityvertices[(visIndex + 1) % visCount];
                var polyLineSegment = new LineSegment(polyCurrent, polyNext);

                var polyLineSegmentLast =
                    new LineSegment(polyLast, polyCurrent);

                var visLineSegment = new LineSegment(visCurrent, visNext);

                if (polyCurrent == visCurrent)
                {
                    // the current poly vertex is a visibility vertex. 
                    // Add it to the list and increase the counter of polyIndex
                    result.Add(polyCurrent);
                    polyIndex++;
                }
                else if (visLineSegment.IsEndpoint(polyCurrent))
                {
                    //If the current poly vertex is equal to the next vis vertex
                    // We know that we can increase the vis index as there cannot
                    // be any more poly vertices on the line between the current
                    // and the next vis vertex
                    // We do not add the current poly vertex to the list as 
                    // this will be done in the next iteration
                    visIndex++;
                }
                else if (visLineSegment.IsOnSegment(polyCurrent))
                {
                    // If the current poly vertex is on the line segment between
                    // the current and the next vis vertices add it to the list
                    // and increase the counter of the polyIndex
                    result.Add(polyCurrent);
                    polyIndex++;
                }
                else if (polyLineSegment.IsOnSegment(visNext))
                {
                    // if the next vis vertex is on the poly line segment 
                    // increase the vis index. 
                    visIndex++;
                }
                else if (polyLineSegmentLast.IsOnSegment(visNext))
                {
                    // if the next vis vertex is on the previous poly
                    // lineSegment then no more points can lie on the 
                    // lineSegment of the current and next vis vertices.
                    visIndex++;
                }
                else
                {
                    // Continue to increase the poly index until the current
                    // poly vertex intersects with the vis lineSegment again
                    polyIndex++;
                }

                //                if (!(polyIndex < polyvertices.Count) && !(visIndex < visibilityvertices.Count))
                //                {
                //                    done = true;
                //                }
                if (!(polyIndex < polyvertices.Count))
                {
                    done = true;
                }

                if (!(visIndex < visibilityvertices.Count))
                {
                    done = true;
                }
            }

            return result;
        }
    }
}
