﻿// Copyright 2012, 2013, 2014 Derek J. Bailey
// Modified work copyright 2016, 2017 Stefan Solntsev
//
// This file (MassExtensions.cs) is part of Chemistry Library.
//
// Chemistry Library is free software: you can redistribute it and/or modify it
// under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Chemistry Library is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public
// License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with Chemistry Library. If not, see <http://www.gnu.org/licenses/>.

using Easy.Common.Extensions;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace Chemistry
{
    public static class ClassExtensions
    {
        private static double before { get; set; }
        private static double after { get; set; }
        private static double current { get; set; }

        /// <summary>
        /// Calculates m/z value for a given mass assuming charge comes from losing or gaining protons
        /// </summary>
        public static double ToMz(this IHasMass objectWithMass, int charge)
        {
            return ToMz(objectWithMass.MonoisotopicMass, charge);
        }

        /// <summary>
        /// Calculates m/z value for a given mass assuming charge comes from losing or gaining protons
        /// </summary>
        public static double ToMz(this double mass, int charge)
        {
            return mass / Math.Abs(charge) + Math.Sign(charge) * Constants.ProtonMass;
        }

        /// <summary>
        /// Determines the original mass from an m/z value, assuming charge comes from a proton
        /// </summary>
        public static double ToMass(this double massToChargeRatio, int charge)
        {
            return Math.Abs(charge) * massToChargeRatio - charge * Constants.ProtonMass;
        }

        public static double? RoundedDouble(this double? myNumber, int places = 9)
        {
            if (myNumber != null)
            {
                myNumber = Math.Round((double)myNumber, places, MidpointRounding.AwayFromZero);
            }
            return myNumber;
        }

        public class TupleList<T1, T2> : List<Tuple<T1, T2>>
        {
            public void Add(T1 item, T2 item2)
            {
                Add(new Tuple<T1, T2>(item, item2));
            }
        }

        /// <summary>
        /// Binary Search that returns the closest position to the value. lowest, closest, higher with default of closest
        /// </summary>
        /// <param name="xArray"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double ClosestBinarySearch(this double[] xArray, double value, ClassExtensions.BinarySearchParameters position=BinarySearchParameters.Closest)
        {
            var search = Array.BinarySearch(xArray, value);
            
            if (search >= 0)
            {
                return search;
            }

            switch (position)
            {
                case BinarySearchParameters.Closest:
                    
                    current = xArray[~search];

                    if(~search > 0)
                    {
                        before = xArray[~search - 1];
                    }
                    else
                    {
                        before = xArray[~search];
                    }

                    if(~search < xArray.Length)
                    {
                        after = xArray[~search + 1];

                    }
                    else
                    {
                        after = xArray[~search];
                    }

                    if(Math.Abs(current - before) < Math.Abs(current - after) || Math.Abs(current - before) > Math.Abs(current - after) && ~search > 0)
                    {
                        return ~search;
                    }
                    //else if(Math.Abs(current - before) > Math.Abs(current - after))
                    //{
                    //    return ~search - 1;
                    //}
                    else
                    {
                        return ~search;
                    }
                    

                case BinarySearchParameters.ClosestDown:

                    if ((~search) - 2 >= 0)
                    {
                        return (~search) - 2;
                    }
                    else
                    {
                        return (~search) - 1;
                    }

                case BinarySearchParameters.ClosestUp:
                    
                    return ~ search;
            }

            return -99;
        }

        public enum BinarySearchParameters
        {
            ClosestDown,
            Closest,
            ClosestUp
        }
    }
}