using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using Util.DataStructures.BST;
using Util.Geometry;
using Util.Geometry.Contour;
using Util.Math;

namespace Util.Algorithms.Polygon
{
    /// <summary>
    /// Implements the <see cref="Union"/> method by using a planeSweep approach
    /// </summary>
    public class Martinez : SweepLine<Martinez.SweepEvent, Martinez.StatusItem>
    {
        // Input properties
        private OperationType Operation { get; set; }
        private ContourPolygon Subject { get; set; }
        private ContourPolygon Clipping { get; set; }

        // Result properties
        /// <summary>
        /// The ResultEvents contains all events in-order that they were encountered.
        /// </summary>
        private List<SweepEvent> ResultEvents { get; set; }

        public ContourPolygon Result { get; private set; }

        // Helper properties (cache etc.)
        private float RightBound { get; set; }
        private Rect SubjectBoundingBox { get; set; }
        private Rect ClippingBoundingBox { get; set; }

        /// <summary>
        /// Creates a new object for executing a single boolean operation using the Martinez algorithm.
        /// </summary>
        /// <param name="subject">The subject polygon. It must adhere to the orientation as specified in the documentation of ContourPolygon.</param>
        /// <param name="clipping">The clipping polygon. It must adhere to the orientation as specified in the documentation of ContourPolygon.</param>
        /// <param name="operation"></param>
        public Martinez(ContourPolygon subject, ContourPolygon clipping, OperationType operation = OperationType.Union)
        {
            Subject = subject;
            Clipping = clipping;
            Operation = operation;
        }

        public ContourPolygon Run()
        {
            Result = new ContourPolygon();
            ResultEvents = new List<SweepEvent>();

            SubjectBoundingBox = Subject.BoundingBox();
            ClippingBoundingBox = Clipping.BoundingBox();
            RightBound = System.Math.Min(SubjectBoundingBox.xMax, ClippingBoundingBox.xMax);
            if (ComputeTrivialResult()) // Trivial cases can be quickly resolved without sweeping the plane
            {
                return Result;
            }

            var events = CreateEvents();

            InitializeEvents(events);
            InitializeStatus(new List<StatusItem>());

            VerticalSweep(HandleEvent);

            ConnectEdges();

            return Result;
        }

