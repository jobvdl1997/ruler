namespace Util.Algorithms.Polygon.Tests
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using UnityEngine;
    using Util.Geometry.Polygon;
    using Math;

    [TestFixture(typeof(UnionNaive))]
    [TestFixture(typeof(UnionSweepLine))]
    public class IUnionTests<TUnion> where TUnion : IUnion, new()
    {
        private TUnion m_union;

        private readonly List<Vector2> m_horizontalRectVertices;
        private readonly List<Vector2> m_verticalRectVertices;
        private readonly Polygon2D m_horizontalRect;
        private readonly Polygon2D m_verticalRect;

        private readonly List<Vector2> m_2by1RectVertices;
        private readonly List<Vector2> m_1by2RectVertices;
        private readonly List<Vector2> m_unitSquareVertices;

        private readonly Polygon2D m_2by1rect;
        private readonly Polygon2D m_1by2rect;
        private readonly Polygon2D m_unitSquare;

        public IUnionTests()
        {
            m_horizontalRectVertices = new List<Vector2>
            {
                new Vector2(-2, 1), new Vector2(2, 1), new Vector2(2, -1), new Vector2(-2, -1)
            };
            m_verticalRectVertices = new List<Vector2>
            {
                new Vector2(-1, 2), new Vector2(1, 2), new Vector2(1, 0), new Vector2(-1, 0)
            };

            m_horizontalRect = new Polygon2D(m_horizontalRectVertices);
            m_verticalRect = new Polygon2D(m_verticalRectVertices);

            m_2by1RectVertices = new List<Vector2>
            {
                new Vector2(0, 1), new Vector2(2, 1), new Vector2(2, 0), new Vector2(0, 0)
            };
            m_1by2RectVertices = new List<Vector2>
            {
                new Vector2(0, 2), new Vector2(1, 2), new Vector2(1, 0), new Vector2(0, 0)
            };
            m_unitSquareVertices = new List<Vector2>
            {
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0)
            };

            m_2by1rect = new Polygon2D(m_2by1RectVertices);
            m_1by2rect = new Polygon2D(m_1by2RectVertices);
            m_unitSquare = new Polygon2D(m_unitSquareVertices);
        }

        [SetUp]
        public void CreateUnion()
        {
            m_union = new TUnion();
        }

        [Test]
        public void UnionTest1()
        {
            var cutout = m_union.Union(new List<Polygon2D> {m_verticalRect, m_horizontalRect});
            Assert.AreEqual(2f, cutout.Area, MathUtil.EPS);
        }

        [Test]
        public void UnionTest2()
        {
            var cutout = m_union.Union(new List<Polygon2D> {m_horizontalRect, m_verticalRect});
            Assert.AreEqual(6f, cutout.Area, MathUtil.EPS);
        }

        [Test]
        public void UnionRectFromSquareCollinearTest1()
        {
            var remainder = m_union.Union(new List<Polygon2D> {m_unitSquare, m_2by1rect});
            Assert.AreEqual(0f, remainder.Area, MathUtil.EPS);
        }

        [Test]
        public void UnionSquareFromRectCollinearTest1()
        {
            var remainder = m_union.Union(new List<Polygon2D> {m_2by1rect, m_unitSquare});
            Assert.AreEqual(1f, remainder.Area, MathUtil.EPS);
        }

        [Test]
        public void UnionRectFromSquareCollinearTest2()
        {
            var remainder = m_union.Union(new List<Polygon2D> {m_unitSquare, m_1by2rect});
            Assert.AreEqual(0f, remainder.Area, MathUtil.EPS);
        }

        [Test]
        public void UnionSquareFromRectCollinearTest2()
        {
            var remainder = m_union.Union(new List<Polygon2D> {m_1by2rect, m_unitSquare});
            Assert.AreEqual(1f, remainder.Area, MathUtil.EPS);
        }

        [Test]
        public void UnionRectFromRectCollinearTest1()
        {
            var remainder = m_union.Union(new List<Polygon2D> {m_1by2rect, m_2by1rect});
            Assert.AreEqual(1f, remainder.Area, MathUtil.EPS);
        }

        [Test]
        public void UnionRectFromRectCollinearTest2()
        {
            var remainder = m_union.Union(new List<Polygon2D> {m_2by1rect, m_1by2rect});
            Assert.AreEqual(1f, remainder.Area, MathUtil.EPS);
        }

        [Test]
        public void UnionNonIntersectingTest()
        {
            var horizontalRectVertices = new List<Vector2>
            {
                new Vector2(0, 0), new Vector2(2, 0), new Vector2(2, -1), new Vector2(0, -1)
            };
            var squareVertices = new List<Vector2>
            {
                new Vector2(10, 10), new Vector2(11, 10), new Vector2(11, 9), new Vector2(10, 9)
            };

            var horizontalRect = new Polygon2D(horizontalRectVertices);
            var square = new Polygon2D(squareVertices);

            var unionResult = m_union.Union(new List<Polygon2D> {square, horizontalRect});
            Assert.AreEqual(1f, unionResult.Area, MathUtil.EPS);

            unionResult = m_union.Union(new List<Polygon2D> {horizontalRect, square});
            Assert.AreEqual(2f, unionResult.Area, MathUtil.EPS);
        }
    }
}