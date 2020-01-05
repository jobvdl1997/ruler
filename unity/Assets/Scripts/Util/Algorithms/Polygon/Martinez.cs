using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public class Martinez : SweepLine<Martinez.StatusItem>
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
        private float MinXMax { get; set; }
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
            MinXMax = System.Math.Min(SubjectBoundingBox.xMax, ClippingBoundingBox.xMax);
            if (ComputeTrivialResult()) // Trivial cases can be quickly resolved without sweeping the plane
            {
                return Result;
            }

            var events = new List<SweepEvent>();
            for (int i = 0; i < Subject.NumberOfContours; i++)
            {
                for (int j = 0; j < Subject.Contours[i].VertexCount; j++)
                {
                    CreateEvents(Subject.Contours[i].Segment(j), PolygonType.Subject, events);
                }
            }

            for (int i = 0; i < Clipping.NumberOfContours; i++)
            {
                for (int j = 0; j < Clipping.Contours[i].VertexCount; j++)
                {
                    CreateEvents(Clipping.Contours[i].Segment(j), PolygonType.Clipping, events);
                }
            }

            InitializeEvents(events.ToArray()); // TODO: Why do we need ToArray here? Something with generics?
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
        /// Creates the events for a segment.
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="polygonType"></param>
        /// <param name="list">The list to add the events to</param>
        private void CreateEvents(LineSegment segment, PolygonType polygonType, ICollection<SweepEvent> list)
        {
            var event1 = new SweepEvent(segment.Point1, true, null, polygonType);
            var event2 = new SweepEvent(segment.Point2, true, event1, polygonType);
            event1.OtherEvent = event2;

            // The segment could be ordered the wrong way around, so we need to set the IsStart field properly
            if (segment.Min == segment.Point1)
            {
                event2.IsStart = false;
            }
            else
            {
                event1.IsStart = false;
            }

            list.Add(event1);
            list.Add(event2);
        }

        private void HandleEvent(IBST<ISweepEvent<StatusItem>> events, IBST<StatusItem> status,
            ISweepEvent<StatusItem> sweepEvent)
        {
            var ev = sweepEvent as SweepEvent;
            if (ev == null)
            {
                throw new ArgumentException("sweepEvent is not a SweepEvent");
            }

            // Optimization 2
            if ((Operation == OperationType.Intersection && ev.Pos.x > MinXMax) ||
                (Operation == OperationType.Difference && ev.Pos.x > SubjectBoundingBox.xMax))
            {
                // We need to connect edges now, so just clear all events
                InitializeEvents(new List<ISweepEvent<StatusItem>>());
                return;
            }

            ResultEvents.Add(ev);
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

        private int PossibleIntersection(SweepEvent ev1, SweepEvent ev2, IBST<ISweepEvent<StatusItem>> events)
        {
            Vector2 ip1, ip2; // Intersection points

            var nIntersections = FindIntersection(ev1.Segment, ev2.Segment, out ip1, out ip2);

            if (nIntersections == 0)
            {
                return 0; // no intersection
            }

            if (nIntersections == 1 && (ev1.Pos == ev2.Pos || ev1.OtherEvent.Pos == ev2.OtherEvent.Pos))
            {
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
                if (ev1.Pos != ip1 && ev1.OtherEvent.Pos != ip1
                ) // If the intersection point is not an endpoint of ev1.Segment
                {
                    DivideSegment(ev1, ip1, events);
                }

                if (ev2.Pos != ip1 && ev2.OtherEvent.Pos != ip1
                ) // If the intersection point is not an endpoint of ev2.Segment
                {
                    DivideSegment(ev2, ip1, events);
                }

                return 1;
            }

            // The line segments associated to ev1 and ev2 overlap
            var sortedEvents = new List<SweepEvent>();
            if (ev1.Pos == ev2.Pos)
            {
                sortedEvents.Add(null);
            }
            else if (SweepEvent.Compare(ev1, ev2))
            {
                sortedEvents.Add(ev2);
                sortedEvents.Add(ev1);
            }
            else
            {
                sortedEvents.Add(ev1);
                sortedEvents.Add(ev2);
            }

            if (ev1.OtherEvent.Pos == ev2.OtherEvent.Pos)
            {
                sortedEvents.Add(null);
            }
            else if (SweepEvent.Compare(ev1.OtherEvent, ev2.OtherEvent))
            {
                sortedEvents.Add(ev2.OtherEvent);
                sortedEvents.Add(ev1.OtherEvent);
            }
            else
            {
                sortedEvents.Add(ev1.OtherEvent);
                sortedEvents.Add(ev2.OtherEvent);
            }

            if ((sortedEvents.Count == 2) || (sortedEvents.Count == 3 && sortedEvents[2] != null))
            {
                // Both line segments are equal or share the left endpoint
                ev1.EdgeType = EdgeType.NonContributing;
                ev2.EdgeType = (ev1.InOut == ev2.InOut) ? EdgeType.SameTransition : EdgeType.DifferentTransition;
                if (sortedEvents.Count == 3)
                {
                    DivideSegment(sortedEvents[2].OtherEvent, sortedEvents[1].Pos, events);
                }

                return 2;
            }

            if (sortedEvents.Count == 3)
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

        private int FindIntersection(LineSegment seg0, LineSegment seg1, out Vector2 pi0, out Vector2 pi1)
        {
            // TODO: Convert this to be more elegant and use existing methods
            pi0 = new Vector2();
            pi1 = new Vector2();

            var p0 = seg0.Point1;
            var d0 = new Vector2(seg0.Point2.x - p0.x, seg0.Point2.y - p0.y);
            var p1 = seg1.Point1;
            var d1 = new Vector2(seg1.Point2.x - p1.x, seg1.Point2.y - p1.y);
            var sqrEpsilon = 0.0000001;
            var E = new Vector2(p1.x - p0.x, p1.y - p0.y);
            var kross = d0.x * d1.y - d0.y * d1.x;
            var sqrKross = kross * kross;
            var sqrLen0 = d0.x * d0.x + d0.y + d0.y;
            var sqrLen1 = d1.x * d1.x + d1.y * d1.y;

            if (sqrKross > sqrEpsilon * sqrLen0 * sqrLen1)
            {
                // Lines of the segments are not parallel
                var s = (E.x * d1.y - E.y * d1.x) / kross;
                if (s < 0 || s > 1)
                {
                    return 0;
                }

                var t = (E.x * d0.y - E.y * d0.x) / kross;
                if (t < 0 || t > 1)
                {
                    return 0;
                }

                // Intersection of lines is a point on each segment
                pi0 = new Vector2(p0.x + s * d0.x, p0.y + s * d0.y);
                if (Vector2.Distance(pi0, seg0.Point1) < 0.00000001)
                {
                    pi0 = seg0.Point1;
                }

                if (Vector2.Distance(pi0, seg0.Point2) < 0.00000001)
                {
                    pi0 = seg0.Point2;
                }

                if (Vector2.Distance(pi0, seg1.Point1) < 0.00000001)
                {
                    pi0 = seg1.Point1;
                }

                if (Vector2.Distance(pi0, seg1.Point2) < 0.00000001)
                {
                    pi0 = seg1.Point2;
                }

                return 1;
            }

            // Lines of the segments are parallel
            var sqrLenE = E.x * E.x + E.y * E.y;
            kross = E.x * d0.y - E.y * d0.x;
            sqrKross = kross * kross;
            if (sqrKross > sqrEpsilon * sqrLen0 * sqrLenE)
            {
                // Lines of the segments are different
                return 0;
            }

            // Lines of the segments are the same. Need to test for overlap of segments.
            var s0 = (d0.x * E.x + d0.y * E.y) / sqrLen0; // s0 = Dot(D0, E) * sqrLen0
            var s1 = s0 + (d0.x * d1.x + d0.y * d1.y) / sqrLen0; // s1 = s0 + Dot(D0, D1) * sqrLen0
            var smin = System.Math.Min(s0, s1);
            var smax = System.Math.Max(s0, s1);
            var w = new float[2];
            var imax = FindIntersection(0, 1, smin, smax, w);

            if (imax > 0)
            {
                pi0 = new Vector2(p0.x + w[0] * d0.x, p0.y + w[0] * d0.y);
                if (Vector2.Distance(pi0, seg0.Point1) < 0.00000001)
                {
                    pi0 = seg0.Point1;
                }

                if (Vector2.Distance(pi0, seg0.Point2) < 0.00000001)
                {
                    pi0 = seg0.Point2;
                }

                if (Vector2.Distance(pi0, seg1.Point1) < 0.00000001)
                {
                    pi0 = seg1.Point1;
                }

                if (Vector2.Distance(pi0, seg1.Point2) < 0.00000001)
                {
                    pi0 = seg1.Point2;
                }

                if (imax > 1)
                {
                    pi1 = new Vector2(p0.x + w[1] * d0.x, p0.y + w[1] * d0.y);
                }
            }

            return imax;
        }

        private int FindIntersection(float u0, float u1, float v0, float v1, float[] w)
        {
            // TODO: Convert this to be more elegant
            if ((u1 < v0) || (u0 > v1))
                return 0;
            if (u1 > v0)
            {
                if (u0 < v1)
                {
                    w[0] = (u0 < v0) ? v0 : u0;
                    w[1] = (u1 > v1) ? v1 : u1;
                    return 2;
                }

                // u0 == v1
                w[0] = u0;
                return 1;
            }
            else
            {
                // u1 == v0
                w[0] = u1;
                return 1;
            }
        }

        private void DivideSegment(SweepEvent ev, Vector2 pos, IBST<ISweepEvent<StatusItem>> events)
        {
            // "Right event" of the "left line segment" resulting from dividing ev.Segment
            var r = new SweepEvent(pos, false, ev, ev.PolygonType);
            // "Left event" of the "right line segment" resulting from dividing ev.Segment
            var l = new SweepEvent(pos, true, ev.OtherEvent, ev.PolygonType);

            if (SweepEvent.Compare(l, ev.OtherEvent)
            ) // Avoid a rounding error. The left event would be processed after the right event
            {
                ev.OtherEvent.IsStart = true;
                l.IsStart = false;
            }

            if (SweepEvent.Compare(ev, r)
            ) // Avoid a rounding error. The left event would be processed after the right event
            {
                // TODO: This
            }

            ev.OtherEvent.OtherEvent = l;
            ev.OtherEvent = r;
            events.Insert(l);
            events.Insert(r);
        }

        private void ConnectEdges()
        {
            var resultEvents = ResultEvents.Where(it => (it.IsStart && it.InResult) || (!it.IsStart && it.OtherEvent.InResult)).ToList();

            // Due to overlapping edges the resultEvents list can be not wholly sorted
            var sorted = false;
            while (!sorted)
            {
                sorted = true;
                for (int i = 0; i < resultEvents.Count; i++)
                {
                    if (i + 1 < resultEvents.Count && resultEvents[i].CompareTo(resultEvents[i + 1]) > 0)
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
                if (resultEvent.IsStart)
                {
                    continue;
                }
                
                var tmp = resultEvent.PositionInResult;
                resultEvent.PositionInResult = resultEvent.OtherEvent.PositionInResult;
                resultEvent.OtherEvent.PositionInResult = tmp;
            }

            var processed = Enumerable.Repeat(false, resultEvents.Count).ToList();
            var depth = new List<int>();
            var holeOf = new List<int>();
            for (int i = 0; i < resultEvents.Count; i++)
            {
                if (processed[i])
                {
                    continue;
                }

                var contour = new Contour();
                Result.Add(contour);
                var contourId = Result.NumberOfContours - 1;
                depth.Add(0);
                holeOf.Add(-1);
                if (resultEvents[i].PreviousInResult != null)
                {
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
                }

                var pos = i;
                var initial = resultEvents[i].Pos;
                contour.AddVertex(initial);
                while (resultEvents[pos].OtherEvent.Pos != initial)
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

                    processed[pos = resultEvents[pos].PositionInResult] = true;
                    contour.AddVertex(resultEvents[pos].Pos);
                    pos = NextPos(pos, resultEvents, processed);
                }

                processed[pos] = processed[resultEvents[pos].PositionInResult] = true;
                resultEvents[pos].OtherEvent.ResultInOut = true;
                resultEvents[pos].OtherEvent.ContourId = contourId;
                if ((depth[contourId] & 1) != 0)
                {
                    contour.ChangeOrientation();
                }
            }
        }

        private int NextPos(int pos, List<SweepEvent> resultEvents, List<bool> processed)
        {
            var newPos = pos + 1;
            while (newPos < resultEvents.Count && resultEvents[newPos].Pos == resultEvents[pos].Pos)
            {
                if (!processed[newPos])
                {
                    return newPos;
                }

                newPos++;
            }

            newPos = pos - 1;
            while (processed[newPos])
            {
                newPos--;
            }

            return newPos;
        }

        public class SweepEvent : ISweepEvent<StatusItem>
        {
            public SweepEvent(Vector2 pos, bool isStart, SweepEvent otherEvent, PolygonType polygonType,
                EdgeType edgeType = EdgeType.Normal)
            {
                Pos = pos;
                IsStart = isStart;
                OtherEvent = otherEvent;
                PolygonType = polygonType;
                EdgeType = edgeType;
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
                get { return MathUtil.EqualsEps(Pos.x, OtherEvent.Pos.x); }
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
            public int CompareTo(ISweepEvent<StatusItem> other)
            {
                var otherEvent = other as SweepEvent;
                if (otherEvent == null)
                {
                    throw new ArgumentException("other is not a SweepEvent");
                }

                if (ReferenceEquals(this, other))
                {
                    return 0;
                }

                return Compare(this, otherEvent) ? 1 : -1;
            }

            /// <summary>
            /// Compare two sweep events.
            /// </summary>
            /// <param name="e1"></param>
            /// <param name="e2"></param>
            /// <returns>True when e1 should be placed before e2, i.e. e1 should be handled after e2</returns>
            public static bool Compare(SweepEvent e1, SweepEvent e2)
            {
                if (e1.Pos.x > e2.Pos.x) // Different x-coordinate
                {
                    return true;
                }

                if (e2.Pos.x > e1.Pos.x) // Different x-coordinate
                {
                    return false;
                }

                if (!MathUtil.EqualsEps(e1.Pos.y, e2.Pos.y)
                ) // Different points, but same x-coordinate. The event with lower y-coordinate is processed first
                {
                    return e1.Pos.y > e2.Pos.y;
                }

                if (e1.IsStart != e2.IsStart
                ) // Same point, but one is a left endpoint and the other a right endpoint. The right endpoint is processed first.
                {
                    return e1.IsStart;
                }

                // Same point, but events are left endpoints or both are right endpoints.
                if (!MathUtil.EqualsEps(MathUtil.SignedArea(e1.Pos, e1.OtherEvent.Pos, e2.OtherEvent.Pos), 0)
                ) // Not collinear
                {
                    return e1.Above(e2.OtherEvent.Pos); // The event associated to the bottom segment is processed first
                }

                return e1.PolygonType > e2.OtherEvent.PolygonType;
            }

            public static bool SegmentCompare(SweepEvent le1, SweepEvent le2)
            {
                if (le1 == le2)
                {
                    return false;
                }

                if (!MathUtil.EqualsEps(
                        MathUtil.SignedArea(le1.Pos, le1.OtherEvent.Pos, le2.Pos), 0) ||
                    !MathUtil.EqualsEps(
                        MathUtil.SignedArea(le1.Pos, le1.OtherEvent.Pos, le2.OtherEvent.Pos),
                        0))
                {
                    // Segments are not collinear
                    // If they share their left endpoint use the right endpoint to sort
                    if (le1.Pos == le2.Pos)
                    {
                        return le1.Below(le2.OtherEvent.Pos);
                    }

                    // Different left endpoint: use the left endpoint to sort
                    if (MathUtil.EqualsEps(le1.Pos.x, le2.Pos.x))
                    {
                        return le1.Pos.y < le2.Pos.y;
                    }

                    if (Compare(le1, le2)
                    ) // Has the line segment associated to this been inserted into S after the line segment associated to other
                    {
                        return le2.Above(le1.Pos);
                    }

                    // The line segment associated to other has been inserted into S after the line segment associated to this
                    return le1.Below(le2.Pos);
                }

                // Segments are collinear
                if (le1.PolygonType != le2.PolygonType)
                {
                    return le1.PolygonType < le2.PolygonType;
                }

                // Just a consistent criterion is used
                if (le1.Pos == le2.Pos)
                {
                    // TODO: this?
                    // return SweepEvent.CompareTo(other.SweepEvent);
                    return RuntimeHelpers.GetHashCode(le1) < RuntimeHelpers.GetHashCode(le2);
                }

                return Compare(le1, le2);
            }

            public bool Equals(ISweepEvent<StatusItem> other)
            {
                throw new NotImplementedException();
            }

            public override string ToString()
            {
                return string.Format(
                    "({0}) ({1}) S:[{2} - {3})] ({4}) ({5}) ({6}) otherInOut: ({7})",
                    Pos, IsStart ? "left" : "right", Segment.Min, Segment.Max,
                    PolygonType.ToString().ToUpper(),
                    EdgeType.ToString().ToUpper(),
                    InOut ? "inOut" : "outIn",
                    OtherInOut ? "inOut" : "outIn"
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

                return SweepEvent.SegmentCompare(this.SweepEvent, other.SweepEvent) ? -1 : 1;
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
    }
}