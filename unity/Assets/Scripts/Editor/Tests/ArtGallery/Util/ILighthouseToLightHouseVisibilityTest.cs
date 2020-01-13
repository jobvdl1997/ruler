using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.ArtGallery.Util;
using NUnit.Framework;
using UnityEngine;
using Util.Geometry.Polygon;

namespace ArtGallery.Tests
{

    [TestFixture(typeof(NaiveLighthouseToLighthouseVisibility))]
    [TestFixture(typeof(SmartLighthouseToLighthouseVisibility))]
    public class
        ILighthouseToLightHouseVisibilityTest<TLighthouseToLightHouseVisibility>
        where TLighthouseToLightHouseVisibility :
        ILighthouseToLightHouseVisibility, new()
    {
        /// <summary>
        /// An instance of a class implementing <see cref="ILighthouseToLightHouseVisibility"/>
        /// </summary>
        private TLighthouseToLightHouseVisibility m_L2LVisibility;

        private readonly Polygon2D arrowPoly;
        private readonly Polygon2D diamondPoly;
        private readonly Polygon2D LShape;

        public ILighthouseToLightHouseVisibilityTest()
        {
            var m_topVertex = new Vector2(1, 1);
            var m_botVertex = new Vector2(0, -1);
            var m_leftVertex = new Vector2(-1, 0);
            var m_rightVertex = new Vector2(1, 0);
            var m_farRightVertex = new Vector2(2, 0);

            arrowPoly = new Polygon2D(
                new List<Vector2>()
                {
                    m_topVertex,
                    m_farRightVertex,
                    m_botVertex,
                    m_rightVertex
                });

            diamondPoly = new Polygon2D(
                new List<Vector2>()
                {
                    m_topVertex,
                    m_rightVertex,
                    m_botVertex,
                    m_leftVertex
                });

            LShape = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(4, 4),
                    new Vector2(4, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });
        }

        [SetUp]
        public void CreateUnion()
        {
            m_L2LVisibility = new TLighthouseToLightHouseVisibility();
        }


        [Test]
        public void VisibleToEachOtherTest()
        {
            var polygon = arrowPoly;

            var vertex1 = polygon.Vertices.First();
            var vertex2 = polygon.Vertices.ElementAt(2);

            bool canSeeEachOther =
                m_L2LVisibility.VisibleToOtherVertex(
                    vertex1,
                    vertex2,
                    polygon);

            Assert.IsFalse(canSeeEachOther);
        }

        [Test]
        public void VisibleToEachOtherTest1()
        {
            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(4, 4),
                    new Vector2(6, 4),
                    new Vector2(8, 4),
                    new Vector2(10, 4),
                    new Vector2(10, 2),
                    new Vector2(8, 2),
                    new Vector2(6, 2),
                    new Vector2(4, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });

            var vertex1 = polygon.Vertices.First();
            var vertex2 = polygon.Vertices.ElementAt(1);

            bool canSeeEachOther =
                m_L2LVisibility.VisibleToOtherVertex(
                    vertex1,
                    vertex2,
                    polygon);

