using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Util.Geometry.Polygon;
using Util.Math;

namespace Util.Algorithms.Polygon.Tests
{
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

            // Just to make sure that they are non overlapping
            Assert.AreEqual(horizontalRect.Area + square.Area, unionResult.Area, MathUtil.EPS);
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
            Assert.AreEqual(257f, unionResult.Area, MathUtil.EPS);
        }

        [Test]
        public void UnionTest3()
        {
            var polygon0 = new Polygon2D(new List<Vector2>
            {
                new Vector2(4.00124f, -0.9741771f), new Vector2(1.329107f, 0.0009262562f),
                new Vector2(-1.329329f, -0.003419161f), new Vector2(-1.336399f, -2.668384f),
                new Vector2(-4.00066f, -2.661608f), new Vector2(-4.00317f, 2.666523f), new Vector2(4.00428f, 2.665259f),
            });
            var polygon1 = new Polygon2D(new List<Vector2>
            {
                new Vector2(4.00124f, -2.664724f), new Vector2(1.333887f, -2.663425f),
                new Vector2(1.329107f, 0.0009263754f), new Vector2(-1.329329f, -0.003419101f),
                new Vector2(-4.00317f, -0.7164398f), new Vector2(-4.00317f, 2.666523f),
                new Vector2(4.00428f, 2.665259f),
            });

            var polygon2Ds = new List<Polygon2D> {polygon0, polygon1};

            var unionResult = m_union.Union(polygon2Ds);

            Assert.AreEqual(35.57237, unionResult.Area, MathUtil.EPS * 100);
        }

        // All following unit tests use base64 strings because this is the easiest way to transfer a failing case in the
        // game to a unit test while preserving the exact precision because these usually fail because of robustness issues.

        [Test]
        public void UnionTest4()
        {
            var polygon0 = new Polygon2D(new List<Vector2>
            {
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("KAqAQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("ODJnvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("MiCqPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("ANhyOg=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("cieqvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("ABRguw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("Ig+rvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("zsYqwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("agWAwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("ylcqwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("9hmAwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("TqgqQA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("ECOAQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("nJMqQA=="), 0)),
            });
            var polygon1 = new Polygon2D(new List<Vector2>
            {
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("KAqAQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("2IoqwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("0LyqPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("j3UqwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("MCCqPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("ANxyOg=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("dieqvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("AA9guw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("+BmAwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("RGUTvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("+BmAwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("TKgqQA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("DyOAQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("nJMqQA=="), 0)),
            });

            var polygon2Ds = new List<Polygon2D> {polygon0, polygon1};

            var unionResult = m_union.Union(polygon2Ds);
            Assert.AreEqual(35.57237, unionResult.Area, MathUtil.EPS * 100);
        }

        [Test]
        public void UnionTest5()
        {
            var subject = new Polygon2D(new List<Vector2>
            {
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("DiOAQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("nJMqQA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("9xmAwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("TqgqQA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("aQWAwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("yVcqwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("JA+rvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("z8YqwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("cieqvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("ABZguw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("MiCqPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("AOByOg=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("KAqAQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("jFamvw=="), 0)),
            });
            var clipping = new Polygon2D(new List<Vector2>
            {
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("KAqAQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("2YoqwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("zryqPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("j3UqwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("MCCqPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("AOByOg=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("cieqvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("ABRguw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("+BmAwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("iDpQvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("9hmAwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("UKgqQA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("DyOAQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("nJMqQA=="), 0)),
            });

            var polygon2Ds = new List<Polygon2D> {subject, clipping};
            var unionResult = m_union.Union(polygon2Ds);

            Assert.AreEqual(35.57237, unionResult.Area, MathUtil.EPS * 100);
        }

        [Test]
        public void UnionTest6()
        {
            var subject = new Polygon2D(new List<Vector2>
            {
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("DNKAvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("DGBAPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("fSoAwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("kElQQA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("ECmAwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("NjR/Pg=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("JU6Avw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("NkVQwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("ZnI7QA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("XPykvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("NS2/Pw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("4FU/vw=="), 0)),
            });
            var clipping = new Polygon2D(new List<Vector2>
            {
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("p79/QA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("amRBvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("JE6Avw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("OEVQwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("OcogwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("cAe/vw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("Ni2/Pw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("4FU/vw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("fivAPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("XukvQA=="), 0)),
            });

            var polygon2Ds = new List<Polygon2D> {subject, clipping};
            var unionResult = m_union.Union(polygon2Ds);

            Assert.Greater(unionResult.Area, 0);
            Assert.AreEqual(22.53488, unionResult.Area, 100 * MathUtil.EPS);
        }
    }
}