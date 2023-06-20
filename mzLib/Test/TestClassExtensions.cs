﻿using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using MzLibUtil;
using System.IO;
using System.Linq;
using System;
using MassSpectrometry;
using System.Collections.Generic;
using static Chemistry.ClassExtensions;
using System.Windows.Media.Animation;
using System.Net.NetworkInformation;

namespace Test
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class TestClassExtensions
    {
        [Test]
        public static void TestBoxCarSmooth()
        {
            double[] inputData = new double[] { 0.19, 0.69, 0.03, 0.85, 0.84, 0.46, 0.09, 0.05, 0.11, 0.5, 0.6, 0.78, 0.48, 0.66, 0.61, 0.78, 0.82, 0.18, 0.77, 0.14, 0.97, 0.48, 0.54, 0.98, 0.01, 0.38, 0.26, 0.4, 0.31, 0.41, 0.03, 0.2, 0.98, 0.36, 0.24, 0.51, 0.14, 0.96, 0.32, 0.9, 0.36, 0.57, 0.97, 0.07, 0.12, 0.73, 0.92, 0.51, 0.04, 0.2, 0.39, 0.32, 0.33, 0.62, 0.32, 0.68, 0.91, 0.3, 0.68, 0.22, 0.89, 0.27, 0.68, 0.08, 0.61, 0.25, 0.82, 0.73, 0.49, 0.76, 0.01, 0.15, 0.13, 0.96, 0.57, 0.58, 0.96, 0.93, 0.5, 0.45, 0.89, 0.44, 0.59, 0.68, 0.71, 0.85, 0.16, 0.18, 0.68, 0.37, 0.22, 0.81, 0.53, 0.26, 0.94, 0.52, 0.66, 0.55, 0.51, 0.14 };
            double[] mySmoothedArray = inputData.BoxCarSmooth(3);
            double[] expectedOutput = new[] { 0.3, 0.52, 0.57, 0.72, 0.46, 0.2, 0.08, 0.22, 0.4, 0.63, 0.62, 0.64, 0.58, 0.68,
                0.74, 0.59, 0.59, 0.36, 0.63, 0.53, 0.66, 0.67, 0.51, 0.46, 0.22, 0.35, 0.32, 0.37, 0.25,
                0.21, 0.4, 0.51, 0.53, 0.37, 0.3, 0.54, 0.47, 0.73, 0.53, 0.61, 0.63, 0.54, 0.39, 0.31, 0.59,
                0.72, 0.49, 0.25, 0.21, 0.3, 0.35, 0.42, 0.42, 0.54, 0.64, 0.63, 0.63, 0.4, 0.6, 0.46, 0.61, 0.34, 0.46,
                0.31, 0.56, 0.6, 0.68, 0.66, 0.42, 0.31, 0.1, 0.41, 0.55, 0.7, 0.7, 0.82, 0.8, 0.63, 0.61, 0.59, 0.64, 0.57, 0.66, 0.75, 0.57, 0.4, 0.34, 0.41, 0.42, 0.47, 0.52, 0.53, 0.58, 0.57, 0.71, 0.58, 0.57, 0.4 };
            Assert.That(expectedOutput, Is.EqualTo(mySmoothedArray).Within(0.1));
        }

        [Test]
        public static void TestScrambledEquals()
        {
            List<int> list1 = new() { 1, 2, 3, 4, 5, 6 };
            List<int> list2 = new() { 1, 3, 5, 6, 4, 2 };
            List<int> list3 = new() { 1, 2, 3, 4, 6, 7 };
            bool expectedTrue = list1.ScrambledEquals(list2);
            bool expectedFalse = list1.ScrambledEquals(list3);

            Assert.True(expectedTrue);
            Assert.False(expectedFalse);
        }

        [Test]
        public static void TestAllSame()
        {
            MzSpectrum spectrum1 = new MzSpectrum(new[] { 2.0, 2.0 }, new[] { 1.0, 1.0 }, false);
            MzSpectrum spectrum2 = new MzSpectrum(new[] { 5.0, 5.0 }, new[] { 2.0, 2.0 }, false);

            var sameInt = new[] { 2, 2 };
            var sameDouble = new[] { 2.0, 2.0 };
            var sameSpectrum = new[] { spectrum1, spectrum1 };
            Assert.That(sameInt.AllSame());
            Assert.That(sameDouble.AllSame());
            Assert.That(sameSpectrum.AllSame());

            var differentInt = new[] { 2, 5 };
            var differentDouble = new[] { 2.0, 5.0 };
            var differentSpectrum = new[] { spectrum1, spectrum2 };
            Assert.That(!differentInt.AllSame());
            Assert.That(!differentDouble.AllSame());
            Assert.That(!differentSpectrum.AllSame());
        }

        [Test]
        [Parallelizable(ParallelScope.All)]
        [TestCase(new double[] { 0.1, 0.2, 0.3, 0.4 }, 0.22, BinarySearchParameters.Closest, 1)]
        [TestCase(new double[] { 0.1, 0.2, 0.3, 0.4 }, 0.4, BinarySearchParameters.Closest, 3)]
        [TestCase(new double[] { 0.1, 0.2, 0.3, 0.4 }, 0.3, BinarySearchParameters.Closest, 2)]
        [TestCase(new double[] { 0.1, 0.2, 0.3, 0.4 }, 0.12, BinarySearchParameters.Closest, 0)]
        [TestCase(new double[] { 0.1, 0.2, 0.3, 0.4 }, 0.22, BinarySearchParameters.ClosestDown, 0)]
        [TestCase(new double[] { 0.1, 0.2, 0.3, 0.4 }, 0.11, BinarySearchParameters.ClosestDown, 0)]
        [TestCase(new double[] { 0.1, 0.2, 0.3, 0.4 }, 0.22, BinarySearchParameters.ClosestUp, 2)]
        [TestCase(new double[] { 0.1, 0.2, 0.3, 0.4 }, 0.1, BinarySearchParameters.ClosestUp, 0)]

        public static void TestClosestBinarySearch(double[] array, double value, BinarySearchParameters position, double expected)
        {
            Console.WriteLine("Supossed to be: " + expected);
            Console.WriteLine("Got: "+Chemistry.ClassExtensions.ClosestBinarySearch(array, value, position));
            Assert.That(Chemistry.ClassExtensions.ClosestBinarySearch(array, value, position).Equals(expected));
        }
    }
}