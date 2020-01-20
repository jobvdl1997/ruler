using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Util.Geometry.Contour;
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

        [Test]
        public void UnionTest7()
        {
            var polygon0 = new Polygon2D(new List<Vector2>
            {
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("qFoawA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("PFXtPg=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("OM17wA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("cQICPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("VId/wA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("UJcZQA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("EOdBwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("kGoZQA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("aREjwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("QrtjPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("vKb5vw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("xNNRPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("1pdsvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("M0vOPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("SmsVvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("xZvPPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("ZEEiPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("lC0ZQA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("ctxsQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("0pcaQA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("qGJtQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("HPwVQA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("bPDcPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("GALePw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("7NTyPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("eJSlPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("UPt6QA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("HFeoPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("BLp7QA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("sDG+vg=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("UArkPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("FkPtvg=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("wJDjPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("RhoZvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("PAYvvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("gD/SPA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("cteMvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("cHD5PA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("/lqIvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("HvApvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("7OTKvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("Iroavw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("0RTKvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("lvdjvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("DXqKvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("n17Zvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("5/6Mvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("FOMZwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("WFr9vw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("eLQZwA=="), 0)),
            });
            var polygon1 = new Polygon2D(new List<Vector2>
            {
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("0RTKvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("lvdjvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("6HyHvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("+DJcvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("5v6Mvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("FeMZwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("iosswA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("aY4ZwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("QHouwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("OMuAvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("9nF/wA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("0sOCvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("nv5/wA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("oNWbPA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("FxkWwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("4P+7PA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("qFoawA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("QFXtPg=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("+IxmwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("LoUZQA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("FqJXwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("WHoZQA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("aBEjwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("QrtjPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("vKb5vw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("yNNRPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("p9f3vw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("smN/Pw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("8v0CwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("cWHKPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("NyHRvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("zY3LPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("7OTKvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("I7oavw=="), 0)),
            });
            var polygon2 = new Polygon2D(new List<Vector2>
            {
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("ffUNvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("LDj9Pw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("8qP6vw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("R631Pw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("2JT/vw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("5PYXQA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("c9xsQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("0pcaQA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("zbNxQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("MhTgPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("bPDcPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("GALePw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("7tTyPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("eJSlPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("zz17QA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("0CE6Pw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("Bbp7QA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("sDG+vg=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("UgrkPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("GEPtvg=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("2v3hPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("Fq2Fvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("x8EzQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("ltAZwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("08ssQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("uu4ZwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("f8cgQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("Lr8JwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("OvEFQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("eE0KwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("iz8JQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("8BkZwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("UDBUPQ=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("evgYwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("OA8avg=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("nNhuvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("oJlpvg=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("KOwVvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("EDgrvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("KIIYvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("PQYvvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("QD/SPA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("xtQ2vw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("wB3VPA=="), 0)),
            });
            var polygon3 = new Polygon2D(new List<Vector2>
            {
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("Og8avg=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("ndhuvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("oJlpvg=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("KuwVvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("Poo4vw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("LBTPPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("SmsVvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("xpvPPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("fPUNvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("LDj9Pw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("/WMcvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("kpoYQA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("fKUMQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("Q+YZQA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("bPDcPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("GALePw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("7dTyPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("eJSlPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("UPt6QA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("HFeoPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("Wol7QA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("YBBxPQ=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("UgrkPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("FkPtvg=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("2f3hPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("Fq2Fvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("/x0FQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("0PWOvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("7Kh9QA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("8dWivw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("fMh/QA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("UYcYwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("IktDQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("TY0ZwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("gMcgQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("Lb8JwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("OvEFQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("eU0KwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("iz8JQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("8BkZwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("BoT3vg=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("6+8YwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("FXIRvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("Sjxwvw=="), 0)),
            });
            var polygon4 = new Polygon2D(new List<Vector2>
            {
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("gMcgQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("Lb8JwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("OvEFQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("eU0KwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("uL/8vg=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("L7sNwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("FHIRvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("Rjxwvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("QA8avg=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("mthuvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("kJlpvg=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("JuwVvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("mNgsvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("HPyovg=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("OAYvvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("AEDSPA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("vPv4vw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("MKdiPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("qNf3vw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("tGN/Pw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("3kwOwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("CrmZPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("7AwPwA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("dDvJPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("SGsVvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("ypvPPw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("ePUNvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("Ljj9Pw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("+NFavw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("w30YQA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("WJoLvw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("T6IYQA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("2f3hPw=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("Fq2Fvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("/x0FQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("0PWOvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("Ps8CQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("fjMxvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("ddl8QA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("9BpZvw=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("fsh/QA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("UYcYwA=="), 0)),
                new Vector2(BitConverter.ToSingle(Convert.FromBase64String("jPUeQA=="), 0),
                    BitConverter.ToSingle(Convert.FromBase64String("qCoawA=="), 0)),
            });
            
            var polygon2Ds = new List<Polygon2D> {polygon0, polygon1, polygon2, polygon3, polygon4};
            var unionResult = m_union.Union(polygon2Ds);

            Assert.Greater(unionResult.Area, 0);
            Debug.Log(unionResult.Area);
        }
    }
}