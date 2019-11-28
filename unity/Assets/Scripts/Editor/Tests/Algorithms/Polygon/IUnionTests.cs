using System.Linq;

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
            var union = m_union.Union(new List<Polygon2D> {m_verticalRect, m_horizontalRect});
            Assert.AreEqual(10f, union.Area, MathUtil.EPS);
        }

        [Test]
        public void UnionTest2()
        {
            var union = m_union.Union(new List<Polygon2D> {m_horizontalRect, m_verticalRect});
            Assert.AreEqual(10f, union.Area, MathUtil.EPS);
        }

        [Test]
        public void UnionRectFromSquareCollinearTest1()
        {
            var remainder = m_union.Union(new List<Polygon2D> {m_unitSquare, m_2by1rect});
            Assert.AreEqual(2f, remainder.Area, MathUtil.EPS);
        }

        [Test]
        public void UnionSquareFromRectCollinearTest1()
        {
            var remainder = m_union.Union(new List<Polygon2D> {m_2by1rect, m_unitSquare});
            Assert.AreEqual(2f, remainder.Area, MathUtil.EPS);
        }

        [Test]
        public void UnionRectFromSquareCollinearTest2()
        {
            var remainder = m_union.Union(new List<Polygon2D> {m_unitSquare, m_1by2rect});
            Assert.AreEqual(2f, remainder.Area, MathUtil.EPS);
        }

        [Test]
        public void UnionSquareFromRectCollinearTest2()
        {
            var remainder = m_union.Union(new List<Polygon2D> {m_1by2rect, m_unitSquare});
            Assert.AreEqual(2f, remainder.Area, MathUtil.EPS);
        }

        [Test]
        public void UnionRectFromRectCollinearTest1()
        {
            var remainder = m_union.Union(new List<Polygon2D> {m_1by2rect, m_2by1rect});
            Assert.AreEqual(3f, remainder.Area, MathUtil.EPS);
        }

        [Test]
        public void UnionRectFromRectCollinearTest2()
        {
            var remainder = m_union.Union(new List<Polygon2D> {m_2by1rect, m_1by2rect});
            Assert.AreEqual(3f, remainder.Area, MathUtil.EPS);
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
            Assert.AreEqual(3f, unionResult.Area, MathUtil.EPS);

            unionResult = m_union.Union(new List<Polygon2D> {horizontalRect, square});
            Assert.AreEqual(3f, unionResult.Area, MathUtil.EPS);
        }

        [Test]
        public void UnionSimpleConvexPolygonsTest1()
        {
            // Area = 11 * 24 / 2 = 132
            // Full area is used = 132
            var triangle1 = new Polygon2D(new List<Vector2>
            {
                new Vector2(5, 1), new Vector2(5, 25), new Vector2(16, 1)
            });
            // Area = 9 * 5 = 45
            // Area in union: 45 - 5*5 = 20
            var rect1 = new Polygon2D(new List<Vector2>
            {
                new Vector2(5, 1), new Vector2(10, 1), new Vector2(10, 10), new Vector2(5, 10)
            });
            // Area = 5 * 9 = 45
            // Area in union: 45 - 45 = 0
            var rect2 = new Polygon2D(new List<Vector2>
            {
                new Vector2(0, 10), new Vector2(9, 10), new Vector2(9, 15), new Vector2(0, 15)
            });
            // Area = 87.5
            // Full area is used = 87.5
            var polygon1 = new Polygon2D(new List<Vector2>
            {
                new Vector2(15, 15), new Vector2(20, 15), new Vector2(20, 30), new Vector2(10, 20)
            });
            // Area = 5 * 15 = 75
            // Area in union: 75 - 5*5/2 = 62.5
            var rect3 = new Polygon2D(new List<Vector2>
            {
                new Vector2(15, 15), new Vector2(20, 15), new Vector2(20, 30), new Vector2(15, 30)
            });

            var polygon2Ds = new List<Polygon2D> {triangle1, rect1, rect2, polygon1, rect3};

            var sumResult = polygon2Ds.Sum(p => p.Area);
            Assert.AreEqual(384.5f, sumResult, MathUtil.EPS);

            var unionResult = m_union.Union(polygon2Ds);
            Assert.AreEqual(302f, unionResult.Area, MathUtil.EPS);
        }
    }
}