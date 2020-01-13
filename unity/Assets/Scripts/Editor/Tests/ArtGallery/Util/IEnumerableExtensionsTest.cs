namespace ArtGallery.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;
    using UnityEngine;
    using Util.Geometry;
    using Util.Geometry.Polygon;
    using Util.Math;
    using Util.Algorithms.Polygon;

    [TestFixture]
    public class IEnumerableExtensionsTest
    {
        [Test]
        public void StartAtTest1()
        {
            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(10, 4),
                    new Vector2(10, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });

            var start = new Vector2(10, 4);

            Assert.AreNotEqual(start, polygon.Vertices.First());

            var actual = polygon.Vertices.StartAt(start);

            Assert.AreEqual(start, actual.First());
        }


        [Test]
        public void StartAtTest2()
        {
            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(10, 4),
                    new Vector2(10, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });

            var start = new Vector2(2, 0);

            Assert.AreNotEqual(start, polygon.Vertices.First());

            var actual = polygon.Vertices.StartAt(start);

            Assert.AreEqual(start, actual.First());
        }

        [Test]
        public void StartAtTest3()
        {
            var input = new List<int>
            {
                1,
                1,
                2,
                2,
                3,
                4,
                5,
                6
            };

            var expectedList = new List<int>
            {
                2,
                2,
                3,
                4,
                5,
                6,
                1,
                1,
            };

            var start = 2;

            Assert.AreNotEqual(start, input.First());

            var actual = input.StartAt(start).ToList();

            Assert.AreEqual(start, actual.First());

            for (int i = 0; i < expectedList.Count; i++)
            {
                Assert.AreEqual(expectedList[i], actual[i]);
            }
        }

        [Test]
        public void StartAtTest4()
        {
            var input = new List<int>
            {
                1,
                1,
                2,
                2,
                3,
                4,
                5,
                6
            };

            var start = 8;

            Assert.Throws<ArgumentException>(() => input.StartAt(start));
        }
    }
}