            Assert.IsTrue(canSeeEachOther);
        }


        [Test]
        public void VisibleToEachOtherTest2()
        {
            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(4, 4),
                    new Vector2(6, 4),
                    new Vector2(8, 4),
                    new Vector2(10, 4),
                    new Vector2(10, 2),
                    new Vector2(8, 2),
                    new Vector2(6, 2),
                    new Vector2(4, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });

            var vertex1 = new Vector2(0, 4);
            var vertex2 = new Vector2(10, 4);

            bool canSeeEachOther =
                m_L2LVisibility.VisibleToOtherVertex(
                    vertex1,
                    vertex2,
                    polygon);

            Assert.IsTrue(canSeeEachOther);
        }

        [Test]
        public void VisibleToEachOtherTest3()
        {
            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(4, 4),
                    new Vector2(6, 4),
                    new Vector2(8, 4),
                    new Vector2(10, 4),
                    new Vector2(10, 2),
                    new Vector2(8, 2),
                    new Vector2(6, 2),
                    new Vector2(4, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });

            var vertex1 = new Vector2(0, 0);
            var vertex2 = new Vector2(10, 2);

            bool canSeeEachOther =
                m_L2LVisibility.VisibleToOtherVertex(
                    vertex1,
                    vertex2,
                    polygon);

            Assert.IsFalse(canSeeEachOther);
        }

        [Test]
        public void VisibleToEachOtherTest4()
        {
            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(4, 4),
                    new Vector2(6, 4),
                    new Vector2(8, 4),
                    new Vector2(10, 4),
                    new Vector2(10, 2),
                    new Vector2(8, 2),
                    new Vector2(6, 2),
                    new Vector2(4, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });

            var vertex1 = new Vector2(0, 0);

            var othervertices = new List<Vector2>()
            {
                new Vector2(10, 2),
                new Vector2(0, 4)
            };

            bool canSeeEachOther =
                m_L2LVisibility.VisibleToOtherVertex(
                    vertex1,
                    othervertices,
                    polygon);

            Assert.IsTrue(canSeeEachOther);
        }

        [Test]
        public void VisibleToEachOtherTest5()
        {
            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(4, 4),
                    new Vector2(6, 4),
                    new Vector2(8, 4),
                    new Vector2(10, 4),
                    new Vector2(10, 2),
                    new Vector2(8, 2),
                    new Vector2(6, 2),
                    new Vector2(4, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });

            var vertex1 = new Vector2(0, 0);

            var othervertices = new List<Vector2>()
            {
                new Vector2(10, 2),
                new Vector2(0, 4)
            };

            bool canSeeEachOther =
                m_L2LVisibility.VisibleToOtherVertex(
                    vertex1,
                    othervertices,
                    polygon);

            Assert.IsTrue(canSeeEachOther);
        }


        [Test]
        public void VisibleToEachOtherTest6()
        {
            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(4, 4),
                    new Vector2(6, 4),
                    new Vector2(8, 4),
                    new Vector2(10, 4),
                    new Vector2(10, 2),
                    new Vector2(8, 2),
                    new Vector2(6, 2),
                    new Vector2(4, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });

            var vertices = new List<Vector2>()
            {
                new Vector2(0, 0),
                new Vector2(10, 2),
                new Vector2(0, 4)
            };

            bool canSeeEachOther =
                m_L2LVisibility.VisibleToOtherVertex(
                    vertices,
                    polygon);

            Assert.IsTrue(canSeeEachOther);
        }

        [Test]
        public void VisibleToEachOtherTest7()
        {
            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(4, 4),
                    new Vector2(6, 4),
                    new Vector2(8, 4),
                    new Vector2(10, 4),
                    new Vector2(10, 2),
                    new Vector2(8, 2),
                    new Vector2(6, 2),
                    new Vector2(4, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });

            var vertices = new List<Vector2>()
            {
                new Vector2(0, 0),
                new Vector2(10, 2),
                new Vector2(8, 2),
                new Vector2(6, 2),
                new Vector2(4, 2),
            };

            bool canSeeEachOther =
                m_L2LVisibility.VisibleToOtherVertex(
                    vertices,
                    polygon);

            Assert.IsFalse(canSeeEachOther);
        }

        [Test]
        public void VisibleToOtherVerticesTest1()
        {
            var m_topVertex = new Vector2(1, 1);
            var m_botVertex = new Vector2(0, -1);
            var m_rightVertex = new Vector2(1, 0);
            var m_farRightVertex = new Vector2(2, 0);

            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    m_topVertex,
                    m_farRightVertex,
                    m_botVertex,
                    m_rightVertex
                });

            var vertices = new List<Vector2>()
            {
                m_botVertex
            };

            var visibleVertices = new List<Vector2>();

            var vertex = m_topVertex;

            var actual =
                m_L2LVisibility.VisibleToOtherVertices(
                    vertex,
                    vertices,
                    polygon);

            Assert.AreEqual(visibleVertices.Count, actual.Count);
        }


        [Test]
        public void VisibleToOtherVerticesTest2()
        {
            var m_topVertex = new Vector2(1, 1);
            var m_botVertex = new Vector2(0, -1);
            var m_rightVertex = new Vector2(1, 0);
            var m_farRightVertex = new Vector2(2, 0);

            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    m_topVertex,
                    m_farRightVertex,
                    m_botVertex,
                    m_rightVertex
                });

            var vertices = new List<Vector2>()
            {
                m_farRightVertex,
                m_botVertex,
                m_rightVertex
            };

            var visibleVertices = new List<Vector2>()
            {
                m_farRightVertex,
                m_rightVertex
            };

            var vertex = m_topVertex;

            var actual =
                m_L2LVisibility.VisibleToOtherVertices(
                    vertex,
                    vertices,
                    polygon);

            Assert.AreEqual(visibleVertices.Count, actual.Count);
        }

        [Test]
        public void VisibleToOtherVerticesTest3()
        {
            var m_topVertex = new Vector2(1, 1);
            var m_botVertex = new Vector2(0, -1);
            var m_rightVertex = new Vector2(1, 0);
            var m_farRightVertex = new Vector2(2, 0);

            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    m_topVertex,
                    m_farRightVertex,
                    m_botVertex,
                    m_rightVertex
                });

            var vertices = new List<Vector2>()
            {
                m_topVertex,
                m_botVertex,
                m_rightVertex
            };

            var actual =
                m_L2LVisibility.VisibleToOtherVertices(
                    vertices,
                    polygon);

            Assert.AreEqual(1, actual[m_topVertex].Count);
            Assert.AreEqual(1, actual[m_botVertex].Count);
            Assert.AreEqual(2, actual[m_rightVertex].Count);
        }

        [Test]
        public void VisibleToOtherVerticesTest4()
        {
            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(4, 4),
                    new Vector2(6, 4),
                    new Vector2(8, 4),
                    new Vector2(10, 4),
                    new Vector2(10, 2),
                    new Vector2(8, 2),
                    new Vector2(6, 2),
                    new Vector2(4, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });

            var vertices = new List<Vector2>()
            {
                new Vector2(0, 0),
                new Vector2(10, 2),
                new Vector2(8, 2),
            };

            var actual =
                m_L2LVisibility.VisibleToOtherVertices(
                    vertices,
                    polygon);

            Assert.AreEqual(0, actual[new Vector2(0, 0)].Count);
            Assert.AreEqual(1, actual[new Vector2(10, 2)].Count);
            Assert.AreEqual(1, actual[new Vector2(8, 2)].Count);
        }
    }
}