        /// <summary>
        /// Computes a trivial result
        /// </summary>
        /// <returns>Whether a trivial result could be computed</returns>
        private bool ComputeTrivialResult()
        {
            // When one of the polygons is empty, the result is trivial
            if (Subject.VertexCount == 0 || Clipping.VertexCount == 0)
            {
                switch (Operation)
                {
                    case OperationType.Difference:
                        Result = Subject;
                        break;
                    case OperationType.Union:
                    case OperationType.Xor:
                        Result = Subject.VertexCount > 0 ? Subject : Clipping;
                        break;
                    // Intersection is empty, so we don't need to do anything here
                }

                return true;
            }

            // Optimization 1: When the polygons do not overlap, the result is trivial
            if (!SubjectBoundingBox.Overlaps(ClippingBoundingBox))
            {
                // The bounding boxes do not overlap
                switch (Operation)
                {
                    case OperationType.Difference:
                        Result = Subject;
                        break;
                    case OperationType.Union:
                    case OperationType.Xor:
                        Result = Subject;
                        Result.Join(Clipping);
                        break;
                    // Intersection is empty again, so no need to do anything
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates the events for the polygons.
        /// </summary>
        /// <returns>A list containing all events</returns>
        private List<SweepEvent> CreateEvents()
        {
            var events = new List<SweepEvent>();
            int contourId = 0;
            for (int i = 0; i < Subject.NumberOfContours; i++)
            {
                if (Subject.Contours[i].External)
                {
                    contourId++;
                }

#if UNITY_DEBUG
                Debug.Log("contour = " + Subject.Contours[i].VertexCount);
#endif
                for (int j = 0; j < Subject.Contours[i].VertexCount; j++)
                {
                    CreateEvents(Subject.Contours[i].Segment(j), PolygonType.Subject, contourId,
                        Subject.Contours[i].External,
                        events);
                }
            }

            for (int i = 0; i < Clipping.NumberOfContours; i++)
            {
                if (Subject.Contours[i].External && Operation != OperationType.Difference)
                {
                    contourId++;
                }

#if UNITY_DEBUG
                Debug.Log("contour = " + Clipping.Contours[i].VertexCount);
#endif

                for (int j = 0; j < Clipping.Contours[i].VertexCount; j++)
                {
                    CreateEvents(Clipping.Contours[i].Segment(j), PolygonType.Clipping, contourId,
                        Subject.Contours[i].External && Operation != OperationType.Difference, events);
                }
            }

#if UNITY_DEBUG
            Debug.Log("Total number of events = " + events.Count);
#endif

            return events;
        }

        /// <summary>
        /// Creates the events for a segment.
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="polygonType"></param>
        /// <param name="contourId"></param>
        /// <param name="externalContour"></param>
        /// <param name="list">The list to add the events to</param>
        private void CreateEvents(LineSegment segment, PolygonType polygonType, int contourId, bool externalContour,
            ICollection<SweepEvent> list)
        {
#if UNITY_DEBUG
            Debug.Log("creating event for " + VT(segment.Point1) + " to " + VT(segment.Point2));
#endif
            var event1 = new SweepEvent(segment.Point1, false, null, polygonType);
            var event2 = new SweepEvent(segment.Point2, false, event1, polygonType);
            event1.OtherEvent = event2;

            if (segment.Point1.Equals(segment.Point2))
            {
                // Skip collapsed edges, or it breaks
                return;
            }

            event1.ContourId = event2.ContourId = contourId;

            if (!externalContour)
            {
                event1.External = event2.External = false;
            }

            // The segment could be ordered the wrong way around, so we need to set the IsStart field properly
            if (SweepEvent.CompareTo(event1, event2) > 0)
            {
#if UNITY_DEBUG
                Debug.Log("Oops " + event1 + " and " + event2);
#endif
                event2.IsStart = true;
            }
            else
            {
                event1.IsStart = true;
            }

            list.Add(event1);
            list.Add(event2);
        }

        private void HandleEvent(IBST<SweepEvent> events, IBST<StatusItem> status, SweepEvent ev)
        {
            /*foreach (var ss in events)
            {
                Debug.Log(string.Format("{0} - {1}", RuntimeHelpers.GetHashCode(ss), ss));
            }*/

#if UNITY_DEBUG
            Debug.Log(string.Format("Handling event {0}, {1}", ev, events.Count));
#endif

            ResultEvents.Add(ev);

            // Optimization 2
            if ((Operation == OperationType.Intersection && ev.Pos.x > RightBound) ||
                (Operation == OperationType.Difference && ev.Pos.x > SubjectBoundingBox.xMax))
            {
                // We need to connect edges now, so just clear all events
                InitializeEvents(new List<SweepEvent>());
                return;
            }

            if (ev.IsStart) // The line segment must be inserted into status
            {
                ev.StatusItem = new StatusItem(ev);
                if (!status.Insert(ev.StatusItem))
                {
                    throw new ArgumentException("Failed to insert into state");
                }

                StatusItem prev;
                var prevFound = status.FindNextSmallest(ev.StatusItem, out prev);

                ComputeFields(ev, prev, prevFound);

                StatusItem next;
                if (status.FindNextBiggest(ev.StatusItem, out next))
                {
#if UNITY_DEBUG
                    Debug.Log("next found " + next.SweepEvent);
#endif
                    // Process a possible intersection between "ev" and its next neighbor in status
                    if (PossibleIntersection(ev, next.SweepEvent, events) == 2)
                    {
                        ComputeFields(ev, prev, prevFound);
                        ComputeFields(next.SweepEvent, ev.StatusItem, true);
                    }
                }

                // Process a possible intersection between "ev" and its previous neighbor in status
                if (prevFound)
                {
#if UNITY_DEBUG
                    Debug.Log("prev found " + prev.SweepEvent);
#endif
                    if (PossibleIntersection(prev.SweepEvent, ev, events) == 2)
                    {
                        StatusItem prevprev;
                        var prevprevFound = status.FindNextSmallest(prev, out prevprev);

                        ComputeFields(prev.SweepEvent, prevprev, prevprevFound);
                        ComputeFields(ev, prev, prevFound);
                    }
                }
            }
            else
            {
                // The line segment must be removed from status
                ev = ev.OtherEvent; // We work with the left event

                StatusItem prev, next;
                var prevFound = status.FindNextSmallest(ev.StatusItem, out prev);
                var nextFound = status.FindNextBiggest(ev.StatusItem, out next);

                // Delete line segment associated to "ev" from status and check for intersection between the neighbors of "ev" in status
                status.Delete(ev.StatusItem);

                if (nextFound && prevFound)
                {
#if UNITY_DEBUG
                    Debug.Log("next and prev found");
#endif
                    PossibleIntersection(prev.SweepEvent, next.SweepEvent, events);
                }
            }
        }

        private void ComputeFields(SweepEvent ev, StatusItem prev, bool prevFound)
        {
            // Compute InOut and OtherInOut fields
            if (!prevFound)
            {
                ev.InOut = false;
                ev.OtherInOut = true;
            }
            else if (ev.PolygonType == prev.SweepEvent.PolygonType
            ) // Previous line segment in status belongs to the same polygon that "ev" belongs to
            {
                ev.InOut = !prev.SweepEvent.InOut;
                ev.OtherInOut = prev.SweepEvent.OtherInOut;
            }
            else // Previous line segment in status belongs to a different polygon that "ev" belongs to
            {
                ev.InOut = !prev.SweepEvent.OtherInOut;
                ev.OtherInOut = prev.SweepEvent.Vertical ? !prev.SweepEvent.InOut : prev.SweepEvent.InOut;
            }

            // Compute PreviousInResult field
            if (prevFound)
            {
                ev.PreviousInResult = (!InResult(prev.SweepEvent) || prev.SweepEvent.Vertical)
                    ? prev.SweepEvent.PreviousInResult
                    : prev.SweepEvent;
            }

            // Check if the line segment belongs to the boolean operation
            ev.InResult = InResult(ev);
        }

        private bool InResult(SweepEvent ev)
        {
            switch (ev.EdgeType)
            {
                case EdgeType.Normal:
                    switch (Operation)
                    {
                        case OperationType.Intersection:
                            return !ev.OtherInOut;
                        case OperationType.Union:
                            return ev.OtherInOut;
                        case OperationType.Difference:
                            return (ev.PolygonType == PolygonType.Subject && ev.OtherInOut) ||
                                   (ev.PolygonType == PolygonType.Clipping && !ev.OtherInOut);
                        case OperationType.Xor:
                            return true;
                    }

                    break;
                case EdgeType.SameTransition:
                    return Operation == OperationType.Intersection || Operation == OperationType.Union;
                case EdgeType.DifferentTransition:
                    return Operation == OperationType.Difference;
                case EdgeType.NonContributing:
                    return false;
            }

            // Just to make it compile
            return false;
        }

        private int PossibleIntersection(SweepEvent ev1, SweepEvent ev2, IBST<SweepEvent> events)
        {
#if UNITY_DEBUG
            Debug.Log(string.Format("Finding intersections between [{0}-{1}] and [{2}-{3}]", VT(ev1.Pos),
                VT(ev1.OtherEvent.Pos), VT(ev2.Pos), VT(ev2.OtherEvent.Pos)));
#endif

            var inter = Intersect(ev1.Segment, ev2.Segment);

            var nIntersections = inter != null ? inter.Count : 0;

            if (nIntersections == 0)
            {
                return 0; // no intersection
            }

#if UNITY_DEBUG
            Debug.Log(string.Format("Number of intersections = {0} [{1}-{2}] [{3}-{4}]", nIntersections, VT(ev1.Pos),
                VT(ev1.OtherEvent.Pos), VT(ev2.Pos), VT(ev2.OtherEvent.Pos)));
#endif

            // If the intersection is between two endpoints
            if (nIntersections == 1 && (ev1.Pos.Equals(ev2.Pos) ||
                                        ev1.OtherEvent.Pos.Equals(ev2.OtherEvent.Pos)))
            {
#if UNITY_DEBUG
                Debug.Log("skipping");
#endif
                return 0; // the line segments intersect at an endpoint of both line segments
            }

            if (nIntersections == 2 && ev1.PolygonType == ev2.PolygonType)
            {
                // The line segments overlap, but they belong to the same polygon
                throw new ArgumentException(string.Format("Sorry, edges of the same polygon overlap ({0} and {1})", ev1,
                    ev2));
            }

            // The line segments associated to ev1 and ev2 intersect
            if (nIntersections == 1)
            {
#if UNITY_DEBUG
                Debug.Log("not skipping " + Vector2.Distance(ev1.Pos, ev2.Pos));
#endif
                var intersectionPoint = inter[0].Vector2;
#if UNITY_DEBUG
                Debug.Log(VT(intersectionPoint));
#endif
                if (!ev1.Pos.Equals(intersectionPoint) && !ev1.OtherEvent.Pos.Equals(intersectionPoint)
                ) // If the intersection point is not an endpoint of ev1.Segment
                {
#if UNITY_DEBUG
                    Debug.Log("not an endpoint 1");
#endif
                    DivideSegment(ev1, intersectionPoint, events);
                }

                if (!ev2.Pos.Equals(intersectionPoint) && !ev2.OtherEvent.Pos.Equals(intersectionPoint)
                ) // If the intersection point is not an endpoint of ev2.Segment
                {
#if UNITY_DEBUG
                    Debug.Log("not an endpoint 2");
#endif
                    DivideSegment(ev2, intersectionPoint, events);
                }

                return 1;
            }

            // The line segments associated to ev1 and ev2 overlap
            var sortedEvents = new List<SweepEvent>();
            var leftCoincide = false;
            var rightCoincide = false;
            if (ev1.Pos.Equals(ev2.Pos))
            {
                leftCoincide = true;
            }
            else if (SweepEvent.CompareTo(ev1, ev2) == 1)
            {
                sortedEvents.Add(ev2);
                sortedEvents.Add(ev1);
            }
            else
            {
                sortedEvents.Add(ev1);
                sortedEvents.Add(ev2);
            }

            if (ev1.OtherEvent.Pos.Equals(ev2.OtherEvent.Pos))
            {
                rightCoincide = true;
            }
            else if (SweepEvent.CompareTo(ev1.OtherEvent, ev2.OtherEvent) == 1)
            {
                sortedEvents.Add(ev2.OtherEvent);
                sortedEvents.Add(ev1.OtherEvent);
            }
            else
            {
                sortedEvents.Add(ev1.OtherEvent);
                sortedEvents.Add(ev2.OtherEvent);
            }

            if ((leftCoincide && rightCoincide) || leftCoincide)
            {
                // Both line segments are equal or share the left endpoint
                ev2.EdgeType = EdgeType.NonContributing;
                ev1.EdgeType = (ev2.InOut == ev1.InOut) ? EdgeType.SameTransition : EdgeType.DifferentTransition;
                if (leftCoincide && !rightCoincide)
                {
                    DivideSegment(sortedEvents[1].OtherEvent, sortedEvents[0].Pos, events);
                }

                return 2;
            }

            if (rightCoincide)
            {
                // The line segments share the right endpoint
                DivideSegment(sortedEvents[0], sortedEvents[1].Pos, events);
                return 3;
            }

            if (sortedEvents[0] != sortedEvents[3].OtherEvent)
            {
                // No line segment includes totally the other one
                DivideSegment(sortedEvents[0], sortedEvents[1].Pos, events);
                DivideSegment(sortedEvents[1], sortedEvents[2].Pos, events);
                return 3;
            }

            // One line segment includes the other one
            DivideSegment(sortedEvents[0], sortedEvents[1].Pos, events);
            DivideSegment(sortedEvents[3].OtherEvent, sortedEvents[2].Pos, events);
            return 3;
        }

        private void DivideSegment(SweepEvent ev, Vector2 pos, IBST<SweepEvent> events)
        {
            // "Right event" of the "left line segment" resulting from dividing ev.Segment
            var r = new SweepEvent(pos, false, ev, ev.PolygonType);
            // "Left event" of the "right line segment" resulting from dividing ev.Segment
            var l = new SweepEvent(pos, true, ev.OtherEvent, ev.PolygonType);

#if UNITY_DEBUG
            Debug.Log(string.Format("Dividing segment: le = {0}, le.otherEvent = {1}, r = {2}, l = {3}", ev,
                ev.OtherEvent, r, l));
#endif

            r.ContourId = l.ContourId = ev.ContourId;

            if (SweepEvent.CompareTo(l, ev.OtherEvent) > 0
            ) // Avoid a rounding error. The left event would be processed after the right event
            {
#if UNITY_DEBUG
                Debug.Log("Oops2");
#endif
                ev.OtherEvent.IsStart = true;
                l.IsStart = false;
            }

            ev.OtherEvent.OtherEvent = l;
            ev.OtherEvent = r;
            events.Insert(l);
            events.Insert(r);
        }

        private void ConnectEdges()
        {
            var resultEvents = ResultEvents
                .Where(it => (it.IsStart && it.InResult) || (!it.IsStart && it.OtherEvent.InResult)).ToList();

            // Due to overlapping edges the resultEvents list can be not wholly sorted
            var sorted = false;
            while (!sorted)
            {
                sorted = true;
                for (int i = 0; i < resultEvents.Count; i++)
                {
                    if (i + 1 < resultEvents.Count && SweepEvent.CompareTo(resultEvents[i], resultEvents[i + 1]) == 1)
                    {
                        var tmp = resultEvents[i];
                        resultEvents[i] = resultEvents[i + 1];
                        resultEvents[i + 1] = tmp;
                        sorted = false;
                    }
                }
            }

            // We cannot do a foreach because we need to set PositionInResult
            for (int i = 0; i < resultEvents.Count; i++)
            {
                var resultEvent = resultEvents[i];
                resultEvent.PositionInResult = i;
            }

            foreach (var resultEvent in resultEvents)
            {
                if (!resultEvent.IsStart)
                {
                    var tmp = resultEvent.PositionInResult;
                    resultEvent.PositionInResult = resultEvent.OtherEvent.PositionInResult;
                    resultEvent.OtherEvent.PositionInResult = tmp;
                }
            }

            var result = new StringBuilder();
            SweepEvent lastEvent = null;
            var current = new List<int>();
            foreach (var resultEvent in resultEvents)
            {
                current.Add(resultEvent.PositionInResult);
                if (lastEvent != null && lastEvent.Pos.Equals(resultEvent.Pos))
                {
                    continue;
                }

                result.AppendFormat("\\node[draw] at ({0:N9},{1:N9}) {{{2}}};\n", resultEvent.Pos.x, resultEvent.Pos.y,
                    string.Join(",", current.Select(x => x.ToString()).ToArray()));

#if UNITY_DEBUG
                Debug.Log(resultEvent.ToString());
#endif

                lastEvent = resultEvent;
                current.Clear();
            }

#if UNITY_DEBUG
            Debug.Log(result.ToString());
#endif

            var processed = new BitArray(resultEvents.Count);
            // var processed = Enumerable.Repeat(false, resultEvents.Count).ToList();
            // var depth = new List<int>();
            // var holeOf = new List<int>();
            for (int i = 0; i < resultEvents.Count; i++)
            {
                if (processed[i])
                {
                    continue;
                }

                var contour = new Contour();
                Result.Add(contour);
                var contourId = Result.NumberOfContours - 1;
                /*depth.Add(0);
                holeOf.Add(-1);
                if (resultEvents[i].PreviousInResult != null)
                {
                    Debug.Log("previous in result!!");
                    var lowerContourId = resultEvents[i].PreviousInResult.ContourId;
                    if (!resultEvents[i].PreviousInResult.ResultInOut)
                    {
                        Result[lowerContourId].AddHole(contourId);
                        holeOf[contourId] = lowerContourId;
                        depth[contourId] = depth[lowerContourId] + 1;
                        contour.External = false;
                    }
                    else if (!Result[lowerContourId].External)
                    {
                        Result[holeOf[lowerContourId]].AddHole(contourId);
                        holeOf[contourId] = holeOf[lowerContourId];
                        depth[contourId] = depth[lowerContourId];
                        contour.External = false;
                    }
                }*/

                var pos = i;
                var initial = resultEvents[i].Pos;
                contour.AddVertex(initial);
                while (pos >= i)
                {
                    processed[pos] = true;
                    if (resultEvents[pos].IsStart)
                    {
                        resultEvents[pos].ResultInOut = false;
                        resultEvents[pos].ContourId = contourId;
                    }
                    else
                    {
                        resultEvents[pos].OtherEvent.ResultInOut = true;
                        resultEvents[pos].OtherEvent.ContourId = contourId;
                    }

#if UNITY_DEBUG
                    Debug.Log("poss = " + pos);
#endif
                    pos = resultEvents[pos].PositionInResult;
#if UNITY_DEBUG
                    Debug.Log("positionInResult = " + pos);
#endif
                    processed[pos] = true;
                    contour.AddVertex(resultEvents[pos].Pos);
#if UNITY_DEBUG
                    Debug.Log("adding vertex " + VT(resultEvents[pos].Pos));
#endif
                    pos = NextPos(pos, resultEvents, processed, i);
                }

                pos = pos == -1 ? i : pos;

                processed[pos] = processed[resultEvents[pos].PositionInResult] = true;
                resultEvents[pos].OtherEvent.ResultInOut = true;
                resultEvents[pos].OtherEvent.ContourId = contourId;
                /*if ((depth[contourId] & 1) != 0)
                {
                    contour.ChangeOrientation();
                }*/
            }
        }

        private int NextPos(int pos, List<SweepEvent> resultEvents, BitArray processed, int origIndex)
        {
#if UNITY_DEBUG
            Debug.Log(string.Format("origIndex = {0}, pos = {1}, count = {2}, processed = ({3})", origIndex, pos,
                resultEvents.Count,
                string.Join(", ", processed.Cast<bool>().Select(v => v ? "true" : "false").ToArray())));
#endif
            var newPos = pos + 1;
            while (newPos < resultEvents.Count && resultEvents[newPos].Pos.Equals(resultEvents[pos].Pos))
            {
                if (!processed[newPos])
                {
                    return newPos;
                }

                newPos++;
            }

            newPos = pos - 1;
#if UNITY_DEBUG
            Debug.Log(string.Format("newPos = {0}", newPos));
#endif
            while (newPos >= origIndex && processed[newPos])
            {
                newPos--;
#if UNITY_DEBUG
                Debug.Log(string.Format("newPos = {0}", newPos));
#endif
            }

            return newPos;
        }

        private static double CrossProduct(Vector2d a, Vector2d b)
        {
            return (a.x * b.y) - (a.y * b.x);
        }

        private static double DotProduct(Vector2d lhs, Vector2d rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y;
        }

        private static Vector2d ToPoint(Vector2d p, double s, Vector2d d)
        {
            return new Vector2d(p.x + s * d.x, p.y + s * d.y);
        }

        private class Vector2d
        {
            /// <summary>
            ///   <para>X component of the vector.</para>
            /// </summary>
            public double x;

            /// <summary>
            ///   <para>Y component of the vector.</para>
            /// </summary>
            public double y;

            /// <summary>
            ///   <para>Constructs a new vector with given x, y components.</para>
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            public Vector2d(double x, double y)
            {
                this.x = x;
                this.y = y;
            }

            public Vector2 Vector2
            {
                get { return new Vector2((float) x, (float) y); }
            }

            public static Vector2d FromVector2(Vector2 pos)
            {
                return new Vector2d(pos.x, pos.y);
            }

            public static Vector2d operator -(Vector2d a, Vector2d b)
            {
                return new Vector2d(a.x - b.x, a.y - b.y);
            }
        }

        [CanBeNull]
        private static List<Vector2d> Intersect(LineSegment a, LineSegment b)
        {
            var a1 = Vector2d.FromVector2(a.Point1);
            var a2 = Vector2d.FromVector2(a.Point2);
            var b1 = Vector2d.FromVector2(b.Point1);
            var b2 = Vector2d.FromVector2(b.Point2);

            // The algorithm expects our lines in the form P + sd, where P is a point,
            // s is on the interval [0, 1], and d is a vector.
            // We are passed two points. P can be the first point of each pair. The
            // vector, then, could be thought of as the distance (in x and y components)
            // from the first point to the second point.
            // So first, let's make our vectors:
            var va = a2 - a1;
            var vb = b2 - b1;
            // We also define a function to convert back to regular point form:
            // The rest is pretty much a straight port of the algorithm.

            var e = b1 - a1;
            var kross = CrossProduct(va, vb);
            var sqrKross = kross * kross;
            var sqrLenA = DotProduct(va, va);
            if (sqrKross > 0)
            {
                var s = CrossProduct(e, vb) / kross;
                if (s < 0 || s > 1)
                {
                    return null;
                }

                var t = CrossProduct(e, va) / kross;
                if (t < 0 || t > 1)
                {
                    return null;
                }

                if (s == 0 || s == 1)
                {
                    return new List<Vector2d> {ToPoint(a1, s, va)};
                }

                if (t == 0 || t == 1)
                {
                    return new List<Vector2d> {ToPoint(b1, t, vb)};
                }

                return new List<Vector2d> {ToPoint(a1, s, va)};
            }

            kross = CrossProduct(e, va);
            sqrKross = kross * kross;

            if (sqrKross > 0)
            {
                return null;
            }

            var sa = DotProduct(va, e) / sqrLenA;
            var sb = sa + DotProduct(va, vb) / sqrLenA;
            var smin = System.Math.Min(sa, sb);
            var smax = System.Math.Max(sa, sb);

            if (smin <= 1 || smax >= 0)
            {
                if (smin == 1)
                {
                    return  new List<Vector2d> {ToPoint(a1, smin > 0 ? smin : 0, va)};
                }

                if (smax == 0)
                {
                    return  new List<Vector2d> {ToPoint(a1, smax < 1 ? smax : 1, va)};
                }

                return new List<Vector2d>
                {
                    ToPoint(a1, smin > 0 ? smin : 0, va),
                    ToPoint(a1, smax < 1 ? smax : 1, va)
                };
            }

            return null;
        }

        public class SweepEvent : ISweepEvent<StatusItem>, IComparable<SweepEvent>, IEquatable<SweepEvent>
        {
            public SweepEvent(Vector2 pos, bool isStart, SweepEvent otherEvent, PolygonType polygonType,
                EdgeType edgeType = EdgeType.Normal)
            {
                Pos = pos;
                IsStart = isStart;
                OtherEvent = otherEvent;
                PolygonType = polygonType;
                EdgeType = edgeType;

                InOut = true;
                OtherInOut = true;
            }

            // Point associated with the event
            public Vector2 Pos { get; private set; }

            public StatusItem StatusItem { get; set; }

            /// <summary>
            /// Is Pos the left endpoint of the edge 
            /// </summary>
            public bool IsStart { get; set; }

            /// <summary>
            /// Is Pos the right endpoint of the edge. Note: not actually used, but required by ISweepEvent
            /// </summary>
            public bool IsEnd
            {
                get { return !IsStart; }
            }

            /// <summary>
            /// Other endpoint of the edge. When the edge is subdivided, this is updated.
            /// </summary>
            public SweepEvent OtherEvent { get; set; }

            /// <summary>
            /// Is this event associated to the Subject or Clipping polygon?
            /// </summary>
            public PolygonType PolygonType { get; private set; }

            public EdgeType EdgeType { get; set; }

            /// <summary>
            /// Indicates if this segment determines an inside-outside transition into the polygon for
            /// a vertical ray that starts below the polygon and intersects the segment.
            /// </summary>
            public bool InOut { get; set; }

            /// <summary>
            /// See InOut, but referred to the closest segment to this segment downwards in status that
            /// belongs to the other polygon. 
            /// </summary>
            public bool OtherInOut { get; set; }

            /// <summary>
            /// The closest edge to the segment downwards in status that belongs to the result polygon.
            /// This field is used in the second stage of the algorithm to compute child contours.
            /// </summary>
            public SweepEvent PreviousInResult { get; set; }

            /// <summary>
            /// Whether this segment is in the result.
            /// </summary>
            public bool InResult { get; set; }

            /// <summary>
            /// Stores the position of the segment in the Result.
            /// </summary>
            public int PositionInResult { get; set; }

            /// <summary>
            /// Indicates if the segment determines an in-out transition into C for a vertical ray that starts
            /// below C and intersects the segment associated to this segment.
            /// </summary>
            public bool ResultInOut { get; set; }

            /// <summary>
            /// The ID that identifies the contour C.
            /// </summary>
            public int ContourId { get; set; }

            public bool External { get; set; }

            /// <summary>
            /// Determines whether the line segment is below point p
            /// </summary>
            /// <param name="p"></param>
            /// <returns>Whether the line segment is below point p</returns>
            private bool Below(Vector2 p)
            {
                return IsStart
                    ? MathUtil.SignedArea(Pos, OtherEvent.Pos, p) > 0
                    : MathUtil.SignedArea(OtherEvent.Pos, Pos, p) > 0;
            }

            /// <summary>
            /// Determines whether the line segment is above point p
            /// </summary>
            /// <param name="p"></param>
            /// <returns>Whether the line segment is above point p</returns>
            private bool Above(Vector2 p)
            {
                return !Below(p);
            }

            /// <summary>
            /// Indicates whether the line segment is a vertical line segment
            /// </summary>
            public bool Vertical
            {
                get { return Pos.x.Equals(OtherEvent.Pos.x); }
            }

            /// <summary>
            /// The line segment associated to this event
            /// </summary>
            public LineSegment Segment
            {
                get { return new LineSegment(Pos, OtherEvent.Pos); }
            }

            /// <summary>
            /// CompareTo is used for sorting the sweep events in the event BST.
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentException"></exception>
            public int CompareTo(SweepEvent other)
            {
                if (this == other)
                {
                    return 0;
                }

                return CompareTo(this, other);
            }

            /// <summary>
            /// Compare two sweep events.
            /// </summary>
            /// <param name="e1"></param>
            /// <param name="e2"></param>
            /// <returns>True when e1 should be placed before e2, i.e. e1 should be handled after e2</returns>
            public static int CompareTo(SweepEvent e1, SweepEvent e2)
            {
                if (e1.Pos.x > e2.Pos.x) // Different x-coordinate
                {
                    return 1;
                }

                if (e1.Pos.x < e2.Pos.x) // Different x-coordinate
                {
                    return -1;
                }

                if (!e1.Pos.y.Equals(e2.Pos.y)
                ) // Different points, but same x-coordinate. The event with lower y-coordinate is processed first
                {
                    return e1.Pos.y > e2.Pos.y ? 1 : -1;
                }

                if (e1.IsStart != e2.IsStart
                ) // Same point, but one is a left endpoint and the other a right endpoint. The right endpoint is processed first.
                {
                    return e1.IsStart ? 1 : -1;
                }

                // Same point, but events are left endpoints or both are right endpoints.
                if (MathUtil.SignedArea(e1.Pos, e1.OtherEvent.Pos, e2.OtherEvent.Pos) != 0)
                {
                    // Not collinear
                    return
                        e1.Above(e2.OtherEvent.Pos)
                            ? 1
                            : -1; // The event associated to the bottom segment is processed first
                }

                // Collinear
                return e1.PolygonType > e2.PolygonType ? 1 : -1;
            }

            public static int SegmentCompare(SweepEvent le1, SweepEvent le2)
            {
                if (le1 == le2)
                {
                    return 0;
                }

                if (MathUtil.SignedArea(le1.Pos, le1.OtherEvent.Pos, le2.Pos) != 0 ||
                    MathUtil.SignedArea(le1.Pos, le1.OtherEvent.Pos, le2.OtherEvent.Pos) != 0)
                {
                    // Segments are not collinear
                    // If they share their left endpoint use the right endpoint to sort
                    if (le1.Pos.Equals(le2.Pos))
                    {
                        return le1.Below(le2.OtherEvent.Pos) ? -1 : 1;
                    }

                    // Different left endpoint: use the left endpoint to sort
                    if (le1.Pos.x.Equals(le2.Pos.x))
                    {
                        return le1.Pos.y < le2.Pos.y ? -1 : 1;
                    }

                    if (CompareTo(le1, le2) == 1
                    ) // Has the line segment associated to this been inserted into S after the line segment associated to other
                    {
                        return le2.Above(le1.Pos) ? -1 : 1;
                    }

                    // The line segment associated to other has been inserted into S after the line segment associated to this
                    return le1.Below(le2.Pos) ? -1 : 1;
                }

                // Segments are collinear
                if (le1.PolygonType != le2.PolygonType)
                {
                    return le1.PolygonType == PolygonType.Subject ? -1 : 1;
                }

                // Same polygon

                // Just a consistent criterion is used
                if (le1.Pos.Equals(le2.Pos))
                {
                    if (le1.OtherEvent.Pos.Equals(le2.OtherEvent.Pos))
                    {
                        return 0;
                    }

                    return le1.ContourId > le2.ContourId ? 1 : -1;
                }

                return CompareTo(le1, le2) == 1 ? 1 : -1;
            }

            public bool Equals(SweepEvent other)
            {
                return this == other;
            }

            public override string ToString()
            {
                return string.Format(
                    "{0} ({1}) S:[{2} - {3}] ({4}) ({5})",
                    VT(Pos), IsStart ? "left" : "right", VT(Pos), VT(OtherEvent.Pos),
                    PolygonType.ToString().ToUpper(),
                    EdgeType.ToString().ToUpper()
                );
            }
        }

        public class StatusItem : IComparable<StatusItem>, IEquatable<StatusItem>
        {
            public SweepEvent SweepEvent { get; private set; }

            public StatusItem(SweepEvent sweepEvent)
            {
                SweepEvent = sweepEvent;
            }

            public int CompareTo(StatusItem other)
            {
                if (ReferenceEquals(this, other))
                {
                    return 0;
                }

                return SweepEvent.SegmentCompare(this.SweepEvent, other.SweepEvent);
            }

            public bool Equals(StatusItem other)
            {
                throw new NotImplementedException();
            }
        }

        public enum PolygonType
        {
            Subject,
            Clipping
        }

        public enum EdgeType
        {
            Normal,
            NonContributing,
            SameTransition,
            DifferentTransition
        }

        public enum OperationType
        {
            Intersection,
            Union,
            Difference,
            Xor
        }

        private static string VT(Vector2 v)
        {
            return string.Format("({0:N9},{1:N9})", v.x, v.y);
        }
    }
}